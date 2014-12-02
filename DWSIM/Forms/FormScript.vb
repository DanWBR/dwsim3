Imports System.IO
Imports System.Text
Imports Microsoft.Scripting.Hosting

Imports System.Drawing.Text
Imports System.Reflection
Imports System.ComponentModel
Imports LuaInterface

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

        Me.txtScript.Document = New Alsing.SourceCode.SyntaxDocument()
        With Me.txtScript.Document
            Select Case language
                Case 0
                    Me.Text += " (VBScript)"
                    .SyntaxFile = My.Application.Info.DirectoryPath & Path.DirectorySeparatorChar & "SyntaxFiles" & Path.DirectorySeparatorChar & "VBScript.syn"
                Case 1
                    Me.Text += " (JScript)"
                    .SyntaxFile = My.Application.Info.DirectoryPath & Path.DirectorySeparatorChar & "SyntaxFiles" & Path.DirectorySeparatorChar & "JavaScript.syn"
                Case 2
                    Me.Text += " (IronPython)"
                    .SyntaxFile = My.Application.Info.DirectoryPath & Path.DirectorySeparatorChar & "SyntaxFiles" & Path.DirectorySeparatorChar & "Python.syn"
                Case 3
                    Me.Text += " (IronRuby)"
                    .SyntaxFile = My.Application.Info.DirectoryPath & Path.DirectorySeparatorChar & "SyntaxFiles" & Path.DirectorySeparatorChar & "Perl.syn"
                Case 4
                    Me.Text += " (Lua)"
                    .SyntaxFile = My.Application.Info.DirectoryPath & Path.DirectorySeparatorChar & "SyntaxFiles" & Path.DirectorySeparatorChar & "C++.syn"
            End Select
        End With

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

        readAssembly(GetType(DWSIM.ClassesBasicasTermodinamica.Fase).Assembly)
        readAssembly(GetType(System.String).Assembly)
        readAssembly(GetType(CapeOpen.BaseUnitEditor).Assembly)
        readAssembly(GetType(CAPEOPEN110.ICapeThermoPhases).Assembly)

        With Me.listBoxAutoComplete
            .Font = New Font("Arial", 8, FontStyle.Regular, GraphicsUnit.Point)
            .Height = 250
        End With


    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton1.Click

        Select Case language
            Case 4
                Dim lscript As New Lua
                lscript("flowsheet") = fc
                Dim Solver As New DWSIM.Flowsheet.COMSolver
                lscript("solver") = Solver
                lscript("this") = Me
                lscript("dwsim") = GetType(DWSIM.ClassesBasicasTermodinamica.Fase).Assembly
                Try
                    Me.txtOutput.Text = Date.Now.ToString & " Running script..." & vbCrLf
                    Me.txtErrorList.Text = ""
                    Dim txtcode As String = ""
                    For Each fname As String In Me.ListBox1.Items
                        txtcode += File.ReadAllText(fname) + vbCrLf
                    Next
                    txtcode += txtScript.Document.Text
                    lscript.DoString(txtcode)
                Catch ex As Exception
                    Me.txtOutput.Text = "Error parsing script. Check the error list for details..."
                    Me.txtErrorList.Text = ex.ToString
                End Try
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
                engine.Runtime.IO.SetOutput(New TextBoxStream(txtOutput), UTF8Encoding.UTF8)
                scope = engine.CreateScope()
                scope.SetVariable("Flowsheet", fc)
                Dim Solver As New DWSIM.Flowsheet.COMSolver
                scope.SetVariable("Solver", Solver)
                Dim txtcode As String = ""
                For Each fname As String In Me.ListBox1.Items
                    txtcode += File.ReadAllText(fname) + vbCrLf
                Next
                txtcode += Me.txtScript.Document.Text
                Dim source As Microsoft.Scripting.Hosting.ScriptSource = Me.engine.CreateScriptSourceFromString(txtcode, Microsoft.Scripting.SourceCodeKind.Statements)
                Try
                    Me.txtOutput.Text = Date.Now.ToString & " Running script..." & vbCrLf
                    Me.txtErrorList.Text = ""
                    source.Execute(Me.scope)
                Catch ex As Exception
                    Dim ops As ExceptionOperations = engine.GetService(Of ExceptionOperations)()
                    Me.txtOutput.Text = "Error parsing script. Check the error list for details..."
                    Me.txtErrorList.Text = ops.FormatException(ex).ToString
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
        If txtScript.Selection.Text <> "" Then
            Clipboard.SetText(txtScript.Selection.Text)
            txtScript.Selection.DeleteSelection()
        End If
    End Sub

    Private Sub CopyToolStripButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CopyToolStripButton.Click
        If txtScript.Selection.Text <> "" Then Clipboard.SetText(txtScript.Selection.Text)
    End Sub

    Private Sub PasteToolStripButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PasteToolStripButton.Click
        If txtScript.Selection.SelLength <> 0 Then
            txtScript.Selection.Text = Clipboard.GetText()
        Else
            txtScript.Document.InsertText(Clipboard.GetText(), txtScript.Caret.Position.X, txtScript.Caret.Position.Y)
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
        If Me.sfd1.ShowDialog = Windows.Forms.DialogResult.OK Then
            My.Computer.FileSystem.WriteAllText(Me.sfd1.FileName, Me.txtScript.Document.Text, False)
        End If
    End Sub

    Private Sub OpenToolStripButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OpenToolStripButton.Click
        If Me.txtScript.Document.Text <> "" Then
            If MessageBox.Show(DWSIM.App.GetLocalString("DesejaSalvaroScriptAtual"), DWSIM.App.GetLocalString("Ateno"), MessageBoxButtons.YesNo) = Windows.Forms.DialogResult.Yes Then
                SaveToolStripButton_Click(sender, e)
            End If
        End If
        If Me.ofd2.ShowDialog = Windows.Forms.DialogResult.OK Then
            Me.txtScript.Document.Text = ""
            For Each fname As String In Me.ofd2.FileNames
                Me.txtScript.Document.Text += File.ReadAllText(fname) & vbCrLf
            Next
        End If
    End Sub

    Private Sub PrintToolStripButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PrintToolStripButton.Click
        Dim pd As Alsing.SourceCode.SourceCodePrintDocument
        pd = New Alsing.SourceCode.SourceCodePrintDocument(txtScript.Document)
        pd1.Document = pd
        If pd1.ShowDialog(Me) = DialogResult.OK Then
            pd.Print()
        End If
    End Sub

    Private Sub tscb1_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles tscb1.SelectedIndexChanged
        txtScript.FontName = tscb1.SelectedItem.ToString
    End Sub

    Private Sub tscb2_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles tscb2.SelectedIndexChanged
        txtScript.FontSize = tscb2.SelectedItem
    End Sub

    Private Sub txtScript__KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtScript.KeyDown
        ' Keep track of the current character, used
        ' for tracking whether to hide the list of members,
        ' when the delete button is pressed
        Dim i As Integer = Me.txtScript.Selection.LogicalSelStart
        Dim currentChar As String = ""

        If i > 0 Then
            currentChar = Me.txtScript.Document.Text.Substring(i - 1, 1)
        End If

        If e.KeyData = Keys.OemPeriod Then
            ' The amazing dot key
            If Not Me.listBoxAutoComplete.Visible Then
                ' Display the member listview if there are
                ' items in it
                If populateListBox() Then
                    Me.listBoxAutoComplete.SelectedIndex = 0
                    ' Find the position of the caret
                    Dim point As Point = New Point(Me.txtScript.Caret.Position.X * Me.txtScript.FontSize + 34, Me.txtScript.Caret.Position.Y)
                    point.Y += CInt(Math.Truncate(Me.txtScript.FontSize)) + 4
                    point.X += 2
                    ' for Courier, may need a better method
                    Me.listBoxAutoComplete.Location = point
                    Me.listBoxAutoComplete.BringToFront()
                    Me.listBoxAutoComplete.Show()
                End If
            Else
                Me.listBoxAutoComplete.Hide()
                typed = ""

            End If
        ElseIf e.KeyCode = Keys.Back Then
            ' Delete key - hides the member list if the character
            ' being deleted is a dot

            Me.textBoxTooltip.Hide()
            If typed.Length > 0 Then
                typed = typed.Substring(0, typed.Length - 1)
            End If
            If currentChar = "." Then
                Me.listBoxAutoComplete.Hide()

            End If
        ElseIf e.KeyCode = Keys.Up Then
            ' The up key moves up our member list, if
            ' the list is visible

            Me.textBoxTooltip.Hide()

            If Me.listBoxAutoComplete.Visible Then
                Me.wordMatched = True
                If Me.listBoxAutoComplete.SelectedIndex > 0 Then
                    Me.listBoxAutoComplete.SelectedIndex -= 1
                End If

                e.Handled = True
                e.SuppressKeyPress = True
            End If
        ElseIf e.KeyCode = Keys.Down Then
            ' The up key moves down our member list, if
            ' the list is visible

            Me.textBoxTooltip.Hide()

            If Me.listBoxAutoComplete.Visible Then
                Me.wordMatched = True
                If Me.listBoxAutoComplete.SelectedIndex < Me.listBoxAutoComplete.Items.Count - 1 Then
                    Me.listBoxAutoComplete.SelectedIndex += 1
                End If

                e.Handled = True
                e.SuppressKeyPress = True
            End If
        ElseIf e.KeyCode = Keys.D9 Then
            ' Trap the open bracket key, displaying a cheap and
            ' cheerful tooltip if the word just typed is in our tree
            ' (the parameters are stored in the tag property of the node)

            Dim word As String = Me.getLastWord()
            Me.foundNode = False
            Me.nameSpaceNode = Nothing

            Me.currentPath = ""
            searchTree(Me.treeViewItems.Nodes, ReplacePath(word), True)

            If Me.nameSpaceNode IsNot Nothing Then
                If TypeOf Me.nameSpaceNode.Tag Is String Then
                    Me.textBoxTooltip.Text = DirectCast(Me.nameSpaceNode.Tag, String)
                    ' Find the position of the caret
                    Dim point As Point = New Point(Me.txtScript.Caret.Position.X * Me.txtScript.FontSize, Me.txtScript.Caret.Position.Y)
                    point.Y += CInt(Math.Truncate(Me.txtScript.FontSize)) + 4
                    Me.textBoxTooltip.Width = Me.textBoxTooltip.Text.Length * 6
                    Me.textBoxTooltip.Size = New Size(Me.textBoxTooltip.Text.Length * 6, Me.textBoxTooltip.Height)
                    ' Resize tooltip for long parameters
                    ' (doesn't wrap text nicely)
                    If Me.textBoxTooltip.Width > 300 Then
                        Me.textBoxTooltip.Width = 300
                        Dim height As Integer = 0
                        height = Me.textBoxTooltip.Text.Length \ 50
                        Me.textBoxTooltip.Height = height * 15
                    End If
                    Me.textBoxTooltip.Location = point
                    Me.textBoxTooltip.Show()
                End If
            End If
        ElseIf e.KeyCode = Keys.D8 Then
            ' Close bracket key, hide the tooltip textbox
            Me.textBoxTooltip.Hide()
        ElseIf e.KeyValue < 48 OrElse (e.KeyValue >= 58 AndAlso e.KeyValue <= 64) OrElse (e.KeyValue >= 91 AndAlso e.KeyValue <= 96) OrElse e.KeyValue > 122 Then
            ' Check for any non alphanumerical key, hiding
            ' member list box if it's visible.

            If Me.listBoxAutoComplete.Visible Then
                ' Check for keys for autofilling (return,tab,space)
                ' and autocomplete the richtextbox when they're pressed.
                If e.KeyCode = Keys.[Return] OrElse e.KeyCode = Keys.Tab OrElse e.KeyCode = Keys.Space Then
                    Me.textBoxTooltip.Hide()

                    ' Autocomplete
                    Me.selectItem()

                    Me.typed = ""
                    Me.wordMatched = False
                    e.Handled = True
                    e.SuppressKeyPress = True
                End If

                ' Hide the member list view
                If e.KeyCode <> Keys.ShiftKey Then Me.listBoxAutoComplete.Hide()
            End If
        ElseIf e.KeyCode = Keys.F5 Then
            Me.Button1_Click(sender, e)
        Else
            ' Letter or number typed, search for it in the listview
            If Me.listBoxAutoComplete.Visible Then
                Dim val As Char = ChrW(e.KeyValue)
                Me.typed += val

                Me.wordMatched = False

                ' Loop through all the items in the listview, looking
                ' for one that starts with the letters typed
                For i = 0 To Me.listBoxAutoComplete.Items.Count - 1
                    If Me.listBoxAutoComplete.Items(i).ToString().ToLower().StartsWith(Me.typed.ToLower()) Then
                        Me.wordMatched = True
                        Me.listBoxAutoComplete.SelectedIndex = i
                        Exit For
                    End If
                Next
            Else
                Me.typed = ""
            End If

        End If
    End Sub

    Private Sub txtScript_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles txtScript.MouseDown
        ' Hide the listview and the tooltip
        Try
            Me.textBoxTooltip.Hide()
            Me.listBoxAutoComplete.Hide()
        Catch ex As Exception

        End Try
    End Sub

    Private Sub listBoxAutoComplete_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles listBoxAutoComplete.KeyDown
        ' Ignore any keys being pressed on the listview
        Me.txtScript.Focus()
    End Sub

    Private Sub listBoxAutoComplete_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles listBoxAutoComplete.DoubleClick
        ' Item double clicked, select it
        If Me.listBoxAutoComplete.SelectedItems.Count = 1 Then
            Me.wordMatched = True
            Me.selectItem()
            Me.listBoxAutoComplete.Hide()
            Me.txtScript.Focus()
            Me.wordMatched = False
        End If
    End Sub

    Private Sub listBoxAutoComplete_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles listBoxAutoComplete.SelectedIndexChanged
        ' Make sure when an item is selected, control is returned back to the richtext
        Me.txtScript.Focus()
    End Sub

    Private Sub textBoxTooltip_Enter(ByVal sender As Object, ByVal e As System.EventArgs) Handles textBoxTooltip.Enter
        ' Stop the fake tooltip's text being selected
        Me.txtScript.Focus()
    End Sub

