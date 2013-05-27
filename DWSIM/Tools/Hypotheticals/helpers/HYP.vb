﻿'    Hypotheticals Calculation Routines 
'    Copyright 2008/2009 Daniel Wagner O. de Medeiros
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

Namespace DWSIM.Utilities.Hypos.Methods

    <System.Serializable()> Public Class Joback

        Protected m_groups As System.Collections.Generic.Dictionary(Of Integer, JobackGroup)

        Public ReadOnly Property Groups() As System.Collections.Generic.Dictionary(Of Integer, JobackGroup)
            Get
                Return m_groups
            End Get
        End Property

        Sub New()

            Dim pathsep = System.IO.Path.DirectorySeparatorChar
            Dim cult As Globalization.CultureInfo = New Globalization.CultureInfo("en-US")

            m_groups = New System.Collections.Generic.Dictionary(Of Integer, JobackGroup)

            Dim filename As String = My.Application.Info.DirectoryPath & pathsep & "data" & pathsep & "joback.txt"
            Dim fields As String()
            Dim delimiter As String = vbTab
            Using parser As New FileIO.TextFieldParser(filename)
                parser.SetDelimiters(delimiter)
                fields = parser.ReadFields()
                While Not parser.EndOfData
                    fields = parser.ReadFields()
                    Me.Groups.Add(fields(0), New JobackGroup(fields(1), fields(0), Double.Parse(fields(4), cult), _
                    Double.Parse(fields(5), cult), Double.Parse(fields(6), cult), Double.Parse(fields(7), cult), _
                    Double.Parse(fields(8), cult), Double.Parse(fields(9), cult), Double.Parse(fields(10), cult), _
                    Double.Parse(fields(15), cult), Double.Parse(fields(16), cult), Double.Parse(fields(12), cult), _
                    Double.Parse(fields(11), cult), Double.Parse(fields(13), cult), Double.Parse(fields(14), cult), Double.Parse(fields(17), cult)))
                End While
            End Using

        End Sub

        Public Function CalcMW(ByVal n() As Integer) As Double

            Dim sum1 As Double
            Dim i As Integer

            sum1 = 0
            For Each jg As JobackGroup In Me.Groups.Values
                sum1 += jg.MW * n(i)
                i = i + 1
            Next

            Dim fval As Double

            fval = sum1

            Return fval 'kg/kmol

        End Function

        Public Function CalcTb(ByVal n() As Integer) As Double

            Dim sum1 As Double
            Dim i As Integer

            sum1 = 0
            For Each jg As JobackGroup In Me.Groups.Values
                sum1 += jg.TB * n(i)
                i = i + 1
            Next

            Dim fval As Double

            fval = 198 + sum1

            Return fval 'K

        End Function

        Public Function CalcTc(ByVal Tb As Double, ByVal n() As Integer) As Double

            Dim sum1 As Double
            Dim i As Integer

            sum1 = 0
            For Each jg As JobackGroup In Me.Groups.Values
                sum1 += jg.TC * n(i)
                i = i + 1
            Next

            Dim fval As Double

            fval = Tb * (0.584 + 0.965 * sum1 - sum1 ^ 2) ^ -1

            Return fval 'K

        End Function

        Public Function CalcPc(ByVal n() As Integer) As Double

            Dim sum1, sum2 As Double
            Dim i As Integer

            sum1 = 0
            sum2 = 0
            For Each jg As JobackGroup In Me.Groups.Values
                sum1 += jg.PC * n(i)
                sum2 += jg.NA * n(i)
                i = i + 1
            Next

            Dim fval As Double

            fval = 101325 * (0.113 + 0.0032 * sum2 - sum1) ^ -2

            Return fval 'Pa

        End Function

        Public Function CalcVc(ByVal n() As Integer) As Double

            Dim sum1 As Double
            Dim i As Integer

            sum1 = 0
            For Each jg As JobackGroup In Me.Groups.Values
                sum1 += jg.VC * n(i)
                i = i + 1
            Next

            Dim fval As Double

            fval = (17.5 + sum1) / 1000

            Return fval 'm3/kmol

        End Function

        Public Function CalcTf(ByVal n() As Integer) As Double

            Dim sum1 As Double
            Dim i As Integer

            sum1 = 0
            For Each jg As JobackGroup In Me.Groups.Values
                sum1 += jg.TF * n(i)
                i = i + 1
            Next

            Dim fval As Double

            fval = 122.5 + sum1

            Return fval 'K

        End Function

        Public Function CalcHf(ByVal n() As Integer) As Double

            Dim sum1 As Double
            Dim i As Integer

            sum1 = 0
            For Each jg As JobackGroup In Me.Groups.Values
                sum1 += jg.HF * n(i)
                i = i + 1
            Next

            Dim fval As Double

            fval = (-0.88 + sum1) * 1000

            Return fval 'KJ/kmol

        End Function

        Public Function CalcDHf(ByVal n() As Integer) As Double

            Dim sum1 As Double
            Dim i As Integer

            sum1 = 0
            For Each jg As JobackGroup In Me.Groups.Values
                sum1 += jg.DH * n(i)
                i = i + 1
            Next

            Dim fval As Double

            fval = (68.29 + sum1) * 1000

            Return fval 'kJ/kmol

        End Function

        Public Function CalcDGf(ByVal n() As Integer) As Double

            Dim sum1 As Double
            Dim i As Integer

            sum1 = 0
            For Each jg As JobackGroup In Me.Groups.Values
                sum1 += jg.DG * n(i)
                i = i + 1
            Next

            Dim fval As Double

            fval = (53.88 + sum1) * 1000

            Return fval 'kJ/kmol

        End Function

        Public Function CalcCpA(ByVal n() As Integer) As Double

            Dim sum1 As Double
            Dim i As Integer

            sum1 = 0
            For Each jg As JobackGroup In Me.Groups.Values
                sum1 += jg.A * n(i)
                i = i + 1
            Next

            Dim fval As Double

            fval = sum1 - 37.93

            Return fval 'for Cp in kJ/kmol.K

        End Function

        Public Function CalcCpB(ByVal n() As Integer) As Double

            Dim sum1 As Double
            Dim i As Integer

            sum1 = 0
            For Each jg As JobackGroup In Me.Groups.Values
                sum1 += jg.B * n(i)
                i = i + 1
            Next

            Dim fval As Double

            fval = sum1 + 0.21

            Return fval 'for Cp in kJ/kmol.K

        End Function

        Public Function CalcCpC(ByVal n() As Integer) As Double

            Dim sum1 As Double
            Dim i As Integer

            sum1 = 0
            For Each jg As JobackGroup In Me.Groups.Values
                sum1 += jg.C * n(i)
                i = i + 1
            Next

            Dim fval As Double

            fval = sum1 - 0.000391

            Return fval 'for Cp in kJ/kmol.K

        End Function

        Public Function CalcCpD(ByVal n() As Integer) As Double

            Dim sum1 As Double
            Dim i As Integer

            sum1 = 0
            For Each jg As JobackGroup In Me.Groups.Values
                sum1 += jg.D * n(i)
                i = i + 1
            Next

            Dim fval As Double

            fval = sum1 + 0.000000206

            Return fval 'for Cp in kJ/kmol.K

        End Function

    End Class

    <System.Serializable()> Public Class JobackGroup

        Private m_group As String
        Private m_groupid As Integer
        Private _mw, _a, _b, _c, _d, _dh, _dg, _tc, _pc, _tb, _tf, _na, _vc, _hf As Double

        Sub New()

        End Sub

        Sub New(ByVal group As String, ByVal id As Integer, ByVal mw As Double, ByVal a As Double, ByVal b As Double, ByVal c As Double, ByVal d As Double, ByVal dh As Double, ByVal dg As Double, ByVal tc As Double, ByVal pc As Double, ByVal vc As Double, ByVal na As Double, ByVal tb As Double, ByVal tf As Double, ByVal hf As Double)

            Me.Group = group
            Me.ID = id
            Me.A = a
            Me.B = b
            Me.C = c
            Me.D = d
            Me.DH = dh
            Me.DG = dg
            Me.TC = tc
            Me.TB = tb
            Me.PC = pc
            Me.VC = vc
            Me.TF = tf
            Me.NA = na
            Me.MW = mw
            Me.HF = hf

        End Sub

        Public Property MW() As Double
            Get
                Return _mw
            End Get
            Set(ByVal value As Double)
                _mw = value
            End Set
        End Property

        Public Property TB() As Double
            Get
                Return _tb
            End Get
            Set(ByVal value As Double)
                _tb = value
            End Set
        End Property
        Public Property HF() As Double
            Get
                Return _hf
            End Get
            Set(ByVal value As Double)
                _hf = value
            End Set
        End Property
        Public Property TF() As Double
            Get
                Return _tf
            End Get
            Set(ByVal value As Double)
                _tf = value
            End Set
        End Property

        Public Property NA() As Double
            Get
                Return _na
            End Get
            Set(ByVal value As Double)
                _na = value
            End Set
        End Property

        Public Property VC() As Double
            Get
                Return _vc
            End Get
            Set(ByVal value As Double)
                _vc = value
            End Set
        End Property

        Public Property PC() As Double
            Get
                Return _pc
            End Get
            Set(ByVal value As Double)
                _pc = value
            End Set
        End Property

        Public Property TC() As Double
            Get
                Return _tc
            End Get
            Set(ByVal value As Double)
                _tc = value
            End Set
        End Property

        Public Property DG() As Double
            Get
                Return _dg
            End Get
            Set(ByVal value As Double)
                _dg = value
            End Set
        End Property

        Public Property DH() As Double
            Get
                Return _dh
            End Get
            Set(ByVal value As Double)
                _dh = value
            End Set
        End Property

        Public Property D() As Double
            Get
                Return _d
            End Get
            Set(ByVal value As Double)
                _d = value
            End Set
        End Property

        Public Property C() As Double
            Get
                Return _c
            End Get
            Set(ByVal value As Double)
                _c = value
            End Set
        End Property

        Public Property B() As Double
            Get
                Return _b
            End Get
            Set(ByVal value As Double)
                _b = value
            End Set
        End Property

        Public Property A() As Double
            Get
                Return _a
            End Get
            Set(ByVal value As Double)
                _a = value
            End Set
        End Property

        Public Property ID() As Integer
            Get
                Return m_groupid
            End Get
            Set(ByVal value As Integer)
                m_groupid = value
            End Set
        End Property

        Public Property Group() As String
            Get
                Return m_group
            End Get
            Set(ByVal value As String)
                m_group = value
            End Set
        End Property

    End Class

    <System.Serializable()> Public Class HYP

        Sub New()

        End Sub

        Function Tc_Joback(ByVal Tb As Double, ByVal nCH3 As Integer, ByVal nCH2 As Integer, ByVal nCH As Integer, ByVal nOH As Integer, _
                            ByVal nACH As Integer, ByVal nACCH2 As Integer, ByVal nACCH3 As Integer, ByVal nACOH As Integer, _
                            ByVal nCH3CO As Integer, ByVal nCH2CO As Integer)

            Dim pCH3 As Integer = 0.0141
            Dim pCH2 As Integer = 0.0189
            Dim pCH As Integer = 0.0164
            Dim pOH As Integer = 0.0741
            Dim pACH As Integer = 0.0082
            Dim pACCH2 As Integer = 0.0256
            Dim pACCH3 As Integer = 0.0284
            Dim pACOH As Integer = 0.0884
            Dim pCH3CO As Integer = 0.0521
            Dim pCH2CO As Integer = 0.0569

            Dim Tc As Double = 0

            Dim sum = nCH3 * pCH3 + nCH2 * pCH2 + nCH * pCH + _
                nOH * pOH + nACH * pACH + nACCH2 * pACCH2 + _
                nACCH3 * pACCH3 + nACOH * pACOH + _
                nCH3CO * pCH3CO + nCH2CO * pCH2CO

            Tc = Tb * (0.584 + 0.965 * sum - sum ^ 2) ^ -1

            Return Tc

        End Function

        Function Pc_Joback(ByVal nCH3 As Integer, ByVal nCH2 As Integer, ByVal nCH As Integer, ByVal nOH As Integer, _
                            ByVal nACH As Integer, ByVal nACCH2 As Integer, ByVal nACCH3 As Integer, ByVal nACOH As Integer, _
                            ByVal nCH3CO As Integer, ByVal nCH2CO As Integer)

            Dim pCH3 As Integer = -0.0012
            Dim pCH2 As Integer = 0
            Dim pCH As Integer = 0.002
            Dim pOH As Integer = 0.0112
            Dim pACH As Integer = 0.0011
            Dim pACCH2 As Integer = -0.002
            Dim pACCH3 As Integer = 0.0004
            Dim pACOH As Integer = 0.012
            Dim pCH3CO As Integer = 0.0019
            Dim pCH2CO As Integer = 0.0031

            Dim Pc As Double = 0

            Dim sum = nCH3 * pCH3 + nCH2 * pCH2 + nCH * pCH + _
                nOH * pOH + nACH * pACH + nACCH2 * pACCH2 + _
                nACCH3 * pACCH3 + nACOH * pACOH + _
                nCH3CO * pCH3CO + nCH2CO * pCH2CO

            Dim na = nCH3 * 4 + nCH2 * 3 + nCH * 2 + _
                nOH * 2 + nACH * 2 + nACCH2 * 4 + _
                nACCH3 * 5 + nACOH * 3 + _
                nCH3CO * 6 + nCH2CO * 5

            Pc = (0.113 + 0.0032 * na - sum) ^ -2

            Return Pc * 100000

        End Function

        Function Vc_Joback(ByVal nCH3 As Integer, ByVal nCH2 As Integer, ByVal nCH As Integer, ByVal nOH As Integer, _
                            ByVal nACH As Integer, ByVal nACCH2 As Integer, ByVal nACCH3 As Integer, ByVal nACOH As Integer, _
                            ByVal nCH3CO As Integer, ByVal nCH2CO As Integer)

            Dim pCH3 As Integer = 65
            Dim pCH2 As Integer = 56
            Dim pCH As Integer = 41
            Dim pOH As Integer = 28
            Dim pACH As Integer = 41
            Dim pACCH2 As Integer = 88
            Dim pACCH3 As Integer = 97
            Dim pACOH As Integer = 60
            Dim pCH3CO As Integer = 127
            Dim pCH2CO As Integer = 118

            Dim Vc As Double = 0

            Dim sum = nCH3 * pCH3 + nCH2 * pCH2 + nCH * pCH + _
                nOH * pOH + nACH * pACH + nACCH2 * pACCH2 + _
                nACCH3 * pACCH3 + nACOH * pACOH + _
                nCH3CO * pCH3CO + nCH2CO * pCH2CO

            Vc = 17.5 + sum

            Return Vc * 0.000001 * 1000

        End Function

        Function Tb_Joback(ByVal nCH3 As Integer, ByVal nCH2 As Integer, ByVal nCH As Integer, ByVal nOH As Integer, _
                            ByVal nACH As Integer, ByVal nACCH2 As Integer, ByVal nACCH3 As Integer, ByVal nACOH As Integer, _
                            ByVal nCH3CO As Integer, ByVal nCH2CO As Integer)

            Dim pCH3 As Integer = 23.58
            Dim pCH2 As Integer = 22.88
            Dim pCH As Integer = 21.74
            Dim pOH As Integer = 92.88
            Dim pACH As Integer = 26.73
            Dim pACCH2 As Integer = 49.19
            Dim pACCH3 As Integer = 54.59
            Dim pACOH As Integer = 123.89
            Dim pCH3CO As Integer = 100.33
            Dim pCH2CO As Integer = 99.63

            Dim Tb As Double = 0

            Dim sum = nCH3 * pCH3 + nCH2 * pCH2 + nCH * pCH + _
                nOH * pOH + nACH * pACH + nACCH2 * pACCH2 + _
                nACCH3 * pACCH3 + nACOH * pACOH + _
                nCH3CO * pCH3CO + nCH2CO * pCH2CO

            Tb = 198 + sum

            Return Tb

        End Function

        Function Hf298_Marrero_Gani(ByVal nCH3 As Integer, ByVal nCH2 As Integer, ByVal nCH As Integer, ByVal nOH As Integer, _
                                   ByVal nACH As Integer, ByVal nACCH2 As Integer, ByVal nACCH3 As Integer, ByVal nACOH As Integer, _
                                   ByVal nCH3CO As Integer, ByVal nCH2CO As Integer)

            Dim pCH3 As Integer = -42.479
            Dim pCH2 As Integer = -20.829
            Dim pCH As Integer = -7.122
            Dim pOH As Integer = -178.36
            Dim pACH As Integer = 12.861
            Dim pACCH2 As Integer = 4.38
            Dim pACCH3 As Integer = -19.258
            Dim pACOH As Integer = -164.191
            Dim pCH3CO As Integer = -180.604
            Dim pCH2CO As Integer = -163.09

            Dim Hf298 As Double = 0

            Dim sum = nCH3 * pCH3 + nCH2 * pCH2 + nCH * pCH + _
                nOH * pOH + nACH * pACH + nACCH2 * pACCH2 + _
                nACCH3 * pACCH3 + nACOH * pACOH + _
                nCH3CO * pCH3CO + nCH2CO * pCH2CO

            Hf298 = 5.549 + sum

            Return Hf298 'kJ/mol

        End Function

        Function Gf298_Marrero_Gani(ByVal nCH3 As Integer, ByVal nCH2 As Integer, ByVal nCH As Integer, ByVal nOH As Integer, _
                                   ByVal nACH As Integer, ByVal nACCH2 As Integer, ByVal nACCH3 As Integer, ByVal nACOH As Integer, _
                                   ByVal nCH3CO As Integer, ByVal nCH2CO As Integer)

            Dim pCH3 As Integer = 2.878
            Dim pCH2 As Integer = 8.064
            Dim pCH As Integer = 8.254
            Dim pOH As Integer = -144.015
            Dim pACH As Integer = 26.732
            Dim pACCH2 As Integer = 31.663
            Dim pACCH3 As Integer = 24.919
            Dim pACOH As Integer = -131.327
            Dim pCH3CO As Integer = -120.667
            Dim pCH2CO As Integer = -120.425

            Dim Gf298 As Double = 0

            Dim sum = nCH3 * pCH3 + nCH2 * pCH2 + nCH * pCH + _
                nOH * pOH + nACH * pACH + nACCH2 * pACCH2 + _
                nACCH3 * pACCH3 + nACOH * pACOH + _
                nCH3CO * pCH3CO + nCH2CO * pCH2CO

            Gf298 = -34.967 + sum

            Return Gf298 'kJ/mol

        End Function

        Function Sf298_Marrero_Gani(ByVal nCH3 As Integer, ByVal nCH2 As Integer, ByVal nCH As Integer, ByVal nOH As Integer, _
                                   ByVal nACH As Integer, ByVal nACCH2 As Integer, ByVal nACCH3 As Integer, ByVal nACOH As Integer, _
                                   ByVal nCH3CO As Integer, ByVal nCH2CO As Integer)

            Dim pCH3 As Integer = -0.1521
            Dim pCH2 As Integer = -0.0969
            Dim pCH As Integer = -0.0516
            Dim pOH As Integer = -0.1152
            Dim pACH As Integer = -0.0465
            Dim pACCH2 As Integer = -0.0915
            Dim pACCH3 As Integer = -0.1482
            Dim pACOH As Integer = -0.1102
            Dim pCH3CO As Integer = -0.201
            Dim pCH2CO As Integer = -0.1431

            Dim Sf298 As Double = 0

            Dim sum = nCH3 * pCH3 + nCH2 * pCH2 + nCH * pCH + _
                nOH * pOH + nACH * pACH + nACCH2 * pACCH2 + _
                nACCH3 * pACCH3 + nACOH * pACOH + _
                nCH3CO * pCH3CO + nCH2CO * pCH2CO

            Sf298 = 0.135891 + sum

            Return Sf298 'kJ/mol.K

        End Function

        Function MM_UNIFAC(ByVal nCH3 As Integer, ByVal nCH2 As Integer, ByVal nCH As Integer, ByVal nOH As Integer, _
                                   ByVal nACH As Integer, ByVal nACCH2 As Integer, ByVal nACCH3 As Integer, ByVal nACOH As Integer, _
                                   ByVal nCH3CO As Integer, ByVal nCH2CO As Integer)

            Dim pCH3 As Integer = 15
            Dim pCH2 As Integer = 14
            Dim pCH As Integer = 13
            Dim pOH As Integer = 17
            Dim pACH As Integer = 13
            Dim pACCH2 As Integer = 26
            Dim pACCH3 As Integer = 27
            Dim pACOH As Integer = 29
            Dim pCH3CO As Integer = 43
            Dim pCH2CO As Integer = 42

            Dim sum = nCH3 * pCH3 + nCH2 * pCH2 + nCH * pCH + _
                nOH * pOH + nACH * pACH + nACCH2 * pACCH2 + _
                nACCH3 * pACCH3 + nACOH * pACOH + _
                nCH3CO * pCH3CO + nCH2CO * pCH2CO

            Return sum 'kg/kmol

        End Function

        Function DHvb_Vetere(ByVal Tc As Double, ByVal Pc As Double, ByVal Tb As Double) As Double

            Dim R = 8.314
            Dim Tbr = Tb / Tc

            Pc = Pc / 100000

            Return (R * Tc * Tbr * (0.4343 * Math.Log(Pc) - 0.69431 + 0.8954 * Tbr) / (0.37691 - 0.37306 * Tbr + 0.15075 * Pc ^ -1 * Tbr ^ -2))
            'kJ/kmol

        End Function

        Function Element_UNIFAC(ByVal element As String, ByVal nCH3 As Integer, ByVal nCH2 As Integer, ByVal nCH As Integer, ByVal nOH As Integer, _
                                   ByVal nACH As Integer, ByVal nACCH2 As Integer, ByVal nACCH3 As Integer, ByVal nACOH As Integer, _
                                   ByVal nCH3CO As Integer, ByVal nCH2CO As Integer)

            Select Case element
                Case "C"
                    Dim pCH3 As Integer = 1
                    Dim pCH2 As Integer = 1
                    Dim pCH As Integer = 1
                    Dim pOH As Integer = 0
                    Dim pACH As Integer = 1
                    Dim pACCH2 As Integer = 2
                    Dim pACCH3 As Integer = 2
                    Dim pACOH As Integer = 1
                    Dim pCH3CO As Integer = 2
                    Dim pCH2CO As Integer = 2

                    Dim sum = nCH3 * pCH3 + nCH2 * pCH2 + nCH * pCH + _
                        nOH * pOH + nACH * pACH + nACCH2 * pACCH2 + _
                        nACCH3 * pACCH3 + nACOH * pACOH + _
                        nCH3CO * pCH3CO + nCH2CO * pCH2CO
                    Return sum
                Case "H"
                    Dim pCH3 As Integer = 3
                    Dim pCH2 As Integer = 2
                    Dim pCH As Integer = 1
                    Dim pOH As Integer = 1
                    Dim pACH As Integer = 1
                    Dim pACCH2 As Integer = 2
                    Dim pACCH3 As Integer = 3
                    Dim pACOH As Integer = 1
                    Dim pCH3CO As Integer = 3
                    Dim pCH2CO As Integer = 2

                    Dim sum = nCH3 * pCH3 + nCH2 * pCH2 + nCH * pCH + _
                        nOH * pOH + nACH * pACH + nACCH2 * pACCH2 + _
                        nACCH3 * pACCH3 + nACOH * pACOH + _
                        nCH3CO * pCH3CO + nCH2CO * pCH2CO

                    Return sum
                Case "O"

                    Dim pCH3 As Integer = 0
                    Dim pCH2 As Integer = 0
                    Dim pCH As Integer = 0
                    Dim pOH As Integer = 1
                    Dim pACH As Integer = 0
                    Dim pACCH2 As Integer = 0
                    Dim pACCH3 As Integer = 0
                    Dim pACOH As Integer = 1
                    Dim pCH3CO As Integer = 1
                    Dim pCH2CO As Integer = 1

                    Dim sum = nCH3 * pCH3 + nCH2 * pCH2 + nCH * pCH + _
                        nOH * pOH + nACH * pACH + nACCH2 * pACCH2 + _
                        nACCH3 * pACCH3 + nACOH * pACOH + _
                        nCH3CO * pCH3CO + nCH2CO * pCH2CO

                    Return sum
                Case "N"
                    Dim pCH3 As Integer = 0
                    Dim pCH2 As Integer = 0
                    Dim pCH As Integer = 0
                    Dim pOH As Integer = 0
                    Dim pACH As Integer = 0
                    Dim pACCH2 As Integer = 0
                    Dim pACCH3 As Integer = 0
                    Dim pACOH As Integer = 0
                    Dim pCH3CO As Integer = 0
                    Dim pCH2CO As Integer = 0

                    Dim sum = nCH3 * pCH3 + nCH2 * pCH2 + nCH * pCH + _
                        nOH * pOH + nACH * pACH + nACCH2 * pACCH2 + _
                        nACCH3 * pACCH3 + nACOH * pACOH + _
                        nCH3CO * pCH3CO + nCH2CO * pCH2CO

                    Return sum
                Case "S"
                    Dim pCH3 As Integer = 0
                    Dim pCH2 As Integer = 0
                    Dim pCH As Integer = 0
                    Dim pOH As Integer = 0
                    Dim pACH As Integer = 0
                    Dim pACCH2 As Integer = 0
                    Dim pACCH3 As Integer = 0
                    Dim pACOH As Integer = 0
                    Dim pCH3CO As Integer = 0
                    Dim pCH2CO As Integer = 0

                    Dim sum = nCH3 * pCH3 + nCH2 * pCH2 + nCH * pCH + _
                        nOH * pOH + nACH * pACH + nACCH2 * pACCH2 + _
                        nACCH3 * pACCH3 + nACOH * pACOH + _
                        nCH3CO * pCH3CO + nCH2CO * pCH2CO

                    Return sum
                Case Else
                    Return 0
            End Select

        End Function

    End Class

End Namespace
