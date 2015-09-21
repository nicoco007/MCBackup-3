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
        Running
        RevertingChanges
        CreatingThumbnail
    End Enum

    ''' <summary>
    ''' Type of backup to create. This can be a world, a version, or the whole Minecraft installation.
    ''' </summary>
    Public Enum BackupType
        World
        Version
        Full
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
#End Region

    ' Threads
    Private BackupThread As Thread = Nothing
    Private BackupProgressThread As Thread = Nothing

    ' Async Operations
    Private BackupAsyncOperation As AsyncOperation = Nothing

    ' Async Operation Callbacks
    Private BackupProgressChangedCallback As SendOrPostCallback
    Private BackupCompletedCallback As SendOrPostCallback

#Region "Events"
    ''' <summary>
    ''' Occurs when backup progress is changed.
    ''' </summary>
    Public Event BackupProgressChanged As BackupProgressChangedEventHandler

    ''' <summary>
    ''' Occurs when backup is complete.
    ''' </summary>
    Public Event BackupCompleted As BackupCompletedEventHandler
#End Region

    ' Exceptions
    Private BackupError As Exception

#Region "Properties"
    Private Busy As Boolean = False

    Public ReadOnly Property IsBusy As Boolean
        Get
            Return Busy
        End Get
    End Property

    Private CancelPending As Boolean = False

    Public ReadOnly Property CancellationPending As Boolean
        Get
            Return CancelPending
        End Get
    End Property
#End Region

    Public Sub New()

        ' Set callbacks
        BackupProgressChangedCallback = New SendOrPostCallback(AddressOf PostBackupProgressChanged)
        BackupCompletedCallback = New SendOrPostCallback(AddressOf PostBackupCompleted)

    End Sub

    Public Sub Cancel()

        ' Set CancelPending to True
        CancelPending = True

    End Sub

    Public Sub BackupAsync(name As String, path As String, type As String, description As String, group As String, launcher As Game.Launcher, modpack As String)

        ' Check if BackupManager is busy
        If Busy Then

            ' Throw exception if BackupManager is busy
            Throw New InvalidOperationException("BackupManager is busy!")

        End If

        ' Set busy to True and CancelPending to false
        Busy = True
        CancelPending = False

        ' Create new AsyncOperation for backup
        BackupAsyncOperation = AsyncOperationManager.CreateOperation(Nothing)

        ' Create BackupEventArgs
        Dim arguments As New BackupEventArgs(name, path, type, description, group, launcher, modpack)

        ' Create new backup thread and start it
        BackupThread = New Thread(AddressOf BackupThreadStart)
        BackupThread.Start(arguments)

        ' Create new backup progress thread and start it
        BackupProgressThread = New Thread(AddressOf BackupProgressThreadStart)
        BackupProgressThread.Start(arguments)
    End Sub

    Private Sub BackupThreadStart(e As BackupEventArgs)

        ' Wrap in try loop in case of exception
        Try

            ' Create backup
            My.Computer.FileSystem.CopyDirectory(e.Path, My.Settings.BackupsFolderLocation + "\" + e.Name, True)

        Catch ex As ThreadAbortException

            ' ThreadAbortException only occurs when Thread.Abort() is called, which means the operation was cancelled
            Log.Print("Thread aborted successfully.")

        Catch ex As Exception

            ' If any other exception is caught, set BackupError to that exception
            BackupError = ex

        End Try

    End Sub

    Private Sub BackupProgressThreadStart(args As BackupEventArgs)

        ' Create variables
        Dim TotalBytes As Long = GetDirectorySize(args.Path)
        Dim BytesCopied As Long = 0
        Dim Progress As Single = 0
        Dim TransferRate As Single = 0
        Dim EstimatedRemainingTime As TimeSpan

        ' Create new stopwatch
        Dim Stopwatch As New Stopwatch()
        Stopwatch.Start()

        ' While copy is in progress and cancel is not pending
        While Progress < 100 And CancelPending = False

            ' Set bytes copied to current copied directory size
            BytesCopied = GetDirectorySize(My.Settings.BackupsFolderLocation + "\" + args.Name)

            ' Check if bytes copied is more than 0 to prevent division by zero error
            If BytesCopied > 0 Then

                ' Set estimated time remaining
                EstimatedRemainingTime = TimeSpan.FromMilliseconds(Stopwatch.ElapsedMilliseconds / BytesCopied * (TotalBytes - BytesCopied))

            End If

            ' Set progress
            Progress = (BytesCopied / TotalBytes) * 100

            ' Set transfer rate (bytes per second)
            TransferRate = BytesCopied / Stopwatch.Elapsed.Seconds

            ' Post backup progress callback with current progress data
            BackupAsyncOperation.Post(BackupProgressChangedCallback, New BackupProgressChangedEventArgs(BackupStatus.Running, Progress, EstimatedRemainingTime, TransferRate))

            ' Make thread sleep to avoid overflow exceptions
            Thread.Sleep(100)
        End While

        ' Stop stopwatch
        Stopwatch.Stop()

        ' Check if cancellation was pending
        If CancelPending Then

            ' Abort copy if operation was cancelled
            BackupThread.Abort()

            ' Post reverting changes to progress callback
            BackupAsyncOperation.Post(BackupProgressChangedCallback, New BackupProgressChangedEventArgs(BackupStatus.RevertingChanges))

            ' Delete backup
            My.Computer.FileSystem.DeleteDirectory(My.Settings.BackupsFolderLocation + "\" + args.Name, FileIO.DeleteDirectoryOption.DeleteAllContents)

            ' Post operation completed to backup completed callback
            BackupAsyncOperation.PostOperationCompleted(BackupCompletedCallback, New BackupCompletedEventArgs(BackupError, True))

            ' Set busy to false
            Busy = False

            ' Exit method
            Return

        End If

        ' Check if backup is a world, user wants thumbnails to be created and mcmap exists
        If args.Type = BackupType.World And My.Settings.CreateThumbOnWorld And File.Exists("mcmap/mcmap.exe") Then

            ' Create MCMap process
            Dim MCMapProcess As New Process()

            ' Set MCMap start info
            With MCMapProcess.StartInfo

                ' Set filename to mcmap executable
                .FileName = Chr(34) & "mcmap\mcmap.exe" & Chr(34)

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

            Dim StepNumber As Integer

            Dim handler = Sub(sender As Object, e As DataReceivedEventArgs)

                              ' Return if data is not set
                              If e.Data = Nothing Then Return

                              ' Log for debug purposes
                              Log.Print("[MCMAP] " & e.Data, Log.Level.Debug)

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

            ' Write info.json file
            WriteInfoJson(args)

            ' Post operation completed to backup completed callback
            BackupAsyncOperation.PostOperationCompleted(BackupCompletedCallback, New BackupCompletedEventArgs(BackupError, False))

            ' Set busy to false
            Busy = False

        Else

            ' Write info.json file
            WriteInfoJson(args)

            ' Post operation completed to backup completed callback
            BackupAsyncOperation.PostOperationCompleted(BackupCompletedCallback, New BackupCompletedEventArgs(BackupError, False))

            ' Set busy to false
            Busy = False

        End If
    End Sub

    Private Sub PostBackupProgressChanged(args As Object)

        ' Raise backup progress changed event
        RaiseEvent BackupProgressChanged(Me, DirectCast(args, BackupProgressChangedEventArgs))

    End Sub

    Private Sub PostBackupCompleted(args As Object)

        ' Raise backup completed event
        RaiseEvent BackupCompleted(Me, DirectCast(args, BackupCompletedEventArgs))

    End Sub

    Private Sub WriteInfoJson(e As BackupEventArgs)
        Dim InfoJson As New JObject

        InfoJson.Add(New JProperty("OriginalName", New DirectoryInfo(e.Path).Name))
        InfoJson.Add(New JProperty("Type", e.Type))
        InfoJson.Add(New JProperty("Description", e.Description))
        InfoJson.Add(New JProperty("Group", e.Group))
        InfoJson.Add(New JProperty("Launcher", e.Launcher))
        InfoJson.Add(New JProperty("Modpack", e.Modpack))

        Using SW As New StreamWriter(My.Settings.BackupsFolderLocation + "\" + e.Name + "\info.json")
            SW.Write(JsonConvert.SerializeObject(InfoJson))
        End Using
    End Sub

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

    Public Function EstimatedTimeSpanToString(span As TimeSpan)
        If span.Hours > 0 Then
            Return String.Format(MCBackup.Language.Dictionary("TimeLeft.HoursMinutesSeconds"), Math.Floor(span.TotalHours), span.Minutes, Math.Round(span.Seconds / 60) * 10)
        ElseIf span.TotalMinutes >= 1 Then
            Return String.Format(MCBackup.Language.Dictionary("TimeLeft.MinutesSeconds"), Math.Floor(span.TotalMinutes), Math.Round(span.Seconds / 60) * 10)
        ElseIf span.Seconds > 5 Then
            Return String.Format(MCBackup.Language.Dictionary("TimeLeft.Seconds"), Math.Round(span.Seconds / 6) * 10)
        Else
            Return MCBackup.Language.Dictionary("TimeLeft.LessThanFive")
        End If
    End Function
End Class
