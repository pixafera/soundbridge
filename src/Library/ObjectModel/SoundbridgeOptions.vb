Imports System.Collections.ObjectModel

Public Class SoundbridgeOptions
    Inherits Pixa.Soundbridge.Library.SoundbridgeObject
    Implements System.Collections.Generic.IDictionary(Of String, String)

    Private _optionKeys As ReadOnlyCollection(Of String)

    Friend Sub New(ByVal obj As SoundbridgeObject)
        MyBase.New(obj)

        Dim l As New List(Of String)
        l.Add("bootmode")
        l.Add("standbyMode")
        l.Add("outputMultichannel")
        l.Add("reventToNowPlaying")
        l.Add("scrollLongInfo")
        l.Add("displayComposer")
        l.Add("skipUnchecked")
        l.Add("wmaThreshold")

        _optionKeys = New ReadOnlyCollection(Of String)(l)
    End Sub

    Friend Sub New(ByVal client As ISoundbridgeClient)
        MyBase.New(client)
    End Sub

#Region " Unsupported "

    Private Sub Add(ByVal item As System.Collections.Generic.KeyValuePair(Of String, String)) Implements System.Collections.Generic.ICollection(Of System.Collections.Generic.KeyValuePair(Of String, String)).Add
        Throw New NotSupportedException("Cannot change elements in this dictionary")
    End Sub

    Private Sub Clear() Implements System.Collections.Generic.ICollection(Of System.Collections.Generic.KeyValuePair(Of String, String)).Clear
        Throw New NotSupportedException("Cannot change elements in this dictionary")
    End Sub

    Private Function Remove(ByVal item As System.Collections.Generic.KeyValuePair(Of String, String)) As Boolean Implements System.Collections.Generic.ICollection(Of System.Collections.Generic.KeyValuePair(Of String, String)).Remove
        Throw New NotSupportedException("Cannot change elements in this dictionary")
    End Function

    Private Sub Add1(ByVal key As String, ByVal value As String) Implements System.Collections.Generic.IDictionary(Of String, String).Add
        Throw New NotSupportedException("Cannot change elements in this dictionary")
    End Sub

    Private Function Remove1(ByVal key As String) As Boolean Implements System.Collections.Generic.IDictionary(Of String, String).Remove
        Throw New NotSupportedException("Cannot change elements in this dictionary")
    End Function

#End Region

#Region " Dictionary "
    Public Function Contains(ByVal item As System.Collections.Generic.KeyValuePair(Of String, String)) As Boolean Implements System.Collections.Generic.ICollection(Of System.Collections.Generic.KeyValuePair(Of String, String)).Contains
        Return ContainsKey(item.Key) AndAlso Me.Item(item.Key) = item.Value
    End Function

    Public Function ContainsKey(ByVal key As String) As Boolean Implements System.Collections.Generic.IDictionary(Of String, String).ContainsKey
        Return _optionKeys.Contains(key)
    End Function

    Public Sub CopyTo(ByVal array() As System.Collections.Generic.KeyValuePair(Of String, String), ByVal arrayIndex As Integer) Implements System.Collections.Generic.ICollection(Of System.Collections.Generic.KeyValuePair(Of String, String)).CopyTo
        For i As Integer = 0 To _optionKeys.Count - 1
            array(arrayIndex + i) = New KeyValuePair(Of String, String)(_optionKeys(i), Item(_optionKeys(i)))
        Next
    End Sub

    Public ReadOnly Property Count() As Integer Implements System.Collections.Generic.ICollection(Of System.Collections.Generic.KeyValuePair(Of String, String)).Count
        Get
            Return _optionKeys.Count
        End Get
    End Property

    Public ReadOnly Property IsReadOnly() As Boolean Implements System.Collections.Generic.ICollection(Of System.Collections.Generic.KeyValuePair(Of String, String)).IsReadOnly
        Get
            Return True
        End Get
    End Property

    Default Public Property Item(ByVal key As String) As String Implements System.Collections.Generic.IDictionary(Of String, String).Item
        Get
            Dim r As String = Client.GetOption(key)

            If r = "ParameterError" Or r = "GenericError" Or r = "ErrorUnsupported" Then ExceptionHelper.ThrowCommandReturnError("GetOption", r)

            Return r
        End Get
        Set(ByVal value As String)
            Dim r As String = Client.SetOption(key, value)

            If r <> "OK" Then ExceptionHelper.ThrowCommandReturnError("SetOption", r)
        End Set
    End Property

    Public ReadOnly Property Keys() As System.Collections.Generic.ICollection(Of String) Implements System.Collections.Generic.IDictionary(Of String, String).Keys
        Get
            Return _optionKeys
        End Get
    End Property

    Public Function TryGetValue(ByVal key As String, ByRef value As String) As Boolean Implements System.Collections.Generic.IDictionary(Of String, String).TryGetValue
        Try
            value = Item(key)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public ReadOnly Property Values() As System.Collections.Generic.ICollection(Of String) Implements System.Collections.Generic.IDictionary(Of String, String).Values
        Get
            Dim l As New List(Of String)

            For Each s As String In _optionKeys
                l.Add(Item(s))
            Next

            Return New ReadOnlyCollection(Of String)(l)
        End Get
    End Property
