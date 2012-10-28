
Imports DWSIM.DWSIM.ClassesBasicasTermodinamica
Imports OutlookStyleControls

Public Class FormConfigCAPEOPEN2

    Inherits FormConfigBase

    Public _selcomps As Dictionary(Of String, DWSIM.ClassesBasicasTermodinamica.ConstantProperties)
    Public _availcomps As Dictionary(Of String, DWSIM.ClassesBasicasTermodinamica.ConstantProperties)

    Public loaded As Boolean = False

    Private prevsort As System.ComponentModel.ListSortDirection = System.ComponentModel.ListSortDirection.Ascending
    Private prevcol As Integer = 1
    Private prevgroup As OutlookStyleControls.IOutlookGridGroup

    Dim ACSC1 As AutoCompleteStringCollection

    Private Sub FormConfigCAPEOPEN2_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Application.EnableVisualStyles()
        Application.DoEvents()

        Me.lblName.Text = _pp.ComponentName
        Me.lblDescription.Text = _pp.ComponentDescription

        Dim comp As DWSIM.ClassesBasicasTermodinamica.ConstantProperties
        If Not loaded Then

            ACSC1 = New AutoCompleteStringCollection

            For Each comp In _selcomps.Values
                Me.ListViewA.Items.Add(comp.Name, DWSIM.App.GetComponentName(comp.Name), 0).Tag = comp.Name
            Next
            For Each comp In _availcomps.Values
                Dim idx As Integer = Me.AddCompToGrid(comp)
                If Not idx = -1 Then
                    For Each c As DataGridViewCell In Me.ogc1.Rows(idx).Cells
                        If comp.Acentric_Factor = 0.0# Or comp.Critical_Compressibility = 0.0# Then
                            c.Style.ForeColor = Color.Red
                            c.ToolTipText = DWSIM.App.GetLocalString("CompMissingData")
                        End If
                    Next
                End If
            Next

        Else

            For Each r As DataGridViewRow In ogc1.Rows
                If _availcomps.ContainsKey(r.Cells(0).Value) Then
                    comp = _availcomps(r.Cells(0).Value)
                    For Each c As DataGridViewCell In r.Cells
                        If comp.Acentric_Factor = 0.0# Or comp.Critical_Compressibility = 0.0# Then
                            c.Style.ForeColor = Color.Red
                            c.ToolTipText = DWSIM.App.GetLocalString("CompMissingData")
                        End If
                    Next
                End If
            Next

            Try
                Me.ogc1.GroupTemplate = Nothing
                Me.ogc1.Sort(ogc1.Columns(1), System.ComponentModel.ListSortDirection.Ascending)
            Catch ex As Exception
            End Try

        End If

        Select Case _pp.FlashAlgorithm
            Case DWSIM.SimulationObjects.PropertyPackages.FlashMethod.DWSIMDefault
                ComboBoxFlashAlg.SelectedIndex = 0
            Case DWSIM.SimulationObjects.PropertyPackages.FlashMethod.InsideOut
                ComboBoxFlashAlg.SelectedIndex = 1
            Case DWSIM.SimulationObjects.PropertyPackages.FlashMethod.InsideOut3P
                ComboBoxFlashAlg.SelectedIndex = 2
            Case DWSIM.SimulationObjects.PropertyPackages.FlashMethod.GibbsMin2P
                ComboBoxFlashAlg.SelectedIndex = 3
            Case DWSIM.SimulationObjects.PropertyPackages.FlashMethod.GibbsMin3P
                ComboBoxFlashAlg.SelectedIndex = 4
            Case DWSIM.SimulationObjects.PropertyPackages.FlashMethod.NestedLoops3P
                ComboBoxFlashAlg.SelectedIndex = 5
            Case Else
                ComboBoxFlashAlg.SelectedIndex = 0
        End Select

        chkIOmode.Checked = _pp._ioquick

        Dim comps, selected As New ArrayList
        If _pp._tpcompids Is Nothing Then _pp._tpcompids = New String() {}
        For Each c As ConstantProperties In _selcomps.Values
            comps.Add(c.Name)
            For Each s As String In _pp._tpcompids
                If s = c.Name Then
                    selected.Add(c.Name)
                    Exit For
                End If
            Next
        Next

        Me.ListView2.Items.Clear()

        Dim i, n As Integer
        n = comps.Count - 1
        For i = 0 To n
            With Me.ListView2.Items.Add(DWSIM.App.GetComponentName(comps(i)))
                For Each s As String In selected
                    If s = comps(i) Then
                        .Checked = True
                        Exit For
                    End If
                Next
                .Tag = comps(i)
            End With
        Next

        Select Case _pp._tpseverity
            Case 0
                Me.RadioButton1.Checked = True
            Case 1
                Me.RadioButton2.Checked = True
            Case 2
                Me.RadioButton3.Checked = True
        End Select

        Me.loaded = True

    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        With Me.ofdcs
            If .ShowDialog = Windows.Forms.DialogResult.OK Then
                Dim filename As String = .FileName
                FormMain.LoadCSDB(filename)
                'chemsep database
                If FormMain.loadedCSDB Then
                    My.Settings.ChemSepDatabasePath = filename
                    Dim name, path2 As String
                    name = "ChemSep"
                    path2 = My.Settings.ChemSepDatabasePath
                    Me.dgvdb.Rows.Add(New Object() {dgvdb.Rows.Count + 1, My.Resources.information, name, path2, DWSIM.App.GetLocalString("Remove")})
                    Me.dgvdb.Rows(Me.dgvdb.Rows.Count - 1).Cells(4).ReadOnly = True
                    For Each r As DataGridViewRow In Me.dgvdb.Rows
                        r.Height = "40"
                    Next
                End If
                MessageBox.Show(DWSIM.App.GetLocalString("NextStartupOnly"))
            End If
        End With
    End Sub

    Private Sub ComboBoxFlashAlg_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ComboBoxFlashAlg.SelectedIndexChanged
        Select Case ComboBoxFlashAlg.SelectedIndex
            Case 0
                Me._pp.FlashAlgorithm = DWSIM.SimulationObjects.PropertyPackages.FlashMethod.DWSIMDefault
                Me.chkIOmode.Enabled = False
                Me.GroupBox11.Enabled = False
            Case 1
                Me._pp.FlashAlgorithm = DWSIM.SimulationObjects.PropertyPackages.FlashMethod.InsideOut
                Me.chkIOmode.Enabled = True
                Me.GroupBox11.Enabled = False
            Case 2
                Me._pp.FlashAlgorithm = DWSIM.SimulationObjects.PropertyPackages.FlashMethod.InsideOut3P
                Me.chkIOmode.Enabled = True
                Me.GroupBox11.Enabled = True
            Case 3
                Me._pp.FlashAlgorithm = DWSIM.SimulationObjects.PropertyPackages.FlashMethod.GibbsMin2P
                Me.chkIOmode.Enabled = False
                Me.GroupBox11.Enabled = False
            Case 4
                Me._pp.FlashAlgorithm = DWSIM.SimulationObjects.PropertyPackages.FlashMethod.GibbsMin3P
                Me.chkIOmode.Enabled = False
                Me.GroupBox11.Enabled = True
            Case 5
                Me._pp.FlashAlgorithm = DWSIM.SimulationObjects.PropertyPackages.FlashMethod.NestedLoops3P
                Me.chkIOmode.Enabled = True
                Me.GroupBox11.Enabled = True
        End Select
    End Sub

    Private Sub chkIOmode_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkIOmode.CheckedChanged
        Me._pp._ioquick = chkIOmode.Checked
    End Sub

    Private Sub RadioButton1_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RadioButton1.CheckedChanged, RadioButton2.CheckedChanged, RadioButton3.CheckedChanged

        If Me.RadioButton1.Checked Then _pp._tpseverity = 0
        If Me.RadioButton2.Checked Then _pp._tpseverity = 1
        If Me.RadioButton3.Checked Then _pp._tpseverity = 2

    End Sub

    Public Function AddCompToGrid(ByRef comp As DWSIM.ClassesBasicasTermodinamica.ConstantProperties) As Integer

        Dim contains As Boolean = False
        For Each r As OutlookGridRow In ogc1.Rows
            If r.Cells(0).Value = comp.Name Then contains = True
        Next

        If Not contains Then
            Dim r As New OutlookGridRow
            r.CreateCells(ogc1, New Object() {comp.Name, DWSIM.App.GetComponentName(comp.Name), comp.OriginalDB, DWSIM.App.GetComponentType(comp), comp.Formula})
            ogc1.Rows.Add(r)
            ACSC1.Add(r.Cells(1).Value.ToString)
            Me.TextBox1.AutoCompleteCustomSource = ACSC1
            Return ogc1.Rows.Count - 1
        Else
            Return -1
        End If

    End Function

    Private Sub Button7_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button7.Click
        If Me.ogc1.SelectedRows.Count > 0 Then
            Me.AddCompToSimulation(Me.ogc1.SelectedRows(0).Index)
        End If
    End Sub

    Private Sub Button10_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button10.Click
        If Me.ListViewA.SelectedItems.Count > 0 Then
            Me.RemoveCompFromSimulation(Me.ListViewA.SelectedItems(0).Tag)
        End If
    End Sub

    Private Sub Button11_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button11.Click
        For Each lvi As ListViewItem In Me.ListViewA.Items
            Me.RemoveCompFromSimulation(lvi.Tag)
        Next
    End Sub

    Private Sub Button6_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button6.Click
        Me.ogc1.GroupTemplate = Nothing
        Me.ogc1.Sort(ogc1.Columns(1), System.ComponentModel.ListSortDirection.Ascending)
    End Sub

    Sub AddComponent(ByVal compID As String)
        If Not _selcomps.ContainsKey(compID) Then
            Dim tmpcomp As New DWSIM.ClassesBasicasTermodinamica.ConstantProperties
            tmpcomp = _availcomps(compID)
            _selcomps.Add(tmpcomp.Name, tmpcomp)
            _availcomps.Remove(tmpcomp.Name)
            Me.ListViewA.Items.Add(tmpcomp.Name, DWSIM.App.GetComponentName(tmpcomp.Name), 0).Tag = tmpcomp.Name
        End If
    End Sub

    Sub RemoveComponent(ByVal compID As String)
        Me.RemoveCompFromSimulation(compID)
    End Sub

    Sub AddCompToSimulation(ByVal index As Integer)

        If Me.loaded Then
            If Not _selcomps.ContainsKey(ogc1.Rows(index).Cells(0).Value) Then
                Dim tmpcomp As New DWSIM.ClassesBasicasTermodinamica.ConstantProperties
                tmpcomp = _availcomps(ogc1.Rows(index).Cells(0).Value)
                _selcomps.Add(tmpcomp.Name, tmpcomp)
                _availcomps.Remove(tmpcomp.Name)
                Me.ListViewA.Items.Add(tmpcomp.Name, DWSIM.App.GetComponentName(tmpcomp.Name), 0).Tag = tmpcomp.Name
                Me.ogc1.Rows.RemoveAt(index)
            End If
        End If

    End Sub

    Sub RemoveCompFromSimulation(ByVal compid As String)

        Dim tmpcomp As New DWSIM.ClassesBasicasTermodinamica.ConstantProperties
        Dim nm As String = compid
        tmpcomp = _selcomps(nm)
        _selcomps.Remove(tmpcomp.Name)
        Me.ListViewA.Items.RemoveByKey(tmpcomp.Name)
        _availcomps.Add(tmpcomp.Name, tmpcomp)
        Me.AddCompToGrid(tmpcomp)

    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        _pp.ReconfigureConfigForm()
        _pp.ConfigForm._pp = _pp
        _pp.ConfigForm._comps = _selcomps
        _pp.ShowConfigForm()
    End Sub

    Private Sub TextBox1_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBox1.TextChanged
        For Each r As DataGridViewRow In ogc1.Rows
            If Not r.Cells(1).Value Is Nothing Then
                If r.Cells(1).Value.ToString = Me.TextBox1.Text Then
                    r.Selected = True
                    If r.Visible Then ogc1.FirstDisplayedScrollingRowIndex = r.Index
                Else
                    r.Selected = False
                End If
            End If
        Next
        If TextBox1.Text = "" Then
            ogc1.FirstDisplayedScrollingRowIndex = 0
            For Each r As DataGridViewRow In ogc1.Rows
                r.Selected = False
            Next
        End If
    End Sub

    Private Sub Button4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button4.Click
        If Me.ListView1.SelectedIndices.Count > 0 Then
            Select Case Me.ListView1.SelectedIndices.Item(0)
                Case 0
                    My.Settings.CultureInfo = "pt-BR"
                Case 1
                    My.Settings.CultureInfo = "en-US"
                Case 2
                    My.Settings.CultureInfo = "es"
                Case 3
                    My.Settings.CultureInfo = "de"
            End Select
            My.Settings.Save()
            My.Application.ChangeUICulture(My.Settings.CultureInfo)
            MessageBox.Show(DWSIM.App.GetLocalString("NextStartupOnly"))
        End If
    End Sub

    Private Sub ListView2_ItemChecked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.ItemCheckedEventArgs) Handles ListView2.ItemChecked

        If loaded Then

            Try
                Dim i As Integer = 0
                Dim sel As New ArrayList
                For Each lvi2 As ListViewItem In Me.ListView2.Items
                    If lvi2.Checked Then sel.Add(lvi2.Tag)
                Next
                _pp._tpcompids = sel.ToArray(Type.GetType("System.String"))
            Catch ex As Exception

            End Try

        End If

    End Sub

    Private Sub TextBox1_KeyDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles TextBox1.KeyDown
        If e.KeyCode = Keys.Enter Then
            Call Button7_Click(sender, e)
            Me.TextBox1.Text = ""
        End If
    End Sub

End Class