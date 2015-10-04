Imports System.ComponentModel
Imports System.IO
Imports System.Threading
Imports System.Windows.Threading

Public Class AsyncFileSystemManager

    Public Delegate Sub DeleteDirectoryProgressChangedEventHandler(sender As Object, e As DeleteDirectoryProgressChangedEventArgs)

    Public Delegate Sub DeleteDirectoryCompletedEventHandler(sender As Object, e As DeleteDirectoryCompletedEventArgs)

    Public Event DeleteDirectoryProgressChanged As DeleteDirectoryProgressChangedEventHandler

    Public Event DeleteDirectoryCompleted As DeleteDirectoryCompletedEventHandler

    Private DeleteDirectoryProgressChangedCallback As SendOrPostCallback
    Private DeleteDirectoryCompletedCallback As SendOrPostCallback

    Private DeleteDirectoryAsyncOperation As AsyncOperation

    Private _IsBusy As Boolean = False
    Public ReadOnly Property IsBusy As Boolean
        Get
            Return _IsBusy
        End Get
    End Property

    Private _CancellationPending As Boolean = False
    Public ReadOnly Property CancellationPending As Boolean
        Get
            Return _CancellationPending
        End Get
    End Property

    Public Sub New()

        ' Set callbacks
        DeleteDirectoryProgressChangedCallback = New SendOrPostCallback(AddressOf SendDeleteDirectoryProgressChanged)
        DeleteDirectoryCompletedCallback = New SendOrPostCallback(AddressOf SendDeleteDirectoryCompleted)

    End Sub

    Public Sub DeleteDirectoryAsync(directory As String, onDirectoryNotEmpty As FileIO.DeleteDirectoryOption)

        ' Check if manager is already busy
        If Me._IsBusy Then

            ' Throw invalid operation exception if manager is already running
            Throw New InvalidOperationException("Async Filesystem Manager is busy!")

        End If

        ' Set manager as busy and cancellation is not pending
        Me._IsBusy = True
        Me._CancellationPending = False

        ' Get total size of directory to delete
        Dim totalSize As Long = GetDirectorySize(directory)

        ' Create global error variable
        Dim err As Exception = Nothing

        ' Create delete directory asynchronous operation
        DeleteDirectoryAsyncOperation = AsyncOperationManager.CreateOperation(Nothing)

        ' Create thread
        Dim DeleteDirectoryThread As New Thread(Sub()

                                                    Try

                                                        My.Computer.FileSystem.DeleteDirectory(directory, onDirectoryNotEmpty)

                                                    Catch ex As Exception

                                                        err = ex

                                                    End Try

                                                End Sub)

        Dim DeleteDirectoryProgressThread As New Thread(Sub()
                                                            Try
                                                                ' Check if delete is in progress and cancellation is not pending
                                                                While DeleteDirectoryThread.IsAlive And Not _CancellationPending

                                                                    ' Get current size of directory to delete
                                                                    Dim currentSize As Long = GetDirectorySize(directory)

                                                                    ' Get progress (subtract from 100 since progress is inverted)
                                                                    Dim progressPercentage As Single = 100 - currentSize / totalSize * 100

                                                                    ' Post progress changed
                                                                    DeleteDirectoryAsyncOperation.Post(DeleteDirectoryProgressChangedCallback, New DeleteDirectoryProgressChangedEventArgs(progressPercentage))

                                                                End While

                                                                ' Abort thread if cancellation was pending
                                                                If _CancellationPending Then DeleteDirectoryThread.Abort()

                                                            Catch ex As Exception

                                                                err = ex

                                                            End Try

                                                            ' Post operation completed
                                                            DeleteDirectoryAsyncOperation.PostOperationCompleted(DeleteDirectoryCompletedCallback, New DeleteDirectoryCompletedEventArgs(err, CancellationPending))

                                                        End Sub)

        ' Start delete directory thread
        DeleteDirectoryThread.Start()
        DeleteDirectoryProgressThread.Start()

    End Sub
    Private Sub SendDeleteDirectoryProgressChanged(e As DeleteDirectoryProgressChangedEventArgs)

        ' Raise progress changed event
        RaiseEvent DeleteDirectoryProgressChanged(Me, e)

    End Sub

    Private Sub SendDeleteDirectoryCompleted(e As DeleteDirectoryCompletedEventArgs)

        ' Set manager as not busy
        Me._IsBusy = False

        ' Raise completed event
        RaiseEvent DeleteDirectoryCompleted(Me, e)

    End Sub

    ''' <summary>
    ''' Gets the size of a directory based on the files it contains.
    ''' </summary>
    ''' <param name="directory">Path of the directory.</param>
    ''' <returns>The directory's size, in bytes.</returns>
    Private Function GetDirectorySize(directory As String) As Long

        ' Check if directory exists
        If IO.Directory.Exists(directory) Then

            ' Create bytes variable
            Dim bytes As Long = 0

            ' Enumerate through files in directory
            For Each file As String In IO.Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories)

                ' Wrap in try loop in case of exception
                Try

                    ' Check if file still exists
                    If IO.File.Exists(file) Then

                        ' Add file size to bytes variable
                        bytes += New FileInfo(file).Length

                    End If

                Catch ex As IOException
                    ' Catch IOException (usually file doesn't exist anymore)
                Catch ex As UnauthorizedAccessException
                    ' Catch UnauthorizedAccessException (file is trying to be read while being deleted)
                End Try

            Next

            ' Return byte count
            Return bytes
        Else

            ' Return 0 if directory doesn't exist
            Return 0

        End If

    End Function

End Class
