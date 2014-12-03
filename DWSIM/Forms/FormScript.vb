Imports System.IO
Imports System.Text
Imports Microsoft.Scripting.Hosting

Imports System.Drawing.Text
Imports System.Reflection
Imports System.ComponentModel
Imports LuaInterface

Imports FarsiLibrary.Win

<System.Serializable()> Public Class FormScript

    Inherits Windows.Forms.Form

    Public scope As Microsoft.Scripting.Hosting.ScriptScope
    Public engine As Microsoft.Scripting.Hosting.ScriptEngine

    Public fc As FormFlowsheet
    Public language As Integer

    '0 = VBScript
    '1 = JScript
    '2 = IronPython
    '3 = IronRuby
    '4 = Lua

#Region "Custom members"
    Private findNodeResult As TreeNode = Nothing
    Private typed As String = ""
    Private wordMatched As Boolean = False
    Private assembly As Assembly
    Private namespaces As Hashtable
    Private nameSpaceNode As TreeNode
    Private foundNode As Boolean = False
    Private currentPath As String
#End Region

    Private Sub FormVBScript_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Me.Load

        'Me.txtScript.Document = New Alsing.SourceCode.SyntaxDocument()
        'With Me.txtScript.Document
        '    Select Case language
        '        Case 0
        '            Me.Text += " (VBScript)"
        '            .SyntaxFile = My.Application.Info.DirectoryPath & Path.DirectorySeparatorChar & "SyntaxFiles" & Path.DirectorySeparatorChar & "VBScript.syn"
        '        Case 1
        '            Me.Text += " (JScript)"
        '            .SyntaxFile = My.Application.Info.DirectoryPath & Path.DirectorySeparatorChar & "SyntaxFiles" & Path.DirectorySeparatorChar & "JavaScript.syn"
        '        Case 2
        '            Me.Text += " (IronPython)"
        '            .SyntaxFile = My.Application.Info.DirectoryPath & Path.DirectorySeparatorChar & "SyntaxFiles" & Path.DirectorySeparatorChar & "Python.syn"
        '        Case 3
        '            Me.Text += " (IronRuby)"
        '            .SyntaxFile = My.Application.Info.DirectoryPath & Path.DirectorySeparatorChar & "SyntaxFiles" & Path.DirectorySeparatorChar & "Perl.syn"
        '        Case 4
        '            Me.Text += " (Lua)"
        '            .SyntaxFile = My.Application.Info.DirectoryPath & Path.DirectorySeparatorChar & "SyntaxFiles" & Path.DirectorySeparatorChar & "C++.syn"
        '    End Select
        'End With

        ' Get the installed fonts collection.
        Dim installed_fonts As New InstalledFontCollection
        ' Get an array of the system's font familiies.
        Dim font_families() As FontFamily = installed_fonts.Families()
        ' Display the font families.
        For Each font_family As FontFamily In font_families
            tscb1.Items.Add(font_family.Name)
        Next font_family

        tscb2.Items.AddRange(New Object() {6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16})

        tscb2.SelectedItem = 10
        tscb1.SelectedItem = "Courier New"

        'readAssembly(GetType(DWSIM.ClassesBasicasTermodinamica.Fase).Assembly)
        'readAssembly(GetType(System.String).Assembly)
        'readAssembly(GetType(CapeOpen.BaseUnitEditor).Assembly)
        'readAssembly(GetType(CAPEOPEN110.ICapeThermoPhases).Assembly)

        'With Me.listBoxAutoComplete
        '    .Font = New Font("Arial", 8, FontStyle.Regular, GraphicsUnit.Point)
        '    .Height = 250
        'End With


    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton1.Click

        Select Case language
            Case 2
                Dim opts As New Dictionary(Of String, Object)()
                opts("Frames") = Microsoft.Scripting.Runtime.ScriptingRuntimeHelpers.True
                engine = IronPython.Hosting.Python.CreateEngine(opts)
                Dim paths(My.Settings.ScriptPaths.Count - 1) As String
                My.Settings.ScriptPaths.CopyTo(paths, 0)
                Try
                    engine.SetSearchPaths(paths)
                Catch ex As Exception
                End Try
                engine.Runtime.LoadAssembly(GetType(System.String).Assembly)
                engine.Runtime.LoadAssembly(GetType(DWSIM.ClassesBasicasTermodinamica.ConstantProperties).Assembly)
                engine.Runtime.LoadAssembly(GetType(Microsoft.Msdn.Samples.GraphicObjects.GraphicObject).Assembly)
                engine.Runtime.LoadAssembly(GetType(Microsoft.Msdn.Samples.DesignSurface.GraphicsSurface).Assembly)
                'engine.Runtime.IO.SetOutput(New TextBoxStream(txtOutput), UTF8Encoding.UTF8)
                scope = engine.CreateScope()
                scope.SetVariable("Flowsheet", fc)
                Dim Solver As New DWSIM.Flowsheet.COMSolver
                scope.SetVariable("Solver", Solver)
                Dim txtcode As String = ""
                For Each fname As String In Me.ListBox1.Items
                    txtcode += File.ReadAllText(fname) + vbCrLf
                Next
                'txtcode += Me.txtScript.Document.Text
                Dim source As Microsoft.Scripting.Hosting.ScriptSource = Me.engine.CreateScriptSourceFromString(txtcode, Microsoft.Scripting.SourceCodeKind.Statements)
                Try
                   source.Execute(Me.scope)
                Catch ex As Exception
                    Dim ops As ExceptionOperations = engine.GetService(Of ExceptionOperations)()

                Finally
                    engine = Nothing
                    scope = Nothing
                    source = Nothing
                End Try
        End Select

    End Sub

    Public Class TextBoxWriter
        Inherits TextWriter
        Private ReadOnly m_encoding As Encoding
        Private ReadOnly textBox As TextBox

        Public Sub New(ByVal textBox As TextBox)
            Me.New(textBox, UTF8Encoding.UTF8)
        End Sub

        Public Sub New(ByVal textBox As TextBox, ByVal encoding As Encoding)
            Me.textBox = textBox
            Me.m_encoding = encoding
        End Sub

        Public Overrides Sub WriteLine(ByVal value As String)
            Me.textBox.AppendText(value)
        End Sub

        Public Overrides Sub Write(ByVal value As Char)
            Me.textBox.AppendText(value.ToString())
        End Sub

        Public Overrides Sub Write(ByVal value As String)
            Me.textBox.AppendText(value)
        End Sub

        Public Overrides Sub Write(ByVal buffer As Char(), ByVal index As Integer, ByVal count As Integer)
            Me.textBox.AppendText(New String(buffer))
        End Sub

        Public Overrides ReadOnly Property Encoding() As Encoding
            Get
                Return Me.m_encoding
            End Get
        End Property
    End Class

    Public Class TextBoxStream
        Inherits MemoryStream
        Private target As TextBox

        Public Sub New(ByVal target As TextBox)
            Me.target = target
        End Sub

        Public Overrides Sub Write(ByVal buffer As Byte(), ByVal offset As Integer, ByVal count As Integer)
            Dim output As String = Encoding.UTF8.GetString(buffer, offset, count)
            target.AppendText(output)
        End Sub
    End Class

    Private Sub ToolStripButton4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton4.Click
        If Me.Opacity < 1.0# Then Me.Opacity += 0.05
    End Sub

    Private Sub ToolStripButton5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton5.Click
        If Me.Opacity > 0.0# Then Me.Opacity -= 0.05
    End Sub

    Private Sub CutToolStripButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CutToolStripButton.Click
        'If txtScript.Selection.Text <> "" Then
        '    Clipboard.SetText(txtScript.Selection.Text)
        '    txtScript.Selection.DeleteSelection()
        'End If
    End Sub

    Private Sub CopyToolStripButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CopyToolStripButton.Click
        'If txtScript.Selection.Text <> "" Then Clipboard.SetText(txtScript.Selection.Text)
    End Sub

    Private Sub PasteToolStripButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PasteToolStripButton.Click
        'If txtScript.Selection.SelLength <> 0 Then
        '    txtScript.Selection.Text = Clipboard.GetText()
        'Else
        '    txtScript.Document.InsertText(Clipboard.GetText(), txtScript.Caret.Position.X, txtScript.Caret.Position.Y)
        'End If
    End Sub

    Private Sub ToolStripButton2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton2.Click
        If Me.ofd1.ShowDialog = Windows.Forms.DialogResult.OK Then
            For Each fname As String In Me.ofd1.FileNames
                Me.ListBox1.Items.Add(fname)
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
        End If
    End Sub

    Private Sub SaveToolStripButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SaveToolStripButton.Click
        'If Me.sfd1.ShowDialog = Windows.Forms.DialogResult.OK Then
        '    My.Computer.FileSystem.WriteAllText(Me.sfd1.FileName, Me.txtScript.Document.Text, False)
        'End If
    End Sub

    Private Sub OpenToolStripButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OpenToolStripButton.Click
        'If Me.txtScript.Document.Text <> "" Then
        '    If MessageBox.Show(DWSIM.App.GetLocalString("DesejaSalvaroScriptAtual"), DWSIM.App.GetLocalString("Ateno"), MessageBoxButtons.YesNo) = Windows.Forms.DialogResult.Yes Then
        '        SaveToolStripButton_Click(sender, e)
        '    End If
        'End If
        'If Me.ofd2.ShowDialog = Windows.Forms.DialogResult.OK Then
        '    Me.txtScript.Document.Text = ""
        '    For Each fname As String In Me.ofd2.FileNames
        '        Me.txtScript.Document.Text += File.ReadAllText(fname) & vbCrLf
        '    Next
        'End If
    End Sub

    Private Sub PrintToolStripButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PrintToolStripButton.Click
        'Dim pd As Alsing.SourceCode.SourceCodePrintDocument
        'pd = New Alsing.SourceCode.SourceCodePrintDocument(txtScript.Document)
        'pd1.Document = pd
        'If pd1.ShowDialog(Me) = DialogResult.OK Then
        '    pd.Print()
        'End If
    End Sub

    Private Sub tscb1_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles tscb1.SelectedIndexChanged
        'txtScript.FontName = tscb1.SelectedItem.ToString
    End Sub

    Private Sub tscb2_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles tscb2.SelectedIndexChanged
        'txtScript.FontSize = tscb2.SelectedItem
    End Sub

    Private Sub NewToolStripButton_Click(sender As Object, e As EventArgs) Handles NewToolStripButton.Click

        Dim p As New Panel With {.Dock = DockStyle.Fill}
        p.Controls.Add(New ScriptEditorControl With {.Dock = DockStyle.Fill})

        Dim stab As New FATabStripItem()
        stab.Controls.Add(p)
        stab.Title = "Script" & TabStripScripts.Items.Count + 1

        TabStripScripts.Items.Add(stab)

        TabStripScripts.SelectedItem = stab

        Me.Invalidate()

    End Sub

End Class




