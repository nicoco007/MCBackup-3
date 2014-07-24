Imports System.Threading
Imports System.IO

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

        Public Shared Function DeleteAsync(Directory As String)
            Dim Thread As New Thread(Sub() Delete(Directory))
            Thread.Start()
            Return Thread
        End Function

        Public Shared Sub Delete(Directory As String)
            My.Computer.FileSystem.DeleteDirectory(Directory, FileIO.DeleteDirectoryOption.DeleteAllContents)
        End Sub

        Public Shared Function DeleteFolderContentsAsync(Directory As String)
            Dim Thread As New Thread(Sub() DeleteFolderContents(Directory))
            Thread.Start()
            Return Thread
        End Function

        Public Shared Sub DeleteFolderContents(Directory As String)
            For Each Folder As DirectoryInfo In New DirectoryInfo(Directory).GetDirectories
                Folder.Delete(True)
            Next

            For Each File As FileInfo In New DirectoryInfo(Directory).GetFiles
                File.Delete()
            Next
        End Sub
    End Class
End Class
