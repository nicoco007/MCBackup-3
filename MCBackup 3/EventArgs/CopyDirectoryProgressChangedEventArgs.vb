Public Class CopyDirectoryProgressChangedEventArgs

    Private _ProgressPercentage As Single
    Public Overloads ReadOnly Property ProgressPercentage As Single
        Get
            Return _ProgressPercentage
        End Get
    End Property

    Private _BytesCopied As Long
    Public ReadOnly Property BytesCopied As Long
        Get
            Return _BytesCopied
        End Get
    End Property

    Private _TotalBytes As Long
    Public ReadOnly Property TotalBytes As Long
        Get
            Return _TotalBytes
        End Get
    End Property

    Sub New(progressPercentage As Single, bytesCopied As Long, totalBytes As Long)
        Me._ProgressPercentage = progressPercentage
        Me._BytesCopied = bytesCopied
        Me._TotalBytes = totalBytes
    End Sub

End Class