#Region "Util methods"

    Public Function ReplacePath(word As String) As String
        word = word.ToLower
        If word.Contains("flowsheet") Then Return word.Replace("flowsheet", "DWSIM.FormFlowsheet")
        If word.Contains("solver") Then Return word.Replace("solver", "DWSIM.DWSIM.Flowsheet.FlowsheetSolver")
        Return word
    End Function

    ''' <summary>
    ''' Takes an assembly filename, opens it and retrieves all types.
    ''' </summary>
    ''' <param name="assmb">assembly to load</param>
    Private Sub readAssembly(ByVal assmb As Assembly)

        'Me.treeViewItems.Nodes.Clear()
        namespaces = New Hashtable()

        assembly = assmb

        Dim assemblyTypes As Type() = assembly.GetTypes()
        'Me.treeViewItems.Nodes.Clear()

        ' Cycle through types
        For Each type As Type In assemblyTypes
            If type.[Namespace] IsNot Nothing Then
                If namespaces.ContainsKey(type.[Namespace]) Then
                    ' Already got namespace, add the class to it
                    Dim treeNode As TreeNode = DirectCast(namespaces(type.[Namespace]), TreeNode)
                    treeNode = treeNode.Nodes.Add(type.Name)
                    Me.addMembers(treeNode, type)

                    If type.IsClass Then
                        treeNode.Tag = MemberTypes.Custom
                    End If
                Else
                    ' New namespace
                    Dim membersNode As TreeNode = Nothing

                    If type.[Namespace].IndexOf(".") <> -1 Then
                        ' Search for already existing parts of the namespace
                        nameSpaceNode = Nothing
                        foundNode = False

                        Me.currentPath = ""
                        searchTree(Me.treeViewItems.Nodes, type.[Namespace], True)

                        ' No existing namespace found
                        If nameSpaceNode Is Nothing Then
                            ' Add the namespace
                            Dim parts As String() = type.[Namespace].Split(".")

                            Dim treeNode As TreeNode = treeViewItems.Nodes.Add(parts(0))
                            Dim sNamespace As String = parts(0)

                            If Not namespaces.ContainsKey(sNamespace) Then
                                namespaces.Add(sNamespace, treeNode)
                            End If

                            For i As Integer = 1 To parts.Length - 1
                                treeNode = treeNode.Nodes.Add(parts(i))
                                sNamespace += "." & parts(i)
                                If Not namespaces.ContainsKey(sNamespace) Then
                                    namespaces.Add(sNamespace, treeNode)
                                End If
                            Next

                            membersNode = treeNode.Nodes.Add(type.Name)
                        Else
                            ' Existing namespace, add this namespace to it,
                            ' and add the class
                            Dim parts As String() = type.[Namespace].Split(".")
                            Dim newNamespaceNode As TreeNode = Nothing

                            If Not namespaces.ContainsKey(type.[Namespace]) Then
                                newNamespaceNode = nameSpaceNode.Nodes.Add(parts(parts.Length - 1))
                                namespaces.Add(type.[Namespace], newNamespaceNode)
                            Else
                                newNamespaceNode = DirectCast(namespaces(type.[Namespace]), TreeNode)
                            End If

                            If newNamespaceNode IsNot Nothing Then
                                membersNode = newNamespaceNode.Nodes.Add(type.Name)
                                If type.IsClass Then
                                    membersNode.Tag = MemberTypes.[Custom]
                                End If
                            End If

                        End If
                    Else
                        ' Single root namespace, add to root
                        membersNode = treeViewItems.Nodes.Add(type.[Namespace])
                    End If

                    ' Add all members
                    If membersNode IsNot Nothing Then
                        Me.addMembers(membersNode, type)
                    End If
                End If

            End If
        Next
    End Sub

    ''' <summary>
    ''' Adds all members to the node's children, grabbing the parameters
    ''' for methods.
    ''' </summary>
    ''' <param name="treeNode"></param>
    ''' <param name="type"></param>
    ''' 
    Private Sub addMembers(ByVal treeNode As TreeNode, ByVal type As System.Type)
        ' Get all members except methods
        Dim memberInfo As MemberInfo() = type.GetMembers()
        For j As Integer = 0 To memberInfo.Length - 1
            If memberInfo(j).ReflectedType.IsPublic And memberInfo(j).MemberType <> MemberTypes.Method Then
                Dim node As TreeNode = treeNode.Nodes.Add(memberInfo(j).Name)
                node.Tag = memberInfo(j).MemberType
            End If
        Next

        ' Get all methods
        Dim methodInfo As MethodInfo() = type.GetMethods()
        For j As Integer = 0 To methodInfo.Length - 1
            Dim node As TreeNode = treeNode.Nodes.Add(methodInfo(j).Name)
            Dim parms As String = ""

            Dim parameterInfo As ParameterInfo() = methodInfo(j).GetParameters()
            For f As Integer = 0 To parameterInfo.Length - 1
                parms += parameterInfo(f).ParameterType.ToString() & " " & parameterInfo(f).Name & ", "
            Next

            ' Knock off remaining ", "
            If parms.Length > 2 Then
                parms = parms.Substring(0, parms.Length - 2)
            End If

            node.Tag = parms
        Next
    End Sub

    ''' <summary>
    ''' Searches the tree view for a namespace, saving the node. The method
    ''' stops and returns as soon as the namespace search can't find any
    ''' more items in its path, unless continueUntilFind is true.
    ''' </summary>
    ''' <param name="treeNodes"></param>
    ''' <param name="path"></param>
    ''' <param name="continueUntilFind"></param>
    Private Sub searchTree(ByVal treeNodes As TreeNodeCollection, ByVal path As String, ByVal continueUntilFind As Boolean)
        If Me.foundNode Then
            Return
        End If

        Dim p As String = ""
        Dim n As Integer = 0
        n = path.IndexOf(".")

        If n <> -1 Then
            p = path.Substring(0, n)

            If currentPath <> "" Then
                currentPath += "." & p
            Else
                currentPath = p
            End If

            ' Knock off the first part
            path = path.Remove(0, n + 1)
        Else
            currentPath += "." & path
        End If

        For i As Integer = 0 To treeNodes.Count - 1
            If treeNodes(i).FullPath.ToLower = currentPath.ToLower Then
                If continueUntilFind Then
                    nameSpaceNode = treeNodes(i)
                End If
                nameSpaceNode = treeNodes(i)
                ' got a dot, continue, or return
                Me.searchTree(treeNodes(i).Nodes, path, continueUntilFind)
            ElseIf Not continueUntilFind Then
                foundNode = True
                Return
            End If
        Next
    End Sub

    ''' <summary>
    ''' Searches the tree until the given path is found, storing
    ''' the found node in a member var.
    ''' </summary>
    ''' <param name="path"></param>
    ''' <param name="treeNodes"></param>
    Private Sub findNode(ByVal path As String, ByVal treeNodes As TreeNodeCollection)
        For i As Integer = 0 To treeNodes.Count - 1
            If treeNodes(i).FullPath.ToLower = path.ToLower Then
                Me.findNodeResult = treeNodes(i)
                Exit For
            ElseIf treeNodes(i).Nodes.Count > 0 Then
                Me.findNode(path, treeNodes(i).Nodes)
            End If
        Next
    End Sub

    ''' <summary>
    ''' Called when a "." is pressed - the previous word is found,
    ''' and if matched in the treeview, the members listbox is
    ''' populated with items from the tree, which are first sorted.
    ''' </summary>
    ''' <returns>Whether an items are found for the word</returns>
    Private Function populateListBox() As Boolean
        Dim result As Boolean = False
        Dim word As String = Me.getLastWord()

        'System.Diagnostics.Debug.WriteLine(" - Path: " +word);

        If word <> "" Then
            findNodeResult = Nothing
            findNode(ReplacePath(word), Me.treeViewItems.Nodes)

            If Me.findNodeResult IsNot Nothing Then
                Me.listBoxAutoComplete.Items.Clear()

                If Me.findNodeResult.Nodes.Count > 0 Then
                    result = True

                    ' Sort alphabetically (this could be replaced with
                    ' a sortable treeview)
                    Dim items As MemberItem() = New MemberItem(Me.findNodeResult.Nodes.Count - 1) {}
                    For n As Integer = 0 To Me.findNodeResult.Nodes.Count - 1
                        Dim memberItem As New MemberItem()
                        memberItem.DisplayText = Me.findNodeResult.Nodes(n).Text
                        memberItem.Tag = Me.findNodeResult.Nodes(n).Tag

                        If Me.findNodeResult.Nodes(n).Tag IsNot Nothing Then
                            System.Diagnostics.Debug.WriteLine(Me.findNodeResult.Nodes(n).Tag.[GetType]().ToString())
                        End If

                        items(n) = memberItem
                    Next
                    Array.Sort(items)

                    For n As Integer = 0 To items.Length - 1
                        Dim imageindex As Integer = 0

                        If items(n).Tag IsNot Nothing Then
                            ' Default to method (contains text for parameters)
                            imageindex = 2
                            If TypeOf items(n).Tag Is MemberTypes Then
                                Dim memberType As MemberTypes = CType(items(n).Tag, MemberTypes)

                                Select Case memberType
                                    Case MemberTypes.[Custom]
                                        imageindex = 1
                                        Exit Select
                                    Case MemberTypes.[Property]
                                        imageindex = 3
                                        Exit Select
                                    Case MemberTypes.[Event]
                                        imageindex = 4
                                        Exit Select
                                End Select
                            End If
                        End If

                        Me.listBoxAutoComplete.Items.Add(New GListBoxItem(items(n).DisplayText, imageindex))
                    Next
                End If
            End If
        End If

        Return result
    End Function

    ''' <summary>
    ''' Autofills the selected item in the member listbox, by
    ''' taking everything before and after the "." in the richtextbox,
    ''' and appending the word in the middle.
    ''' </summary>
    Private Sub selectItem()
        If Me.wordMatched Then
            Dim selstart As Integer = Me.txtScript.Selection.LogicalSelStart
            Dim prefixend As Integer = Me.txtScript.Selection.LogicalSelStart - typed.Length
            Dim suffixstart As Integer = Me.txtScript.Selection.LogicalSelStart + typed.Length

            If suffixstart >= Me.txtScript.Document.Text.Length Then
                suffixstart = Me.txtScript.Document.Text.Length
            End If

            Dim prefix As String = Me.txtScript.Document.Text.Substring(0, prefixend)
            Dim fill As String = Me.listBoxAutoComplete.SelectedItem.ToString()
            Dim suffix As String = Me.txtScript.Document.Text.Substring(suffixstart, Me.txtScript.Document.Text.Length - suffixstart)

            Me.txtScript.Document.Text = prefix & fill & suffix
            'Me.txtScript.Selection.LogicalSelStart = prefix.Length + fill.Length
            Me.txtScript.Caret.MoveEnd(False)
        End If
    End Sub

    ''' <summary>
    ''' Searches backwards from the current caret position, until
    ''' a space or newline is found.
    ''' </summary>
    ''' <returns>The previous word from the carret position</returns>
    Private Function getLastWord() As String
        Dim word As String = ""

        Dim pos As Integer = Me.txtScript.Selection.LogicalSelStart
        If pos > 1 Then

            Dim tmp As String = ""
            Dim f As New Char()
            While f <> " " And pos > 0
                pos -= 1
                tmp = Me.txtScript.Document.Text.Substring(pos, 1)
                f = CChar(tmp(0))
                word += f
            End While

            Dim ca As Char() = word.ToCharArray()
            Array.Reverse(ca)

            word = New [String](ca)
        End If
        Return word.Trim()

    End Function

#End Region

End Class




