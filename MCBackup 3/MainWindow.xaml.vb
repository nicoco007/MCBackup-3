'   ╔═══════════════════════════════════════════════════════════════════════════╗
'   ║                         Copyright 2013 nicoco007                          ║
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

Imports System.IO
Imports System.Net
Imports Scripting
Imports System.Windows.Threading
Imports System.ComponentModel.BackgroundWorker
Imports System.ComponentModel
Imports System.Windows.Interop.Imaging

Imports MCBackup.CloseAction
Imports System.Globalization

Partial Class MainWindow
#Region "Variables"
    Private AppData As String = Environ("APPDATA")
    Public BackupInfo(3) As String
    Public RestoreInfo(2) As String

    Private FolderBrowserDialog As New System.Windows.Forms.FolderBrowserDialog

    Private BackupBackgroundWorker As New BackgroundWorker()
    Private DeleteForRestoreBackgroundWorker As New BackgroundWorker()
    Private RestoreBackgroundWorker As New BackgroundWorker()
    Private DeleteBackgroundWorker As New BackgroundWorker()
    Private ThumbnailBackgroundWorker As New BackgroundWorker()

    Public StartupPath As String = System.IO.Directory.GetCurrentDirectory()
    Public ApplicationVersion As String = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()
    Public LatestVersion As String

    Public WithEvents NotifyIcon As New System.Windows.Forms.NotifyIcon

    Public AutoBackupWindow As New AutoBackup
    Private Splash As New Splash
#End Region

