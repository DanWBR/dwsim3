'    Rigorous Column Solvers
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
Imports DWSIM.DWSIM.MathEx
Imports System.Math
Imports Mapack
Imports DWSIM.DWSIM.Flowsheet.FlowsheetSolver
Imports System.Threading.Tasks

Namespace DWSIM.SimulationObjects.UnitOps.Auxiliary.SepOps.SolvingMethods

    Public Enum DistColSolvingMethod
        Russell_InsideOut = 0
        WangHenke_BubblePoint = 1
        NaphtaliSandholm_SimultaneousCorrection = 3
    End Enum

    Public Enum AbsColSolvingMethod
        Russell_InsideOut = 0
        Burningham_Otto_SumRates = 2
        NaphtaliSandholm_SimultaneousCorrection = 3
    End Enum

    Public Enum RebAbsColSolvingMethod
        Russell_InsideOut = 0
        WangHenke_BubblePoint = 1
        NaphtaliSandholm_SimultaneousCorrection = 3
    End Enum

    Public Enum RefAbsColSolvingMethod
        Russell_InsideOut = 0
        WangHenke_BubblePoint = 1
        NaphtaliSandholm_SimultaneousCorrection = 3
    End Enum

    <System.Serializable()> Public Class Tomich

        Public Shared Function TDMASolve(ByVal a As Double(), ByVal b As Double(), ByVal c As Double(), ByVal d As Double()) As Double()

            ' Warning: will modify c and d!

            Dim n As Integer = d.Length
            ' All arrays should be the same length
            Dim x As Double() = New Double(n - 1) {}
            Dim id As Double

            ' Modify the coefficients.

            c(0) /= b(0)
            ' Division by zero risk.
            d(0) /= b(0)
            ' Division by zero would imply a singular matrix.
            For i As Integer = 1 To n - 1
                id = b(i) - c(i - 1) * a(i)
                c(i) /= id
                ' This calculation during the last iteration is redundant.
                d(i) = (d(i) - d(i - 1) * a(i)) / id
            Next

            ' Now back substitute.

            x(n - 1) = d(n - 1)
            For i As Integer = n - 2 To 0 Step -1
                x(i) = d(i) - c(i) * x(i + 1)
            Next

            Return x
        End Function

    End Class

    <System.Serializable()> Public Class RussellMethod

        Sub New()

        End Sub


        Private Function CalcKbj1(ByVal ns As Integer, ByVal nc As Integer, ByVal K(,) As Object, _
                                        ByVal z()() As Double, ByVal y()() As Double, ByVal T() As Double, _
                                        ByVal P() As Double, ByRef pp As PropertyPackages.PropertyPackage) As Object

            Dim i, j As Integer

            Dim Kbj1(ns) As Object

            For i = 0 To ns
                Kbj1(i) = K(i, 0)
                For j = 1 To nc - 1
                    If Abs(K(i, j) - 1) < Abs(Kbj1(i) - 1) And z(i)(j) <> 0 Then Kbj1(i) = K(i, j)
                Next
            Next

            Return Kbj1

        End Function

        Private Function CalcKbj2(ByVal ns As Integer, ByVal nc As Integer, ByVal K(,) As Object, _
                                      ByVal z()() As Double, ByVal y()() As Double, ByVal T() As Double, _
                                      ByVal P() As Double, ByRef pp As PropertyPackages.PropertyPackage) As Object

            Dim i, j As Integer

            Dim Kbj1(ns) As Object
            Dim Kw11(ns)(), Kw21(ns)() As Double
            Dim wi(ns, nc - 1), ti(ns, nc - 1), sumwi(ns), sumti(ns) As Double
            For i = 0 To ns
                Array.Resize(Kw11(i), nc)
                Array.Resize(Kw21(i), nc)
            Next

            For i = 0 To ns
                Kw11(i) = pp.DW_CalcKvalue(z(i), T(i), P(i))
                Kw21(i) = pp.DW_CalcKvalue(z(i), T(i) + 0.1, P(i))
                CheckCalculatorStatus()
            Next

            For i = 0 To ns
                sumti(i) = 0
                For j = 0 To nc - 1
                    ti(i, j) = y(i)(j) * (Log(Kw21(i)(j)) - Log(Kw11(i)(j))) / (1 / (T(i) + 0.1) - 1 / T(i))
                    sumti(i) += Abs(ti(i, j))
                Next
            Next

            For i = 0 To ns
                If sumti(i) <> 0 Then
                    For j = 0 To nc - 1
                        wi(i, j) = Abs(ti(i, j)) / sumti(i)
                    Next
                Else
                    For j = 0 To nc - 1
                        wi(i, j) = z(i)(j)
                    Next
                End If
            Next

            For i = 0 To ns
                Kbj1(i) = 0
                For j = 0 To nc - 1
                    Kbj1(i) += wi(i, j) * Log(K(i, j))
                Next
                Kbj1(i) = Exp(Kbj1(i))
                If Kbj1(i) < 0 Then
                    Kbj1(i) = K(i, 0)
                    For j = 1 To nc - 1
                        If Abs(K(i, j) - 1) < Abs(Kbj1(i) - 1) Then Kbj1(i) = K(i, j)
                    Next
                End If
            Next

            Return Kbj1

        End Function

        Dim ndeps As Double = 0.01

        Dim _ns, _nc As Integer
        Dim _Bj, _Aj, _Cj, _Dj, _Ej, _Fj, _eff, _T_, _Tj, _Lj, _Vj, _LSSj, _VSSj, _LSS, _VSS, _Rlj, _Rvj, _F, _P, _HF, _Q As Double()
        Dim _S, _alpha As Double(,)
        Dim _fc, _xc, _yc, _lc, _vc, _zc As Double()()
        Dim _Kbj As Object()
        Dim _rr, _Sb, _maxF As Double
        Dim _pp As PropertyPackages.PropertyPackage
        Dim _coltype As Column.ColType
        Dim _condtype As Column.condtype
        Dim _bx, _dbx As Double()
        Dim _vcnt, _lcnt As Integer
        Dim _specs As Dictionary(Of String, SepOps.ColumnSpec)
        Dim llextr As Boolean = False

        Public Function FunctionValue(ByVal x() As Double) As Double()


            Dim errors(x.Length - 1) As Double

            Dim cv As New SistemasDeUnidades.Conversor
            Dim spval1, spval2, spfval1, spfval2 As Double
            Dim spci1, spci2 As Integer

            spval1 = cv.ConverterParaSI(_specs("C").SpecUnit, _specs("C").SpecValue)
            spci1 = _specs("C").ComponentIndex
            spval2 = cv.ConverterParaSI(_specs("R").SpecUnit, _specs("R").SpecValue)
            spci2 = _specs("R").ComponentIndex

            Dim sum1(_ns) As Double
            Dim i, j As Integer

            For i = 0 To _ns
                If i = 0 And _condtype = Column.condtype.Total_Condenser Or i = 0 And _condtype = Column.condtype.Partial_Condenser Then
                    _Rlj(i) = Exp(x(i))
                Else
                    For j = 0 To _nc - 1
                        _S(i, j) = Exp(x(i)) * _alpha(i, j) * _Sb
                    Next
                End If
            Next

            Dim m1, m2 As Integer
            m1 = 0
            m2 = 0

            If _vcnt > 0 Then
                For i = _ns + 1 To _vcnt + _ns
                    For j = m1 To _ns
                        If _Rvj(j) <> 1 Then
                            m1 = j + 1
                            Exit For
                        End If
                    Next
                    _Rvj(m1 - 1) = Exp(x(i))
                Next
            End If

            If _lcnt > 0 Then
                For i = _vcnt + _ns + 1 To _vcnt + _lcnt + _ns
                    For j = m2 + 1 To _ns
                        If _Rlj(j) <> 1 Then
                            m2 = j + 1
                            Exit For
                        End If
                    Next
                    _Rlj(m2 - 1) = Exp(x(i))
                Next
            End If

            'step4

            'find component liquid flows by the tridiagonal matrix method

            Dim Bs(_ns, _nc - 1), Cs(_ns, _nc - 1) As Double
            Dim at(_nc - 1)(), bt(_nc - 1)(), ct(_nc - 1)(), dt(_nc - 1)(), xt(_nc - 1)() As Double

            For i = 0 To _nc - 1
                Array.Resize(at(i), _ns + 1)
                Array.Resize(bt(i), _ns + 1)
                Array.Resize(ct(i), _ns + 1)
                Array.Resize(dt(i), _ns + 1)
                Array.Resize(xt(i), _ns + 1)
            Next

            Dim ic0 As Integer = 0

            For i = 0 To _ns
                For j = 0 To _nc - 1
                    If i = 0 And _condtype = Column.condtype.Total_Condenser Then
                        Bs(i, j) = -(_Rlj(i))
                    Else
                        Bs(i, j) = -(_Rlj(i) + _S(i, j) * _Rvj(i))
                    End If
                    If i < _ns Then Cs(i, j) = _S(i + 1, j)
                Next
            Next

            For i = 0 To _nc - 1
                For j = 0 To _ns
                    dt(i)(j) = -_fc(j)(i) * _F(j)
                    bt(i)(j) = Bs(j, i)
                    If j < _ns Then ct(i)(j) = Cs(j, i)
                    If j > 0 Then at(i)(j) = 1
                Next
            Next

            'solve matrices

            'tomich
            For i = 0 To _nc - 1
                xt(i) = Tomich.TDMASolve(at(i), bt(i), ct(i), dt(i))
            Next

            For i = 0 To _ns
                Array.Resize(_xc(i), _nc)
                Array.Resize(_yc(i), _nc)
                Array.Resize(_lc(i), _nc)
                Array.Resize(_vc(i), _nc)
                Array.Resize(_zc(i), _nc)
            Next

            'step5

            For i = 0 To _ns
                _Lj(i) = 0
                For j = 0 To _nc - 1
                    _lc(i)(j) = xt(j)(i)
                    _Lj(i) += _lc(i)(j)
                Next
                'If _Lj(i) < 0 Then
                '    _Lj(i) = 1.0E-20
                'End If
            Next

            For i = 0 To _ns
                For j = 0 To _nc - 1
                    _xc(i)(j) = _lc(i)(j) / _Lj(i)
                Next
            Next

            For i = _ns To 0 Step -1
                _Vj(i) = 0
                For j = 0 To _nc - 1
                    If i < _ns Then
                        '_vc(i)(j) = _eff(i) * (_S(i, j) * _lc(i)(j) - _vc(i + 1)(j) * _Vj(i) / _Vj(i + 1)) + _vc(i + 1)(j) * _Vj(i) / _Vj(i + 1)
                        _vc(i)(j) = _S(i, j) * _lc(i)(j)
                    Else
                        _vc(i)(j) = _S(i, j) * _lc(i)(j)
                    End If
                    _Vj(i) += _vc(i)(j)
                Next
                'If _Vj(i) < 0 Then
                '    _Vj(i) = 1.0E-20
                'End If
            Next

            'departures from product flows

            Dim sumLSS As Double = 0
            Dim sumVSS As Double = 0
            Dim sumF As Double = 0
            For i = 0 To _ns
                If i > 0 Then sumLSS += _LSSj(i)
                sumVSS += _VSSj(i)
                sumF += _F(i)
            Next
            If _condtype = Column.condtype.Total_Condenser Then
                _LSSj(0) = sumF - sumLSS - sumVSS - _Lj(_ns)
                _Rlj(0) = 1 + _LSSj(0) / _Lj(0)
            ElseIf _condtype = Column.condtype.Partial_Condenser Then
                _LSSj(0) = sumF - sumLSS - sumVSS - _Vj(0) - _Lj(_ns)
                _Rlj(0) = 1 + _LSSj(0) / _Lj(0)
            Else
                _LSSj(0) = 0.0
                _Rlj(0) = 1
            End If
            'If _Lj(0) <> 0 Or Not Double.IsNaN(_Lj(0)) Or Not Double.IsInfinity(_Lj(0)) Then
            '    _Rlj(0) = 1 + _LSSj(0) / _Lj(0)
            'Else
            '    _Rlj(0) = 1
            'End If

            For i = 0 To _ns
                _VSSj(i) = (_Rvj(i) - 1) * _Vj(i)
                If i > 0 Then _LSSj(i) = (_Rlj(i) - 1) * _Lj(i)
            Next

            'For i = 0 To _ns
            '    sum1(i) = 0
            '    For j = 0 To i
            '        sum1(i) += _F(j) - _LSSj(j) - _VSSj(j)
            '    Next
            'Next

            ''Ljs
            'For i = 0 To _ns
            '    If i < _ns Then _Lj(i) = _Vj(i + 1) + sum1(i) - _Vj(0) Else _Lj(i) = sum1(i) - _Vj(0)
            '    If _Lj(i) < 0 Then
            '        _Lj(i) = 0.0000000001
            '    End If
            'Next

            For i = 0 To _ns
                For j = 0 To _nc - 1
                    If _Vj(i) <> 0 Then
                        _yc(i)(j) = _vc(i)(j) / _Vj(i)
                    Else
                        _yc(i)(j) = _vc(i)(j)
                    End If
                    _zc(i)(j) = (_lc(i)(j) + _vc(i)(j)) / (_Lj(i) + _Vj(i))
                Next
            Next

            Dim sum_axi(_ns) As Double

            For i = 0 To _ns
                sum_axi(i) = 0
                For j = 0 To _nc - 1
                    sum_axi(i) += _alpha(i, j) * _xc(i)(j)
                Next
            Next

            'step6

            'calculate new temperatures

            Dim _Tj_ant(_ns) As Double

            For i = 0 To _ns
                _Kbj(i) = 1 / sum_axi(i)
                _Tj_ant(i) = _Tj(i)
                _Tj(i) = _Bj(i) / (_Aj(i) - Log(_Kbj(i)))
                If Abs(_Tj(i) - _Tj_ant(i)) > 100 Or Double.IsNaN(_Tj(i)) Or Double.IsInfinity(_Tj(i)) Then
                    'switch to a bubble point temperature calculation...
                    Dim tmp = _pp.DW_CalcBubT(_xc(i), _P(i), Nothing, Nothing, False)
                    _Tj(i) = tmp(4)
                    CheckCalculatorStatus()
                End If
            Next

            'step7

            'calculate enthalpies

            Dim Hv(_ns), Hl(_ns), Hidv(_ns), Hidl(_ns), DHv(_ns), DHl(_ns) As Double

            For i = 0 To _ns
                'Hidv(i) = _pp.RET_Hid(298.15, _Tj(i), _yc(i))
                'Hidl(i) = _pp.RET_Hid(298.15, _Tj(i), _xc(i))
                'DHv(i) = _Cj(i) + _Dj(i) * (_Tj(i) - _T_(i))
                'DHl(i) = _Ej(i) + _Fj(i) * (_Tj(i) - _T_(i))
                'Hv(i) = (Hidv(i) + DHv(i)) * _pp.AUX_MMM(_yc(i)) / 1000
                'Hl(i) = (Hidl(i) + DHl(i)) * _pp.AUX_MMM(_xc(i)) / 1000
                If llextr Then
                    Hv(i) = _pp.DW_CalcEnthalpy(_yc(i), _Tj(i), _P(i), PropertyPackages.State.Liquid) * _pp.AUX_MMM(_yc(i)) / 1000
                Else
                    Hv(i) = _pp.DW_CalcEnthalpy(_yc(i), _Tj(i), _P(i), PropertyPackages.State.Vapor) * _pp.AUX_MMM(_yc(i)) / 1000
                End If
                Hl(i) = _pp.DW_CalcEnthalpy(_xc(i), _Tj(i), _P(i), PropertyPackages.State.Liquid) * _pp.AUX_MMM(_xc(i)) / 1000
                CheckCalculatorStatus()
            Next

            'reboiler and condenser heat duties
            Select Case _coltype
                Case Column.ColType.DistillationColumn
                    If Not _specs("C").SType = ColumnSpec.SpecType.Heat_Duty Then
                        _Q(0) = Hl(0) * _Rlj(0) * _Lj(0) + Hv(0) * _Rvj(0) * _Vj(0) - Hv(1) * _Vj(1) - _HF(0) * _F(0)
                        _Q(0) = -_Q(0)
                    End If
                    If Not _specs("R").SType = ColumnSpec.SpecType.Heat_Duty Then
                        _Q(_ns) = Hl(_ns) * _Rlj(_ns) * _Lj(_ns) + Hv(_ns) * _Rvj(_ns) * _Vj(_ns) - Hl(_ns - 1) * _Lj(_ns - 1) - _HF(_ns) * _F(_ns)
                        _Q(_ns) = -_Q(_ns)
                    End If
                Case Column.ColType.AbsorptionColumn
                    'use provided values
                Case Column.ColType.RefluxedAbsorber
                    If Not _specs("C").SType = ColumnSpec.SpecType.Heat_Duty Then
                        _Q(0) = Hl(0) * _Rlj(0) * _Lj(0) + Hv(0) * _Rvj(0) * _Vj(0) - Hv(1) * _Vj(1) - _HF(0) * _F(0)
                        _Q(0) = -_Q(0)
                    End If
                Case Column.ColType.ReboiledAbsorber
                    If Not _specs("R").SType = ColumnSpec.SpecType.Heat_Duty Then
                        _Q(_ns) = Hl(_ns) * _Rlj(_ns) * _Lj(_ns) + Hv(_ns) * _Rvj(_ns) * _Vj(_ns) - Hl(_ns - 1) * _Lj(_ns - 1) - _HF(_ns) * _F(_ns)
                        _Q(_ns) = -_Q(_ns)
                    End If
            End Select

            'handle user specs

            'Condenser Specs
            Select Case _specs("C").SType
                Case ColumnSpec.SpecType.Component_Fraction
                    If _condtype = Column.condtype.Total_Condenser Or _condtype = Column.condtype.Partial_Condenser Then
                        If _specs("C").SpecUnit = "M" Then
                            spfval1 = _xc(0)(spci1) - spval1
                        Else 'W
                            spfval1 = _pp.AUX_CONVERT_MOL_TO_MASS(_xc(0))(spci1) - spval1
                        End If
                    Else
                        If _specs("C").SpecUnit = "M" Then
                            spfval1 = _yc(0)(spci1) - spval1
                        Else 'W
                            spfval1 = _pp.AUX_CONVERT_MOL_TO_MASS(_yc(0))(spci1) - spval1
                        End If
                    End If
                Case ColumnSpec.SpecType.Component_Mass_Flow_Rate
                    If _condtype = Column.condtype.Total_Condenser Or _condtype = Column.condtype.Partial_Condenser Then
                        spfval1 = _LSSj(0) * _xc(0)(spci1) - spval1 / _pp.RET_VMM()(spci1) * 1000 / _maxF
                    Else
                        spfval1 = _Vj(0) * _yc(0)(spci1) - spval1 / _pp.RET_VMM()(spci1) * 1000 / _maxF
                    End If
                Case ColumnSpec.SpecType.Component_Molar_Flow_Rate
                    If _condtype = Column.condtype.Total_Condenser Or _condtype = Column.condtype.Partial_Condenser Then
                        spfval1 = _LSSj(0) * _xc(0)(spci1) - spval1 / _maxF
                    Else
                        spfval1 = _Vj(0) * _yc(0)(spci1) - spval1 / _maxF
                    End If
                Case ColumnSpec.SpecType.Component_Recovery
                    Dim rec As Double = spval1 / 100
                    Dim sumc As Double = 0
                    For j = 0 To _ns
                        sumc += _fc(j)(spci1)
                    Next
                    sumc *= rec
                    If _condtype = Column.condtype.Total_Condenser Or _condtype = Column.condtype.Partial_Condenser Then
                        If _specs("C").SpecUnit = "% M/M" Then
                            spfval1 = _xc(0)(spci1) * _LSSj(0) - sumc
                        Else '% W/W
                            spfval1 = _pp.RET_VMM()(spci1) * 1000 * (_xc(0)(spci1) * _LSSj(0) - sumc)
                        End If
                    Else
                        If _specs("C").SpecUnit = "% M/M" Then
                            spfval1 = _yc(0)(spci1) * _Vj(0) - sumc
                        Else '% W/W
                            spfval1 = _pp.RET_VMM()(spci1) * 1000 * (_yc(0)(spci1) * _Vj(0) - sumc)
                        End If
                    End If
                Case ColumnSpec.SpecType.Heat_Duty
                    _Q(0) = spval1 / _maxF
                Case ColumnSpec.SpecType.Product_Mass_Flow_Rate
                    If _condtype = Column.condtype.Total_Condenser Or _condtype = Column.condtype.Partial_Condenser Then
                        spfval1 = _LSSj(0) - spval1 / _pp.AUX_MMM(_xc(0)) * 1000 / _maxF
                    Else
                        spfval1 = _Vj(0) - spval1 / _pp.AUX_MMM(_yc(0)) * 1000 / _maxF
                    End If
                Case ColumnSpec.SpecType.Product_Molar_Flow_Rate
                    If _condtype = Column.condtype.Total_Condenser Or _condtype = Column.condtype.Partial_Condenser Then
                        spfval1 = _LSSj(0) - spval1 / _maxF
                    Else
                        spfval1 = _Vj(0) - spval1 / _maxF
                    End If
                Case ColumnSpec.SpecType.Stream_Ratio
                    If _condtype = Column.condtype.Total_Condenser Or _condtype = Column.condtype.Partial_Condenser Then
                        spfval1 = _Lj(0) - spval1 * _LSSj(0)
                    Else
                        spfval1 = _Lj(0) - spval1 * _Vj(0)
                    End If
                Case ColumnSpec.SpecType.Temperature
                    spfval1 = _Tj(0) - spval1
            End Select

            'Reboiler Specs
            Select Case _specs("R").SType
                Case ColumnSpec.SpecType.Component_Fraction
                    If _specs("R").SpecUnit = "M" Then
                        spfval2 = _xc(_ns)(spci2) - spval2
                    Else 'W
                        spfval2 = _pp.AUX_CONVERT_MOL_TO_MASS(_xc(_ns))(spci2) - spval2
                    End If
                Case ColumnSpec.SpecType.Component_Mass_Flow_Rate
                    spfval2 = _Lj(_ns) * _xc(_ns)(spci2) - spval2 / _pp.RET_VMM()(spci2) * 1000 / _maxF
                Case ColumnSpec.SpecType.Component_Molar_Flow_Rate
                    spfval2 = _Lj(_ns) * _xc(_ns)(spci2) - spval2 / _maxF
                Case ColumnSpec.SpecType.Component_Recovery
                    Dim rec As Double = spval2 / 100
                    Dim sumc As Double = 0
                    For j = 0 To _ns
                        sumc += _fc(j)(spci2)
                    Next
                    sumc *= rec
                    If _specs("R").SpecUnit = "% M/M" Then
                        spfval2 = _lc(_ns)(spci2) - sumc
                    Else '% W/W
                        spfval2 = _pp.RET_VMM()(spci2) * 1000 * (_lc(_ns)(spci2) - sumc)
                    End If
                Case ColumnSpec.SpecType.Heat_Duty
                    _Q(_ns) = spval2 / _maxF
                Case ColumnSpec.SpecType.Product_Mass_Flow_Rate
                    spfval2 = _Lj(_ns) - spval2 / _pp.AUX_MMM(_xc(_ns)) * 1000 / _maxF
                Case ColumnSpec.SpecType.Product_Molar_Flow_Rate
                    spfval2 = _Lj(_ns) - spval2 / _maxF
                Case ColumnSpec.SpecType.Stream_Ratio
                    spfval2 = _Vj(_ns) - spval2 * _Lj(_ns)
                Case ColumnSpec.SpecType.Temperature
                    spfval2 = _Tj(_ns) - spval2
            End Select

            'enthalpy balances

            Dim entbal(_ns) As Double

            For i = 0 To _ns
                If i = 0 Then
                    entbal(i) = (Hl(i) * _Rlj(i) * _Lj(i) + Hv(i) * _Rvj(i) * _Vj(i) - Hv(i + 1) * _Vj(i + 1) - _HF(i) * _F(i) - _Q(i))
                ElseIf i = _ns Then
                    entbal(i) = (Hl(i) * _Rlj(i) * _Lj(i) + Hv(i) * _Rvj(i) * _Vj(i) - Hl(i - 1) * _Lj(i - 1) - _HF(i) * _F(i) - _Q(i))
                Else
                    entbal(i) = (Hl(i) * _Rlj(i) * _Lj(i) + Hv(i) * _Rvj(i) * _Vj(i) - Hl(i - 1) * _Lj(i - 1) - Hv(i + 1) * _Vj(i + 1) - _HF(i) * _F(i) - _Q(i))
                End If
                entbal(i) /= (Hv(i) - Hl(i))
            Next

            Select Case _coltype
                Case Column.ColType.DistillationColumn
                    entbal(0) = spfval1
                    entbal(_ns) = spfval2
                Case Column.ColType.AbsorptionColumn
                    'do nothing
                Case Column.ColType.ReboiledAbsorber
                    entbal(_ns) = spfval2
                Case Column.ColType.RefluxedAbsorber
                    entbal(0) = spfval1
            End Select

            For i = 0 To x.Length - 1
                If i <= _ns Then
                    errors(i) = entbal(i)
                ElseIf i > _ns And i <= _vcnt + _ns Then
                    For j = 0 To _ns
                        If _Rvj(j) <> 1 Then
                            errors(i) = (_VSS(j) - _VSSj(j)) '/ _VSS(j)
                            i += 1
                        End If
                    Next
                End If
                If i > _vcnt + _ns And i <= _vcnt + _lcnt + _ns Then
                    For j = 1 To _ns
                        If _Rlj(j) <> 1 Then
                            errors(i) = (_LSS(j) - _LSSj(j)) '/ _LSS(j)
                            i += 1
                        End If
                    Next
                End If
            Next

            Return errors

        End Function

        Private Function FunctionGradient(ByVal x() As Double) As Double(,)

            Dim epsilon As Double = ndeps
            Dim hs As Double
            Dim f1(), f2() As Double
            Dim g(x.Length - 1, x.Length - 1), x1(x.Length - 1), x2(x.Length - 1) As Double
            Dim i, j, k As Integer

            f1 = FunctionValue(x)
            For i = 0 To x.Length - 1
                For j = 0 To x.Length - 1
                    If i <> j Then
                        'x1(j) = x(j)
                        x2(j) = x(j)
                    Else
                        'x1(j) = x(j)
                        'x2(j) = x(j) * (1 + epsilon) + (epsilon / 2) ^ 2
                        x2(j) = x(j) + epsilon
                    End If
                Next
                f2 = FunctionValue(x2)
                For k = 0 To x.Length - 1
                    hs = epsilon
                    g(k, i) = (f2(k) - f1(k)) / hs
                Next
            Next

            Return g

        End Function

        Public Function MinimizeError(ByVal t As Double) As Double

            Dim cv As New SistemasDeUnidades.Conversor
            Dim spval1, spval2, spfval1, spfval2 As Double
            Dim spci1, spci2 As Integer

            spval1 = cv.ConverterParaSI(_specs("C").SpecUnit, _specs("C").SpecValue)
            spci1 = _specs("C").ComponentIndex
            spval2 = cv.ConverterParaSI(_specs("R").SpecUnit, _specs("R").SpecValue)
            spci2 = _specs("R").ComponentIndex

            Dim sum1(_ns) As Double
            Dim i, j As Integer

            For i = 0 To _ns
                If i = 0 And _condtype = Column.condtype.Total_Condenser Or i = 0 And _condtype = Column.condtype.Partial_Condenser Then
                    _Rlj(i) = Exp(_bx(i) + _dbx(i) * t)
                Else
                    For j = 0 To _nc - 1
                        _S(i, j) = Exp(_bx(i) + _dbx(i) * t) * _alpha(i, j) * _Sb
                    Next
                End If

            Next

            Dim m1, m2 As Integer

            m1 = 0
            m2 = 0

            If _vcnt > 0 Then
                For i = _ns + 1 To _vcnt + _ns
                    For j = m1 To _ns
                        If _Rvj(j) <> 1 Then
                            m1 = j + 1
                            Exit For
                        End If
                    Next
                    _Rvj(m1 - 1) = Exp(_bx(i) + _dbx(i) * t)
                Next
            End If

            If _lcnt > 0 Then
                For i = _vcnt + _ns + 1 To _vcnt + _lcnt + _ns
                    For j = m2 + 1 To _ns
                        If _Rlj(j) <> 1 Then
                            m2 = j + 1
                            Exit For
                        End If
                    Next
                    _Rlj(m2 - 1) = Exp(_bx(i) + _dbx(i) * t)
                Next
            End If


            'step4

            'find component liquid flows by the tridiagonal matrix method

            Dim Bs(_ns, _nc - 1), Cs(_ns, _nc - 1) As Double
            Dim at(_nc - 1)(), bt(_nc - 1)(), ct(_nc - 1)(), dt(_nc - 1)(), xt(_nc - 1)() As Double

            For i = 0 To _nc - 1
                Array.Resize(at(i), _ns + 1)
                Array.Resize(bt(i), _ns + 1)
                Array.Resize(ct(i), _ns + 1)
                Array.Resize(dt(i), _ns + 1)
                Array.Resize(xt(i), _ns + 1)
            Next

            Dim ic0 As Integer = 0

            For i = 0 To _ns
                For j = 0 To _nc - 1
                    If i = 0 And _condtype = Column.condtype.Total_Condenser Then
                        Bs(i, j) = -(_Rlj(i))
                    Else
                        Bs(i, j) = -(_Rlj(i) + _S(i, j) * _Rvj(i))
                    End If
                    If i < _ns Then Cs(i, j) = _S(i + 1, j)
                Next
            Next

            For i = 0 To _nc - 1
                For j = 0 To _ns
                    dt(i)(j) = -_fc(j)(i) * _F(j)
                    bt(i)(j) = Bs(j, i)
                    If j < _ns Then ct(i)(j) = Cs(j, i)
                    If j > 0 Then at(i)(j) = 1
                Next
            Next

            'solve matrices

            'tomich
            For i = 0 To _nc - 1
                xt(i) = Tomich.TDMASolve(at(i), bt(i), ct(i), dt(i))
                CheckCalculatorStatus()
            Next

            For i = 0 To _ns
                Array.Resize(_xc(i), _nc)
                Array.Resize(_yc(i), _nc)
                Array.Resize(_lc(i), _nc)
                Array.Resize(_vc(i), _nc)
                Array.Resize(_zc(i), _nc)
            Next

            'step5

            For i = 0 To _ns
                _Lj(i) = 0
                For j = 0 To _nc - 1
                    'lc(i)(j) = lm(j)(i, 0)
                    _lc(i)(j) = xt(j)(i)
                    'If _lc(i)(j) < 0 Then _lc(i)(j) = 0
                    _Lj(i) += _lc(i)(j)
                Next
                'If _Lj(i) < 0 Then _Lj(i) = 0.0000000001
            Next

            For i = 0 To _ns
                _Vj(i) = 0
                For j = 0 To _nc - 1
                    _xc(i)(j) = _lc(i)(j) / _Lj(i)
                    'If Double.IsNaN(_xc(i)(j)) Then _xc(i)(j) = 0
                Next
            Next

            For i = _ns To 0 Step -1
                _Vj(i) = 0
                For j = 0 To _nc - 1
                    If i < _ns Then
                        '_vc(i)(j) = _eff(i) * (_S(i, j) * _lc(i)(j) - _vc(i + 1)(j) * _Vj(i) / _Vj(i + 1)) + _vc(i + 1)(j) * _Vj(i) / _Vj(i + 1)
                        _vc(i)(j) = _S(i, j) * _lc(i)(j)
                    Else
                        _vc(i)(j) = _S(i, j) * _lc(i)(j)
                    End If
                    'If _vc(i)(j) < 0 Or Double.IsNaN(_vc(i)(j)) Then _vc(i)(j) = 0
                    _Vj(i) += _vc(i)(j)
                Next
                'If _Vj(i) < 0 Then _Vj(i) = 0.0000000001
            Next

            'departures from product flows

            Dim sumLSS As Double = 0
            Dim sumVSS As Double = 0
            Dim sumF As Double = 0
            For i = 0 To _ns
                If i > 0 Then sumLSS += _LSSj(i)
                sumVSS += _VSSj(i)
                sumF += _F(i)
            Next
            If _condtype = Column.condtype.Total_Condenser Then
                _LSSj(0) = sumF - sumLSS - sumVSS - _Lj(_ns)
                _Rlj(0) = 1 + _LSSj(0) / _Lj(0)
            ElseIf _condtype = Column.condtype.Partial_Condenser Then
                _LSSj(0) = sumF - sumLSS - sumVSS - _Vj(0) - _Lj(_ns)
                _Rlj(0) = 1 + _LSSj(0) / _Lj(0)
            Else
                _LSSj(0) = 0.0
                _Rlj(0) = 1
            End If
            'If _Lj(0) <> 0 Or Not Double.IsNaN(_Lj(0)) Or Not Double.IsInfinity(_Lj(0)) Then
            '    _Rlj(0) = 1 + _LSSj(0) / _Lj(0)
            'Else
            '    _Rlj(0) = 1
            'End If

            For i = 0 To _ns
                _VSSj(i) = (_Rvj(i) - 1) * _Vj(i)
                If i > 0 Then _LSSj(i) = (_Rlj(i) - 1) * _Lj(i)
            Next

            'For i = 0 To _ns
            '    sum1(i) = 0
            '    For j = 0 To i
            '        sum1(i) += _F(j) - _LSSj(j) - _VSSj(j)
            '    Next
            'Next

            ''Ljs
            'For i = 0 To _ns
            '    If i < _ns Then _Lj(i) = _Vj(i + 1) + sum1(i) - _Vj(0) Else _Lj(i) = sum1(i) - _Vj(0)
            'Next

            For i = 0 To _ns
                For j = 0 To _nc - 1
                    If _Vj(i) <> 0 Then
                        _yc(i)(j) = _vc(i)(j) / _Vj(i)
                    Else
                        _yc(i)(j) = _vc(i)(j)
                    End If
                    _zc(i)(j) = (_lc(i)(j) + _vc(i)(j)) / (_Lj(i) + _Vj(i))
                Next
            Next

            Dim sum_axi(_ns) As Double

            For i = 0 To _ns
                sum_axi(i) = 0
                For j = 0 To _nc - 1
                    sum_axi(i) += _alpha(i, j) * _xc(i)(j)
                Next
            Next

            'step6

            'calculate new temperatures

            Dim _Tj0(_ns) As Double

            For i = 0 To _ns
                _Kbj(i) = 1 / sum_axi(i)
                _Tj0(i) = _Tj(i)
                _Tj(i) = _Bj(i) / (_Aj(i) - Log(_Kbj(i)))
                If Double.IsNaN(_Tj(i)) Or _Tj(i) = 0 Then _Tj(i) = _T_(i)
            Next

            'step7

            'calculate enthalpies

            Dim Hv(_ns), Hl(_ns), Hidv(_ns), Hidl(_ns), DHv(_ns), DHl(_ns) As Double

            For i = 0 To _ns
                'Hidv(i) = _pp.RET_Hid(298.15, _Tj(i), _yc(i))
                'Hidl(i) = _pp.RET_Hid(298.15, _Tj(i), _xc(i))
                'DHv(i) = _Cj(i) + _Dj(i) * (_Tj(i) - _T_(i))
                'DHl(i) = _Ej(i) + _Fj(i) * (_Tj(i) - _T_(i))
                'Hv(i) = (Hidv(i) + DHv(i)) * _pp.AUX_MMM(_yc(i)) / 1000
                'Hl(i) = (Hidl(i) + DHl(i)) * _pp.AUX_MMM(_xc(i)) / 1000
                If llextr Then
                    Hv(i) = _pp.DW_CalcEnthalpy(_yc(i), _Tj(i), _P(i), PropertyPackages.State.Liquid) * _pp.AUX_MMM(_yc(i)) / 1000
                Else
                    Hv(i) = _pp.DW_CalcEnthalpy(_yc(i), _Tj(i), _P(i), PropertyPackages.State.Vapor) * _pp.AUX_MMM(_yc(i)) / 1000
                End If
                Hl(i) = _pp.DW_CalcEnthalpy(_xc(i), _Tj(i), _P(i), PropertyPackages.State.Liquid) * _pp.AUX_MMM(_xc(i)) / 1000
                CheckCalculatorStatus()
            Next

            'reboiler and condenser heat duties
            Select Case _coltype
                Case Column.ColType.DistillationColumn
                    If Not _specs("C").SType = ColumnSpec.SpecType.Heat_Duty Then
                        _Q(0) = Hl(0) * _Rlj(0) * _Lj(0) + Hv(0) * _Rvj(0) * _Vj(0) - Hv(1) * _Vj(1) - _HF(0) * _F(0)
                        _Q(0) = -_Q(0)
                    End If
                    If Not _specs("R").SType = ColumnSpec.SpecType.Heat_Duty Then
                        _Q(_ns) = Hl(_ns) * _Rlj(_ns) * _Lj(_ns) + Hv(_ns) * _Rvj(_ns) * _Vj(_ns) - Hl(_ns - 1) * _Lj(_ns - 1) - _HF(_ns) * _F(_ns)
                        _Q(_ns) = -_Q(_ns)
                    End If
                Case Column.ColType.AbsorptionColumn
                    'use provided values
                Case Column.ColType.RefluxedAbsorber
                    If Not _specs("C").SType = ColumnSpec.SpecType.Heat_Duty Then
                        _Q(0) = Hl(0) * _Rlj(0) * _Lj(0) + Hv(0) * _Rvj(0) * _Vj(0) - Hv(1) * _Vj(1) - _HF(0) * _F(0)
                        _Q(0) = -_Q(0)
                    End If
                Case Column.ColType.ReboiledAbsorber
                    If Not _specs("R").SType = ColumnSpec.SpecType.Heat_Duty Then
                        _Q(_ns) = Hl(_ns) * _Rlj(_ns) * _Lj(_ns) + Hv(_ns) * _Rvj(_ns) * _Vj(_ns) - Hl(_ns - 1) * _Lj(_ns - 1) - _HF(_ns) * _F(_ns)
                        _Q(_ns) = -_Q(_ns)
                    End If
            End Select

            'enthalpy balances

            Dim entbal(_ns) As Double

            'handle user specs

            'Condenser Specs
            Select Case _specs("C").SType
                Case ColumnSpec.SpecType.Component_Fraction
                    If _condtype = Column.condtype.Total_Condenser Or _condtype = Column.condtype.Partial_Condenser Then
                        If _specs("C").SpecUnit = "M" Then
                            spfval1 = _xc(0)(spci1) - spval1
                        Else 'W
                            spfval1 = _pp.AUX_CONVERT_MOL_TO_MASS(_xc(0))(spci1) - spval1
                        End If
                    Else
                        If _specs("C").SpecUnit = "M" Then
                            spfval1 = _yc(0)(spci1) - spval1
                        Else 'W
                            spfval1 = _pp.AUX_CONVERT_MOL_TO_MASS(_yc(0))(spci1) - spval1
                        End If
                    End If
                Case ColumnSpec.SpecType.Component_Mass_Flow_Rate
                    If _condtype = Column.condtype.Total_Condenser Or _condtype = Column.condtype.Partial_Condenser Then
                        spfval1 = _LSSj(0) * _xc(0)(spci1) - spval1 / _pp.RET_VMM()(spci1) * 1000 / _maxF
                    Else
                        spfval1 = _Vj(0) * _yc(0)(spci1) - spval1 / _pp.RET_VMM()(spci1) * 1000 / _maxF
                    End If
                Case ColumnSpec.SpecType.Component_Molar_Flow_Rate
                    If _condtype = Column.condtype.Total_Condenser Or _condtype = Column.condtype.Partial_Condenser Then
                        spfval1 = _LSSj(0) * _xc(0)(spci1) - spval1 / _maxF
                    Else
                        spfval1 = _Vj(0) * _yc(0)(spci1) - spval1 / _maxF
                    End If
                Case ColumnSpec.SpecType.Component_Recovery
                    Dim rec As Double = spval1 / 100
                    Dim sumc As Double = 0
                    For j = 0 To _ns
                        sumc += _fc(j)(spci1)
                    Next
                    sumc *= rec
                    If _condtype = Column.condtype.Total_Condenser Or _condtype = Column.condtype.Partial_Condenser Then
                        If _specs("C").SpecUnit = "% M/M" Then
                            spfval1 = _xc(0)(spci1) * _LSSj(0) - sumc
                        Else '% W/W
                            spfval1 = _pp.RET_VMM()(spci1) * 1000 * (_xc(0)(spci1) * _LSSj(0) - sumc)
                        End If
                    Else
                        If _specs("C").SpecUnit = "% M/M" Then
                            spfval1 = _yc(0)(spci1) * _Vj(0) - sumc
                        Else '% W/W
                            spfval1 = _pp.RET_VMM()(spci1) * 1000 * (_yc(0)(spci1) * _Vj(0) - sumc)
                        End If
                    End If
                Case ColumnSpec.SpecType.Heat_Duty
                    _Q(0) = spval1 / _maxF
                Case ColumnSpec.SpecType.Product_Mass_Flow_Rate
                    If _condtype = Column.condtype.Total_Condenser Or _condtype = Column.condtype.Partial_Condenser Then
                        spfval1 = _LSSj(0) - spval1 / _pp.AUX_MMM(_xc(0)) * 1000 / _maxF
                    Else
                        spfval1 = _Vj(0) - spval1 / _pp.AUX_MMM(_yc(0)) * 1000 / _maxF
                    End If
                Case ColumnSpec.SpecType.Product_Molar_Flow_Rate
                    If _condtype = Column.condtype.Total_Condenser Or _condtype = Column.condtype.Partial_Condenser Then
                        spfval1 = _LSSj(0) - spval1 / _maxF
                    Else
                        spfval1 = _Vj(0) - spval1 / _maxF
                    End If
                Case ColumnSpec.SpecType.Stream_Ratio
                    If _condtype = Column.condtype.Total_Condenser Or _condtype = Column.condtype.Partial_Condenser Then
                        spfval1 = _Lj(0) - spval1 * _LSSj(0)
                    Else
                        spfval1 = _Lj(0) - spval1 * _Vj(0)
                    End If
                Case ColumnSpec.SpecType.Temperature
                    spfval1 = _Tj(0) - spval1
            End Select

            'Reboiler Specs
            Select Case _specs("R").SType
                Case ColumnSpec.SpecType.Component_Fraction
                    If _specs("R").SpecUnit = "M" Then
                        spfval2 = _xc(_ns)(spci2) - spval2
                    Else 'W
                        spfval2 = _pp.AUX_CONVERT_MOL_TO_MASS(_xc(_ns))(spci2) - spval2
                    End If
                Case ColumnSpec.SpecType.Component_Mass_Flow_Rate
                    spfval2 = _Lj(_ns) * _xc(_ns)(spci2) - spval2 / _pp.RET_VMM()(spci2) * 1000 / _maxF
                Case ColumnSpec.SpecType.Component_Molar_Flow_Rate
                    spfval2 = _Lj(_ns) * _xc(_ns)(spci2) - spval2 / _maxF
                Case ColumnSpec.SpecType.Component_Recovery
                    Dim rec As Double = spval2 / 100
                    Dim sumc As Double = 0
                    For j = 0 To _ns
                        sumc += _fc(j)(spci2)
                    Next
                    sumc *= rec
                    If _specs("R").SpecUnit = "% M/M" Then
                        spfval2 = _lc(_ns)(spci2) - sumc
                    Else '% W/W
                        spfval2 = _pp.RET_VMM()(spci2) * 1000 * (_lc(_ns)(spci2) - sumc)
                    End If
                Case ColumnSpec.SpecType.Heat_Duty
                    _Q(_ns) = spval2 / _maxF
                Case ColumnSpec.SpecType.Product_Mass_Flow_Rate
                    spfval2 = _Lj(_ns) - spval2 / _pp.AUX_MMM(_xc(_ns)) * 1000 / _maxF
                Case ColumnSpec.SpecType.Product_Molar_Flow_Rate
                    spfval2 = _Lj(_ns) - spval2 / _maxF
                Case ColumnSpec.SpecType.Stream_Ratio
                    spfval2 = _Vj(_ns) - spval2 * _Lj(_ns)
                Case ColumnSpec.SpecType.Temperature
                    spfval2 = _Tj(_ns) - spval2
            End Select

            For i = 0 To _ns
                If i = 0 Then
                    entbal(i) = (Hl(i) * _Rlj(i) * _Lj(i) + Hv(i) * _Rvj(i) * _Vj(i) - Hv(i + 1) * _Vj(i + 1) - _HF(i) * _F(i) - _Q(i))
                ElseIf i = _ns Then
                    entbal(i) = (Hl(i) * _Rlj(i) * _Lj(i) + Hv(i) * _Rvj(i) * _Vj(i) - Hl(i - 1) * _Lj(i - 1) - _HF(i) * _F(i) - _Q(i))
                Else
                    entbal(i) = (Hl(i) * _Rlj(i) * _Lj(i) + Hv(i) * _Rvj(i) * _Vj(i) - Hl(i - 1) * _Lj(i - 1) - Hv(i + 1) * _Vj(i + 1) - _HF(i) * _F(i) - _Q(i))
                End If
                entbal(i) = entbal(i) / (Hv(i) - Hl(i))
                Select Case _coltype
                    Case Column.ColType.DistillationColumn
                        entbal(0) = spfval1
                        entbal(_ns) = spfval2
                    Case Column.ColType.AbsorptionColumn
                        'do nothing
                    Case Column.ColType.ReboiledAbsorber
                        entbal(_ns) = spfval2
                    Case Column.ColType.RefluxedAbsorber
                        entbal(0) = spfval1
                End Select
            Next

            For i = 0 To _ns
                _Tj(i) = _Tj0(i)
            Next
            Dim errors(_bx.Length - 1) As Double

            For i = 0 To _bx.Length - 1
                If i <= _ns Then
                    errors(i) = entbal(i)
                ElseIf i > _ns And i <= _vcnt + _ns Then
                    For j = 0 To _ns
                        If _Rvj(j) <> 1 Then
                            errors(i) = (_VSS(j) - _VSSj(j)) '/ _VSS(j)
                            i += 1
                        End If
                    Next
                End If
                If i > _vcnt + _ns And i <= _vcnt + _lcnt + _ns Then
                    For j = 1 To _ns
                        If _Rlj(j) <> 1 Then
                            errors(i) = (_LSS(j) - _LSSj(j)) '/ _LSS(j)
                            i += 1
                        End If
                    Next
                End If
            Next


            Dim il_err As Double = 0
            For i = 0 To _bx.Length - 1
                il_err += errors(i) ^ 2
            Next

            Return il_err

        End Function

        Public Function Solve(ByVal nc As Integer, ByVal ns As Integer, ByVal maxits As Integer, _
                                ByVal tol As Array, ByVal F As Array, ByVal V As Array, _
                                ByVal Q As Array, ByVal L As Array, _
                                ByVal VSS As Array, ByVal LSS As Array, ByVal Kval()() As Double, _
                                ByVal x()() As Double, ByVal y()() As Double, ByVal z()() As Double, _
                                ByVal fc()() As Double, _
                                ByVal HF As Array, ByVal T As Array, ByVal P As Array, _
                                ByVal condt As DistillationColumn.condtype, _
                                ByVal eff() As Double, _
                                ByVal UseDampingFactor As Boolean, _
                                ByVal UseNewtonUpdate As Boolean, _
                                ByVal AdjustSb As Boolean, ByVal UseIJ As Boolean, _
                                ByVal coltype As Column.ColType, ByVal KbjWA As Boolean, _
                                ByVal pp As PropertyPackages.PropertyPackage, _
                                ByVal specs As Dictionary(Of String, SepOps.ColumnSpec), _
                                ByVal reuseJ As Boolean, ByVal jac0 As Object, _
                                ByVal epsilon As Double, _
                                ByVal maxvarchgfac As Integer, _
                                ByVal dfmin As Double, ByVal dfmax As Double, _
                                ByVal deltat_el As Double, _
                                Optional ByVal llex As Boolean = False) As Object

            Dim doparallel As Boolean = My.Settings.EnableParallelProcessing
            Dim poptions As New ParallelOptions() With {.MaxDegreeOfParallelism = My.Settings.MaxDegreeOfParallelism}

            llextr = llex 'liq-liq extractor

            ndeps = epsilon

            Dim brentsolver As New BrentOpt.BrentMinimize
            brentsolver.DefineFuncDelegate(AddressOf MinimizeError)

            Dim cv As New SistemasDeUnidades.Conversor
            Dim spval1, spval2 As Double
            Dim spci1, spci2 As Integer

            spval1 = cv.ConverterParaSI(specs("C").SpecUnit, specs("C").SpecValue)
            spci1 = specs("C").ComponentIndex
            spval2 = cv.ConverterParaSI(specs("R").SpecUnit, specs("R").SpecValue)
            spci2 = specs("R").ComponentIndex

            _ns = ns
            _nc = nc

            Dim ic, ec, iic As Integer
            Dim Tj(ns), Tj_ant(ns), T_(ns) As Double
            Dim Lj(ns), Vj(ns), xc(ns)(), yc(ns)(), lc(ns)(), vc(ns)(), zc(ns)() As Double
            Dim sum1(ns) As Double
            Dim i, j, w, m1, m2 As Integer

            'step0
            'normalize initial estimates

            Dim maxF As Double = Common.Max(F)

            For i = 0 To ns
                F(i) = F(i) / maxF
                HF(i) = HF(i) / 1000
                L(i) = L(i) / maxF
                V(i) = V(i) / maxF
                LSS(i) = LSS(i) / maxF
                VSS(i) = VSS(i) / maxF
                Q(i) = Q(i) / maxF
            Next

            'step1

            Dim sumF As Double = 0
            Dim sumLSS As Double = 0
            Dim sumVSS As Double = 0
            For i = 0 To ns
                sumF += F(i)
                If i > 0 Then sumLSS += LSS(i)
                sumVSS += VSS(i)
            Next

            Dim B As Double
            If condt = Column.condtype.Total_Condenser Then
                B = sumF - sumLSS - sumVSS - LSS(0)
            ElseIf condt = Column.condtype.Partial_Condenser Then
                B = sumF - sumLSS - sumVSS - V(0) - LSS(0)
            Else
                B = sumF - sumLSS - sumVSS - V(0)
            End If

            'step2

            Dim lsi, vsi As New ArrayList
            Dim el As Integer

            'size jacobian

            el = ns
            For i = 0 To ns
                If VSS(i) <> 0 Then
                    el += 1
                    vsi.Add(i)
                End If
                If LSS(i) <> 0 And i > 0 Then
                    el += 1
                    lsi.Add(i)
                End If
            Next

            Dim hes(el, el) As Double
            Dim bx(el), bxb(el), bf(el), bfb(el), bp(el), bp_ant(el) As Double

            Dim f1(el), f2(el), f3(el), f4(el) As Double
            Dim u As Integer = 0

            'find Kbref 

            Dim Kbj(ns), Kbj_ant(ns) As Object
            Dim K(ns, nc - 1), K_ant(ns, nc - 1), K2j(ns, nc - 1) As Object

            Dim Kw1(ns)(), Kw2(ns)() As Object
            Dim wi(ns, nc - 1), ti(ns, nc - 1), sumwi(ns), sumti(ns) As Double
            For i = 0 To ns
                Array.Resize(Kw1(i), nc)
                Array.Resize(Kw2(i), nc)
            Next

            Dim tmp0 As Object = Nothing

            For i = 0 To ns
                If Not llextr Then tmp0 = pp.DW_CalcKvalue(z(i), T(i), P(i))
                For j = 0 To nc - 1
                    If Not llextr Then K(i, j) = tmp0(j) Else K(i, j) = Kval(i)(j)
                    If Double.IsNaN(K(i, j)) Or Double.IsInfinity(K(i, j)) Or K(i, j) = 0 Then K(i, j) = pp.AUX_PVAPi(j, T(i)) / P(i)
                Next
                CheckCalculatorStatus()
            Next

            If KbjWA = False Then
                Kbj = CalcKbj1(ns, nc, K, z, y, T, P, pp)
            Else
                Kbj = CalcKbj2(ns, nc, K, z, y, T, P, pp)
            End If

            'relative volatilities

            Dim alpha(ns, nc - 1), alpha_ant(ns, nc - 1) As Double

            For i = 0 To ns
                For j = 0 To nc - 1
                    alpha(i, j) = K(i, j) / Kbj(i)
                Next
            Next

            'initialize A/B/C/D/E/F

            Dim Kbj1(ns), Kbj2(ns) As Object
            Dim Tj1(ns), Tj2(ns), Aj(ns), Bj(ns), Cj(ns), Dj(ns), Ej(ns), Fj(ns) As Double
            Dim Aj_ant(ns), Bj_ant(ns), Cj_ant(ns), Dj_ant(ns), Ej_ant(ns), Fj_ant(ns) As Double
            Dim Hl1(ns), Hl2(ns), Hv1(ns), Hv2(ns) As Double
            Dim K2(ns)() As Double
            Dim Hv(ns), Hl(ns), DHv(ns), DHl(ns), Hidv(ns), Hidl(ns) As Double

            For i = 0 To ns
                T_(i) = T(i) - 1
                Tj(i) = T(i)
                'Kbjs, Ts
                Tj1(i) = T(i)
                Tj2(i) = T(i) + 1
                Kbj1(i) = Kbj(i)
                'new Ks
                If llextr Then
                    K2(i) = pp.DW_CalcKvalue(x(i), y(i), Tj2(i), P(i), "LL")
                    Hv1(i) = pp.DW_CalcEnthalpyDeparture(y(i), Tj1(i), P(i), PropertyPackages.State.Liquid)
                    Hv2(i) = pp.DW_CalcEnthalpyDeparture(y(i), Tj2(i), P(i), PropertyPackages.State.Liquid)
                Else
                    K2(i) = pp.DW_CalcKvalue(x(i), y(i), Tj2(i), P(i))
                    'Hv1(i) = pp.DW_CalcEnthalpyDeparture(y(i), Tj1(i), P(i), PropertyPackages.State.Vapor)
                    'Hv2(i) = pp.DW_CalcEnthalpyDeparture(y(i), Tj2(i), P(i), PropertyPackages.State.Vapor)
                End If
                'Hl1(i) = pp.DW_CalcEnthalpyDeparture(x(i), Tj1(i), P(i), PropertyPackages.State.Liquid)
                'Hl2(i) = pp.DW_CalcEnthalpyDeparture(x(i), Tj2(i), P(i), PropertyPackages.State.Liquid)
                For j = 0 To nc - 1
                    K2j(i, j) = K2(i)(j)
                    If Double.IsNaN(K2(i)(j)) Or Double.IsInfinity(K2(i)(j)) Then K2(i)(j) = pp.AUX_PVAPi(j, T(i)) / P(i)
                Next
            Next

            If KbjWA = False Then
                Kbj2 = CalcKbj1(ns, nc, K2j, z, y, Tj2, P, pp)
            Else
                Kbj2 = CalcKbj2(ns, nc, K2j, z, y, Tj2, P, pp)
            End If

            For i = 0 To ns
                Bj(i) = Log(Kbj1(i) / Kbj2(i)) / (1 / Tj2(i) - 1 / Tj1(i))
                Aj(i) = Log(Kbj1(i)) + Bj(i) * (1 / Tj1(i))
                'Dj(i) = (Hv1(i) - Hv2(i)) / (Tj1(i) - Tj2(i))
                'Cj(i) = Hv1(i) - Dj(i) * (Tj1(i) - T_(i))
                'Fj(i) = (Hl1(i) - Hl2(i)) / (Tj1(i) - Tj2(i))
                'Ej(i) = Hl1(i) - Fj(i) * (Tj1(i) - T_(i))
            Next

            'external loop

            Dim Sb, sbf, sbf_ant, sbf_ant2, sbx, sbx_ant, sbx_ant2, fval As Double
            Dim SbOK As Boolean = True
            Dim BuildingJacobian As Boolean = False

            If AdjustSb Then SbOK = False

            Sb = 1

            Dim el_err As Double = 0.0#
            Dim el_err_ant As Double = 0.0#
            Dim il_err As Double = 0.0#
            Dim il_err_ant As Double = 0.0#

            'independent variables -> stripping and withdrawal factors

            Dim Sbj(ns), lnSbj0(ns), lnSbj(ns), S(ns, nc - 1) As Double
            Dim Rvj(ns), Rlj(ns), lnRvj(ns), lnRlj(ns), lnRvj0(ns), lnRlj0(ns) As Double
            Dim VSSj(ns), LSSj(ns), PSbj As Double
            Dim Nss As Integer = ns + 1
            'Calculo de Sbj, Rlj y Rvj del Lazo Externo

            For i = 0 To ns
                Sbj(i) = Kbj(i) * V(i) / L(i)
            Next
            If AdjustSb Then
                SbOK = False
                PSbj = 1
                For i = 0 To ns
                    If i = 0 And condt = Column.condtype.Total_Condenser Then
                        Nss -= 1
                    Else
                        PSbj *= Sbj(i)
                    End If
                Next
                Sb = PSbj ^ (1 / (Nss))
            Else
                Sb = 1
            End If

            For i = 0 To ns
                If Sbj(i) = 0 Then Sbj(i) = 1.0E-20
                lnSbj(i) = Log(Sbj(i))
                If V(i) <> 0 Then Rvj(i) = 1 + VSS(i) / V(i) Else Rvj(i) = 1
                lnRvj(i) = Log(Rvj(i))
                If L(i) <> 0 Then Rlj(i) = 1 + LSS(i) / L(i) Else Rlj(i) = 1
                lnRlj(i) = Log(Rlj(i))
                For j = 0 To nc - 1
                    S(i, j) = Sbj(i) * alpha(i, j) * Sb
                Next
            Next

            Dim vcnt, lcnt As Integer

            vcnt = 0
            For i = 0 To ns
                If lnRvj(i) <> 0 And Not Double.IsInfinity(lnRvj(i)) Then
                    vcnt += 1
                End If
            Next

            lcnt = 0
            For i = 1 To ns
                If lnRlj(i) <> 0 And Not Double.IsInfinity(lnRlj(i)) Then
                    lcnt += 1
                End If
            Next

            'internal loop

            Dim check1 As Boolean = False
            Dim num, denom, x0, fx0 As New ArrayList

            w = 0

