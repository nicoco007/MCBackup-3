'   ╔═══════════════════════════════════════════════════════════════════════════╗
'   ║                      Copyright © 2013-2015 nicoco007                      ║
'   ║                                                                           ║
'   ║      Licensed under the Apache License, Version 2.0 (the "License");      ║
'   ║      you may not use this file except in compliance with the License.     ║
'   ║                  You may obtain a copy of the License at                  ║
'   ║                                                                           ║
'   ║                 http://www.apache.org/licenses/LICENSE-2.0                ║
'   ║                                                                           ║
'   ║    Unless required by applicable law or agreed to in writing, software    ║
'   ║     distributed under the License is distributed on an "AS IS" BASIS,     ║
'   ║  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. ║
'   ║     See the License for the specific language governing permissions and   ║
'   ║                      limitations under the License.                       ║
'   ╚═══════════════════════════════════════════════════════════════════════════╝

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
