'    UNIFAC Property Package 
'    Copyright 2008-2011 Daniel Wagner O. de Medeiros
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

Imports Microsoft.VisualBasic.FileIO
Imports DWSIM.DWSIM.MathEx
Imports Cudafy
Imports Cudafy.Translator
Imports Cudafy.Host

Namespace DWSIM.SimulationObjects.PropertyPackages.Auxiliary

    <System.Serializable()> Public Class Unifac

        Public UnifGroups As UnifacGroups

        Sub New()

            UnifGroups = New UnifacGroups

        End Sub

        Function ID2Group(ByVal id As Integer) As String

            For Each group As UnifacGroup In Me.UnifGroups.Groups.Values
                If group.Secondary_Group = id Then Return group.GroupName
            Next

            Return ""

        End Function

        Function Group2ID(ByVal groupname As String) As String

            For Each group As UnifacGroup In Me.UnifGroups.Groups.Values
                If group.GroupName = groupname Then
                    Return group.Secondary_Group
                End If
            Next

            Return 0

        End Function

        Function GAMMA(ByVal T, ByVal Vx, ByVal VQ, ByVal VR, ByVal VEKI, ByVal index)

            CheckParameters(VEKI)

            Dim Q(), R(), j(), L()
            Dim i, k, m As Integer

            Dim n = UBound(Vx)
            Dim n2 = UBound(VEKI, 2)

            Dim beta(,), teta(n2), s(n2), Vgammac(), Vgammar(), Vgamma(), b(,)
            ReDim Preserve beta(n, n2), Vgammac(n), Vgammar(n), Vgamma(n), b(n, n2)
            ReDim Q(n), R(n), j(n), L(n)

            i = 0
            Do
                k = 0
                Do
                    beta(i, k) = 0
                    m = 0
                    Do
                        beta(i, k) = beta(i, k) + VEKI(i, m) * TAU(m, k, T)
                        m = m + 1
                    Loop Until m = n2 + 1
                    k = k + 1
                Loop Until k = n2 + 1
                i = i + 1
            Loop Until i = n + 1

            Dim soma_xq = 0
            i = 0
            Do
                Q(i) = VQ(i)
                soma_xq = soma_xq + Vx(i) * Q(i)
                i = i + 1
            Loop Until i = n + 1

            k = 0
            Do
                i = 0
                Do
                    teta(k) = teta(k) + Vx(i) * Q(i) * VEKI(i, k)
                    i = i + 1
                Loop Until i = n + 1
                teta(k) = teta(k) / soma_xq
                k = k + 1
            Loop Until k = n2 + 1

            k = 0
            Do
                m = 0
                Do
                    s(k) = s(k) + teta(m) * TAU(m, k, T)
                    m = m + 1
                Loop Until m = n2 + 1
                k = k + 1
            Loop Until k = n2 + 1

            Dim soma_xr = 0
            i = 0
            Do
                R(i) = VR(i)
                soma_xr = soma_xr + Vx(i) * R(i)
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                j(i) = R(i) / soma_xr
                L(i) = Q(i) / soma_xq
                Vgammac(i) = 1 - j(i) + Math.Log(j(i)) - 5 * Q(i) * (1 - j(i) / L(i) + Math.Log(j(i) / L(i)))
                k = 0
                Dim tmpsum = 0
                Do
                    tmpsum = tmpsum + teta(k) * beta(i, k) / s(k) - VEKI(i, k) * Math.Log(beta(i, k) / s(k))
                    k = k + 1
                Loop Until k = n2 + 1
                Vgammar(i) = Q(i) * (1 - tmpsum)
                Vgamma(i) = Math.Exp(Vgammac(i) + Vgammar(i))
                If Vgamma(i) = 0 Then Vgamma(i) = 0.000001
                i = i + 1
            Loop Until i = n + 1

            i = 1
            k = 1
            GAMMA = Vgamma(index)

        End Function

        Function GAMMA_MR(ByVal T As Double, ByVal Vx As Double(), ByVal VQ As Double(), ByVal VR As Double(), ByVal VEKI As Double(,))

            If My.Settings.EnableGPUProcessing Then
                Return GAMMA_MR_GPU(T, Vx, VQ, VR, VEKI)
            Else
                Return GAMMA_MR_CPU(T, Vx, VQ, VR, VEKI)
            End If

        End Function

        Private Sub unifac_gpu_func(n As Integer, n2 As Integer, beta As Double(,), VTAU As Double(,), VEKI As Double(,), T As Double, Vx As Double(), s As Double(), teta As Double(), VQ As Double(), VR As Double(), Vgamma As Double())

            Dim sum1(n), sum2(n), soma_xq, soma_xr As Double

            Dim gpu As GPGPU = My.MyApplication.gpu

            If gpu.IsMultithreadingEnabled Then gpu.Lock()

            ' allocate the memory on the GPU
            Dim dev_beta As Double(,) = gpu.Allocate(Of Double)(beta)
            Dim dev_VEKI As Double(,) = gpu.Allocate(Of Double)(VEKI)
            Dim dev_VTAU As Double(,) = gpu.Allocate(Of Double)(VTAU)
            Dim dev_sum1 As Double() = gpu.Allocate(Of Double)(sum1)
            Dim dev_sum2 As Double() = gpu.Allocate(Of Double)(sum2)
            Dim dev_Vx As Double() = gpu.Allocate(Of Double)(Vx)
            Dim dev_VQ As Double() = gpu.Allocate(Of Double)(VQ)
            Dim dev_VR As Double() = gpu.Allocate(Of Double)(VR)
            Dim dev_s As Double() = gpu.Allocate(Of Double)(s)
            Dim dev_teta As Double() = gpu.Allocate(Of Double)(teta)
            Dim dev_Vgamma As Double() = gpu.Allocate(Of Double)(Vgamma)

            ' copy the arrays to the GPU
            gpu.CopyToDevice(beta, dev_beta)
            gpu.CopyToDevice(VEKI, dev_VEKI)
            gpu.CopyToDevice(VTAU, dev_VTAU)
            gpu.CopyToDevice(sum1, dev_sum1)
            gpu.CopyToDevice(sum2, dev_sum2)
            gpu.CopyToDevice(Vx, dev_Vx)
            gpu.CopyToDevice(VQ, dev_VQ)
            gpu.CopyToDevice(VR, dev_VR)
            gpu.CopyToDevice(s, dev_s)
            gpu.CopyToDevice(teta, dev_teta)
            gpu.CopyToDevice(Vgamma, dev_Vgamma)

            ' launch subs
            gpu.Launch(New dim3(n + 1, n2 + 1), 1).unifac_gpu_sum1(dev_beta, dev_VEKI, dev_VTAU, T)
            gpu.Launch(n + 1, 1).unifac_gpu_sum2(dev_sum1, dev_Vx, dev_VQ)
            gpu.CopyFromDevice(dev_sum1, sum1)
            soma_xq = MathEx.Common.Sum(sum1)
            gpu.Launch(n2 + 1, 1).unifac_gpu_sum3(dev_Vx, dev_teta, dev_VQ, dev_VEKI, soma_xq)
            gpu.Launch(n2 + 1, 1).unifac_gpu_sum4(dev_s, dev_teta, dev_VQ, dev_VTAU)
            gpu.Launch(n + 1, 1).unifac_gpu_sum5(dev_sum2, dev_Vx, dev_VR)
            gpu.CopyFromDevice(dev_sum2, sum2)
            soma_xr = MathEx.Common.Sum(sum2)
            gpu.Launch(n + 1, 1).unifac_gpu_sum6(dev_Vgamma, dev_s, dev_VR, dev_VQ, dev_teta, dev_beta, dev_VEKI, soma_xq, soma_xr, n, n2)

            ' copy the arrays back from the GPU to the CPU
            gpu.CopyFromDevice(dev_s, s)
            gpu.CopyFromDevice(dev_teta, teta)
            gpu.CopyFromDevice(dev_Vgamma, Vgamma)

            ' free the memory allocated on the GPU
            gpu.Free(dev_beta)
            gpu.Free(dev_VEKI)
            gpu.Free(dev_VTAU)
            gpu.Free(dev_sum1)
            gpu.Free(dev_sum2)
            gpu.Free(dev_Vx)
            gpu.Free(dev_VQ)
            gpu.Free(dev_VR)
            gpu.Free(dev_s)
            gpu.Free(dev_teta)
            gpu.Free(dev_Vgamma)

            If gpu.IsMultithreadingEnabled Then gpu.Unlock()

        End Sub

        <Cudafy.Cudafy()> Private Shared Sub unifac_gpu_sum1(thread As Cudafy.GThread, beta As Double(,), VEKI As Double(,), VTAU As Double(,), T As Double)

            Dim i As Integer = thread.blockIdx.x
            Dim k As Integer = thread.blockIdx.y
            Dim m As Integer
            Dim n As Integer = beta.GetLength(0)
            Dim n2 As Integer = VEKI.GetLength(1)

            beta(i, k) = 0
            m = 0
            Do
                beta(i, k) = beta(i, k) + VEKI(i, m) * VTAU(m, k)
                m = m + 1
            Loop Until m = n2
            
        End Sub

        <Cudafy.Cudafy()> Private Shared Sub unifac_gpu_sum2(thread As Cudafy.GThread, sum1 As Double(), Vx As Double(), VQ As Double())

            Dim i As Integer = thread.blockIdx.x

            sum1(i) = Vx(i) * VQ(i)

        End Sub

        <Cudafy.Cudafy()> Private Shared Sub unifac_gpu_sum3(thread As Cudafy.GThread, Vx As Double(), teta As Double(), Q As Double(), VEKI As Double(,), soma_xq As Double)

            Dim k As Integer = thread.blockIdx.x
            Dim n As Integer = Vx.GetLength(0)

            Dim i As Integer = 0
            Do
                teta(k) = teta(k) + Vx(i) * Q(i) * VEKI(i, k)
                i = i + 1
            Loop Until i = n
            teta(k) = teta(k) / soma_xq

        End Sub

        <Cudafy.Cudafy()> Private Shared Sub unifac_gpu_sum4(thread As Cudafy.GThread, s As Double(), teta As Double(), Q As Double(), VTAU As Double(,))

            Dim k As Integer = thread.blockIdx.x
            Dim n2 As Integer = VTAU.GetLength(1)
            Dim m As Integer

            s(k) = 0.0#
            m = 0
            Do
                s(k) = s(k) + teta(m) * VTAU(m, k)
                m = m + 1
            Loop Until m = n2

        End Sub

        <Cudafy.Cudafy()> Private Shared Sub unifac_gpu_sum5(thread As Cudafy.GThread, sum2 As Double(), Vx As Double(), VR As Double())

            Dim i As Integer = thread.blockIdx.x

            sum2(i) = Vx(i) * VR(i)

        End Sub

        <Cudafy.Cudafy()> Private Shared Sub unifac_gpu_sum6(thread As Cudafy.GThread, Vgamma As Double(), s As Double(), VR As Double(), VQ As Double(), teta As Double(), beta As Double(,), VEKI As Double(,), soma_xq As Double, soma_xr As Double, n As Integer, n2 As Integer)

            Dim i As Integer = thread.blockIdx.x
            Dim k As Integer = 0
            Dim tmpsum As Double = 0
            Do
                tmpsum = tmpsum + teta(k) * beta(i, k) / s(k) - VEKI(i, k) * Math.Log(beta(i, k) / s(k))
                k = k + 1
            Loop Until k = n2 + 1
            Vgamma(i) = Math.Exp(1 - VR(i) / soma_xr + Math.Log(VR(i) / soma_xr) - 5 * VQ(i) * (1 - VR(i) / soma_xr / (VQ(i) / soma_xq) + Math.Log((VR(i) / soma_xr) / (VQ(i) / soma_xq))) + VQ(i) * (1 - tmpsum))
            
        End Sub

        Function GAMMA_MR_GPU(ByVal T As Double, ByVal Vx As Double(), ByVal VQ As Double(), ByVal VR As Double(), ByVal VEKI As Double(,))

            CheckParameters(VEKI)

            Dim k, m As Integer

            Dim n As Integer = UBound(Vx)
            Dim n2 As Integer = UBound(VEKI, 2)

            Dim teta(n2), s(n2) As Double
            Dim beta(n, n2), Vgammac(n), Vgammar(n), Vgamma(n), b(n, n2), VTAU(n2, n2) As Double
            Dim Q(n), R(n), j(n), L(n) As Double

            For k = 0 To n2
                For m = 0 To n2
                    VTAU(m, k) = TAU(m, k, T)
                Next
            Next

            unifac_gpu_func(n, n2, beta, VTAU, VEKI, T, Vx, s, teta, VQ, VR, Vgamma)

            Return Vgamma

        End Function

        Function GAMMA_MR_CPU(ByVal T, ByVal Vx, ByVal VQ, ByVal VR, ByVal VEKI)

            CheckParameters(VEKI)

            Dim i, k, m As Integer

            Dim n = UBound(Vx)
            Dim n2 = UBound(VEKI, 2)

            Dim teta(n2), s(n2) As Double
            Dim beta(n, n2), Vgammac(n), Vgammar(n), Vgamma(n), b(n, n2) As Double
            Dim Q(n), R(n), j(n), L(n) As Double

            i = 0
            Do
                k = 0
                Do
                    beta(i, k) = 0
                    m = 0
                    Do
                        If VEKI(i, m) <> 0.0# And Not Double.IsNaN(VEKI(i, m)) Then
                            beta(i, k) = beta(i, k) + VEKI(i, m) * TAU(m, k, T)
                        Else

                        End If
                        m = m + 1
                    Loop Until m = n2 + 1
                    k = k + 1
                Loop Until k = n2 + 1
                i = i + 1
            Loop Until i = n + 1

            Dim soma_xq = 0
            i = 0
            Do
                Q(i) = VQ(i)
                soma_xq = soma_xq + Vx(i) * Q(i)
                i = i + 1
            Loop Until i = n + 1

            k = 0
            Do
                i = 0
                Do
                    teta(k) = teta(k) + Vx(i) * Q(i) * VEKI(i, k)
                    i = i + 1
                Loop Until i = n + 1
                teta(k) = teta(k) / soma_xq
                k = k + 1
            Loop Until k = n2 + 1

            k = 0
            Do
                m = 0
                Do
                    If teta(m) <> 0.0# And Not Double.IsNaN(teta(m)) Then s(k) = s(k) + teta(m) * TAU(m, k, T)
                    m = m + 1
                Loop Until m = n2 + 1
                k = k + 1
            Loop Until k = n2 + 1

            Dim soma_xr = 0
            i = 0
            Do
                R(i) = VR(i)
                soma_xr = soma_xr + Vx(i) * R(i)
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                j(i) = R(i) / soma_xr
                L(i) = Q(i) / soma_xq
                Vgammac(i) = 1 - j(i) + Math.Log(j(i)) - 5 * Q(i) * (1 - j(i) / L(i) + Math.Log(j(i) / L(i)))
                k = 0
                Dim tmpsum = 0
                Do
                    tmpsum = tmpsum + teta(k) * beta(i, k) / s(k) - VEKI(i, k) * Math.Log(beta(i, k) / s(k))
                    k = k + 1
                Loop Until k = n2 + 1
                Vgammar(i) = Q(i) * (1 - tmpsum)
                Vgamma(i) = Math.Exp(Vgammac(i) + Vgammar(i))
                If Vgamma(i) = 0 Then Vgamma(i) = 0.000001
                i = i + 1
            Loop Until i = n + 1

            Return Vgamma

        End Function

        Sub CheckParameters(ByVal VEKI)

            Dim i, j As Integer

            Dim n1 = UBound(VEKI, 1)
            Dim n2 = UBound(VEKI, 2)

            Dim ids As New ArrayList

            For i = 0 To n1
                For j = 0 To n2
                    If VEKI(i, j) <> 0.0# And Not ids.Contains(j) Then ids.Add(j)
                Next
            Next

            For Each id1 As Integer In ids
                For Each id2 As Integer In ids
                    If id1 <> id2 Then
                        Dim g1, g2 As Integer
                        g1 = Me.UnifGroups.Groups(id1 + 1).PrimaryGroup
                        g2 = Me.UnifGroups.Groups(id2 + 1).PrimaryGroup
                        If Me.UnifGroups.InteracParam.ContainsKey(g1) Then
                            If Not Me.UnifGroups.InteracParam(g1).ContainsKey(g2) Then
                                If Me.UnifGroups.InteracParam.ContainsKey(g2) Then
                                    If Not Me.UnifGroups.InteracParam(g2).ContainsKey(g1) Then
                                        Throw New Exception("UNIFAC Error: Could not find interaction parameter for groups " & Me.UnifGroups.Groups(id1 + 1).GroupName & " / " & _
                                                            Me.UnifGroups.Groups(id2 + 1).GroupName & ". Activity coefficient calculation will give you inconsistent results for this system.")
                                    End If
                                End If
                            End If
                        Else
                            If Me.UnifGroups.InteracParam.ContainsKey(g2) Then
                                If Not Me.UnifGroups.InteracParam(g2).ContainsKey(g1) Then
                                    Throw New Exception("UNIFAC Error: Could not find interaction parameter for groups " & Me.UnifGroups.Groups(id1 + 1).GroupName & " / " & _
                                                        Me.UnifGroups.Groups(id2 + 1).GroupName & ". Activity coefficient calculation will give you inconsistent results for this system.")
                                End If
                            Else
                                Throw New Exception("UNIFAC Error: Could not find interaction parameter for groups " & Me.UnifGroups.Groups(id1 + 1).GroupName & " / " & _
                                                    Me.UnifGroups.Groups(id2 + 1).GroupName & ". Activity coefficient calculation will give you inconsistent results for this system.")
                            End If
                        End If
                    End If
                Next
            Next

        End Sub

        Function TAU(ByVal group_1, ByVal group_2, ByVal T)

            Dim g1, g2 As Integer
            Dim res As Double
            g1 = Me.UnifGroups.Groups(group_1 + 1).PrimaryGroup
            g2 = Me.UnifGroups.Groups(group_2 + 1).PrimaryGroup

            If Me.UnifGroups.InteracParam.ContainsKey(g1) Then
                If Me.UnifGroups.InteracParam(g1).ContainsKey(g2) Then
                    res = Me.UnifGroups.InteracParam(g1)(g2)
                Else
                    res = 0
                End If
            Else
                res = 0
            End If

            Return Math.Exp(-res / T)

        End Function

        Function RET_Ri(ByVal VN As Object) As Double

            Dim i As Integer = 0
            Dim res As Double

            For Each group As UnifacGroup In Me.UnifGroups.Groups.Values
                res += group.R * VN(i)
                i += 1
            Next

            Return res

        End Function

        Function RET_Qi(ByVal VN As Object) As Double

            Dim i As Integer = 0
            Dim res As Double

            For Each group As UnifacGroup In Me.UnifGroups.Groups.Values
                res += group.Q * VN(i)
                i += 1
            Next

            Return res

        End Function

        Function RET_EKI(ByVal VN As Object, ByVal Q As Double) As Object

            Dim i As Integer = 0
            Dim res As New ArrayList

            For Each group As UnifacGroup In Me.UnifGroups.Groups.Values
                If Q <> 0 Then res.Add(group.Q * VN(i) / Q) Else res.Add(0.0#)
                i += 1
            Next

            Return res.ToArray(Type.GetType("System.Double"))

        End Function

        Function RET_VN(ByVal cp As DWSIM.ClassesBasicasTermodinamica.ConstantProperties)

            Dim i As Integer = 0
            Dim res As New ArrayList
            Dim added As Boolean = False

            res.Clear()

            For Each group As UnifacGroup In Me.UnifGroups.Groups.Values
                For Each s As String In cp.UNIFACGroups.Collection.Keys
                    If s = group.GroupName Then
                        res.Add(cp.UNIFACGroups.Collection(s))
                        added = True
                        Exit For
                    End If
                Next
                If Not added Then res.Add(0)
                added = False
            Next

            Return res.ToArray

        End Function

    End Class

    <System.Serializable()> Public Class UnifacLL

        Public UnifGroups As UnifacGroupsLL

        Sub New()

            UnifGroups = New UnifacGroupsLL

        End Sub

        Function ID2Group(ByVal id As Integer) As String

            For Each group As UnifacGroup In Me.UnifGroups.Groups.Values
                If group.Secondary_Group = id Then Return group.GroupName
            Next

            Return ""

        End Function

        Function Group2ID(ByVal groupname As String) As String

            For Each group As UnifacGroup In Me.UnifGroups.Groups.Values
                If group.GroupName = groupname Then
                    Return group.Secondary_Group
                End If
            Next

            Return 0

        End Function

        Function GAMMA(ByVal T, ByVal Vx, ByVal VQ, ByVal VR, ByVal VEKI, ByVal index)

            CheckParameters(VEKI)

            Dim Q(), R(), j(), L()
            Dim i, k, m As Integer

            Dim n = UBound(Vx)
            Dim n2 = UBound(VEKI, 2)

            Dim beta(,), teta(n2), s(n2), Vgammac(), Vgammar(), Vgamma(), b(,)
            ReDim Preserve beta(n, n2), Vgammac(n), Vgammar(n), Vgamma(n), b(n, n2)
            ReDim Q(n), R(n), j(n), L(n)

            i = 0
            Do
                k = 0
                Do
                    beta(i, k) = 0
                    m = 0
                    Do
                        beta(i, k) = beta(i, k) + VEKI(i, m) * TAU(m, k, T)
                        m = m + 1
                    Loop Until m = n2 + 1
                    k = k + 1
                Loop Until k = n2 + 1
                i = i + 1
            Loop Until i = n + 1

            Dim soma_xq = 0
            i = 0
            Do
                Q(i) = VQ(i)
                soma_xq = soma_xq + Vx(i) * Q(i)
                i = i + 1
            Loop Until i = n + 1

            k = 0
            Do
                i = 0
                Do
                    teta(k) = teta(k) + Vx(i) * Q(i) * VEKI(i, k)
                    i = i + 1
                Loop Until i = n + 1
                teta(k) = teta(k) / soma_xq
                k = k + 1
            Loop Until k = n2 + 1

            k = 0
            Do
                m = 0
                Do
                    s(k) = s(k) + teta(m) * TAU(m, k, T)
                    m = m + 1
                Loop Until m = n2 + 1
                k = k + 1
            Loop Until k = n2 + 1

            Dim soma_xr = 0
            i = 0
            Do
                R(i) = VR(i)
                soma_xr = soma_xr + Vx(i) * R(i)
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                j(i) = R(i) / soma_xr
                L(i) = Q(i) / soma_xq
                Vgammac(i) = 1 - j(i) + Math.Log(j(i)) - 5 * Q(i) * (1 - j(i) / L(i) + Math.Log(j(i) / L(i)))
                k = 0
                Dim tmpsum = 0
                Do
                    tmpsum = tmpsum + teta(k) * beta(i, k) / s(k) - VEKI(i, k) * Math.Log(beta(i, k) / s(k))
                    k = k + 1
                Loop Until k = n2 + 1
                Vgammar(i) = Q(i) * (1 - tmpsum)
                Vgamma(i) = Math.Exp(Vgammac(i) + Vgammar(i))
                If Vgamma(i) = 0 Then Vgamma(i) = 0.000001
                i = i + 1
            Loop Until i = n + 1

            i = 1
            k = 1
            GAMMA = Vgamma(index)

        End Function

        Function GAMMA_MR(ByVal T, ByVal Vx, ByVal VQ, ByVal VR, ByVal VEKI)

            CheckParameters(VEKI)

            Dim i, k, m As Integer

            Dim n = UBound(Vx)
            Dim n2 = UBound(VEKI, 2)

            Dim teta(n2), s(n2) As Double
            Dim beta(n, n2), Vgammac(n), Vgammar(n), Vgamma(n), b(n, n2) As Double
            Dim Q(n), R(n), j(n), L(n) As Double

            i = 0
            Do
                k = 0
                Do
                    beta(i, k) = 0
                    m = 0
                    Do
                        beta(i, k) = beta(i, k) + VEKI(i, m) * TAU(m, k, T)
                        m = m + 1
                    Loop Until m = n2 + 1
                    k = k + 1
                Loop Until k = n2 + 1
                i = i + 1
            Loop Until i = n + 1

            Dim soma_xq = 0
            i = 0
            Do
                Q(i) = VQ(i)
                soma_xq = soma_xq + Vx(i) * Q(i)
                i = i + 1
            Loop Until i = n + 1

            k = 0
            Do
                i = 0
                Do
                    teta(k) = teta(k) + Vx(i) * Q(i) * VEKI(i, k)
                    i = i + 1
                Loop Until i = n + 1
                teta(k) = teta(k) / soma_xq
                k = k + 1
            Loop Until k = n2 + 1

            k = 0
            Do
                m = 0
                Do
                    s(k) = s(k) + teta(m) * TAU(m, k, T)
                    m = m + 1
                Loop Until m = n2 + 1
                k = k + 1
            Loop Until k = n2 + 1

            Dim soma_xr = 0
            i = 0
            Do
                R(i) = VR(i)
                soma_xr = soma_xr + Vx(i) * R(i)
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                j(i) = R(i) / soma_xr
                L(i) = Q(i) / soma_xq
                Vgammac(i) = 1 - j(i) + Math.Log(j(i)) - 5 * Q(i) * (1 - j(i) / L(i) + Math.Log(j(i) / L(i)))
                k = 0
                Dim tmpsum = 0
                Do
                    tmpsum = tmpsum + teta(k) * beta(i, k) / s(k) - VEKI(i, k) * Math.Log(beta(i, k) / s(k))
                    k = k + 1
                Loop Until k = n2 + 1
                Vgammar(i) = Q(i) * (1 - tmpsum)
                Vgamma(i) = Math.Exp(Vgammac(i) + Vgammar(i))
                If Vgamma(i) = 0 Then Vgamma(i) = 0.000001
                i = i + 1
            Loop Until i = n + 1

            Return Vgamma

        End Function

        Sub CheckParameters(ByVal VEKI)

            Dim i, j As Integer

            Dim n1 = UBound(VEKI, 1)
            Dim n2 = UBound(VEKI, 2)

            Dim ids As New ArrayList

            For i = 0 To n1
                For j = 0 To n2
                    If VEKI(i, j) <> 0.0# And Not ids.Contains(j) Then ids.Add(j)
                Next
            Next

            For Each id1 As Integer In ids
                For Each id2 As Integer In ids
                    If id1 <> id2 Then
                        Dim g1, g2 As Integer
                        g1 = Me.UnifGroups.Groups(id1 + 1).PrimaryGroup
                        g2 = Me.UnifGroups.Groups(id2 + 1).PrimaryGroup
                        If Me.UnifGroups.InteracParam.ContainsKey(g1) Then
                            If Not Me.UnifGroups.InteracParam(g1).ContainsKey(g2) Then
                                If Me.UnifGroups.InteracParam.ContainsKey(g2) Then
                                    If Not Me.UnifGroups.InteracParam(g2).ContainsKey(g1) Then
                                        Throw New Exception("UNIFAC Error: Could not find interaction parameter for groups " & Me.UnifGroups.Groups(id1 + 1).GroupName & " / " & _
                                                            Me.UnifGroups.Groups(id2 + 1).GroupName & ". Activity coefficient calculation will give you inconsistent results for this system.")
                                    End If
                                End If
                            End If
                        Else
                            If Me.UnifGroups.InteracParam.ContainsKey(g2) Then
                                If Not Me.UnifGroups.InteracParam(g2).ContainsKey(g1) Then
                                    Throw New Exception("UNIFAC Error: Could not find interaction parameter for groups " & Me.UnifGroups.Groups(id1 + 1).GroupName & " / " & _
                                                        Me.UnifGroups.Groups(id2 + 1).GroupName & ". Activity coefficient calculation will give you inconsistent results for this system.")
                                End If
                            Else
                                Throw New Exception("UNIFAC Error: Could not find interaction parameter for groups " & Me.UnifGroups.Groups(id1 + 1).GroupName & " / " & _
                                                    Me.UnifGroups.Groups(id2 + 1).GroupName & ". Activity coefficient calculation will give you inconsistent results for this system.")
                            End If
                        End If
                    End If
                Next
            Next

        End Sub

        Function TAU(ByVal group_1, ByVal group_2, ByVal T)

            Dim g1, g2 As Integer
            Dim res As Double
            g1 = Me.UnifGroups.Groups(group_1 + 1).PrimaryGroup
            g2 = Me.UnifGroups.Groups(group_2 + 1).PrimaryGroup

            If Me.UnifGroups.InteracParam.ContainsKey(g1) Then
                If Me.UnifGroups.InteracParam(g1).ContainsKey(g2) Then
                    res = Me.UnifGroups.InteracParam(g1)(g2)
                Else
                    res = 0
                End If
            Else
                res = 0
            End If

            Return Math.Exp(-res / T)

        End Function

        Function RET_Ri(ByVal VN As Object) As Double

            Dim i As Integer = 0
            Dim res As Double

            For Each group As UnifacGroup In Me.UnifGroups.Groups.Values
                res += group.R * VN(i)
                i += 1
            Next

            Return res

        End Function

        Function RET_Qi(ByVal VN As Object) As Double

            Dim i As Integer = 0
            Dim res As Double

            For Each group As UnifacGroup In Me.UnifGroups.Groups.Values
                res += group.Q * VN(i)
                i += 1
            Next

            Return res

        End Function

        Function RET_EKI(ByVal VN As Object, ByVal Q As Double) As Object

            Dim i As Integer = 0
            Dim res As New ArrayList

            For Each group As UnifacGroup In Me.UnifGroups.Groups.Values
                If Q <> 0 Then res.Add(group.Q * VN(i) / Q) Else res.Add(0.0#)
                i += 1
            Next

            Return res.ToArray(Type.GetType("System.Double"))

        End Function

        Function RET_VN(ByVal cp As DWSIM.ClassesBasicasTermodinamica.ConstantProperties)

            Dim i As Integer = 0
            Dim res As New ArrayList
            Dim added As Boolean = False

            res.Clear()

            For Each group As UnifacGroup In Me.UnifGroups.Groups.Values
                For Each s As String In cp.UNIFACGroups.Collection.Keys
                    If s = group.GroupName Then
                        res.Add(cp.UNIFACGroups.Collection(s))
                        added = True
                        Exit For
                    End If
                Next
                If Not added Then res.Add(0)
                added = False
            Next

            Return res.ToArray

        End Function

    End Class

    <System.Serializable()> Public Class UnifacGroups

        Public InteracParam As System.Collections.Generic.Dictionary(Of Integer, System.Collections.Generic.Dictionary(Of Integer, Double))
        Protected m_groups As System.Collections.Generic.Dictionary(Of Integer, UnifacGroup)

        Sub New()

            Dim pathsep = System.IO.Path.DirectorySeparatorChar

            m_groups = New System.Collections.Generic.Dictionary(Of Integer, UnifacGroup)
            InteracParam = New System.Collections.Generic.Dictionary(Of Integer, System.Collections.Generic.Dictionary(Of Integer, Double))

            Dim cult As Globalization.CultureInfo = New Globalization.CultureInfo("en-US")

            Dim filename As String = My.Application.Info.DirectoryPath & pathsep & "data" & pathsep & "unifac.txt"
            Dim fields As String()
            Dim delimiter As String = ","
            Using parser As New TextFieldParser(filename)
                parser.SetDelimiters(delimiter)
                fields = parser.ReadFields()
                fields = parser.ReadFields()
                While Not parser.EndOfData
                    fields = parser.ReadFields()
                    Me.Groups.Add(fields(1), New UnifacGroup(fields(2), fields(3), fields(0), fields(1), Double.Parse(fields(4), cult), Double.Parse(fields(5), cult)))
                End While
            End Using

            filename = My.Application.Info.DirectoryPath & pathsep & "data" & pathsep & "unifac_ip.txt"
            Using parser As New TextFieldParser(filename)
                delimiter = vbTab
                parser.SetDelimiters(delimiter)
                fields = parser.ReadFields()
                While Not parser.EndOfData
                    fields = parser.ReadFields()
                    If Not Me.InteracParam.ContainsKey(fields(0)) Then
                        Me.InteracParam.Add(fields(0), New System.Collections.Generic.Dictionary(Of Integer, Double))
                        Me.InteracParam(fields(0)).Add(fields(2), Double.Parse(fields(4), cult))
                    Else
                        If Not Me.InteracParam(fields(0)).ContainsKey(fields(2)) Then
                            Me.InteracParam(fields(0)).Add(fields(2), Double.Parse(fields(4), cult))
                        Else
                            Me.InteracParam(fields(0))(fields(2)) = Double.Parse(fields(4), cult)
                        End If
                    End If
                    If Not Me.InteracParam.ContainsKey(fields(2)) Then
                        Me.InteracParam.Add(fields(2), New System.Collections.Generic.Dictionary(Of Integer, Double))
                        Me.InteracParam(fields(2)).Add(fields(0), Double.Parse(fields(5), cult))
                    Else
                        If Not Me.InteracParam(fields(2)).ContainsKey(fields(0)) Then
                            Me.InteracParam(fields(2)).Add(fields(0), Double.Parse(fields(5), cult))
                        Else
                            Me.InteracParam(fields(2))(fields(0)) = Double.Parse(fields(5), cult)
                        End If
                    End If
                End While
            End Using



        End Sub

        Public ReadOnly Property Groups() As System.Collections.Generic.Dictionary(Of Integer, UnifacGroup)
            Get
                Return m_groups
            End Get
        End Property

    End Class

    <System.Serializable()> Public Class UnifacGroupsLL

        Public InteracParam As System.Collections.Generic.Dictionary(Of Integer, System.Collections.Generic.Dictionary(Of Integer, Double))
        Protected m_groups As System.Collections.Generic.Dictionary(Of Integer, UnifacGroup)

        Sub New()

            Dim pathsep = System.IO.Path.DirectorySeparatorChar

            m_groups = New System.Collections.Generic.Dictionary(Of Integer, UnifacGroup)
            InteracParam = New System.Collections.Generic.Dictionary(Of Integer, System.Collections.Generic.Dictionary(Of Integer, Double))

            Dim cult As Globalization.CultureInfo = New Globalization.CultureInfo("en-US")

            Dim filename As String = My.Application.Info.DirectoryPath & pathsep & "data" & pathsep & "unifac.txt"
            Dim fields As String()
            Dim delimiter As String = ","
            Using parser As New TextFieldParser(filename)
                parser.SetDelimiters(delimiter)
                fields = parser.ReadFields()
                fields = parser.ReadFields()
                While Not parser.EndOfData
                    fields = parser.ReadFields()
                    Me.Groups.Add(fields(1), New UnifacGroup(fields(2), fields(3), fields(0), fields(1), Double.Parse(fields(4), cult), Double.Parse(fields(5), cult)))
                End While
            End Using

            filename = My.Application.Info.DirectoryPath & pathsep & "data" & pathsep & "unifac_ll_ip.txt"
            Using parser As New TextFieldParser(filename)
                delimiter = ","
                parser.SetDelimiters(delimiter)
                While Not parser.EndOfData
                    fields = parser.ReadFields()
                    If Not Me.InteracParam.ContainsKey(fields(0)) Then
                        Me.InteracParam.Add(fields(0), New System.Collections.Generic.Dictionary(Of Integer, Double))
                        Me.InteracParam(fields(0)).Add(fields(2), Double.Parse(fields(4), cult))
                    Else
                        If Not Me.InteracParam(fields(0)).ContainsKey(fields(2)) Then
                            Me.InteracParam(fields(0)).Add(fields(2), Double.Parse(fields(4), cult))
                        Else
                            Me.InteracParam(fields(0))(fields(2)) = Double.Parse(fields(4), cult)
                        End If
                    End If
                    If Not Me.InteracParam.ContainsKey(fields(2)) Then
                        Me.InteracParam.Add(fields(2), New System.Collections.Generic.Dictionary(Of Integer, Double))
                        Me.InteracParam(fields(2)).Add(fields(0), Double.Parse(fields(5), cult))
                    Else
                        If Not Me.InteracParam(fields(2)).ContainsKey(fields(0)) Then
                            Me.InteracParam(fields(2)).Add(fields(0), Double.Parse(fields(5), cult))
                        Else
                            Me.InteracParam(fields(2))(fields(0)) = Double.Parse(fields(5), cult)
                        End If
                    End If
                End While
            End Using



        End Sub

        Public ReadOnly Property Groups() As System.Collections.Generic.Dictionary(Of Integer, UnifacGroup)
            Get
                Return m_groups
            End Get
        End Property

    End Class

    <System.Serializable()> Public Class UnifacGroup

        Protected m_primarygroupname As String
        Protected m_groupname As String

        Protected m_main_group As Integer
        Protected m_secondary_group As Integer

        Protected m_r As Double
        Protected m_q As Double
        Public Property PrimGroupName() As String
            Get
                Return m_primarygroupname
            End Get
            Set(ByVal value As String)
                m_primarygroupname = value
            End Set
        End Property
        Public Property GroupName() As String
            Get
                Return m_groupname
            End Get
            Set(ByVal value As String)
                m_groupname = value
            End Set
        End Property

        Public Property PrimaryGroup() As String
            Get
                Return m_main_group
            End Get
            Set(ByVal value As String)
                m_main_group = value
            End Set
        End Property

        Public Property Secondary_Group() As String
            Get
                Return m_secondary_group
            End Get
            Set(ByVal value As String)
                m_secondary_group = value
            End Set
        End Property

        Public Property R() As Double
            Get
                Return m_r
            End Get
            Set(ByVal value As Double)
                m_r = value
            End Set
        End Property

        Public Property Q() As Double
            Get
                Return m_q
            End Get
            Set(ByVal value As Double)
                m_q = value
            End Set
        End Property

        Sub New(ByVal MainGroupName As String, ByVal Nome As String, ByVal PrimGroup As String, ByVal SecGroup As String, ByVal R As Double, ByVal Q As Double)
            Me.PrimGroupName = MainGroupName
            Me.GroupName = Nome
            Me.PrimaryGroup = PrimGroup
            Me.Secondary_Group = SecGroup
            Me.R = R
            Me.Q = Q
        End Sub

        Sub New()
        End Sub

    End Class

End Namespace