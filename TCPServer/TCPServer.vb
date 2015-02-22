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
  
    Sub Main()

        Console.WriteLine()
        Console.WriteLine("DWSIM - Open Source Process Simulator")
        Console.WriteLine("Network TCP/IP Solver Server")
        Console.WriteLine(My.Application.Info.Copyright)
        Dim dt As DateTime = CType("01/01/2000", DateTime).AddDays(My.Application.Info.Version.Build).AddSeconds(My.Application.Info.Version.Revision * 2)
        Console.WriteLine("Version " & My.Application.Info.Version.Major & "." & My.Application.Info.Version.Minor & _
        ", Build " & My.Application.Info.Version.Build & " (" & Format(dt, "dd/MM/yyyy HH:mm") & ")")
        Console.WriteLine("Microsoft .NET Framework Runtime Version " & System.Runtime.InteropServices.RuntimeEnvironment.GetSystemVersion.ToString())
        Console.WriteLine()

        server = New TcpComm.Server(AddressOf Process)
        lat = New TcpComm.Utilities.LargeArrayTransferHelper(server)

        Dim port As Integer

        If My.Application.CommandLineArgs.Count > 0 Then
            port = My.Application.CommandLineArgs(0)
        Else
            Console.Write("Please enter the TCP Port Number to listen to: ")
            port = Console.ReadLine()
        End If

        server.Start(port)

        Console.WriteLine("[" & Date.Now.ToString & "] " & "Server is running and listening to incoming data on port " & port & "...")

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
        If lat.HandleIncomingBytes(bytes, 100, sessionID) Then Return

        If dataChannel = 100 Then

            Dim errmsg As String = ""

            Console.WriteLine("[" & Date.Now.ToString & "] " & "Data received from " & server.GetSession(sessionID).machineId & ", flowsheet solving started!")
            If Not server.SendText("Data received from " & server.GetSession(sessionID).machineId & ", flowsheet solving started!", 2, sessionID, errmsg) Then
                Console.WriteLine(errmsg)
            End If

            Task.Factory.StartNew(Sub()
                                      ProcessData(bytes, sessionID, dataChannel)
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

    Sub ProcessData(bytes As Byte(), sessionid As Integer, datachannel As Byte)
        Dim errmsg As String = ""
        Try
            Using bytestream As New MemoryStream(bytes)
                Dim form As FormFlowsheet = DWSIM.DWSIM.SimulationObjects.UnitOps.Flowsheet.InitializeFlowsheet(bytestream)
                DWSIM.DWSIM.Flowsheet.FlowsheetSolver.CalculateAll2(form, 2)
                Dim retbytes As MemoryStream = DWSIM.DWSIM.SimulationObjects.UnitOps.Flowsheet.ReturnProcessData(form)
                Using retbytes
                    Dim uncompressedbytes As Byte() = retbytes.ToArray
                    Using compressedstream As New MemoryStream()
                        Using gzs As New BufferedStream(New Compression.GZipStream(compressedstream, Compression.CompressionMode.Compress, True), 64 * 1024)
                            gzs.Write(uncompressedbytes, 0, uncompressedbytes.Length)
                            gzs.Close()
                            lat.SendArray(compressedstream.ToArray, 100, sessionid, errmsg)
                            Console.WriteLine("[" & Date.Now.ToString & "] " & "Byte array length: " & compressedstream.Length)
                        End Using
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Console.WriteLine("[" & Date.Now.ToString & "] " & "Error solving flowsheet: " & ex.ToString)
            errmsg = ""
            If Not server.SendText("Error solving flowsheet: " & ex.ToString, 3, sessionid, errmsg) Then
                Console.WriteLine(errmsg)
            End If
        Finally
            Console.WriteLine("[" & Date.Now.ToString & "] " & "Closing current session with " & server.GetSession(sessionid).machineId & ".")
            errmsg = ""
            If Not server.SendText("Closing current session with " & server.GetSession(sessionid).machineId & ".", 2, sessionid, errmsg) Then
                Console.WriteLine(errmsg)
            End If
        End Try

    End Sub


End Module
