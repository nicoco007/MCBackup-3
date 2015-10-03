Public Class ExtractProgressChangedEventArgs

    Private _ProgressPercentage As Single
    Public Overloads ReadOnly Property ProgressPercentage As Single
        Get
            Return _ProgressPercentage
        End Get
    End Property

    Private _BytesExtracted As Long
    Public ReadOnly Property BytesExtracted As Long
        Get
            Return _BytesExtracted
        End Get
    End Property

    Private _TotalBytes As Long
    Public ReadOnly Property TotalBytes As Long
        Get
            Return _TotalBytes
        End Get
    End Property

    Sub New(progress As Single)
        Me._ProgressPercentage = progress
    End Sub

    Sub New(progress As Single, bytesExtracted As Long, totalBytes As Long)
        Me._ProgressPercentage = progress
        Me._BytesExtracted = bytesExtracted
        Me._TotalBytes = totalBytes
    End Sub
End Class