#Region "Load"
    Public Sub New()
        Splash.Show()

        Splash.Status.Content = "Starting..."
        Splash.Progress.Value = 0

        Log.StartNew()
        Log.Print("Starting MCBackup")

        InitializeComponent()

        AddHandler BackupBackgroundWorker.DoWork, New DoWorkEventHandler(AddressOf BackupBackgroundWorker_DoWork)
        AddHandler BackupBackgroundWorker.RunWorkerCompleted, New RunWorkerCompletedEventHandler(AddressOf BackupBackgroundWorker_RunWorkerCompleted)
        AddHandler DeleteForRestoreBackgroundWorker.DoWork, New DoWorkEventHandler(AddressOf DeleteForRestoreBackgroundWorker_DoWork)
        AddHandler DeleteForRestoreBackgroundWorker.RunWorkerCompleted, New RunWorkerCompletedEventHandler(AddressOf DeleteForRestoreBackgroundWorker_RunWorkerCompleted)
        AddHandler RestoreBackgroundWorker.DoWork, New DoWorkEventHandler(AddressOf RestoreBackgroundWorker_DoWork)
        AddHandler RestoreBackgroundWorker.RunWorkerCompleted, New RunWorkerCompletedEventHandler(AddressOf RestoreBackgroundWorker_RunWorkerCompleted)
        AddHandler DeleteBackgroundWorker.DoWork, New DoWorkEventHandler(AddressOf DeleteBackgroundWorker_DoWork)
        AddHandler DeleteBackgroundWorker.RunWorkerCompleted, New RunWorkerCompletedEventHandler(AddressOf DeleteBackgroundWorker_RunWorkerCompleted)
        AddHandler ThumbnailBackgroundWorker.DoWork, New DoWorkEventHandler(AddressOf ThumbnailBackgroundWorker_DoWork)
        AddHandler ThumbnailBackgroundWorker.RunWorkerCompleted, New RunWorkerCompletedEventHandler(AddressOf ThumbnailBackgroundWorker_RunWorkerCompleted)

        Me.Title = "MCBackup " & ApplicationVersion

        NotifyIcon.Icon = New System.Drawing.Icon(Application.GetResourceStream(New Uri("pack://application:,,,/Resources/MCBackup.ico")).Stream)
        Dim ContextMenu As New System.Windows.Forms.ContextMenu
        Dim ExitToolbarMenuItem As New System.Windows.Forms.MenuItem
        ExitToolbarMenuItem.Text = "E&xit"
        AddHandler ExitToolbarMenuItem.Click, AddressOf ExitToolbarMenuItem_Click
        ContextMenu.MenuItems.Add(ExitToolbarMenuItem)
        NotifyIcon.ContextMenu = ContextMenu
        NotifyIcon.Visible = True

        Splash.Status.Content = "Loading Language..."
        Splash.Progress.Value = 1

        Dim DefaultLanguage As String = "en_US"

        Select Case CultureInfo.CurrentCulture.ThreeLetterISOLanguageName
            Case "eng"
                DefaultLanguage = "en_US"
            Case "fra"
                DefaultLanguage = "fr_FR"
        End Select

        Try
            If My.Settings.Language = "" Then
                My.Settings.Language = DefaultLanguage
                MCBackup.Language.Load(My.Settings.Language & ".lang")
            Else
                MCBackup.Language.Load(My.Settings.Language & ".lang")
            End If
        Catch ex As Exception
            Log.Print("Could not load language file: """ & My.Settings.Language & """", Log.Type.Severe)
            Log.Print(ex.Message, Log.Type.Severe)
            ErrorWindow.Show("Error!", "Error: Language file not found (" & My.Settings.Language & ")! MCBackup will now exit.", ex, True)
            My.Settings.Language = DefaultLanguage
            Me.ClsType = CloseType.ForceClose
            Me.Close()
        End Try

        Splash.Status.Content = "Loading properties..."
        Splash.Progress.Value = 2

        Main.ListView.Opacity = My.Settings.InterfaceOpacity / 100
        Main.Sidebar.Background = New SolidColorBrush(Color.FromArgb(My.Settings.InterfaceOpacity * 2.55, 255, 255, 255))

        StatusLabel.Foreground = New SolidColorBrush(My.Settings.StatusLabelColor)

        If My.Settings.BackgroundImageLocation = "" Then
            Me.Background = New SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 240, 240, 240))
        Else
            Dim Brush As New ImageBrush(New BitmapImage(New Uri(My.Settings.BackgroundImageLocation)))
            Brush.Stretch = My.Settings.BackgroundImageStretch
            Me.Background = Brush
        End If

        If My.Settings.BackupsFolderLocation = "" Then
            My.Settings.BackupsFolderLocation = StartupPath & "\backups"
        End If

        Log.Print("Set Backups folder location to """ & My.Settings.BackupsFolderLocation & """")

        My.Computer.FileSystem.CreateDirectory(My.Settings.BackupsFolderLocation)
    End Sub

    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs)
        Splash.Status.Content = "Checking for updates..."
        Splash.Progress.Value = 3

        If My.Settings.CheckForUpdates Then
            Try
                Dim WebClient As New WebClient
                WebClient.DownloadString("http://content.nicoco007.com/downloads/mcbackup-3/version")

                Dim LatestVersionInt As Integer = LatestVersion.Replace(".", "")
                Dim ApplicationVersionInt As Integer = ApplicationVersion.Replace(".", "")

                If LatestVersionInt > ApplicationVersionInt Then
                    Log.Print("New MCBackup version available (Version " & LatestVersion & ")!")
                    Dim UpdateDialog As New UpdateDialog
                    UpdateDialog.Owner = Me
                    UpdateDialog.Show()
                ElseIf LatestVersionInt < ApplicationVersionInt Then
                    Log.Print("MCBackup is running in beta mode (Version " & ApplicationVersion & ")")
                    Me.Title += " BETA"
                Else
                    Log.Print("MCBackup is up-to-date (Version " & ApplicationVersion & ")")
                End If
            Catch ex As Exception
                Log.Print(ex.Message, Log.Type.Severe)
            End Try
        End If

        Splash.Status.Content = "Finding Minecraft..."
        Splash.Progress.Value = 4

        If Not My.Computer.FileSystem.FileExists(My.Settings.MinecraftFolderLocation & "\launcher.jar") Then ' Check if saved directory exists AND still has Minecraft installed in it
            If My.Computer.FileSystem.FileExists(AppData & "\.minecraft\launcher.jar") Then ' If not, check for the usual Minecraft folder location
                My.Settings.MinecraftFolderLocation = AppData & "\.minecraft" ' Set folder location to default Minecraft folder location
                My.Settings.SavesFolderLocation = My.Settings.MinecraftFolderLocation & "\saves"
            Else
                MetroMessageBox.Show(MCBackup.Language.Dictionnary("Message.Error.NoMinecraftInstall"), MCBackup.Language.Dictionnary("Message.Caption.Error"), MessageBoxButton.OK, MessageBoxImage.Error)
                Log.Print("Minecraft folder not found", Log.Type.Warning)
                MinecraftFolderSearch()
                Exit Sub
            End If
        End If

        My.Computer.FileSystem.CreateDirectory(My.Settings.SavesFolderLocation)

        Log.Print("Minecraft folder set to """ & My.Settings.MinecraftFolderLocation & """")
        Log.Print("Saves folder set to """ & My.Settings.SavesFolderLocation & """")
        RefreshBackupsList()
        Splash.Status.Content = "Done."
        Splash.Progress.Value = 5
        Splash.Hide()
    End Sub

    Private Sub MinecraftFolderSearch()
        If FolderBrowserDialog.ShowDialog = Windows.Forms.DialogResult.OK Then
            If My.Computer.FileSystem.FileExists(FolderBrowserDialog.SelectedPath & "\launcher.jar") Then ' Check if Minecraft exists in that folder
                MetroMessageBox.Show(String.Format(MCBackup.Language.Dictionnary("Message.Info.MinecraftFolderSetTo"), FolderBrowserDialog.SelectedPath), MCBackup.Language.Dictionnary("Message.Caption.Information"), MessageBoxButton.OK, MessageBoxImage.Error) ' Tell user that folder has been selected successfully
                My.Settings.MinecraftFolderLocation = FolderBrowserDialog.SelectedPath
                My.Settings.SavesFolderLocation = My.Settings.MinecraftFolderLocation & "\saves"
                Log.Print("Minecraft folder set to """ & My.Settings.MinecraftFolderLocation & """")
                Log.Print("Saves folder set to """ & My.Settings.SavesFolderLocation & """")
                Exit Sub
            Else
                If MetroMessageBox.Show(MCBackup.Language.Dictionnary("Message.NotInstalledInFolder"), MCBackup.Language.Dictionnary("Message.Caption.Error"), MessageBoxButton.YesNo, MessageBoxImage.Error) = Windows.Forms.DialogResult.Yes Then ' Ask if user wants to try finding folder again
                    MinecraftFolderSearch() ' Restart from beginning if "Yes"
                Else
                    Me.ClsType = CloseType.ForceClose
                    Me.Close() ' Close program if "No"
                End If
            End If
        Else
            Me.ClsType = CloseType.ForceClose
            Me.Close() ' Close program if "Cancel" or "X" buttons are pressed
        End If
    End Sub

    Public Sub RefreshBackupsList()
        ListView.Items.Clear() ' Clear ListView items
        Dim Directory As New IO.DirectoryInfo(My.Settings.BackupsFolderLocation) ' Create a DirectoryInfo variable for the backups folder
        Dim Folders As IO.DirectoryInfo() = Directory.GetDirectories() ' Get all the directories in the backups folder
        Dim Folder As IO.DirectoryInfo ' Used to designate a single folder in the backups folder

        For Each Folder In Folders ' For each folder in the backups folder
            Dim Type As String = "[ERROR]"                  ' <╗
            Dim Description As String = "[ERROR]"           ' <╬ Create variables with default value [ERROR], in case one of the values doesn't exist
            Dim OriginalFolderName As String = "[ERROR]"    ' <╝

            Try
                Using SR As New StreamReader(Directory.ToString & "\" & Folder.ToString & "\info.mcb")
                    Do While SR.Peek <> -1
                        Dim Line As String = SR.ReadLine
                        If Not Line.StartsWith("#") Then
                            If Line.StartsWith("desc=") Then ' If the line starts with description... 
                                Description = Line.Substring(5) ' ...set description subitem to that
                            ElseIf Line.StartsWith("type=") Then
                                Type = Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Line.Substring(5)) ' Set type to capitalized "type=" line
                            ElseIf Line.StartsWith("baseFolderName=") Then
                                OriginalFolderName = Line.Substring(15) ' Set original folder name to "baseFolderName=" line
                            End If
                        End If
                    Loop
                End Using
            Catch ex As Exception
                Log.Print(ex.Message, Log.Type.Severe)
            End Try

            If GetFolderDateCreated(Directory.ToString & "\" & Folder.ToString).AddDays(14) < DateTime.Today Then
                ListView.Items.Add(New ListViewBackupItem(Folder.ToString, GetFolderDateCreated(Directory.ToString & "\" & Folder.ToString), Description, New SolidColorBrush(Color.FromRgb(255, 0, 0))))
            ElseIf GetFolderDateCreated(Directory.ToString & "\" & Folder.ToString).AddDays(7) < DateTime.Today Then
                ListView.Items.Add(New ListViewBackupItem(Folder.ToString, GetFolderDateCreated(Directory.ToString & "\" & Folder.ToString), Description, New SolidColorBrush(Color.FromRgb(225, 175, 0))))
            Else
                ListView.Items.Add(New ListViewBackupItem(Folder.ToString, GetFolderDateCreated(Directory.ToString & "\" & Folder.ToString), Description, New SolidColorBrush(Color.FromRgb(0, 100, 0))))
            End If
        Next

        ListView_SelectionChanged(New Object, New EventArgs)
    End Sub

    Private Sub ListView_SelectionChanged(sender As Object, e As EventArgs) Handles ListView.SelectionChanged
        If ListView.SelectedItems.Count = 0 Then
            RestoreButton.IsEnabled = False
            RenameButton.IsEnabled = False ' Don't allow anything when no items are selected
            DeleteButton.IsEnabled = False
        ElseIf ListView.SelectedItems.Count = 1 Then
            RestoreButton.IsEnabled = True
            RenameButton.IsEnabled = True ' Allow anything if only 1 item is selected
            DeleteButton.IsEnabled = True
        Else
            RestoreButton.IsEnabled = False
            RenameButton.IsEnabled = False ' Only allow deletion if more than 1 item is selected
            DeleteButton.IsEnabled = True
        End If

        If ListView.SelectedItems.Count = 1 Then
            If My.Computer.FileSystem.FileExists(My.Settings.BackupsFolderLocation & "\" & ListView.SelectedItem.Name & "\thumb.png") Then
                ThumbnailImage.Source = BitmapFromUri(New Uri(My.Settings.BackupsFolderLocation & "\" & ListView.SelectedItem.Name & "\thumb.png"))
            Else
                ThumbnailImage.Source = New BitmapImage(New Uri("pack://application:,,,/Resources/nothumb.png"))
            End If

            Dim Type As String = "N/A"
            Dim OriginalFolderName As String = "N/A"

            Try
                Using SR As New StreamReader(My.Settings.BackupsFolderLocation & "\" & ListView.SelectedItem.Name.ToString & "\info.mcb")
                    Do While SR.Peek <> -1
                        Dim Line As String = SR.ReadLine
                        If Not Line.StartsWith("#") Then
                            If Line.StartsWith("type=") Then
                                Type = Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Line.Substring(5)) ' Set type to capitalized "type=" line
                            ElseIf Line.StartsWith("baseFolderName=") Then
                                OriginalFolderName = Line.Substring(15) ' Set original folder name to "baseFolderName=" line
                            End If
                        End If
                    Loop
                End Using
            Catch ex As Exception
                Log.Print(ex.Message, Log.Type.Severe)
            End Try

            OriginalBackupName.Text = OriginalFolderName
            OriginalBackupName.ToolTip = OriginalFolderName
            BackupType.Text = Type
            BackupType.ToolTip = Type
        ElseIf ListView.SelectedItems.Count = 0 Then
            ThumbnailImage.Source = New BitmapImage(New Uri("pack://application:,,,/Resources/nothumb.png"))
            OriginalBackupName.Text = "N/A"
            OriginalBackupName.ToolTip = "No backup selected."
            BackupType.Text = "N/A"
            BackupType.ToolTip = "No backup selected."
        End If
    End Sub

    Public Sub LoadLanguage()
        Try
            Main.BackupButton.Content = MCBackup.Language.Dictionnary("MainWindow.BackupButton.Content")
            Main.RestoreButton.Content = MCBackup.Language.Dictionnary("MainWindow.RestoreButton.Content")
            Main.DeleteButton.Content = MCBackup.Language.Dictionnary("MainWindow.DeleteButton.Content")
            Main.RenameButton.Content = MCBackup.Language.Dictionnary("MainWindow.RenameButton.Content")

            Main.AutomaticBackupButton.Content = MCBackup.Language.Dictionnary("MainWindow.AutomaticBackupButton.Content") & " >>"

            Main.ListViewGridView.Columns(0).Header = MCBackup.Language.Dictionnary("MainWindow.ListView.Columns(0).Header")
            Main.ListViewGridView.Columns(1).Header = MCBackup.Language.Dictionnary("MainWindow.ListView.Columns(1).Header")
            Main.ListViewGridView.Columns(2).Header = MCBackup.Language.Dictionnary("MainWindow.ListView.Columns(2).Header")
            Main.OriginalNameLabel.Text = MCBackup.Language.Dictionnary("MainWindow.OriginalNameLabel.Text") & ":"
            Main.TypeLabel.Text = MCBackup.Language.Dictionnary("MainWindow.TypeLabel.Text") & ":"

            Main.EditToolbarButton.Content = MCBackup.Language.Dictionnary("MainWindow.MenuBar.Items(1).Header")
            Main.EditContextMenu.Items(0).Header = MCBackup.Language.Dictionnary("MainWindow.MenuBar.Items(1).Items(0).Header")
            Main.EditContextMenu.Items(1).Header = MCBackup.Language.Dictionnary("MainWindow.MenuBar.Items(1).Items(1).Header")
            Main.ToolsToolbarButton.Content = MCBackup.Language.Dictionnary("MainWindow.MenuBar.Items(2).Header")
            Main.ToolsContextMenu.Items(0).Header = MCBackup.Language.Dictionnary("MainWindow.MenuBar.Items(2).Items(0).Header")
            Main.HelpToolbarButton.Content = MCBackup.Language.Dictionnary("MainWindow.MenuBar.Items(3).Header")
            Main.HelpContextMenu.Items(0).Header = MCBackup.Language.Dictionnary("MainWindow.MenuBar.Items(3).Items(0).Header")
            Main.HelpContextMenu.Items(2).Header = MCBackup.Language.Dictionnary("MainWindow.MenuBar.Items(3).Items(2).Header")
            Main.HelpContextMenu.Items(3).Header = MCBackup.Language.Dictionnary("MainWindow.MenuBar.Items(3).Items(3).Header")


            Main.StatusLabel.Content = MCBackup.Language.Dictionnary("Status.Ready")
        Catch
        End Try
    End Sub
#End Region

#Region "Backup"
    Private Delegate Sub UpdateProgressBarDelegate(ByVal dp As System.Windows.DependencyProperty, ByVal value As Object)

    Private Sub BackupButton_Click(sender As Object, e As EventArgs) Handles BackupButton.Click
        Dim BackupWindow As New Backup
        BackupWindow.Owner = Me
        BackupWindow.ShowDialog()
    End Sub

    Public Sub StartBackup()
        Log.Print("Starting new backup <name=""" & BackupInfo(0) & """; description=""" & BackupInfo(1) & """; path=""" & BackupInfo(2) & """; type=""" & BackupInfo(3) & """;>")
        ListView.IsEnabled = False
        BackupButton.IsEnabled = False
        RestoreButton.IsEnabled = False
        DeleteButton.IsEnabled = False
        RenameButton.IsEnabled = False
        BackupBackgroundWorker.RunWorkerAsync()
        UpdateBackupProgress()
    End Sub

    Private Sub BackupBackgroundWorker_DoWork(sender As Object, e As DoWorkEventArgs)
        Try
            My.Computer.FileSystem.CopyDirectory(BackupInfo(2), My.Settings.BackupsFolderLocation & "\" & BackupInfo(0), True) ' Copy selected save/version/everything to backups folder
            Using SW As New StreamWriter(My.Settings.BackupsFolderLocation & "\" & BackupInfo(0) & "\info.mcb") ' Create information fie (stores description and type)
                SW.WriteLine("baseFolderName=" & BackupInfo(2).Split("\").Last) ' Write save/version folder name
                SW.WriteLine("type=" & BackupInfo(3)) ' Write type in file
                SW.Write("desc=" & BackupInfo(1)) ' Write description if file
            End Using
        Catch ex As Exception
            ErrorWindow.Show(MCBackup.Language.Dictionnary("Message.Caption.Error"), MCBackup.Language.Dictionnary("Message.BackupError"), ex)
            If My.Settings.ShowBalloonTips Then NotifyIcon.ShowBalloonTip(2000, MCBackup.Language.Dictionnary("BalloonTip.Title.BackupError"), MCBackup.Language.Dictionnary("BalloonTip.BackupError"), System.Windows.Forms.ToolTipIcon.Error)
            Log.Print(ex.Message, Log.Type.Severe)
        End Try
    End Sub

    Private Sub UpdateBackupProgress()
        My.Computer.FileSystem.CreateDirectory(My.Settings.BackupsFolderLocation & "\" & BackupInfo(0))
        Dim PercentComplete As Double = 0

        Dim UpdateProgressBarDelegate As New UpdateProgressBarDelegate(AddressOf ProgressBar.SetValue)

        Do Until Int(PercentComplete) = 100
            PercentComplete = Int(GetFolderSize(My.Settings.BackupsFolderLocation & "\" & BackupInfo(0)) / GetFolderSize(BackupInfo(2)) * 100)
            StatusLabel.Content = String.Format(MCBackup.Language.Dictionnary("Status.BackingUp"), Math.Round(PercentComplete, 2))
            Dispatcher.Invoke(UpdateProgressBarDelegate, System.Windows.Threading.DispatcherPriority.Background, New Object() {ProgressBar.ValueProperty, PercentComplete})
        Loop
    End Sub

    Private Sub BackupBackgroundWorker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs)
        ProgressBar.Value = 100
        If BackupInfo(3) = "save" And My.Settings.CreateThumbOnWorld Then
            StatusLabel.Content = String.Format(MCBackup.Language.Dictionnary("Status.CreatingThumb"), "0")
            Log.Print("Creating thumbnail")
            CreateThumb(BackupInfo(2))
        Else
            RefreshBackupsList()
            If My.Settings.ShowBalloonTips Then NotifyIcon.ShowBalloonTip(2000, MCBackup.Language.Dictionnary("BalloonTip.Title.BackupComplete"), MCBackup.Language.Dictionnary("BalloonTip.BackupComplete"), System.Windows.Forms.ToolTipIcon.Info)
            StatusLabel.Content = MCBackup.Language.Dictionnary("Status.BackupComplete")
            Log.Print("Backup Complete")
            ListView.IsEnabled = True
            BackupButton.IsEnabled = True
        End If
    End Sub

    Private WorldPath As String = ""

    Private Sub CreateThumb(Path As String)
        'ProgressBar.IsIndeterminate = True
        StatusLabel.Content = "Creating thumbnail, please wait..."
        ThumbnailBackgroundWorker.RunWorkerAsync()
        WorldPath = Path
    End Sub

    Private MCMap As New Process()

    Private Sub ThumbnailBackgroundWorker_DoWork()
        Try
            With MCMap.StartInfo
                .FileName = Chr(34) & StartupPath & "\mcmap\mcmap.exe" & Chr(34)
                .WorkingDirectory = StartupPath & "\mcmap\"
                .Arguments = " -from -15 -15 -to 15 15 -file """ & My.Settings.BackupsFolderLocation & "\" & BackupInfo(0) & "\thumb.png"" " & """" & WorldPath & """"
                .CreateNoWindow = True
                .UseShellExecute = False
                .RedirectStandardError = True
                .RedirectStandardOutput = True
            End With

            AddHandler MCMap.OutputDataReceived, AddressOf MCMap_OutputDataReceived
            AddHandler MCMap.ErrorDataReceived, AddressOf MCMap_ErrorDataReceived

            With MCMap
                .Start()
                .BeginOutputReadLine()
                .BeginErrorReadLine()
                .WaitForExit()
            End With
        Catch ex As Exception
            Log.Print(ex.Message, Log.Type.Severe)
        End Try
    End Sub

    Private Sub MCMap_OutputDataReceived(sender As Object, e As DataReceivedEventArgs)
        MCMap_DataReceived(sender, e)
    End Sub

    Private Sub MCMap_ErrorDataReceived(sender As Object, e As DataReceivedEventArgs)
        MCMap_DataReceived(sender, e)
    End Sub

    Private StepNumber As Integer
    Private Sub MCMap_DataReceived(sender As Object, e As DataReceivedEventArgs)
        If e.Data = Nothing Then
            Exit Sub
        End If

        Select Case e.Data
            Case "Loading all chunks.."
                StepNumber = 0
            Case "Optimizing terrain..."
                StepNumber = 1
            Case "Drawing map..."
                StepNumber = 2
            Case "Writing to file..."
                StepNumber = 3
        End Select

        If e.Data.Contains("[") And e.Data.Contains("]") Then
            Dim PercentComplete As Double = (Val(e.Data.Substring(2).Remove(1)) / 4) + (StepNumber * 25)
            UpdateProgress(PercentComplete)
            StatusLabel_Content(String.Format(MCBackup.Language.Dictionnary("Status.CreatingThumb"), Int(PercentComplete)))
        End If
    End Sub

    Private Sub ThumbnailBackgroundWorker_RunWorkerCompleted()
        UpdateProgress(100)
        RefreshBackupsList()
        StatusLabel.Content = MCBackup.Language.Dictionnary("Status.BackupComplete")
        If My.Settings.ShowBalloonTips Then NotifyIcon.ShowBalloonTip(2000, MCBackup.Language.Dictionnary("BalloonTip.Title.BackupComplete"), MCBackup.Language.Dictionnary("BalloonTip.BackupComplete"), System.Windows.Forms.ToolTipIcon.Info)
        Log.Print("Backup Complete")
        ListView.IsEnabled = True
        BackupButton.IsEnabled = True
    End Sub
#End Region

#Region "Restore"
    Private Sub RestoreButton_Click(sender As Object, e As EventArgs) Handles RestoreButton.Click
        If MetroMessageBox.Show(MCBackup.Language.Dictionnary("Message.RestoreAreYouSure"), MCBackup.Language.Dictionnary("Message.Caption.AreYouSure"), MessageBoxButton.YesNo, MessageBoxImage.Question) = Forms.DialogResult.Yes Then
            Log.Print("Starting Restore")
            RestoreInfo(0) = ListView.SelectedItems(0).Name ' Set place 0 of RestoreInfo array to the backup name

            Dim BaseFolderName As String = ""

            Using SR As New StreamReader(My.Settings.BackupsFolderLocation & "\" & RestoreInfo(0) & "\info.mcb")
                Do While SR.Peek <> -1
                    Dim Line As String = SR.ReadLine
                    If Not Line.StartsWith("#") Then
                        If Line.StartsWith("baseFolderName=") Then
                            BaseFolderName = Line.Substring(15)
                        ElseIf Line.StartsWith("type=") Then
                            RestoreInfo(2) = Line.Substring(5)
                        End If
                    End If
                Loop
            End Using

            Select Case RestoreInfo(2)
                Case "save"
                    RestoreInfo(1) = My.Settings.MinecraftFolderLocation & "\saves\" & BaseFolderName
                Case "version"
                    RestoreInfo(1) = My.Settings.MinecraftFolderLocation & "\versions\" & BaseFolderName
                Case "everything"
                    RestoreInfo(1) = My.Settings.MinecraftFolderLocation
            End Select

            DeleteForRestoreBackgroundWorker.RunWorkerAsync()
            ProgressBar.IsIndeterminate = True
            StatusLabel.Content = MCBackup.Language.Dictionnary("Status.RemovingOldContent")
            Log.Print("Removing old content")
        Else
            Exit Sub
        End If
    End Sub

    Private Sub DeleteForRestoreBackgroundWorker_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs)
        If My.Computer.FileSystem.DirectoryExists(RestoreInfo(1)) Then
            Try
                My.Computer.FileSystem.DeleteDirectory(RestoreInfo(1), FileIO.DeleteDirectoryOption.DeleteAllContents, FileIO.RecycleOption.SendToRecycleBin)
            Catch ex As Exception
                Log.Print(ex.Message, Log.Type.Severe)
            End Try
        End If
    End Sub

    Private Sub DeleteForRestoreBackgroundWorker_RunWorkerCompleted(sender As Object, e As System.ComponentModel.RunWorkerCompletedEventArgs)
        ProgressBar.IsIndeterminate = False
        RestoreBackgroundWorker.RunWorkerAsync()
        UpdateRestoreProgress()
        Log.Print("Removed old content, restoring...")
    End Sub

    Private Sub RestoreBackgroundWorker_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs)
        Try
            My.Computer.FileSystem.CopyDirectory(My.Settings.BackupsFolderLocation & "\" & RestoreInfo(0), RestoreInfo(1), True)
            My.Computer.FileSystem.DeleteFile(RestoreInfo(1) & "\info.mcb")
            If My.Computer.FileSystem.FileExists(RestoreInfo(1) & "\thumb.png") Then
                My.Computer.FileSystem.DeleteFile(RestoreInfo(1) & "\thumb.png")
            End If
        Catch ex As Exception
            ErrorWindow.Show(MCBackup.Language.Dictionnary("Message.Caption.Error"), MCBackup.Language.Dictionnary("Message.RestoreError"), ex)
            If My.Settings.ShowBalloonTips Then NotifyIcon.ShowBalloonTip(2000, MCBackup.Language.Dictionnary("BalloonTip.Title.RestoreError"), MCBackup.Language.Dictionnary("BalloonTip.RestoreError"), System.Windows.Forms.ToolTipIcon.Error)
            Log.Print(ex.Message, Log.Type.Severe)
        End Try
    End Sub

    Private Sub UpdateRestoreProgress()
        Dim PercentComplete As Integer = 0

        Dim UpdateRestoreProgressBarDelegate As New UpdateProgressBarDelegate(AddressOf ProgressBar.SetValue)

        Do Until PercentComplete = 100
            If My.Computer.FileSystem.DirectoryExists(RestoreInfo(1)) Then
                PercentComplete = GetFolderSize(RestoreInfo(1)) / GetFolderSize(My.Settings.BackupsFolderLocation & "\" & RestoreInfo(0)) * 100
                StatusLabel.Content = String.Format(MCBackup.Language.Dictionnary("Status.Restoring"), PercentComplete)
                Dispatcher.Invoke(UpdateRestoreProgressBarDelegate, System.Windows.Threading.DispatcherPriority.Background, New Object() {ProgressBar.ValueProperty, Convert.ToDouble(PercentComplete)})
            End If
        Loop
    End Sub

    Private Sub RestoreBackgroundWorker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs)
        StatusLabel.Content = MCBackup.Language.Dictionnary("Status.RestoreComplete")
        If My.Settings.ShowBalloonTips Then NotifyIcon.ShowBalloonTip(2000, MCBackup.Language.Dictionnary("BalloonTip.Title.RestoreComplete"), MCBackup.Language.Dictionnary("BalloonTip.RestoreComplete"), System.Windows.Forms.ToolTipIcon.Info)
        Log.Print("Restore Complete")
        RefreshBackupsList()
    End Sub
#End Region

#Region "Functions"
    Private Function GetFolderSize(FolderPath As String)
        Try
            Dim FSO As FileSystemObject = New FileSystemObject
            Return FSO.GetFolder(FolderPath).Size ' Get FolderPath's size
        Catch ex As Exception
            Log.Print("Could not find " & FolderPath & "'s size: " & ex.Message, Log.Type.Severe)
        End Try
        Return 0
    End Function

    Private Function GetFolderDateCreated(FolderPath As String)
        Try
            Dim FSO As FileSystemObject = New FileSystemObject
            Return FSO.GetFolder(FolderPath).DateCreated ' Get FolderPath's date of creation
        Catch ex As Exception
            Log.Print("Could not find " & FolderPath & "'s creation date: " & ex.Message, Log.Type.Severe)
        End Try
        Return 0
    End Function

    Public Shared Function BitmapToBitmapSource(bitmap As System.Drawing.Bitmap) As BitmapSource
        Return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions())
    End Function

    Public Function BitmapFromUri(Source As Uri) As ImageSource
        Try
            Dim Bitmap = New BitmapImage()
            Bitmap.BeginInit()
            Bitmap.UriSource = Source
            Bitmap.CacheOption = BitmapCacheOption.OnLoad
            Bitmap.EndInit()
            Return Bitmap
        Catch ex As Exception
            Log.Print(ex.Message)
        End Try
        Return Nothing
    End Function

    Private Sub UpdateProgress(Value As Double)
        Dim UpdateProgressBarDelegate As New UpdateProgressBarDelegate(AddressOf ProgressBar.SetValue)
        Dispatcher.Invoke(UpdateProgressBarDelegate, System.Windows.Threading.DispatcherPriority.Background, New Object() {ProgressBar.ValueProperty, Value})
    End Sub

    Private Sub StatusLabel_Content(Text As String)
        If StatusLabel.Dispatcher.CheckAccess() Then
            StatusLabel.Content = Text
        Else
            StatusLabel.Dispatcher.Invoke(Sub() StatusLabel_Content(Text))
        End If
    End Sub
