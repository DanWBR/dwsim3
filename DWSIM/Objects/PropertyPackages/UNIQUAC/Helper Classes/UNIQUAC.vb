'    UNIQUAC Property Package 
'    Copyright 2008 Daniel Wagner O. de Medeiros
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

Imports System.Collections.Generic
Imports FileHelpers

Namespace DWSIM.SimulationObjects.PropertyPackages.Auxiliary

    <DelimitedRecord(";")> <IgnoreFirst()> <System.Serializable()> _
    Public Class UNIQUAC_IPData

        Implements ICloneable

        Public ID1 As Integer = -1
        Public ID2 As Integer = -1
        Public A12 As Double = 0
        Public A21 As Double = 0
        Public comment As String = ""

        Public Function Clone() As Object Implements System.ICloneable.Clone

            Dim newclass As New UNIQUAC_IPData
            With newclass
                .ID1 = Me.ID1
                .ID2 = Me.ID2
                .A12 = Me.A12
                .A21 = Me.A21
                .comment = Me.comment
            End With
            Return newclass
        End Function

        Public Function CloneToLIQUAC() As LIQUAC2_IPData

            Dim newclass As New LIQUAC2_IPData
            With newclass
                .ID1 = Me.ID1
                .ID2 = Me.ID2
                .Group1 = .ID1
                .Group2 = .ID2
                .A12 = Me.A12
                .A21 = Me.A21
            End With
            Return newclass
        End Function

    End Class

    <System.Serializable()> Public Class UNIQUAC

        Private _ip As Dictionary(Of String, Dictionary(Of String, UNIQUAC_IPData))
        'Private _ip2 As Dictionary(Of String, Dictionary(Of String, UNIQUAC_IPData))

        Public ReadOnly Property InteractionParameters() As Dictionary(Of String, Dictionary(Of String, UNIQUAC_IPData))
            Get
                Return _ip
            End Get
        End Property

        ' Public ReadOnly Property InteractionParameters2() As Dictionary(Of String, Dictionary(Of String, NRTL_IPData))
        '    Get
        '        Return _ip2
        '    End Get
        'End Property

        Sub New()

            _ip = New Dictionary(Of String, Dictionary(Of String, UNIQUAC_IPData))
            '_ip2 = New Dictionary(Of String, Dictionary(Of String, UNIQUAC_IPData))

            Dim pathsep As Char = System.IO.Path.DirectorySeparatorChar

            Dim uniquacip As UNIQUAC_IPData
            Dim uniquacipc() As UNIQUAC_IPData
            'Dim uniquacipc2() As UNIQUAC_IPData
            Dim fh1 As New FileHelperEngine(Of UNIQUAC_IPData)
            'Dim fh2 As New FileHelperEngine(Of UNIQUAC_IPData)
            uniquacipc = fh1.ReadFile(My.Application.Info.DirectoryPath & pathsep & "data" & pathsep & "uniquac.dat")
            'uniquacipc2 = fh2.ReadFile(My.Application.Info.DirectoryPath & pathsep & "data" & pathsep & "uniquacip.dat")

            Dim csdb As New DWSIM.Databases.ChemSep

            For Each uniquacip In uniquacipc
                If Me.InteractionParameters.ContainsKey(csdb.GetDWSIMName(uniquacip.ID1)) Then
                    If Me.InteractionParameters(csdb.GetDWSIMName(uniquacip.ID1)).ContainsKey(csdb.GetDWSIMName(uniquacip.ID2)) Then
                    Else
                        Me.InteractionParameters(csdb.GetDWSIMName(uniquacip.ID1)).Add(csdb.GetDWSIMName(uniquacip.ID2), uniquacip.Clone)
                    End If
                Else
                    Me.InteractionParameters.Add(csdb.GetDWSIMName(uniquacip.ID1), New Dictionary(Of String, UNIQUAC_IPData))
                    Me.InteractionParameters(csdb.GetDWSIMName(uniquacip.ID1)).Add(csdb.GetDWSIMName(uniquacip.ID2), uniquacip.Clone)
                End If
            Next
            For Each uniquacip In uniquacipc
                If Me.InteractionParameters.ContainsKey(csdb.GetCSName(uniquacip.ID1)) Then
                    If Me.InteractionParameters(csdb.GetCSName(uniquacip.ID1)).ContainsKey(csdb.GetCSName(uniquacip.ID2)) Then
                    Else
                        Me.InteractionParameters(csdb.GetCSName(uniquacip.ID1)).Add(csdb.GetCSName(uniquacip.ID2), uniquacip.Clone)
                    End If
                Else
                    Me.InteractionParameters.Add(csdb.GetCSName(uniquacip.ID1), New Dictionary(Of String, UNIQUAC_IPData))
                    Me.InteractionParameters(csdb.GetCSName(uniquacip.ID1)).Add(csdb.GetCSName(uniquacip.ID2), uniquacip.Clone)
                End If
            Next

            uniquacip = Nothing
            uniquacipc = Nothing
            'uniquacipc2 = Nothing
            fh1 = Nothing
            'fh2 = Nothing

        End Sub

        Function GAMMA(ByVal T As Double, ByVal Vx As Array, ByVal Vids As Array, ByVal VQ As Array, ByVal VR As Array, ByVal index As Integer)

            Dim n As Integer = UBound(Vx)

            Dim tau_ij(n, n), tau_ji(n, n), a12(n, n), a21(n, n) As Double

            Dim i, j As Integer

            i = 0
            Do
                j = 0
                Do
                    If Me.InteractionParameters.ContainsKey(Vids(i)) Then
                        If Me.InteractionParameters(Vids(i)).ContainsKey(Vids(j)) Then
                            a12(i, j) = Me.InteractionParameters(Vids(i))(Vids(j)).A12
                            a21(i, j) = Me.InteractionParameters(Vids(i))(Vids(j)).A21
                        Else
                            If Me.InteractionParameters.ContainsKey(Vids(j)) Then
                                If Me.InteractionParameters(Vids(j)).ContainsKey(Vids(i)) Then
                                    a12(i, j) = Me.InteractionParameters(Vids(j))(Vids(i)).A21
                                    a21(i, j) = Me.InteractionParameters(Vids(j))(Vids(i)).A12
                                Else
                                    a12(i, j) = 0
                                    a21(i, j) = 0
                                End If
                            Else
                                a12(i, j) = 0
                                a21(i, j) = 0
                            End If
                        End If
                    Else
                        a12(i, j) = 0
                        a21(i, j) = 0
                    End If
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                j = 0
                Do
                    tau_ij(i, j) = Math.Exp(-a12(i, j) / (1.98721 * T))
                    tau_ji(j, i) = Math.Exp(-a21(i, j) / (1.98721 * T))
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            Dim r, q As Double

            i = 0
            Do
                r += Vx(i) * VR(i)
                q += Vx(i) * VQ(i)
                i = i + 1
            Loop Until i = n + 1

            Dim teta(n), fi(n), l(n), S(n), lngc(n), lngr(n), lng(n), g(n), sum1(n), sum2 As Double
            Dim z As Double = 10.0#

            i = 0
            Do
                fi(i) = Vx(i) * VR(i) / r
                teta(i) = Vx(i) * VQ(i) / q
                l(i) = z / 2 * (VR(i) - VQ(i)) - (VR(i) - 1)
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                S(i) = 0
                j = 0
                Do
                    S(i) += teta(j) * tau_ji(j, i)
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                sum1(i) = 0
                j = 0
                Do
                    sum1(i) += teta(j) * tau_ij(i, j) / S(j)
                    j = j + 1
                Loop Until j = n + 1
                sum2 += Vx(i) * l(i)
                i = i + 1
            Loop Until i = n + 1

           
            i = 0
            Do
                'from chemsep book (error!)
                'lngc(i) = (1 - z / 2 * VQ(i)) * Math.Log(fi(i) / Vx(i)) + z / 2 * VQ(i) * Math.Log(teta(i) / Vx(i)) - VR(i) / r + z / 2 * VQ(i) * (VR(i) / r - VQ(i) / q)
                'from wikipedia (correct?!)
                'lngc(i) = 1 - VR(i) / r + Math.Log(VR(i) / r) - z / 2 * VQ(i) * (1 - fi(i) / teta(i) + Math.Log(fi(i) / teta(i)))
                'lngr(i) = VQ(i) * (1 - Math.Log(S(i)) - sum1(i))
                'lng(i) = lngc(i) + lngr(i)
                lng(i) = Math.Log(fi(i) / Vx(i)) + z / 2 * VQ(i) * Math.Log(teta(i) / fi(i)) + l(i) - fi(i) / Vx(i) * sum2 - VQ(i) * Math.Log(S(i)) + VQ(i) - VQ(i) * sum1(i)
                g(i) = Math.Exp(lng(i))
                i = i + 1
            Loop Until i = n + 1

            Return g(index)

        End Function

        Function GAMMA_DINF(ByVal T, ByVal Vx, ByVal Vids, ByVal VQ, ByVal VR, ByVal index)

            Dim n As Integer = UBound(Vx)

            Dim tau_ij(n, n), tau_ji(n, n), a12(n, n), a21(n, n), Vx2(n) As Double

            Dim i, j As Integer

            i = 0
            Do
                j = 0
                Do
                    If Me.InteractionParameters.ContainsKey(Vids(i)) Then
                        If Me.InteractionParameters(Vids(i)).ContainsKey(Vids(j)) Then
                            a12(i, j) = Me.InteractionParameters(Vids(i))(Vids(j)).A12
                            a21(i, j) = Me.InteractionParameters(Vids(i))(Vids(j)).A21
                        Else
                            If Me.InteractionParameters.ContainsKey(Vids(j)) Then
                                If Me.InteractionParameters(Vids(j)).ContainsKey(Vids(i)) Then
                                    a12(i, j) = Me.InteractionParameters(Vids(j))(Vids(i)).A21
                                    a21(i, j) = Me.InteractionParameters(Vids(j))(Vids(i)).A12
                                Else
                                    a12(i, j) = 0
                                    a21(i, j) = 0
                                End If
                            Else
                                a12(i, j) = 0
                                a21(i, j) = 0
                            End If
                        End If
                    Else
                        a12(i, j) = 0
                        a21(i, j) = 0
                    End If
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                j = 0
                Do
                    tau_ij(i, j) = Math.Exp(-a12(i, j) / (1.98721 * T))
                    tau_ji(j, i) = Math.Exp(-a21(i, j) / (1.98721 * T))
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Dim Vxid_ant = Vx(index)
            Do
                If i <> index Then
                    Vx2(n) = Vx(i) + Vxid_ant / (n - 1)
                End If
                Vx2(index) = 1.0E-20
                i = i + 1
            Loop Until i = n + 1

            Dim r, q As Double

            i = 0
            Do
                r += Vx2(i) * VR(i)
                q += Vx2(i) * VQ(i)
                i = i + 1
            Loop Until i = n + 1

            Dim teta(n), fi(n), S(n), lngc(n), lngr(n), lng(n), g(n), sum1(n) As Double

            i = 0
            Do
                fi(i) = Vx2(i) * VR(i) / r
                teta(i) = Vx2(i) * VQ(i) / q
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                S(i) = 0
                j = 0
                Do
                    S(i) += teta(i) * tau_ji(j, i)
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                sum1(i) = 0
                j = 0
                Do
                    sum1(i) += teta(j) * tau_ij(i, j) / S(j)
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            Dim z As Double = 10.0#

            i = 0
            Do
                'from chemsep book (error!)
                'lngc(i) = (1 - z / 2 * VQ(i)) * Math.Log(fi(i) / Vx(i)) + z / 2 * VQ(i) * Math.Log(teta(i) / Vx(i)) - VR(i) / r + z / 2 * VQ(i) * (VR(i) / r - VQ(i) / q)
                'from wikipedia (correct?!)
                lngc(i) = 1 - VR(i) / r + Math.Log(VR(i) / r) - z / 2 * VQ(i) * (1 - fi(i) / teta(i) + Math.Log(fi(i) / teta(i)))
                lngr(i) = VQ(i) * (1 - Math.Log(S(i)) - sum1(i))
                lng(i) = lngc(i) + lngr(i)
                g(i) = Math.Exp(lng(i))
                i = i + 1
            Loop Until i = n + 1

            Return g(index)

        End Function

        Function GAMMA_MR(ByVal T, ByVal Vx, ByVal Vids, ByVal VQ, ByVal VR)

            Dim n As Integer = UBound(Vx)

            Dim tau_ij(n, n), tau_ji(n, n), a12(n, n), a21(n, n) As Double

            Dim i, j As Integer

            i = 0
            Do
                j = 0
                Do
                    If Me.InteractionParameters.ContainsKey(Vids(i)) Then
                        If Me.InteractionParameters(Vids(i)).ContainsKey(Vids(j)) Then
                            a12(i, j) = Me.InteractionParameters(Vids(i))(Vids(j)).A12
                            a21(i, j) = Me.InteractionParameters(Vids(i))(Vids(j)).A21
                        Else
                            If Me.InteractionParameters.ContainsKey(Vids(j)) Then
                                If Me.InteractionParameters(Vids(j)).ContainsKey(Vids(i)) Then
                                    a12(i, j) = Me.InteractionParameters(Vids(j))(Vids(i)).A21
                                    a21(i, j) = Me.InteractionParameters(Vids(j))(Vids(i)).A12
                                Else
                                    a12(i, j) = 0
                                    a21(i, j) = 0
                                End If
                            Else
                                a12(i, j) = 0
                                a21(i, j) = 0
                            End If
                        End If
                    Else
                        a12(i, j) = 0
                        a21(i, j) = 0
                    End If
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                j = 0
                Do
                    tau_ij(i, j) = Math.Exp(-a12(i, j) / (1.98721 * T))
                    tau_ji(j, i) = Math.Exp(-a21(i, j) / (1.98721 * T))
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            Dim r, q As Double

            i = 0
            Do
                r += Vx(i) * VR(i)
                q += Vx(i) * VQ(i)
                i = i + 1
            Loop Until i = n + 1

            Dim teta(n), fi(n), l(n), S(n), lngc(n), lngr(n), lng(n), g(n), sum1(n), sum2 As Double
            Dim z As Double = 10.0#

            i = 0
            Do
                fi(i) = Vx(i) * VR(i) / r
                teta(i) = Vx(i) * VQ(i) / q
                l(i) = z / 2 * (VR(i) - VQ(i)) - (VR(i) - 1)
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                S(i) = 0
                j = 0
                Do
                    S(i) += teta(j) * tau_ji(j, i)
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                sum1(i) = 0
                j = 0
                Do
                    sum1(i) += teta(j) * tau_ij(i, j) / S(j)
                    j = j + 1
                Loop Until j = n + 1
                sum2 += Vx(i) * l(i)
                i = i + 1
            Loop Until i = n + 1


            i = 0
            Do
                'from chemsep book (error!)
                'lngc(i) = (1 - z / 2 * VQ(i)) * Math.Log(fi(i) / Vx(i)) + z / 2 * VQ(i) * Math.Log(teta(i) / Vx(i)) - VR(i) / r + z / 2 * VQ(i) * (VR(i) / r - VQ(i) / q)
                'from wikipedia (correct?!)
                'lngc(i) = 1 - VR(i) / r + Math.Log(VR(i) / r) - z / 2 * VQ(i) * (1 - fi(i) / teta(i) + Math.Log(fi(i) / teta(i)))
                'lngr(i) = VQ(i) * (1 - Math.Log(S(i)) - sum1(i))
                'lng(i) = lngc(i) + lngr(i)
                lng(i) = Math.Log(fi(i) / Vx(i)) + z / 2 * VQ(i) * Math.Log(teta(i) / fi(i)) + l(i) - fi(i) / Vx(i) * sum2 - VQ(i) * Math.Log(S(i)) + VQ(i) - VQ(i) * sum1(i)
                g(i) = Math.Exp(lng(i))
                i = i + 1
            Loop Until i = n + 1

            Return g


        End Function

        Function GAMMA_DINF_MR(ByVal T, ByVal Vx, ByVal Vids, ByVal VQ, ByVal VR, ByVal index)

            Dim n As Integer = UBound(Vx)

            Dim tau_ij(n, n), tau_ji(n, n), a12(n, n), a21(n, n), Vx2(n) As Double

            Dim i, j As Integer

            i = 0
            Do
                j = 0
                Do
                    If Me.InteractionParameters.ContainsKey(Vids(i)) Then
                        If Me.InteractionParameters(Vids(i)).ContainsKey(Vids(j)) Then
                            a12(i, j) = Me.InteractionParameters(Vids(i))(Vids(j)).A12
                            a21(i, j) = Me.InteractionParameters(Vids(i))(Vids(j)).A21
                        Else
                            If Me.InteractionParameters.ContainsKey(Vids(j)) Then
                                If Me.InteractionParameters(Vids(j)).ContainsKey(Vids(i)) Then
                                    a12(i, j) = Me.InteractionParameters(Vids(j))(Vids(i)).A21
                                    a21(i, j) = Me.InteractionParameters(Vids(j))(Vids(i)).A12
                                Else
                                    a12(i, j) = 0
                                    a21(i, j) = 0
                                End If
                            Else
                                a12(i, j) = 0
                                a21(i, j) = 0
                            End If
                        End If
                    Else
                        a12(i, j) = 0
                        a21(i, j) = 0
                    End If
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                j = 0
                Do
                    tau_ij(i, j) = Math.Exp(-a12(i, j) / (1.98721 * T))
                    tau_ji(j, i) = Math.Exp(-a21(i, j) / (1.98721 * T))
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Dim Vxid_ant = Vx(index)
            Do
                If i <> index Then
                    Vx2(n) = Vx(i) + Vxid_ant / (n - 1)
                End If
                Vx2(index) = 1.0E-20
                i = i + 1
            Loop Until i = n + 1

            Dim r, q As Double

            i = 0
            Do
                r += Vx2(i) * VR(i)
                q += Vx2(i) * VQ(i)
                i = i + 1
            Loop Until i = n + 1

            Dim teta(n), fi(n), S(n), lngc(n), lngr(n), lng(n), g(n), sum1(n) As Double

            i = 0
            Do
                fi(i) = Vx2(i) * VR(i) / r
                teta(i) = Vx2(i) * VQ(i) / q
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                S(i) = 0
                j = 0
                Do
                    S(i) += teta(i) * tau_ji(j, i)
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                sum1(i) = 0
                j = 0
                Do
                    sum1(i) += teta(j) * tau_ij(i, j) / S(j)
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            Dim z As Double = 10.0#

            i = 0
            Do
                'from chemsep book (error!)
                'lngc(i) = (1 - z / 2 * VQ(i)) * Math.Log(fi(i) / Vx(i)) + z / 2 * VQ(i) * Math.Log(teta(i) / Vx(i)) - VR(i) / r + z / 2 * VQ(i) * (VR(i) / r - VQ(i) / q)
                'from wikipedia (correct?!)
                lngc(i) = 1 - VR(i) / r + Math.Log(VR(i) / r) - z / 2 * VQ(i) * (1 - fi(i) / teta(i) + Math.Log(fi(i) / teta(i)))
                lngr(i) = VQ(i) * (1 - Math.Log(S(i)) - sum1(i))
                lng(i) = lngc(i) + lngr(i)
                g(i) = Math.Exp(lng(i))
                i = i + 1
            Loop Until i = n + 1

            Return g

        End Function

    End Class

End Namespace
