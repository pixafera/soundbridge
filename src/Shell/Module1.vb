Imports System.IO
Imports System.Net
Imports System.Net.Sockets

Module Module1

    Dim _client As TcpClient

    Sub Main(ByVal args() As String)
        _client = New TcpClient(args(0), 5555)

        Dim d As New Action(AddressOf ReadCon)
        d.BeginInvoke(Nothing, Nothing)

        Dim input As String
        Dim sw As New StreamWriter(_client.GetStream)

        Do
            input = Console.ReadLine
            If input = "exit" Then Exit Sub

            sw.Write(input & vbCrLf)
            sw.Flush()
            If Not _client.Connected Then Exit Sub
        Loop
    End Sub

    Sub ReadCon()
        Dim sr As New StreamReader(_client.GetStream)

        While _client.Connected
            Console.WriteLine(sr.ReadLine)
        End While
    End Sub
End Module
