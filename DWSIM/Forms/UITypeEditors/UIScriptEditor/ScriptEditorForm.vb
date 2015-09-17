'    UITypeEditor for Custom UO Script
'    Copyright 2010 Daniel Wagner O. de Medeiros
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

Imports System.IO
Imports System.Reflection
Imports System.Drawing.Text
Imports ScintillaNET
Imports System.Xml.Linq
Imports System.Linq

<System.Serializable()> Public Class ScriptEditorForm

    Public scripttext As String
    Public language As Integer
    Public includes As String()
    Public fontname As String = "Courier New"
    Public fontsize As Integer = 10
    Public highlighttabs As Boolean = False
    Public highlightspaces As Boolean = False

    Private reader As Jolt.XmlDocCommentReader

    '0 = VBScript
    '1 = JScript
    '2 = IronPython
    '3 = IronRuby

#Region "Custom members"
    Private maxLineNumberCharLength As Integer
    Private loaded As Boolean = False
#End Region

    Private Sub ScriptEditorForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        scripttext = txtScript.Text
    End Sub

    Private Sub ScriptEditorForm_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        reader = New Jolt.XmlDocCommentReader(Assembly.GetExecutingAssembly())

        Me.txtScript.Text = scripttext

        Me.ListBox1.Items.Clear()
        If Not includes Is Nothing Then
            For Each i As String In includes
                Me.ListBox1.Items.Add(i)
            Next
        End If

        ' Get the installed fonts collection.
        Dim installed_fonts As New InstalledFontCollection
        ' Get an array of the system's font familiies.
        Dim font_families() As FontFamily = installed_fonts.Families()
        ' Display the font families.
        For Each font_family As FontFamily In font_families
            tscb1.Items.Add(font_family.Name)
        Next font_family

        tscb1.SelectedItem = fontname

        tscb2.Items.AddRange(New Object() {6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16})

        tscb2.SelectedItem = fontsize

        btnHighlightSpaces.Checked = highlightspaces
        btnHighlightTabs.Checked = highlighttabs

        SetEditorStyle()

        loaded = True

    End Sub

    Private Sub OpenToolStripButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OpenToolStripButton.Click
        If Me.txtScript.Text <> "" Then
            If MessageBox.Show(DWSIM.App.GetLocalString("DesejaSalvaroScriptAtual"), DWSIM.App.GetLocalString("Ateno"), MessageBoxButtons.YesNo) = Windows.Forms.DialogResult.Yes Then
                SaveToolStripButton_Click(sender, e)
            End If
        End If
        If Me.ofd2.ShowDialog = Windows.Forms.DialogResult.OK Then
            Me.txtScript.Text = ""
            For Each fname As String In Me.ofd2.FileNames
                Me.txtScript.Text += File.ReadAllText(fname) & vbCrLf
            Next
        End If
    End Sub

    Private Sub SaveToolStripButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SaveToolStripButton.Click
        If Me.sfd1.ShowDialog = Windows.Forms.DialogResult.OK Then
            My.Computer.FileSystem.WriteAllText(Me.sfd1.FileName, Me.txtScript.Text, False)
        End If
    End Sub

    Private Sub PrintToolStripButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PrintToolStripButton.Click
        'Dim pd As Alsing.SourceCode.SourceCodePrintDocument
        'pd = New Alsing.SourceCode.SourceCodePrintDocument(txtScript.Document)
        'pd1.Document = pd
        'If pd1.ShowDialog(Me) = DialogResult.OK Then
        '    pd.Print()
        'End If
    End Sub

    Private Sub CutToolStripButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CutToolStripButton.Click
        txtScript.Cut()
    End Sub

    Private Sub CopyToolStripButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CopyToolStripButton.Click
        If txtScript.SelectedText <> "" Then Clipboard.SetText(txtScript.SelectedText)
    End Sub

    Private Sub PasteToolStripButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PasteToolStripButton.Click
        txtScript.Paste()
    End Sub

    Private Sub tscb1_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles tscb1.SelectedIndexChanged
        If loaded Then SetEditorStyle()
   End Sub

    Private Sub tscb2_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles tscb2.SelectedIndexChanged
        If loaded Then SetEditorStyle()
    End Sub

    Private Sub ToolStripButton2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton2.Click
        If Me.ofd1.ShowDialog = Windows.Forms.DialogResult.OK Then
            For Each fname As String In Me.ofd1.FileNames
                Me.ListBox1.Items.Add(fname)
            Next
            ReDim includes(Me.ListBox1.Items.Count - 1)
            Dim i As Integer = 0
            For Each item As Object In Me.ListBox1.Items
                includes(i) = item.ToString
                i += 1
            Next
        End If
    End Sub

    Private Sub ToolStripButton3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton3.Click
        If Me.ListBox1.SelectedItems.Count > 0 Then
            Dim names As New ArrayList
            For Each fname As Object In Me.ListBox1.SelectedItems
                names.Add(fname)
            Next
            For Each fname As String In names
                Me.ListBox1.Items.Remove(fname)
            Next
            names = Nothing
            ReDim includes(Me.ListBox1.Items.Count - 1)
            Dim i As Integer = 0
            For Each item As Object In Me.ListBox1.Items
                includes(i) = item.ToString
                i += 1
            Next
        End If
    End Sub

    Private Sub ToolStripButton4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton4.Click
        If Me.Opacity < 1.0# Then Me.Opacity += 0.05
    End Sub

    Private Sub ToolStripButton5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton5.Click
        If Me.Opacity > 0.1# Then Me.Opacity -= 0.05
    End Sub

    Private Sub APIHelptsbutton_Click(sender As Object, e As EventArgs) Handles APIHelptsbutton.Click
        Process.Start("http://dwsim.inforside.com.br/api_help/index.html")
    End Sub

    Private Sub HelpToolStripButton_Click(sender As Object, e As EventArgs) Handles HelpToolStripButton.Click
        Process.Start("http://dwsim.inforside.com.br/wiki/index.php?title=Using_the_IronPython_Script_Manager")
    End Sub

    Private Sub btnDebug_Click(sender As Object, e As EventArgs) Handles btnDebug.Click
        Dim mycuo As DWSIM.SimulationObjects.UnitOps.CustomUO = My.Application.ActiveSimulation.Collections.ObjectCollection(My.Application.ActiveSimulation.FormSurface.FlowsheetDesignSurface.SelectedObject.Name)
        mycuo.Includes = includes
        mycuo.ScriptText = Me.txtScript.Text
        DWSIM.Flowsheet.FlowsheetSolver.CalculateObject(My.Application.ActiveSimulation, mycuo.Name)
    End Sub

    Private Function getLastWord() As String

        Dim word As String = ""

        Dim pos As Integer = Me.txtScript.SelectionStart
        If pos > 1 Then

            Dim tmp As String = ""
            Dim f As New Char()
            While f <> " " And pos > 0
                pos -= 1
                tmp = Me.txtScript.Text.Substring(pos, 1)
                f = CChar(tmp(0))
                word += f
            End While

            Dim ca As Char() = word.ToCharArray()
            Array.Reverse(ca)

            word = New [String](ca)
        End If
        Return word.Trim()

    End Function

    Private Sub txtScript__KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtScript.KeyDown

        Dim i As Integer = Me.txtScript.SelectionStart
        Dim currentChar As String = ""

        If i > 0 Then
            currentChar = Me.txtScript.Text.Substring(i - 1, 1)
        End If

        If e.KeyData = Keys.OemPeriod Then

            ' The amazing dot key
            Dim word As String = Me.getLastWord()

        ElseIf e.KeyValue = Keys.F5 Then

            btnDebug_Click(sender, e)

        Else

        End If
    End Sub

    Sub SetEditorStyle()

        Dim scintilla = txtScript

        ' Reset the styles
        scintilla.StyleResetDefault()
        scintilla.Styles(Style.[Default]).Font = tscb1.SelectedItem.ToString
        scintilla.Styles(Style.[Default]).Size = tscb2.SelectedItem.ToString
        scintilla.StyleClearAll()
        ' i.e. Apply to all
        ' Set the lexer
        scintilla.Lexer = Lexer.Python

        ' Known lexer properties:
        ' "tab.timmy.whinge.level",
        ' "lexer.python.literals.binary",
        ' "lexer.python.strings.u",
        ' "lexer.python.strings.b",
        ' "lexer.python.strings.over.newline",
        ' "lexer.python.keywords2.no.sub.identifiers",
        ' "fold.quotes.python",
        ' "fold.compact",
        ' "fold"

        ' Some properties we like
        scintilla.SetProperty("tab.timmy.whinge.level", "1")
        scintilla.SetProperty("fold", "1")

        scintilla.Margins(0).Width = 20

        ' Use margin 2 for fold markers
        scintilla.Margins(1).Type = MarginType.Symbol
        scintilla.Margins(1).Mask = Marker.MaskFolders
        scintilla.Margins(1).Sensitive = True
        scintilla.Margins(1).Width = 20

        ' Reset folder markers
        For i As Integer = Marker.FolderEnd To Marker.FolderOpen
            scintilla.Markers(i).SetForeColor(SystemColors.ControlLightLight)
            scintilla.Markers(i).SetBackColor(SystemColors.ControlDark)
        Next

        ' Style the folder markers
        scintilla.Markers(Marker.Folder).Symbol = MarkerSymbol.BoxPlus
        scintilla.Markers(Marker.Folder).SetBackColor(SystemColors.ControlText)
        scintilla.Markers(Marker.FolderOpen).Symbol = MarkerSymbol.BoxMinus
        scintilla.Markers(Marker.FolderEnd).Symbol = MarkerSymbol.BoxPlusConnected
        scintilla.Markers(Marker.FolderEnd).SetBackColor(SystemColors.ControlText)
        scintilla.Markers(Marker.FolderMidTail).Symbol = MarkerSymbol.TCorner
        scintilla.Markers(Marker.FolderOpenMid).Symbol = MarkerSymbol.BoxMinusConnected
        scintilla.Markers(Marker.FolderSub).Symbol = MarkerSymbol.VLine
        scintilla.Markers(Marker.FolderTail).Symbol = MarkerSymbol.LCorner

        ' Enable automatic folding
        scintilla.AutomaticFold = (AutomaticFold.Show Or AutomaticFold.Click Or AutomaticFold.Change)

        ' Set the styles
        scintilla.Styles(Style.Python.[Default]).ForeColor = Color.FromArgb(&H80, &H80, &H80)
        scintilla.Styles(Style.Python.CommentLine).ForeColor = Color.FromArgb(&H0, &H7F, &H0)
        scintilla.Styles(Style.Python.CommentLine).Italic = True
        scintilla.Styles(Style.Python.Number).ForeColor = Color.FromArgb(&H0, &H7F, &H7F)
        scintilla.Styles(Style.Python.[String]).ForeColor = Color.FromArgb(&H7F, &H0, &H7F)
        scintilla.Styles(Style.Python.Character).ForeColor = Color.FromArgb(&H7F, &H0, &H7F)
        scintilla.Styles(Style.Python.Word).ForeColor = Color.FromArgb(&H0, &H0, &H7F)
        scintilla.Styles(Style.Python.Word).Bold = True
        scintilla.Styles(Style.Python.Triple).ForeColor = Color.FromArgb(&H7F, &H0, &H0)
        scintilla.Styles(Style.Python.TripleDouble).ForeColor = Color.FromArgb(&H7F, &H0, &H0)
        scintilla.Styles(Style.Python.ClassName).ForeColor = Color.FromArgb(&H0, &H0, &HFF)
        scintilla.Styles(Style.Python.ClassName).Bold = True
        scintilla.Styles(Style.Python.DefName).ForeColor = Color.FromArgb(&H0, &H7F, &H7F)
        scintilla.Styles(Style.Python.DefName).Bold = True
        scintilla.Styles(Style.Python.[Operator]).Bold = True
        scintilla.Styles(Style.Python.CommentBlock).ForeColor = Color.FromArgb(&H7F, &H7F, &H7F)
        scintilla.Styles(Style.Python.CommentBlock).Italic = True
        scintilla.Styles(Style.Python.StringEol).ForeColor = Color.FromArgb(&H0, &H0, &H0)
        scintilla.Styles(Style.Python.StringEol).BackColor = Color.FromArgb(&HE0, &HC0, &HE0)
        scintilla.Styles(Style.Python.StringEol).FillLine = True

        scintilla.Styles(Style.Python.DefName).ForeColor = Color.Brown
        scintilla.Styles(Style.Python.DefName).Bold = True

        scintilla.Styles(Style.Python.Word2).ForeColor = Color.DarkRed
        scintilla.Styles(Style.Python.Word2).Bold = True

        With scintilla.Styles(Style.CallTip)
            .Font = tscb1.SelectedItem.ToString
            .Size = Integer.Parse(tscb2.SelectedItem.ToString) - 2
            .ForeColor = Color.FromKnownColor(KnownColor.ActiveCaptionText)
        End With

        ' Important for Python
        scintilla.ViewWhitespace = btnHighlightSpaces.Checked
        If btnHighlightTabs.Checked Then scintilla.IndentationGuides = IndentView.LookForward Else scintilla.IndentationGuides = IndentView.None

        ' Keyword lists:
        ' 0 "Keywords",
        ' 1 "Highlighted identifiers"

        Dim python2 = "and as assert break class continue def del elif else except exec finally for from global if import in is lambda not or pass print raise return try while with yield"
        Dim python3 = "False None True and as assert break class continue def del elif else except finally for from global if import in is lambda nonlocal not or pass raise return try while with yield"

        Dim netprops As String = ""

        Dim props = Type.GetType("DWSIM.DWSIM.SimulationObjects.Streams.MaterialStream").GetProperties()
        For Each p In props
            netprops += p.Name + " "
        Next
        Dim methods = Type.GetType("DWSIM.DWSIM.SimulationObjects.Streams.MaterialStream").GetMethods()
        For Each m In methods
            netprops += m.Name + " "
        Next
        props = Type.GetType("DWSIM.DWSIM.SimulationObjects.Streams.EnergyStream").GetProperties()
        For Each p In props
            netprops += p.Name + " "
        Next
        methods = Type.GetType("DWSIM.DWSIM.SimulationObjects.Streams.EnergyStream").GetMethods()
        For Each m In methods
            netprops += m.Name + " "
        Next
        props = Type.GetType("DWSIM.FormFlowsheet").GetProperties()
        For Each p In props
            If p.PropertyType.Namespace <> "System.Windows.Forms" Then netprops += p.Name + " "
        Next
        methods = Type.GetType("DWSIM.FormFlowsheet").GetMethods()
        For Each m In methods
            netprops += m.Name + " "
        Next
        props = Type.GetType("DWSIM.SpreadsheetForm").GetProperties()
        For Each p In props
            If p.PropertyType.Namespace <> "System.Windows.Forms" Then netprops += p.Name + " "
        Next
        methods = Type.GetType("DWSIM.SpreadsheetForm").GetMethods()
        For Each m In methods
            netprops += m.Name + " "
        Next

        Dim objects As String = "ims1 ims2 ims3 ims4 ims5 ims6 ies1 oms1 oms2 oms3 oms4 oms5 oms6 oes1 Flowsheet Spreadsheet Plugins Solver Me DWSIM"

        scintilla.SetKeywords(0, python2 + " " + python3)
        scintilla.SetKeywords(1, objects + " " + netprops)

        SetColumnMargins()

    End Sub

    Sub SetColumnMargins()

        ' Did the number of characters in the line number display change?
        ' i.e. nnn VS nn, or nnnn VS nn, etc...
        Dim maxLineNumberCharLength = txtScript.Lines.Count.ToString().Length

        ' Calculate the width required to display the last line number
        ' and include some padding for good measure.
        Const padding As Integer = 2
        txtScript.Margins(0).Width = txtScript.TextWidth(Style.LineNumber, New String("9"c, maxLineNumberCharLength + 1)) + padding
        Me.maxLineNumberCharLength = maxLineNumberCharLength

    End Sub

    Private Sub txtScript_TextChanged(sender As Object, e As EventArgs) Handles txtScript.TextChanged

        SetColumnMargins()

        btnUndo.Enabled = txtScript.CanUndo
        btnRedo.Enabled = txtScript.CanRedo

        ShowAutoComplete()

        ShowToolTip()

    End Sub

    Private Sub ToolStripButton1_Click(sender As Object, e As EventArgs) Handles btnUndo.Click
        txtScript.Undo()
    End Sub

    Private Sub btnRedo_Click(sender As Object, e As EventArgs) Handles btnRedo.Click
        txtScript.Redo()
    End Sub

    Private Sub btnHighlightSpaces_Click(sender As Object, e As EventArgs) Handles btnHighlightSpaces.CheckedChanged, btnHighlightTabs.CheckedChanged
        highlightspaces = btnHighlightSpaces.Checked
        highlighttabs = btnHighlightTabs.Checked
        If loaded Then SetEditorStyle()
    End Sub

    Sub ShowAutoComplete()

        Dim suggestions As String = ""

        Dim text = getLastWord().Split({".", "(", ")"}, StringSplitOptions.RemoveEmptyEntries)
        Dim lastchar = Chr(txtScript.GetCharAt(txtScript.CurrentPosition - 1))

        If text.Length >= 1 Then
            Dim lastkeyword As String = ""
            If text.Length >= 2 Then
                lastkeyword = text(text.Length - 2)
            Else
                lastkeyword = text(text.Length - 1)
            End If
            Select Case lastkeyword
                Case "ims1", "ims2", "ims3", "ims4", "ims5", "ims6", "oms1", "oms2", "oms3", "oms4", "oms5"
                    Dim props = Type.GetType("DWSIM.DWSIM.SimulationObjects.Streams.MaterialStream").GetProperties()
                    For Each p In props
                        suggestions += (p.Name) + " "
                    Next
                    Dim methods = Type.GetType("DWSIM.DWSIM.SimulationObjects.Streams.MaterialStream").GetMethods()
                    For Each m In methods
                        suggestions += (m.Name) + " "
                    Next
                Case "ies1", "oes1"
                    Dim props = Type.GetType("DWSIM.DWSIM.SimulationObjects.Streams.EnergyStream").GetProperties()
                    For Each p In props
                        suggestions += (p.Name) + " "
                    Next
                    Dim methods = Type.GetType("DWSIM.DWSIM.SimulationObjects.Streams.EnergyStream").GetMethods()
                    For Each m In methods
                        suggestions += (m.Name) + " "
                    Next
                Case "Flowsheet"
                    Dim props = Type.GetType("DWSIM.FormFlowsheet").GetProperties()
                    For Each p In props
                        If p.PropertyType.Namespace <> "System.Windows.Forms" Then suggestions += (p.Name) + " "
                    Next
                    Dim methods = Type.GetType("DWSIM.FormFlowsheet").GetMethods()
                    For Each m In methods
                        suggestions += (m.Name) + " "
                    Next
                Case "Spreadsheet"
                    Dim props = Type.GetType("DWSIM.SpreadsheetForm").GetProperties()
                    For Each p In props
                        If p.PropertyType.Namespace <> "System.Windows.Forms" Then suggestions += (p.Name) + " "
                    Next
                    Dim methods = Type.GetType("DWSIM.SpreadsheetForm").GetMethods()
                    For Each m In methods
                        suggestions += (m.Name) + " "
                    Next
                Case "PropertyPackage"
                    Dim props = Type.GetType("DWSIM.DWSIM.SimulationObjects.PropertyPackages.PropertyPackage").GetProperties()
                    For Each p In props
                        suggestions += (p.Name) + " "
                    Next
                    Dim methods = Type.GetType("DWSIM.DWSIM.SimulationObjects.PropertyPackages.PropertyPackage").GetMethods()
                    For Each m In methods
                        suggestions += (m.Name) + " "
                    Next
                Case Else
                    Exit Sub
            End Select
        Else
            suggestions = "ims1 ims2 ims3 ims4 ims5 ims6 ies1 oms1 oms2 oms3 oms4 oms5 oms6 oes1 Flowsheet Spreadsheet Plugins Solver Me DWSIM"
        End If

        Dim currentPos = txtScript.CurrentPosition
        Dim wordStartPos = txtScript.WordStartPosition(currentPos, True)

        ' Display the autocompletion list
        Dim lenEntered = currentPos - wordStartPos
      
        txtScript.AutoCShow(lenEntered, suggestions)

    End Sub

    Sub ShowToolTip()

        Dim text = getLastWord().Split({".", "(", ")"}, StringSplitOptions.RemoveEmptyEntries)
        Dim lastchar = Chr(txtScript.GetCharAt(txtScript.CurrentPosition))

        Dim helptext As String = ""

        If text.Length >= 2 Then
            Dim lastkeyword = text(text.Length - 1)
            Dim lastobj = text(text.Length - 2)
            Select Case lastobj
                Case "ims1", "ims2", "ims3", "ims4", "ims5", "ims6", "oms1", "oms2", "oms3", "oms4", "oms5"
                    Dim prop = Type.GetType("DWSIM.DWSIM.SimulationObjects.Streams.MaterialStream").GetMember(lastkeyword)
                    If prop.Length > 0 Then helptext = FormatHelpTip(prop(0))
                Case "ies1", "oes1"
                    Dim prop = Type.GetType("DWSIM.DWSIM.SimulationObjects.Streams.EnergyStream").GetMember(lastkeyword)
                    If prop.Length > 0 Then helptext = FormatHelpTip(prop(0))
                Case "Flowsheet"
                    Dim prop = Type.GetType("DWSIM.FormFlowsheet").GetMember(lastkeyword)
                    If prop.Length > 0 Then helptext = FormatHelpTip(prop(0))
                Case "Spreadsheet"
                    Dim prop = Type.GetType("DWSIM.SpreadsheetForm").GetMember(lastkeyword)
                    If prop.Length > 0 Then helptext = FormatHelpTip(prop(0))
                Case "PropertyPackage"
                    Dim prop = Type.GetType("DWSIM.DWSIM.DWSIM.SimulationObjects.PropertyPackages.PropertyPackage").GetMember(lastkeyword)
                    If prop.Length > 0 Then helptext = FormatHelpTip(prop(0))
            End Select

            If helptext <> "" Then txtScript.CallTipShow(txtScript.CurrentPosition, helptext) Else txtScript.CallTipCancel()

        Else

            txtScript.CallTipCancel()

        End If

    End Sub

    Function FormatHelpTip(member As MemberInfo) As String

        Select Case member.MemberType

            Case MemberTypes.Method

                Dim method = Type.GetType(member.DeclaringType.FullName).GetMethod(member.Name)

                Dim summary As String = ""
                Dim returntype As String = ""
                Dim returndescription As String = ""
                Dim remarks As String = ""

                Dim argumentdescriptions As New Dictionary(Of String, String)

                Dim xmlhelp = reader.GetComments(method)

                If Not xmlhelp Is Nothing Then
                    summary = xmlhelp.Elements("summary").FirstOrDefault.Value
                    Dim params = xmlhelp.Elements("param").ToList
                    For Each p In params
                        If p.Value.ToString.Length > 70 Then
                            argumentdescriptions.Add(p.Attribute("name"), p.Value.ToString.Substring(0, 70).Trim(vbLf) & " [...]")
                        Else
                            argumentdescriptions.Add(p.Attribute("name"), p.Value.ToString.Trim(vbLf))
                        End If
                    Next
                    If method.ReturnType.Name <> "Void" Then
                        Dim rdesc = xmlhelp.Elements("returns").FirstOrDefault
                        If Not rdesc Is Nothing Then
                            returndescription = rdesc.Value
                        End If
                    End If
                    Dim redesc = xmlhelp.Elements("remarks").FirstOrDefault
                    If Not redesc Is Nothing Then
                        If redesc.Value.Length > 1000 Then
                            remarks = redesc.Value.Substring(0, 1000) & " [...]"
                        Else
                            remarks = redesc.Value
                        End If
                    End If
                End If

                Dim txthelp As String = "Method '" & member.Name & "'" & vbCrLf & vbCrLf

                If method.GetParameters.Count > 0 Then
                    txthelp += "Parameters:" & vbCrLf & vbCrLf
                    For Each par In method.GetParameters
                        If argumentdescriptions.ContainsKey(par.Name) Then
                            txthelp += par.ParameterType.ToString.PadRight(18) & par.Name.PadRight(15) & argumentdescriptions(par.Name) & vbCrLf
                        Else
                            txthelp += par.ParameterType.ToString.PadRight(18) & par.Name.PadRight(15) & vbCrLf
                        End If
                    Next
                    txthelp += vbCrLf
                End If

                txthelp += "Return Type: " & method.ReturnType.ToString
                If returndescription <> "" Then txthelp += vbCrLf & "Return Parameter Description: " & returndescription
                If remarks <> "" Then txthelp += vbCrLf & vbCrLf & "Remarks: " & remarks

                Return txthelp

            Case MemberTypes.Property

                Dim prop = Type.GetType(member.DeclaringType.FullName).GetProperty(member.Name)

                Dim summary As String = ""
                Dim proptype As String = ""

                Dim txthelp As String = "Property '" & prop.Name & "'" & vbCrLf
                txthelp += "Type: " & prop.PropertyType.ToString

                Dim xmlhelp = reader.GetComments(prop)

                If Not xmlhelp Is Nothing Then
                    Dim redesc = xmlhelp.Elements("summary").FirstOrDefault
                    If Not redesc Is Nothing Then
                        txthelp += vbCrLf & "Description: " & redesc.Value
                    End If
                End If

                Return txthelp

            Case Else

                Return ""

        End Select

    End Function

End Class