#End Region

#Region " Enumerator "
    Public Function GetEnumerator() As System.Collections.Generic.IEnumerator(Of System.Collections.Generic.KeyValuePair(Of String, String)) Implements System.Collections.Generic.IEnumerable(Of System.Collections.Generic.KeyValuePair(Of String, String)).GetEnumerator
        Return New SoundbridgeOptionsEnumerator(Me)
    End Function

    Private Function GetNonGenericEnumerator() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
        Return GetEnumerator()
    End Function

    Private Class SoundbridgeOptionsEnumerator
        Implements System.Collections.Generic.IEnumerator(Of KeyValuePair(Of String, String))

        Private _options As SoundbridgeOptions
        Private _index As Integer = 0
        Private _current As KeyValuePair(Of String, String)

        Public Sub New(ByVal options As SoundbridgeOptions)
            _options = options
        End Sub

        Public ReadOnly Property Current() As System.Collections.Generic.KeyValuePair(Of String, String) Implements System.Collections.Generic.IEnumerator(Of System.Collections.Generic.KeyValuePair(Of String, String)).Current
            Get
                Return _current
            End Get
        End Property

        Private ReadOnly Property Current1() As Object Implements System.Collections.IEnumerator.Current
            Get
                Return Current
            End Get
        End Property

        Public Function MoveNext() As Boolean Implements System.Collections.IEnumerator.MoveNext
            _index += 1

            If _index >= _options.Count Then Return False

            UpdateCurrent()

            Return True
        End Function

        Public Sub Reset() Implements System.Collections.IEnumerator.Reset
            _index = 0
        End Sub

        Private Sub UpdateCurrent()
            Dim key As String = _options._optionKeys(_index)
            _current = New KeyValuePair(Of String, String)(key, _options(key))
        End Sub

#Region " IDisposable Support "
        Private disposedValue As Boolean = False        ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: free other state (managed objects).
                End If

                ' TODO: free your own state (unmanaged objects).
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

    End Class
#End Region

#Region " Option Properties "
    Public Property BootMode() As BootMode
        Get
            Select Case Item("bootMode")
                Case "lastState"
                    Return Library.BootMode.LastSource

                Case "standby"
                    Return Library.BootMode.Standby

                Case "lastSource"
                    Return Library.BootMode.LastSource

                Case "serverList"
                    Return Library.BootMode.ServerList

            End Select
        End Get
        Set(ByVal value As BootMode)
            Dim sValue As String

            Select Case value
                Case Library.BootMode.LastSource
                    sValue = "lastSource"

                Case Library.BootMode.LastState
                    sValue = "lastState"

                Case Library.BootMode.ServerList
                    sValue = "serverList"

                Case Library.BootMode.Standby
                    sValue = "standby"

            End Select

            Item("bootMode") = value
        End Set
    End Property

    Public Property ShowClockInStandby() As Boolean
        Get
            Return Item("standbyMode") = "clock"
        End Get
        Set(ByVal value As Boolean)
            Item("standbyMode") = If(value, "clock", "screenOff")
        End Set
    End Property

    Public Property OutputMultichannel() As Boolean
        Get
            Return Item("outputMultichannel") = "1"
        End Get
        Set(ByVal value As Boolean)
            Item("outputMultichannel") = If(value, 1, 0)
        End Set
    End Property

    Public Property RevertToNowPlaying() As Boolean
        Get
            Return Item("revertToNowPlaying") = "1"
        End Get
        Set(ByVal value As Boolean)
            Item("revertToNowPlaying") = If(value, 1, 0)
        End Set
    End Property

    Public Property ScrollLongInfo() As Boolean
        Get
            Return Item("scrollLongInfo") = "1"
        End Get
        Set(ByVal value As Boolean)
            Item("scrollLongInfo") = If(value, 1, 0)
        End Set
    End Property

    Public Property DisplayComposer() As Boolean
        Get
            Return Item("displayComposer") = "1"
        End Get
        Set(ByVal value As Boolean)
            Item("displayComposer") = If(value, 1, 0)
        End Set
    End Property

    Public Property SkipUnchecked() As Boolean
        Get
            Return Item("skipUnchecked") = "1"
        End Get
        Set(ByVal value As Boolean)
            Item("skipUnchecked") = If(value, 1, 0)
        End Set
    End Property

    Public Property WmaThreshold() As Integer
        Get
            Return Integer.Parse(Item("wmaThreshold"))
        End Get
        Set(ByVal value As Integer)
            Item("wmaThreshold") = value
        End Set
    End Property
#End Region

End Class

Public Enum BootMode
    LastState
    Standby
    LastSource
    ServerList
End Enum