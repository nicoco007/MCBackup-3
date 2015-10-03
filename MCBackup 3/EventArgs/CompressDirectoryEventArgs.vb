Imports System.ComponentModel

Public Class CompressDirectoryEventArgs
    Inherits CancelEventArgs
    Public Property Directory() As String
        Get
            Return m_Directory
        End Get
        Set
            m_Directory = Value
        End Set
    End Property
    Private m_Directory As String
    Public Property FileName() As String
        Get
            Return m_FileName
        End Get
        Set
            m_FileName = Value
        End Set
    End Property
    Private m_FileName As String

    Public Sub New(directory As String, fileName As String)
        Me.Directory = directory
        Me.FileName = fileName
    End Sub
End Class
