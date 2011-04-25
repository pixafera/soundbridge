Imports System.Reflection
Imports System.Threading

''' <summary>
''' Contains metadata about Subscription RCP Commands.
''' </summary>
''' <remarks></remarks>
Public NotInheritable Class RcpSubscriptionCommandAttribute
    Inherits Pixa.Soundbridge.Library.RcpCommandAttribute

    Private _eventRaiserMethodName As String

    Public Sub New(ByVal command As String, ByVal eventRaiserMethodName As String)
        MyBase.New(command)
        _eventRaiserMethodName = eventRaiserMethodName
    End Sub

    Public ReadOnly Property EventRaiserMethodName() As String
        Get
            Return _eventRaiserMethodName
        End Get
    End Property

    Public Overrides Function CreateResponseProcessor(ByVal client As SoundbridgeClient, ByVal waitHandle As System.Threading.EventWaitHandle) As IResponseProcessor
        Dim d As Action(Of String) = [Delegate].CreateDelegate(GetType(Action(Of String)), client, EventRaiserMethodName)
        Return New SubscriptionResponseProcessor(client, Command, waitHandle, d)
    End Function
End Class
