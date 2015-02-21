'    DWSIM Flowsheet Solver Server for Microsoft Azure (TM) Virtual Machine
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

Imports DWSIM
Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks
Imports Microsoft.ServiceBus
Imports Microsoft.ServiceBus.Messaging

Module AzureServer

    Private nm As NamespaceManager
    Private qc As QueueClient
    Private queueName As String = "DWSIM"

    Sub Main(args() As String)

        Dim connectionString As String = args(0)

        Try

            nm = NamespaceManager.CreateFromConnectionString(connectionString)
            If Not nm.QueueExists(queueName) Then nm.CreateQueue(queueName)

            qc = QueueClient.CreateFromConnectionString(connectionString, queueName)

            Console.WriteLine("Server is clearing messages on queue '" & queueName & "'...")

            While (qc.Peek() IsNot Nothing)
                Dim brokeredMessage = qc.Receive(New TimeSpan(0, 0, 0))
                brokeredMessage.Complete()
            End While

            Console.WriteLine("Cleared ok")

            While True

                Console.WriteLine("Server is running and listening to incoming data on queue '" & queueName & "'...")

                Thread.Sleep(1000)

                Dim message As BrokeredMessage

                message = qc.Receive(New TimeSpan(0, 0, 0))

                If Not message Is Nothing Then

                    If message.Properties("origin") = "client" Then

                        Dim requestID As String = message.Properties("requestID")

                        Try

                            If message.Properties("type") = "data" Then

                                Dim bytes As Byte() = message.GetBody(Of Byte())()

                                message.Complete()

                                If message.Properties("multipart") = False Then
                                    Console.WriteLine("Data received, flowsheet solving started!")
                                    Dim msg As New BrokeredMessage("Data received, flowsheet solving started!")
                                    msg.Properties.Add("requestID", requestID)
                                    msg.Properties.Add("type", "text")
                                    msg.Properties.Add("origin", "server")
                                    qc.Send(msg)
                                    Task.Factory.StartNew(Sub()
                                                              ProcessData(bytes, requestID)
                                                          End Sub)
                                End If

                            End If

                        Catch ex As Exception

                            Dim msg As New BrokeredMessage(ex.ToString)
                            msg.Properties.Add("requestID", requestID)
                            msg.Properties.Add("type", "exception")
                            msg.Properties.Add("origin", "server")
                            qc.Send(msg)

                        End Try

                    End If

                End If

            End While

        Catch ex As Exception

            Console.WriteLine(ex.ToString)

        End Try

    End Sub

    Sub ProcessData(bytes As Byte(), requestID As String)
       Try
            Using bytestream As New MemoryStream(bytes)
                Dim form As FormFlowsheet = DWSIM.DWSIM.SimulationObjects.UnitOps.Flowsheet.InitializeFlowsheet(bytestream)
                DWSIM.DWSIM.Flowsheet.FlowsheetSolver.CalculateAll2(form, 1)
                Dim retbytes As MemoryStream = DWSIM.DWSIM.SimulationObjects.UnitOps.Flowsheet.ReturnProcessData(form)
                Using retbytes
                    Dim uncompressedbytes As Byte() = retbytes.ToArray
                    Using compressedstream As New MemoryStream()
                        Using gzs As New BufferedStream(New Compression.GZipStream(compressedstream, Compression.CompressionMode.Compress, True), 64 * 1024)
                            gzs.Write(uncompressedbytes, 0, uncompressedbytes.Length)
                            gzs.Close()
                            If compressedstream.Length < 220 * 1024 Then
                                Dim msg As New BrokeredMessage(compressedstream.ToArray)
                                msg.Properties.Add("multipart", False)
                                msg.Properties.Add("requestID", requestID)
                                msg.Properties.Add("origin", "server")
                                msg.Properties.Add("type", "data")
                                qc.Send(msg)
                                Console.WriteLine("Byte array length: " & compressedstream.Length)
                            Else
                                Dim i, n As Integer
                                Dim bytearray As ArrayList = Split(compressedstream.ToArray, 220)
                                n = bytearray.Count
                                For Each b As Byte() In bytearray
                                    Dim msg As New BrokeredMessage(compressedstream.ToArray)
                                    msg.Properties.Add("multipart", True)
                                    msg.Properties.Add("partnumber", i)
                                    msg.Properties.Add("totalparts", n)
                                    msg.Properties.Add("type", "data")
                                    msg.Properties.Add("requestID", requestID)
                                    msg.Properties.Add("origin", "server")
                                    qc.Send(msg)
                                Next
                            End If
                        End Using
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Console.WriteLine(ex.ToString)
            Dim msg As New BrokeredMessage(ex.ToString)
            msg.Properties.Add("requestID", requestID)
            msg.Properties.Add("type", "exception")
            msg.Properties.Add("origin", "server")
            qc.Send(msg)
        End Try

    End Sub

    Private Function Split(filebytes As Byte(), partsizeKB As Integer) As ArrayList

        partsizeKB *= 1000

        Dim pos As Integer = 0
        Dim remaining As Integer

        Dim result As New ArrayList()

        While remaining > 0

            Dim block As Byte() = New Byte(Math.Min(remaining, partsizeKB) - 1) {}

            Array.Copy(filebytes, pos, block, 0, block.Length)
            result.Add(block)

            pos += block.Length
            remaining = filebytes.Length - pos

        End While

        Return result

    End Function

End Module
