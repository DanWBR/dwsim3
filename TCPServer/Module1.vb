Imports System.IO
Imports System.Threading.Tasks
Imports DWSIM

Module Module1

    Private server As TcpComm.Server
    Private lat As TcpComm.Utilities.LargeArrayTransferHelper

    Sub Main()

        server = New TcpComm.Server(AddressOf Process)
        lat = New TcpComm.Utilities.LargeArrayTransferHelper(server)
        server.Start(22490)

        While server.IsRunning
            Threading.Thread.Sleep(1000)
        End While

    End Sub

    Public Sub Process(ByVal bytes() As Byte, ByVal sessionID As Int32, ByVal dataChannel As Byte)

        ' Use TcpComm.Utilities.LargeArrayTransferHelper to make it easier to send and receive 
        ' large arrays sent via lat.SendArray()
        ' The LargeArrayTransferHelperb will assemble any number of incoming large arrays
        ' on any channel or from any sessionId, and pass them back to this callback
        ' when they are complete. Returns True if it has handled this incomming packet,
        ' so we exit the callback when it returns true.
        If lat.HandleIncomingBytes(bytes, dataChannel, sessionID) Then Return

        ' We're on the main UI thread now.
        If dataChannel < 251 Then

            'Me.lbTextInput.Items.Add("Session " & sessionID.ToString & ": " & TcpComm.Utilities.BytesToString(bytes))

            'receive bytes and convert them to a stream

            Using bytestream As New MemoryStream(bytes)

                Dim form As FormFlowsheet = DWSIM.DWSIM.SimulationObjects.UnitOps.Flowsheet.InitializeFlowsheet(bytestream)

                DWSIM.DWSIM.Flowsheet.FlowsheetSolver.CalculateAll2(form, 1)

                Dim retbytes As MemoryStream = DWSIM.DWSIM.SimulationObjects.UnitOps.Flowsheet.ReturnProcessData(form)

                server.SendBytes(retbytes.ToArray, dataChannel, sessionID)

            End Using

        ElseIf dataChannel = 255 Then

            Dim tmp = ""
            Dim msg As String = TcpComm.Utilities.BytesToString(bytes)
            Dim dontReport As Boolean = False
            ' server has finished sending the bytes you put into sendBytes()
            If msg.Length > 3 Then tmp = msg.Substring(0, 3)
            If tmp = "UBS" Then ' User Bytes Sent.
                Dim parts() As String = Split(msg, "UBS:")
                msg = "Data sent to session: " & parts(1)
            End If

        End If

    End Sub

    Private Sub UpdateClientsList()

        Dim sessionList As List(Of TcpComm.Server.SessionCommunications) = server.GetSessionCollection()

        For Each session As TcpComm.Server.SessionCommunications In sessionList
            If session.IsRunning Then
                'lvi = New ListViewItem(" Connected", 0, lvClients.Groups.Item(0))
            Else
                ' lvi = New ListViewItem(" Disconnected", 1, lvClients.Groups.Item(1))
            End If
        Next

    End Sub

    Private Sub SendAFileToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs)

        ' Get the session using the sessionID pulled from the selected listview item
        Dim session As TcpComm.Server.SessionCommunications = server.GetSession(11)
        Dim fileName As String = ""

        If session Is Nothing Then
            MsgBox("This session is disconnected.", MsgBoxStyle.Critical, "TcpDemoApp")
            Return
        End If

        If Not server.SendFile(fileName, session.sessionID) Then
            MsgBox("This session is disconnected.", MsgBoxStyle.Critical, "TcpDemoApp")
        End If

    End Sub

    Private Sub TestLargeArrayTransferHelperToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)

        ' Get the session using the sessionID pulled from the selected listview item
        Dim session As TcpComm.Server.SessionCommunications = server.GetSession(0)

        If session Is Nothing Then
            MsgBox("You can't send a large array to a disconnected session!", MsgBoxStyle.Critical, "TcpComm Demo App")
            Return
        End If

        Dim msg = "This version if the library includes a helper function for people attempting to send arrays larger then the maximum packetsize. " & _
            "In those cases, the array will be broken up into multiple packets, and delivered one by one. This helper class can be used to send the large arrays and " & _
            "have LAT (the TcpComm.Utilities.LargeArrayTransferHelper tool) assemble them for you in the remote machine. " & vbCrLf & vbCrLf & _
            "This test will read about 500k of a large text file into a byte array, and send it to the client you selected (this would normally arrive in about 8 pieces). When it arrives, it will be " & _
            "displayed in the 'Lat Viewer', a form with a multiline textbox on it that you can use to verify that all the text has been delivered and assembled " & _
            "properly, and verify the message size."

        Dim retVal As MsgBoxResult = MsgBox(msg, MsgBoxStyle.Information Or MsgBoxStyle.OkCancel, "TcpComm Demo App")
        If retVal = MsgBoxResult.Ok Then
            If session IsNot Nothing Then
                Dim fileBytes() As Byte = System.IO.File.ReadAllBytes("big.txt")
                Dim errMsg As String = ""
                If Not lat.SendArray(fileBytes, 100, session.sessionID, errMsg) Then MsgBox(errMsg, MsgBoxStyle.Critical, "TcpComm Demo App")
            End If
        End If
    End Sub

End Module
