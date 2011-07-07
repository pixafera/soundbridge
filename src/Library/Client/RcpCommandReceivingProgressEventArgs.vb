Public Class RcpCommandReceivingProgressEventArgs
    Inherits Pixa.Soundbridge.Library.RcpCommandProgressEventArgs

    Private _progress As Integer
    Private _total As Integer

    Public Sub New(ByVal command As String)
        MyClass.New(command, -1)
    End Sub

    Public Sub New(ByVal command As String, ByVal progress As Integer)
        MyClass.New(command, progress, -1)
    End Sub

    Public Sub New(ByVal command As String, ByVal progress As Integer, ByVal total As Integer)
        MyBase.New(command)
        _progress = progress
        _total = total
    End Sub

    ''' <summary>
    ''' Gets the progress of the transaction
    ''' </summary>
    ''' <value></value>
    ''' <returns>The progress of the transaction, or -1 if this data was not sent.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Progress() As Integer
        Get
            Return _progress
        End Get
    End Property

    ''' <summary>
    ''' Gets the total size of the transaction being executed.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The total size of the transaction being executed, or -1 if this data was not sent.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Total() As Integer
        Get
            Return _total
        End Get
    End Property
End Class