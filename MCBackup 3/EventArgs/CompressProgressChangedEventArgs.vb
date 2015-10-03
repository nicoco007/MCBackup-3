Imports System.ComponentModel

Public Class CompressProgressChangedEventArgs

    Private _ProgressPercentage As Single
    Public Overloads ReadOnly Property ProgressPercentage As Single
        Get
            Return _ProgressPercentage
        End Get
    End Property

    Private _BytesCompressed As Long
    Public ReadOnly Property BytesCompressed As Long
        Get
            Return _BytesCompressed
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

    Sub New(progress As Single, bytesCompressed As Long, totalBytes As Long)
        Me._ProgressPercentage = progress
        Me._BytesCompressed = bytesCompressed
        Me._TotalBytes = totalBytes
    End Sub
End Class