1:          Dim ic0 As Integer = 0

            Do

                If Not SbOK Then

                    sbf_ant2 = sbf_ant
                    sbf_ant = sbf

                    sumF = 0
                    sumLSS = 0
                    sumVSS = 0
                    For j = 0 To _ns
                        sumF += F(j)
                        If j > 0 Then sumLSS += LSS(j)
                        sumVSS += VSS(j)
                    Next

                    sbf = sumF - sumLSS - sumVSS - V(0) - L(ns) - LSS(0)

                    sbx_ant2 = sbx_ant
                    sbx_ant = sbx

                    If ic0 > 1 Then
                        If Abs((-sbf * (sbx - sbx_ant2) / (sbf - sbf_ant2)) / sbx) > 1 Then
                            sbx = sbx_ant2 * 1.01
                        Else
                            sbx = sbx - sbf * (sbx - sbx_ant2) / (sbf - sbf_ant2)
                        End If
                    Else
                        sbx = Sb * 1.01
                    End If

                    If sbx < 0 Then sbx = Abs(sbx)

                    Sb = sbx

                    For i = 0 To ns
                        Sbj(i) = Kbj(i) * V(i) / L(i)
                        If Sbj(i) = 0 Then Sbj(i) = 1.0E-20
                        lnSbj(i) = Log(Sbj(i))
                        If V(i) <> 0 Then Rvj(i) = 1 + VSS(i) / V(i) Else Rvj(i) = 1
                        lnRvj(i) = Log(Rvj(i))
                        If L(i) <> 0 Then Rlj(i) = 1 + LSS(i) / L(i) Else Rlj(i) = 1
                        lnRlj(i) = Log(Rlj(i))
                        For j = 0 To nc - 1
                            S(i, j) = Sbj(i) * alpha(i, j) * Sb
                        Next
                    Next
                End If

                If SbOK Then Exit Do

                If sbx > 10 Then Sb = sbx_ant2

                ic0 += 1

                CheckCalculatorStatus()

                If Double.IsNaN(sbf) Then Throw New Exception(DWSIM.App.GetLocalString("DCSbError"))

            Loop Until Abs(sbf) < 0.001

            SbOK = True

            Dim fx(el), dfdx(el, el), dfdx_ant(el, el), dx(el), xvar(el), xvar_ant(el), itol As Double
            Dim jac As New Mapack.Matrix(el + 1, el + 1), hesm As New Mapack.Matrix(el + 1, el + 1)
            Dim perturb As Boolean = False, bypass As Boolean = False

            ec = 0
            ic = 0
            Do

                iic = 0

                'step3


                For i = 0 To ns
                    'store initial values
                    lnSbj0(i) = lnSbj(i)
                    lnRvj0(i) = lnRvj(i)
                    lnRlj0(i) = lnRlj(i)
                Next

                'update inner loop parameters

                Dim lnSbj_ant(ns), lnRvj_ant(ns), lnRlj_ant(ns), df, df_ant, xlowbound As Double

                df_ant = df

                _Bj = Bj.Clone
                _Aj = Aj.Clone
                '_Cj = Cj.Clone
                '_Dj = Dj.Clone
                '_Ej = Ej.Clone
                '_Fj = Fj.Clone
                _eff = eff.Clone
                _Tj = Tj.Clone
                _T_ = T_.Clone
                _Lj = Lj.Clone
                _Vj = Vj.Clone
                _LSSj = LSS.Clone
                _VSSj = VSS.Clone
                _LSS = LSS.Clone
                _VSS = VSS.Clone
                _Rlj = Rlj.Clone
                _Rvj = Rvj.Clone
                _F = F.Clone
                _P = P.Clone
                _HF = HF.Clone
                _Q = Q.Clone
                _S = S.Clone
                _condtype = condt
                _alpha = alpha.Clone
                _fc = fc.Clone
                _xc = xc.Clone
                _yc = yc.Clone
                _lc = lc.Clone
                _vc = vc.Clone
                _zc = zc.Clone
                _Kbj = Kbj.Clone
                _pp = pp
                _coltype = coltype
                _bx = bx.Clone
                _dbx = bp.Clone
                _Sb = Sb
                _vcnt = vcnt
                _lcnt = lcnt
                _specs = specs
                _maxF = maxF

                'solve using newton's method

                For i = 0 To ns
                    If i = 0 And condt = Column.condtype.Total_Condenser Or i = 0 And condt = Column.condtype.Partial_Condenser Then
                        xvar(i) = lnRlj(i)
                    Else
                        xvar(i) = lnSbj(i)
                    End If
                Next

                m1 = 0

                If vcnt > 0 Then
                    For i = ns + 1 To vcnt + ns
                        For j = m1 To ns
                            If Rvj(j) <> 1 Then
                                m1 = j + 1
                                Exit For
                            End If
                        Next
                        xvar(i) = lnRvj(m1 - 1)
                    Next
                End If

                m2 = 0

                If lcnt > 0 Then
                    For i = vcnt + ns + 1 To vcnt + lcnt + ns
                        For j = m2 + 1 To ns
                            If Rlj(j) <> 1 Then
                                m2 = j + 1
                                Exit For
                            End If
                        Next
                        xvar(i) = lnRlj(m2 - 1)
                    Next
                End If

                ic0 = 0

                'first run (to initialize variables)
                'fx = Me.FunctionValue(xvar)

                Do

