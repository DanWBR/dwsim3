'    Modified UNIFAC (NIST) Property Package 
'    Copyright 2015 Daniel Wagner O. de Medeiros
'    Copyright 2015 Gregor Reichert
'
'    Based on the paper entitled "New modified UNIFAC parameters using critically 
'    evaluated phase equilibrium data", http://dx.doi.org/10.1016/j.fluid.2014.12.042
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

Namespace DWSIM.SimulationObjects.PropertyPackages.Auxiliary

    <System.Serializable()> Public Class NISTMFAC

        Public Shadows ModfGroups As NistModfacGroups

        Sub New()

            ModfGroups = New NistModfacGroups

        End Sub


        Function ID2Group(ByVal id As Integer) As String

            For Each group As ModfacGroup In Me.ModfGroups.Groups.Values
                If group.Secondary_Group = id Then Return group.GroupName
            Next

            Return ""

        End Function

        Function Group2ID(ByVal groupname As String) As String

            For Each group As ModfacGroup In Me.ModfGroups.Groups.Values
                If group.GroupName = groupname Then
                    Return group.Secondary_Group
                End If
            Next

            Return 0

        End Function

        Function GAMMA_MR(ByVal T As Double, ByVal Vx As Double(), ByVal VQ As Double(), ByVal VR As Double(), ByVal VEKI As List(Of Dictionary(Of Integer, Double))) As Double()

            CheckParameters(VEKI)

            Dim i, k, m As Integer

            Dim n = UBound(Vx)

            Dim n2 = Me.ModfGroups.Groups.Count - 1

            Dim teta(n2), s(n2) As Double
            Dim beta(n, n2), Vgammac(n), Vgammar(n), Vgamma(n), b(n, n2) As Double
            Dim Q(n), R(n), j(n), L(n) As Double
            Dim j_(n)

            i = 0
            Do
                k = 0
                Do
                    beta(i, k) = 0.0#
                    m = 0
                    Do
                        If VEKI(i).ContainsKey(m + 1) Then
                            beta(i, k) = beta(i, k) + VEKI(i)(m + 1) * TAU(m, k, T)
                        End If
                        m = m + 1
                    Loop Until m = n2 + 1
                    If beta(i, k) = 0.0# Then beta(i, k) = 1.0#
                    k = k + 1
                Loop Until k = n2 + 1
                i = i + 1
            Loop Until i = n + 1

            Dim soma_xq = 0.0#
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
                    If VEKI(i).ContainsKey(k + 1) Then
                        teta(k) = teta(k) + Vx(i) * Q(i) * VEKI(i)(k + 1)
                    End If
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

            Dim soma_xr = 0.0#
            Dim soma_xr_ = 0.0#
            i = 0
            Do
                R(i) = VR(i)
                soma_xr = soma_xr + Vx(i) * R(i)
                soma_xr_ = soma_xr_ + Vx(i) * R(i) ^ (3 / 4)
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                j(i) = R(i) / soma_xr
                j_(i) = R(i) ^ (3 / 4) / soma_xr_
                L(i) = Q(i) / soma_xq
                Vgammac(i) = 1 - j_(i) + Math.Log(j_(i)) - 5 * Q(i) * (1 - j(i) / L(i) + Math.Log(j(i) / L(i)))
                k = 0
                Dim tmpsum = 0.0#
                Do
                    If VEKI(i).ContainsKey(k + 1) Then
                        tmpsum = tmpsum + teta(k) * beta(i, k) / s(k) - VEKI(i)(k + 1) * Math.Log(beta(i, k) / s(k))
                    End If
                    k = k + 1
                Loop Until k = n2 + 1
                Vgammar(i) = Q(i) * (1 - tmpsum)
                Vgamma(i) = Math.Exp(Vgammac(i) + Vgammar(i))
                If Vgamma(i) = 0 Then Vgamma(i) = 0.000001
                i = i + 1
            Loop Until i = n + 1

            Return Vgamma

        End Function

        Sub CheckParameters(ByVal VEKI As List(Of Dictionary(Of Integer, Double)))

            Dim ids As New ArrayList

            For Each item In VEKI
                For Each item2 In item
                    If item(item2.Key) <> 0.0# And Not ids.Contains(item(item2.Key)) Then ids.Add(item(item2.Key))
                Next
            Next

            For Each id1 As Integer In ids
                For Each id2 As Integer In ids
                    If id1 <> id2 Then
                        Dim g1, g2 As Integer
                        g1 = Me.ModfGroups.Groups(id1 + 1).PrimaryGroup
                        g2 = Me.ModfGroups.Groups(id2 + 1).PrimaryGroup
                        If Me.ModfGroups.InteracParam_aij.ContainsKey(g1) Then
                            If Not Me.ModfGroups.InteracParam_aij(g1).ContainsKey(g2) Then
                                If Me.ModfGroups.InteracParam_aij.ContainsKey(g2) Then
                                    If Not Me.ModfGroups.InteracParam_aij(g2).ContainsKey(g1) And g2 <> g1 Then
                                        Throw New Exception("MODFAC Error: Could not find interaction parameter for groups " & Me.ModfGroups.Groups(id1 + 1).GroupName & " / " & _
                                                            Me.ModfGroups.Groups(id2 + 1).GroupName & ". Activity coefficient calculation will give you inconsistent results for this system.")
                                    End If
                                End If
                            End If
                        Else
                            If Me.ModfGroups.InteracParam_aij.ContainsKey(g2) Then
                                If Not Me.ModfGroups.InteracParam_aij(g2).ContainsKey(g1) And g2 <> g1 Then
                                    Throw New Exception("MODFAC Error: Could not find interaction parameter for groups " & Me.ModfGroups.Groups(id1 + 1).GroupName & " / " & _
                                                        Me.ModfGroups.Groups(id2 + 1).GroupName & ". Activity coefficient calculation will give you inconsistent results for this system.")
                                End If
                            Else
                                Throw New Exception("MODFAC Error: Could not find interaction parameter for groups " & Me.ModfGroups.Groups(id1 + 1).GroupName & " / " & _
                                                    Me.ModfGroups.Groups(id2 + 1).GroupName & ". Activity coefficient calculation will give you inconsistent results for this system.")
                            End If
                        End If
                    End If
                Next
            Next

        End Sub

        Function TAU(ByVal group_1, ByVal group_2, ByVal T)

            Dim g1, g2 As Integer
            Dim res As Double

            If Not Me.ModfGroups.Groups.ContainsKey(group_1 + 1) Or Not Me.ModfGroups.Groups.ContainsKey(group_2 + 1) Then Return 0.0#

            g1 = Me.ModfGroups.Groups(group_1 + 1).PrimaryGroup
            g2 = Me.ModfGroups.Groups(group_2 + 1).PrimaryGroup

            If g1 <> g2 Then
                If Me.ModfGroups.InteracParam_aij.ContainsKey(g1) Then
                    If Me.ModfGroups.InteracParam_aij(g1).ContainsKey(g2) Then
                        res = Me.ModfGroups.InteracParam_aij(g1)(g2) + Me.ModfGroups.InteracParam_bij(g1)(g2) * T + Me.ModfGroups.InteracParam_cij(g1)(g2) * T ^ 2
                    Else
                        If Me.ModfGroups.InteracParam_aij.ContainsKey(g2) Then
                            If Me.ModfGroups.InteracParam_aij(g2).ContainsKey(g1) Then
                                res = Me.ModfGroups.InteracParam_aij(g2)(g1) + Me.ModfGroups.InteracParam_bij(g2)(g1) * T + Me.ModfGroups.InteracParam_cij(g2)(g1) * T ^ 2
                            Else
                                res = 0.0#
                            End If
                        Else
                            res = 0.0#
                        End If
                    End If
                ElseIf Me.ModfGroups.InteracParam_aij.ContainsKey(g2) Then
                    If Me.ModfGroups.InteracParam_aij(g2).ContainsKey(g1) Then
                        res = Me.ModfGroups.InteracParam_aij(g2)(g1) + Me.ModfGroups.InteracParam_bij(g2)(g1) * T + Me.ModfGroups.InteracParam_cij(g2)(g1) * T ^ 2
                    Else
                        res = 0.0#
                    End If
                Else
                    res = 0.0#
                End If
            Else
                res = 0.0#
            End If

            Return Math.Exp(-res / T)

        End Function

        Function RET_Ri(ByVal VN As Dictionary(Of Integer, Double)) As Double

            Dim i As Integer = 0
            Dim res As Double

            For Each kvp In VN
                res += Me.ModfGroups.Groups(kvp.Key).R * VN(kvp.Key)
                i += 1
            Next

            Return res

        End Function

        Function RET_Qi(ByVal VN As Dictionary(Of Integer, Double)) As Double

            Dim i As Integer = 0
            Dim res As Double

            For Each kvp In VN
                res += Me.ModfGroups.Groups(kvp.Key).Q * VN(kvp.Key)
                i += 1
            Next

            Return res

        End Function

        Function RET_EKI(ByVal VN As Dictionary(Of Integer, Double), ByVal Q As Double) As Dictionary(Of Integer, Double)

            Dim i As Integer = 0
            Dim res As New Dictionary(Of Integer, Double)

            For Each kvp In VN
                res.Add(kvp.Key, Me.ModfGroups.Groups(kvp.Key).Q * VN(kvp.Key) / Q)
                i += 1
            Next

            Return res

        End Function

        Function RET_VN(ByVal cp As DWSIM.ClassesBasicasTermodinamica.ConstantProperties) As Dictionary(Of Integer, Double)

            Dim i As Integer = 0
            Dim res As New Dictionary(Of Integer, Double)
            Dim added As Boolean = False

            res.Clear()

            For Each group As ModfacGroup In Me.ModfGroups.Groups.Values
                For Each s As String In cp.MODFACGroups.Collection.Keys
                    If s = group.GroupName Then
                        res.Add(group.Secondary_Group, cp.MODFACGroups.Collection(s))
                        Exit For
                    End If
                Next
            Next

            Return res

        End Function

        Function DLNGAMMA_DT(ByVal T As Double, ByVal Vx As Array, ByVal VQ As Double(), ByVal VR As Double(), ByVal VEKI As List(Of Dictionary(Of Integer, Double))) As Array

            Dim gamma1, gamma2 As Double()

            Dim epsilon As Double = 0.001

            gamma1 = GAMMA_MR(T, Vx, VQ, VR, VEKI)
            gamma2 = GAMMA_MR(T + epsilon, Vx, VQ, VR, VEKI)

            Dim dgamma(gamma1.Length - 1) As Double

            For i As Integer = 0 To Vx.Length - 1
                dgamma(i) = (gamma2(i) - gamma1(i)) / (epsilon)
            Next

            Return dgamma

        End Function

        Function HEX_MIX(ByVal T As Double, ByVal Vx As Array, ByVal VQ As Double(), ByVal VR As Double(), ByVal VEKI As List(Of Dictionary(Of Integer, Double))) As Double

            Dim dgamma As Double() = DLNGAMMA_DT(T, Vx, VQ, VR, VEKI)

            Dim hex As Double = 0.0#

            For i As Integer = 0 To Vx.Length - 1
                hex += -8.314 * T ^ 2 * Vx(i) * dgamma(i)
            Next

            Return hex 'kJ/kmol

        End Function

        Function CPEX_MIX(ByVal T As Double, ByVal Vx As Array, ByVal VQ As Double(), ByVal VR As Double(), ByVal VEKI As List(Of Dictionary(Of Integer, Double))) As Double

            Dim hex1, hex2, cpex As Double

            Dim epsilon As Double = 0.001

            hex1 = HEX_MIX(T, Vx, VQ, VR, VEKI)
            hex2 = HEX_MIX(T + epsilon, Vx, VQ, VR, VEKI)

            cpex = (hex2 - hex1) / epsilon

            Return cpex 'kJ/kmol.K

        End Function

    End Class

    <System.Serializable()> Public Class NistModfacGroups

        Public InteracParam_aij As System.Collections.Generic.Dictionary(Of Integer, System.Collections.Generic.Dictionary(Of Integer, Double))
        Public InteracParam_bij As System.Collections.Generic.Dictionary(Of Integer, System.Collections.Generic.Dictionary(Of Integer, Double))
        Public InteracParam_cij As System.Collections.Generic.Dictionary(Of Integer, System.Collections.Generic.Dictionary(Of Integer, Double))

        Protected m_groups As System.Collections.Generic.Dictionary(Of Integer, ModfacGroup)

        Sub New()

            Dim pathsep = System.IO.Path.DirectorySeparatorChar

            m_groups = New System.Collections.Generic.Dictionary(Of Integer, ModfacGroup)
            InteracParam_aij = New System.Collections.Generic.Dictionary(Of Integer, System.Collections.Generic.Dictionary(Of Integer, Double))
            InteracParam_bij = New System.Collections.Generic.Dictionary(Of Integer, System.Collections.Generic.Dictionary(Of Integer, Double))
            InteracParam_cij = New System.Collections.Generic.Dictionary(Of Integer, System.Collections.Generic.Dictionary(Of Integer, Double))

            Dim cult As Globalization.CultureInfo = New Globalization.CultureInfo("en-US")

            Dim filename As String = My.Application.Info.DirectoryPath & pathsep & "data" & pathsep & "NIST-MODFAC_RiQi.txt"
            Dim fields As String()
            Dim delimiter As String = vbTab
            Dim maingroup As Integer = 1
            Dim mainname As String = ""
            Using parser As New TextFieldParser(filename)
                parser.SetDelimiters(delimiter)
                parser.ReadLine()
                parser.ReadLine()
                Dim i As Integer = 1
                While Not parser.EndOfData
                    fields = parser.ReadFields()
                    If fields(0).StartsWith("(") Then
                        maingroup = fields(0).Split(")")(0).Substring(1)
                        mainname = fields(0).Trim().Split(")")(1).Trim
                    Else
                        'Me.Groups.Add(i, New ModfacGroup(fields(1), mainname, maingroup, fields(0), Double.Parse(fields(3), cult), Double.Parse(fields(2), cult)))
                        Me.Groups.Add(fields(0), New ModfacGroup(fields(1), mainname, maingroup, fields(0), Double.Parse(fields(3), cult), Double.Parse(fields(2), cult)))
                        i += 1
                    End If
                End While
            End Using

            filename = My.Application.Info.DirectoryPath & pathsep & "data" & pathsep & "NIST-MODFAC_IP.txt"
            Using parser As New TextFieldParser(filename)
                delimiter = vbTab
                parser.SetDelimiters(delimiter)
                fields = parser.ReadFields()
                While Not parser.EndOfData
                    fields = parser.ReadFields()
                    If Not Me.InteracParam_aij.ContainsKey(fields(0)) Then
                        Me.InteracParam_aij.Add(fields(0), New System.Collections.Generic.Dictionary(Of Integer, Double))
                        Me.InteracParam_aij(fields(0)).Add(fields(1), Double.Parse(fields(2), cult))
                        Me.InteracParam_bij.Add(fields(0), New System.Collections.Generic.Dictionary(Of Integer, Double))
                        Me.InteracParam_bij(fields(0)).Add(fields(1), Double.Parse(fields(3), cult))
                        Me.InteracParam_cij.Add(fields(0), New System.Collections.Generic.Dictionary(Of Integer, Double))
                        Me.InteracParam_cij(fields(0)).Add(fields(1), Double.Parse(fields(4) / 1000, cult))
                      Else
                        If Not Me.InteracParam_aij(fields(0)).ContainsKey(fields(1)) Then
                            Me.InteracParam_aij(fields(0)).Add(fields(1), Double.Parse(fields(2), cult))
                            Me.InteracParam_bij(fields(0)).Add(fields(1), Double.Parse(fields(3), cult))
                            Me.InteracParam_cij(fields(0)).Add(fields(1), Double.Parse(fields(4) / 1000, cult))
                         Else
                            Me.InteracParam_aij(fields(0))(fields(1)) = Double.Parse(fields(2), cult)
                            Me.InteracParam_bij(fields(0))(fields(1)) = Double.Parse(fields(3), cult)
                            Me.InteracParam_cij(fields(0))(fields(1)) = Double.Parse(fields(4) / 1000, cult)
                        End If
                    End If
                End While
            End Using

        End Sub

        Public ReadOnly Property Groups() As System.Collections.Generic.Dictionary(Of Integer, ModfacGroup)
            Get
                Return m_groups
            End Get
        End Property

    End Class

End Namespace
