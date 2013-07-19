'    Electrolyte-related Functions and Procedures
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

Imports System.Collections.Generic
Imports FileHelpers
Imports System.Math
Imports System.Linq
Imports DWSIM.DWSIM.ClassesBasicasTermodinamica

Namespace DWSIM.SimulationObjects.PropertyPackages.Auxiliary

    <System.Serializable()> Public Class Electrolyte

        Function FreezingPointDepression(ByVal Vx As Double(), activcoeff As Double(), cprops As List(Of ConstantProperties)) As Double()

            Dim n As Integer = UBound(Vx)
            Dim wid As Integer = cprops.IndexOf((From c As ConstantProperties In cprops Select c Where c.Name = "Water").SingleOrDefault)
            Dim Tnfp, DHm, DT, Td As Double

            Tnfp = cprops(wid).TemperatureOfFusion
            DHm = cprops(wid).EnthalpyOfFusionAtTf * cprops(wid).Molar_Weight

            DT = 8.314 * Tnfp ^ 2 / DHm * Math.Log(Vx(wid) * activcoeff(wid))
            Td = DT + Tnfp

            Return New Double() {Td, DT}

        End Function

        Function OsmoticCoeff(Vx As Double(), activcoeff As Double(), cprops As List(Of ConstantProperties)) As Double

            Dim n As Integer = UBound(Vx)
            Dim i As Integer
            Dim molality(n), summ As Double

            Dim wid As Integer = cprops.IndexOf((From c As ConstantProperties In cprops Select c Where c.Name = "Water").SingleOrDefault)

            'calculate molality considering 1 mol of mixture.

            Dim wtotal As Double = 0

            i = 0
            Do
                If cprops(i).Name = "Water" Then
                    wtotal += Vx(i) * cprops(i).Molar_Weight / 1000
                End If
                i += 1
            Loop Until i = n + 1

            i = 0
            Do
                If cprops(i).IsIon Then
                    molality(i) = Vx(i) / wtotal
                    summ += molality(i)
                End If
                i += 1
            Loop Until i = n + 1

            Dim oc As Double = -Log(Vx(wid) * activcoeff(wid)) / (cprops(wid).Molar_Weight / 1000 * summ)

            Return oc

        End Function

        Function pH(Vx As Double(), T As Double, activcoeff As Double(), cprops As List(Of ConstantProperties)) As Double

            Dim n As Integer = UBound(Vx)
            Dim i As Integer
            Dim molality(n), summ As Double

            Dim wid As Integer = cprops.IndexOf((From c As ConstantProperties In cprops Select c Where c.Name = "Water").SingleOrDefault)
            Dim hid As Integer = cprops.IndexOf((From c As ConstantProperties In cprops Select c Where c.Name = "Hydron").SingleOrDefault)

            'calculate molality considering 1 mol of mixture.

            Dim wtotal As Double = 0

            i = 0
            Do
                If cprops(i).Name = "Water" Then
                    wtotal += Vx(i) * cprops(i).Molar_Weight / 1000
                End If
                i += 1
            Loop Until i = n + 1

            i = 0
            Do
                If cprops(i).IsIon Then
                    molality(i) = Vx(i) / wtotal
                    summ += molality(i)
                End If
                i += 1
            Loop Until i = n + 1

            Dim phvalue As Double = 0.0#

            If hid <> -1 Then phvalue = -Log10(molality(hid) * activcoeff(hid))

            Return phvalue

        End Function

        Function SolidEnthalpy(ByVal T As Double, ByVal Vx As Double(), cprops As List(Of ConstantProperties)) As Double

            Dim n As Integer = UBound(Vx)
            Dim i As Integer
            Dim HS As Double = 0.0#
            Dim MW As Double = 0.0#

            For i = 0 To n
                HS += Vx(i) * cprops(i).Electrolyte_Cp0 * (T - 298)
                MW += Vx(i) * cprops(i).Molar_Weight
            Next

            Return HS * 1000 / MW 'kJ/kg

        End Function

        Function HeatCapacityCp(ByVal T As Double, ByVal Vx As Double(), cprops As List(Of ConstantProperties)) As Double

            Dim n As Integer = UBound(Vx)
            Dim i As Integer
            Dim Cp As Double = 0.0#
            Dim MW As Double = 0.0#

            For i = 0 To n
                Cp += Vx(i) * cprops(i).Electrolyte_Cp0
                MW += Vx(i) * cprops(i).Molar_Weight
            Next

            Return Cp * 1000 / MW 'kJ/kg

        End Function

        Function LiquidDensity(Vx As Double(), T As Double, cprops As List(Of ConstantProperties)) As Double

            Dim wid As Integer = cprops.IndexOf((From c As ConstantProperties In cprops Select c Where c.Name = "Water").SingleOrDefault)

            Dim n As Integer = UBound(Vx)
            Dim molality(n) As Double
            Dim i As Integer
            Dim vol As Double = 0.0#
            Dim MW As Double = 0.0#
            Dim Tr As Double = 0

            Dim wtotal As Double = 0

            i = 0
            Do
                If cprops(i).Name = "Water" Then
                    wtotal += Vx(i) * cprops(i).Molar_Weight / 1000
                End If
                i += 1
            Loop Until i = n + 1

            Dim Xsolv As Double = 1

            i = 0
            Do
                molality(i) = Vx(i) / wtotal
                'salt-free mole fractions
                If cprops(i).Name <> "Water" Then
                    Xsolv -= Vx(i)
                End If
                i += 1
            Loop Until i = n + 1

            i = 0
            Dim Im As Double = 0.0#
            Do
                Im += cprops(i).Charge ^ 2 * molality(i) / 2
                i += 1
            Loop Until i = n + 1

            vol = 0.0#
            For i = 0 To n
                If i = wid Then
                    Tr = T / cprops(i).Critical_Temperature
                    vol += Vx(i) * (1 / (32.51621 * (1 - Tr) ^ (-3.213004 + 7.92411 * Tr + -7.359898 * Tr ^ 2 + 2.703522 * Tr ^ 3))) 'm3/kmol
                ElseIf cprops(i).IsIon Then
                    With cprops(i)
                        vol += Vx(i) * ((.StandardStateMolarVolume + .MolarVolume_v2i * (T - 298.15) + .MolarVolume_v3i * (T - 298.15) ^ 2) +
                                (.MolarVolume_k1i + .MolarVolume_k2i * (T - 298.15) + .MolarVolume_k3i * (T - 298.15) ^ 2) * Im ^ 0.5) * 0.001 'm3/kmol
                    End With
                Else
                    vol += 0.0#
                End If
                MW += Vx(i) * cprops(i).Molar_Weight
            Next

            Return MW / vol 'kg/m3

        End Function

    End Class

End Namespace
