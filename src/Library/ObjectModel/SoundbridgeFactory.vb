Imports System.Net

Public Module SoundbridgeFactory

    Public Function CreateFromTcp(ByVal endPoint As IPEndPoint) As Soundbridge
        Return New Soundbridge(New TcpSoundbridgeClient(endPoint))
    End Function

    Public Function CreateFromTcp(ByVal hostname As String) As Soundbridge
        Return New Soundbridge(New TcpSoundbridgeClient(hostname))
    End Function

    Public Function CreateFromTcp(ByVal hostname As String, ByVal port As Integer) As Soundbridge
        Return New Soundbridge(New TcpSoundbridgeClient(hostname, port))
    End Function

End Module
