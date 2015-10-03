Imports System.ComponentModel
Imports Ionic.Zip

Public Class ExtractEventArgs
    Inherits CancelEventArgs
    Public Property ExistingFileAction() As ExtractExistingFileAction
        Get
            Return m_ExistingFileAction
        End Get
        Set
            m_ExistingFileAction = Value
        End Set
    End Property
    Private m_ExistingFileAction As ExtractExistingFileAction

    Public Property FileName() As String
        Get
            Return m_FileName
        End Get
        Set
            m_FileName = Value
        End Set
    End Property
    Private m_FileName As String

    Public Property Path() As String
        Get
            Return m_Path
        End Get
        Set
            m_Path = Value
        End Set
    End Property
    Private m_Path As String

    Public Sub New(fileName As String, path As String, existingFileAction As ExtractExistingFileAction)
        Me.ExistingFileAction = existingFileAction
        Me.FileName = fileName
        Me.Path = path
    End Sub
End Class
