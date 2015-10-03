Public Class DeleteDirectoryProgressChangedEventArgs

    Private _ProgressPercentage As Single
    Public ReadOnly Property ProgressPercentage As Single
        Get
            Return _ProgressPercentage
        End Get
    End Property

    Sub New(progressPercentage As Single)

        Me._ProgressPercentage = progressPercentage

    End Sub

End Class
