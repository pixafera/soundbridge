Public MustInherit Class SoundbridgeObject
   Implements System.IDisposable

   Private _client As ISoundbridgeClient

   Protected Sub New(ByVal obj As SoundbridgeObject)
      MyClass.New(obj.Client)
   End Sub

   Protected Sub New(ByVal client As ISoundbridgeClient)
      _client = client
   End Sub

   Protected Friend ReadOnly Property Client() As ISoundbridgeClient
      Get
         Return _client
      End Get
   End Property

#Region " IDisposable Support "
   Private _disposed As Boolean = False      ' To detect redundant calls

   Public ReadOnly Property Disposed() As Boolean
      Get
         Return _disposed
      End Get
   End Property

   Public Overridable ReadOnly Property ShouldCacheDispose() As Boolean
      Get
         Return True
      End Get
   End Property

   ' IDisposable
   Protected Overridable Sub Dispose(ByVal disposing As Boolean)
      If Not Me._disposed Then
         If disposing Then
            _client = Nothing
         End If

         ' TODO: free your own state (unmanaged objects).
         ' TODO: set large fields to null.
      End If
      Me._disposed = True
   End Sub

   ' This code added by Visual Basic to correctly implement the disposable pattern.
   Public Sub Dispose() Implements IDisposable.Dispose
      ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
      Dispose(True)
      GC.SuppressFinalize(Me)
   End Sub
#End Region

End Class
