Imports System.Reflection
Imports System.Threading

''' <summary>
''' Contains metadata about Subscription RCP Commands.
''' </summary>
''' <remarks></remarks>
Public NotInheritable Class RcpSubscriptionCommandAttribute
   Inherits Pixa.Soundbridge.Library.RcpCommandAttribute

   Private _eventRaiserMethodName As String

   ''' <summary>
   ''' Initialises a new instance of <see cref="RcpSubscriptionCommandAttribute"/>.
   ''' </summary>
   ''' <param name="command">The name of the command to be sent to the
   ''' soundbridge.</param>
   ''' <param name="eventRaiserMethodName">The name of the method to be called
   ''' when a subscription notification is received.</param>
   Public Sub New(ByVal command As String, ByVal eventRaiserMethodName As String)
      MyBase.New(command)
      _eventRaiserMethodName = eventRaiserMethodName
   End Sub

   ''' <summary>
   ''' Gets the name of the method to be called when a subscription notification
   ''' is reeived.
   ''' </summary>
   Public ReadOnly Property EventRaiserMethodName() As String
      Get
         Return _eventRaiserMethodName
      End Get
   End Property

   ''' <summary>
   ''' Creates a <see cref="IResponseProcessor"/> to handle responses from this
   ''' command.
   ''' </summary>
   ''' <param name="client">The <see cref="TcpSoundbridgeClient"/> to handle
   ''' responses for.</param>
   ''' <param name="waitHandle">The <see cref="EventWaitHandle"/> to signal when
   ''' a response is received.</param>
   ''' <returns></returns>
   ''' <remarks></remarks>
   Public Overrides Function CreateResponseProcessor(ByVal client As TcpSoundbridgeClient, ByVal waitHandle As EventWaitHandle) As IResponseProcessor
      Dim d As Action(Of String)

      Try
         d = [Delegate].CreateDelegate(GetType(Action(Of String)), client, EventRaiserMethodName)
      Catch aex As ArgumentException
         Throw New MissingMethodException(String.Format("The method {0} could not be found on {1}", EventRaiserMethodName, client.GetType().FullName))
      End Try

      Return New SubscriptionResponseProcessor(client, Command, waitHandle, d)
   End Function
End Class
