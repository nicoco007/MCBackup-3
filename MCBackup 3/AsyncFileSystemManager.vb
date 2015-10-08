Imports System.ComponentModel
Imports System.IO
Imports System.Threading

Public Class AsyncFileSystemManager

    Public Delegate Sub DeleteDirectoryProgressChangedEventHandler(sender As Object, e As DeleteDirectoryProgressChangedEventArgs)
    Public Delegate Sub DeleteDirectoryCompletedEventHandler(sender As Object, e As DeleteDirectoryCompletedEventArgs)
    Public Delegate Sub CopyDirectoryProgressChangedEventHandler(sender As Object, e As CopyDirectoryProgressChangedEventArgs)
    Public Delegate Sub CopyDirectoryCompletedEventHandler(sender As Object, e As CopyDirectoryCompletedEventArgs)

    Public Event DeleteDirectoryProgressChanged As DeleteDirectoryProgressChangedEventHandler
    Public Event DeleteDirectoryCompleted As DeleteDirectoryCompletedEventHandler
    Public Event CopyDirectoryProgressChanged As CopyDirectoryProgressChangedEventHandler
    Public Event CopyDirectoryCompleted As CopyDirectoryCompletedEventHandler

    Private DeleteDirectoryProgressChangedCallback As SendOrPostCallback
    Private DeleteDirectoryCompletedCallback As SendOrPostCallback
    Private CopyDirectoryProgressChangedCallback As SendOrPostCallback
    Private CopyDirectoryCompletedCallback As SendOrPostCallback

    Private DeleteDirectoryAsyncOperation As AsyncOperation
    Private CopyDirectoryAsyncOperation As AsyncOperation

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

    Public Sub Cancel()

        Me._CancellationPending = True

    End Sub

    Public Sub New()

        ' Set callbacks
        DeleteDirectoryProgressChangedCallback = New SendOrPostCallback(AddressOf SendDeleteDirectoryProgressChanged)
        DeleteDirectoryCompletedCallback = New SendOrPostCallback(AddressOf SendDeleteDirectoryCompleted)
        CopyDirectoryProgressChangedCallback = New SendOrPostCallback(AddressOf SendCopyDirectoryProgressChanged)
        CopyDirectoryCompletedCallback = New SendOrPostCallback(AddressOf SendCopyDirectoryCompleted)

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

                                                    My.Computer.FileSystem.DeleteDirectory(directory, FileIO.UIOption.OnlyErrorDialogs, FileIO.RecycleOption.DeletePermanently, FileIO.UICancelOption.DoNothing)

                                                End Sub)

        Dim DeleteDirectoryProgressThread As New Thread(Sub()
                                                            Try
                                                                ' Check if delete is in progress and cancellation is not pending
                                                                While DeleteDirectoryThread.IsAlive And Not _CancellationPending

                                                                    ' Get current size of directory to delete
                                                                    Dim currentSize As Long = GetDirectorySize(directory)

                                                                    Dim progressPercentage As Single = 0

                                                                    ' Make sure we are not dividing by 0
                                                                    If totalSize <> 0 And currentSize <> 0 Then

                                                                        ' Get progress (subtract from 100 since progress is inverted)
                                                                        progressPercentage = 100 - currentSize / totalSize * 100

                                                                        ' Post progress changed
                                                                        DeleteDirectoryAsyncOperation.Post(DeleteDirectoryProgressChangedCallback, New DeleteDirectoryProgressChangedEventArgs(progressPercentage))

                                                                    Else

                                                                        Log.Warn("Division by zero prevented during delete directory operation - may be caused by empty directory")

                                                                    End If
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
        Me._CancellationPending = False

        ' Raise completed event
        RaiseEvent DeleteDirectoryCompleted(Me, e)

    End Sub

    Public Sub CopyDirectoryAsync(sourceDirectory As String, destinationDirectory As String, Optional overwrite As Boolean = False)

        ' Check if manager is already busy
        If Me._IsBusy Then

            ' Throw invalid operation exception if manager is already running
            Throw New InvalidOperationException("Async Filesystem Manager is busy!")

        End If

        Me._IsBusy = True
        Me._CancellationPending = False

        Dim totalBytes As Long = GetDirectorySize(sourceDirectory)

        Dim err As Exception = Nothing

        CopyDirectoryAsyncOperation = AsyncOperationManager.CreateOperation(Nothing)

        Dim CopyDirectoryThread As New Thread(Sub()

                                                  My.Computer.FileSystem.CopyDirectory(sourceDirectory, destinationDirectory, overwrite)

                                              End Sub)

        Dim CopyDirectoryProgressThread As New Thread(Sub()

                                                          Try

                                                              ' Check if delete is in progress and cancellation is not pending
                                                              While CopyDirectoryThread.IsAlive And Not _CancellationPending

                                                                  ' Get current size of directory to delete
                                                                  Dim bytesCopied As Long = GetDirectorySize(destinationDirectory)

                                                                  Dim progressPercentage As Single = 0

                                                                  ' Make sure we are not dividing by 0
                                                                  If totalBytes <> 0 And bytesCopied <> 0 Then

                                                                      ' Get progress (subtract from 100 since progress is inverted)
                                                                      progressPercentage = 100 - bytesCopied / totalBytes * 100

                                                                      ' Post progress changed
                                                                      CopyDirectoryAsyncOperation.Post(CopyDirectoryProgressChangedCallback, New CopyDirectoryProgressChangedEventArgs(progressPercentage, bytesCopied, totalBytes))

                                                                  Else

                                                                      Log.Warn("Division by zero prevented during delete directory operation - may be caused by empty directory")

                                                                  End If
                                                              End While

                                                              ' Abort thread if cancellation was pending
                                                              If _CancellationPending Then CopyDirectoryThread.Abort()

                                                          Catch ex As Exception

                                                              err = ex

                                                          End Try

                                                          CopyDirectoryAsyncOperation.PostOperationCompleted(CopyDirectoryCompletedCallback, New CopyDirectoryCompletedEventArgs(err, CancellationPending))

                                                      End Sub)

        CopyDirectoryThread.Start()
        CopyDirectoryProgressThread.Start()

    End Sub

    Private Sub SendCopyDirectoryProgressChanged(e As CopyDirectoryProgressChangedEventArgs)

        RaiseEvent CopyDirectoryProgressChanged(Me, e)

    End Sub

    Private Sub SendCopyDirectoryCompleted(e As CopyDirectoryCompletedEventArgs)

        Me._IsBusy = False
        Me._CancellationPending = False

        RaiseEvent CopyDirectoryCompleted(Me, e)

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

            Try

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

            Catch ex As IOException
                Log.Warn("File enumeration failed: " + ex.Message)
            Catch ex As UnauthorizedAccessException
                Log.Warn("File enumeration failed: " + ex.Message)
            End Try

            ' Return byte count
            Return bytes

        Else

            ' Return 0 if directory doesn't exist
            Return 0

        End If

    End Function

End Class
