Imports System.ComponentModel
Imports System.Globalization
Imports System.IO
Imports System.Threading
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Public Class BackupManager
    Public Enum BackupStatus
        Running
        RevertingChanges
        CreatingThumbnail
    End Enum

    Public Enum BackupType
        World
        Version
        Full
    End Enum

    Public Delegate Sub BackupProgressChangedEventHandler(sender As Object, e As BackupProgressChangedEventArgs)
    Public Delegate Sub BackupCompletedEventHandler(sender As Object, e As BackupCompletedEventArgs)

    Private BackupThread As Thread = Nothing
    Private BackupProgressThread As Thread = Nothing

    Private BackupAsyncOperation As AsyncOperation = Nothing

    Private BackupProgressChangedCallback As SendOrPostCallback
    Private BackupCompletedCallback As SendOrPostCallback

    Public Event BackupProgressChanged As BackupProgressChangedEventHandler
    Public Event BackupCompleted As BackupCompletedEventHandler

    Private BackupError As Exception

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

    Public Sub New()
        BackupProgressChangedCallback = New SendOrPostCallback(AddressOf PostBackupProgressChanged)
        BackupCompletedCallback = New SendOrPostCallback(AddressOf PostBackupCompleted)
    End Sub

    Public Sub Cancel()
        CancelPending = True
    End Sub

    Public Sub BackupAsync(name As String, path As String, type As String, description As String, group As String, launcher As Game.Launcher, modpack As String)
        If Busy Then
            Throw New InvalidOperationException("BackupManager is busy!")
        End If

        Busy = True
        CancelPending = False

        BackupAsyncOperation = AsyncOperationManager.CreateOperation(Nothing)

        Dim arguments As New BackupEventArgs(name, path, type, description, group, launcher, modpack)

        BackupThread = New Thread(AddressOf BackupThreadStart)
        BackupThread.Start(arguments)

        BackupProgressThread = New Thread(AddressOf BackupProgressThreadStart)
        BackupProgressThread.Start(arguments)
    End Sub

    Private Sub BackupThreadStart(args As Object)
        Dim err As Exception = Nothing
        Dim cancelled As Boolean = False
        Dim e = DirectCast(args, BackupEventArgs)

        Try
            My.Computer.FileSystem.CopyDirectory(e.Path, My.Settings.BackupsFolderLocation + "\" + e.Name, True)
        Catch ex As ThreadAbortException
            Log.Print("Thread aborted successfully.")
        Catch ex As Exception
            BackupError = ex
        End Try
    End Sub

    Private Sub BackupProgressThreadStart(args As BackupEventArgs)
        Dim TotalBytes As Long = GetDirectorySize(args.Path)
        Dim BytesCopied As Long = 0
        Dim Progress As Single = 0
        Dim TransferRate As Single = 0
        Dim EstimatedRemainingTime As TimeSpan

        Dim Stopwatch As New Stopwatch()
        Stopwatch.Start()

        While Progress < 100 And CancelPending = False
            If BytesCopied > 0 Then
                EstimatedRemainingTime = TimeSpan.FromMilliseconds(Stopwatch.ElapsedMilliseconds / BytesCopied * (TotalBytes - BytesCopied))
            End If

            BytesCopied = GetDirectorySize(My.Settings.BackupsFolderLocation + "\" + args.Name)

            Progress = (BytesCopied / TotalBytes) * 100

            TransferRate = BytesCopied / EstimatedRemainingTime.TotalSeconds

            Debug.Print("{0}\r\n{1}\r\n{2}", BytesCopied, TotalBytes, Progress)

            BackupAsyncOperation.Post(BackupProgressChangedCallback, New BackupProgressChangedEventArgs(BackupStatus.Running, Progress, EstimatedRemainingTime, TransferRate))

            Thread.Sleep(100)
        End While

        Stopwatch.Stop()

        If CancelPending Then
            BackupThread.Abort()

            BackupAsyncOperation.Post(BackupProgressChangedCallback, New BackupProgressChangedEventArgs(BackupStatus.RevertingChanges))

            My.Computer.FileSystem.DeleteDirectory(My.Settings.BackupsFolderLocation + "\" + args.Name, FileIO.DeleteDirectoryOption.DeleteAllContents)

            Dim cancelArgs As BackupCompletedEventArgs = New BackupCompletedEventArgs(BackupError, True)
            BackupAsyncOperation.PostOperationCompleted(BackupCompletedCallback, cancelArgs)
            Busy = False

            Return
        End If

        If File.Exists("mcmap/mcmap.exe") Then
            Dim MCMap As New Process()

            With MCMap.StartInfo
                .FileName = Chr(34) & "mcmap\mcmap.exe" & Chr(34)
                .WorkingDirectory = "mcmap\"
                .Arguments = String.Format(" -from -15 -15 -to 15 15 -file ""{0}\{1}\thumb.png"" ""{2}""", My.Settings.BackupsFolderLocation, args.Name, args.Path)
                .CreateNoWindow = True
                .UseShellExecute = False
                .RedirectStandardOutput = True
                .RedirectStandardError = True
            End With

            AddHandler MCMap.OutputDataReceived, AddressOf MCMap_DataReceived
            AddHandler MCMap.ErrorDataReceived, AddressOf MCMap_DataReceived

            With MCMap
                .Start()
                .BeginOutputReadLine()
                .BeginErrorReadLine()
                .WaitForExit()
            End With

            WriteInfoJson(args)
            BackupAsyncOperation.PostOperationCompleted(BackupCompletedCallback, New BackupCompletedEventArgs(BackupError, False))
            Busy = False
        Else
            WriteInfoJson(args)

            Dim completedArgs As BackupCompletedEventArgs = New BackupCompletedEventArgs(BackupError, CancelPending)
            BackupAsyncOperation.PostOperationCompleted(BackupCompletedCallback, completedArgs)
            Busy = False
        End If
    End Sub

    Private StepNumber As Integer

    Private Sub MCMap_DataReceived(sender As Object, e As DataReceivedEventArgs)
        If e.Data = Nothing Then
            Exit Sub
        End If

        Log.Print("[MCMAP] " & e.Data, Log.Level.Debug)

        If e.Data.Contains("Loading all chunks") Then
            StepNumber = 0
        ElseIf e.Data.Contains("Optimizing terrain") Then
            StepNumber = 1
        ElseIf e.Data.Contains("Drawing map") Then
            StepNumber = 2
        ElseIf e.Data.Contains("Writing to file") Then
            StepNumber = 3
        End If

        If CancellationPending Then
            Return
        End If

        If e.Data.Contains("[") And e.Data.Contains("]") Then
            Dim PercentComplete As Double = Convert.ToDouble((e.Data.Substring(1).Remove(e.Data.IndexOf(".") - 1) / 4) + (StepNumber * 25), New CultureInfo("en-US"))

            BackupAsyncOperation.Post(BackupProgressChangedCallback, New BackupProgressChangedEventArgs(BackupStatus.CreatingThumbnail, PercentComplete))
        End If
    End Sub

    Private Sub PostBackupProgressChanged(args As Object)
        OnBackupProgressChanged(DirectCast(args, BackupProgressChangedEventArgs))
    End Sub

    Private Sub PostBackupCompleted(args As Object)
        OnBackupCompleted(DirectCast(args, BackupCompletedEventArgs))
    End Sub

    Protected Sub OnBackupProgressChanged(e As BackupProgressChangedEventArgs)
        RaiseEvent BackupProgressChanged(Me, e)
    End Sub

    Protected Sub OnBackupCompleted(e As BackupCompletedEventArgs)
        RaiseEvent BackupCompleted(Me, e)
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
End Class
