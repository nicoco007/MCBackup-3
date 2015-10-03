Imports System.ComponentModel

Public Class DeleteDirectoryEventArgs
    Inherits CancelEventArgs

    Private _Directory As String
    Public Property Directory As String
        Get
            Return _Directory
        End Get
        Set(value As String)
            _Directory = value
        End Set
    End Property

    Private _OnDirectoryNotEmpty As FileIO.DeleteDirectoryOption
    Public Property OnDirectoryNotEmpty As FileIO.DeleteDirectoryOption
        Get
            Return _OnDirectoryNotEmpty
        End Get
        Set(value As FileIO.DeleteDirectoryOption)
            _OnDirectoryNotEmpty = value
        End Set
    End Property

    Public Sub New(directory As String, onDirectoryNotEmpty As FileIO.DeleteDirectoryOption)
        Me._Directory = directory
        Me._OnDirectoryNotEmpty = onDirectoryNotEmpty
    End Sub

End Class
