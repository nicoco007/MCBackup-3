﻿Public Class RestoreInfo
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

    Private _BackupType As BackupType
    Public Property BackupType As BackupType
        Get
            Return _BackupType
        End Get
        Set(value As BackupType)
            _BackupType = value
        End Set
    End Property
End Class