restart:            fx = Me.FunctionValue(xvar)
                    If UseNewtonUpdate Then
                        dfdx_ant = dfdx
                        dfdx = Me.FunctionGradient(xvar)
                        Dim success As Boolean
                        success = MathEx.SysLin.rsolve.rmatrixsolve(dfdx, fx, el + 1, dx)
                        If Not success Then
                            dfdx = dfdx_ant
                            success = MathEx.SysLin.rsolve.rmatrixsolve(dfdx, fx, el + 1, dx)
                        End If
                        For i = 0 To el
                            dx(i) = -dx(i)
                        Next
                        _bx = xvar.Clone
                        _dbx = dx.Clone
                    Else
                        If ic = 0 Then
                            If UseIJ Then
                                For i = 0 To el
                                    For j = 0 To el
                                        If i = j Then hes(i, j) = 1 Else hes(i, j) = 0
                                    Next
                                Next
                            Else
                                dfdx = Me.FunctionGradient(xvar)
                                For i = 0 To el
                                    For j = 0 To el
                                        jac(i, j) = dfdx(i, j)
                                    Next
                                Next
                                hesm = jac.Inverse
                                For i = 0 To el
                                    For j = 0 To el
                                        hes(i, j) = hesm(i, j)
                                    Next
                                Next
                            End If
                            bx = xvar
                            bf = fx
                            Broyden.broydn(el, bx, bf, bp, bxb, bfb, hes, 0)
                            dx = bp
                        Else
                            bx = xvar
                            bf = fx
                            Broyden.broydn(el, bx, bf, bp, bxb, bfb, hes, 1)
                            dx = bp
                        End If
                        _bx = xvar.Clone
                        _dbx = bp.Clone
                    End If

                    'this call to the brent solver calculates the damping factor which minimizes the error (fval).
                    itol = tol(0) * ns
                    df = 1
                    If UseDampingFactor Then fval = brentsolver.brentoptimize(dfmin, dfmax, tol(0), df)

                    perturb = False
                    bypass = False
                    xlowbound = 0.1
                    For i = 0 To el
                        xvar_ant(i) = xvar(i)
                        xvar(i) += dx(i) * df

                        If Abs((dx(i) * df) / xvar_ant(i)) > 10 Then
                            'perturb = True
                            xvar(i) = xvar_ant(i) - df * (xvar_ant(i) - xlowbound) * 0.5
                        End If
                        If Double.IsNaN(dx(i)) Or Double.IsInfinity(dx(i)) Then
                            bypass = True
                        End If
                    Next

                    'If perturb Then
                    '    For i = 0 To el
                    '        xvar(i) = xvar_ant(i) * (1 + 0.3 * Math.Sign(dx(i)))
                    '    Next
                    'End If

                    'If bypass Then
                    '    For i = 0 To el
                    '        xvar(i) = xvar_ant(i) * 0.95
                    '    Next
                    'End If


                    il_err_ant = il_err

                    il_err = 0
                    For i = 0 To el
                        il_err += fx(i) ^ 2
                    Next

                    ic += 1
                    ic0 += 1

                    'If bypass Then GoTo restart

                    If ic0 >= maxits Then Throw New Exception(DWSIM.App.GetLocalString("DCMaxIterationsReached"))
                    If Double.IsNaN(il_err) Then Throw New Exception(DWSIM.App.GetLocalString("DCGeneralError"))
                    If MathEx.Common.AbsSum(dx) = 0.0# Or Abs((il_err - il_err_ant) / il_err) < itol Then Exit Do


                    CheckCalculatorStatus()

                Loop Until il_err < itol

                For i = 0 To ns
                    If i = 0 And _condtype = Column.condtype.Total_Condenser Then
                        lnRlj_ant(i) = lnRlj(i)
                        lnRlj(i) = xvar(i)
                        Rlj(i) = Exp(lnRlj(i))
                    Else
                        lnSbj_ant(i) = lnSbj(i)
                        lnSbj(i) = xvar(i)
                        Sbj(i) = Exp(lnSbj(i))
                        For j = 0 To nc - 1
                            S(i, j) = Sbj(i) * alpha(i, j) * Sb
                        Next
                    End If
                Next

                m1 = 0

                If vcnt > 0 Then
                    For i = ns To vcnt + ns
                        For j = m1 To ns
                            If Rvj(j) <> 1 Then
                                m1 = j + 1
                                Exit For
                            End If
                        Next
                        lnRvj_ant(m1 - 1) = lnRvj(m1 - 1)
                        lnRvj(m1 - 1) = xvar(i)
                        Rvj(m1 - 1) = Exp(lnRvj(m1 - 1))
                    Next
                End If

                m2 = 0

                If lcnt > 0 Then
                    For i = vcnt + ns + 1 To vcnt + lcnt + ns
                        For j = m2 + 1 To ns
                            If Rlj(j) <> 1 Then
                                m2 = j + 1
                                Exit For
                            End If
                        Next
                        lnRlj_ant(m2 - 1) = lnRlj(m2 - 1)
                        lnRlj(m2 - 1) = xvar(i)
                        Rlj(m2 - 1) = Exp(lnRlj(m2 - 1))
                    Next
                End If

                ic += 1
                iic += 1

                'step9 (external loop)

                Tj_ant = Tj.Clone
                Tj = _Tj.Clone
                T_ = _T_.Clone
                Lj = _Lj.Clone
                Vj = _Vj.Clone
                Q = _Q.Clone
                LSSj = _LSSj.Clone
                VSSj = _VSSj.Clone
                xc = _xc.Clone
                yc = _yc.Clone
                lc = _lc.Clone
                vc = _vc.Clone
                zc = _zc.Clone
                Kbj = _Kbj.Clone

                For i = 0 To ns

                    T_(i) = Tj(i)
                    Tj1(i) = Tj(i)
                    Tj2(i) = Tj(i) + deltat_el * (i + 1)

                Next

                'update external loop variables using rigorous models

                el_err_ant = el_err
                el_err = 0

                Dim tmp(ns) As Object

                If doparallel Then
                    My.Application.IsRunningParallelTasks = True
                    Dim task1 As Task = Task.Factory.StartNew(Sub() Parallel.For(0, ns + 1, poptions,
                                                             Sub(ipar)
                                                                 If llextr Then
                                                                     tmp(ipar) = pp.DW_CalcKvalue(xc(ipar), yc(ipar), Tj(ipar), P(ipar), "LL")
                                                                 Else
                                                                     tmp(ipar) = pp.DW_CalcKvalue(xc(ipar), yc(ipar), Tj(ipar), P(ipar))
                                                                 End If
                                                             End Sub))
                    While Not task1.IsCompleted
                        Application.DoEvents()
                    End While
                    For i = 0 To ns
                        For j = 0 To nc - 1
                            K_ant(i, j) = K(i, j)
                            K(i, j) = tmp(i)(j)
                        Next
                        Kbj_ant(i) = Kbj(i)
                    Next
                    My.Application.IsRunningParallelTasks = False
                Else
                    For i = 0 To ns
                        If llextr Then
                            tmp(i) = pp.DW_CalcKvalue(xc(i), yc(i), Tj(i), P(i), "LL")
                        Else
                            tmp(i) = pp.DW_CalcKvalue(xc(i), yc(i), Tj(i), P(i))
                        End If
                        For j = 0 To nc - 1
                            K_ant(i, j) = K(i, j)
                            K(i, j) = tmp(i)(j)
                        Next
                        Kbj_ant(i) = Kbj(i)
                    Next
                End If

                If KbjWA = False Then
                    Kbj1 = CalcKbj1(ns, nc, K, zc, yc, Tj1, P, pp)
                Else
                    Kbj1 = CalcKbj2(ns, nc, K, zc, yc, Tj1, P, pp)
                End If
                'Kbj1 = Kbj
                Kbj = Kbj1

                'update relative volatilities

                For i = 0 To ns
                    For j = 0 To nc - 1
                        alpha_ant(i, j) = alpha(i, j)
                        alpha(i, j) = K(i, j) / Kbj(i)
                        el_err += Abs((alpha(i, j) - alpha_ant(i, j)) / alpha_ant(i, j)) ^ 2
                    Next
                Next

                For i = 0 To ns
                    Sbj(i) = Kbj(i) * Vj(i) / Lj(i)
                    If Sbj(i) = 0.0# Then Sbj(i) = 1.0E-20
                    lnSbj(i) = Log(Sbj(i))
                    If Vj(i) <> 0 Then Rvj(i) = 1 + VSSj(i) / Vj(i) Else Rvj(i) = 1
                    lnRvj(i) = Log(Rvj(i))
                    If Lj(i) <> 0 Then Rlj(i) = 1 + LSSj(i) / Lj(i) Else Rlj(i) = 1
                    lnRlj(i) = Log(Rlj(i))
                    For j = 0 To nc - 1
                        S(i, j) = Sbj(i) * alpha(i, j) * Sb
                    Next
                Next

                'update A/B/C/D/E/F

                If doparallel Then
                    My.Application.IsRunningParallelTasks = True
                    Dim task1 As Task = Task.Factory.StartNew(Sub() Parallel.For(0, ns + 1, poptions,
                                                             Sub(ipar)
                                                                 'new Ks
                                                                 K2(ipar) = pp.DW_CalcKvalue(xc(ipar), yc(ipar), Tj2(ipar), P(ipar))
                                                             End Sub))
                    While Not task1.IsCompleted
                        Application.DoEvents()
                    End While
                    For i = 0 To ns
                        For j = 0 To nc - 1
                            K2j(i, j) = K2(i)(j)
                            If Double.IsNaN(K2(i)(j)) Or Double.IsInfinity(K2(i)(j)) Then K2(i)(j) = pp.AUX_PVAPi(j, Tj(i)) / P(i)
                        Next
                    Next
                    My.Application.IsRunningParallelTasks = False
                Else
                    For i = 0 To ns

                        'new Ks
                        K2(i) = pp.DW_CalcKvalue(xc(i), yc(i), Tj2(i), P(i))

                        For j = 0 To nc - 1
                            K2j(i, j) = K2(i)(j)
                            If Double.IsNaN(K2(i)(j)) Or Double.IsInfinity(K2(i)(j)) Then K2(i)(j) = pp.AUX_PVAPi(j, Tj(i)) / P(i)
                        Next

                    Next
                End If

                If doparallel Then
                    My.Application.IsRunningParallelTasks = True
                    Dim task1 As Task = Task.Factory.StartNew(Sub() Parallel.For(0, ns + 1, poptions,
                                                             Sub(ipar)
                                                                 'enthalpies
                                                                 If llextr Then
                                                                     Hv1(ipar) = pp.DW_CalcEnthalpyDeparture(yc(ipar), Tj1(ipar), P(ipar), PropertyPackages.State.Liquid)
                                                                     Hv2(ipar) = pp.DW_CalcEnthalpyDeparture(yc(ipar), Tj2(ipar), P(ipar), PropertyPackages.State.Liquid)
                                                                 Else
                                                                     'Hv1(ipar) = pp.DW_CalcEnthalpyDeparture(yc(ipar), Tj1(ipar), P(ipar), PropertyPackages.State.Vapor)
                                                                     'Hv2(ipar) = pp.DW_CalcEnthalpyDeparture(yc(ipar), Tj2(ipar), P(ipar), PropertyPackages.State.Vapor)
                                                                 End If
                                                                 'Hl1(ipar) = pp.DW_CalcEnthalpyDeparture(xc(ipar), Tj1(ipar), P(ipar), PropertyPackages.State.Liquid)
                                                                 'Hl2(ipar) = pp.DW_CalcEnthalpyDeparture(xc(ipar), Tj2(ipar), P(ipar), PropertyPackages.State.Liquid)
                                                             End Sub))
                    While Not task1.IsCompleted
                        Application.DoEvents()
                    End While
                    My.Application.IsRunningParallelTasks = False
                Else
                    For i = 0 To ns

                        'enthalpies
                        If llextr Then
                            Hv1(i) = pp.DW_CalcEnthalpyDeparture(yc(i), Tj1(i), P(i), PropertyPackages.State.Liquid)
                            Hv2(i) = pp.DW_CalcEnthalpyDeparture(yc(i), Tj2(i), P(i), PropertyPackages.State.Liquid)
                        Else
                            'Hv1(i) = pp.DW_CalcEnthalpyDeparture(yc(i), Tj1(i), P(i), PropertyPackages.State.Vapor)
                            'Hv2(i) = pp.DW_CalcEnthalpyDeparture(yc(i), Tj2(i), P(i), PropertyPackages.State.Vapor)
                        End If
                        'Hl1(i) = pp.DW_CalcEnthalpyDeparture(xc(i), Tj1(i), P(i), PropertyPackages.State.Liquid)
                        'Hl2(i) = pp.DW_CalcEnthalpyDeparture(xc(i), Tj2(i), P(i), PropertyPackages.State.Liquid)

                    Next
                End If

                If KbjWA = False Then
                    Kbj2 = CalcKbj1(ns, nc, K2j, zc, yc, Tj2, P, pp)
                Else
                    Kbj2 = CalcKbj2(ns, nc, K2j, zc, yc, Tj2, P, pp)
                End If

                Dim Aerr(ns), Berr(ns) As Double

                For i = 0 To ns
                    Bj_ant(i) = Bj(i)
                    Bj(i) = Log(Kbj1(i) / Kbj2(i)) / (1 / Tj2(i) - 1 / Tj1(i))
                    Berr(i) = Bj(i) - Bj_ant(i)
                    Aj_ant(i) = Aj(i)
                    Aj(i) = Log(Kbj1(i)) + Bj(i) * (1 / Tj1(i))
                    Aerr(i) = Aj(i) - Aj_ant(i)
                    'Dj_ant(i) = Dj(i)
                    'Dj(i) = (Hv1(i) - Hv2(i)) / (Tj1(i) - Tj2(i))
                    'Cj_ant(i) = Cj(i)
                    'Cj(i) = Hv1(i) - Dj(i) * (Tj1(i) - T_(i))
                    'Fj_ant(i) = Fj(i)
                    'Fj(i) = (Hl1(i) - Hl2(i)) / (Tj1(i) - Tj2(i))
                    'Ej_ant(i) = Ej(i)
                    'Ej(i) = Hl1(i) - Fj(i) * (Tj1(i) - T_(i))
                Next

                ec += 1

                If ec >= maxits Then Throw New Exception(DWSIM.App.GetLocalString("DCMaxIterationsReached"))
                If Double.IsNaN(el_err) Then Throw New Exception(DWSIM.App.GetLocalString("DCGeneralError"))

                If AdjustSb Then SbOK = False
                Sb = 1

                CheckCalculatorStatus()

            Loop Until Abs(el_err) < tol(1) * el

            If Abs(il_err) > tol(0) * ns Then
                My.Application.ActiveSimulation.WriteToLog("The sum of squared absolute errors (internal loop) isn't changing anymore. Final value is " & il_err & ".", Color.Green, FormClasses.TipoAviso.Aviso)
            End If

            ' finished, de-normalize and return arrays

            For i = 0 To ns
                Lj(i) = Lj(i) * maxF
                Vj(i) = Vj(i) * maxF
                LSSj(i) = LSSj(i) * maxF
                VSSj(i) = VSSj(i) * maxF
                F(i) = F(i) * maxF
                L(i) = L(i) * maxF
                V(i) = V(i) * maxF
                LSS(i) = LSS(i) * maxF
                VSS(i) = VSS(i) * maxF
                Q(i) = Q(i) * maxF
            Next

            If Not UseNewtonUpdate Then
                For i = 0 To el
                    For j = 0 To el
                        hesm(i, j) = hes(i, j)
                    Next
                Next
                jac = hesm.Inverse
                For i = 0 To el
                    For j = 0 To el
                        dfdx(i, j) = jac(i, j)
                    Next
                Next
            End If

            Return New Object() {Tj, Vj, Lj, VSSj, LSSj, yc, xc, K, Q, ic, il_err, ec, el_err, dfdx}

        End Function

    End Class

    <System.Serializable()> Public Class WangHenkeMethod

        Public Shared Function Solve(ByVal nc As Integer, ByVal ns As Integer, ByVal maxits As Integer, _
                                ByVal tol As Array, ByVal F As Array, ByVal V As Array, _
                                ByVal Q As Array, ByVal L As Array, _
                                ByVal VSS As Array, ByVal LSS As Array, ByVal Kval()() As Double, _
                                ByVal x()() As Double, ByVal y()() As Double, ByVal z()() As Double, _
                                ByVal fc()() As Double, _
                                ByVal HF As Array, ByVal T As Array, ByVal P As Array, _
                                ByVal condt As DistillationColumn.condtype, _
                                ByVal stopatitnumber As Integer, _
                                ByVal eff() As Double, _
                                ByVal coltype As Column.ColType, _
                                ByVal pp As PropertyPackages.PropertyPackage, _
                                ByVal specs As Dictionary(Of String, SepOps.ColumnSpec)) As Object

            Dim doparallel As Boolean = My.Settings.EnableParallelProcessing
            Dim poptions As New ParallelOptions() With {.MaxDegreeOfParallelism = My.Settings.MaxDegreeOfParallelism}

            Dim ic As Integer
            Dim t_error, t_error_ant As Double
            Dim Tj(ns), Tj_ant(ns) As Double
            Dim Fj(ns), Lj(ns), Vj(ns), Vj_ant(ns), xc(ns)(), fcj(ns)(), yc(ns)(), lc(ns)(), vc(ns)(), zc(ns)(), K(ns)() As Double
            Dim Hfj(ns), Hv(ns), Hl(ns) As Double
            Dim VSSj(ns), LSSj(ns) As Double

            'step0

            Dim cv As New SistemasDeUnidades.Conversor
            Dim spval1, spval2 As Double

            spval1 = cv.ConverterParaSI(specs("C").SpecUnit, specs("C").SpecValue)
            spval2 = cv.ConverterParaSI(specs("R").SpecUnit, specs("R").SpecValue)

            Select Case specs("C").SType
                Case ColumnSpec.SpecType.Component_Fraction, _
                ColumnSpec.SpecType.Component_Mass_Flow_Rate, _
                ColumnSpec.SpecType.Component_Molar_Flow_Rate, _
                ColumnSpec.SpecType.Component_Recovery, _
                ColumnSpec.SpecType.Temperature
                    Throw New Exception(DWSIM.App.GetLocalString("DCUnsupportedError1"))
            End Select
            Select Case specs("R").SType
                Case ColumnSpec.SpecType.Component_Fraction, _
                ColumnSpec.SpecType.Component_Mass_Flow_Rate, _
                ColumnSpec.SpecType.Component_Molar_Flow_Rate, _
                ColumnSpec.SpecType.Component_Recovery, _
                ColumnSpec.SpecType.Temperature
                    Throw New Exception(DWSIM.App.GetLocalString("DCUnsupportedError1"))
            End Select

            'step1

            Dim rr, B, D2 As Double

            'step2

            Dim i, j As Integer

            For i = 0 To ns
                Array.Resize(fcj(i), nc)
                Array.Resize(xc(i), nc)
                Array.Resize(yc(i), nc)
                Array.Resize(lc(i), nc)
                Array.Resize(vc(i), nc)
                Array.Resize(zc(i), nc)
                Array.Resize(zc(i), nc)
                Array.Resize(K(i), nc)
            Next

            For i = 0 To ns
                VSSj(i) = VSS(i)
                LSSj(i) = LSS(i)
                Lj(i) = L(i)
                Vj(i) = V(i)
                Tj(i) = T(i)
                K(i) = Kval(i)
                Fj(i) = F(i)
                Hfj(i) = HF(i) / 1000
                fcj(i) = fc(i)
            Next

            Dim sumFHF As Double = 0
            Dim sumF As Double = 0
            Dim sumLSSHl As Double = 0
            Dim sumLSS As Double = 0
            Dim sumVSSHv As Double = 0
            Dim sumVSS As Double = 0
            For i = 0 To ns
                sumF += F(i)
                If i > 0 Then sumLSS += LSS(i)
                sumVSS += VSS(i)
                sumFHF += Fj(i) * Hfj(i)
            Next

            If doparallel Then
                My.Application.IsRunningParallelTasks = True
                Dim task1 As Task = Task.Factory.StartNew(Sub() Parallel.For(0, ns + 1, poptions,
                                                         Sub(ipar)
                                                             Hl(ipar) = pp.DW_CalcEnthalpy(x(ipar), Tj(ipar), P(ipar), PropertyPackages.State.Liquid) * pp.AUX_MMM(x(ipar)) / 1000
                                                             Hv(ipar) = pp.DW_CalcEnthalpy(y(ipar), Tj(ipar), P(ipar), PropertyPackages.State.Vapor) * pp.AUX_MMM(y(ipar)) / 1000
                                                         End Sub))
                While Not task1.IsCompleted
                    Application.DoEvents()
                End While
                My.Application.IsRunningParallelTasks = False
            Else
                For i = 0 To ns
                    CheckCalculatorStatus()
                    Hl(i) = pp.DW_CalcEnthalpy(x(i), Tj(i), P(i), PropertyPackages.State.Liquid) * pp.AUX_MMM(x(i)) / 1000
                    Hv(i) = pp.DW_CalcEnthalpy(y(i), Tj(i), P(i), PropertyPackages.State.Vapor) * pp.AUX_MMM(y(i)) / 1000
                Next
            End If

            Select Case specs("C").SType
                Case ColumnSpec.SpecType.Product_Mass_Flow_Rate
                    LSSj(0) = spval1 / pp.AUX_MMM(x(0)) * 1000
                    rr = (Lj(0) + LSSj(0)) / LSSj(0)
                Case ColumnSpec.SpecType.Product_Molar_Flow_Rate
                    LSSj(0) = spval1
                    rr = (Lj(0) + LSSj(0)) / LSSj(0)
                Case ColumnSpec.SpecType.Stream_Ratio
                    rr = spval1
                Case ColumnSpec.SpecType.Heat_Duty
                    Q(0) = spval1
                    'Q(0) = (Vj(1) * Hv(1) + F(0) * Hfj(0) - (Lj(0) + LSSj(0)) * Hl(0) - (Vj(0) + VSSj(0)) * Hv(0))
                    LSSj(0) = -Lj(0) - (Q(0) - Vj(1) * Hv(1) - F(0) * Hfj(0) + (Vj(0) + VSSj(0)) * Hv(0)) / Hl(0)
                    rr = (Lj(0) + LSSj(0)) / LSSj(0)
            End Select

            Select Case specs("R").SType
                Case ColumnSpec.SpecType.Product_Mass_Flow_Rate
                    B = spval2 / pp.AUX_MMM(x(ns)) * 1000
                Case ColumnSpec.SpecType.Product_Molar_Flow_Rate
                    B = spval2
                Case ColumnSpec.SpecType.Stream_Ratio
                    B = Vj(ns) / spval2
                Case ColumnSpec.SpecType.Heat_Duty
                    Q(ns) = spval2
                    Dim sum3, sum4, val1 As Double
                    sum3 = 0
                    sum4 = 0
                    For i = 0 To ns
                        sum3 += F(i) * Hfj(i) - LSSj(i) * Hl(i) - VSSj(i) * Hv(i)
                    Next
                    val1 = sum3 - Q(ns)
                    sum4 = 0
                    For i = 0 To ns - 1
                        sum4 += Q(i) '- Lj(ns) * Hl(ns)
                    Next
                    B = -(val1 - (sum4 - Vj(0) * Hv(0))) / Hl(ns)
            End Select
            D2 = sumF - B - sumLSS - sumVSS - Vj(0)

            LSSj(0) = D2

            'step3

            'internal loop

            ic = 0
            Do

                Dim il_err As Double = 0.0#
                Dim il_err_ant As Double = 0.0#
                Dim num, denom, x0, fx0 As New ArrayList

                'step4

                'find component liquid flows by the tridiagonal matrix method

                Dim at(nc - 1)(), bt(nc - 1)(), ct(nc - 1)(), dt(nc - 1)(), xt(nc - 1)() As Double

                For i = 0 To nc - 1
                    Array.Resize(at(i), ns + 1)
                    Array.Resize(bt(i), ns + 1)
                    Array.Resize(ct(i), ns + 1)
                    Array.Resize(dt(i), ns + 1)
                    Array.Resize(xt(i), ns + 1)
                Next

                Dim sum1(ns), sum2(ns) As Double

                For i = 0 To ns
                    sum1(i) = 0
                    sum2(i) = 0
                    For j = 0 To i
                        sum1(i) += Fj(j) - LSSj(j) - VSSj(j)
                    Next
                    If i > 0 Then
                        For j = 0 To i - 1
                            sum2(i) += Fj(j) - LSSj(j) - VSSj(j)
                        Next
                    End If
                Next

                For i = 0 To nc - 1
                    For j = 0 To ns
                        dt(i)(j) = -Fj(j) * fcj(j)(i)
                        If j < ns Then
                            bt(i)(j) = -(Vj(j + 1) + sum1(j) - Vj(0) + LSSj(j) + (Vj(j) + VSSj(j)) * K(j)(i))
                        Else
                            bt(i)(j) = -(sum1(j) - Vj(0) + LSSj(j) + (Vj(j) + VSSj(j)) * K(j)(i))
                        End If
                        'tdma solve
                        If j < ns Then ct(i)(j) = Vj(j + 1) * K(j + 1)(i)
                        If j > 0 Then at(i)(j) = Vj(j) + sum2(j) - Vj(0)
                    Next
                Next

                'solve matrices

                'tomich
                If doparallel Then
                    My.Application.IsRunningParallelTasks = True
                    Dim t1 As Task = Task.Factory.StartNew(Sub() Parallel.For(0, nc, poptions,
                                                             Sub(ipar)
                                                                 xt(ipar) = Tomich.TDMASolve(at(ipar), bt(ipar), ct(ipar), dt(ipar))
                                                             End Sub))
                    While Not t1.IsCompleted
                        Application.DoEvents()
                    End While
                    My.Application.IsRunningParallelTasks = False
                Else
                    For i = 0 To nc - 1
                        xt(i) = Tomich.TDMASolve(at(i), bt(i), ct(i), dt(i))
                    Next
                End If

                Dim sumx(ns), sumy(ns) As Double

                For i = 0 To ns
                    sumx(i) = 0
                    For j = 0 To nc - 1
                        lc(i)(j) = xt(j)(i)
                        If lc(i)(j) < 0 Then lc(i)(j) = 0.0000001
                        sumx(i) += lc(i)(j)
                    Next
                Next

                For i = 0 To ns
                    For j = 0 To nc - 1
                        xc(i)(j) = lc(i)(j) / sumx(i)
                    Next
                Next

                For i = 0 To ns
                    Lj(i) = 0
                    For j = 0 To nc - 1
                        lc(i)(j) = xt(j)(i)
                        Lj(i) += lc(i)(j)
                    Next
                    If Lj(i) < 0 Then Lj(i) = 0.001
                Next

                Dim tmp As Object

                'calculate new temperatures

                For i = 0 To ns
                    Tj_ant(i) = Tj(i)
                Next

                If doparallel Then
                    My.Application.IsRunningParallelTasks = True
                    Dim t1 As Task = Task.Factory.StartNew(Sub() Parallel.For(0, ns + 1, poptions,
                                                             Sub(ipar)
                                                                 Dim tmpvar As Object = pp.DW_CalcBubT(xc(ipar), P(ipar), Tj(ipar), K(ipar), True)
                                                                 Tj(ipar) = tmpvar(4)
                                                                 K(ipar) = tmpvar(6)
                                                             End Sub))
                    While Not t1.IsCompleted
                        Application.DoEvents()
                    End While
                    My.Application.IsRunningParallelTasks = False
                Else
                    For i = 0 To ns
                        CheckCalculatorStatus()
                        tmp = pp.DW_CalcBubT(xc(i), P(i), Tj(i), K(i), True)
                        Tj(i) = tmp(4)
                        K(i) = tmp(6)
                        If Tj(i) < 0 Then Tj(i) = Tj_ant(i)
                    Next
                End If

                t_error_ant = t_error
                t_error = 0
                For i = 0 To ns
                    For j = 1 To nc
                        If Double.IsNaN(K(i)(j - 1)) Or Double.IsInfinity(K(i)(j - 1)) Then K(i)(j - 1) = pp.AUX_PVAPi(j - 1, Tj(i)) / P(i)
                    Next
                    t_error += Abs(Tj(i) - Tj_ant(i)) ^ 2
                Next

                For i = ns To 0 Step -1
                    sumy(i) = 0
                    For j = 0 To nc - 1
                        If i = ns Then
                            yc(i)(j) = K(i)(j) * xc(i)(j)
                        Else
                            yc(i)(j) = eff(i) * K(i)(j) * xc(i)(j) + (1 - eff(i)) * yc(i + 1)(j)
                        End If
                        sumy(i) += yc(i)(j)
                    Next
                Next

                For i = 0 To ns
                    For j = 0 To nc - 1
                        yc(i)(j) = yc(i)(j) / sumy(i)
                    Next
                Next

                ''''''''''''''''''''

                If doparallel Then
                    My.Application.IsRunningParallelTasks = True
                    Dim t1 As Task = Task.Factory.StartNew(Sub() Parallel.For(0, ns + 1, poptions,
                                                             Sub(ipar)
                                                                 Hl(ipar) = pp.DW_CalcEnthalpy(xc(ipar), Tj(ipar), P(ipar), PropertyPackages.State.Liquid) * pp.AUX_MMM(xc(ipar)) / 1000
                                                                 Hv(ipar) = pp.DW_CalcEnthalpy(yc(ipar), Tj(ipar), P(ipar), PropertyPackages.State.Vapor) * pp.AUX_MMM(yc(ipar)) / 1000
                                                             End Sub))
                    While Not t1.IsCompleted
                        Application.DoEvents()
                    End While
                    My.Application.IsRunningParallelTasks = False
                Else
                    For i = 0 To ns
                        CheckCalculatorStatus()
                        Hl(i) = pp.DW_CalcEnthalpy(xc(i), Tj(i), P(i), PropertyPackages.State.Liquid) * pp.AUX_MMM(xc(i)) / 1000
                        Hv(i) = pp.DW_CalcEnthalpy(yc(i), Tj(i), P(i), PropertyPackages.State.Vapor) * pp.AUX_MMM(yc(i)) / 1000
                    Next
                End If


                'handle specs

                Select Case specs("C").SType
                    Case ColumnSpec.SpecType.Product_Mass_Flow_Rate
                        LSSj(0) = spval1 / pp.AUX_MMM(xc(0)) * 1000
                        rr = (Lj(0) + LSSj(0)) / LSSj(0)
                    Case ColumnSpec.SpecType.Product_Molar_Flow_Rate
                        LSSj(0) = spval1
                        rr = (Lj(0) + LSSj(0)) / LSSj(0)
                    Case ColumnSpec.SpecType.Stream_Ratio
                        rr = spval1
                    Case ColumnSpec.SpecType.Heat_Duty
                        Q(0) = spval1
                        'Q(0) = (Vj(1) * Hv(1) + F(0) * Hfj(0) - (Lj(0) + LSSj(0)) * Hl(0) - (Vj(0) + VSSj(0)) * Hv(0))
                        LSSj(0) = -Lj(0) - (Q(0) - Vj(1) * Hv(1) - F(0) * Hfj(0) + (Vj(0) + VSSj(0)) * Hv(0)) / Hl(0)
                        rr = (Lj(0) + LSSj(0)) / LSSj(0)
                End Select

                Select Case specs("R").SType
                    Case ColumnSpec.SpecType.Product_Mass_Flow_Rate
                        B = spval2 / pp.AUX_MMM(xc(ns)) * 1000
                    Case ColumnSpec.SpecType.Product_Molar_Flow_Rate
                        B = spval2
                    Case ColumnSpec.SpecType.Stream_Ratio
                        B = Vj(ns) / spval2
                    Case ColumnSpec.SpecType.Heat_Duty
                        Q(ns) = spval2
                        Dim sum3, sum4, val1 As Double
                        sum3 = 0
                        sum4 = 0
                        For i = 0 To ns
                            sum3 += F(i) * Hfj(i) - LSSj(i) * Hl(i) - VSSj(i) * Hv(i)
                        Next
                        val1 = sum3 - Q(ns)
                        sum4 = 0
                        For i = 0 To ns - 1
                            sum4 += Q(i) '- Lj(ns) * Hl(ns)
                        Next
                        B = -(val1 - (sum4 - Vj(0) * Hv(0))) / Hl(ns)
                End Select

                sumF = 0
                sumLSS = 0
                sumVSS = 0
                For i = 0 To ns
                    sumF += F(i)
                    If i > 0 Then sumLSS += LSS(i)
                    sumVSS += VSS(i)
                Next

                LSSj(0) = sumF - B - sumLSS - sumVSS - Vj(0)

                'reboiler and condenser heat duties

                Dim alpha(ns), beta(ns), gamma(ns) As Double

                For i = 0 To ns
                    sum1(i) = 0
                    sum2(i) = 0
                    For j = 0 To i
                        sum1(i) += Fj(j) - LSSj(j) - VSSj(j)
                    Next
                    If i > 0 Then
                        For j = 0 To i - 1
                            sum2(i) += Fj(j) - LSSj(j) - VSSj(j)
                        Next
                    End If
                Next

                For j = 1 To ns
                    gamma(j) = (sum2(j) - Vj(0)) * (Hl(j) - Hl(j - 1)) + Fj(j) * (Hl(j) - Hfj(j)) + VSSj(j) * (Hv(j) - Hl(j)) + Q(j)
                    alpha(j) = Hl(j - 1) - Hv(j)
                    If j < ns Then beta(j) = Hv(j + 1) - Hl(j)
                Next

                'solve matrices

                For i = 0 To ns
                    Vj_ant(i) = Vj(i)
                Next

                Vj(0) = V(0)
                Vj(1) = (rr + 1) * LSSj(0) - Fj(0) + Vj(0)
                For i = 2 To ns
                    Vj(i) = (gamma(i - 1) - alpha(i - 1) * Vj(i - 1)) / beta(i - 1)
                    If Vj(i) < 0 Then Vj(i) = 0.000001
                Next

                'Ljs
                For i = 0 To ns
                    If i < ns Then Lj(i) = Vj(i + 1) + sum1(i) - Vj(0) Else Lj(i) = sum1(i) - Vj(0)
                Next

                'reboiler and condenser heat duties
                Select Case coltype
                    Case Column.ColType.DistillationColumn
                        If Not specs("C").SType = ColumnSpec.SpecType.Heat_Duty Then
                            Q(0) = (Vj(1) * Hv(1) + F(0) * Hfj(0) - (Lj(0) + LSSj(0)) * Hl(0) - (Vj(0) + VSSj(0)) * Hv(0))
                        End If
                        If Not specs("R").SType = ColumnSpec.SpecType.Heat_Duty Then
                            Dim sum3, sum4 As Double
                            sum3 = 0
                            sum4 = 0
                            For i = 0 To ns
                                sum3 += F(i) * Hfj(i) - LSSj(i) * Hl(i) - VSSj(i) * Hv(i)
                            Next
                            For i = 0 To ns - 1
                                sum4 += Q(i)
                            Next
                            Q(ns) = sum3 - sum4 - Vj(0) * Hv(0) - Lj(ns) * Hl(ns)
                        End If
                    Case Column.ColType.AbsorptionColumn
                        'use provided values
                    Case Column.ColType.RefluxedAbsorber
                        If Not specs("C").SType = ColumnSpec.SpecType.Heat_Duty Then
                            Q(0) = (Vj(1) * Hv(1) + F(0) * Hfj(0) - (Lj(0) + LSSj(0)) * Hl(0) - (Vj(0) + VSSj(0)) * Hv(0))
                        End If
                    Case Column.ColType.ReboiledAbsorber
                        If Not specs("R").SType = ColumnSpec.SpecType.Heat_Duty Then
                            Q(ns) = (Lj(ns - 1) * Hl(ns - 1) + F(ns) * Hfj(ns) - (Lj(ns) + LSSj(ns)) * Hl(ns) - (Vj(ns) + VSSj(ns)) * Hv(ns))
                        End If
                End Select

                ic = ic + 1

                If ic >= maxits Then Throw New Exception(DWSIM.App.GetLocalString("DCMaxIterationsReached"))
                If Double.IsNaN(t_error) Then Throw New Exception(DWSIM.App.GetLocalString("DCGeneralError"))
                If Abs((t_error - t_error_ant) / t_error) < tol(1) Then
                    My.Application.ActiveSimulation.WriteToLog("The column temperature profile isn't changing anymore. Final sum of temperature errors is " & t_error & " K.", Color.Green, FormClasses.TipoAviso.Aviso)
                    Exit Do
                End If
                If ic = stopatitnumber - 1 Then Exit Do

                CheckCalculatorStatus()

            Loop Until t_error < tol(1)

            ' finished, return arrays

            Return New Object() {Tj, Vj, Lj, VSSj, LSSj, yc, xc, K, Q, ic, t_error}

        End Function

    End Class

    <System.Serializable()> Public Class BurninghamOttoMethod

        Public Shared Function Solve(ByVal nc As Integer, ByVal ns As Integer, ByVal maxits As Integer, _
                                ByVal tol As Array, ByVal F As Array, ByVal V As Array, _
                                ByVal Q As Array, ByVal L As Array, _
                                ByVal VSS As Array, ByVal LSS As Array, ByVal Kval()() As Double, _
                                ByVal x()() As Double, ByVal y()() As Double, ByVal z()() As Double, _
                                ByVal fc()() As Double, _
                                ByVal HF As Array, ByVal T As Array, ByVal P As Array, _
                                ByVal stopatitnumber As Integer, _
                                ByVal eff() As Double, _
                                ByVal pp As PropertyPackages.PropertyPackage, _
                                ByVal specs As Dictionary(Of String, SepOps.ColumnSpec), _
                                Optional ByVal llextr As Boolean = False) As Object

            Dim doparallel As Boolean = My.Settings.EnableParallelProcessing
            Dim poptions As New ParallelOptions() With {.MaxDegreeOfParallelism = My.Settings.MaxDegreeOfParallelism}

            Dim ic As Integer
            Dim t_error As Double
            Dim Tj(ns), Tj_ant(ns) As Double
            Dim Fj(ns), Lj(ns), Vj(ns), Vj_ant(ns), xc(ns)(), fcj(ns)(), yc(ns)(), lc(ns)(), vc(ns)(), zc(ns)(), K(ns)() As Double
            Dim Hfj(ns), Hv(ns), Hl(ns) As Double
            Dim VSSj(ns), LSSj(ns) As Double
            Dim sum1(ns), sum2(ns), sum3(ns) As Double

            'step1

            'step2

            Dim i, j As Integer

            For i = 0 To ns
                Array.Resize(fcj(i), nc)
                Array.Resize(xc(i), nc)
                Array.Resize(yc(i), nc)
                Array.Resize(lc(i), nc)
                Array.Resize(vc(i), nc)
                Array.Resize(zc(i), nc)
                Array.Resize(zc(i), nc)
                Array.Resize(K(i), nc)
            Next

            For i = 0 To ns
                VSSj(i) = VSS(i)
                LSSj(i) = LSS(i)
                Lj(i) = L(i)
                Vj(i) = V(i)
                Tj(i) = T(i)
                K(i) = Kval(i)
                Fj(i) = F(i)
                Hfj(i) = HF(i) / 1000
                fcj(i) = fc(i)
            Next

            'step3

            'internal loop
            ic = 0
            Do

                Dim il_err As Double = 0.0#
                Dim il_err_ant As Double = 0.0#
                Dim num, denom, x0, fx0 As New ArrayList

                'step4

                'find component liquid flows by the tridiagonal matrix method

                Dim at(nc - 1)(), bt(nc - 1)(), ct(nc - 1)(), dt(nc - 1)(), xt(nc - 1)() As Double

                For i = 0 To nc - 1
                    Array.Resize(at(i), ns + 1)
                    Array.Resize(bt(i), ns + 1)
                    Array.Resize(ct(i), ns + 1)
                    Array.Resize(dt(i), ns + 1)
                    Array.Resize(xt(i), ns + 1)
                Next

                For i = 0 To ns
                    sum1(i) = 0
                    sum2(i) = 0
                    For j = 0 To i
                        sum1(i) += Fj(j) - LSSj(j) - VSSj(j)
                    Next
                    If i > 0 Then
                        For j = 0 To i - 1
                            sum2(i) += Fj(j) - LSSj(j) - VSSj(j)
                        Next
                    End If
                Next

                For i = 0 To nc - 1
                    For j = 0 To ns
                        dt(i)(j) = -Fj(j) * fcj(j)(i)
                        If j < ns Then
                            bt(i)(j) = -(Vj(j + 1) + sum1(j) - Vj(0) + LSSj(j) + (Vj(j) + VSSj(j)) * K(j)(i))
                        Else
                            bt(i)(j) = -(sum1(j) - Vj(0) + LSSj(j) + (Vj(j) + VSSj(j)) * K(j)(i))
                        End If
                        'tdma solve
                        If j < ns Then ct(i)(j) = Vj(j + 1) * K(j + 1)(i)
                        If j > 0 Then at(i)(j) = Vj(j) + sum2(j) - Vj(0)
                    Next
                Next

                'solve matrices

                'tomich
                For i = 0 To nc - 1
                    xt(i) = Tomich.TDMASolve(at(i), bt(i), ct(i), dt(i))
                Next

                Dim sumx(ns), sumy(ns), sumz(ns) As Double

                For i = 0 To ns
                    sumx(i) = 0
                    For j = 0 To nc - 1
                        lc(i)(j) = xt(j)(i)
                        If lc(i)(j) < 0 Then lc(i)(j) = 0.0000001
                        sumx(i) += lc(i)(j)
                    Next
                Next

                'Ljs
                For i = 0 To ns
                    'If sumx(i) > 1.5 Then
                    '    Lj(i) = Lj(i) * 1.5
                    'ElseIf sumx(i) < 0.5 Then
                    '    Lj(i) = Lj(i) * 0.5
                    'Else
                    Lj(i) = Lj(i) * sumx(i)
                    'End If
                Next

                For i = 0 To ns
                    For j = 0 To nc - 1
                        xc(i)(j) = lc(i)(j) / sumx(i)
                        yc(i)(j) = xc(i)(j) * K(i)(j)
                        sumy(i) += yc(i)(j)
                    Next
                Next

                For i = 0 To ns
                    For j = 0 To nc - 1
                        yc(i)(j) = yc(i)(j) / sumy(i)
                    Next
                Next

                For i = 0 To ns
                    sum3(i) = 0
                    For j = i To ns
                        sum3(i) += Fj(j) - LSSj(j) - VSSj(j)
                    Next
                Next

                'solve matrices

                For i = 0 To ns
                    Vj_ant(i) = Vj(i)
                Next

                For i = 0 To ns
                    If i > 0 Then
                        Vj(i) = Lj(i - 1) - Lj(ns) + sum3(i)
                    Else
                        Vj(i) = -Lj(ns) + sum3(i)
                    End If
                    'If Vj(i) < 0 Then Vj(i) = 0.01 '-Vj(i)
                Next

                For i = 0 To ns
                    sumz(i) = 0
                    For j = 0 To nc - 1
                        vc(i)(j) = xc(i)(j) * Vj(i) * K(i)(j)
                        zc(i)(j) = (lc(i)(j) + vc(i)(j)) / (Lj(i) + Vj(i))
                        sumz(i) += zc(i)(j)
                    Next
                Next

                For i = 0 To ns
                    For j = 0 To nc - 1
                        zc(i)(j) = zc(i)(j) / sumz(i)
                    Next
                Next

                'Dim tmp As Object

                'calculate new temperatures

                ''''''''''''''''''''
                Dim H(ns), dHldT(ns), dHvdT(ns), dHdTa(ns), dHdTb(ns), dHdTc(ns), dHl(ns), dHv(ns) As Double

                If doparallel Then
                    My.Application.IsRunningParallelTasks = True
                    Dim task1 As Task = Task.Factory.StartNew(Sub() Parallel.For(0, ns + 1, poptions,
                                                             Sub(ipar)
                                                                 Hl(ipar) = pp.DW_CalcEnthalpy(xc(ipar), Tj(ipar), P(ipar), PropertyPackages.State.Liquid) * pp.AUX_MMM(xc(ipar)) / 1000
                                                                 dHl(ipar) = pp.DW_CalcEnthalpy(xc(ipar), Tj(ipar) - 0.01, P(ipar), PropertyPackages.State.Liquid) * pp.AUX_MMM(xc(ipar)) / 1000
                                                                 If llextr Then
                                                                     Hv(ipar) = pp.DW_CalcEnthalpy(yc(ipar), Tj(ipar), P(ipar), PropertyPackages.State.Liquid) * pp.AUX_MMM(yc(ipar)) / 1000
                                                                     dHv(ipar) = pp.DW_CalcEnthalpy(yc(ipar), Tj(ipar) - 0.01, P(ipar), PropertyPackages.State.Liquid) * pp.AUX_MMM(yc(ipar)) / 1000
                                                                 Else
                                                                     Hv(ipar) = pp.DW_CalcEnthalpy(yc(ipar), Tj(ipar), P(ipar), PropertyPackages.State.Vapor) * pp.AUX_MMM(yc(ipar)) / 1000
                                                                     dHv(ipar) = pp.DW_CalcEnthalpy(yc(ipar), Tj(ipar) - 0.01, P(ipar), PropertyPackages.State.Vapor) * pp.AUX_MMM(yc(ipar)) / 1000
                                                                 End If
                                                             End Sub))
                    While Not task1.IsCompleted
                        Application.DoEvents()
                    End While
                    My.Application.IsRunningParallelTasks = False
                Else
                    For i = 0 To ns
                        Hl(i) = pp.DW_CalcEnthalpy(xc(i), Tj(i), P(i), PropertyPackages.State.Liquid) * pp.AUX_MMM(xc(i)) / 1000
                        dHl(i) = pp.DW_CalcEnthalpy(xc(i), Tj(i) - 0.01, P(i), PropertyPackages.State.Liquid) * pp.AUX_MMM(xc(i)) / 1000
                        If llextr Then
                            Hv(i) = pp.DW_CalcEnthalpy(yc(i), Tj(i), P(i), PropertyPackages.State.Liquid) * pp.AUX_MMM(yc(i)) / 1000
                            dHv(i) = pp.DW_CalcEnthalpy(yc(i), Tj(i) - 0.01, P(i), PropertyPackages.State.Liquid) * pp.AUX_MMM(yc(i)) / 1000
                        Else
                            Hv(i) = pp.DW_CalcEnthalpy(yc(i), Tj(i), P(i), PropertyPackages.State.Vapor) * pp.AUX_MMM(yc(i)) / 1000
                            dHv(i) = pp.DW_CalcEnthalpy(yc(i), Tj(i) - 0.01, P(i), PropertyPackages.State.Vapor) * pp.AUX_MMM(yc(i)) / 1000
                        End If
                        CheckCalculatorStatus()
                    Next
                End If

                For i = 0 To ns
                    If i = 0 Then
                        H(i) = Vj(i + 1) * Hv(i + 1) + Fj(i) * Hfj(i) - (Lj(i) + LSSj(i)) * Hl(i) - (Vj(i) + VSSj(i)) * Hv(i) - Q(i)
                    ElseIf i = ns Then
                        H(i) = Lj(i - 1) * Hl(i - 1) + Fj(i) * Hfj(i) - (Lj(i) + LSSj(i)) * Hl(i) - (Vj(i) + VSSj(i)) * Hv(i) - Q(i)
                    Else
                        H(i) = Lj(i - 1) * Hl(i - 1) + Vj(i + 1) * Hv(i + 1) + Fj(i) * Hfj(i) - (Lj(i) + LSSj(i)) * Hl(i) - (Vj(i) + VSSj(i)) * Hv(i) - Q(i)
                    End If
                    dHldT(i) = (Hl(i) - dHl(i)) / 0.01
                    dHvdT(i) = (Hv(i) - dHv(i)) / 0.01
                Next

                For i = 0 To ns
                    If i > 0 Then dHdTa(i) = Lj(i - 1) * dHldT(i - 1)
                    dHdTb(i) = -(Lj(i) + LSSj(i)) * dHldT(i) - (Vj(i) + VSSj(i)) * dHvdT(i)
                    If i < ns Then dHdTc(i) = Vj(i + 1) * dHvdT(i + 1)
                Next

                Dim ath(ns), bth(ns), cth(ns), dth(ns), xth(ns) As Double

                For i = 0 To ns
                    dth(i) = -H(i)
                    bth(i) = dHdTb(i)
                    If i < ns Then cth(i) = dHdTc(i)
                    If i > 0 Then ath(i) = dHdTa(i)
                Next

                'solve matrices
                'tomich

                xth = Tomich.TDMASolve(ath, bth, cth, dth)

                Dim tmp As Object

                t_error = 0
                For i = 0 To ns
                    Tj_ant(i) = Tj(i)
                    Tj(i) = Tj(i) + xth(i)
                    If Tj(i) < 0 Then Tj(i) = Tj_ant(i)
                    tmp = pp.DW_CalcKvalue(xc(i), yc(i), Tj(i), P(i), "LL")
                    sumy(i) = 0
                    For j = 0 To nc - 1
                        K(i)(j) = tmp(j)
                        If Double.IsNaN(K(i)(j)) Or Double.IsInfinity(K(i)(j)) Then
                            If llextr Then
                                K(i)(j) = 1.0#
                            Else
                                K(i)(j) = pp.AUX_PVAPi(j, Tj(i)) / P(i)
                            End If
                        End If
                        yc(i)(j) = K(i)(j) * xc(i)(j)
                        sumy(i) += yc(i)(j)
                    Next
                    t_error += Abs(Tj(i) - Tj_ant(i)) ^ 2
                    CheckCalculatorStatus()
                Next

                For i = 0 To ns
                    For j = 0 To nc - 1
                        yc(i)(j) = yc(i)(j) / sumy(i)
                    Next
                Next

                ic = ic + 1

                If ic >= maxits Then Throw New Exception(DWSIM.App.GetLocalString("DCMaxIterationsReached"))
                If Double.IsNaN(t_error) Then Throw New Exception(DWSIM.App.GetLocalString("DCGeneralError"))

                CheckCalculatorStatus()

            Loop Until t_error <= tol(1)

            ' finished, return arrays

            Return New Object() {Tj, Vj, Lj, VSSj, LSSj, yc, xc, K, Q, ic, t_error}

        End Function

    End Class

    <System.Serializable()> Public Class NaphtaliSandholmMethod

        Sub New()

        End Sub

        Dim ndeps As Double = 0.1

        Dim _nc, _ns As Integer
        Dim _VSS, _LSS As Double()
        Dim _spval1, _spval2 As Double
        Dim _spci1, _spci2 As Integer
        Dim _eff, _F, _Q, _P, _HF As Double()
        Dim _fc()(), _maxF As Double
        Dim _pp As PropertyPackages.PropertyPackage
        Dim _coltype As Column.ColType
        Dim _specs As Dictionary(Of String, SepOps.ColumnSpec)
        Dim _bx, _dbx As Double()
        Dim _condtype As DistillationColumn.condtype
        Dim llextr As Boolean = False
        Dim _Kval()() As Double

        Public Function FunctionValue(ByVal x() As Double) As Double()

            Dim doparallel As Boolean = My.Settings.EnableParallelProcessing
            Dim poptions As New ParallelOptions() With {.MaxDegreeOfParallelism = My.Settings.MaxDegreeOfParallelism}

            Dim nc, ns As Integer
            Dim i, j As Integer
            Dim VSS, LSS, F, Q, P, HF, eff As Double()
            Dim fc()() As Double
            Dim spval1, spval2, spfval1, spfval2, maxF As Double
            Dim spci1, spci2 As Integer
            Dim coltype As Column.ColType = _coltype

            F = _F
            Q = _Q
            P = _P
            HF = _HF
            eff = _eff
            fc = _fc
            maxF = _maxF

            spval1 = _spval1
            spval2 = _spval2
            spci1 = _spci1
            spci2 = _spci2

            VSS = _VSS
            LSS = _LSS

            nc = _nc
            ns = _ns

            Dim Sl(ns), Sv(ns) As Double
            Dim sumF As Double = 0
            Dim sumLSS As Double = 0
            Dim sumVSS As Double = 0
            Dim Tj(ns), vc(ns)(), lc(ns)(), zc(ns)(), Vj(ns), Lj(ns), xc(ns)(), yc(ns)(), Kval(ns)() As Double

            For i = 0 To ns
                Array.Resize(lc(i), nc)
                Array.Resize(vc(i), nc)
                Array.Resize(zc(i), nc)
                Array.Resize(xc(i), nc)
                Array.Resize(yc(i), nc)
                Array.Resize(Kval(i), nc)
            Next

            For i = 0 To ns
                Tj(i) = x(i * (2 * nc + 1))
                For j = 0 To nc - 1
                    vc(i)(j) = x(i * (2 * nc + 1) + j + 1)
                    lc(i)(j) = x(i * (2 * nc + 1) + j + 1 + nc)
                Next
            Next

            Dim VSSj(ns), LSSj(ns), Hv(ns), Hl(ns), Hv0(ns), Hl0(ns) As Double
            Dim sumvkj(ns), sumlkj(ns) As Double

            Dim M(ns, nc - 1), E(ns, nc - 1), H(ns) As Double
            Dim M_ant(ns, nc - 1), E_ant(ns, nc - 1), H_ant(ns) As Double

            For i = 0 To ns
                VSSj(i) = VSS(i)
                If i > 0 Then LSSj(i) = LSS(i)
            Next

            For i = 0 To ns
                sumvkj(i) = 0
                sumlkj(i) = 0
                For j = 0 To nc - 1
                    sumvkj(i) += vc(i)(j)
                    sumlkj(i) += lc(i)(j)
                Next
                Vj(i) = sumvkj(i)
                Lj(i) = sumlkj(i)
            Next

            For i = 0 To ns
                For j = 0 To nc - 1
                    xc(i)(j) = lc(i)(j) / sumlkj(i)
                Next
                For j = 0 To nc - 1
                    yc(i)(j) = vc(i)(j) / sumvkj(i)
                Next
            Next

            For i = 0 To ns
                For j = 0 To nc - 1
                    zc(i)(j) = (lc(i)(j) + vc(i)(j))
                Next
            Next

            sumF = 0
            sumLSS = 0
            sumVSS = 0
            For i = 0 To ns
                sumVSS += VSSj(i)
            Next
            For i = 1 To ns
                sumLSS += LSSj(i)
            Next
            LSSj(0) = 1 - Lj(ns) - sumLSS - sumVSS - Vj(0)

            For i = 0 To ns
                If Vj(i) <> 0 Then Sv(i) = VSSj(i) / Vj(i) Else Sv(i) = 0
                If Lj(i) <> 0 Then Sl(i) = LSSj(i) / Lj(i) Else Sl(i) = 0
            Next

            'calculate K-values

            If doparallel Then
                My.Application.IsRunningParallelTasks = True
                Dim task1 As Task = Task.Factory.StartNew(Sub() Parallel.For(0, ns + 1, poptions,
                                                         Sub(ipar)
                                                             Dim tmp0 As Object
                                                             If llextr Then
                                                                 tmp0 = _pp.DW_CalcKvalue(xc(ipar), yc(ipar), Tj(ipar), P(ipar), "LL")
                                                             Else
                                                                 tmp0 = _pp.DW_CalcKvalue(xc(ipar), yc(ipar), Tj(ipar), P(ipar))
                                                             End If
                                                             Dim jj As Integer
                                                             For jj = 0 To nc - 1
                                                                 Kval(ipar)(jj) = tmp0(jj)
                                                             Next
                                                         End Sub))
                While Not task1.IsCompleted
                    Application.DoEvents()
                End While
                My.Application.IsRunningParallelTasks = False
            Else
                Dim tmp0 As Object
                For i = 0 To ns
                    If llextr Then
                        tmp0 = _pp.DW_CalcKvalue(xc(i), yc(i), Tj(i), P(i), "LL")
                    Else
                        tmp0 = _pp.DW_CalcKvalue(xc(i), yc(i), Tj(i), P(i))
                    End If
                    For j = 0 To nc - 1
                        Kval(i)(j) = tmp0(j)
                    Next
                    CheckCalculatorStatus()
                Next
            End If

            _Kval = Kval

            'calculate enthalpies

            If doparallel Then
                My.Application.IsRunningParallelTasks = True
                Dim task1 As Task = Task.Factory.StartNew(Sub() Parallel.For(0, ns + 1, poptions,
                                                         Sub(ipar)
                                                             If Vj(ipar) <> 0 Then
                                                                 If llextr Then
                                                                     Hv(ipar) = _pp.DW_CalcEnthalpy(yc(ipar), Tj(ipar), P(ipar), PropertyPackages.State.Liquid) * _pp.AUX_MMM(yc(ipar)) / 1000
                                                                 Else
                                                                     Hv(ipar) = _pp.DW_CalcEnthalpy(yc(ipar), Tj(ipar), P(ipar), PropertyPackages.State.Vapor) * _pp.AUX_MMM(yc(ipar)) / 1000
                                                                 End If
                                                             Else
                                                                 Hv(ipar) = 0
                                                             End If
                                                             If Lj(ipar) <> 0 Then
                                                                 Hl(ipar) = _pp.DW_CalcEnthalpy(xc(ipar), Tj(ipar), P(ipar), PropertyPackages.State.Liquid) * _pp.AUX_MMM(xc(ipar)) / 1000
                                                             Else
                                                                 Hl(ipar) = 0
                                                             End If
                                                         End Sub))
                While Not task1.IsCompleted
                    Application.DoEvents()
                End While
                My.Application.IsRunningParallelTasks = False
            Else
                For i = 0 To ns
                    If Vj(i) <> 0 Then
                        If llextr Then
                            Hv(i) = _pp.DW_CalcEnthalpy(yc(i), Tj(i), P(i), PropertyPackages.State.Liquid) * _pp.AUX_MMM(yc(i)) / 1000
                        Else
                            Hv(i) = _pp.DW_CalcEnthalpy(yc(i), Tj(i), P(i), PropertyPackages.State.Vapor) * _pp.AUX_MMM(yc(i)) / 1000
                        End If
                    Else
                        Hv(i) = 0
                    End If
                    If Lj(i) <> 0 Then
                        Hl(i) = _pp.DW_CalcEnthalpy(xc(i), Tj(i), P(i), PropertyPackages.State.Liquid) * _pp.AUX_MMM(xc(i)) / 1000
                    Else
                        Hl(i) = 0
                    End If
                    CheckCalculatorStatus()
                Next
            End If

            'reboiler and condenser heat duties

            Select Case coltype
                Case Column.ColType.DistillationColumn
                    If Not _specs("C").SType = ColumnSpec.SpecType.Heat_Duty Then
                        i = 0
                        Q(0) = (Hl(i) * (1 + Sl(i)) * sumlkj(i) + Hv(i) * (1 + Sv(i)) * sumvkj(i) - Hv(i + 1) * sumvkj(i + 1) - HF(i) * F(i))
                        Q(0) = -Q(0)
                    End If
                    If Not _specs("R").SType = ColumnSpec.SpecType.Heat_Duty Then
                        i = ns
                        Q(ns) = (Hl(i) * (1 + Sl(i)) * sumlkj(i) + Hv(i) * (1 + Sv(i)) * sumvkj(i) - Hl(i - 1) * sumlkj(i - 1) - HF(i) * F(i))
                        Q(ns) = -Q(ns)
                    End If
                Case Column.ColType.AbsorptionColumn
                    'use provided values
                Case Column.ColType.RefluxedAbsorber
                    If Not _specs("C").SType = ColumnSpec.SpecType.Heat_Duty Then
                        i = 0
                        Q(0) = (Hl(i) * (1 + Sl(i)) * sumlkj(i) + Hv(i) * (1 + Sv(i)) * sumvkj(i) - Hv(i + 1) * sumvkj(i + 1) - HF(i) * F(i))
                        Q(0) = -Q(0)
                    End If
                Case Column.ColType.ReboiledAbsorber
                    If Not _specs("R").SType = ColumnSpec.SpecType.Heat_Duty Then
                        i = ns
                        Q(ns) = (Hl(i) * (1 + Sl(i)) * sumlkj(i) + Hv(i) * (1 + Sv(i)) * sumvkj(i) - Hl(i - 1) * sumlkj(i - 1) - HF(i) * F(i))
                        Q(ns) = -Q(ns)
                    End If
            End Select

            'handle user specs

            Select Case _specs("C").SType
                Case ColumnSpec.SpecType.Component_Fraction
                    If _specs("C").SpecUnit = "M" Then
                        spfval1 = xc(0)(spci1) - spval1
                    Else 'W
                        spfval1 = _pp.AUX_CONVERT_MOL_TO_MASS(xc(0))(spci1) - spval1
                    End If
                Case ColumnSpec.SpecType.Component_Mass_Flow_Rate
                    spfval1 = LSSj(0) * xc(0)(spci1) - spval1 / _pp.RET_VMM()(spci1) * 1000 / maxF
                Case ColumnSpec.SpecType.Component_Molar_Flow_Rate
                    spfval1 = LSSj(0) * xc(0)(spci1) - spval1 / maxF
                Case ColumnSpec.SpecType.Component_Recovery
                    Dim rec As Double = spval1 / 100
                    Dim sumc As Double = 0
                    For j = 0 To ns
                        sumc += fc(j)(spci1)
                    Next
                    sumc *= rec
                    If _specs("C").SpecUnit = "% M/M" Then
                        spfval1 = xc(0)(spci1) * LSSj(0) - sumc
                    Else '% W/W
                        spfval1 = _pp.RET_VMM()(spci1) * 1000 * (xc(0)(spci1) * LSSj(0) - sumc)
                    End If
                Case ColumnSpec.SpecType.Heat_Duty
                    Q(0) = spval1 / maxF
                Case ColumnSpec.SpecType.Product_Mass_Flow_Rate
                    spfval1 = LSSj(0) - spval1 / _pp.AUX_MMM(xc(0)) * 1000 / maxF
                Case ColumnSpec.SpecType.Product_Molar_Flow_Rate
                    spfval1 = LSSj(0) - spval1 / maxF
                Case ColumnSpec.SpecType.Stream_Ratio
                    spfval1 = Lj(0) / LSSj(0) - spval1
                Case ColumnSpec.SpecType.Temperature
                    spfval1 = Tj(0) - spval1
            End Select

            Select Case _specs("R").SType
                Case ColumnSpec.SpecType.Component_Fraction
                    If _specs("R").SpecUnit = "M" Then
                        spfval2 = xc(ns)(spci2) - spval2
                    Else 'W
                        spfval2 = _pp.AUX_CONVERT_MOL_TO_MASS(xc(ns))(spci2) - spval2
                    End If
                Case ColumnSpec.SpecType.Component_Mass_Flow_Rate
                    spfval2 = Lj(ns) * xc(ns)(spci2) - spval2 / _pp.RET_VMM()(spci2) * 1000 / maxF
                Case ColumnSpec.SpecType.Component_Molar_Flow_Rate
                    spfval2 = Lj(ns) * xc(ns)(spci2) - spval2 / maxF
                Case ColumnSpec.SpecType.Component_Recovery
                    Dim rec As Double = spval2 / 100
                    Dim sumc As Double = 0
                    For j = 0 To ns
                        sumc += fc(j)(spci2)
                    Next
                    sumc *= rec
                    If _specs("R").SpecUnit = "% M/M" Then
                        spfval2 = lc(ns)(spci2) - sumc
                    Else '% W/W
                        spfval2 = _pp.RET_VMM()(spci2) * 1000 * (xc(0)(spci1) * Lj(ns) - sumc)
                    End If
                Case ColumnSpec.SpecType.Heat_Duty
                    Q(ns) = spval2 / maxF
                Case ColumnSpec.SpecType.Product_Mass_Flow_Rate
                    spfval2 = Lj(ns) - spval2 / _pp.AUX_MMM(xc(ns)) * 1000 / maxF
                Case ColumnSpec.SpecType.Product_Molar_Flow_Rate
                    spfval2 = Lj(ns) - spval2 / maxF
                Case ColumnSpec.SpecType.Stream_Ratio
                    spfval2 = Vj(ns) / Lj(ns) - spval2
                Case ColumnSpec.SpecType.Temperature
                    spfval2 = Tj(ns) - spval2
            End Select

            For i = 0 To ns
                For j = 0 To nc - 1
                    M_ant(i, j) = M(i, j)
                    E_ant(i, j) = E(i, j)
                    H_ant(i) = H(i)
                    If i = 0 Then
                        M(i, j) = lc(i)(j) * (1 + Sl(i)) + vc(i)(j) * (1 + Sv(i)) - vc(i + 1)(j) - fc(i)(j)
                        E(i, j) = eff(i) * Kval(i)(j) * lc(i)(j) * sumvkj(i) / sumlkj(i) - vc(i)(j) + (1 - eff(i)) * vc(i + 1)(j) * sumvkj(i) / sumvkj(i + 1)
                    ElseIf i = ns Then
                        M(i, j) = lc(i)(j) * (1 + Sl(i)) + vc(i)(j) * (1 + Sv(i)) - lc(i - 1)(j) - fc(i)(j)
                        E(i, j) = eff(i) * Kval(i)(j) * lc(i)(j) * sumvkj(i) / sumlkj(i) - vc(i)(j)
                    Else
                        M(i, j) = lc(i)(j) * (1 + Sl(i)) + vc(i)(j) * (1 + Sv(i)) - lc(i - 1)(j) - vc(i + 1)(j) - fc(i)(j)
                        E(i, j) = eff(i) * Kval(i)(j) * lc(i)(j) * sumvkj(i) / sumlkj(i) - vc(i)(j) + (1 - eff(i)) * vc(i + 1)(j) * sumvkj(i) / sumvkj(i + 1)
                    End If
                Next
                If i = 0 Then
                    H(i) = (Hl(i) * (1 + Sl(i)) * sumlkj(i) + Hv(i) * (1 + Sv(i)) * sumvkj(i) - Hv(i + 1) * sumvkj(i + 1) - HF(i) * F(i) - Q(i)) '/ (Hv(i) - Hl(i))
                ElseIf i = ns Then
                    H(i) = (Hl(i) * (1 + Sl(i)) * sumlkj(i) + Hv(i) * (1 + Sv(i)) * sumvkj(i) - Hl(i - 1) * sumlkj(i - 1) - HF(i) * F(i) - Q(i)) '/ (Hv(i) - Hl(i))
                Else
                    H(i) = (Hl(i) * (1 + Sl(i)) * sumlkj(i) + Hv(i) * (1 + Sv(i)) * sumvkj(i) - Hl(i - 1) * sumlkj(i - 1) - Hv(i + 1) * sumvkj(i + 1) - HF(i) * F(i) - Q(i)) '/ (Hv(i) - Hl(i))
                End If
                H(i) /= (Hv(i) - Hl(i))
                Select Case coltype
                    Case Column.ColType.DistillationColumn
                        H(0) = spfval1
                        H(ns) = spfval2
                    Case Column.ColType.AbsorptionColumn
                        'do nothing
                    Case Column.ColType.ReboiledAbsorber
                        H(ns) = spfval2
                    Case Column.ColType.RefluxedAbsorber
                        H(0) = spfval1
                End Select
            Next

            If _condtype = Column.condtype.Total_Condenser Then
                Dim sum1 As Double = 0
                For j = 0 To nc - 1
                    sum1 += Kval(0)(j) * xc(0)(j)
                Next
                i = 0
                For j = 0 To nc - 1
                    If j = 0 Then
                        E(i, j) = 1 - sum1
                    Else
                        E(i, j) = xc(i)(j) - yc(i)(j)
                    End If
                Next
            End If

            Dim errors(x.Length - 1) As Double

            For i = 0 To ns
                errors(i * (2 * nc + 1)) = H(i)
                For j = 0 To nc - 1
                    errors(i * (2 * nc + 1) + j + 1) = M(i, j)
                    errors(i * (2 * nc + 1) + j + 1 + nc) = E(i, j)
                Next
            Next

            Return errors

        End Function

        Public Function MinimizeError(ByVal t As Double) As Double

            Dim nc, ns As Integer
            Dim i, j As Integer
            Dim VSS, LSS, F, Q, P, HF, eff As Double()
            Dim fc()() As Double
            Dim spval1, spval2, spfval1, spfval2, maxF As Double
            Dim spci1, spci2 As Integer
            Dim coltype As Column.ColType = _coltype

            F = _F
            Q = _Q
            P = _P
            HF = _HF
            eff = _eff
            fc = _fc
            maxF = _maxF

            spval1 = _spval1
            spval2 = _spval2
            spci1 = _spci1
            spci2 = _spci2

            VSS = _VSS
            LSS = _LSS

            nc = _nc
            ns = _ns

            Dim Sl(ns), Sv(ns) As Double
            Dim sumF As Double = 0
            Dim sumLSS As Double = 0
            Dim sumVSS As Double = 0
            Dim Tj(ns), vc(ns)(), lc(ns)(), zc(ns)(), Vj(ns), Lj(ns), xc(ns)(), yc(ns)(), Kval(ns)() As Double

            For i = 0 To ns
                Array.Resize(lc(i), nc)
                Array.Resize(vc(i), nc)
                Array.Resize(zc(i), nc)
                Array.Resize(xc(i), nc)
                Array.Resize(yc(i), nc)
                Array.Resize(Kval(i), nc)
            Next

            For i = 0 To ns
                If _bx(i * (2 * nc + 1)) + t * _dbx(i * (2 * nc + 1)) < 0 Then
                    Tj(i) = _bx(i * (2 * nc + 1)) * Exp(t * _dbx(i * (2 * nc + 1))) / _bx(i * (2 * nc + 1))
                Else
                    Tj(i) = _bx(i * (2 * nc + 1)) + t * _dbx(i * (2 * nc + 1))
                End If
                For j = 0 To nc - 1
                    If _bx(i * (2 * nc + 1) + j + 1) + t * _dbx(i * (2 * nc + 1) + j + 1) < 0 Then
                        vc(i)(j) = _bx(i * (2 * nc + 1) + j + 1) * Exp(t * _dbx(i * (2 * nc + 1) + j + 1) / _bx(i * (2 * nc + 1) + j + 1))
                    Else
                        vc(i)(j) = _bx(i * (2 * nc + 1) + j + 1) + t * _dbx(i * (2 * nc + 1) + j + 1)
                    End If
                    If _bx(i * (2 * nc + 1) + j + 1 + nc) + t * _dbx(i * (2 * nc + 1) + j + 1 + nc) < 0 Then
                        lc(i)(j) = _bx(i * (2 * nc + 1) + j + 1 + nc) * Exp(t * _dbx(i * (2 * nc + 1) + j + 1 + nc) / _bx(i * (2 * nc + 1) + j + 1 + nc))
                    Else
                        lc(i)(j) = _bx(i * (2 * nc + 1) + j + 1 + nc) + t * _dbx(i * (2 * nc + 1) + j + 1 + nc)
                    End If
                Next
            Next

            Dim VSSj(ns), LSSj(ns), Hv(ns), Hl(ns), Hv0(ns), Hl0(ns) As Double
            Dim sumvkj(ns), sumlkj(ns) As Double
            Dim Fxvar((ns + 1) * (2 * nc + 1) - 1) As Double
            Dim xvar((ns + 1) * (2 * nc + 1) - 1) As Double
            Dim dFdXvar((ns + 1) * (2 * nc + 1) - 1, (ns + 1) * (2 * nc + 1) - 1) As Double

            Dim f1((ns + 1) * (2 * nc + 1) - 1), f2((ns + 1) * (2 * nc + 1) - 1), f3((ns + 1) * (2 * nc + 1) - 1), f4((ns + 1) * (2 * nc + 1) - 1) As Double
            Dim u As Integer = 0

            Dim hesm As New Mapack.Matrix((ns + 1) * (2 * nc + 1), (ns + 1) * (2 * nc + 1))
            Dim dX As New Mapack.Matrix((ns + 1) * (2 * nc + 1), 1)
            Dim fX As New Mapack.Matrix((ns + 1) * (2 * nc + 1), 1)
            Dim inv As New Mapack.Matrix((ns + 1) * (2 * nc + 1), (ns + 1) * (2 * nc + 1))
            Dim num, denom, x0, fx0 As New ArrayList

            Dim hes((ns + 1) * (2 * nc + 1) - 1, (ns + 1) * (2 * nc + 1) - 1) As Double
            Dim bx((ns + 1) * (2 * nc + 1) - 1), bx_ant((ns + 1) * (2 * nc + 1) - 1), bxb((ns + 1) * (2 * nc + 1) - 1), bf((ns + 1) * (2 * nc + 1) - 1), bfb((ns + 1) * (2 * nc + 1) - 1), bp((ns + 1) * (2 * nc + 1) - 1), bp_ant((ns + 1) * (2 * nc + 1) - 1) As Double
            Dim M(ns, nc - 1), E(ns, nc - 1), H(ns) As Double
            Dim M_ant(ns, nc - 1), E_ant(ns, nc - 1), H_ant(ns) As Double

            For i = 0 To ns
                VSSj(i) = VSS(i)
                If i > 0 Then LSSj(i) = LSS(i)
            Next

            For i = 0 To ns
                sumvkj(i) = 0
                sumlkj(i) = 0
                For j = 0 To nc - 1
                    sumvkj(i) += vc(i)(j)
                    sumlkj(i) += lc(i)(j)
                Next
                Vj(i) = sumvkj(i)
                Lj(i) = sumlkj(i)
            Next

            For i = 0 To ns
                For j = 0 To nc - 1
                    xc(i)(j) = lc(i)(j) / sumlkj(i)
                Next
                For j = 0 To nc - 1
                    yc(i)(j) = vc(i)(j) / sumvkj(i)
                Next
            Next

            For i = 0 To ns
                For j = 0 To nc - 1
                    zc(i)(j) = (lc(i)(j) + vc(i)(j))
                Next
            Next

            sumF = 0
            sumLSS = 0
            For i = 0 To ns
                sumVSS += VSSj(i)
            Next
            For i = 1 To ns
                sumLSS += LSSj(i)
            Next
            LSSj(0) = 1 - Lj(ns) - sumLSS - sumVSS - Vj(0)

            For i = 0 To ns
                If Vj(i) <> 0 Then Sv(i) = VSSj(i) / Vj(i) Else Sv(i) = 0
                If Lj(i) <> 0 Then Sl(i) = LSSj(i) / Lj(i) Else Sl(i) = 0
            Next

            'calculate K-values
            Dim tmp0 As Object

            For i = 0 To ns
                If llextr Then
                    tmp0 = _pp.DW_CalcKvalue(xc(i), yc(i), Tj(i), P(i), "LL")
                Else
                    tmp0 = _pp.DW_CalcKvalue(xc(i), yc(i), Tj(i), P(i))
                End If
                For j = 0 To nc - 1
                    Kval(i)(j) = tmp0(j)
                Next
                CheckCalculatorStatus()
            Next

            'calculate enthalpies

            For i = 0 To ns
                If Vj(i) <> 0 Then
                    If llextr Then
                        Hv(i) = _pp.DW_CalcEnthalpy(yc(i), Tj(i), P(i), PropertyPackages.State.Liquid) * _pp.AUX_MMM(yc(i)) / 1000
                    Else
                        Hv(i) = _pp.DW_CalcEnthalpy(yc(i), Tj(i), P(i), PropertyPackages.State.Vapor) * _pp.AUX_MMM(yc(i)) / 1000
                    End If
                Else
                    Hv(i) = 0
                End If
                If Lj(i) <> 0 Then
                    Hl(i) = _pp.DW_CalcEnthalpy(xc(i), Tj(i), P(i), PropertyPackages.State.Liquid) * _pp.AUX_MMM(xc(i)) / 1000
                Else
                    Hl(i) = 0
                End If
                CheckCalculatorStatus()
            Next

            'reboiler and condenser heat duties

            Select Case coltype
                Case Column.ColType.DistillationColumn
                    If Not _specs("C").SType = ColumnSpec.SpecType.Heat_Duty Then
                        i = 0
                        Q(0) = (Hl(i) * (1 + Sl(i)) * sumlkj(i) + Hv(i) * (1 + Sv(i)) * sumvkj(i) - Hv(i + 1) * sumvkj(i + 1) - HF(i) * F(i))
                        Q(0) = -Q(0)
                    End If
                    If Not _specs("R").SType = ColumnSpec.SpecType.Heat_Duty Then
                        i = ns
                        Q(ns) = (Hl(i) * (1 + Sl(i)) * sumlkj(i) + Hv(i) * (1 + Sv(i)) * sumvkj(i) - Hl(i - 1) * sumlkj(i - 1) - HF(i) * F(i))
                        Q(ns) = -Q(ns)
                    End If
                Case Column.ColType.AbsorptionColumn
                    'use provided values
                Case Column.ColType.RefluxedAbsorber
                    If Not _specs("C").SType = ColumnSpec.SpecType.Heat_Duty Then
                        i = 0
                        Q(0) = (Hl(i) * (1 + Sl(i)) * sumlkj(i) + Hv(i) * (1 + Sv(i)) * sumvkj(i) - Hv(i + 1) * sumvkj(i + 1) - HF(i) * F(i))
                        Q(0) = -Q(0)
                    End If
                Case Column.ColType.ReboiledAbsorber
                    If Not _specs("R").SType = ColumnSpec.SpecType.Heat_Duty Then
                        i = ns
                        Q(ns) = (Hl(i) * (1 + Sl(i)) * sumlkj(i) + Hv(i) * (1 + Sv(i)) * sumvkj(i) - Hl(i - 1) * sumlkj(i - 1) - HF(i) * F(i))
                        Q(ns) = -Q(ns)
                    End If
            End Select

            'handle user specs

            Select Case _specs("C").SType
                Case ColumnSpec.SpecType.Component_Fraction
                    If _specs("C").SpecUnit = "M" Then
                        spfval1 = xc(0)(spci1) - spval1
                    Else 'W
                        spfval1 = _pp.AUX_CONVERT_MOL_TO_MASS(xc(0))(spci1) - spval1
                    End If
                Case ColumnSpec.SpecType.Component_Mass_Flow_Rate
                    spfval1 = LSSj(0) * xc(0)(spci1) - spval1 / _pp.RET_VMM()(spci1) * 1000 / maxF
                Case ColumnSpec.SpecType.Component_Molar_Flow_Rate
                    spfval1 = LSSj(0) * xc(0)(spci1) - spval1 / maxF
                Case ColumnSpec.SpecType.Component_Recovery
                    Dim rec As Double = spval1 / 100
                    Dim sumc As Double = 0
                    For j = 0 To ns
                        sumc += fc(j)(spci1)
                    Next
                    sumc *= rec
                    If _specs("C").SpecUnit = "% M/M" Then
                        spfval1 = xc(0)(spci1) * LSSj(0) - sumc
                    Else '% W/W
                        spfval1 = _pp.RET_VMM()(spci1) * 1000 * (xc(0)(spci1) * LSSj(0) - sumc)
                    End If
                Case ColumnSpec.SpecType.Heat_Duty
                    Q(0) = spval1 / maxF
                Case ColumnSpec.SpecType.Product_Mass_Flow_Rate
                    spfval1 = LSSj(0) - spval1 / _pp.AUX_MMM(xc(0)) * 1000 / maxF
                Case ColumnSpec.SpecType.Product_Molar_Flow_Rate
                    spfval1 = LSSj(0) - spval1 / maxF
                Case ColumnSpec.SpecType.Stream_Ratio
                    spfval1 = Lj(0) - spval1 * (LSSj(0))
                Case ColumnSpec.SpecType.Temperature
                    spfval1 = Tj(0) - spval1
            End Select

            Select Case _specs("R").SType
                Case ColumnSpec.SpecType.Component_Fraction
                    If _specs("R").SpecUnit = "M" Then
                        spfval2 = xc(ns)(spci2) - spval2
                    Else 'W
                        spfval2 = _pp.AUX_CONVERT_MOL_TO_MASS(xc(ns))(spci2) - spval2
                    End If
                Case ColumnSpec.SpecType.Component_Mass_Flow_Rate
                    spfval2 = Lj(ns) * xc(ns)(spci2) - spval2 / _pp.RET_VMM()(spci2) * 1000 / maxF
                Case ColumnSpec.SpecType.Component_Molar_Flow_Rate
                    spfval2 = Lj(ns) * xc(ns)(spci2) - spval2 / maxF
                Case ColumnSpec.SpecType.Component_Recovery
                    Dim rec As Double = spval2 / 100
                    Dim sumc As Double = 0
                    For j = 0 To ns
                        sumc += fc(j)(spci2)
                    Next
                    sumc *= rec
                    If _specs("R").SpecUnit = "% M/M" Then
                        spfval2 = lc(ns)(spci2) - sumc
                    Else '% W/W
                        spfval2 = _pp.RET_VMM()(spci2) * 1000 * (xc(0)(spci1) * Lj(ns) - sumc)
                    End If
                Case ColumnSpec.SpecType.Heat_Duty
                    Q(ns) = spval2 / maxF
                Case ColumnSpec.SpecType.Product_Mass_Flow_Rate
                    spfval2 = Lj(ns) - spval2 / _pp.AUX_MMM(xc(ns)) * 1000 / maxF
                Case ColumnSpec.SpecType.Product_Molar_Flow_Rate
                    spfval2 = Lj(ns) - spval2 / maxF
                Case ColumnSpec.SpecType.Stream_Ratio
                    spfval2 = Vj(ns) - spval2 * (Lj(ns))
                Case ColumnSpec.SpecType.Temperature
                    spfval2 = Tj(ns) - spval2
            End Select


            For i = 0 To ns
                For j = 0 To nc - 1
                    M_ant(i, j) = M(i, j)
                    E_ant(i, j) = E(i, j)
                    H_ant(i) = H(i)
                    If i = 0 Then
                        M(i, j) = lc(i)(j) * (1 + Sl(i)) + vc(i)(j) * (1 + Sv(i)) - vc(i + 1)(j) - fc(i)(j)
                        E(i, j) = eff(i) * Kval(i)(j) * lc(i)(j) * sumvkj(i) / sumlkj(i) - vc(i)(j) + (1 - eff(i)) * vc(i + 1)(j) * sumvkj(i) / sumvkj(i + 1)
                    ElseIf i = ns Then
                        M(i, j) = lc(i)(j) * (1 + Sl(i)) + vc(i)(j) * (1 + Sv(i)) - lc(i - 1)(j) - fc(i)(j)
                        E(i, j) = eff(i) * Kval(i)(j) * lc(i)(j) * sumvkj(i) / sumlkj(i) - vc(i)(j)
                    Else
                        M(i, j) = lc(i)(j) * (1 + Sl(i)) + vc(i)(j) * (1 + Sv(i)) - lc(i - 1)(j) - vc(i + 1)(j) - fc(i)(j)
                        E(i, j) = eff(i) * Kval(i)(j) * lc(i)(j) * sumvkj(i) / sumlkj(i) - vc(i)(j) + (1 - eff(i)) * vc(i + 1)(j) * sumvkj(i) / sumvkj(i + 1)
                    End If
                Next
                If i = 0 Then
                    H(i) = (Hl(i) * (1 + Sl(i)) * sumlkj(i) + Hv(i) * (1 + Sv(i)) * sumvkj(i) - Hv(i + 1) * sumvkj(i + 1) - HF(i) * F(i) - Q(i)) / (Hv(i) - Hl(i))
                ElseIf i = ns Then
                    H(i) = (Hl(i) * (1 + Sl(i)) * sumlkj(i) + Hv(i) * (1 + Sv(i)) * sumvkj(i) - Hl(i - 1) * sumlkj(i - 1) - HF(i) * F(i) - Q(i)) / (Hv(i) - Hl(i))
                Else
                    H(i) = (Hl(i) * (1 + Sl(i)) * sumlkj(i) + Hv(i) * (1 + Sv(i)) * sumvkj(i) - Hl(i - 1) * sumlkj(i - 1) - Hv(i + 1) * sumvkj(i + 1) - HF(i) * F(i) - Q(i)) / (Hv(i) - Hl(i))
                End If
                H(i) /= (Hv(i) - Hl(i))
                Select Case coltype
                    Case Column.ColType.DistillationColumn
                        H(0) = spfval1
                        H(ns) = spfval2
                    Case Column.ColType.AbsorptionColumn
                        'do nothing
                    Case Column.ColType.ReboiledAbsorber
                        H(ns) = spfval2
                    Case Column.ColType.RefluxedAbsorber
                        H(0) = spfval1
                End Select
            Next

            If _condtype = Column.condtype.Total_Condenser Then
                Dim sum1 As Double = 0
                For j = 0 To nc - 1
                    sum1 += Kval(0)(j) * xc(0)(j)
                Next
                i = 0
                For j = 0 To nc - 1
                    If j = 0 Then
                        E(i, j) = 1 - sum1
                    Else
                        E(i, j) = xc(i)(j) - yc(i)(j)
                    End If
                Next
            End If

            Dim errors(_bx.Length - 1) As Double

            For i = 0 To ns
                errors(i * (2 * nc + 1)) = H(i)
                For j = 0 To nc - 1
                    errors(i * (2 * nc + 1) + j + 1) = M(i, j)
                    errors(i * (2 * nc + 1) + j + 1 + nc) = E(i, j)
                Next
            Next

            Dim errt As Double = 0
            For i = 0 To UBound(errors) - 1
                errt += errors(i) ^ 2
            Next

            Return errt

        End Function

        Private Function FunctionGradient(ByVal x() As Double) As Double(,)

            Dim epsilon As Double = ndeps

            Dim f2(), f3() As Double
            Dim g(x.Length - 1, x.Length - 1), x1(x.Length - 1), x2(x.Length - 1), x3(x.Length - 1), x4(x.Length - 1) As Double
            Dim i, j, k As Integer

            For i = 0 To x.Length - 1
                For j = 0 To x.Length - 1
                    If i <> j Then
                        'x1(j) = x(j)
                        x2(j) = x(j)
                        x3(j) = x(j)
                        'x4(j) = x(j)
                    Else
                        If x(j) <> 0.0# Then
                            'x1(j) = x(j) + 2 * epsilon
                            x2(j) = x(j) * (1 + epsilon)
                            x3(j) = x(j) * (1 - epsilon)
                            'x4(j) = x(j) - 2 * epsilon
                        Else
                            'x1(j) = x(j) + 2 * epsilon
                            x2(j) = x(j) + epsilon
                            x3(j) = x(j)
                            'x4(j) = x(j) - 2 * epsilon
                        End If
                    End If
                Next
                'f1 = FunctionValue(x1)
                f2 = FunctionValue(x2)
                f3 = FunctionValue(x3)
                'f4 = FunctionValue(x4)
                For k = 0 To x.Length - 1
                    g(k, i) = (f2(k) - f3(k)) / (x2(i) - x3(i))
                Next
            Next

            Return g

        End Function

        Public Function Solve(ByVal nc As Integer, ByVal ns As Integer, ByVal maxits As Integer, _
                                ByVal tol As Array, ByVal F As Array, ByVal V As Array, _
                                ByVal Q As Array, ByVal L As Array, _
                                ByVal VSS As Array, ByVal LSS As Array, ByVal Kval()() As Double, _
                                ByVal x()() As Double, ByVal y()() As Double, ByVal z()() As Double, _
                                ByVal fc()() As Double, _
                                ByVal HF As Array, ByVal T As Array, ByVal P As Array, _
                                ByVal condt As DistillationColumn.condtype, _
                                ByVal eff() As Double, _
                                ByVal UseDampingFactor As Boolean, _
                                ByVal UseNewtonUpdate As Boolean, _
                                ByVal UseIJ As Boolean, _
                                ByVal coltype As Column.ColType, _
                                ByVal pp As PropertyPackages.PropertyPackage, _
                                ByVal specs As Dictionary(Of String, SepOps.ColumnSpec), _
                                ByVal reuseJ As Boolean, ByVal jac0 As Object, _
                                ByVal df As Double, ByVal maxtc As Double, ByVal epsilon As Double, _
                                ByVal maxvarchgfac As Integer, _
                                Optional ByVal LLEX As Boolean = False) As Object

            llextr = LLEX 'liquid-liquid extractor

            ndeps = epsilon

            Dim brentsolver As New BrentOpt.BrentMinimize
            brentsolver.DefineFuncDelegate(AddressOf MinimizeError)

            Dim cv As New SistemasDeUnidades.Conversor
            Dim spval1, spval2 As Double
            Dim spci1, spci2 As Integer

            spval1 = cv.ConverterParaSI(specs("C").SpecUnit, specs("C").SpecValue)
            spci1 = specs("C").ComponentIndex
            spval2 = cv.ConverterParaSI(specs("R").SpecUnit, specs("R").SpecValue)
            spci2 = specs("R").ComponentIndex

            Dim ic, ec As Integer
            Dim Tj(ns), Tj_ant(ns), T_(ns) As Double
            Dim Lj(ns), Vj(ns), xc(ns)(), yc(ns)(), lc(ns)(), vc(ns)(), zc(ns)() As Double
            Dim Tj0(ns), lc0(ns)(), vc0(ns)(), Kval0(ns)() As Double
            Dim i, j As Integer
            Dim BuildingJacobian As Boolean = False

            For i = 0 To ns
                Array.Resize(xc(i), nc)
                Array.Resize(yc(i), nc)
                Array.Resize(lc(i), nc)
                Array.Resize(vc(i), nc)
                Array.Resize(zc(i), nc)
                Array.Resize(lc0(i), nc)
                Array.Resize(vc0(i), nc)
            Next

            'step0

            'normalize initial estimates

            Dim maxF As Double = Common.Max(F)

            For i = 0 To ns
                F(i) = F(i) / maxF
                HF(i) = HF(i) / 1000
                L(i) = L(i) / maxF
                V(i) = V(i) / maxF
                LSS(i) = LSS(i) / maxF
                VSS(i) = VSS(i) / maxF
                Q(i) = Q(i) / maxF
            Next

            Dim Sl(ns), Sv(ns) As Double

            For i = 0 To ns
                For j = 0 To nc - 1
                    vc(i)(j) = y(i)(j) * V(i)
                    lc(i)(j) = x(i)(j) * L(i)
                    xc(i)(j) = x(i)(j)
                    yc(i)(j) = y(i)(j)
                    zc(i)(j) = z(i)(j)
                    Tj(i) = T(i)
                Next
                Sv(i) = VSS(i) / V(i)
                Sl(i) = LSS(i) / L(i)
            Next

            'step1

            Dim sumF As Double = 0
            Dim sumLSS As Double = 0
            Dim sumVSS As Double = 0
            For i = 0 To ns
                sumF += F(i)
                sumLSS += LSS(i)
                sumVSS += VSS(i)
            Next
            Dim B As Double = sumF - sumLSS - sumVSS - V(0)

            'step2

            Dim lsi, vsi As New ArrayList
            Dim el As Integer

            'size jacobian

            el = ns
            For i = 0 To ns
                If VSS(i) <> 0 Then
                    el += 1
                    vsi.Add(i)
                End If
                If LSS(i) <> 0 Then
                    el += 1
                    lsi.Add(i)
                End If
            Next

            Dim el_err As Double = 0.0#
            Dim el_err_ant As Double = 0.0#
            Dim il_err As Double = 0.0#
            Dim il_err_ant As Double = 0.0#

            'independent variables

            Dim VSSj(ns), LSSj(ns), Hv(ns), Hl(ns), Hv0(ns), Hl0(ns) As Double
            Dim sumvkj(ns), sumlkj(ns) As Double
            Dim fxvar((ns + 1) * (2 * nc + 1) - 1) As Double
            Dim xvar((ns + 1) * (2 * nc + 1) - 1) As Double
            Dim dxvar((ns + 1) * (2 * nc + 1) - 1) As Double
            Dim dFdXvar((ns + 1) * (2 * nc + 1) - 1, (ns + 1) * (2 * nc + 1) - 1) As Double
            Dim hes((ns + 1) * (2 * nc + 1) - 1, (ns + 1) * (2 * nc + 1) - 1) As Double
            Dim fval As Double

            Dim bx((ns + 1) * (2 * nc + 1) - 1), bx_ant((ns + 1) * (2 * nc + 1) - 1), bxb((ns + 1) * (2 * nc + 1) - 1), bf((ns + 1) * (2 * nc + 1) - 1), bfb((ns + 1) * (2 * nc + 1) - 1), bp((ns + 1) * (2 * nc + 1) - 1), bp_ant((ns + 1) * (2 * nc + 1) - 1) As Double

            For i = 0 To ns
                VSSj(i) = VSS(i)
                LSSj(i) = LSS(i)
            Next

            'solve using newton's method

            _nc = nc
            _ns = ns
            _VSS = VSS.Clone
            _LSS = LSS.Clone
            _spval1 = spval1
            _spval2 = spval2
            _spci1 = spci1
            _spci2 = spci2
            _eff = eff.Clone
            _F = F.Clone
            _Q = Q.Clone
            _P = P.Clone
            _HF = HF.Clone
            _fc = fc.Clone
            _maxF = maxF
            _pp = pp
            _coltype = coltype
            _specs = specs
            _condtype = condt

            For i = 0 To ns
                xvar(i * (2 * nc + 1)) = Tj(i)
                For j = 0 To nc - 1
                    xvar(i * (2 * nc + 1) + j + 1) = vc(i)(j)
                    xvar(i * (2 * nc + 1) + j + 1 + nc) = lc(i)(j)
                Next
            Next

            'first run (to initialize variables)

            fxvar = Me.FunctionValue(xvar)

            Dim jac As New Mapack.Matrix(xvar.Length, xvar.Length), hesm As New Mapack.Matrix(xvar.Length, xvar.Length)

            Do

                fxvar = Me.FunctionValue(xvar)

                il_err_ant = il_err
                il_err = 0
                For i = 0 To xvar.Length - 1
                    il_err += fxvar(i) ^ 2
                Next

                If il_err < tol(1) Then Exit Do

                If UseNewtonUpdate Then
                    dFdXvar = Me.FunctionGradient(xvar)
                    Dim success As Boolean
                    success = MathEx.SysLin.rsolve.rmatrixsolve(dFdXvar, fxvar, xvar.Length, dxvar)
                    For i = 0 To xvar.Length - 1
                        dxvar(i) = -dxvar(i)
                    Next
                    _bx = xvar.Clone
                    _dbx = dxvar.Clone
                Else
                    If ic = 0 Then
                        dFdXvar = Me.FunctionGradient(xvar)
                        For i = 0 To xvar.Length - 1
                            For j = 0 To xvar.Length - 1
                                jac(i, j) = dFdXvar(i, j)
                            Next
                        Next
                        'Console.WriteLine(jac.ToString())
                        hesm = jac.Inverse
                        For i = 0 To xvar.Length - 1
                            For j = 0 To xvar.Length - 1
                                hes(i, j) = hesm(i, j)
                            Next
                        Next
                        bx = xvar
                        bf = fxvar
                        Broyden.broydn(xvar.Length - 1, bx, bf, bp, bxb, bfb, hes, 0)
                        dxvar = bp
                    Else
                        bx = xvar
                        bf = fxvar
                        Broyden.broydn(xvar.Length - 1, bx, bf, bp, bxb, bfb, hes, 1)
                        dxvar = bp
                    End If
                    _bx = xvar.Clone
                    _dbx = bp.Clone
                End If

                'this call to the brent solver calculates the damping factor which minimizes the error (fval).
                df = 1
                If UseDampingFactor Then fval = brentsolver.brentoptimize(0, 2, tol(0), df)

                Dim tmultpl As Double = 1.0#, dampenT As Boolean = False

                For i = 0 To xvar.Length - 1
                    If i = i * (2 * nc + 1) Then
                        If Abs(dxvar(i) * df) > maxtc Then
                            dampenT = True
                        End If
                    End If
                Next

                If dampenT Then tmultpl = maxtc / MathEx.Common.Min(Tj)

                For i = 0 To xvar.Length - 1
                    If xvar(i) + dxvar(i) * df < 0 Then
                        xvar(i) = xvar(i) * Exp(df * dxvar(i) * tmultpl / xvar(i))
                    Else
                        xvar(i) += dxvar(i) * df * tmultpl
                    End If
                Next


                ic += 1

                If ic >= maxits Then Throw New Exception(DWSIM.App.GetLocalString("DCMaxIterationsReached"))
                If Double.IsNaN(il_err) Then Throw New Exception(DWSIM.App.GetLocalString("DCGeneralError"))
                If Abs(MathEx.Common.AbsSum(dxvar)) < tol(0) Then Exit Do

                CheckCalculatorStatus()

            Loop

            ec = ic

            If Abs(il_err) > tol(1) Then
                My.Application.ActiveSimulation.WriteToLog("The sum of squared absolute errors (internal loop) isn't changing anymore. Final value is " & il_err & ".", Color.Green, FormClasses.TipoAviso.Aviso)
            End If

            For i = 0 To ns
                Tj(i) = xvar(i * (2 * nc + 1))
                For j = 0 To nc - 1
                    vc(i)(j) = xvar(i * (2 * nc + 1) + j + 1)
                    lc(i)(j) = xvar(i * (2 * nc + 1) + j + 1 + nc)
                Next
            Next

            For i = 0 To ns
                sumvkj(i) = 0
                sumlkj(i) = 0
                For j = 0 To nc - 1
                    sumvkj(i) += vc(i)(j)
                    sumlkj(i) += lc(i)(j)
                Next
                Vj(i) = sumvkj(i)
                Lj(i) = sumlkj(i)
            Next

            For i = 0 To ns
                For j = 0 To nc - 1
                    xc(i)(j) = lc(i)(j) / sumlkj(i)
                Next
                For j = 0 To nc - 1
                    yc(i)(j) = vc(i)(j) / sumvkj(i)
                Next
            Next

            ' finished, de-normalize and return arrays
            Dim K(ns, nc - 1) As Object
            For i = 0 To ns
                For j = 0 To nc - 1
                    K(i, j) = _Kval(i)(j)
                Next
            Next

            For i = 0 To ns
                If Vj(i) <> 0 Then Sv(i) = VSSj(i) / Vj(i) Else Sv(i) = 0
                If Lj(i) <> 0 Then Sl(i) = LSSj(i) / Lj(i) Else Sl(i) = 0
            Next

            Q = _Q.Clone

            For i = 0 To ns
                Lj(i) = sumlkj(i) * maxF
                Vj(i) = sumvkj(i) * maxF
                LSSj(i) = Sl(i) * Lj(i)
                VSSj(i) = Sv(i) * Vj(i)
                F(i) = F(i) * maxF
                L(i) = L(i) * maxF
                V(i) = V(i) * maxF
                LSS(i) = LSS(i) * maxF
                VSS(i) = VSS(i) * maxF
                Q(i) = Q(i) * maxF
            Next

            Return New Object() {Tj, Vj, Lj, VSSj, LSSj, yc, xc, K, Q, ec, il_err, ic, el_err, dFdXvar}

        End Function

    End Class

End Namespace
