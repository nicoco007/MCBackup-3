Imports MCBackup.BackupManager

Public Class BackupProgressChangedEventArgs
    Private _Status As BackupStatus
    Public ReadOnly Property Status As BackupStatus
        Get
            Return _Status
        End Get
    End Property

    Private _EstimatedTimeRemaining As TimeSpan
    Public ReadOnly Property EstimatedTimeRemaining As TimeSpan
        Get
            Return _EstimatedTimeRemaining
        End Get
    End Property

    Private _ProgressPercentage As Single
    Public ReadOnly Property ProgressPercentage As Single
        Get
            Return _ProgressPercentage
        End Get
    End Property

    Private _TransferRate As Single
    Public ReadOnly Property TransferRate As Single
        Get
            Return _TransferRate
        End Get
    End Property

    Sub New(status As BackupStatus)
        Me._Status = status
        Me._ProgressPercentage = Nothing
        Me._EstimatedTimeRemaining = Nothing
        Me._TransferRate = Nothing
    End Sub

    Sub New(status As BackupStatus, progressPercentage As Single)
        Me._Status = status
        Me._ProgressPercentage = progressPercentage
        Me._EstimatedTimeRemaining = Nothing
        Me._TransferRate = Nothing
    End Sub

    Sub New(status As BackupStatus, progressPercentage As Single, estimatedTimeRemaining As TimeSpan, transferRate As Single)
        Me._Status = status
        Me._ProgressPercentage = progressPercentage
        Me._EstimatedTimeRemaining = estimatedTimeRemaining
        Me._TransferRate = transferRate
    End Sub
End Class
