Imports System.Threading

Public Class FileSystemOperations
    Public Class Directory
        Public Shared Function CopyAsync(SourceDirectory As String, DestinationDirectory As String, Overwrite As Boolean)
            Dim Thread As New Thread(Sub() Copy(SourceDirectory, DestinationDirectory, Overwrite))
            Thread.Start()
            Return Thread
        End Function

        Private Shared Sub Copy(SourceDirectory As String, DestinationDirectory As String, Overwrite As Boolean)
            My.Computer.FileSystem.CopyDirectory(SourceDirectory, DestinationDirectory, Overwrite)
        End Sub

        Public Shared Sub DeleteAsync(Directory As String)
            Dim Thread As New Thread(Sub() Delete(Directory))
            Thread.Start()
        End Sub

        Private Shared Sub Delete(Directory As String)
            My.Computer.FileSystem.DeleteDirectory(Directory, FileIO.DeleteDirectoryOption.DeleteAllContents)
        End Sub
    End Class
End Class