#End Region

#Region "Menu Bar"
    Private Sub ExitMenuItem_Click(sender As Object, e As RoutedEventArgs)
        Me.Close()
    End Sub

    Private Sub OptionsMenuItem_Click(sender As Object, e As RoutedEventArgs)
        Dim OptionsWindow As New Options
        OptionsWindow.Owner = Me
        OptionsWindow.ShowDialog()
    End Sub

    Private Sub BackupsFolderMenuItem_Click(sender As Object, e As RoutedEventArgs)
        Process.Start(My.Settings.BackupsFolderLocation)
    End Sub

    Private Sub WebsiteMenuItem_Click(sender As Object, e As RoutedEventArgs)
        Process.Start("http://www.nicoco007.com/minecraft/applications/mcbackup-3")
    End Sub

    Private Sub AboutMenuItem_Click(sender As Object, e As RoutedEventArgs)
        Dim AboutWindow As New About
        AboutWindow.Owner = Me
        AboutWindow.ShowDialog()
    End Sub

    Private Sub ReportBugMenuItem_Click(sender As Object, e As RoutedEventArgs)
        Process.Start("http://bugtracker.nicoco007.com/index.php?do=newtask&project=2")
    End Sub

    Private Sub RefreshBackupsList_Click(sender As Object, e As RoutedEventArgs)
        RefreshBackupsList()
    End Sub
