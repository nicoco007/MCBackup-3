Imports System.ComponentModel

Public Class BackupEventArgs
    Inherits CancelEventArgs

    Private _Name As String
    Public Property Name As String
        Get
            Return _Name
        End Get
        Set(value As String)
            _Name = value
        End Set
    End Property

    Private _Path As String
    Public Property Path As String
        Get
            Return _Path
        End Get
        Set(value As String)
            _Path = value
        End Set
    End Property

    Private _Description As String
    Public Property Description As String
        Get
            Return _Description
        End Get
        Set(value As String)
            _Description = value
        End Set
    End Property

    Private _Type As BackupManager.BackupTypes
    Public Property Type As BackupManager.BackupTypes
        Get
            Return _Type
        End Get
        Set(value As BackupManager.BackupTypes)
            _Type = value
        End Set
    End Property

    Private _Group As String
    Public Property Group As String
        Get
            Return _Group
        End Get
        Set(value As String)
            _Group = value
        End Set
    End Property

    Private _Launcher As Game.Launcher
    Public Property Launcher As Game.Launcher
        Get
            Return _Launcher
        End Get
        Set(value As Game.Launcher)
            _Launcher = value
        End Set
    End Property

    Private _Modpack As String
    Public Property Modpack As String
        Get
            Return _Modpack
        End Get
        Set(value As String)
            _Modpack = value
        End Set
    End Property

    Public Sub New(Name As String, Path As String, Type As String, Description As String, Group As String, Launcher As Game.Launcher, Modpack As String)
        Me.Name = Name
        Me.Path = Path
        Me.Type = Type
        Me.Description = Description
        Me.Group = Group
        Me.Launcher = Launcher
        Me.Modpack = Modpack
    End Sub
End Class
