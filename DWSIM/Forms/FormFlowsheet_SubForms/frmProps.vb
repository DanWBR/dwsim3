Imports WeifenLuo.WinFormsUI.Docking
Imports Microsoft.Msdn.Samples.GraphicObjects

Imports DWSIM.DWSIM.Flowsheet.FlowsheetSolver
Imports DWSIM.DWSIM
Imports System.Text
Imports PropertyGridEx

Public Class frmProps
    Inherits DockContent

    Private ChildParent As FormFlowsheet
    Private Conversor As New DWSIM.SistemasDeUnidades.Conversor

    Private Sub _Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        ChildParent = My.Application.ActiveSimulation

    End Sub

    Public Function ReturnForm(ByVal str As String) As IDockContent

        If str = Me.ToString Then
            Return Me
        Else
            Return Nothing
        End If

    End Function

    Public Sub PGEx1_PropertyValueChanged(ByVal s As Object, ByVal e As System.Windows.Forms.PropertyValueChangedEventArgs) Handles PGEx1.PropertyValueChanged

        If TypeOf Me.ParentForm Is FormFlowsheet Then
            ChildParent = Me.ParentForm
        Else
            ChildParent = My.Application.ActiveSimulation
        End If

        Dim sobj As Microsoft.MSDN.Samples.GraphicObjects.GraphicObject = ChildParent.FormSurface.FlowsheetDesignSurface.SelectedObject

        'handle changes internally
        If Not sobj Is Nothing Then

            If sobj.TipoObjeto <> TipoObjeto.GO_Tabela And sobj.TipoObjeto <> TipoObjeto.GO_MasterTable And sobj.TipoObjeto <> TipoObjeto.GO_SpreadsheetTable Then

                ChildParent.Collections.ObjectCollection(sobj.Name).PropertyValueChanged(s, e)

            End If

        End If

    End Sub

    Public Sub HandleObjectStatusChanged(ByVal obj As Microsoft.Msdn.Samples.GraphicObjects.GraphicObject)

        If obj.Active = False Then
            LblStatusObj.Text = DWSIM.App.GetLocalString("Inativo")
            LblStatusObj.ForeColor = Color.DimGray
        ElseIf obj.Calculated = False Then
            LblStatusObj.Text = DWSIM.App.GetLocalString("NoCalculado")
            LblStatusObj.ForeColor = Color.Red
        Else
            LblStatusObj.Text = DWSIM.App.GetLocalString("Calculado")
            LblStatusObj.ForeColor = Color.DarkGreen
        End If

    End Sub


    Private Sub PGEx2_PropertyValueChanged(ByVal s As Object, ByVal e As System.Windows.Forms.PropertyValueChangedEventArgs) Handles PGEx2.PropertyValueChanged
        If TypeOf Me.ParentForm Is FormFlowsheet Then
            ChildParent = Me.ParentForm
        Else
            ChildParent = My.Application.ActiveSimulation
        End If
        If e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Nome")) Then
            Try
                If Not ChildParent.Collections.ObjectCollection(ChildParent.FormSurface.FlowsheetDesignSurface.SelectedObject.Name).Tabela Is Nothing Then
                    ChildParent.Collections.ObjectCollection(ChildParent.FormSurface.FlowsheetDesignSurface.SelectedObject.Name).Tabela.HeaderText = ChildParent.FormSurface.FlowsheetDesignSurface.SelectedObject.Tag
                End If
                ChildParent.FormObjList.TreeViewObj.Nodes.Find(ChildParent.FormSurface.FlowsheetDesignSurface.SelectedObject.Name, True)(0).Text = e.ChangedItem.Value
            Catch ex As Exception
                'ChildParent.WriteToLog(ex.ToString, Color.Red, FormClasses.TipoAviso.Erro)
            Finally
                'CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.OldValue, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).Tag = e.ChangedItem.Value
                For Each g As GraphicObject In ChildParent.FormSurface.FlowsheetDesignSurface.drawingObjects
                    If g.TipoObjeto = TipoObjeto.GO_MasterTable Then
                        CType(g, DWSIM.GraphicObjects.MasterTableGraphic).Update(ChildParent)
                    End If
                Next
            End Try
            ChildParent.FormSurface.FlowsheetDesignSurface.Invalidate()
        End If
    End Sub

    Private Sub ToolStripButton1_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles ToolStripButton1.Click

        If sfdxml1.ShowDialog = Windows.Forms.DialogResult.OK Then
            Dim str As New StringBuilder
            str.AppendLine(Label1.Text & vbTab & LblNomeObj.Text)
            str.AppendLine(Label3.Text & vbTab & LblTipoObj.Text)
            str.AppendLine(Label4.Text & vbTab & LblStatusObj.Text)
            str.AppendLine(DWSIM.App.GetLocalString("Propriedades") & ":")
            For Each item As CustomProperty In PGEx1.Item
                If TypeOf item.Value Is CustomPropertyCollection Then
                    For Each item2 As CustomProperty In item.Value
                        str.AppendLine("[" & item.Category.ToString & "] " & vbTab & item.Name & vbTab & item2.Name & vbTab & item2.Value)
                    Next
                Else
                    str.AppendLine("[" & item.Category.ToString & "] " & vbTab & item.Name & vbTab & item.Value.ToString)
                End If
            Next
            IO.File.WriteAllText(sfdxml1.FileName, str.ToString)
        End If

    End Sub

    Private Sub FloatToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles FloatToolStripMenuItem.Click, DocumentToolStripMenuItem.Click,
                                                                         DockLeftToolStripMenuItem.Click, DockLeftAutoHideToolStripMenuItem.Click,
                                                                         DockRightAutoHideToolStripMenuItem.Click, DockRightToolStripMenuItem.Click,
                                                                         DockTopAutoHideToolStripMenuItem.Click, DockTopToolStripMenuItem.Click,
                                                                         DockBottomAutoHideToolStripMenuItem.Click, DockBottomToolStripMenuItem.Click

        For Each ts As ToolStripMenuItem In dckMenu.Items
            ts.Checked = False
        Next

        sender.Checked = True

        Select Case sender.Name
            Case "FloatToolStripMenuItem"
                Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.Float
            Case "DocumentToolStripMenuItem"
                Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.Document
            Case "DockLeftToolStripMenuItem"
                Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockLeft
            Case "DockLeftAutoHideToolStripMenuItem"
                Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockLeftAutoHide
            Case "DockRightAutoHideToolStripMenuItem"
                Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockRightAutoHide
            Case "DockRightToolStripMenuItem"
                Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockRight
            Case "DockBottomAutoHideToolStripMenuItem"
                Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockBottomAutoHide
            Case "DockBottomToolStripMenuItem"
                Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockBottom
            Case "DockTopAutoHideToolStripMenuItem"
                Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockTopAutoHide
            Case "DockTopToolStripMenuItem"
                Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockTop
            Case "HiddenToolStripMenuItem"
                Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.Hidden
        End Select

    End Sub

End Class