Imports MCBackup.BackupManager

Public Class RestoreEventArgs
    Inherits ComponentModel.CancelEventArgs

    Private _BackupName As String
    Public Property BackupName As String
        Get
            Return _BackupName
        End Get
        Set(value As String)
            _BackupName = value
        End Set
    End Property

    Private _RestoreLocation As String
    Public Property RestoreLocation As String
        Get
            Return _RestoreLocation
        End Get
        Set(value As String)
            _RestoreLocation = value
        End Set
    End Property

    Private _BackupType As BackupTypes
    Public Property BackupType As BackupTypes
        Get
            Return _BackupType
        End Get
        Set(value As BackupTypes)
            _BackupType = value
        End Set
    End Property

    Public Sub New(backupName As String, restoreLocation As String, backupType As BackupTypes)
        Me._BackupName = backupName
        Me._RestoreLocation = restoreLocation
        Me._BackupType = backupType
    End Sub

End Class
