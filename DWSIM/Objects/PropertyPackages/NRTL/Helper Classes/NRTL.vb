'    NRTL Property Package 
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
    Public Class NRTL_IPData

        Implements ICloneable

        <FieldConverter(GetType(ObjectConverter))> Public ID1 As Object = ""
        <FieldConverter(GetType(ObjectConverter))> Public ID2 As Object = ""
        Public A12 As Double = 0
        Public A21 As Double = 0
        Public alpha12 As Double = 0
        Public comment As String = ""
        <FieldIgnored()> Public B12 As Double = 0
        <FieldIgnored()> Public B21 As Double = 0
        <FieldIgnored()> Public C12 As Double = 0
        <FieldIgnored()> Public C21 As Double = 0

        Public Function Clone() As Object Implements System.ICloneable.Clone

            Dim newclass As New NRTL_IPData
            With newclass
                .ID1 = Me.ID1
                .ID2 = Me.ID2
                .A12 = Me.A12
                .A21 = Me.A21
                .B12 = Me.B12
                .B21 = Me.B21
                .C12 = Me.C12
                .C21 = Me.C21
                .alpha12 = Me.alpha12
                .comment = Me.comment
            End With
            Return newclass
        End Function

    End Class

    <System.Serializable()> Public Class ObjectConverter

        Inherits ConverterBase

        Public Overrides Function StringToField(ByVal from As String) As Object
            Return [from]
        End Function


        Public Overrides Function FieldToString(ByVal fieldValue As Object) As String

            Return fieldValue.ToString

        End Function

    End Class

    <System.Serializable()> Public Class NRTL

        Private _ip As Dictionary(Of String, Dictionary(Of String, NRTL_IPData))

        Public ReadOnly Property InteractionParameters() As Dictionary(Of String, Dictionary(Of String, NRTL_IPData))
            Get
                Return _ip
            End Get
        End Property

        Sub New()

            _ip = New Dictionary(Of String, Dictionary(Of String, NRTL_IPData))

            Dim pathsep As Char = System.IO.Path.DirectorySeparatorChar

            Dim nrtlip As NRTL_IPData
            Dim nrtlipc() As NRTL_IPData
            Dim fh1 As New FileHelperEngine(Of NRTL_IPData)
            With fh1
                nrtlipc = .ReadFile(My.Application.Info.DirectoryPath & pathsep & "data" & pathsep & "nrtl.dat")
            End With

            Dim csdb As New DWSIM.Databases.ChemSep

            'load ChemSep database interactions
            For Each nrtlip In nrtlipc
                If Me.InteractionParameters.ContainsKey(csdb.GetDWSIMName(nrtlip.ID1)) Then
                    If Not Me.InteractionParameters(csdb.GetDWSIMName(nrtlip.ID1)).ContainsKey(csdb.GetDWSIMName(nrtlip.ID2)) Then
                        Me.InteractionParameters(csdb.GetDWSIMName(nrtlip.ID1)).Add(csdb.GetDWSIMName(nrtlip.ID2), nrtlip.Clone)
                    End If
                    If Not Me.InteractionParameters(csdb.GetDWSIMName(nrtlip.ID1)).ContainsKey(csdb.GetCSName(nrtlip.ID2)) Then
                        Me.InteractionParameters(csdb.GetDWSIMName(nrtlip.ID1)).Add(csdb.GetCSName(nrtlip.ID2), nrtlip.Clone)
                    End If
                Else
                    Me.InteractionParameters.Add(csdb.GetDWSIMName(nrtlip.ID1), New Dictionary(Of String, NRTL_IPData))
                    Me.InteractionParameters(csdb.GetDWSIMName(nrtlip.ID1)).Add(csdb.GetDWSIMName(nrtlip.ID2), nrtlip.Clone)
                    If Not Me.InteractionParameters(csdb.GetDWSIMName(nrtlip.ID1)).ContainsKey(csdb.GetCSName(nrtlip.ID2)) Then
                        Me.InteractionParameters(csdb.GetDWSIMName(nrtlip.ID1)).Add(csdb.GetCSName(nrtlip.ID2), nrtlip.Clone)
                    End If
                End If
            Next

            For Each nrtlip In nrtlipc
                If Me.InteractionParameters.ContainsKey(csdb.GetCSName(nrtlip.ID1)) Then
                    If Not Me.InteractionParameters(csdb.GetCSName(nrtlip.ID1)).ContainsKey(csdb.GetCSName(nrtlip.ID2)) Then
                        Me.InteractionParameters(csdb.GetCSName(nrtlip.ID1)).Add(csdb.GetCSName(nrtlip.ID2), nrtlip.Clone)
                    End If
                    If Not Me.InteractionParameters(csdb.GetCSName(nrtlip.ID1)).ContainsKey(csdb.GetDWSIMName(nrtlip.ID2)) Then
                        Me.InteractionParameters(csdb.GetCSName(nrtlip.ID1)).Add(csdb.GetDWSIMName(nrtlip.ID2), nrtlip.Clone)
                    End If
                Else
                    Me.InteractionParameters.Add(csdb.GetCSName(nrtlip.ID1), New Dictionary(Of String, NRTL_IPData))
                    Me.InteractionParameters(csdb.GetCSName(nrtlip.ID1)).Add(csdb.GetCSName(nrtlip.ID2), nrtlip.Clone)
                    If Not Me.InteractionParameters(csdb.GetCSName(nrtlip.ID1)).ContainsKey(csdb.GetDWSIMName(nrtlip.ID2)) Then
                        Me.InteractionParameters(csdb.GetCSName(nrtlip.ID1)).Add(csdb.GetDWSIMName(nrtlip.ID2), nrtlip.Clone)
                    End If
                End If
            Next

            'load user database interactions
            If Not My.Settings.UserInteractionsDatabases Is Nothing Then
                For Each IPDBPath As String In My.Settings.UserInteractionsDatabases
                    Dim Interactions As DWSIM.ClassesBasicasTermodinamica.InteractionParameters()
                    Dim IP As DWSIM.ClassesBasicasTermodinamica.InteractionParameters
                    Dim IPD As New NRTL_IPData
                    Try
                        Interactions = DWSIM.Databases.UserDB.ReadInteractions(IPDBPath, "NRTL")
                        For Each IP In Interactions
                            IPD.A12 = IP.Parameters.Collection.Item("A12").ToString
                            IPD.A21 = IP.Parameters.Collection.Item("A21").ToString
                            IPD.alpha12 = IP.Parameters.Collection.Item("alpha12").ToString
                            IPD.comment = IP.Parameters.Collection.Item("Description")
                            If IP.Parameters.Collection.ContainsKey("B12") Then IPD.B12 = IP.Parameters.Collection.Item("B12").ToString
                            If IP.Parameters.Collection.ContainsKey("B21") Then IPD.B21 = IP.Parameters.Collection.Item("B21").ToString
                            If IP.Parameters.Collection.ContainsKey("C12") Then IPD.C12 = IP.Parameters.Collection.Item("C12").ToString
                            If IP.Parameters.Collection.ContainsKey("C21") Then IPD.C21 = IP.Parameters.Collection.Item("C21").ToString

                            If Me.InteractionParameters.ContainsKey(IP.Comp1) Then
                                If Me.InteractionParameters(IP.Comp1).ContainsKey(IP.Comp2) Then
                                Else
                                    Me.InteractionParameters(IP.Comp1).Add(IP.Comp2, IPD.Clone)
                                End If
                            Else
                                Me.InteractionParameters.Add(IP.Comp1, New Dictionary(Of String, NRTL_IPData))
                                Me.InteractionParameters(IP.Comp1).Add(IP.Comp2, IPD.Clone)
                            End If
                        Next
                    Catch ex As Exception
                        MessageBox.Show(ex.Message, DWSIM.App.GetLocalString("Erroaocarregararquiv"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                    End Try
                Next
            End If

            'load biodiesel database interactions
            With fh1
                nrtlipc = .ReadFile(My.Application.Info.DirectoryPath & pathsep & "data" & pathsep & "biod_intparm_nrtl.dat")
            End With

            For Each nrtlip In nrtlipc
                If Me.InteractionParameters.ContainsKey(nrtlip.ID1) Then
                    If Me.InteractionParameters((nrtlip.ID1)).ContainsKey((nrtlip.ID2)) Then
                    Else
                        Me.InteractionParameters((nrtlip.ID1)).Add((nrtlip.ID2), nrtlip.Clone)
                    End If
                Else
                    Me.InteractionParameters.Add((nrtlip.ID1), New Dictionary(Of String, NRTL_IPData))
                    Me.InteractionParameters((nrtlip.ID1)).Add((nrtlip.ID2), nrtlip.Clone)
                End If
            Next

            nrtlip = Nothing
            nrtlipc = Nothing
            fh1 = Nothing

        End Sub

        Function GAMMA(ByVal T As Double, ByVal Vx As Array, ByVal Vids As Array, ByVal index As Integer) As Double

            Dim n As Integer = UBound(Vx)

            Dim sum1(n), sum2(n), sum3(n), sum4(n), sum5(n) As Double
            Dim Gij(n, n), tau_ij(n, n), Gji(n, n), tau_ji(n, n), alpha12(n, n) As Double

            Dim i, j As Integer

            i = 0
            Do
                j = 0
                Do
                    If Me.InteractionParameters.ContainsKey(Vids(i)) Then
                        If Me.InteractionParameters(Vids(i)).ContainsKey(Vids(j)) Then
                            tau_ij(i, j) = (Me.InteractionParameters(Vids(i))(Vids(j)).A12 + Me.InteractionParameters(Vids(i))(Vids(j)).B12 * T + Me.InteractionParameters(Vids(i))(Vids(j)).C12 * T ^ 2) / (1.98721 * T)
                            tau_ji(i, j) = (Me.InteractionParameters(Vids(i))(Vids(j)).A21 + Me.InteractionParameters(Vids(i))(Vids(j)).B21 * T + Me.InteractionParameters(Vids(i))(Vids(j)).C21 * T ^ 2) / (1.98721 * T)
                            alpha12(i, j) = Me.InteractionParameters(Vids(i))(Vids(j)).alpha12
                        Else
                            If Me.InteractionParameters.ContainsKey(Vids(j)) Then
                                If Me.InteractionParameters(Vids(j)).ContainsKey(Vids(i)) Then
                                    tau_ji(i, j) = (Me.InteractionParameters(Vids(j))(Vids(i)).A12 + Me.InteractionParameters(Vids(j))(Vids(i)).B12 * T + Me.InteractionParameters(Vids(j))(Vids(i)).C12 * T ^ 2) / (1.98721 * T)
                                    tau_ij(i, j) = (Me.InteractionParameters(Vids(j))(Vids(i)).A21 + Me.InteractionParameters(Vids(j))(Vids(i)).B21 * T + Me.InteractionParameters(Vids(j))(Vids(i)).C21 * T ^ 2) / (1.98721 * T)
                                    alpha12(i, j) = Me.InteractionParameters(Vids(j))(Vids(i)).alpha12
                                Else
                                    tau_ij(i, j) = 0
                                    tau_ji(i, j) = 0
                                    alpha12(i, j) = 0
                                End If
                            Else
                                tau_ij(i, j) = 0
                                tau_ji(i, j) = 0
                                alpha12(i, j) = 0
                            End If
                        End If
                    ElseIf Me.InteractionParameters.ContainsKey(Vids(j)) Then
                        If Me.InteractionParameters(Vids(j)).ContainsKey(Vids(i)) Then
                            tau_ji(i, j) = (Me.InteractionParameters(Vids(j))(Vids(i)).A12 + Me.InteractionParameters(Vids(j))(Vids(i)).B12 * T + Me.InteractionParameters(Vids(j))(Vids(i)).C12 * T ^ 2) / (1.98721 * T)
                            tau_ij(i, j) = (Me.InteractionParameters(Vids(j))(Vids(i)).A21 + Me.InteractionParameters(Vids(j))(Vids(i)).B21 * T + Me.InteractionParameters(Vids(j))(Vids(i)).C21 * T ^ 2) / (1.98721 * T)
                            alpha12(i, j) = Me.InteractionParameters(Vids(j))(Vids(i)).alpha12
                        Else
                            tau_ij(i, j) = 0
                            tau_ji(i, j) = 0
                            alpha12(i, j) = 0
                        End If
                    Else
                        tau_ij(i, j) = 0
                        tau_ji(i, j) = 0
                        alpha12(i, j) = 0
                    End If
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                j = 0
                Do
                    Gij(i, j) = Math.Exp(-alpha12(i, j) * tau_ij(i, j))
                    Gji(i, j) = Math.Exp(-alpha12(i, j) * tau_ji(i, j))
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            Dim S(n), C(n) As Double

            i = 0
            Do
                S(i) = 0
                C(i) = 0
                j = 0
                Do
                    S(i) += Vx(j) * Gji(i, j)
                    C(i) += Vx(j) * Gji(i, j) * tau_ji(i, j)
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            Dim Vg(n), lnVg(n) As Double

            i = 0
            Do
                lnVg(i) = C(i) / S(i)
                j = 0
                Do
                    lnVg(i) += Vx(j) * Gij(i, j) * (tau_ij(i, j) - C(j) / S(j)) / S(j)
                    Vg(i) = Math.Exp(lnVg(i))
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            Return Vg(index)

        End Function

        Function GAMMA_DINF(ByVal T As Double, ByVal Vx As Array, ByVal Vids As Array, ByVal index As Integer) As Double

            Dim n As Integer = UBound(Vx)

            Dim sum1(n), sum2(n), sum3(n), sum4(n), sum5(n), Vx2(n) As Double
            Dim Gij(n, n), tau_ij(n, n), Gji(n, n), tau_ji(n, n), alpha12(n, n) As Double

            Dim i, j As Integer

            i = 0
            Do
                j = 0
                Do
                    If Me.InteractionParameters.ContainsKey(Vids(i)) Then
                        If Me.InteractionParameters(Vids(i)).ContainsKey(Vids(j)) Then
                            tau_ij(i, j) = (Me.InteractionParameters(Vids(i))(Vids(j)).A12 + Me.InteractionParameters(Vids(i))(Vids(j)).B12 * T + Me.InteractionParameters(Vids(i))(Vids(j)).C12 * T ^ 2) / (1.98721 * T)
                            tau_ji(i, j) = (Me.InteractionParameters(Vids(i))(Vids(j)).A21 + Me.InteractionParameters(Vids(i))(Vids(j)).B21 * T + Me.InteractionParameters(Vids(i))(Vids(j)).C21 * T ^ 2) / (1.98721 * T)
                            alpha12(i, j) = Me.InteractionParameters(Vids(i))(Vids(j)).alpha12
                        Else
                            If Me.InteractionParameters.ContainsKey(Vids(j)) Then
                                If Me.InteractionParameters(Vids(j)).ContainsKey(Vids(i)) Then
                                    tau_ji(i, j) = (Me.InteractionParameters(Vids(j))(Vids(i)).A12 + Me.InteractionParameters(Vids(j))(Vids(i)).B12 * T + Me.InteractionParameters(Vids(j))(Vids(i)).C12 * T ^ 2) / (1.98721 * T)
                                    tau_ij(i, j) = (Me.InteractionParameters(Vids(j))(Vids(i)).A21 + Me.InteractionParameters(Vids(j))(Vids(i)).B21 * T + Me.InteractionParameters(Vids(j))(Vids(i)).C21 * T ^ 2) / (1.98721 * T)
                                    alpha12(i, j) = Me.InteractionParameters(Vids(j))(Vids(i)).alpha12
                                Else
                                    tau_ij(i, j) = 0
                                    tau_ji(i, j) = 0
                                    alpha12(i, j) = 0
                                End If
                            Else
                                tau_ij(i, j) = 0
                                tau_ji(i, j) = 0
                                alpha12(i, j) = 0
                            End If
                        End If
                    ElseIf Me.InteractionParameters.ContainsKey(Vids(j)) Then
                        If Me.InteractionParameters(Vids(j)).ContainsKey(Vids(i)) Then
                            tau_ji(i, j) = (Me.InteractionParameters(Vids(j))(Vids(i)).A12 + Me.InteractionParameters(Vids(j))(Vids(i)).B12 * T + Me.InteractionParameters(Vids(j))(Vids(i)).C12 * T ^ 2) / (1.98721 * T)
                            tau_ij(i, j) = (Me.InteractionParameters(Vids(j))(Vids(i)).A21 + Me.InteractionParameters(Vids(j))(Vids(i)).B21 * T + Me.InteractionParameters(Vids(j))(Vids(i)).C21 * T ^ 2) / (1.98721 * T)
                            alpha12(i, j) = Me.InteractionParameters(Vids(j))(Vids(i)).alpha12
                        Else
                            tau_ij(i, j) = 0
                            tau_ji(i, j) = 0
                            alpha12(i, j) = 0
                        End If
                    Else
                        tau_ij(i, j) = 0
                        tau_ji(i, j) = 0
                        alpha12(i, j) = 0
                    End If
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                j = 0
                Do
                    Gij(i, j) = Math.Exp(-alpha12(i, j) * tau_ij(i, j))
                    Gji(i, j) = Math.Exp(-alpha12(i, j) * tau_ji(i, j))
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

            Dim S(n), C(n) As Double

            i = 0
            Do
                S(i) = 0
                C(i) = 0
                j = 0
                Do
                    S(i) += Vx2(j) * Gji(j, i)
                    C(i) += Vx2(j) * Gji(i, j) * tau_ji(i, j)
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            Dim Vg(n), lnVg(n) As Double

            i = 0
            Do
                lnVg(i) = C(i) / S(i)
                j = 0
                Do
                    lnVg(i) += Vx2(j) * Gij(i, j) * (tau_ij(i, j) - C(j) / S(j)) / S(j)
                    Vg(i) = Math.Exp(lnVg(i))
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            Return Vg(index)

        End Function

        Function GAMMA_MR(ByVal T As Double, ByVal Vx As Array, ByVal Vids As Array) As Array

            Dim n As Integer = UBound(Vx)

            Dim Gij(n, n), tau_ij(n, n), Gji(n, n), tau_ji(n, n), alpha12(n, n) As Double

            Dim i, j As Integer

            i = 0
            Do
                j = 0
                Do
                    If Me.InteractionParameters.ContainsKey(Vids(i)) Then
                        If Me.InteractionParameters(Vids(i)).ContainsKey(Vids(j)) Then
                            tau_ij(i, j) = (Me.InteractionParameters(Vids(i))(Vids(j)).A12 + Me.InteractionParameters(Vids(i))(Vids(j)).B12 * T + Me.InteractionParameters(Vids(i))(Vids(j)).C12 * T ^ 2) / (1.98721 * T)
                            tau_ji(i, j) = (Me.InteractionParameters(Vids(i))(Vids(j)).A21 + Me.InteractionParameters(Vids(i))(Vids(j)).B21 * T + Me.InteractionParameters(Vids(i))(Vids(j)).C21 * T ^ 2) / (1.98721 * T)
                            alpha12(i, j) = Me.InteractionParameters(Vids(i))(Vids(j)).alpha12
                        Else
                            If Me.InteractionParameters.ContainsKey(Vids(j)) Then
                                If Me.InteractionParameters(Vids(j)).ContainsKey(Vids(i)) Then
                                    tau_ji(i, j) = (Me.InteractionParameters(Vids(j))(Vids(i)).A12 + Me.InteractionParameters(Vids(j))(Vids(i)).B12 * T + Me.InteractionParameters(Vids(j))(Vids(i)).C12 * T ^ 2) / (1.98721 * T)
                                    tau_ij(i, j) = (Me.InteractionParameters(Vids(j))(Vids(i)).A21 + Me.InteractionParameters(Vids(j))(Vids(i)).B21 * T + Me.InteractionParameters(Vids(j))(Vids(i)).C21 * T ^ 2) / (1.98721 * T)
                                    alpha12(i, j) = Me.InteractionParameters(Vids(j))(Vids(i)).alpha12
                                Else
                                    tau_ij(i, j) = 0
                                    tau_ji(i, j) = 0
                                    alpha12(i, j) = 0
                                End If
                            Else
                                tau_ij(i, j) = 0
                                tau_ji(i, j) = 0
                                alpha12(i, j) = 0
                            End If
                        End If
                    ElseIf Me.InteractionParameters.ContainsKey(Vids(j)) Then
                        If Me.InteractionParameters(Vids(j)).ContainsKey(Vids(i)) Then
                            tau_ji(i, j) = (Me.InteractionParameters(Vids(j))(Vids(i)).A12 + Me.InteractionParameters(Vids(j))(Vids(i)).B12 * T + Me.InteractionParameters(Vids(j))(Vids(i)).C12 * T ^ 2) / (1.98721 * T)
                            tau_ij(i, j) = (Me.InteractionParameters(Vids(j))(Vids(i)).A21 + Me.InteractionParameters(Vids(j))(Vids(i)).B21 * T + Me.InteractionParameters(Vids(j))(Vids(i)).C21 * T ^ 2) / (1.98721 * T)
                            alpha12(i, j) = Me.InteractionParameters(Vids(j))(Vids(i)).alpha12
                        Else
                            tau_ij(i, j) = 0
                            tau_ji(i, j) = 0
                            alpha12(i, j) = 0
                        End If
                    Else
                        tau_ij(i, j) = 0
                        tau_ji(i, j) = 0
                        alpha12(i, j) = 0
                    End If
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                j = 0
                Do
                    Gij(i, j) = Math.Exp(-alpha12(i, j) * tau_ij(i, j))
                    Gji(i, j) = Math.Exp(-alpha12(i, j) * tau_ji(i, j))
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            Dim S(n), C(n) As Double

            i = 0
            Do
                S(i) = 0
                C(i) = 0
                j = 0
                Do
                    S(i) += Vx(j) * Gji(i, j)
                    C(i) += Vx(j) * Gji(i, j) * tau_ji(i, j)
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            Dim Vg(n), lnVg(n) As Double

            i = 0
            Do
                lnVg(i) = C(i) / S(i)
                j = 0
                Do
                    lnVg(i) += Vx(j) * Gij(i, j) * (tau_ij(i, j) - C(j) / S(j)) / S(j)
                    Vg(i) = Math.Exp(lnVg(i))
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            Return Vg

        End Function

        Function GAMMA_DINF_MR(ByVal T As Double, ByVal Vx As Array, ByVal Vids As Array, ByVal index As Integer) As Array

            Dim n As Integer = UBound(Vx)

            Dim sum1(n), sum2(n), sum3(n), sum4(n), sum5(n), Vx2(n) As Double
            Dim Gij(n, n), tau_ij(n, n), Gji(n, n), tau_ji(n, n), alpha12(n, n) As Double

            Dim i, j As Integer

            i = 0
            Do
                j = 0
                Do
                    If Me.InteractionParameters.ContainsKey(Vids(i)) Then
                        If Me.InteractionParameters(Vids(i)).ContainsKey(Vids(j)) Then
                            tau_ij(i, j) = (Me.InteractionParameters(Vids(i))(Vids(j)).A12 + Me.InteractionParameters(Vids(i))(Vids(j)).B12 * T + Me.InteractionParameters(Vids(i))(Vids(j)).C12 * T ^ 2) / (1.98721 * T)
                            tau_ji(i, j) = (Me.InteractionParameters(Vids(i))(Vids(j)).A21 + Me.InteractionParameters(Vids(i))(Vids(j)).B21 * T + Me.InteractionParameters(Vids(i))(Vids(j)).C21 * T ^ 2) / (1.98721 * T)
                            alpha12(i, j) = Me.InteractionParameters(Vids(i))(Vids(j)).alpha12
                        Else
                            If Me.InteractionParameters.ContainsKey(Vids(j)) Then
                                If Me.InteractionParameters(Vids(j)).ContainsKey(Vids(i)) Then
                                    tau_ji(i, j) = (Me.InteractionParameters(Vids(j))(Vids(i)).A12 + Me.InteractionParameters(Vids(j))(Vids(i)).B12 * T + Me.InteractionParameters(Vids(j))(Vids(i)).C12 * T ^ 2) / (1.98721 * T)
                                    tau_ij(i, j) = (Me.InteractionParameters(Vids(j))(Vids(i)).A21 + Me.InteractionParameters(Vids(j))(Vids(i)).B21 * T + Me.InteractionParameters(Vids(j))(Vids(i)).C21 * T ^ 2) / (1.98721 * T)
                                    alpha12(i, j) = Me.InteractionParameters(Vids(j))(Vids(i)).alpha12
                                Else
                                    tau_ij(i, j) = 0
                                    tau_ji(i, j) = 0
                                    alpha12(i, j) = 0
                                End If
                            Else
                                tau_ij(i, j) = 0
                                tau_ji(i, j) = 0
                                alpha12(i, j) = 0
                            End If
                        End If
                    ElseIf Me.InteractionParameters.ContainsKey(Vids(j)) Then
                        If Me.InteractionParameters(Vids(j)).ContainsKey(Vids(i)) Then
                            tau_ji(i, j) = (Me.InteractionParameters(Vids(j))(Vids(i)).A12 + Me.InteractionParameters(Vids(j))(Vids(i)).B12 * T + Me.InteractionParameters(Vids(j))(Vids(i)).C12 * T ^ 2) / (1.98721 * T)
                            tau_ij(i, j) = (Me.InteractionParameters(Vids(j))(Vids(i)).A21 + Me.InteractionParameters(Vids(j))(Vids(i)).B21 * T + Me.InteractionParameters(Vids(j))(Vids(i)).C21 * T ^ 2) / (1.98721 * T)
                            alpha12(i, j) = Me.InteractionParameters(Vids(j))(Vids(i)).alpha12
                        Else
                            tau_ij(i, j) = 0
                            tau_ji(i, j) = 0
                            alpha12(i, j) = 0
                        End If
                    Else
                        tau_ij(i, j) = 0
                        tau_ji(i, j) = 0
                        alpha12(i, j) = 0
                    End If
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                j = 0
                Do
                    Gij(i, j) = Math.Exp(-alpha12(i, j) * tau_ij(i, j))
                    Gji(i, j) = Math.Exp(-alpha12(i, j) * tau_ji(i, j))
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

            Dim S(n), C(n) As Double

            i = 0
            Do
                S(i) = 0
                C(i) = 0
                j = 0
                Do
                    S(i) += Vx2(j) * Gji(j, i)
                    C(i) += Vx2(j) * Gji(i, j) * tau_ji(i, j)
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            Dim Vg(n), lnVg(n) As Double

            i = 0
            Do
                lnVg(i) = C(i) / S(i)
                j = 0
                Do
                    lnVg(i) += Vx2(j) * Gij(i, j) * (tau_ij(i, j) - C(j) / S(j)) / S(j)
                    Vg(i) = Math.Exp(lnVg(i))
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            Return Vg

        End Function

    End Class

End Namespace