#End Region

#Region "Delete"
    Private ListViewItems As New ArrayList

    Private Sub DeleteButton_Click(sender As Object, e As EventArgs) Handles DeleteButton.Click
        If MetroMessageBox.Show(MCBackup.Language.Dictionnary("Message.DeleteAreYouSure"), MCBackup.Language.Dictionnary("Message.Caption.AreYouSure"), MessageBoxButton.YesNo, MessageBoxImage.Question) = Windows.Forms.DialogResult.Yes Then
            ListViewItems.Clear()
            For Each Item In ListView.SelectedItems
                ListViewItems.Add(Item.Name)
            Next
            ListView.SelectedIndex = -1
            DeleteBackgroundWorker.RunWorkerAsync()
            StatusLabel.Content = MCBackup.Language.Dictionnary("Status.Deleting")
            ProgressBar.IsIndeterminate = True
        End If
    End Sub

    Private Sub DeleteBackgroundWorker_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs)
        For Each Item As String In ListViewItems
            Try
                My.Computer.FileSystem.DeleteDirectory(My.Settings.BackupsFolderLocation & "\" & Item, FileIO.DeleteDirectoryOption.DeleteAllContents)
            Catch ex As Exception
                ErrorWindow.Show(MCBackup.Language.Dictionnary("Message.Caption.Error"), MCBackup.Language.Dictionnary("Message.DeleteError"), ex)
            End Try
        Next
    End Sub

    Private Sub DeleteBackgroundWorker_RunWorkerCompleted(sender As Object, e As System.ComponentModel.RunWorkerCompletedEventArgs)
        StatusLabel.Content = MCBackup.Language.Dictionnary("Status.DeleteComplete")
        ProgressBar.IsIndeterminate = False
        RefreshBackupsList()
    End Sub
#End Region

#Region "Buttons"
    Private Sub RenameButton_Click(sender As Object, e As EventArgs) Handles RenameButton.Click
        Dim RenameWindow As New Rename
        RenameWindow.Owner = Me
        RenameWindow.ShowDialog()
    End Sub
#End Region

#Region "Automatic Backup"
    Public IsMoving As Boolean

    Public Sub AutomaticBackupButton_Click(sender As Object, e As RoutedEventArgs) Handles AutomaticBackupButton.Click
        AutoBackupWindow.Owner = Me
        If AutoBackupWindow.IsVisible Then
            AutoBackupWindow.Hide()
            Me.Left = Me.Left + (AutoBackupWindow.Width / 2)
            AutomaticBackupButton.Content = MCBackup.Language.Dictionnary("MainWindow.AutomaticBackupButton.Content") & " >>"
        Else
            AutoBackupWindow.Show()
            Me.Left = Me.Left - (AutoBackupWindow.Width / 2)
            AutomaticBackupButton.Content = MCBackup.Language.Dictionnary("MainWindow.AutomaticBackupButton.Content") & " <<"
        End If
    End Sub

    Private Sub Window_Activated(sender As Object, e As EventArgs)
        AutoBackupWindow.Focus()
        Me.Focus()
    End Sub

    Private Sub Main_SizeChanged(sender As Object, e As SizeChangedEventArgs) Handles Main.SizeChanged
        Window_LocationChanged(sender, e)
    End Sub

    Private Sub Window_LocationChanged(sender As Object, e As EventArgs)
        If Not AutoBackupWindow.IsMoving Then
            IsMoving = True
            AutoBackupWindow.Left = Me.Left + (Me.Width + 5)
            AutoBackupWindow.Top = Me.Top
            IsMoving = False
        End If
    End Sub
#End Region

#Region "Tray Icon"
    Private Sub ExitToolbarMenuItem_Click(sender As Object, e As EventArgs)
        Me.ClsType = CloseType.ForceClose
        Me.Close()
    End Sub

    Private Sub NotifyIcon_DoubleClick(sender As Object, e As EventArgs) Handles NotifyIcon.DoubleClick, NotifyIcon.BalloonTipClicked
        Me.Show()
        Me.Activate()
        If AutoBackupWindow.IsVisible Then
            AutoBackupWindow.Show()
            AutoBackupWindow.Activate()
        End If
    End Sub
#End Region

#Region "Close to Tray"
    Public ClsType As CloseType

    Private Sub Window_Closing(sender As Object, e As CancelEventArgs)
        If Not ClsType = CloseType.ForceClose Then
            Dim CloseToTrayWindow As New CloseToTray
            CloseToTrayWindow.Owner = Me
            If My.Settings.SaveCloseState Then
                If My.Settings.CloseToTray Then
                    ClsType = CloseType.CloseToTray
                Else
                    ClsType = CloseType.CloseCompletely
                End If
            Else
                CloseToTrayWindow.ShowDialog()
            End If

            Select Case ClsType
                Case CloseType.CloseToTray
                    e.Cancel = True
                    Me.Hide()
                    NotifyIcon.ShowBalloonTip(2000, MCBackup.Language.Dictionnary("BalloonTip.Title.RunningBackground"), MCBackup.Language.Dictionnary("BalloonTip.RunningBackground"), System.Windows.Forms.ToolTipIcon.Info)
                    Log.Print("Closing to tray")
                    Exit Sub
                Case CloseType.CloseCompletely
                    Exit Select
                Case CloseType.Cancel
                    e.Cancel = True
                    Exit Sub
            End Select
        End If

        Try
            If Process.GetProcessesByName("mcmap").Count > 0 Then
                Log.Print("Killing Cartograph Process")
                MCMap.Kill()
            End If
        Catch
        End Try

        Log.Print("Someone is closing me!")
        My.Settings.Save()
    End Sub
#End Region

    Private Sub EditToolbarButton_Click(sender As Object, e As RoutedEventArgs) Handles EditToolbarButton.Click
        EditContextMenu.PlacementTarget = EditToolbarButton
        EditContextMenu.Placement = Primitives.PlacementMode.Bottom
        EditContextMenu.IsOpen = True
    End Sub

    Private Sub ToolsToolbarButton_Click(sender As Object, e As RoutedEventArgs) Handles ToolsToolbarButton.Click
        ToolsContextMenu.PlacementTarget = ToolsToolbarButton
        ToolsContextMenu.Placement = Primitives.PlacementMode.Bottom
        ToolsContextMenu.IsOpen = True
    End Sub

    Private Sub HelpToolbarButton_Click(sender As Object, e As RoutedEventArgs) Handles HelpToolbarButton.Click
        HelpContextMenu.PlacementTarget = HelpToolbarButton
        HelpContextMenu.Placement = Primitives.PlacementMode.Bottom
        HelpContextMenu.IsOpen = True
    End Sub
End Class

Public Class CloseAction
    Public Enum CloseType As Integer
        CloseToTray
        CloseCompletely
        Cancel
        ForceClose
    End Enum
End Class

Public Class ListViewBackupItem
    Private m_Name As String
    Public Property Name() As String
        Get
            Return m_Name
        End Get
        Set(value As String)
            m_Name = value
        End Set
    End Property

    Private m_DateCreated As String
    Public Property DateCreated() As String
        Get
            Return m_DateCreated
        End Get
        Set(value As String)
            m_DateCreated = value
        End Set
    End Property

    Private m_Description As String
    Public Property Description() As String
        Get
            Return m_Description
        End Get
        Set(value As String)
            m_Description = value
        End Set
    End Property

    Private m_Color As SolidColorBrush
    Public Property Color() As SolidColorBrush
        Get
            Return m_Color
        End Get
        Set(value As SolidColorBrush)
            m_Color = value
        End Set
    End Property

    Public Sub New(Name As String, DateCreated As String, Description As String, Color As SolidColorBrush)
        Me.Name = Name
        Me.DateCreated = DateCreated
        Me.Description = Description
        Me.Color = Color
    End Sub
End Class