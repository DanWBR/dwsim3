Imports DWSIM.DWSIM.SimulationObjects

Public Class frmMatList

    Inherits WeifenLuo.WinFormsUI.Docking.DockContent

    Protected Conversor As DWSIM.SistemasDeUnidades.Conversor
    Protected filename As String = ""
    Protected ChildParent As FormFlowsheet
    Protected RowsCreated As Boolean = False

    Public Function ReturnForm(ByVal str As String) As WeifenLuo.WinFormsUI.Docking.IDockContent

        If str = Me.ToString Then
            Return Me
        Else
            Return Nothing
        End If

    End Function

    Private Sub frmMatList_Enter(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Enter
        Me.ChildParent = My.Application.ActiveSimulation
        If Not ChildParent Is Nothing Then
            'TABELA DE CORRENTES
            Dim ms As DWSIM.SimulationObjects.Streams.MaterialStream
            DataGridView1.Columns.Clear()
            RowsCreated = False
            For Each ms In ChildParent.Collections.CLCS_MaterialStreamCollection.Values
                AddColumn(ms)
            Next
        End If
    End Sub

    Private Sub frmMatList_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Me.ChildParent = My.Application.ActiveSimulation

    End Sub

    Sub AddColumn(ByRef ms As DWSIM.SimulationObjects.Streams.MaterialStream)

        Me.ChildParent = My.Application.ActiveSimulation
        Me.Conversor = New DWSIM.SistemasDeUnidades.Conversor

        Dim su As DWSIM.SistemasDeUnidades.Unidades
        su = ChildParent.Options.SelectedUnitSystem

        Me.DataGridView1.Columns.Add(ms.Nome, ms.GraphicObject.Tag)
        Me.DataGridView1.Columns(ms.Nome).SortMode = DataGridViewColumnSortMode.NotSortable

        Dim props As String() = ms.GetProperties(SimulationObjects_BaseClass.PropertyType.RW)
        Dim unit As String = ""

        If Not RowsCreated Then

            Me.SuspendLayout()

            'create rows
            For Each prop As String In props
                With Me.DataGridView1.Rows
                    .Add()
                    unit = ms.GetPropertyUnit(prop, su)
                    If unit <> "" Then
                        .Item(.Count - 1).HeaderCell.Value = DWSIM.App.GetPropertyName(prop) & " (" & ms.GetPropertyUnit(prop, su) & ")"
                    Else
                        .Item(.Count - 1).HeaderCell.Value = DWSIM.App.GetPropertyName(prop)
                    End If
                End With
            Next

            Me.ResumeLayout()

        End If

        RowsCreated = True

        'populate rows
        Dim col As DataGridViewColumn = Me.DataGridView1.Columns(ms.Nome)
        Dim i As Integer = 0
        Dim value As String

        For Each prop As String In props
            value = ms.GetPropertyValue(prop, su)
            If Double.TryParse(value, New Double) Then
                Me.DataGridView1.Rows.Item(i).Cells(col.Index).Value = Format(Double.Parse(value), ChildParent.Options.NumberFormat)
            Else
                Me.DataGridView1.Rows.Item(i).Cells(col.Index).Value = value
            End If
            i += 1
        Next

    End Sub

End Class