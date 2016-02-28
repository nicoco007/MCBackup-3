Imports System.ComponentModel
Imports System.Globalization
Imports System.IO
Imports System.Threading
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Public NotInheritable Class BackupManager
    ''' <summary>
    ''' Status of the backup currently running.
    ''' </summary>
    Public Enum BackupStatus
        Starting
        Running
        RevertingChanges
        CreatingThumbnail
    End Enum

    ''' <summary>
    ''' Type of backup to create. This can be a world, a version, or the whole Minecraft installation.
    ''' </summary>
    Public Enum BackupTypes
        World
        Version
        Full
    End Enum

    Public Enum RestoreStatus
        Starting
        RemovingOldFiles
        Restoring
    End Enum

#Region "Delegates"
    ''' <summary>
    ''' Represents the method that will handle the BackupProgressChanged event of the BackupManager class.
    ''' </summary>
    ''' <param name="sender">The source of the event.</param>
    ''' <param name="e">A BackupProgressChangedEventArgs that contains the event data.</param>
    Public Delegate Sub BackupProgressChangedEventHandler(sender As Object, e As BackupProgressChangedEventArgs)

    ''' <summary>
    ''' Represents the method that will handle the BackupCompleted event of the BackupManager class.
    ''' </summary>
    ''' <param name="sender">The source of the event.</param>
    ''' <param name="e">A BackupCompletedEventArgs that contains the event data.</param>
    Public Delegate Sub BackupCompletedEventHandler(sender As Object, e As BackupCompletedEventArgs)

    Public Delegate Sub RestoreProgressChangedEventHandler(sender As Object, e As RestoreProgressChangedEventArgs)

    Public Delegate Sub RestoreCompletedEventHandler(sender As Object, e As RestoreCompletedEventArgs)
#End Region

    ' Threads
    Private PostBackupThread As Thread = Nothing

    Private RestoreThread As Thread = Nothing
    Private PreRestoreThread As Thread = Nothing

    Private DeleteDirectoryThread As Thread = Nothing

    ' Async Operations
    Private BackupAsyncOperation As AsyncOperation = Nothing
    Private RestoreAsyncOperation As AsyncOperation = Nothing

    ' Async Operation Callbacks
    Private BackupProgressChangedCallback As SendOrPostCallback
    Private BackupCompletedCallback As SendOrPostCallback

    Private RestoreProgressChangedCallback As SendOrPostCallback
    Private RestoreCompletedCallback As SendOrPostCallback

#Region "Events"
    ''' <summary>
    ''' Occurs when backup progress is changed.
    ''' </summary>
    Public Event BackupProgressChanged As BackupProgressChangedEventHandler

    ''' <summary>
    ''' Occurs when backup is complete.
    ''' </summary>
    Public Event BackupCompleted As BackupCompletedEventHandler

    Public Event RestoreProgressChanged As RestoreProgressChangedEventHandler

    Public Event RestoreCompleted As RestoreCompletedEventHandler
#End Region

    ' Exceptions
    Private BackupError As Exception
    Private RestoreError As Exception

