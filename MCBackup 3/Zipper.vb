Imports Ionic.Zip
Imports System.ComponentModel
Imports System.IO
Imports System.Threading

Class Zipper
    ''' <summary>
    ''' Represents the method that will handle the CompressCompleted event of a Zipper class.
    ''' </summary>
    ''' <param name="sender">The source of the event.</param>
    ''' <param name="e">A CompressCompletedEventArgs that contains the event data.</param>
    Public Delegate Sub CompressCompletedEventHandler(sender As Object, e As CompressCompletedEventArgs)

    ''' <summary>
    ''' Represents the method that will handle the ExtractCompleted event of a Zipper class.
    ''' </summary>
    ''' <param name="sender">The source of the event.</param>
    ''' <param name="e">A ExtractCompletedEventArgs that contains the event data.</param>
    Public Delegate Sub ExtractCompletedEventHandler(sender As Object, e As ExtractCompletedEventArgs)

    Public Delegate Sub CompressProgressChangedEventHandler(sender As Object, e As CompressProgressChangedEventArgs)

    Public Delegate Sub ExtractProgressChangedEventHandler(sender As Object, e As ExtractProgressChangedEventArgs)

    Private compressAsyncOperation As AsyncOperation = Nothing
    Private extractAsyncOperation As AsyncOperation = Nothing

    Private compressThread As Thread
    Private extractThread As Thread

    ''' <summary>
    ''' Occurs when compression progress has been updated.
    ''' </summary>
    Public Event CompressProgressChanged As CompressProgressChangedEventHandler

    ''' <summary>
    ''' Occurs when compression has completed, has been canceled, or has raised an exception.
    ''' </summary>
    Public Event CompressCompleted As CompressCompletedEventHandler

    ''' <summary>
    ''' Occurs when extraction progress has been updated.
    ''' </summary>
    Public Event ExtractProgressChanged As ExtractProgressChangedEventHandler

    ''' <summary>
    ''' Occurs when extraction has completed, has been canceled, or has raised an exception.
    ''' </summary>
    Public Event ExtractCompleted As ExtractCompletedEventHandler

    Private ReadOnly compressCompletedCallback As SendOrPostCallback
    Private ReadOnly compressProgressCallback As SendOrPostCallback
    Private ReadOnly extractCompletedCallback As SendOrPostCallback
    Private ReadOnly extractProgressCallback As SendOrPostCallback


    Private _isBusy As Boolean = False

    Public ReadOnly Property IsBusy As Boolean
        Get
            Return _isBusy
        End Get
    End Property

    Private _CancellationPending As Boolean = False

    Public ReadOnly Property CancellationPending As Boolean
        Get
            Return _CancellationPending
        End Get
    End Property

    ''' <summary>
    ''' Creates a new Zipper class.
    ''' </summary>
    Public Sub New()
        compressCompletedCallback = New SendOrPostCallback(AddressOf PostCompressCompleted)
        compressProgressCallback = New SendOrPostCallback(AddressOf PostCompressProgress)

        extractCompletedCallback = New SendOrPostCallback(AddressOf PostExtractCompleted)
        extractProgressCallback = New SendOrPostCallback(AddressOf PostExtractProgress)
    End Sub

#Region "Compress Directory"
    Public Sub CompressDirectory(directory As String, fileName As String)
        Dim args As New CompressDirectoryEventArgs(directory, fileName)

        OnCompressStart(args)
    End Sub

    Public Sub CompressDirectoryAsync(directory As String, fileName As String)
        If _isBusy Then
            Throw New InvalidOperationException("Zipper is already running something!")
        End If

        _isBusy = True
        _CancellationPending = False

        compressAsyncOperation = AsyncOperationManager.CreateOperation(Nothing)

        Dim args As New CompressDirectoryEventArgs(directory, fileName)

        compressThread = New Thread(AddressOf CompressThreadStart)
        compressThread.Start(args)
    End Sub

    Private Sub CompressThreadStart(args As Object)
        Dim [error] As Exception = Nothing
        Dim cancelled As Boolean = False

        Try
            Dim doWorkArgs As CompressDirectoryEventArgs = DirectCast(args, CompressDirectoryEventArgs)
            OnCompressStart(doWorkArgs)

            If doWorkArgs.Cancel Then
                cancelled = True
            End If
        Catch ex As Exception
            [error] = ex
        End Try

        Dim e As New CompressCompletedEventArgs([error], cancelled)

        compressAsyncOperation.PostOperationCompleted(compressCompletedCallback, e)
    End Sub

    Public Sub CancelCompress()
        If Me._isBusy Then
            _CancellationPending = True
        End If
    End Sub

    Private Sub PostCompressCompleted(args As Object)
        _isBusy = False
        compressAsyncOperation = Nothing

        OnCompressCompleted(DirectCast(args, CompressCompletedEventArgs))
    End Sub

    Private Sub PostCompressProgress(args As Object)
        OnCompressProgressChanged(DirectCast(args, CompressProgressChangedEventArgs))
    End Sub

    Protected Overridable Sub OnCompressStart(e As CompressDirectoryEventArgs)
        Using zip As New ZipFile(e.FileName)
            zip.AddDirectory(e.Directory)

            If compressAsyncOperation IsNot Nothing Then
                Dim totalSize As Long = 0
                Dim compressedSize As Long = 0
                Dim prevBytesTransferred As Long = 0
                Dim prevEntry As ZipEntry = Nothing

                For Each file As FileInfo In New DirectoryInfo(e.Directory).GetFiles("*", SearchOption.AllDirectories)
                    totalSize += file.Length
                Next

                AddHandler zip.SaveProgress, Sub(s, args)
                                                 If prevEntry IsNot args.CurrentEntry Then
                                                     prevBytesTransferred = 0
                                                 End If

                                                 If CancellationPending Then
                                                     args.Cancel = True

                                                     Return
                                                 End If

                                                 If args.BytesTransferred <> 0 AndAlso args.CurrentEntry.CompressedSize = 0 Then
                                                     compressedSize += args.BytesTransferred - prevBytesTransferred

                                                     Dim progress As Integer = CInt((CDbl(compressedSize) / CDbl(totalSize)) * 100)

                                                     compressAsyncOperation.Post(compressProgressCallback, New CompressProgressChangedEventArgs(progress, compressedSize, totalSize))
                                                 End If

                                                 prevBytesTransferred = args.BytesTransferred
                                                 prevEntry = args.CurrentEntry

                                             End Sub
            End If

            zip.Save()
        End Using
    End Sub

    Protected Overridable Sub OnCompressProgressChanged(e As CompressProgressChangedEventArgs)
        RaiseEvent CompressProgressChanged(Me, e)
    End Sub

    Protected Overridable Sub OnCompressCompleted(e As CompressCompletedEventArgs)
        RaiseEvent CompressCompleted(Me, e)
    End Sub
#End Region

#Region "Extract Directory"
    Public Sub Extract(fileName As String, path As String)
        Extract(fileName, path, ExtractExistingFileAction.[Throw])
    End Sub

    Public Sub Extract(fileName As String, path As String, existingFileAction As ExtractExistingFileAction)
        Dim args As New ExtractEventArgs(fileName, path, existingFileAction)

        OnExtractStart(args)
    End Sub

    Public Sub ExtractAsync(fileName As String, path As String)
        ExtractAsync(fileName, path, ExtractExistingFileAction.[Throw])
    End Sub

    Public Sub ExtractAsync(fileName As String, path As String, existingFileAction As ExtractExistingFileAction)
        If _isBusy Then
            Throw New InvalidOperationException("Zipper is already running something!")
        End If

        _isBusy = True
        _CancellationPending = False

        extractAsyncOperation = AsyncOperationManager.CreateOperation(Nothing)

        Dim args As New ExtractEventArgs(fileName, path, existingFileAction)

        extractThread = New Thread(AddressOf ExtractThreadStart)
        extractThread.Start(args)
    End Sub

    Private Sub ExtractThreadStart(args As Object)
        Dim [error] As Exception = Nothing
        Dim cancelled As Boolean = False

        Try
            Dim doWorkArgs As ExtractEventArgs = DirectCast(args, ExtractEventArgs)
            OnExtractStart(doWorkArgs)

            If doWorkArgs.Cancel Then
                cancelled = True
            End If
        Catch ex As Exception
            [error] = ex
        End Try

        Dim e As New ExtractCompletedEventArgs([error], cancelled)

        extractAsyncOperation.PostOperationCompleted(extractCompletedCallback, e)
    End Sub

    Public Sub CancelExtract()
        If extractThread IsNot Nothing AndAlso extractThread.IsAlive Then
            _CancellationPending = True

            extractThread.Abort()

            Dim e As New ExtractCompletedEventArgs(Nothing, True)

            extractAsyncOperation.PostOperationCompleted(extractCompletedCallback, e)
        End If
    End Sub

    Private Sub PostExtractCompleted(args As Object)
        _isBusy = False
        extractAsyncOperation = Nothing

        OnExtractCompleted(DirectCast(args, ExtractCompletedEventArgs))
    End Sub

    Private Sub PostExtractProgress(args As Object)
        OnExtractProgressChanged(DirectCast(args, ExtractProgressChangedEventArgs))
    End Sub

    Protected Overridable Sub OnExtractStart(e As ExtractEventArgs)
        Using zip As New ZipFile(e.FileName)
            If extractAsyncOperation IsNot Nothing Then
                Dim totalSize As Long = 0
                Dim extractedSize As Long = 0
                Dim prevBytesTransferred As Long = 0
                Dim prevEntry As ZipEntry = Nothing

                For Each entry As ZipEntry In zip.Entries
                    totalSize += entry.UncompressedSize
                Next

                AddHandler zip.ExtractProgress, Sub(s, args)
                                                    If prevEntry IsNot args.CurrentEntry Then
                                                        prevBytesTransferred = 0
                                                    End If

                                                    If args.BytesTransferred <> 0 Then
                                                        extractedSize += args.BytesTransferred - prevBytesTransferred

                                                        Dim progress As Integer = CInt((CDbl(extractedSize) / CDbl(totalSize)) * 100)

                                                        extractAsyncOperation.Post(extractProgressCallback, New ExtractProgressChangedEventArgs(progress, extractedSize, totalSize))
                                                    End If

                                                    prevBytesTransferred = args.BytesTransferred
                                                    prevEntry = args.CurrentEntry

                                                End Sub
            End If

            zip.ExtractAll(e.Path, ExtractExistingFileAction.OverwriteSilently)
        End Using
    End Sub

    Protected Overridable Sub OnExtractProgressChanged(e As ExtractProgressChangedEventArgs)
        RaiseEvent ExtractProgressChanged(Me, e)
    End Sub

    Protected Overridable Sub OnExtractCompleted(e As ExtractCompletedEventArgs)
        RaiseEvent ExtractCompleted(Me, e)
    End Sub
#End Region
End Class