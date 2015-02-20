'    DWSIM Network TCP Flowsheet Solver Client & Auxiliary Functions
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
Imports DWSIM.DWSIM.SimulationObjects
Imports System.Threading.Tasks
Imports System.Threading

Namespace DWSIM.Flowsheet
    Public Class TCPSolverClient

        Public IsClosing As Boolean = False
        Public client As TcpComm.Client
        Public lat As TcpComm.Utilities.LargeArrayTransferHelper

        Dim results As Byte()

        Public Sub SolveFlowsheet(fs As FormFlowsheet)

            Dim errMsg As String = ""

            client = New TcpComm.Client(AddressOf Update, True, 15)

            If client.Connect(My.Settings.ServerIPAddress, My.Settings.ServerPort, My.User.Name & "@" & My.Computer.Name, errMsg) Then

                Dim tmpfile As String = My.Computer.FileSystem.GetTempFileName

                FormMain.SaveXML(tmpfile, fs)

                results = Nothing

                lat = New TcpComm.Utilities.LargeArrayTransferHelper(client)

                lat.SendArray(IO.File.ReadAllBytes(tmpfile), 1)

                File.Delete(tmpfile)

                While results.Length = 0
                    Thread.Sleep(1000)
                End While

                Using ms As New MemoryStream(results)

                    Dim xdoc As XDocument = XDocument.Load(ms)

                    DWSIM.SimulationObjects.UnitOps.Flowsheet.UpdateProcessData(fs, xdoc)

                End Using

            Else

                If errMsg.Trim <> "" Then MsgBox(errMsg)

            End If

        End Sub

        Public Sub Update(ByVal bytes() As Byte, ByVal dataChannel As Byte)

            ' Use TcpComm.Utilities.LargeArrayTransferHelper to make it easier to send and receive 
            ' large arrays sent via lat.SendArray()
            ' The LargeArrayTransferHelperb will assemble an incoming large array
            ' on any channel we choose to evaluate, and pass it back to this callback
            ' when it is complete. Returns True if it has handled this incomming packet,
            ' so we exit the callback when it returns true.
            If Not lat Is Nothing AndAlso lat.HandleIncomingBytes(bytes, dataChannel, , {100, 100}) Then Return

            ' We're on the main UI thread now.
            Dim dontReport As Boolean = False

            If dataChannel < 251 Then

                ' This is a large array delivered by LAT. Display it in the 
                ' large transfer viewer form.
                results = bytes

            ElseIf dataChannel = 255 Then

                Dim msg As String = TcpComm.Utilities.BytesToString(bytes)
                Dim tmp As String = ""

                ' _Client as finished sending the bytes you put into sendBytes()
                If msg.Length > 4 Then tmp = msg.Substring(0, 4)
                If tmp = "UBS:" Then ' User Bytes Sent on channel:???.
                End If

                ' We have an error message. Could be local, or from the server.
                If msg.Length > 4 Then tmp = msg.Substring(0, 5)
                If tmp = "ERR: " Then
                    Dim msgParts() As String
                    msgParts = Split(msg, ": ")
                    MsgBox("" & msgParts(1), MsgBoxStyle.Critical, "Test Tcp Communications App")
                    dontReport = True
                End If

                'If msg.Equals("Disconnected.") Then Me.Button2.Text = "Connect"

            End If

        End Sub

    End Class

End Namespace