#Region "Properties"
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
#End Region

    Public Sub New()

        ' Set callbacks
        BackupProgressChangedCallback = New SendOrPostCallback(AddressOf SendBackupProgressChanged)
        BackupCompletedCallback = New SendOrPostCallback(AddressOf SendBackupCompleted)

        RestoreProgressChangedCallback = New SendOrPostCallback(AddressOf SendRestoreProgressChanged)
        RestoreCompletedCallback = New SendOrPostCallback(AddressOf SendRestoreCompleted)

    End Sub

    Public Sub Cancel()

        ' Set CancelPending to True
        _CancellationPending = True

    End Sub

    Public Sub BackupAsync(name As String, path As String, type As String, description As String, group As String, launcher As Game.Launcher, modpack As String)

        ' Check if BackupManager is busy
        If _IsBusy Then

            ' Throw exception if BackupManager is busy
            Throw New InvalidOperationException("BackupManager is busy!")

        End If

        ' Set busy to True and CancelPending to false
        _IsBusy = True
        _CancellationPending = False

        ' Create new AsyncOperation for backup
        BackupAsyncOperation = AsyncOperationManager.CreateOperation(Nothing)

        SendBackupProgressChanged(New BackupProgressChangedEventArgs(BackupStatus.Starting))

        Dim zipper As New Zipper()
        Dim progress As Single
        Dim EstimatedTimeRemaining As TimeSpan = TimeSpan.FromSeconds(0)
        Dim Stopwatch As New Stopwatch()
        Dim TransferRate As Single

        AddHandler zipper.CompressProgressChanged, Sub(s, args)

                                                       ' If cancellation is pending, stop compression and exit method
                                                       If _CancellationPending Then

                                                           zipper.CancelCompress()

                                                           Return

                                                       End If

                                                       ' Check if bytes copied is more than 0 to prevent division by zero error
                                                       If args.BytesCompressed > 0 Then

                                                           ' Set estimated time remaining
                                                           EstimatedTimeRemaining = TimeSpan.FromMilliseconds(Stopwatch.ElapsedMilliseconds / args.BytesCompressed * (args.TotalBytes - args.BytesCompressed))

                                                       End If

                                                       ' Set progress
                                                       progress = (args.BytesCompressed / args.TotalBytes) * 100

                                                       ' Set transfer rate (bytes per second)
                                                       TransferRate = args.BytesCompressed / Stopwatch.Elapsed.TotalSeconds

                                                       ' Post backup progress callback with current progress data
                                                       BackupAsyncOperation.Post(BackupProgressChangedCallback, New BackupProgressChangedEventArgs(BackupStatus.Running, progress, EstimatedTimeRemaining, TransferRate))

                                                   End Sub

        AddHandler zipper.CompressCompleted, Sub(s, args)

                                                 ' Stop stopwatch
                                                 Stopwatch.Stop()

                                                 ' Check if an error occured or was cancelled
                                                 If args.Error IsNot Nothing Then

                                                     ' Send error to backup completed callback
                                                     BackupAsyncOperation.PostOperationCompleted(BackupCompletedCallback, New BackupCompletedEventArgs(args.Error, False))

                                                 Else

                                                     ' Create BackupEventArgs
                                                     Dim arguments As New BackupEventArgs(name, path, type, description, group, launcher, modpack)

                                                     ' Start post backup thread
                                                     PostBackupThread = New Thread(AddressOf PostBackupThreadStart)
                                                     PostBackupThread.Start(arguments)

                                                 End If

                                             End Sub

        Directory.CreateDirectory(IO.Path.Combine(My.Settings.BackupsFolderLocation, name))

        Stopwatch.Start()

        zipper.CompressDirectoryAsync(path, IO.Path.Combine(My.Settings.BackupsFolderLocation, name, "backup.zip"))
    End Sub

    Private Sub PostBackupThreadStart(args As BackupEventArgs)

        ' Check if backup was cancelled
        If _CancellationPending Then

            ' Post reverting changes as progress
            BackupAsyncOperation.Post(BackupProgressChangedCallback, New BackupProgressChangedEventArgs(BackupStatus.RevertingChanges))

            ' Delete backup
            My.Computer.FileSystem.DeleteDirectory(Path.Combine(My.Settings.BackupsFolderLocation, args.Name), FileIO.DeleteDirectoryOption.DeleteAllContents)

            ' Post operation completed
            BackupAsyncOperation.PostOperationCompleted(BackupCompletedCallback, New BackupCompletedEventArgs(Nothing, True))

            ' Exit method
            Return

        End If

        ' Check if backup is a world, user wants thumbnails to be created and mcmap exists
        If args.Type = BackupTypes.World And My.Settings.CreateThumbOnWorld And File.Exists("mcmap/mcmap.exe") And File.Exists(Path.Combine(args.Path, "level.dat")) Then

            ' Set progress to creating thumbnail
            BackupAsyncOperation.Post(BackupProgressChangedCallback, New BackupProgressChangedEventArgs(BackupStatus.CreatingThumbnail, 0))

            ' Copy level data over so it can be read by Substrate when the backup is selected
            My.Computer.FileSystem.CopyFile(Path.Combine(args.Path, "level.dat"), Path.Combine(My.Settings.BackupsFolderLocation, args.Name, "level.dat"))

            ' Create MCMap process
            Dim MCMapProcess As New Process()

            ' Set MCMap start info
            With MCMapProcess.StartInfo

                ' Set filename to mcmap executable
                .FileName = """mcmap\mcmap.exe"""

                ' Set working directory to mcmap directory
                .WorkingDirectory = "mcmap\"

                ' Set arguments to create a 30×30 chunk area for thumb
                .Arguments = String.Format(" -from -15 -15 -to 15 15 -file ""{0}\{1}\thumb.png"" ""{2}""", My.Settings.BackupsFolderLocation, args.Name, args.Path)

                ' Don't create a command prompt window
                .CreateNoWindow = True

                ' Don't use shell execute to allow output redirection
                .UseShellExecute = False

                ' Redirect output and error
                .RedirectStandardOutput = True
                .RedirectStandardError = True
            End With

            ' Create external step number variable
            Dim StepNumber As Integer

            ' Create method for receiving MCMap output
            Dim handler = Sub(sender As Object, e As DataReceivedEventArgs)

                              ' Return if data is not set
                              If e.Data = Nothing Then Return

                              ' Log for debug purposes
                              Log.Verbose("[MCMAP] " & e.Data)

                              ' Set step number according to output text
                              If e.Data.Contains("Loading all chunks") Then
                                  StepNumber = 0
                              ElseIf e.Data.Contains("Optimizing terrain") Then
                                  StepNumber = 1
                              ElseIf e.Data.Contains("Drawing map") Then
                                  StepNumber = 2
                              ElseIf e.Data.Contains("Writing to file") Then
                                  StepNumber = 3
                              End If

                              ' Kill MCMap and return if cancellation is pending
                              If CancellationPending Then

                                  ' Kill MCMap process
                                  MCMapProcess.Kill()

                                  ' Exit method
                                  Return

                              End If

                              ' Check if output contains square brackets, indicating percentage
                              If e.Data.Contains("[") And e.Data.Contains("]") Then

                                  ' Extract percentage from output text
                                  Dim PercentComplete As Double = Convert.ToDouble((e.Data.Substring(1).Remove(e.Data.IndexOf(".") - 1) / 4) + (StepNumber * 25), New CultureInfo("en-US"))

                                  ' Post progress to callback
                                  BackupAsyncOperation.Post(BackupProgressChangedCallback, New BackupProgressChangedEventArgs(BackupStatus.CreatingThumbnail, PercentComplete))

                              End If

                          End Sub

            ' Set redirect handlers to same method
            AddHandler MCMapProcess.OutputDataReceived, handler
            AddHandler MCMapProcess.ErrorDataReceived, handler

            ' Start MCMap, being reading output and error and wait for process to finish
            With MCMapProcess
                .Start()
                .BeginOutputReadLine()
                .BeginErrorReadLine()
                .WaitForExit()
            End With

        End If

        ' Write info.json file
        WriteInfoJson(args)

        ' Post operation completed to backup completed callback
        BackupAsyncOperation.PostOperationCompleted(BackupCompletedCallback, New BackupCompletedEventArgs(BackupError, False))
    End Sub

    ''' <summary>
    ''' Called using BackupAsyncOperation to update progress on the calling thread.
    ''' </summary>
    ''' <param name="args">Event arguments.</param>
    Private Sub SendBackupProgressChanged(args As Object)

        ' Raise backup progress changed event
        RaiseEvent BackupProgressChanged(Me, DirectCast(args, BackupProgressChangedEventArgs))

    End Sub

    ''' <summary>
    ''' Called by BackupAsyncOperation after all backup related operations have been completed.
    ''' </summary>
    ''' <param name="args">Event arguments.</param>
    Private Sub SendBackupCompleted(args As Object)

        ' Reset everything to default values
        _IsBusy = False
        _CancellationPending = False
        BackupAsyncOperation = Nothing

        ' Raise backup completed event
        RaiseEvent BackupCompleted(Me, DirectCast(args, BackupCompletedEventArgs))

    End Sub

    Public Sub RestoreAsync(backupName As String, restoreLocation As String, backupType As BackupTypes)

        If Me._IsBusy Then

            Throw New InvalidOperationException("Backup Manager is busy!")

        End If

        Me._IsBusy = True
        Me._CancellationPending = False

        RestoreAsyncOperation = AsyncOperationManager.CreateOperation(Nothing)

        SendRestoreProgressChanged(New RestoreProgressChangedEventArgs(RestoreStatus.Starting))

        If Directory.Exists(restoreLocation) Then

            Dim fileSystemManager As New AsyncFileSystemManager()

            AddHandler fileSystemManager.DeleteDirectoryProgressChanged, Sub(sender, e)

                                                                             If _CancellationPending Then

                                                                                 fileSystemManager.Cancel()

                                                                             End If

                                                                             RestoreAsyncOperation.Post(RestoreProgressChangedCallback, New RestoreProgressChangedEventArgs(RestoreStatus.RemovingOldFiles, e.ProgressPercentage))

                                                                         End Sub

            AddHandler fileSystemManager.DeleteDirectoryCompleted, Sub(sender, e)

                                                                       If e.Error IsNot Nothing Then

                                                                           RestoreAsyncOperation.PostOperationCompleted(RestoreCompletedCallback, New RestoreCompletedEventArgs(e.Error, _CancellationPending))

                                                                       ElseIf e.Cancelled

                                                                           RestoreAsyncOperation.PostOperationCompleted(RestoreCompletedCallback, New RestoreCompletedEventArgs(Nothing, True))

                                                                       Else

                                                                           RestoreThread = New Thread(AddressOf RestoreThreadStart)
                                                                           RestoreThread.Start(New RestoreEventArgs(backupName, restoreLocation, backupType))

                                                                       End If

                                                                   End Sub

            fileSystemManager.DeleteDirectoryAsync(restoreLocation, FileIO.DeleteDirectoryOption.DeleteAllContents)

        Else

            RestoreThread = New Thread(AddressOf RestoreThreadStart)
            RestoreThread.Start(New RestoreEventArgs(backupName, restoreLocation, backupType))

        End If

    End Sub

    Private Sub RestoreThreadStart(e As RestoreEventArgs)

        Dim BackupPath As String = Path.Combine(My.Settings.BackupsFolderLocation, e.BackupName, "backup.zip")

        Dim stopwatch As New Stopwatch()
        Dim estimatedTimeRemaining As TimeSpan = TimeSpan.FromSeconds(0)
        Dim progress As Single
        Dim transferRate As Single

        If File.Exists(BackupPath) Then

            Dim zipper As New Zipper()

            AddHandler zipper.ExtractProgressChanged, Sub(s, args)

                                                          ' If cancellation is pending, stop compression and exit method
                                                          If _CancellationPending Then

                                                              zipper.CancelExtract()

                                                              Return

                                                          End If

                                                          ' Check if bytes copied is more than 0 to prevent division by zero error
                                                          If args.BytesExtracted > 0 Then

                                                              ' Set estimated time remaining
                                                              estimatedTimeRemaining = TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds / args.BytesExtracted * (args.TotalBytes - args.BytesExtracted))

                                                          End If

                                                          ' Set progress
                                                          progress = (args.BytesExtracted / args.TotalBytes) * 100

                                                          ' Set transfer rate (bytes per second)
                                                          transferRate = args.BytesExtracted / stopwatch.Elapsed.TotalSeconds

                                                          ' Post backup progress callback with current progress data
                                                          RestoreAsyncOperation.Post(RestoreProgressChangedCallback, New RestoreProgressChangedEventArgs(RestoreStatus.Restoring, progress, estimatedTimeRemaining, transferRate))

                                                      End Sub

            AddHandler zipper.ExtractCompleted, Sub(s, args)

                                                    RestoreAsyncOperation.PostOperationCompleted(RestoreCompletedCallback, New RestoreCompletedEventArgs(Nothing, _CancellationPending))

                                                End Sub

            stopwatch.Start()

            zipper.ExtractAsync(BackupPath, e.RestoreLocation)

        Else

            Dim fileSystemManager As New AsyncFileSystemManager()

            AddHandler fileSystemManager.CopyDirectoryProgressChanged, Sub(s, args)

                                                                           ' If cancellation is pending, stop compression and exit method
                                                                           If _CancellationPending Then

                                                                               fileSystemManager.Cancel()

                                                                               Return

                                                                           End If

                                                                           ' Check if bytes copied is more than 0 to prevent division by zero error
                                                                           If args.BytesCopied > 0 Then

                                                                               ' Set estimated time remaining
                                                                               estimatedTimeRemaining = TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds / args.BytesCopied * (args.TotalBytes - args.BytesCopied))

                                                                           End If

                                                                           ' Set progress
                                                                           progress = (args.BytesCopied / args.TotalBytes) * 100

                                                                           ' Set transfer rate (bytes per second)
                                                                           transferRate = args.BytesCopied / stopwatch.Elapsed.TotalSeconds

                                                                           ' Post backup progress callback with current progress data
                                                                           RestoreAsyncOperation.Post(RestoreProgressChangedCallback, New RestoreProgressChangedEventArgs(RestoreStatus.Restoring, progress, estimatedTimeRemaining, transferRate))

                                                                       End Sub

            AddHandler fileSystemManager.CopyDirectoryCompleted, Sub(s, args)

                                                                     RestoreAsyncOperation.PostOperationCompleted(RestoreCompletedCallback, New RestoreCompletedEventArgs(Nothing, _CancellationPending))

                                                                 End Sub

            stopwatch.Start()

            fileSystemManager.CopyDirectoryAsync(Path.Combine(My.Settings.BackupsFolderLocation, e.BackupName), e.RestoreLocation, True)

        End If

    End Sub

    ' TODO: AsyncOps with delete & copy, move to other class
    Private Sub DeleteDirectoryAsync(directory As String, onDirectoryNotEmpty As FileIO.DeleteDirectoryOption)

        DeleteDirectoryThread = New Thread(AddressOf DeleteDirectory)
        DeleteDirectoryThread.Start(directory)

    End Sub

    Private Sub DeleteDirectory(directory As String, Optional toplevel As Boolean = True)

        Dim DirectoryInfo As New DirectoryInfo(directory)

        For Each Subdirectory As DirectoryInfo In DirectoryInfo.GetDirectories()

            DeleteDirectory(Subdirectory.FullName, False)

        Next

        For Each File As FileInfo In DirectoryInfo.GetFiles()

            File.Delete()

            If _CancellationPending Then

                Exit For

            End If

        Next

        If _CancellationPending Then

            If toplevel Then

                RestoreAsyncOperation.PostOperationCompleted(RestoreCompletedCallback, New RestoreCompletedEventArgs(Nothing, True))

            End If

            Return

        End If

        IO.Directory.Delete(directory)

    End Sub

    Private Sub SendRestoreProgressChanged(e As RestoreProgressChangedEventArgs)

        RaiseEvent RestoreProgressChanged(Me, e)

    End Sub

    Private Sub SendRestoreCompleted(e As RestoreCompletedEventArgs)

        _IsBusy = False
        _CancellationPending = False
        RestoreAsyncOperation = Nothing

        RaiseEvent RestoreCompleted(Me, e)

    End Sub

    ''' <summary>
    ''' Writes backup information to a JSON encoded file.
    ''' </summary>
    ''' <param name="e">A BackupEventArgs that contains the backup's information</param>
    Private Sub WriteInfoJson(e As BackupEventArgs)

        ' Create new JObject
        Dim InfoJson As New JObject

        ' Add backup information to JObject
        InfoJson.Add(New JProperty("OriginalName", New DirectoryInfo(e.Path).Name))
        InfoJson.Add(New JProperty("Type", e.Type))
        InfoJson.Add(New JProperty("Description", e.Description))
        InfoJson.Add(New JProperty("Group", e.Group))
        InfoJson.Add(New JProperty("Launcher", e.Launcher))
        InfoJson.Add(New JProperty("Modpack", e.Modpack))

        ' Write to info.json file
        Using SW As New StreamWriter(My.Settings.BackupsFolderLocation + "\" + e.Name + "\info.json")

            ' Write JSON to file
            SW.Write(JsonConvert.SerializeObject(InfoJson))

        End Using

    End Sub

    ''' <summary>
    ''' Gets the size of a directory based on the files it contains.
    ''' </summary>
    ''' <param name="directory">Path of the directory.</param>
    ''' <returns>The directory's size, in bytes.</returns>
    Private Function GetDirectorySize(directory As String) As Long
        If IO.Directory.Exists(directory) Then
            Dim Bytes As Long = 0

            For Each File As FileInfo In New DirectoryInfo(directory).GetFiles("*", SearchOption.AllDirectories)
                Bytes += File.Length
            Next

            Return Bytes
        Else
            Return 0
        End If
    End Function

    ''' <summary>
    ''' Creates an estimated time remaining string from language settings and time span.
    ''' </summary>
    ''' <param name="span">Time to convert to text.</param>
    ''' <returns>A string describing the estimated time left.</returns>
    Public Function EstimatedTimeSpanToString(span As TimeSpan)
        If span.Hours > 0 Then
            Return String.Format(MCBackup.Language.GetString("TimeLeft.HoursMinutesSeconds"), Math.Floor(span.TotalHours), span.Minutes, Math.Round(span.Seconds / 10) * 10)
        ElseIf span.TotalMinutes >= 1 Then
            Return String.Format(MCBackup.Language.GetString("TimeLeft.MinutesSeconds"), Math.Floor(span.TotalMinutes), Math.Round(span.Seconds / 10) * 10)
        ElseIf span.Seconds > 5 Then
            Return String.Format(MCBackup.Language.GetString("TimeLeft.Seconds"), Math.Round(span.Seconds / 10) * 10)
        Else
            Return MCBackup.Language.GetString("TimeLeft.LessThanFive")
        End If
    End Function
End Class
