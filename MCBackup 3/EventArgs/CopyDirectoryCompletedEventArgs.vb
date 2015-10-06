Public Class CopyDirectoryCompletedEventArgs
    Inherits ComponentModel.AsyncCompletedEventArgs

    Public Sub New([error] As Exception, cancelled As Boolean)
        MyBase.New([error], cancelled, Nothing)
    End Sub

End Class
