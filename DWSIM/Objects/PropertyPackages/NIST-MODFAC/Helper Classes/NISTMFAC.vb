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

        Inherits Modfac

        Public Shadows ModfGroups As NistModfacGroups

        Sub New()

            ModfGroups = New NistModfacGroups

        End Sub

    End Class

    <System.Serializable()> Public Class NistModfacGroups

        Public InteracParam_aij As System.Collections.Generic.Dictionary(Of Integer, System.Collections.Generic.Dictionary(Of Integer, Double))
        Public InteracParam_bij As System.Collections.Generic.Dictionary(Of Integer, System.Collections.Generic.Dictionary(Of Integer, Double))
        Public InteracParam_cij As System.Collections.Generic.Dictionary(Of Integer, System.Collections.Generic.Dictionary(Of Integer, Double))
        Public InteracParam_aji As System.Collections.Generic.Dictionary(Of Integer, System.Collections.Generic.Dictionary(Of Integer, Double))
        Public InteracParam_bji As System.Collections.Generic.Dictionary(Of Integer, System.Collections.Generic.Dictionary(Of Integer, Double))
        Public InteracParam_cji As System.Collections.Generic.Dictionary(Of Integer, System.Collections.Generic.Dictionary(Of Integer, Double))

        Protected m_groups As System.Collections.Generic.Dictionary(Of Integer, ModfacGroup)

        Sub New()

            Dim pathsep = System.IO.Path.DirectorySeparatorChar

            m_groups = New System.Collections.Generic.Dictionary(Of Integer, ModfacGroup)
            InteracParam_aij = New System.Collections.Generic.Dictionary(Of Integer, System.Collections.Generic.Dictionary(Of Integer, Double))
            InteracParam_bij = New System.Collections.Generic.Dictionary(Of Integer, System.Collections.Generic.Dictionary(Of Integer, Double))
            InteracParam_cij = New System.Collections.Generic.Dictionary(Of Integer, System.Collections.Generic.Dictionary(Of Integer, Double))
            InteracParam_aji = New System.Collections.Generic.Dictionary(Of Integer, System.Collections.Generic.Dictionary(Of Integer, Double))
            InteracParam_bji = New System.Collections.Generic.Dictionary(Of Integer, System.Collections.Generic.Dictionary(Of Integer, Double))
            InteracParam_cji = New System.Collections.Generic.Dictionary(Of Integer, System.Collections.Generic.Dictionary(Of Integer, Double))

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
                While Not parser.EndOfData
                    fields = parser.ReadFields()
                    If fields(0).StartsWith("(") Then
                        maingroup = fields(0).Split(")")(0).Substring(1)
                        mainname = fields(0).Trim().Split(")")(1).Trim
                    Else
                        Me.Groups.Add(fields(0), New ModfacGroup(fields(1), mainname, maingroup, fields(0), Double.Parse(fields(3), cult), Double.Parse(fields(2), cult)))
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
