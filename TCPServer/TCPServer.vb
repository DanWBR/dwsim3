'    DWSIM Network TCP Flowsheet Solver Server & Auxiliary Functions
'    Copyright 2015 Daniel Wagner O. de Medeiros
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
Imports System.Threading.Tasks
Imports DWSIM

Module TCPServer

    Private server As TcpComm.Server
    Private lat As TcpComm.Utilities.LargeArrayTransferHelper

    Sub Main(args As String())

        server = New TcpComm.Server(AddressOf Process)
        lat = New TcpComm.Utilities.LargeArrayTransferHelper(server)
        server.Start(args(0)) 'port

        While server.IsRunning
            Console.WriteLine("Server is running and listening to incoming data on port " & args(0) & "...")
            Threading.Thread.Sleep(5000)
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

        If dataChannel < 251 Then

            Task.Factory.StartNew(Sub()
                                      Using bytestream As New MemoryStream(bytes)
                                          Dim form As FormFlowsheet = DWSIM.DWSIM.SimulationObjects.UnitOps.Flowsheet.InitializeFlowsheet(bytestream)
                                          DWSIM.DWSIM.Flowsheet.FlowsheetSolver.CalculateAll2(form, 1)
                                          Dim retbytes As MemoryStream = DWSIM.DWSIM.SimulationObjects.UnitOps.Flowsheet.ReturnProcessData(form)
                                          lat.SendArray(retbytes.ToArray, dataChannel, sessionID)
                                      End Using
                                  End Sub).ContinueWith(Sub()
                                                            server.GetSession(sessionID).Close()
                                                        End Sub)

        ElseIf dataChannel = 255 Then

            Dim tmp = ""
            Dim msg As String = TcpComm.Utilities.BytesToString(bytes)
            ' server has finished sending the bytes you put into sendBytes()
            If msg.Length > 3 Then tmp = msg.Substring(0, 3)
            If tmp = "UBS" Then ' User Bytes Sent.

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

End Module
