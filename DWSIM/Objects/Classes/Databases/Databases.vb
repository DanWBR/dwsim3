'    Copyright 2008-2012 Daniel Wagner O. de Medeiros
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
'
'    Imports DWSIM.SimulationObjects

Imports FileHelpers
Imports System.Xml
Imports System.IO
Imports DWSIM.DWSIM.SimulationObjects.PropertyPackages.Auxiliary

Namespace DWSIM.Databases

    <DelimitedRecord(";")> <IgnoreFirst()> <System.Serializable()> _
    Public Class ChemSepNameIDPair

        Implements ICloneable

        Public ID As Integer = -1
        Public ChemSepName As String = ""
        Public DWSIMName As String = ""

        Public Function Clone() As Object Implements System.ICloneable.Clone

            Dim newclass As New ChemSepNameIDPair
            With newclass
                .ID = Me.ID
                .ChemSepName = Me.ChemSepName
                .DWSIMName = Me.DWSIMName
            End With
            Return newclass
        End Function

    End Class

    <System.Serializable()> Public Class ChemSep

        Private _ids As System.Collections.Generic.Dictionary(Of Integer, ChemSepNameIDPair)
        Private xmldoc As XmlDocument

        Public ReadOnly Property IDs() As System.Collections.Generic.Dictionary(Of Integer, ChemSepNameIDPair)
            Get
                Return _ids
            End Get
        End Property

        Sub New()

            _ids = New System.Collections.Generic.Dictionary(Of Integer, ChemSepNameIDPair)

            Dim pathsep As Char = System.IO.Path.DirectorySeparatorChar

            Dim csid As ChemSepNameIDPair
            Dim csidc() As ChemSepNameIDPair
            Dim fh1 As New FileHelperEngine(Of ChemSepNameIDPair)
            With fh1
                csidc = .ReadFile(My.Application.Info.DirectoryPath & pathsep & "data" & pathsep & "csid.dat")
            End With

            For Each csid In csidc
                Me.IDs.Add(csid.ID, csid.Clone)
            Next

            csid = Nothing
            csidc = Nothing
            fh1 = Nothing

        End Sub

        Public Function GetCSName(ByVal id As String)

            If Me.IDs.ContainsKey(id) Then
                Return Me.IDs(id).ChemSepName
            Else
                Return id
            End If

        End Function

        Public Function GetDWSIMName(ByVal id As String)

            If Me.IDs.ContainsKey(id) Then
                Return Me.IDs(id).DWSIMName
            Else
                Return id
            End If

        End Function

        Public Sub Load(ByVal filename As String)

            Dim mytxt As String = ""
            mytxt = File.ReadAllText(filename)

            xmldoc = New XmlDocument
            xmldoc.LoadXml(mytxt)

            mytxt = Nothing

        End Sub

        Public Function Transfer() As ClassesBasicasTermodinamica.ConstantProperties()

            Dim cp As ClassesBasicasTermodinamica.ConstantProperties
            Dim cpa As New ArrayList()
            Dim cult As Globalization.CultureInfo = New Globalization.CultureInfo("en-US")
            Dim nf As Globalization.NumberFormatInfo = cult.NumberFormat

            Dim unif As New Global.DWSIM.DWSIM.SimulationObjects.PropertyPackages.Auxiliary.Unifac
            Dim modf As New Global.DWSIM.DWSIM.SimulationObjects.PropertyPackages.Auxiliary.Modfac

            For Each node As XmlNode In xmldoc.ChildNodes(0).ChildNodes
                cp = New ClassesBasicasTermodinamica.ConstantProperties
                With cp
                    .CurrentDB = "ChemSep"
                    .OriginalDB = "ChemSep"
                    .IsHYPO = "0"
                    .IsPF = "0"
                End With
                For Each node2 As XmlNode In node.ChildNodes
                    Select Case node2.Name
                        Case "LibraryIndex"
                            cp.ID = node2.Attributes("value").Value
                        Case "CompoundID"
                            cp.Name = node2.Attributes("value").Value
                        Case "StructureFormula"
                            cp.Formula = node2.Attributes("value").Value
                        Case "CriticalTemperature" 'K
                            cp.Critical_Temperature = Double.Parse(node2.Attributes("value").Value, nf)
                        Case "CriticalPressure" 'Pa
                            cp.Critical_Pressure = Double.Parse(node2.Attributes("value").Value, nf)
                        Case "CriticalVolume" 'm3/kmol
                            cp.Critical_Volume = Double.Parse(node2.Attributes("value").Value, nf)
                        Case "CriticalCompressibility"
                            cp.Critical_Compressibility = Double.Parse(node2.Attributes("value").Value, nf)
                        Case "NormalBoilingPointTemperature" 'K
                            cp.Normal_Boiling_Point = Double.Parse(node2.Attributes("value").Value, nf)
                        Case "NormalMeltingPointTemperature"
                            cp.TemperatureOfFusion = Double.Parse(node2.Attributes("value").Value, nf)
                        Case "MolecularWeight"
                            cp.Molar_Weight = Double.Parse(node2.Attributes("value").Value, nf)
                        Case "AcentricityFactor"
                            cp.Acentric_Factor = Double.Parse(node2.Attributes("value").Value, nf)
                        Case "DipoleMoment" 'coloumb.m
                            cp.Dipole_Moment = Double.Parse(node2.Attributes("value").Value, nf)
                        Case "HeatOfFusionAtMeltingPoint" ' kJ/kg
                            cp.EnthalpyOfFusionAtTf = Double.Parse(node2.Attributes("value").Value, nf) / 1000 / cp.Molar_Weight
                        Case "HeatOfFormation" '/1000/MW, kJ/kg
                            cp.IG_Enthalpy_of_Formation_25C = Double.Parse(node2.Attributes("value").Value, nf) / 1000 / cp.Molar_Weight
                        Case "GibbsEnergyOfFormation" '/1000/MW, kJ/kg
                            cp.IG_Gibbs_Energy_of_Formation_25C = Double.Parse(node2.Attributes("value").Value, nf) / 1000 / cp.Molar_Weight
                        Case "AbsEntropy" '/1000/MW, kJ/kg
                            cp.IG_Entropy_of_Formation_25C = Double.Parse(node2.Attributes("value").Value, nf) / 1000 / cp.Molar_Weight
                        Case "RacketParameter"
                            cp.Z_Rackett = Double.Parse(node2.Attributes("value").Value, nf)
                        Case "ChaoSeaderAcentricFactor"
                            cp.Chao_Seader_Acentricity = Double.Parse(node2.Attributes("value").Value, nf)
                        Case "ChaoSeaderSolubilityParameter"
                            cp.Chao_Seader_Solubility_Parameter = Double.Parse(node2.Attributes("value").Value, nf) * 0.238846 / 1000000.0
                        Case "ChaoSeaderLiquidVolume"
                            cp.Chao_Seader_Liquid_Molar_Volume = Double.Parse(node2.Attributes("value").Value, nf) * 1000
                        Case "CAS"
                            cp.CAS_Number = node2.Attributes("value").Value
                        Case "Smiles"
                            cp.SMILES = node2.Attributes("value").Value
                        Case "StructureFormula"
                            cp.ChemicalStructure = node2.Attributes("value").Value
                        Case "UniquacR"
                            cp.UNIQUAC_R = Double.Parse(node2.Attributes("value").Value, nf)
                        Case "UniquacQ"
                            cp.UNIQUAC_Q = Double.Parse(node2.Attributes("value").Value, nf)
                        Case "VaporPressure"
                            '<vp_c name="Vapour pressure"  units="Pa" >
                            For Each node3 As XmlNode In node2.ChildNodes
                                Select Case node3.Name
                                    Case "eqno"
                                        cp.VaporPressureEquation = node3.Attributes("value").Value
                                    Case "A"
                                        cp.Vapor_Pressure_Constant_A = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "B"
                                        cp.Vapor_Pressure_Constant_B = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "C"
                                        cp.Vapor_Pressure_Constant_C = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "D"
                                        cp.Vapor_Pressure_Constant_D = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "E"
                                        cp.Vapor_Pressure_Constant_E = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "Tmin"
                                        cp.Vapor_Pressure_TMIN = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "Tmax"
                                        cp.Vapor_Pressure_TMAX = Double.Parse(node3.Attributes("value").Value, nf)
                                End Select
                            Next
                        Case "HeatOfVaporization"
                            '<hvpc name="Heat of vaporization"  units="J/kmol" >
                            For Each node3 As XmlNode In node2.ChildNodes
                                Select Case node3.Name
                                    Case "eqno"
                                        cp.VaporizationEnthalpyEquation = node3.Attributes("value").Value
                                    Case "A"
                                        cp.HVap_A = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "B"
                                        cp.HVap_B = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "C"
                                        cp.HVap_C = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "D"
                                        cp.HVap_D = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "Tmin"
                                        cp.HVap_TMIN = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "Tmax"
                                        cp.HVap_TMAX = Double.Parse(node3.Attributes("value").Value, nf)
                                End Select
                            Next
                        Case "IdealGasHeatCapacityCp"
                            '<icpc name="Ideal gas heat capacity"  units="J/kmol/K" >
                            For Each node3 As XmlNode In node2.ChildNodes
                                Select Case node3.Name
                                    Case "eqno"
                                        cp.IdealgasCpEquation = node3.Attributes("value").Value
                                    Case "A"
                                        cp.Ideal_Gas_Heat_Capacity_Const_A = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "B"
                                        cp.Ideal_Gas_Heat_Capacity_Const_B = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "C"
                                        cp.Ideal_Gas_Heat_Capacity_Const_C = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "D"
                                        cp.Ideal_Gas_Heat_Capacity_Const_D = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "E"
                                        cp.Ideal_Gas_Heat_Capacity_Const_E = Double.Parse(node3.Attributes("value").Value, nf)
                                End Select
                            Next
                        Case "LiquidViscosity"
                            '<lvsc name="Liquid viscosity"  units="Pa.s" >
                            For Each node3 As XmlNode In node2.ChildNodes
                                Select Case node3.Name
                                    Case "eqno"
                                        cp.LiquidViscosityEquation = node3.Attributes("value").Value
                                    Case "A"
                                        cp.Liquid_Viscosity_Const_A = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "B"
                                        cp.Liquid_Viscosity_Const_B = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "C"
                                        cp.Liquid_Viscosity_Const_C = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "D"
                                        cp.Liquid_Viscosity_Const_D = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "E"
                                        cp.Liquid_Viscosity_Const_E = Double.Parse(node3.Attributes("value").Value, nf)
                                End Select
                            Next
                        Case "LiquidThermalConductivity"
                            '- <LiquidThermalConductivity name="Liquid thermal conductivity" units="W/m/K">
                            For Each node3 As XmlNode In node2.ChildNodes
                                Select Case node3.Name
                                    Case "eqno"
                                        cp.LiquidThermalConductivityEquation = node3.Attributes("value").Value
                                    Case "A"
                                        cp.Liquid_Thermal_Conductivity_Const_A = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "B"
                                        cp.Liquid_Thermal_Conductivity_Const_B = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "C"
                                        cp.Liquid_Thermal_Conductivity_Const_C = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "D"
                                        cp.Liquid_Thermal_Conductivity_Const_D = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "E"
                                        cp.Liquid_Thermal_Conductivity_Const_E = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "Tmin"
                                        cp.Liquid_Thermal_Conductivity_Tmin = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "Tmax"
                                        cp.Liquid_Thermal_Conductivity_Tmax = Double.Parse(node3.Attributes("value").Value, nf)
                                End Select
                            Next
                        Case "LiquidDensity"
                            '- <LiquidDensity name="Liquid density" units="kmol/m3">
                            For Each node3 As XmlNode In node2.ChildNodes
                                Select Case node3.Name
                                    Case "eqno"
                                        cp.LiquidDensityEquation = node3.Attributes("value").Value
                                    Case "A"
                                        cp.Liquid_Density_Const_A = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "B"
                                        cp.Liquid_Density_Const_B = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "C"
                                        cp.Liquid_Density_Const_C = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "D"
                                        cp.Liquid_Density_Const_D = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "E"
                                        cp.Liquid_Density_Const_E = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "Tmin"
                                        cp.Liquid_Density_Tmin = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "Tmax"
                                        cp.Liquid_Density_Tmax = Double.Parse(node3.Attributes("value").Value, nf)
                                End Select
                            Next
                        Case "LiquidHeatCapacityCp"
                            '- <LiquidHeatCapacityCp name="Liquid heat capacity" units="J/kmol/K">
                            For Each node3 As XmlNode In node2.ChildNodes
                                Select Case node3.Name
                                    Case "eqno"
                                        cp.LiquidHeatCapacityEquation = node3.Attributes("value").Value
                                    Case "A"
                                        cp.Liquid_Heat_Capacity_Const_A = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "B"
                                        cp.Liquid_Heat_Capacity_Const_B = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "C"
                                        cp.Liquid_Heat_Capacity_Const_C = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "D"
                                        cp.Liquid_Heat_Capacity_Const_D = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "E"
                                        cp.Liquid_Heat_Capacity_Const_E = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "Tmin"
                                        cp.Liquid_Heat_Capacity_Tmin = Double.Parse(node3.Attributes("value").Value, nf)
                                    Case "Tmax"
                                        cp.Liquid_Heat_Capacity_Tmax = Double.Parse(node3.Attributes("value").Value, nf)
                                End Select
                            Next
                        Case "UnifacVLE"
                            If cp.UNIFACGroups.Collection Is Nothing Then cp.UNIFACGroups.Collection = New SortedList
                            For Each node3 As XmlNode In node2.ChildNodes
                                Select Case node3.Name
                                    Case "group"
                                        If Not cp.UNIFACGroups.Collection.ContainsKey(unif.ID2Group(Integer.Parse(node3.Attributes("id").Value))) Then
                                            cp.UNIFACGroups.Collection.Add(unif.ID2Group(Integer.Parse(node3.Attributes("id").Value)), Integer.Parse(node3.Attributes("value").Value))
                                        End If
                                End Select
                            Next
                        Case "ModifiedUnifac"
                            If cp.MODFACGroups.Collection Is Nothing Then cp.MODFACGroups.Collection = New SortedList
                            For Each node3 As XmlNode In node2.ChildNodes
                                Select Case node3.Name
                                    Case "group"
                                        If Not cp.MODFACGroups.Collection.ContainsKey(modf.ID2Group(Integer.Parse(node3.Attributes("id").Value))) Then
                                            cp.MODFACGroups.Collection.Add(modf.ID2Group(Integer.Parse(node3.Attributes("id").Value)), Integer.Parse(node3.Attributes("value").Value))
                                        End If
                                End Select
                            Next
                    End Select
                Next
                cpa.Add(cp)
            Next

            Return cpa.ToArray(Type.GetType("DWSIM.DWSIM.ClassesBasicasTermodinamica.ConstantProperties"))

        End Function

    End Class

    <System.Serializable()> Public Class DWSIM

        Private xmldoc As XmlDocument

        Sub New()

        End Sub

        Public Sub Load(ByVal filename As String)
            Dim pathsep As Char = Path.DirectorySeparatorChar

            Dim settings As New XmlReaderSettings()
            settings.ConformanceLevel = ConformanceLevel.Fragment
            settings.IgnoreWhitespace = True
            settings.IgnoreComments = True
            settings.CheckCharacters = False
            Dim reader As XmlReader = XmlReader.Create(filename)
            reader.Read()

            xmldoc = New XmlDocument
            xmldoc.Load(reader)

        End Sub

        Public Function Transfer() As ClassesBasicasTermodinamica.ConstantProperties()

            Dim cp As ClassesBasicasTermodinamica.ConstantProperties
            Dim cpa As New ArrayList()
            Dim cult As Globalization.CultureInfo = New Globalization.CultureInfo("en-US")
            Dim nf As Globalization.NumberFormatInfo = cult.NumberFormat

            For Each node As XmlNode In xmldoc.ChildNodes(1)
                cp = New ClassesBasicasTermodinamica.ConstantProperties
                With cp
                    .OriginalDB = "DWSIM"
                    For Each node2 As XmlNode In node.ChildNodes
                        Select Case node2.Name
                            Case "Name"
                                .Name = node2.InnerText
                            Case "CAS_Number"
                                .CAS_Number = node2.InnerText
                            Case "Formula"
                                .Formula = node2.InnerText
                            Case "Molar_Weight"
                                .Molar_Weight = Double.Parse(node2.InnerText, nf)
                            Case "Critical_Temperature"
                                .Critical_Temperature = Double.Parse(node2.InnerText, nf)
                            Case "Critical_Pressure"
                                .Critical_Pressure = Double.Parse(node2.InnerText, nf)
                            Case "Critical_Volume"
                                .Critical_Volume = Double.Parse(node2.InnerText, nf)
                            Case "Critical_Compressibility"
                                .Critical_Compressibility = Double.Parse(node2.InnerText, nf)
                            Case "Acentric_Factor"
                                .Acentric_Factor = Double.Parse(node2.InnerText, nf)
                            Case "Z_Rackett"
                                .Z_Rackett = Double.Parse(node2.InnerText, nf)
                            Case "PR_Volume_Translation_Coefficient"
                                .PR_Volume_Translation_Coefficient = Double.Parse(node2.InnerText, nf)
                            Case "SRK_Volume_Translation_Coefficient"
                                .SRK_Volume_Translation_Coefficient = Double.Parse(node2.InnerText, nf)
                            Case "CS_Acentric_Factor"
                                .Chao_Seader_Acentricity = Double.Parse(node2.InnerText, nf)
                            Case "CS_Solubility_Parameter"
                                .Chao_Seader_Solubility_Parameter = Double.Parse(node2.InnerText, nf)
                            Case "CS_Liquid_Molar_Volume"
                                .Chao_Seader_Liquid_Molar_Volume = Double.Parse(node2.InnerText, nf)
                            Case "IG_Entropy_of_Formation_25C"
                                .IG_Entropy_of_Formation_25C = Double.Parse(node2.InnerText, nf)
                            Case "IG_Enthalpy_of_Formation_25C"
                                .IG_Enthalpy_of_Formation_25C = Double.Parse(node2.InnerText, nf)
                            Case "IG_Gibbs_Energy_of_Formation_25C"
                                .IG_Gibbs_Energy_of_Formation_25C = Double.Parse(node2.InnerText, nf)
                            Case "Dipole_Moment"
                                .Dipole_Moment = Double.Parse(node2.InnerText, nf)
                            Case "DIPPR_Vapor_Pressure_Constant_A"
                                .Vapor_Pressure_Constant_A = Double.Parse(node2.InnerText, nf)
                            Case "DIPPR_Vapor_Pressure_Constant_B"
                                .Vapor_Pressure_Constant_B = Double.Parse(node2.InnerText, nf)
                            Case "DIPPR_Vapor_Pressure_Constant_C"
                                .Vapor_Pressure_Constant_C = Double.Parse(node2.InnerText, nf)
                            Case "DIPPR_Vapor_Pressure_Constant_D"
                                .Vapor_Pressure_Constant_D = Double.Parse(node2.InnerText, nf)
                            Case "DIPPR_Vapor_Pressure_Constant_E"
                                .Vapor_Pressure_Constant_E = Double.Parse(node2.InnerText, nf)
                            Case "DIPPR_Vapor_Pressure_TMIN"
                                .Vapor_Pressure_TMIN = Double.Parse(node2.InnerText, nf)
                            Case "DIPPR_Vapor_Pressure_TMAX"
                                .Vapor_Pressure_TMAX = Double.Parse(node2.InnerText, nf)
                            Case "Ideal_Gas_Heat_Capacity_Const_A"
                                .Ideal_Gas_Heat_Capacity_Const_A = Double.Parse(node2.InnerText, nf)
                            Case "Ideal_Gas_Heat_Capacity_Const_B"
                                .Ideal_Gas_Heat_Capacity_Const_B = Double.Parse(node2.InnerText, nf)
                            Case "Ideal_Gas_Heat_Capacity_Const_C"
                                .Ideal_Gas_Heat_Capacity_Const_C = Double.Parse(node2.InnerText, nf)
                            Case "Ideal_Gas_Heat_Capacity_Const_D"
                                .Ideal_Gas_Heat_Capacity_Const_D = Double.Parse(node2.InnerText, nf)
                            Case "Ideal_Gas_Heat_Capacity_Const_E"
                                .Ideal_Gas_Heat_Capacity_Const_E = Double.Parse(node2.InnerText, nf)
                            Case "Liquid_Viscosity_Const_A"
                                .Liquid_Viscosity_Const_A = Double.Parse(node2.InnerText, nf)
                            Case "Liquid_Viscosity_Const_B"
                                .Liquid_Viscosity_Const_B = Double.Parse(node2.InnerText, nf)
                            Case "Liquid_Viscosity_Const_C"
                                .Liquid_Viscosity_Const_C = Double.Parse(node2.InnerText, nf)
                            Case "Liquid_Viscosity_Const_D"
                                .Liquid_Viscosity_Const_D = Double.Parse(node2.InnerText, nf)
                            Case "Liquid_Viscosity_Const_E"
                                .Liquid_Viscosity_Const_E = Double.Parse(node2.InnerText, nf)
                            Case "Normal_Boiling_Point"
                                .Normal_Boiling_Point = Double.Parse(node2.InnerText, nf)
                            Case "ID"
                                .ID = Integer.Parse(node2.InnerText)
                            Case "isPf"
                                .IsPF = Integer.Parse(node2.InnerText)
                            Case "isHYP"
                                .IsHYPO = Integer.Parse(node2.InnerText)
                            Case "HVapA"
                                .HVap_A = Double.Parse(node2.InnerText, nf)
                            Case "HVapB"
                                .HVap_B = Double.Parse(node2.InnerText, nf)
                            Case "HvapC"
                                .HVap_C = Double.Parse(node2.InnerText, nf)
                            Case "HVapD"
                                .HVap_D = Double.Parse(node2.InnerText, nf)
                            Case "HvapTmin"
                                .HVap_TMIN = Double.Parse(node2.InnerText, nf)
                            Case "HvapTMAX"
                                .HVap_TMAX = Double.Parse(node2.InnerText, nf)
                            Case "UNIQUAC_r"
                                If node2.InnerText <> "" Then .UNIQUAC_R = Double.Parse(node2.InnerText, nf)
                            Case "UNIQUAC_q"
                                If node2.InnerText <> "" Then .UNIQUAC_Q = Double.Parse(node2.InnerText, nf)
                            Case "UNIFAC"
                                .UNIFACGroups.Collection = New SortedList
                                For Each node3 As XmlNode In node2.ChildNodes
                                    .UNIFACGroups.Collection.Add(node3.Attributes("name").InnerText, Integer.Parse(node3.InnerText))
                                Next
                                .MODFACGroups.Collection = New SortedList
                                For Each node3 As XmlNode In node2.ChildNodes
                                    .MODFACGroups.Collection.Add(node3.Attributes("name").InnerText, Integer.Parse(node3.InnerText))
                                Next
                            Case "elements"
                                .Elements.Collection = New SortedList
                                For Each node3 As XmlNode In node2.ChildNodes
                                    .Elements.Collection.Add(node3.Attributes("name").InnerText, Integer.Parse(node3.InnerText))
                                Next
                            Case "COSMODBName"
                                cp.COSMODBName = node2.InnerText
                        End Select
                    Next
                End With
                cpa.Add(cp)
            Next

            Return cpa.ToArray(Type.GetType("DWSIM.DWSIM.ClassesBasicasTermodinamica.ConstantProperties"))

        End Function

    End Class

    Public Class Biodiesel

        Private xmldoc As XmlDocument

        Sub New()

        End Sub

        Public Sub Load(ByVal filename As String)
            Dim pathsep As Char = Path.DirectorySeparatorChar

            Dim settings As New XmlReaderSettings()
            settings.ConformanceLevel = ConformanceLevel.Fragment
            settings.IgnoreWhitespace = True
            settings.IgnoreComments = True
            settings.CheckCharacters = False
            Dim reader As XmlReader = XmlReader.Create(filename)
            reader.Read()

            xmldoc = New XmlDocument
            xmldoc.Load(reader)

        End Sub

        Public Function Transfer() As ClassesBasicasTermodinamica.ConstantProperties()

            Dim cp As ClassesBasicasTermodinamica.ConstantProperties
            Dim cpa As New ArrayList()
            Dim cult As Globalization.CultureInfo = New Globalization.CultureInfo("en-US")
            Dim nf As Globalization.NumberFormatInfo = cult.NumberFormat
            Dim i As Integer = 100000
            For Each node As XmlNode In xmldoc.ChildNodes(1)
                cp = New ClassesBasicasTermodinamica.ConstantProperties
                With cp
                    .OriginalDB = "Biodiesel"
                    For Each node2 As XmlNode In node.ChildNodes
                        Select Case node2.Name
                            Case "Name"
                                .Name = node2.InnerText
                                .Formula = .Name
                            Case "Molecular_Weight"
                                .Molar_Weight = Double.Parse(node2.InnerText, nf)
                            Case "Normal_Boiling_Pt_C"
                                .NBP = Double.Parse(node2.InnerText, nf) + 273.15
                                .Normal_Boiling_Point = .NBP
                            Case "Temperature_C"
                                .Critical_Temperature = Double.Parse(node2.InnerText, nf) + 273.15
                            Case "Pressure_kPa"
                                .Critical_Pressure = Double.Parse(node2.InnerText, nf) * 1000
                            Case "Volume_m3_kgmole"
                                .Critical_Volume = Double.Parse(node2.InnerText, nf)
                            Case "Acentricity"
                                .Acentric_Factor = Double.Parse(node2.InnerText, nf)
                            Case "GS_CS_-_Solubility_Parameter"
                                .Chao_Seader_Solubility_Parameter = Double.Parse(node2.InnerText, nf)
                            Case "GS_CS_-_Mol_Vol_m3_kgmole"
                                .Chao_Seader_Liquid_Molar_Volume = Double.Parse(node2.InnerText, nf)
                            Case "GS_CS_-_Acentricity"
                                .Chao_Seader_Acentricity = Double.Parse(node2.InnerText, nf)
                            Case "UNIQUAC_-_R"
                                .UNIQUAC_R = Double.Parse(node2.InnerText, nf)
                            Case "UNIQUAC_-_Q"
                                .UNIQUAC_Q = Double.Parse(node2.InnerText, nf)
                            Case "Heat_of_Form_25_C_kJ_kgmole"
                                .IG_Enthalpy_of_Formation_25C = Double.Parse(node2.InnerText, nf) / .Molar_Weight
                            Case "Ideal_Gas_Enthalpy_kJ_kg_b"
                                .Ideal_Gas_Heat_Capacity_Const_A = Double.Parse(node2.InnerText, nf)
                            Case "Ideal_Gas_Enthalpy_kJ_kg_c"
                                .Ideal_Gas_Heat_Capacity_Const_B = Double.Parse(node2.InnerText, nf)
                            Case "Ideal_Gas_Enthalpy_kJ_kg_d"
                                .Ideal_Gas_Heat_Capacity_Const_C = Double.Parse(node2.InnerText, nf)
                            Case "Ideal_Gas_Enthalpy_kJ_kg_e"
                                .Ideal_Gas_Heat_Capacity_Const_D = Double.Parse(node2.InnerText, nf)
                            Case "Ideal_Gas_Enthalpy_kJ_kg_f"
                                .Ideal_Gas_Heat_Capacity_Const_E = Double.Parse(node2.InnerText, nf)
                            Case "Vapor_Pressure_kPa_a"
                                .Vapor_Pressure_Constant_A = Double.Parse(node2.InnerText, nf)
                            Case "Vapor_Pressure_kPa_b"
                                .Vapor_Pressure_Constant_B = Double.Parse(node2.InnerText, nf)
                            Case "Vapor_Pressure_kPa_d"
                                .Vapor_Pressure_Constant_C = Double.Parse(node2.InnerText, nf)
                            Case "Vapor_Pressure_kPa_e"
                                .Vapor_Pressure_Constant_D = Double.Parse(node2.InnerText, nf)
                            Case "Vapor_Pressure_kPa_f"
                                .Vapor_Pressure_Constant_E = Double.Parse(node2.InnerText, nf)
                            Case "IG_Gibbs_Form_kJ_kmol_a"
                                .IG_Gibbs_Energy_of_Formation_25C = Double.Parse(node2.InnerText, nf)
                            Case "IG_Gibbs_Form_kJ_kmol_b"
                                .IG_Gibbs_Energy_of_Formation_25C += Double.Parse(node2.InnerText, nf) * 298.15
                                .IG_Gibbs_Energy_of_Formation_25C /= .Molar_Weight
                                .IG_Entropy_of_Formation_25C = (.IG_Gibbs_Energy_of_Formation_25C - .IG_Enthalpy_of_Formation_25C) / 298.15
                            Case "ZRa"
                                .Z_Rackett = Double.Parse(node2.InnerText, nf)
                        End Select
                    Next
                    .ID = i
                    .IsHYPO = False
                    .IsPF = False
                    .VaporPressureEquation = 101
                    .IdealgasCpEquation = 5
                    .Critical_Compressibility = 0.291 - 0.08 * .Acentric_Factor
                End With
                cpa.Add(cp)
                i += 1
            Next

            Return cpa.ToArray(Type.GetType("DWSIM.DWSIM.ClassesBasicasTermodinamica.ConstantProperties"))

        End Function

    End Class

    <System.Serializable()> Public Class UserDB

        Public Shared Sub CreateNew(ByVal path As String)

            Dim writer As New XmlTextWriter(path, Nothing)

            With writer
                .Formatting = Formatting.Indented
                .WriteStartDocument()
                .WriteStartElement("compounds")
                .WriteEndElement()
                .WriteEndDocument()
                .Flush()
                .Close()
            End With

        End Sub

        Public Shared Sub AddCompounds(ByVal comps() As ClassesBasicasTermodinamica.ConstantProperties, ByVal xmlpath As String, ByVal replace As Boolean)

            Dim xmldoc As XmlDocument
            Dim reader As XmlReader = XmlReader.Create(xmlpath)
            Try
                reader.Read()
            Catch ex As Exception
                reader.Close()
                CreateNew(xmlpath)
                reader = XmlReader.Create(xmlpath)
                reader.Read()
            End Try

            Dim cult As Globalization.CultureInfo = New Globalization.CultureInfo("en-US")

            xmldoc = New XmlDocument
            xmldoc.Load(reader)

            For Each comp As ClassesBasicasTermodinamica.ConstantProperties In comps
                Dim index As Integer = -1
                Dim i As Integer = 0
                If xmldoc.ChildNodes.Count > 0 Then
                    For Each node As XmlNode In xmldoc.ChildNodes(1)
                        For Each node2 As XmlNode In node.ChildNodes
                            If node2.Name = "ID" Then
                                If node2.InnerText = comp.ID Then
                                    index = i
                                    Exit For
                                End If
                            End If
                        Next
                        i += 1
                    Next
                End If
                If replace Then
                    If index <> -1 Then xmldoc.ChildNodes(1).RemoveChild(xmldoc.ChildNodes(1).ChildNodes(index))
                End If
                Dim newnode As XmlNode = xmldoc.CreateNode("element", "compound", "")
                With newnode
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Name", "")).InnerText = comp.Name
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "CAS_Number", "")).InnerText = comp.CAS_Number
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "ID", "")).InnerText = comp.ID
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Formula", "")).InnerText = comp.Formula
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Molar_Weight", "")).InnerText = comp.Molar_Weight.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Critical_Temperature", "")).InnerText = comp.Critical_Temperature.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Critical_Pressure", "")).InnerText = comp.Critical_Pressure.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Critical_Volume", "")).InnerText = comp.Critical_Volume.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Critical_Compressibility", "")).InnerText = comp.Critical_Compressibility.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Acentric_Factor", "")).InnerText = comp.Acentric_Factor.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Z_Rackett", "")).InnerText = comp.Z_Rackett.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "PR_Volume_Translation_Coefficient", "")).InnerText = comp.PR_Volume_Translation_Coefficient.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "SRK_Volume_Translation_Coefficient", "")).InnerText = comp.SRK_Volume_Translation_Coefficient.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "CS_Acentric_Factor", "")).InnerText = comp.Chao_Seader_Acentricity.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "CS_Solubility_Parameter", "")).InnerText = comp.Chao_Seader_Solubility_Parameter.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "CS_Liquid_Molar_Volume", "")).InnerText = comp.Chao_Seader_Liquid_Molar_Volume.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "IG_Enthalpy_of_Formation_25C", "")).InnerText = comp.IG_Enthalpy_of_Formation_25C.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "IG_Gibbs_Energy_of_Formation_25C", "")).InnerText = comp.IG_Gibbs_Energy_of_Formation_25C.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Vapor_Pressure_Constant_EqNo", "")).InnerText = comp.VaporPressureEquation
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Vapor_Pressure_Constant_A", "")).InnerText = comp.Vapor_Pressure_Constant_A.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Vapor_Pressure_Constant_B", "")).InnerText = comp.Vapor_Pressure_Constant_B.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Vapor_Pressure_Constant_C", "")).InnerText = comp.Vapor_Pressure_Constant_C.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Vapor_Pressure_Constant_D", "")).InnerText = comp.Vapor_Pressure_Constant_D.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Vapor_Pressure_Constant_E", "")).InnerText = comp.Vapor_Pressure_Constant_E.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Ideal_Gas_Heat_Capacity_EqNo", "")).InnerText = comp.IdealgasCpEquation
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Ideal_Gas_Heat_Capacity_Const_A", "")).InnerText = comp.Ideal_Gas_Heat_Capacity_Const_A.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Ideal_Gas_Heat_Capacity_Const_B", "")).InnerText = comp.Ideal_Gas_Heat_Capacity_Const_B.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Ideal_Gas_Heat_Capacity_Const_C", "")).InnerText = comp.Ideal_Gas_Heat_Capacity_Const_C.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Ideal_Gas_Heat_Capacity_Const_D", "")).InnerText = comp.Ideal_Gas_Heat_Capacity_Const_D.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Ideal_Gas_Heat_Capacity_Const_E", "")).InnerText = comp.Ideal_Gas_Heat_Capacity_Const_E.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Liquid_Viscosity_EqNo", "")).InnerText = comp.LiquidViscosityEquation
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Liquid_Viscosity_Const_A", "")).InnerText = comp.Liquid_Viscosity_Const_A.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Liquid_Viscosity_Const_B", "")).InnerText = comp.Liquid_Viscosity_Const_B.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Liquid_Viscosity_Const_C", "")).InnerText = comp.Liquid_Viscosity_Const_C.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Liquid_Viscosity_Const_D", "")).InnerText = comp.Liquid_Viscosity_Const_D.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Liquid_Viscosity_Const_E", "")).InnerText = comp.Liquid_Viscosity_Const_E.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Liquid_Density_EqNo", "")).InnerText = comp.LiquidDensityEquation
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Liquid_Density_Const_A", "")).InnerText = comp.Liquid_Density_Const_A.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Liquid_Density_Const_B", "")).InnerText = comp.Liquid_Density_Const_B.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Liquid_Density_Const_C", "")).InnerText = comp.Liquid_Density_Const_C.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Liquid_Density_Const_D", "")).InnerText = comp.Liquid_Density_Const_D.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Liquid_Density_Const_E", "")).InnerText = comp.Liquid_Density_Const_E.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "Normal_Boiling_Point", "")).InnerText = comp.Normal_Boiling_Point.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "UNIQUAC_q", "")).InnerText = comp.UNIQUAC_R.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "UNIQUAC_r", "")).InnerText = comp.UNIQUAC_Q.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "PC_SAFT_sigma", "")).InnerText = comp.PC_SAFT_sigma.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "PC_SAFT_m", "")).InnerText = comp.PC_SAFT_m.ToString(cult)
                    .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "PC_SAFT_epsilon_k", "")).InnerText = comp.PC_SAFT_epsilon_k.ToString(cult)
                    With .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "UNIFAC", ""))
                        For Each kvp As DictionaryEntry In comp.UNIFACGroups.Collection
                            .AppendChild(xmldoc.CreateNode(XmlNodeType.Element, "", "UNIFACGroup", "")).InnerText = kvp.Value
                            .ChildNodes(.ChildNodes.Count - 1).Attributes.Append(xmldoc.CreateAttribute("name"))
                            .ChildNodes(.ChildNodes.Count - 1).Attributes("name").Value = kvp.Key
                        Next
                    End With
                End With
                xmldoc.ChildNodes(1).AppendChild(newnode)
            Next

            reader.Close()
            reader = Nothing

            xmldoc.Save(xmlpath)
            xmldoc = Nothing


        End Sub

        Public Shared Sub RemoveCompound(ByVal xmlpath As String, ByVal compID As String)

            Dim xmldoc As XmlDocument
            Dim reader As XmlReader = XmlReader.Create(xmlpath)
            reader.Read()

            xmldoc = New XmlDocument
            xmldoc.Load(reader)

            Dim index As Integer = -1
            Dim i As Integer = 0
            For Each node As XmlNode In xmldoc.ChildNodes(1)
                For Each node2 As XmlNode In node.ChildNodes
                    If node2.Name = "ID" Then
                        If node2.InnerText = compID Then
                            index = i
                            Exit For
                        End If
                    End If
                Next
                i += 1
            Next

            If index <> -1 Then xmldoc.ChildNodes(1).RemoveChild(xmldoc.ChildNodes(1).ChildNodes(index))


            reader.Close()
            reader = Nothing

            xmldoc.Save(xmlpath)
            xmldoc = Nothing

        End Sub

        Public Shared Function Read(ByVal xmlpath As String) As ClassesBasicasTermodinamica.ConstantProperties()

            Dim xmldoc As XmlDocument
            Dim reader As XmlReader = XmlReader.Create(xmlpath)
            reader.Read()

            xmldoc = New XmlDocument
            xmldoc.Load(reader)

            Dim cp As ClassesBasicasTermodinamica.ConstantProperties
            Dim cpa As New ArrayList()
            Dim cult As Globalization.CultureInfo = New Globalization.CultureInfo("en-US")
            Dim nf As Globalization.NumberFormatInfo = cult.NumberFormat

            For Each node As XmlNode In xmldoc.ChildNodes(1)
                cp = New ClassesBasicasTermodinamica.ConstantProperties
                With cp
                    .OriginalDB = "User"
                    .CurrentDB = "User"
                    For Each node2 As XmlNode In node.ChildNodes
                        Select Case node2.Name
                            Case "Name"
                                .Name = node2.InnerText
                            Case "CAS_Number"
                                .CAS_Number = node2.InnerText
                            Case "Formula"
                                .Formula = node2.InnerText
                            Case "Molar_Weight"
                                .Molar_Weight = Double.Parse(node2.InnerText, nf)
                            Case "Critical_Temperature"
                                .Critical_Temperature = Double.Parse(node2.InnerText, nf)
                            Case "Critical_Pressure"
                                .Critical_Pressure = Double.Parse(node2.InnerText, nf)
                            Case "Critical_Volume"
                                .Critical_Volume = Double.Parse(node2.InnerText, nf)
                            Case "Critical_Compressibility"
                                .Critical_Compressibility = Double.Parse(node2.InnerText, nf)
                            Case "Acentric_Factor"
                                .Acentric_Factor = Double.Parse(node2.InnerText, nf)
                            Case "Z_Rackett"
                                .Z_Rackett = Double.Parse(node2.InnerText, nf)
                            Case "PR_Volume_Translation_Coefficient"
                                .PR_Volume_Translation_Coefficient = Double.Parse(node2.InnerText, nf)
                            Case "SRK_Volume_Translation_Coefficient"
                                .SRK_Volume_Translation_Coefficient = Double.Parse(node2.InnerText, nf)
                            Case "CS_Acentric_Factor"
                                .Chao_Seader_Acentricity = Double.Parse(node2.InnerText, nf)
                            Case "CS_Solubility_Parameter"
                                .Chao_Seader_Solubility_Parameter = Double.Parse(node2.InnerText, nf)
                            Case "CS_Liquid_Molar_Volume"
                                .Chao_Seader_Liquid_Molar_Volume = Double.Parse(node2.InnerText, nf)
                            Case "IG_Enthalpy_of_Formation_25C"
                                .IG_Enthalpy_of_Formation_25C = Double.Parse(node2.InnerText, nf)
                            Case "IG_Gibbs_Energy_of_Formation_25C"
                                .IG_Gibbs_Energy_of_Formation_25C = Double.Parse(node2.InnerText, nf)
                            Case "Vapor_Pressure_Constant_EqNo"
                                .VaporPressureEquation = Integer.Parse(node2.InnerText, nf)
                            Case "Vapor_Pressure_Constant_A"
                                .Vapor_Pressure_Constant_A = Double.Parse(node2.InnerText, nf)
                            Case "Vapor_Pressure_Constant_B"
                                .Vapor_Pressure_Constant_B = Double.Parse(node2.InnerText, nf)
                            Case "Vapor_Pressure_Constant_C"
                                .Vapor_Pressure_Constant_C = Double.Parse(node2.InnerText, nf)
                            Case "Vapor_Pressure_Constant_D"
                                .Vapor_Pressure_Constant_D = Double.Parse(node2.InnerText, nf)
                            Case "Vapor_Pressure_Constant_E"
                                .Vapor_Pressure_Constant_E = Double.Parse(node2.InnerText, nf)
                            Case "Ideal_Gas_Heat_Capacity_EqNo"
                                .IdealgasCpEquation = Integer.Parse(node2.InnerText, nf)
                            Case "Ideal_Gas_Heat_Capacity_Const_A"
                                .Ideal_Gas_Heat_Capacity_Const_A = Double.Parse(node2.InnerText, nf)
                            Case "Ideal_Gas_Heat_Capacity_Const_B"
                                .Ideal_Gas_Heat_Capacity_Const_B = Double.Parse(node2.InnerText, nf)
                            Case "Ideal_Gas_Heat_Capacity_Const_C"
                                .Ideal_Gas_Heat_Capacity_Const_C = Double.Parse(node2.InnerText, nf)
                            Case "Ideal_Gas_Heat_Capacity_Const_D"
                                .Ideal_Gas_Heat_Capacity_Const_D = Double.Parse(node2.InnerText, nf)
                            Case "Ideal_Gas_Heat_Capacity_Const_E"
                                .Ideal_Gas_Heat_Capacity_Const_E = Double.Parse(node2.InnerText, nf)
                            Case "Liquid_Viscosity_Const_EqNo"
                                .LiquidViscosityEquation = Integer.Parse(node2.InnerText, nf)
                            Case "Liquid_Viscosity_Const_A"
                                .Liquid_Viscosity_Const_A = Double.Parse(node2.InnerText, nf)
                            Case "Liquid_Viscosity_Const_B"
                                .Liquid_Viscosity_Const_B = Double.Parse(node2.InnerText, nf)
                            Case "Liquid_Viscosity_Const_C"
                                .Liquid_Viscosity_Const_C = Double.Parse(node2.InnerText, nf)
                            Case "Liquid_Viscosity_Const_D"
                                .Liquid_Viscosity_Const_D = Double.Parse(node2.InnerText, nf)
                            Case "Liquid_Viscosity_Const_E"
                                .Liquid_Viscosity_Const_E = Double.Parse(node2.InnerText, nf)
                            Case "Liquid_Density_Const_EqNo"
                                .LiquidDensityEquation = Integer.Parse(node2.InnerText, nf)
                            Case "Liquid_Density_Const_A"
                                .Liquid_Density_Const_A = Double.Parse(node2.InnerText, nf)
                            Case "Liquid_Density_Const_B"
                                .Liquid_Density_Const_B = Double.Parse(node2.InnerText, nf)
                            Case "Liquid_Density_Const_C"
                                .Liquid_Density_Const_C = Double.Parse(node2.InnerText, nf)
                            Case "Liquid_Density_Const_D"
                                .Liquid_Density_Const_D = Double.Parse(node2.InnerText, nf)
                            Case "Liquid_Density_Const_E"
                                .Liquid_Density_Const_E = Double.Parse(node2.InnerText, nf)
                            Case "Normal_Boiling_Point"
                                .Normal_Boiling_Point = Double.Parse(node2.InnerText, nf)
                            Case "ID"
                                .ID = Integer.Parse(node2.InnerText)
                            Case "UNIQUAC_r"
                                If node2.InnerText <> "" Then .UNIQUAC_R = Double.Parse(node2.InnerText, nf)
                            Case "UNIQUAC_q"
                                If node2.InnerText <> "" Then .UNIQUAC_Q = Double.Parse(node2.InnerText, nf)
                            Case "UNIFAC"
                                .UNIFACGroups.Collection = New SortedList
                                For Each node3 As XmlNode In node2.ChildNodes
                                    .UNIFACGroups.Collection.Add(node3.Attributes("name").InnerText, Integer.Parse(node3.InnerText))
                                Next
                                .MODFACGroups.Collection = New SortedList
                                For Each node3 As XmlNode In node2.ChildNodes
                                    .MODFACGroups.Collection.Add(node3.Attributes("name").InnerText, Integer.Parse(node3.InnerText))
                                Next
                            Case "PC_SAFT_sigma"
                                .PC_SAFT_sigma = Double.Parse(node2.InnerText, nf)
                            Case "PC_SAFT_m"
                                .PC_SAFT_m = Double.Parse(node2.InnerText, nf)
                            Case "PC_SAFT_epsilon_k"
                                .PC_SAFT_epsilon_k = Double.Parse(node2.InnerText, nf)
                        End Select
                    Next
                End With
                cpa.Add(cp)
            Next

            xmldoc = Nothing

            reader.Close()
            reader = Nothing

            Return cpa.ToArray(Type.GetType("DWSIM.DWSIM.ClassesBasicasTermodinamica.ConstantProperties"))

        End Function

    End Class

    Public Class Electrolyte

        Private xmldoc As XmlDocument

        Sub New()

        End Sub

        Public Sub Load(ByVal filename As String)
            Dim pathsep As Char = Path.DirectorySeparatorChar

            Dim settings As New XmlReaderSettings()
            settings.ConformanceLevel = ConformanceLevel.Fragment
            settings.IgnoreWhitespace = True
            settings.IgnoreComments = True
            settings.CheckCharacters = False
            Dim reader As XmlReader = XmlReader.Create(filename)
            reader.Read()

            xmldoc = New XmlDocument
            xmldoc.Load(reader)

        End Sub

        Public Function Transfer() As ClassesBasicasTermodinamica.ConstantProperties()

            Dim cp As ClassesBasicasTermodinamica.ConstantProperties
            Dim cpa As New ArrayList()
            Dim cult As Globalization.CultureInfo = New Globalization.CultureInfo("en-US")
            Dim nf As Globalization.NumberFormatInfo = cult.NumberFormat
            Dim i As Integer = 200000
            For Each node As XmlNode In xmldoc.ChildNodes(1)
                cp = New ClassesBasicasTermodinamica.ConstantProperties
                With cp
                    .OriginalDB = "Electrolytes"
                    For Each node2 As XmlNode In node.ChildNodes
                        Select Case node2.Name
                            Case "Name"
                                .Name = node2.InnerText
                            Case "Formula"
                                .Formula = node2.InnerText
                            Case "MW"
                                .Molar_Weight = Double.Parse(node2.InnerText, nf)
                            Case "Ion"
                                .IsIon = node2.InnerText
                            Case "Salt"
                                .IsSalt = node2.InnerText
                            Case "HydratedSalt"
                                .IsHydratedSalt = node2.InnerText
                            Case "HydrationNumber"
                                .HydrationNumber = node2.InnerText
                            Case "Charge"
                                .Charge = node2.InnerText
                            Case "PositiveIon"
                                .PositiveIon = node2.InnerText
                            Case "NegativeIon"
                                .NegativeIon = node2.InnerText
                            Case "PositiveIonStoichCoeff"
                                .PositiveIonStoichCoeff = node2.InnerText
                            Case "NegativeIonStoichCoeff"
                                .NegativeIonStoichCoeff = node2.InnerText
                            Case "StoichSum"
                                .StoichSum = node2.InnerText
                            Case "DelGF_kJ_mol"
                                .Electrolyte_DelGF = Double.Parse(node2.InnerText, nf) 'kJ/mol
                                .IG_Gibbs_Energy_of_Formation_25C = Double.Parse(node2.InnerText, nf) * 1000 / .Molar_Weight 'kJ/kg
                            Case "DelHf_kJ_mol"
                                .Electrolyte_DelHF = Double.Parse(node2.InnerText, nf) 'kJ/mol
                                .IG_Enthalpy_of_Formation_25C = Double.Parse(node2.InnerText, nf) * 1000 / .Molar_Weight 'kJ/kg
                            Case "Cp_J_mol_K"
                                .Electrolyte_Cp0 = Double.Parse(node2.InnerText, nf) / 1000 'kJ/mol.K
                            Case "Tf_C"
                                .TemperatureOfFusion = Double.Parse(node2.InnerText, nf) + 273.15 'K
                            Case "Hfus_at_Tf_kJ_mol"
                                .EnthalpyOfFusionAtTf = Double.Parse(node2.InnerText, nf) 'kJ/mol
                            Case "DenS_T_C"
                                .SolidTs = Double.Parse(node2.InnerText, nf) + 273.15 'K
                            Case "DenS_g_mL"
                                .SolidDensityAtTs = Double.Parse(node2.InnerText, nf) * 1000 'kg/m3
                        End Select
                    Next
                    .ID = i
                    .IsHYPO = False
                    .IsPF = False
                    .VaporPressureEquation = 101
                    .IdealgasCpEquation = 5
                End With
                cpa.Add(cp)
                i += 1
            Next

            Return cpa.ToArray(Type.GetType("DWSIM.DWSIM.ClassesBasicasTermodinamica.ConstantProperties"))

        End Function

    End Class

End Namespace
