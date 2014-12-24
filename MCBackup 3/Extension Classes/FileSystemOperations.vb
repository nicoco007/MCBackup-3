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
            Try
                My.Computer.FileSystem.CopyDirectory(SourceDirectory, DestinationDirectory, Overwrite)
            Catch ex As Exception
                Log.Print(ex.Message)
            End Try
        End Sub

        'Public Shared Function DeleteAsync(Directory As String)
        '    Dim Thread As New Thread(Sub() Delete(Directory))
        '    Thread.Start()
        '    Return Thread
        'End Function

        'Public Shared Sub Delete(Directory As String)
        '    For Each File As FileInfo In New DirectoryInfo(Directory).GetFiles
        '        File.Delete()
        '    Next

        '    For Each Folder As DirectoryInfo In New DirectoryInfo(Directory).GetDirectories
        '        Delete(Folder.FullName)
        '    Next

        '    IO.Directory.Delete(Directory)
        'End Sub

        'Public Shared Function DeleteFolderContentsAsync(Directory As String)
        '    Dim Thread As New Thread(Sub() DeleteFolderContents(Directory))
        '    Thread.Start()
        '    Return Thread
        'End Function

        'Public Shared Sub DeleteFolderContents(Directory As String)
        '    For Each File As FileInfo In New DirectoryInfo(Directory).GetFiles
        '        IO.File.Delete(File.FullName)
        '    Next

        '    For Each Folder As DirectoryInfo In New DirectoryInfo(Directory).GetDirectories
        '        Delete(Folder.FullName)
        '    Next
        'End Sub

        Public Shared Function DeleteAsync(Directory As String)
            Dim Thread As New Thread(Sub() Delete(Directory))
            Thread.Start()
            Return Thread
        End Function

        Public Shared Sub Delete(Directory As String)
            For Each File As String In IO.Directory.GetFiles(Directory)
                IO.File.Delete(File)
            Next

            For Each Folder As String In IO.Directory.GetDirectories(Directory)
                Delete(Folder)
            Next

            IO.Directory.Delete(Directory)
        End Sub
    End Class
End Class
