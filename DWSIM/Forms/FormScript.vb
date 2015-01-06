Imports System.IO
Imports System.Text
Imports Microsoft.Scripting.Hosting

Imports System.Drawing.Text
Imports System.Reflection
Imports System.ComponentModel
Imports LuaInterface

Imports FarsiLibrary.Win
Imports DWSIM.DWSIM.Outros

<System.Serializable()> Public Class FormScript

    Inherits WeifenLuo.WinFormsUI.Docking.DockContent

    Public scope As Microsoft.Scripting.Hosting.ScriptScope
    Public engine As Microsoft.Scripting.Hosting.ScriptEngine

    Public fc As FormFlowsheet
    Public language As Integer

    '0 = VBScript
    '1 = JScript
    '2 = IronPython
    '3 = IronRuby
    '4 = Lua

    Private Sub FormVBScript_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Me.Load

        ' Get the installed fonts collection.
        Dim installed_fonts As New InstalledFontCollection
        ' Get an array of the system's font familiies.
        Dim font_families() As FontFamily = installed_fonts.Families()
        ' Display the font families.
        For Each font_family As FontFamily In font_families
            tscb1.Items.Add(font_family.Name)
        Next

        tscb2.Items.AddRange(New Object() {6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16})

        tscb2.SelectedItem = 10
        tscb1.SelectedItem = "Courier New"

        'load existing scripts
        For Each s As Script In fc.ScriptCollection.Values
            InsertScriptTab(s)
        Next

    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton1.Click

        If Not Me.TabStripScripts.SelectedItem Is Nothing Then

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
            engine.Runtime.IO.SetOutput(New DataGridViewTextStream(fc), UTF8Encoding.UTF8)
            scope = engine.CreateScope()
            scope.SetVariable("Flowsheet", fc)
            Dim Solver As New DWSIM.Flowsheet.COMSolver
            scope.SetVariable("Solver", Solver)
            For Each obj As SimulationObjects_BaseClass In fc.Collections.ObjectCollection.Values
                scope.SetVariable(obj.GraphicObject.Tag.Replace("-", "_"), obj)
            Next
            Dim txtcode As String = ""
            For Each fname As String In Me.ListBox1.Items
                txtcode += File.ReadAllText(fname) + vbCrLf
            Next
            txtcode += DirectCast(Me.TabStripScripts.SelectedItem.Controls(0).Controls(0), ScriptEditorControl).txtScript.Document.Text
            Dim source As Microsoft.Scripting.Hosting.ScriptSource = Me.engine.CreateScriptSourceFromString(txtcode, Microsoft.Scripting.SourceCodeKind.Statements)
            Try
                source.Execute(Me.scope)
            Catch ex As Exception
                Dim ops As ExceptionOperations = engine.GetService(Of ExceptionOperations)()
                fc.WriteToLog("Error running script '" & Me.TabStripScripts.SelectedItem.Title & "': " & ops.FormatException(ex).ToString, Color.Red, DWSIM.FormClasses.TipoAviso.Erro)
            Finally
                engine = Nothing
                scope = Nothing
                source = Nothing
            End Try

        End If

    End Sub

    Private Sub CutToolStripButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CutToolStripButton.Click
        Dim scontrol As ScriptEditorControl = DirectCast(TabStripScripts.SelectedItem.Controls(0).Controls(0), ScriptEditorControl)
        If scontrol.txtScript.Selection.Text <> "" Then
            Clipboard.SetText(scontrol.txtScript.Selection.Text)
            scontrol.txtScript.Selection.DeleteSelection()
        End If
    End Sub

    Private Sub CopyToolStripButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CopyToolStripButton.Click
        Dim scontrol As ScriptEditorControl = DirectCast(TabStripScripts.SelectedItem.Controls(0).Controls(0), ScriptEditorControl)
        If scontrol.txtScript.Selection.Text <> "" Then Clipboard.SetText(scontrol.txtScript.Selection.Text)
    End Sub

    Private Sub PasteToolStripButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PasteToolStripButton.Click
        Dim scontrol As ScriptEditorControl = DirectCast(TabStripScripts.SelectedItem.Controls(0).Controls(0), ScriptEditorControl)
        If scontrol.txtScript.Selection.SelLength <> 0 Then
            scontrol.txtScript.Selection.Text = Clipboard.GetText()
        Else
            scontrol.txtScript.Document.InsertText(Clipboard.GetText(), scontrol.txtScript.Caret.Position.X, scontrol.txtScript.Caret.Position.Y)
        End If
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

        fc.ScriptCollection.Clear()

        For Each tab As FATabStripItem In TabStripScripts.Items
            Dim scr As New Script() With {.ID = Guid.NewGuid().ToString, .Title = tab.Title, .ScriptText = DirectCast(tab.Controls(0).Controls(0), ScriptEditorControl).txtScript.Document.Text}
            fc.ScriptCollection.Add(scr.ID, scr)
        Next

        fc.WriteToLog("Script Data updated sucessfully.", Color.Blue, DWSIM.FormClasses.TipoAviso.Informacao)

    End Sub

    Private Sub PrintToolStripButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PrintToolStripButton.Click
        Dim pd As Alsing.SourceCode.SourceCodePrintDocument
        pd = New Alsing.SourceCode.SourceCodePrintDocument(DirectCast(TabStripScripts.SelectedItem.Controls(0).Controls(0), ScriptEditorControl).txtScript.Document)
        pd1.Document = pd
        If pd1.ShowDialog(Me) = DialogResult.OK Then
            pd.Print()
        End If
    End Sub

    Private Sub tscb1_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles tscb1.SelectedIndexChanged
        For Each ft As FATabStripItem In TabStripScripts.Items
            DirectCast(ft.Controls(0).Controls(0), ScriptEditorControl).txtScript.FontName = tscb1.SelectedItem.ToString
        Next
    End Sub

    Private Sub tscb2_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles tscb2.SelectedIndexChanged
        For Each ft As FATabStripItem In TabStripScripts.Items
            DirectCast(ft.Controls(0).Controls(0), ScriptEditorControl).txtScript.FontSize = tscb2.SelectedItem
        Next
    End Sub

    Private Sub NewToolStripButton_Click(sender As Object, e As EventArgs) Handles NewToolStripButton.Click

        InsertScriptTab(New Script())

    End Sub

    Private Sub InsertScriptTab(scriptdata As Script)

        Dim p As New Panel With {.Dock = DockStyle.Fill}
        Dim scontrol As New ScriptEditorControl With {.Dock = DockStyle.Fill}

        With scontrol

            .txtScript.Document = New Alsing.SourceCode.SyntaxDocument()
            .txtScript.Document.SyntaxFile = My.Application.Info.DirectoryPath & Path.DirectorySeparatorChar & "SyntaxFiles" & Path.DirectorySeparatorChar & "Python.syn"
            .txtScript.FontName = tscb1.SelectedItem.ToString
            .txtScript.FontSize = tscb2.SelectedItem

            .txtScript.Document.Text = scriptdata.ScriptText

            .readAssembly(GetType(DWSIM.ClassesBasicasTermodinamica.Fase).Assembly)
            .readAssembly(GetType(System.String).Assembly)
            .readAssembly(GetType(CapeOpen.BaseUnitEditor).Assembly)
            .readAssembly(GetType(CAPEOPEN110.ICapeThermoPhases).Assembly)

            .listBoxAutoComplete.Font = New Font("Arial", 8, FontStyle.Regular, GraphicsUnit.Point)
            .listBoxAutoComplete.Height = 250

            .form = fc

        End With

        p.Controls.Add(scontrol)

        Dim stab As New FATabStripItem()
        stab.Controls.Add(p)
        stab.Tag = scriptdata.ID
        If scriptdata.Title = "" Then stab.Title = "Script" & TabStripScripts.Items.Count + 1 Else stab.Title = scriptdata.Title

        TabStripScripts.Items.Add(stab)

        TabStripScripts.SelectedItem = stab

        Me.tsTextBoxRename.Text = stab.Title

        Me.Invalidate()

    End Sub

    Private Sub tsTextBoxRename_KeyDown(sender As Object, e As KeyEventArgs) Handles tsTextBoxRename.KeyDown
        If e.KeyCode = Keys.Enter Then
            TabStripScripts.SelectedItem.Title = tsTextBoxRename.Text
        End If
    End Sub

    Private Sub TabStripScripts_TabStripItemSelectionChanged(e As TabStripItemChangedEventArgs) Handles TabStripScripts.TabStripItemSelectionChanged
        tsTextBoxRename.Text = TabStripScripts.SelectedItem.Title
    End Sub

    Private Sub TabStripScripts_TabStripItemClosing(e As TabStripItemClosingEventArgs) Handles TabStripScripts.TabStripItemClosing
        If MessageBox.Show(DWSIM.App.GetLocalString("RemoveScriptQuestion"), "DWSIM", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = Windows.Forms.DialogResult.No Then
            e.Cancel = True
        End If
    End Sub

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

Public Class DataGridViewTextStream
    Inherits MemoryStream
    Private target As FormFlowsheet

    Public Sub New(ByVal target As FormFlowsheet)
        Me.target = target
    End Sub

    Public Overrides Sub Write(ByVal buffer As Byte(), ByVal offset As Integer, ByVal count As Integer)
        Dim output As String = Encoding.UTF8.GetString(buffer, offset, count)
        target.WriteToLog(output, Color.DarkGray, DWSIM.FormClasses.TipoAviso.Informacao)
    End Sub

End Class



