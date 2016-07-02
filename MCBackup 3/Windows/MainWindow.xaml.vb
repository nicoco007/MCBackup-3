'   ╔═══════════════════════════════════════════════════════════════════════════╗
'   ║                      Copyright © 2013-2016 nicoco007                      ║
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

Imports System.ComponentModel
Imports System.Configuration
Imports System.Globalization
Imports System.IO
Imports System.Net
Imports System.Threading
Imports System.Windows.Threading
Imports MahApps.Metro
Imports MahApps.Metro.Controls
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports Substrate.Nbt
Imports Substrate.Core

Partial Class MainWindow
    Inherits MetroWindow

#Region "Variables"
    Private AppData As String = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)

    'Public BackupInfo As New BackupInfo
    Public RestoreInfo As New RestoreInfo

    Private DeleteThread As Thread

    Private FolderBrowserDialog As New Forms.FolderBrowserDialog
    Public StartupPath As String = Directory.GetCurrentDirectory()
    Public Shared ApplicationVersion As Version = Reflection.Assembly.GetExecutingAssembly().GetName().Version
    Public Shared LatestVersion As Version

    Public WithEvents NotifyIcon As New Forms.NotifyIcon

    Public AutoBackupWindow As New AutoBackupWindow

    Public AutoBackupWindowWasShown As Boolean = False

    Public BackgroundImageBitmap As BitmapImage
#End Region

#Region "Load"
    Public Sub New()

        InitializeComponent()

        AddHandler BackupManager.BackupStarted, AddressOf BackupManager_BackupStarted
        AddHandler BackupManager.BackupProgressChanged, AddressOf BackupManager_BackupProgressChanged
        AddHandler BackupManager.BackupCompleted, AddressOf BackupManager_BackupCompleted
        'AddHandler BackupManager.RestoreStarted, AddressOf BackupManager_RestoreStarted
        AddHandler BackupManager.RestoreProgressChanged, AddressOf BackupManager_RestoreProgressChanged
        AddHandler BackupManager.RestoreCompleted, AddressOf BackupManager_RestoreCompleted

    End Sub

    Public Sub Window_Loaded() Handles MyBase.Loaded

        ' Call theme updater
        UpdateTheme()

        ' Show splash
        Dim Splash As New Splash()
        Splash.Show()

        ' Print system relevant information to log
        Log.Print("")
        Log.Print("---------- Starting MCBackup v{0} @ {1} ----------", ApplicationVersion, Log.DebugTimeStamp())
        Log.Info("OS Name: " & Log.GetWindowsName())
        Log.Info("OS Version: " & Log.GetWindowsVersion())
        Log.Info("Architecture: " & Log.GetWindowsArch())

        ' Check if language is set if and language file exists
        If String.IsNullOrEmpty(My.Settings.Language) Or Not File.Exists(My.Settings.Language + ".mo") Then

            ' Set default language variable according to system language
            Select Case CultureInfo.CurrentCulture.ThreeLetterISOLanguageName

                Case "eng"

                    My.Settings.Language = "en_US"

                Case "fra"

                    My.Settings.Language = "fr_FR"

                Case Else

                    My.Settings.Language = "en_US"

            End Select

        End If

        ' Load language
        Try
            Application.Language.Load(Path.Combine(Directory.GetCurrentDirectory(), "language", My.Settings.Language + ".mo"))
        Catch ex As Exception
            ErrorReportDialog.ShowDialog("Unable to load language file", ex)
            My.Settings.Language = "en_US"
            Application.Language.Clear()
        End Try

        ' Add step to splash progress
        Splash.StepProgress()

        ' Find configuration file
        Dim configurationFile As String = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath

        Log.Debug("Configuration file: " + configurationFile)

        ' Get configuration directory from configuration file
        Dim configurationDirectory As DirectoryInfo = New FileInfo(configurationFile).Directory.Parent

        ' Check if upgrade is necessary and if configuration directory exists
        If My.Settings.CallUpgrade And configurationDirectory.Exists() Then

            Log.Debug("[CONFIGURATION] Configuration directory found! " + configurationDirectory.FullName)

            ' Iterate through all version directories in configuration directory
            For Each versionDirectory As DirectoryInfo In configurationDirectory.GetDirectories()

                Log.Verbose("[CONFIGURATION] Found version " + versionDirectory.Name)

                ' Check if configuration file exists for version
                If File.Exists(Path.Combine(versionDirectory.FullName, "user.config")) Then

                    Log.Info("[CONFIGURATION] Previous configuration (version {0}) found! Prompting user to upgrade settings.", versionDirectory.Name)

                    ' Prompt user to upgrade settings
                    If MetroMessageBox.Show(Application.Language.GetString("An older version of MCBackup has been detected. Would you like to migrate your settings?"), Application.Language.GetString("Migrate settings?"), MessageBoxButton.YesNo) = MessageBoxResult.Yes Then

                        Log.Info("[CONFIGURATION] Upgrading settings!")

                        ' Upgrade settings
                        My.Settings.Upgrade()

                        ' Call theme updater
                        UpdateTheme()

                        ' Reload language
                        Try
                            Application.Language.Load(Path.Combine(Directory.GetCurrentDirectory(), "language", My.Settings.Language + ".mo"))
                        Catch
                            Try
                                My.Settings.Language = "en_US"
                                Application.Language.Load(Path.Combine(Directory.GetCurrentDirectory(), "language", My.Settings.Language + ".mo"))
                            Catch ex As Exception
                                ErrorReportDialog.ShowDialog("Unable to load language file", ex)
                            End Try
                        End Try

                    End If

                    Log.Info("[CONFIGURATION] Settings upgrade skipped.")

                    ' Set callupgrade to false, so user is not prompted again
                    My.Settings.CallUpgrade = False

                    ' Exit for loop to prevent more dialogs from being shown
                    Exit For

                End If

            Next

        End If

        ' Set window size
        Me.Width = My.Settings.WindowSize.Width
        Me.Height = My.Settings.WindowSize.Height

        ' Add step to splash progress
        Splash.StepProgress()

        ' Set default backup names according to langauge if they are not set
        If String.IsNullOrEmpty(My.Settings.DefaultBackupName) Then My.Settings.DefaultBackupName = Application.Language.GetString("%worldname% - %timestamp:yyyy-MM-dd (hh\hmm\mss\s tt)%")
        If String.IsNullOrEmpty(My.Settings.DefaultAutoBackupName) Then My.Settings.DefaultAutoBackupName = Application.Language.GetString("[AUTO] %worldname% - %timestamp:yyyy-MM-dd (hh\hmm\mss\s tt)%")

        ' Add step to splash progress
        Splash.StepProgress()

        Log.Debug("Attempting to create notification icon...")

        ' Set notification icon text and icon
        NotifyIcon.Text = "MCBackup " + ApplicationVersion.ToString()
        NotifyIcon.Icon = New System.Drawing.Icon(Application.GetResourceStream(New Uri("pack://application:,,,/Resources/icon.ico")).Stream)

        ' Create context menu for notification icon
        Dim contextMenu As New Forms.ContextMenu

        ' Add exit item to notification icon
        contextMenu.MenuItems.Add(New Forms.MenuItem With {.Text = Application.Language.GetString("Exit")})

        ' Set notification icon context menu to created context menu
        NotifyIcon.ContextMenu = contextMenu

        ' Show notification icon
        NotifyIcon.Visible = True

        ' Add step to splash progress
        Splash.StepProgress()

        Log.Debug("Loading appearance settings...")

        ' Check if a background image is set and file exists
        If Not String.IsNullOrEmpty(My.Settings.BackgroundImageLocation) And My.Computer.FileSystem.FileExists(My.Settings.BackgroundImageLocation) Then

            Log.Debug("Attempting to load background image " + My.Settings.BackgroundImageLocation)

            ' Set background image bitmap to saved file
            BackgroundImageBitmap = New BitmapImage(New Uri(My.Settings.BackgroundImageLocation))

            ' Adjust background according to main window size
            AdjustBackground()

        End If

        ' Set status label text color
        StatusLabel.Foreground = New SolidColorBrush(My.Settings.StatusLabelColor)

        ' Set window state
        Me.WindowState = IIf(My.Settings.IsWindowMaximized, WindowState.Maximized, WindowState.Normal)

        ' Set backups list/sidebar width
        ' TODO: Find less hacky way to do this!
        GridSidebarColumn.Width = New GridLength(My.Settings.SidebarWidth.Value, GridUnitType.Star)
        GridListViewColumn.Width = New GridLength(My.Settings.ListViewWidth.Value, GridUnitType.Star)

        ' Add step to splash progress
        Splash.StepProgress()

        Log.Info("Searching for Minecraft directory...")

        ' Check if Minecraft directory exists
        If Not Directory.Exists(My.Settings.MinecraftFolderLocation) Then

            Log.Warn("Minecraft installation directory was not found!")

            ' Check if launcher is default Minecraft and default Minecraft directory exists
            If My.Settings.Launcher = Launcher.Minecraft AndAlso Directory.Exists(Path.Combine(AppData, ".minecraft")) Then

                Log.Warn("Default Minecraft directory found! Minecraft folder location has been reset to default.")

                ' Reset Minecraft location to default Minecraft directory
                My.Settings.MinecraftFolderLocation = Path.Combine(AppData, ".minecraft")
                My.Settings.SavesFolderLocation = Path.Combine(AppData, ".minecraft", "saves")

            Else

                Log.Warn("Launcher is not Minecraft or default Minecraft does not exist - Prompting user to select directory.")

                ' Prompt user to select a new Minecraft directory
                If MetroMessageBox.Show(Application.Language.GetString("MCBackup was unable to find an installation of Minecraft on your computer. Please select your Minecraft folder in the following dialog."), Application.Language.GetString("Error"), MessageBoxButton.OKCancel, MessageBoxImage.Error) = MessageBoxResult.OK Then

                    ' Show directory selection dialog
                    Dim SetMinecraftFolderWindow As New SetMinecraftFolderWindow
                    SetMinecraftFolderWindow.ShowDialog()

                    If String.IsNullOrEmpty(My.Settings.MinecraftFolderLocation) Then

                        ' Close MCBackup
                        Splash.Close()
                        Me.Close()
                        Return

                    End If

                Else

                    ' Close MCBackup
                    Splash.Close()
                    Me.Close()
                    Return

                End If

            End If

        End If

        Log.Info("Minecraft folder location: " + My.Settings.MinecraftFolderLocation)

        ' Check if launcher is default Minecraft
        If My.Settings.Launcher = Launcher.Minecraft Then

            ' Check if saves folder location is empty or doesn't exist
            If String.IsNullOrEmpty(My.Settings.SavesFolderLocation) Or Not Directory.Exists(My.Settings.SavesFolderLocation) Then

                Log.Warn("Saves folder does not exist! Reset to default.")

                ' Reset saves directory
                My.Settings.SavesFolderLocation = Path.Combine(My.Settings.MinecraftFolderLocation, "saves")

                ' TODO: add prompt

            End If

            Log.Info("Saves folder location: " + My.Settings.SavesFolderLocation)

        Else

            ' Set saves directory to nothing if launcher isn't default Minecraft
            My.Settings.SavesFolderLocation = Nothing

        End If

        ' Add step to splash progress
        Splash.StepProgress()

        Log.Info("Searching for backups directory...")

        ' Check if backups directory is set
        If String.IsNullOrEmpty(My.Settings.BackupsFolderLocation) Then

            ' If backups directory is not set, MCBackup has not been launched yet. Set backups directory to default directory.
            My.Settings.BackupsFolderLocation = Path.Combine(Directory.GetCurrentDirectory(), "backups")

            ' Create backups directory
            Directory.CreateDirectory(My.Settings.BackupsFolderLocation)

        ElseIf Not Directory.Exists(My.Settings.BackupsFolderLocation) Then

            Log.Warn("Backups folder not found!")

            ' Tell user backups directory was not found and will be reset
            If MetroMessageBox.Show(Application.Language.GetString("Your backups folder cannot be found. This either means it has been deleted, or it is on a disconnected network drive. Press OK to reset your backups folder. Press Cancel to quit MCBackup.\n\nThe backups directory set is:\n{0}", My.Settings.BackupsFolderLocation), Application.Language.GetString("Error"), MessageBoxButton.OKCancel) = MessageBoxResult.OK Then

                ' Reset backups folder location
                My.Settings.BackupsFolderLocation = Path.Combine(Directory.GetCurrentDirectory(), "backups")

                ' Create backups directory
                Directory.CreateDirectory(My.Settings.BackupsFolderLocation)

                Log.Info("Backups folder location reset.")

            Else

                ' Close MCBackup
                Splash.Close()
                Me.Close()
                Return

            End If

        End If

        Log.Info("Backups folder location: " + My.Settings.BackupsFolderLocation)

        ' Add step to splash progress
        Splash.StepProgress()

        ' Check if user wants to check for updates
        If My.Settings.CheckForUpdates Then

            Log.Info("Checking for an update...")

            ' Get most recent version from official website
            Dim client As New WebClient
            AddHandler client.DownloadStringCompleted, AddressOf DownloadVersionStringCompleted
            client.DownloadStringAsync(New Uri("http://content.nicoco007.com/downloads/mcbackup-3/version"))

        Else

            Log.Info("Update check skipped as per settings.")

        End If

        ' Add step to splash progress
        Splash.StepProgress()

        ' Check if user allows anonymous data collection
        If My.Settings.SendAnonymousData Then

            ' Send +1 to StatCounter
            Dim client As New WebClient
            client.DownloadDataAsync(New Uri("http://c.statcounter.com/10065404/0/6bad5aa6/1/"))

        End If

        ' Add step to splash progress
        Splash.StepProgress()

        Log.Debug("Loading language for main window...")

        ' Load language for main window
        LoadLanguage()

        ' Add step to splash progress
        Splash.StepProgress()

        ' Close splash
        Splash.Close()

        ' Set close action to ask
        Application.CloseAction = Application.AppCloseAction.Ask
    End Sub

    Private Async Sub Window_ContentRendered(sender As Object, e As EventArgs) Handles Window.ContentRendered
        ReloadBackupGroups()
        Await RefreshBackupsList()

        If My.Settings.ShowNewsOnStartup Then
            'Dim newsWindow As New NewsWindow()
            'newsWindow.Owner = Me
            'newsWindow.Width = Me.Width - 100
            'newsWindow.Height = newsWindow.Width / 16 * 9
            'newsWindow.Show()
        End If
    End Sub

    Private Sub DownloadVersionStringCompleted(sender As Object, e As DownloadStringCompletedEventArgs)
        If e.Error Is Nothing Then
            LatestVersion = New Version(e.Result)
            Dim comparison = LatestVersion.CompareTo(ApplicationVersion)
            If comparison > 0 Then
                Log.Info("A new version is available (version " + LatestVersion.ToString() + ")!")
                Dim UpdateDialog As New UpdateDialog()
                UpdateDialog.Owner = Me
                UpdateDialog.Show()
            ElseIf comparison < 0 Then
                Log.Info("MCBackup seems to be running a beta version (version " + ApplicationVersion.ToString() + ")!")
            ElseIf comparison = 0 Then
                Log.Info("MCBackup is up-to-date (version " + ApplicationVersion.ToString() + ").")
            End If
        Else
            Log.Warn("An error occured while trying to retrieve the latest version: " + e.Error.Message)
            LatestVersion = New Version(0, 0, 0, 0)
        End If
    End Sub

    Public Async Function RefreshBackupsList() As Task
        If Not Directory.Exists(My.Settings.BackupsFolderLocation) Then Return

        EnableUI(False)
        Progress.IsIndeterminate = True
        StatusLabel.Content = Application.Language.GetString("Refreshing backups list...")

        SetTitle(Application.Language.GetString("Refreshing backups list..."))

        Dim Items As New List(Of ListViewBackupItem)

        Dim asyncOp As AsyncOperation = AsyncOperationManager.CreateOperation(Nothing)
        Dim callback As New SendOrPostCallback(Sub(arg As Object)

                                                   Dim metadata As BackupMetadata = arg

                                                   Items.Add(New ListViewBackupItem(metadata.Name, metadata.GetDateCreated(), New SolidColorBrush(metadata.GetColor()), metadata.OriginalName, metadata.Type.GetString(), metadata.Launcher))

                                               End Sub)

        Dim group As String = Nothing

        If (GroupsTabControl.Items.Count > 0 And GroupsTabControl.SelectedItem IsNot Nothing) Then
            group = DirectCast(GroupsTabControl.SelectedItem, TaggedTabItem).Tag
        End If

        Await Task.Factory.StartNew(Sub()

                                        For Each Backup As DirectoryInfo In New DirectoryInfo(My.Settings.BackupsFolderLocation).GetDirectories

                                            Dim backupMetadata As New BackupMetadata(Backup.FullName)

                                            If (backupMetadata.Group = group Or group Is Nothing) Then
                                                asyncOp.Post(callback, backupMetadata)
                                            End If

                                        Next

                                    End Sub)

        ListView.ItemsSource = Items
        ListView.SelectedIndex = -1
        SidebarTitle.Text = Application.Language.GetString("{0} elements", Items.Count)

        If ListView.Items.Count = 0 Then
            NoBackupsOverlay.Visibility = Visibility.Visible
        Else
            NoBackupsOverlay.Visibility = Visibility.Collapsed
        End If

        Select Case My.Settings.ListViewGroupBy
            Case BackupsListView.GroupBy.OriginalName
                SetListViewGroupDescriptions("OriginalName")
            Case BackupsListView.GroupBy.Type
                SetListViewGroupDescriptions("Type")
            Case Else
                SetListViewGroupDescriptions(Nothing)
        End Select

        Select Case My.Settings.ListViewSortBy
            Case BackupsListView.SortBy.Name
                ListViewSortByNameItem_Click(Nothing, Nothing)
            Case BackupsListView.SortBy.Type
                ListViewSortByTypeItem_Click(Nothing, Nothing)
            Case Else
                ListViewSortByDateCreatedItem_Click(Nothing, Nothing)
        End Select

        Select Case My.Settings.ListViewSortByDirection
            Case ListSortDirection.Ascending
                ListViewSortAscendingItem_Click(Nothing, Nothing)
            Case ListSortDirection.Descending
                ListViewSortDescendingItem_Click(Nothing, Nothing)
            Case Else
                ListViewSortAscendingItem_Click(Nothing, Nothing)
        End Select
        ListView_SelectionChanged(New Object, New EventArgs)

        Progress.IsIndeterminate = False
        StatusLabel.Content = Application.Language.GetString("Ready")
        ResetTitle()
        EnableUI(True)
    End Function

    Private Sub ListView_SelectionChanged(sender As Object, e As EventArgs) Handles ListView.SelectionChanged
        Select Case ListView.SelectedItems.Count
            Case 0
                RestoreButton.IsEnabled = False
                RenameButton.IsEnabled = False ' Don't allow anything when no items are selected
                DeleteButton.IsEnabled = False

                SidebarTitle.Text = Application.Language.GetString("{0} elements", ListView.Items.Count)        'Show total number of elements
                SidebarTitle.ToolTip = Application.Language.GetString("{0} elements", ListView.Items.Count)

                ListViewRestoreItem.IsEnabled = False
                ListViewDeleteItem.IsEnabled = False         'Disable ContextMenu items
                ListViewRenameItem.IsEnabled = False
                ListViewMoveToGroupItem.IsEnabled = False

                ThumbnailImage.Source = New BitmapImage(New Uri("pack://application:,,,/Resources/nothumb.png"))
                SidebarOriginalNameContent.Text = "-"
                SidebarOriginalNameContent.ToolTip = Application.Language.GetString("No backup selected.")
                SidebarTypeContent.Text = "-"
                SidebarTypeContent.ToolTip = Application.Language.GetString("No backup selected.")

                DescriptionTextBox.Text = Application.Language.GetString("No item selected.")

                SidebarPlayerHealth.Visibility = Visibility.Collapsed
                SidebarPlayerHunger.Visibility = Visibility.Collapsed

                SidebarTypeLabel.Visibility = Visibility.Collapsed
                SidebarTypeContent.Visibility = Visibility.Collapsed
                SidebarOriginalNameLabel.Visibility = Visibility.Collapsed
                SidebarOriginalNameContent.Visibility = Visibility.Collapsed
                SidebarDescriptionLabel.Visibility = Visibility.Collapsed
                DescriptionTextBox.Visibility = Visibility.Collapsed
            Case 1
                Dim Thread As New Thread(AddressOf LoadBackupInfo)
                Thread.Start()
            Case Else
                RestoreButton.IsEnabled = False
                RenameButton.IsEnabled = False ' Only allow deletion if more than 1 item is selected
                DeleteButton.IsEnabled = True

                SidebarTitle.Text = Application.Language.GetString("{0} elements selected", ListView.SelectedItems.Count)   'Set sidebar title to number of selected items
                SidebarTitle.ToolTip = Application.Language.GetString("{0} elements selected", ListView.SelectedItems.Count)

                ListViewRestoreItem.IsEnabled = False
                ListViewDeleteItem.IsEnabled = True
                ListViewRenameItem.IsEnabled = False
                ListViewMoveToGroupItem.IsEnabled = True

                ThumbnailImage.Source = New BitmapImage(New Uri("pack://application:,,,/Resources/nothumb.png"))

                SidebarPlayerHealth.Visibility = Visibility.Collapsed
                SidebarPlayerHunger.Visibility = Visibility.Collapsed

                SidebarTypeLabel.Visibility = Visibility.Collapsed
                SidebarTypeContent.Visibility = Visibility.Collapsed
                SidebarOriginalNameLabel.Visibility = Visibility.Collapsed
                SidebarOriginalNameContent.Visibility = Visibility.Collapsed
                SidebarDescriptionLabel.Visibility = Visibility.Collapsed
                DescriptionTextBox.Visibility = Visibility.Collapsed
        End Select
    End Sub

    Private Sub LoadBackupInfo()
        Dim SelectedItem As ListViewBackupItem = Nothing
        Dispatcher.Invoke(Sub()
                              SelectedItem = ListView.SelectedItem
                              If SelectedItem Is Nothing Then
                                  Exit Sub
                              End If
                              RestoreButton.IsEnabled = True
                              RenameButton.IsEnabled = True ' Allow anything if only 1 item is selected
                              DeleteButton.IsEnabled = True

                              SidebarTitle.Text = SelectedItem.Name     'Set sidebar title to backup name
                              SidebarTitle.ToolTip = SelectedItem.Name

                              ListViewRestoreItem.IsEnabled = True
                              ListViewDeleteItem.IsEnabled = True     'Enable ContextMenu items
                              ListViewRenameItem.IsEnabled = True
                              ListViewMoveToGroupItem.IsEnabled = True

                              SidebarPlayerHealth.Visibility = Visibility.Collapsed
                              SidebarPlayerHunger.Visibility = Visibility.Collapsed

                              SidebarTypeLabel.Visibility = Visibility.Visible
                              SidebarTypeContent.Visibility = Visibility.Visible
                              SidebarOriginalNameLabel.Visibility = Visibility.Visible
                              SidebarOriginalNameContent.Visibility = Visibility.Visible
                              SidebarDescriptionLabel.Visibility = Visibility.Visible
                              DescriptionTextBox.Visibility = Visibility.Visible

                              If My.Computer.FileSystem.FileExists(My.Settings.BackupsFolderLocation & "\" & SelectedItem.Name & "\thumb.png") Then
                                  Try
                                      ThumbnailImage.Source = BitmapFromUri(New Uri(My.Settings.BackupsFolderLocation & "\" & SelectedItem.Name & "\thumb.png"))
                                  Catch ex As Exception
                                      Dispatcher.Invoke(Sub() ErrorReportDialog.ShowDialog("An error occured while trying to load the backup's thumbnail", ex))
                                  End Try
                              Else
                                  Select Case SelectedItem.Launcher
                                      Case Launcher.Minecraft
                                          ThumbnailImage.Source = New BitmapImage(New Uri("pack://application:,,,/Resources/thumb/minecraft.png"))
                                      Case Launcher.Technic
                                          ThumbnailImage.Source = New BitmapImage(New Uri("pack://application:,,,/Resources/thumb/technic.png"))
                                      Case Launcher.FeedTheBeast
                                          ThumbnailImage.Source = New BitmapImage(New Uri("pack://application:,,,/Resources/thumb/ftb.png"))
                                      Case Launcher.ATLauncher
                                          ThumbnailImage.Source = New BitmapImage(New Uri("pack://application:,,,/Resources/thumb/atlauncher.png"))
                                      Case Else
                                          ThumbnailImage.Source = New BitmapImage(New Uri("pack://application:,,,/Resources/nothumb.png"))
                                  End Select
                              End If
                          End Sub)

        Dim Type As BackupType = BackupType.World, OriginalFolderName As String = "-", Description As String = ""

        Try
            Dim InfoJson As JObject
            Using SR As New StreamReader(My.Settings.BackupsFolderLocation & "\" & SelectedItem.Name & "\info.json")
                InfoJson = JsonConvert.DeserializeObject(SR.ReadToEnd)
            End Using
            Type = CInt(InfoJson("Type"))
            OriginalFolderName = InfoJson("OriginalName")
            Description = InfoJson("Description")
        Catch ex As Exception
            Log.Severe("Could not read info.json! " + ex.Message)
        End Try

        Dispatcher.Invoke(Sub()
                              SidebarOriginalNameContent.Text = OriginalFolderName
                              SidebarOriginalNameContent.ToolTip = OriginalFolderName

                              Select Case Type
                                  Case BackupType.World
                                      SidebarTypeContent.Text = Application.Language.GetString("World")
                                      SidebarTypeContent.ToolTip = Application.Language.GetString("World")
                                  Case BackupType.Version
                                      SidebarTypeContent.Text = Application.Language.GetString("Version")
                                      SidebarTypeContent.ToolTip = Application.Language.GetString("Version")
                                  Case BackupType.Full
                                      SidebarTypeContent.Text = Application.Language.GetString("Everything")
                                      SidebarTypeContent.ToolTip = Application.Language.GetString("Everything")
                              End Select

                              DescriptionTextBox.Text = IIf(String.IsNullOrEmpty(Description), Application.Language.GetString("No description."), Description)
                          End Sub)

        If Type = BackupType.World AndAlso SelectedItem IsNot Nothing AndAlso File.Exists(Path.Combine(My.Settings.BackupsFolderLocation, SelectedItem.Name, "level.dat")) Then
            Dispatcher.Invoke(Sub()
                                  SidebarPlayerHealthGrid.Children.Clear()
                                  SidebarPlayerHungerGrid.Children.Clear()

                                  Dim backup As String = Path.Combine(My.Settings.BackupsFolderLocation, SelectedItem.Name, "level.dat")

                                  Log.Info(backup)

                                  Dim tree As NbtTree = If(TryReadNbtFile(backup, CompressionType.GZip), TryReadNbtFile(backup, CompressionType.None))

                                  If (tree.Root IsNot Nothing) Then
                                      Dim dataNode As TagNodeCompound = tree.Root.Item("Data").ToTagCompound()
                                      Dim playerNode As TagNodeCompound = dataNode.Item("Player").ToTagCompound()
                                      Dim healthNode As TagNode = playerNode.Item("Health")
                                      Dim health As Integer

                                      ' Get player health (IIf doesn't seem to work here for some reason)
                                      If (TypeOf healthNode Is TagNodeFloat) Then
                                          health = healthNode.ToTagFloat().Data
                                      Else
                                          health = healthNode.ToTagShort().Data
                                      End If

                                      Dim hunger As Integer = playerNode.Item("foodLevel").ToTagInt().Data

                                      SidebarPlayerHealthGrid.ToolTip = health.ToString() + "/20"
                                      SidebarPlayerHungerGrid.ToolTip = hunger.ToString() + "/20"

                                      For i As Integer = 0 To health \ 2 - 1
                                          SidebarPlayerHealthGrid.Children.Add(New Game.Images.Health(New Thickness(SidebarPlayerHealthGrid.Children.Count * 10, 0, 0, 0), Game.Images.State.Full))
                                      Next
                                      If health Mod 2 <> 0 Then
                                          SidebarPlayerHealthGrid.Children.Add(New Game.Images.Health(New Thickness(SidebarPlayerHealthGrid.Children.Count * 10, 0, 0, 0), Game.Images.State.Half))
                                      End If
                                      For i As Integer = 0 To (20 - health) \ 2 - 1
                                          SidebarPlayerHealthGrid.Children.Add(New Game.Images.Health(New Thickness(SidebarPlayerHealthGrid.Children.Count * 10, 0, 0, 0), Game.Images.State.Empty))
                                      Next

                                      For i As Integer = 0 To hunger \ 2 - 1
                                          SidebarPlayerHungerGrid.Children.Add(New Game.Images.Hunger(New Thickness(90 - SidebarPlayerHungerGrid.Children.Count * 10, 0, 0, 0), Game.Images.State.Full))
                                      Next
                                      If hunger Mod 2 <> 0 Then
                                          SidebarPlayerHungerGrid.Children.Add(New Game.Images.Hunger(New Thickness(90 - SidebarPlayerHungerGrid.Children.Count * 10, 0, 0, 0), Game.Images.State.Half))
                                      End If
                                      For i As Integer = 0 To (20 - hunger) \ 2 - 1
                                          SidebarPlayerHungerGrid.Children.Add(New Game.Images.Hunger(New Thickness(90 - SidebarPlayerHungerGrid.Children.Count * 10, 0, 0, 0), Game.Images.State.Empty))
                                      Next

                                      SidebarPlayerHealth.Visibility = Visibility.Visible
                                      SidebarPlayerHunger.Visibility = Visibility.Visible
                                  End If
                              End Sub)
        Else
            Dispatcher.Invoke(Sub()
                                  SidebarPlayerHealth.Visibility = Visibility.Collapsed
                                  SidebarPlayerHunger.Visibility = Visibility.Collapsed
                              End Sub)
        End If
    End Sub

    Public Function TryReadNbtFile(path As String, compression As CompressionType)
        Dim file As NBTFile = New NBTFile(path)
        Dim tree As NbtTree = New NbtTree()
        tree.ReadFrom(file.GetDataInputStream())

        If (tree.Root Is Nothing) Then
            Return Nothing
        End If

        Return tree
    End Function

    Public Sub LoadLanguage()
        Try
            BackupButton.Content = Application.Language.GetStringWithContext("verb", "Backup")
            RestoreButton.Content = Application.Language.GetString("Restore")
            DeleteButton.Content = Application.Language.GetString("Delete")
            RenameButton.Content = Application.Language.GetString("Rename")
            CullButton.Content = Application.Language.GetString("Selective Deletion")
            ListViewMoveToGroupItem.Header = Application.Language.GetString("Move to Group")
            AutomaticBackupButton.Content = Application.Language.GetString("Automatic Backup") & IIf(AutoBackupWindow.IsVisible, " <<", " >>")

            NameColumnHeader.Content = Application.Language.GetString("Name")
            DateCreatedColumnHeader.Content = Application.Language.GetString("Date Created")
            TypeColumnHeader.Content = Application.Language.GetString("Type")

            SidebarOriginalNameLabel.Text = Application.Language.GetString("Original Name")
            SidebarTypeLabel.Text = Application.Language.GetString("Type")
            SidebarDescriptionLabel.Text = Application.Language.GetString("Description")
            SidebarPlayerHealthLabel.Text = Application.Language.GetString("Player Health")
            SidebarPlayerHungerLabel.Text = Application.Language.GetString("Player Hunger")

            FileToolbarButton.Content = Application.Language.GetString("File")
            FileContextMenu.Items(0).Header = Application.Language.GetString("Quit")
            EditToolbarButton.Content = Application.Language.GetString("Edit")
            EditContextMenu.Items(0).Header = Application.Language.GetString("Refresh Backups List")
            EditContextMenu.Items(1).Header = Application.Language.GetString("Open Backups Folder in Explorer")
            ToolsToolbarButton.Content = Application.Language.GetString("Tools")
            ToolsContextMenu.Items(0).Header = Application.Language.GetString("Preferences")
            HelpToolbarButton.Content = Application.Language.GetString("Help")
            HelpContextMenu.Items(0).Header = Application.Language.GetString("Report a Bug")
            HelpContextMenu.Items(2).Header = Application.Language.GetString("Website")
            HelpContextMenu.Items(3).Header = Application.Language.GetString("About")

            StatusLabel.Content = Application.Language.GetString("Ready")
            ResetTitle()

            SearchTextBox.PlaceholderText = Application.Language.GetString("Search...")

            ListViewSortByItem.Header = Application.Language.GetString("Sort by")
            ListViewSortByNameItem.Header = Application.Language.GetString("Name")
            ListViewSortByDateCreatedItem.Header = Application.Language.GetString("Date Created")
            ListViewSortByTypeItem.Header = Application.Language.GetString("Type")
            ListViewSortAscendingItem.Header = Application.Language.GetString("Ascending")
            ListViewSortDescendingItem.Header = Application.Language.GetString("Descending")

            ListViewGroupByItem.Header = Application.Language.GetString("Group by")
            ListViewGroupByNameItem.Header = Application.Language.GetString("Original Name")
            ListViewGroupByTypeItem.Header = Application.Language.GetString("Type")
            ListViewGroupByNothingItem.Header = Application.Language.GetString("Nothing")

            ListViewRestoreItem.Header = Application.Language.GetString("Restore")
            ListViewDeleteItem.Header = Application.Language.GetString("Delete")
            ListViewRenameItem.Header = Application.Language.GetString("Rename")

            ListViewOpenInExplorerItem.Header = Application.Language.GetString("Open in Explorer")

            NoBackupsOverlay.Text = Application.Language.GetString("There are no backups here. Press 'Backup' to create one!")

            CancelButton.Content = Application.Language.GetString("Cancel")
        Catch ex As Exception
            Dispatcher.Invoke(Sub() ErrorReportDialog.ShowDialog("Could not load language.", ex))
        End Try
    End Sub

    Public Sub ReloadBackupGroups()
        GroupsTabControl.Items.Clear()
        GroupsTabControl.Items.Add(New TaggedTabItem(Application.Language.GetString("All"), Nothing))
        For Each Group As String In My.Settings.BackupGroups
            GroupsTabControl.Items.Add(New TaggedTabItem(Group, Group))
        Next
        GroupsTabControl.SelectedIndex = 0
    End Sub
#End Region

#Region "Backup"
    Private Sub BackupButton_Click(sender As Object, e As EventArgs) Handles BackupButton.Click

        ' Create new instance of BackupDialog to prompt user to select what to backup
        Dim BackupDialog As New BackupDialog

        ' Set backup owner to this window, so dialog is centered
        BackupDialog.Owner = Me

        ' Show dialog
        BackupDialog.ShowDialog()

    End Sub

    Private Sub BackupManager_BackupStarted(e As EventArgs)

        ' Disable UI buttons
        EnableUI(False)

    End Sub

    Private Sub BackupManager_BackupProgressChanged(e As BackupProgressChangedEventArgs)
        Progress.Maximum = 100

        ' Report progress depending on status
        Select Case e.Status
            Case BackupStatus.Starting

                Progress.IsIndeterminate = True
                Progress.Value = 0

                StatusLabel.Content = Application.Language.GetString("Starting backup...")

            Case BackupStatus.Running

                ' Set progress style to indeterminate and value to current progress percentage
                Progress.IsIndeterminate = False
                Progress.Value = e.ProgressPercentage

                ' Set status label & window title text to reflect status & progress
                StatusLabel.Content = Application.Language.GetString("Backing up... ({0:0.00}% Complete at {1:0.00} MiB/s, {2})", e.ProgressPercentage, IIf(Single.IsNaN(e.TransferRate), 0, e.TransferRate / 1048576), BackupManager.EstimatedTimeSpanToString(e.EstimatedTimeRemaining))
                SetTitle(Application.Language.GetString("Backup... {0:0.00}%", e.ProgressPercentage))

            Case BackupStatus.RevertingChanges

                ' Set progress style to indeterminate and value to current progress percentage
                Progress.IsIndeterminate = True
                Progress.Value = 0

                ' Set status label & window title text to reflect status
                StatusLabel.Content = Application.Language.GetString("Reverting changes...")
                SetTitle(Application.Language.GetString("Reverting changes..."))

            Case BackupStatus.CreatingThumbnail

                ' Set progress style to indeterminate and value to current progress percentage
                Progress.IsIndeterminate = False
                Progress.Value = e.ProgressPercentage

                ' Set status label & window title text to reflect status & progress
                StatusLabel.Content = Application.Language.GetString("Creating thumbnail... ({0:0.00}% Complete)", e.ProgressPercentage)
                SetTitle(Application.Language.GetString("Creating thumbnail... {0:0.00}%", e.ProgressPercentage))

        End Select
    End Sub

    Private Async Sub BackupManager_BackupCompleted(e As BackupCompletedEventArgs)
        ProgressBar.Value = 100

        ' Check if an error occured
        If e.Error IsNot Nothing Then

            ' Show error balloon tip
            If My.Settings.ShowBalloonTips Then NotifyIcon.ShowBalloonTip(2000, Application.Language.GetString("Backup Error!"), Application.Language.GetString("An error occurred during the backup."), Forms.ToolTipIcon.Error)

            ' Show error report dialog
            ErrorReportDialog.ShowDialog(Application.Language.GetString("An error occurred during the backup."), e.Error)

        Else

            ' Show backup completed balloon tip
            If My.Settings.ShowBalloonTips Then NotifyIcon.ShowBalloonTip(2000, Application.Language.GetString("Backup complete!"), Application.Language.GetString("Your backup has completed."), Forms.ToolTipIcon.Info)

        End If

        ' Send +1 to StatCounter
        If My.Settings.SendAnonymousData Then

            ' Create new webclient
            Dim WebClient As New WebClient

            ' Download web page (and therefore send +1) using webclient
            WebClient.DownloadDataAsync(New Uri("http://c.statcounter.com/9820848/0/90ee98bc/1/"))

        End If

        ' Reload backup groups & refresh backups list
        ReloadBackupGroups()
        Await RefreshBackupsList()

        ' Set status to ready
        StatusLabel.Content = Application.Language.GetString("Ready")
        ResetTitle()

        ' Set progress to 100%
        Progress.Maximum = 100
        Progress.Value = 100

        ' Re-enable UI
        EnableUI(True)
    End Sub
#End Region

#Region "Restore"
    Private RestoreStopWatch As New Stopwatch

    Private Sub RestoreButton_Click(sender As Object, e As EventArgs) Handles RestoreButton.Click, ListViewRestoreItem.Click
        If MetroMessageBox.Show(Application.Language.GetString("Are you sure you want to restore this backup? This will overwrite any existing content!"), Application.Language.GetString("Are you sure?"), MessageBoxButton.YesNo, MessageBoxImage.Question) = Forms.DialogResult.Yes Then
            EnableUI(False)
            'Cancel = False
            Log.Print("Starting Restore")
            RestoreInfo.BackupName = ListView.SelectedItems(0).Name

            ListView.SelectedIndex = -1

            Dim BaseFolderName As String = "",
                Launcher As Launcher = Launcher.Minecraft,
                Modpack As String = "",
                InfoJson As JObject

            If Not File.Exists(My.Settings.BackupsFolderLocation & "\" & RestoreInfo.BackupName & "\info.json") Then
                MetroMessageBox.Show(Application.Language.GetString("Backup information does not exist. Cannot restore."))
                Exit Sub
            End If

            Using SR As New StreamReader(My.Settings.BackupsFolderLocation & "\" & RestoreInfo.BackupName & "\info.json")
                InfoJson = JsonConvert.DeserializeObject(SR.ReadToEnd)
                BaseFolderName = InfoJson("OriginalName")
                RestoreInfo.BackupType = CInt(InfoJson("Type"))

                Dim Temp As Object = InfoJson("Launcher")
                If IsNumeric(Temp) Then
                    If Temp > [Enum].GetValues(GetType(Launcher)).Cast(Of Launcher).Last() Or Temp < 0 Then
                        Launcher = Launcher.Minecraft
                    Else
                        Launcher = Temp
                    End If
                ElseIf Not String.IsNullOrEmpty(Temp) Then

                    Select Case Temp.ToString().ToLower()
                        Case "minecraft"
                            Launcher = Launcher.Minecraft
                        Case "technic"
                            Launcher = Launcher.Technic
                        Case "ftb"
                            Launcher = Launcher.FeedTheBeast
                        Case "feedthebeast"
                            Launcher = Launcher.FeedTheBeast
                        Case "atlauncher"
                            Launcher = Launcher.ATLauncher
                        Case Else
                            Launcher = Launcher.Minecraft
                    End Select
                Else
                    Launcher = Launcher.Minecraft
                End If

                Modpack = InfoJson("Modpack")
            End Using

            If Launcher <> My.Settings.Launcher Then
                MetroMessageBox.Show(Application.Language.GetString("This backup is not compatible with your current configuration! It is designed for {0} installations only.", Launcher.GetString()), Application.Language.GetString("Error"), MessageBoxButton.OK, MessageBoxImage.Error)
                EnableUI(True)
                Exit Sub
            End If

            Select Case RestoreInfo.BackupType
                Case BackupType.World
                    Select Case My.Settings.Launcher
                        Case Launcher.Minecraft
                            RestoreInfo.RestoreLocation = My.Settings.SavesFolderLocation & "\" & BaseFolderName
                        Case Launcher.Technic
                            RestoreInfo.RestoreLocation = My.Settings.MinecraftFolderLocation & "\modpacks\" & Modpack & "\saves\" & BaseFolderName
                        Case Launcher.FeedTheBeast
                            RestoreInfo.RestoreLocation = My.Settings.MinecraftFolderLocation & "\" & Modpack & "\minecraft\saves\" & BaseFolderName
                        Case Launcher.ATLauncher
                            RestoreInfo.RestoreLocation = My.Settings.MinecraftFolderLocation & "\Instances\" & Modpack & "\saves\" & BaseFolderName
                    End Select
                Case BackupType.Version
                    Select Case My.Settings.Launcher
                        Case Launcher.Minecraft
                            RestoreInfo.RestoreLocation = My.Settings.MinecraftFolderLocation & "\versions\" & BaseFolderName
                        Case Launcher.Technic
                            RestoreInfo.RestoreLocation = My.Settings.MinecraftFolderLocation & "\modpacks\" & BaseFolderName
                        Case Launcher.FeedTheBeast
                            RestoreInfo.RestoreLocation = My.Settings.MinecraftFolderLocation & "\" & BaseFolderName
                        Case Launcher.ATLauncher
                            RestoreInfo.RestoreLocation = My.Settings.MinecraftFolderLocation & "\Instances\" & BaseFolderName
                    End Select
                Case BackupType.Full
                    RestoreInfo.RestoreLocation = My.Settings.MinecraftFolderLocation
            End Select

            BackupManager.RestoreAsync(RestoreInfo.BackupName, RestoreInfo.RestoreLocation, RestoreInfo.BackupType)
        End If
    End Sub

    Private Sub BackupManager_RestoreProgressChanged(e As RestoreProgressChangedEventArgs)

        Progress.Maximum = 100

        Select Case e.RestoreStatus

            Case RestoreStatus.Starting

                Progress.IsIndeterminate = True
                Progress.Value = 0

                StatusLabel.Content = Application.Language.GetString("Starting restore...")

            Case RestoreStatus.RemovingOldFiles

                Progress.IsIndeterminate = False
                Progress.Value = e.ProgressPercentage

                StatusLabel.Content = Application.Language.GetString("Removing old content, please wait...", e.ProgressPercentage)
                SetTitle(Application.Language.GetString("Removing old content...", e.ProgressPercentage))

            Case RestoreStatus.Restoring

                Progress.IsIndeterminate = False
                Progress.Value = e.ProgressPercentage

                StatusLabel.Content = Application.Language.GetString("Restoring... ({0:0.00}% Complete at {1:0.00} MiB/s, {2})", e.ProgressPercentage, e.TransferRate / 1048576, BackupManager.EstimatedTimeSpanToString(e.EstimatedTimeRemaining))
                SetTitle(Application.Language.GetString("Restoring... {0:0.00}%", e.ProgressPercentage))

        End Select

    End Sub

    Private Async Sub BackupManager_RestoreCompleted(e As RestoreCompletedEventArgs)

        If e.Error IsNot Nothing Then

            ErrorReportDialog.ShowDialog(e.Error.Message, e.Error)

        End If

        StatusLabel.Content = Application.Language.GetString("Ready")
        ResetTitle()
        Progress.Maximum = 100
        Progress.Value = 100
        EnableUI(True)
        Await RefreshBackupsList()
        ReloadBackupGroups()

    End Sub
#End Region

#Region "Functions"
    Private Function GetFolderSize(FolderPath As String)
        Try
            Dim TotalSize As Long
            For Each File As FileInfo In New DirectoryInfo(FolderPath).GetFiles("*", SearchOption.AllDirectories)
                TotalSize += File.Length
            Next
            Return TotalSize
        Catch ex As DirectoryNotFoundException
            Return 0
        Catch ex As Exception
            Dispatcher.Invoke(Sub() ErrorReportDialog.ShowDialog(String.Format("Could not find size of '{0}'", FolderPath), ex))

            Return 0
        End Try
    End Function

    Public Function GetFolderDateCreated(FolderPath As String) As DateTime
        Try
            Dim FSO As New Scripting.FileSystemObject
            Return FSO.GetFolder(FolderPath).DateCreated ' Get FolderPath's date of creation
        Catch ex As Exception
            Dispatcher.Invoke(Sub() ErrorReportDialog.ShowDialog(String.Format("Could not find creation date of '{0}'", FolderPath), ex))
        End Try
        Return Nothing
    End Function

    Public Shared Function BitmapToBitmapSource(bitmap As System.Drawing.Bitmap) As BitmapSource
        Return Interop.Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions())
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
            Dispatcher.Invoke(Sub() ErrorReportDialog.ShowDialog(String.Format("Could not convert source {0} to bitmap:", Source), ex))
        End Try
        Return Nothing
    End Function
#End Region

#Region "Menu Bar"
    Private Sub ExitMenuItem_Click(sender As Object, e As RoutedEventArgs)
        Application.CloseAction = Application.AppCloseAction.Close
        Me.Close()
    End Sub

    Public Sub OptionsMenuItem_Click(sender As Object, e As RoutedEventArgs)
        Dim OptionsWindow As New OptionsDialog()
        OptionsWindow.Owner = Me
        OptionsWindow.Show()
    End Sub

    Private Sub BackupsFolderMenuItem_Click(sender As Object, e As RoutedEventArgs)
        Process.Start(My.Settings.BackupsFolderLocation)
    End Sub

    Private Sub WebsiteMenuItem_Click(sender As Object, e As RoutedEventArgs)
        Process.Start("http://go.nicoco007.com/fwlink/?LinkID=1000&utm_source=mcbackup&utm_medium=mcbackup")
    End Sub

    Private Sub AboutMenuItem_Click(sender As Object, e As RoutedEventArgs)
        Dim AboutDialog As New AboutDialog
        AboutDialog.Owner = Me
        AboutDialog.ShowDialog()
    End Sub

    Private Sub ReportBugMenuItem_Click(sender As Object, e As RoutedEventArgs)
        Process.Start("http://go.nicoco007.com/fwlink/?LinkID=5000")
    End Sub

    Private Async Sub RefreshBackupsList_Click(sender As Object, e As RoutedEventArgs)
        Await RefreshBackupsList()
    End Sub
#End Region

#Region "Delete"
    Private Sub DeleteButton_Click(sender As Object, e As EventArgs) Handles DeleteButton.Click, ListViewDeleteItem.Click
        If My.Settings.ShowDeleteDialog Then
            If DeleteDialog.Show(Me) = Forms.DialogResult.Yes Then
                Dim ListViewItems As New ArrayList
                For Each Item In ListView.SelectedItems
                    ListViewItems.Add(Item.Name)
                Next
                StartDelete(ListViewItems)
            End If
        Else
            Dim ListViewItems As New ArrayList
            For Each Item In ListView.SelectedItems
                ListViewItems.Add(Item.Name)
            Next
            StartDelete(ListViewItems)
        End If
    End Sub

    Private Sub StartDelete(ItemsToDelete As ArrayList)
        EnableUI(False)
        ListView.SelectedIndex = -1
        Dim TotalSize As Double
        For Each Directory As String In ItemsToDelete
            TotalSize += GetFolderSize(My.Settings.BackupsFolderLocation & "\" & Directory)
        Next
        Progress.Maximum = TotalSize
        Dim t As New Thread(Sub() Delete(ItemsToDelete, TotalSize))
        t.Start()
    End Sub

    Private Sub Delete(ItemsToDelete As ArrayList, TotalSize As Double)
        DeleteThread = New Thread(Sub() DeleteBackgroundWorker_DoWork(ItemsToDelete))
        DeleteThread.Start()

        Dim PercentComplete As Double = 100
        Do Until Int(PercentComplete) <= 0
            Dim CurrentSize As Double = 0
            For Each Directory As String In ItemsToDelete
                If IO.Directory.Exists(Path.Combine(My.Settings.BackupsFolderLocation, Directory)) Then
                    CurrentSize += GetFolderSize(Path.Combine(My.Settings.BackupsFolderLocation, Directory))
                End If
            Next

            PercentComplete = CurrentSize / TotalSize * 100

            Dispatcher.Invoke(Sub()
                                  StatusLabel.Content = Application.Language.GetString("Deleting... ({0:0.00}% Complete)", 100 - PercentComplete)
                                  SetTitle(Application.Language.GetString("Deleting... {0:0.00}%", 100 - PercentComplete))
                                  Progress.Value = CurrentSize
                              End Sub)
            Thread.Sleep(200)
        Loop

        Dispatcher.Invoke(Sub()
                              StatusLabel.Content = Application.Language.GetString("Ready")
                              ResetTitle()
                              Progress.Value = 0
                              EnableUI(True)
                          End Sub)
    End Sub

    Private Sub DeleteBackgroundWorker_DoWork(ItemsToDelete As ArrayList)
        Try
            For Each Item As String In ItemsToDelete
                Log.Print("Deleting " & Item)
                My.Computer.FileSystem.DeleteDirectory(My.Settings.BackupsFolderLocation & "\" & Item, FileIO.DeleteDirectoryOption.DeleteAllContents)
            Next
            Me.Dispatcher.Invoke(Sub()
                                     DeleteBackgroundWorker_RunWorkerCompleted()
                                 End Sub)
        Catch ex As Exception
            If TypeOf ex Is ThreadAbortException Then
                Log.Severe("Delete thread aborted!")
                Me.Dispatcher.Invoke(Sub()
                                         Progress.Value = 0
                                         StatusLabel.Content = Application.Language.GetString("Operation Canceled – Ready")
                                         ResetTitle()
                                     End Sub)
            Else
                Dispatcher.Invoke(Sub() ErrorReportDialog.ShowDialog(Application.Language.GetString("An error occurred during the removal."), ex))
            End If
        End Try

        Log.Print("Done.")
    End Sub

    Private Async Sub DeleteBackgroundWorker_RunWorkerCompleted()
        EnableUI(True)
        Await RefreshBackupsList()
        ReloadBackupGroups()
        StatusLabel.Content = Application.Language.GetString("Ready")
        ResetTitle()
        Progress.IsIndeterminate = False
    End Sub
#End Region

#Region "Buttons"
    Private Sub RenameButton_Click(sender As Object, e As EventArgs) Handles RenameButton.Click, ListViewRenameItem.Click
        Dim RenameDialog As New RenameDialog
        RenameDialog.Owner = Me
        RenameDialog.ShowDialog()
    End Sub

    Private Sub CullButton_Click(sender As Object, e As RoutedEventArgs) Handles CullButton.Click
        Dim CullDialog As New CullDialog
        CullDialog.Owner = Me
        CullDialog.Show()
    End Sub
#End Region

#Region "Automatic Backup"
    Public IsMoving As Boolean

    Public Sub AutomaticBackupButton_Click(sender As Object, e As RoutedEventArgs) Handles AutomaticBackupButton.Click
        AutoBackupWindow.Owner = Me
        If AutoBackupWindow.IsVisible Then
            AutoBackupWindow.Hide()
            Me.Left = Me.Left + (AutoBackupWindow.Width / 2)
            AutomaticBackupButton.Content = Application.Language.GetString("Automatic Backup") & " >>"
            AdjustBackground()
        Else
            Me.Left = Me.Left - (AutoBackupWindow.Width / 2)
            AutomaticBackupButton.Content = Application.Language.GetString("Automatic Backup") & " <<"
            AutoBackupWindow.Top = Me.Top
            AutoBackupWindow.Left = Me.Left + Me.Width + 5
            AutoBackupWindow.Show()
            AdjustBackground()
        End If
    End Sub

    Private Sub Main_Activated(sender As Object, e As EventArgs) Handles MyBase.Activated
        AutoBackupWindow.Focus()
        Me.Focus()
    End Sub

    Private Sub Main_SizeChanged(sender As Object, e As SizeChangedEventArgs) Handles MyBase.SizeChanged
        Main_LocationChanged(sender, Nothing)
        AutoBackupWindow.Height = Me.Height
        AdjustBackground(False)
    End Sub

    Private Sub Main_LocationChanged(sender As Object, e As EventArgs) Handles MyBase.LocationChanged
        If Not AutoBackupWindow.IsMoving Then
            IsMoving = True
            AutoBackupWindow.Left = Me.Left + (Me.Width + 5)
            AutoBackupWindow.Top = Me.Top
            IsMoving = False
        End If
    End Sub
#End Region

#Region "ListView Context Menu"
#Region "Sorting"
    Private CurrentColumn As GridViewColumnHeader = Nothing
    Private SortAdorner As SortAdorner = Nothing

    Private Sub ListViewColumns_Click(sender As Object, e As RoutedEventArgs) Handles NameColumnHeader.Click, DateCreatedColumnHeader.Click, TypeColumnHeader.Click
        Dim ClickedColumn As GridViewColumnHeader = TryCast(sender, GridViewColumnHeader)
        Dim field As String = TryCast(ClickedColumn.Tag, String)

        If CurrentColumn IsNot Nothing Then
            AdornerLayer.GetAdornerLayer(CurrentColumn).Remove(SortAdorner)
            ListView.Items.SortDescriptions.Clear()
        End If

        Dim Direction As ListSortDirection = ListSortDirection.Ascending
        If CurrentColumn Is ClickedColumn AndAlso SortAdorner.Direction = Direction Then
            Direction = ListSortDirection.Descending
        End If

        CurrentColumn = ClickedColumn
        SortAdorner = New SortAdorner(CurrentColumn, Direction)
        AdornerLayer.GetAdornerLayer(CurrentColumn).Add(SortAdorner)
        ListView.Items.SortDescriptions.Add(New SortDescription(field, Direction))

        If Direction = ListSortDirection.Ascending Then
            ListViewSortAscendingItem.IsChecked = True
            ListViewSortDescendingItem.IsChecked = False
        Else
            ListViewSortAscendingItem.IsChecked = False
            ListViewSortDescendingItem.IsChecked = True
        End If

        If ClickedColumn Is NameColumnHeader Then
            ListViewSortByNameItem.IsChecked = True
            ListViewSortByDateCreatedItem.IsChecked = False
            ListViewSortByTypeItem.IsChecked = False
            My.Settings.ListViewSortBy = BackupsListView.SortBy.Name
        ElseIf ClickedColumn Is DateCreatedColumnHeader Then
            ListViewSortByNameItem.IsChecked = False
            ListViewSortByDateCreatedItem.IsChecked = True
            ListViewSortByTypeItem.IsChecked = False
            My.Settings.ListViewSortBy = BackupsListView.SortBy.DateCreated
        ElseIf ClickedColumn Is TypeColumnHeader Then
            ListViewSortByNameItem.IsChecked = False
            ListViewSortByDateCreatedItem.IsChecked = False
            ListViewSortByTypeItem.IsChecked = True
            My.Settings.ListViewSortBy = BackupsListView.SortBy.Type
        End If
    End Sub

    Private Sub ListViewGroupByItems_Click(sender As Object, e As RoutedEventArgs) Handles ListViewGroupByNameItem.Click, ListViewGroupByTypeItem.Click, ListViewGroupByNothingItem.Click
        If sender Is ListViewGroupByNameItem Then
            SetListViewGroupDescriptions("OriginalName")
        ElseIf sender Is ListViewGroupByTypeItem Then
            SetListViewGroupDescriptions("Type")
        Else
            SetListViewGroupDescriptions(Nothing)
        End If
    End Sub

    Private Sub SetListViewGroupDescriptions(Group As String)
        ListViewGroupByNameItem.IsChecked = (Group = "OriginalName")
        ListViewGroupByTypeItem.IsChecked = (Group = "Type")
        ListViewGroupByNothingItem.IsChecked = (Group = Nothing)

        Dim View As CollectionView = DirectCast(CollectionViewSource.GetDefaultView(ListView.ItemsSource), CollectionView)
        View.GroupDescriptions.Clear()

        If Group <> Nothing Then
            View.GroupDescriptions.Add(New PropertyGroupDescription(Group))
        End If

        My.Settings.ListViewGroupBy = BackupsListView.GroupBy.Type
    End Sub

    Private Sub ListViewSortByNameItem_Click(sender As Object, e As RoutedEventArgs) Handles ListViewSortByNameItem.Click
        If CurrentColumn IsNot Nothing Then
            AdornerLayer.GetAdornerLayer(CurrentColumn).Remove(SortAdorner)
            ListView.Items.SortDescriptions.Clear()
        End If

        CurrentColumn = NameColumnHeader
        SortAdorner = New SortAdorner(CurrentColumn, ListSortDirection.Ascending)
        AdornerLayer.GetAdornerLayer(CurrentColumn).Add(SortAdorner)
        ListView.Items.SortDescriptions.Add(New SortDescription("Name", ListSortDirection.Ascending))

        ListViewSortByNameItem.IsChecked = True
        ListViewSortByDateCreatedItem.IsChecked = False
        ListViewSortByTypeItem.IsChecked = False

        My.Settings.ListViewSortBy = BackupsListView.SortBy.Name
    End Sub

    Private Sub ListViewSortByDateCreatedItem_Click(sender As Object, e As RoutedEventArgs) Handles ListViewSortByDateCreatedItem.Click
        If CurrentColumn IsNot Nothing Then
            AdornerLayer.GetAdornerLayer(CurrentColumn).Remove(SortAdorner)
            ListView.Items.SortDescriptions.Clear()
        End If

        CurrentColumn = DateCreatedColumnHeader
        SortAdorner = New SortAdorner(CurrentColumn, ListSortDirection.Ascending)
        AdornerLayer.GetAdornerLayer(CurrentColumn).Add(SortAdorner)
        ListView.Items.SortDescriptions.Add(New SortDescription("DateCreated", ListSortDirection.Ascending))

        ListViewSortByNameItem.IsChecked = False
        ListViewSortByDateCreatedItem.IsChecked = True
        ListViewSortByTypeItem.IsChecked = False

        My.Settings.ListViewSortBy = BackupsListView.SortBy.DateCreated
    End Sub

    Private Sub ListViewSortByTypeItem_Click(sender As Object, e As RoutedEventArgs) Handles ListViewSortByTypeItem.Click
        If CurrentColumn IsNot Nothing Then
            AdornerLayer.GetAdornerLayer(CurrentColumn).Remove(SortAdorner)
            ListView.Items.SortDescriptions.Clear()
        End If

        CurrentColumn = TypeColumnHeader
        SortAdorner = New SortAdorner(CurrentColumn, ListSortDirection.Ascending)
        AdornerLayer.GetAdornerLayer(CurrentColumn).Add(SortAdorner)
        ListView.Items.SortDescriptions.Add(New SortDescription("Type", ListSortDirection.Ascending))

        ListViewSortByNameItem.IsChecked = False
        ListViewSortByDateCreatedItem.IsChecked = False
        ListViewSortByTypeItem.IsChecked = True

        My.Settings.ListViewSortBy = BackupsListView.SortBy.Type
    End Sub

    Private Sub ListViewSortAscendingItem_Click(sender As Object, e As RoutedEventArgs) Handles ListViewSortAscendingItem.Click
        Dim field As String = TryCast(CurrentColumn.Tag, String)
        If CurrentColumn IsNot Nothing Then
            AdornerLayer.GetAdornerLayer(CurrentColumn).Remove(SortAdorner)
            ListView.Items.SortDescriptions.Clear()
        End If

        SortAdorner = New SortAdorner(CurrentColumn, ListSortDirection.Ascending)
        AdornerLayer.GetAdornerLayer(CurrentColumn).Add(SortAdorner)
        ListView.Items.SortDescriptions.Add(New SortDescription(field, ListSortDirection.Ascending))

        ListViewSortAscendingItem.IsChecked = True
        ListViewSortDescendingItem.IsChecked = False


    End Sub

    Private Sub ListViewSortDescendingItem_Click(sender As Object, e As RoutedEventArgs) Handles ListViewSortDescendingItem.Click
        Dim field As String = TryCast(CurrentColumn.Tag, String)
        If CurrentColumn IsNot Nothing Then
            AdornerLayer.GetAdornerLayer(CurrentColumn).Remove(SortAdorner)
            ListView.Items.SortDescriptions.Clear()
        End If

        SortAdorner = New SortAdorner(CurrentColumn, ListSortDirection.Descending)
        AdornerLayer.GetAdornerLayer(CurrentColumn).Add(SortAdorner)
        ListView.Items.SortDescriptions.Add(New SortDescription(field, ListSortDirection.Descending))

        ListViewSortAscendingItem.IsChecked = False
        ListViewSortDescendingItem.IsChecked = True
    End Sub
#End Region
#End Region

#Region "Tray Icon"
    Private Sub ExitToolbarMenuItem_Click(sender As Object, e As EventArgs)
        Application.CloseAction = Application.AppCloseAction.Close
        Me.Close()
    End Sub

    Private Sub NotifyIcon_Click(sender As Object, e As EventArgs) Handles NotifyIcon.DoubleClick, NotifyIcon.BalloonTipClicked
        Me.Show()
        Me.Activate()
        Me.Focus()
        If AutoBackupWindowWasShown Then
            AutoBackupWindow.Show()
            AutoBackupWindow.Activate()
            AdjustBackground()
        End If
    End Sub
#End Region

#Region "Close To Tray"
    Private Sub Main_Closing(sender As Object, e As CancelEventArgs) Handles MyBase.Closing
        Me.Focus()

        If Not Application.CloseAction = Application.AppCloseAction.Force Then
            If Application.CloseAction = Application.AppCloseAction.Ask Then
                If Not My.Settings.SaveCloseState Then
                    Dim CloseToTrayDialog As New CloseToTrayDialog
                    CloseToTrayDialog.Owner = Me
                    Select Case CloseToTrayDialog.ShowDialog()
                        Case Forms.DialogResult.Yes
                            Me.Hide()
                            AutoBackupWindowWasShown = AutoBackupWindow.IsVisible
                            If AutoBackupWindow.IsVisible Then AutoBackupWindow.Hide()
                            If My.Settings.FirstCloseToTray Then
                                NotifyIcon.ShowBalloonTip(2000, Application.Language.GetString("I'm Here!"), Application.Language.GetString("MCBackup is running in background."), Forms.ToolTipIcon.Info)
                                My.Settings.FirstCloseToTray = False
                            End If

                            Log.Print("Closing To tray")

                            My.Settings.CloseToTray = True

                            e.Cancel = True
                        Case Forms.DialogResult.No
                            If BackupManager.IsBusy Or ThreadIsNotNothingAndAlive(DeleteThread) Then
                                MetroMessageBox.Show(Application.Language.GetString("MCBackup is currently working. Please wait for the operation to complete or cancel it."), Application.Language.GetString("MCBackup is working!"), MessageBoxButton.OK, MessageBoxImage.Question)
                                e.Cancel = True
                            End If
                        Case Forms.DialogResult.Cancel
                            e.Cancel = True
                        Case Else
                            e.Cancel = True
                    End Select
                Else
                    If My.Settings.CloseToTray Then
                        Me.Hide()
                        AutoBackupWindowWasShown = AutoBackupWindow.IsVisible
                        If AutoBackupWindow.IsVisible Then AutoBackupWindow.Hide()
                        If My.Settings.FirstCloseToTray Then
                            NotifyIcon.ShowBalloonTip(2000, Application.Language.GetString("I'm Here!"), Application.Language.GetString("MCBackup is running in background."), Forms.ToolTipIcon.Info)
                            My.Settings.FirstCloseToTray = False
                        End If

                        Log.Print("Closing To tray")

                        e.Cancel = True
                    Else
                        If BackupManager.IsBusy Or ThreadIsNotNothingAndAlive(DeleteThread) Then
                            MetroMessageBox.Show(Application.Language.GetString("MCBackup is currently working. Please wait for the operation to complete or cancel it."), Application.Language.GetString("MCBackup is working!"), MessageBoxButton.OK, MessageBoxImage.Question)
                            e.Cancel = True
                        End If
                    End If
                End If
            End If

            If e.Cancel Then Exit Sub

            NotifyIcon.Visible = False
            NotifyIcon.Dispose()

            My.Settings.SidebarWidth = GridSidebarColumn.Width
            My.Settings.ListViewWidth = GridListViewColumn.Width

            Dim View As CollectionView = DirectCast(CollectionViewSource.GetDefaultView(ListView.ItemsSource), CollectionView)

            If Not Me.WindowState = Windows.WindowState.Maximized Then My.Settings.WindowSize = New Size(Me.Width, Me.Height)

            My.Settings.IsWindowMaximized = IIf(Me.WindowState = WindowState.Maximized, True, False)

            My.Settings.Save()

            Log.Print("Someone is closing me!")
        End If
    End Sub
#End Region

#Region "Toolbar"
    Private Sub FileToolbarButton_Click(sender As Object, e As RoutedEventArgs) Handles FileToolbarButton.Click
        FileContextMenu.PlacementTarget = FileToolbarButton
        FileContextMenu.Placement = Primitives.PlacementMode.Bottom
        FileContextMenu.IsOpen = True
    End Sub

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
#End Region

#Region "Filter Text Box"
    Dim PreventUpdate As Boolean = False
    Dim WithEvents FilterTimer As New DispatcherTimer With {.Interval = TimeSpan.FromMilliseconds(300)}

    Private Sub SearchTextBox_TextChanged(sender As Object, e As TextChangedEventArgs) Handles SearchTextBox.TextChanged
        If Not PreventUpdate Then
            FilterTimer.Stop()
            FilterTimer.Start()
        End If
    End Sub

    Private Async Sub SearchTextBox_KeyDown(sender As Object, e As KeyEventArgs) Handles SearchTextBox.KeyDown
        If e.Key = Key.Enter Then
            Await RefreshBackupsList()
            FilterTimer.Stop()
        End If
    End Sub

    Private Async Sub FilterTimer_Tick(sender As Object, e As EventArgs) Handles FilterTimer.Tick
        FilterTimer.Stop()
        Await RefreshBackupsList()
    End Sub
#End Region

#Region "ListView"
    Private Sub ListView_ContextMenuOpening(sender As Object, e As ContextMenuEventArgs) Handles ListView.ContextMenuOpening
        Select Case ListView.SelectedItems.Count
            Case Is > 1
                ListViewRestoreItem.IsEnabled = False
                ListViewDeleteItem.IsEnabled = True
                ListViewRenameItem.IsEnabled = False
                ListViewOpenInExplorerItem.IsEnabled = False
            Case 1
                ListViewRestoreItem.IsEnabled = True
                ListViewDeleteItem.IsEnabled = True
                ListViewRenameItem.IsEnabled = True
                ListViewOpenInExplorerItem.IsEnabled = True
            Case 0
                ListViewRestoreItem.IsEnabled = False
                ListViewDeleteItem.IsEnabled = False
                ListViewRenameItem.IsEnabled = False
                ListViewOpenInExplorerItem.IsEnabled = False
        End Select
    End Sub

    Private Async Sub TabControl_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles GroupsTabControl.SelectionChanged
        Await RefreshBackupsList()
    End Sub

    Private Sub ListView_MouseDown(sender As Object, e As MouseButtonEventArgs) Handles ListView.MouseDown
        ListView.SelectedIndex = -1
        ListView.Focus()
    End Sub

    Private Sub ListView_KeyDown(sender As Object, e As KeyEventArgs) Handles ListView.KeyDown
        If e.Key = Key.Delete And ListView.SelectedItems.Count > 0 Then
            DeleteButton_Click(sender, Nothing)
        End If
    End Sub
#End Region

    Private Sub EditBackupGroupsButton_Click(sender As Object, e As RoutedEventArgs) Handles EditBackupGroupsButton.Click
        Dim OptionsWindow As New OptionsDialog()
        OptionsWindow.Owner = Me
        OptionsWindow.ShowDialog(3)
    End Sub

    Private Sub CancelButton_Click(sender As Object, e As RoutedEventArgs) Handles CancelButton.Click
        BackupManager.Cancel()

        If DeleteThread IsNot Nothing Then
            If DeleteThread.IsAlive Then
                If MetroMessageBox.Show(Application.Language.GetString("Are you sure you want to cancel the deletion?"), Application.Language.GetString("Are you sure?"), MessageBoxButton.YesNo, MessageBoxImage.Exclamation) = MessageBoxResult.Yes Then
                    If DeleteThread.IsAlive Then
                        DeleteThread.Abort()
                    End If
                End If
            End If
        End If
    End Sub

    Private Sub EnableUI(IsEnabled As Boolean)
        Me.Cursor = IIf(IsEnabled, Cursors.Arrow, Cursors.AppStarting)

        BackupButton.IsEnabled = IsEnabled
        RestoreButton.IsEnabled = IsEnabled
        DeleteButton.IsEnabled = IsEnabled
        RenameButton.IsEnabled = IsEnabled
        CullButton.IsEnabled = IsEnabled
        CancelButton.IsEnabled = Not IsEnabled
        GroupsTabControl.IsEnabled = IsEnabled
        ListView.IsEnabled = IsEnabled
        ListView_SelectionChanged(Nothing, Nothing)
        SearchTextBox.IsEnabled = IsEnabled
        FileToolbarButton.IsEnabled = IsEnabled
        EditToolbarButton.IsEnabled = IsEnabled
        ToolsToolbarButton.IsEnabled = IsEnabled
        HelpToolbarButton.IsEnabled = IsEnabled
    End Sub

    Private Sub QuitToolbarMenuItem_Click(sender As Object, e As RoutedEventArgs)
        Application.CloseAction = Application.AppCloseAction.Close
        Me.Close()
    End Sub

    Private Function ThreadIsNotNothingAndAlive(Thread As Thread)
        If Thread IsNot Nothing Then
            Return Thread.IsAlive
        Else
            Return False
        End If
    End Function

    Private Function ProcessIsNotNothingAndRunning(Process As Process)
        If Process IsNot Nothing Then
            Return Not Process.HasExited
        Else
            Return False
        End If
    End Function

    Private Sub ListViewMoveToGroupItem_Click(sender As Object, e As RoutedEventArgs) Handles ListViewMoveToGroupItem.Click
        Dim MoveToGroupDialog As New MoveToGroupDialog(ListView.SelectedItems.Cast(Of ListViewBackupItem).ToList)
        MoveToGroupDialog.Owner = Me
        MoveToGroupDialog.ShowDialog()
    End Sub

    Public Async Sub MoveToGroup(SelectedItems As List(Of ListViewBackupItem), Group As String)
        Me.Dispatcher.Invoke(Sub()
                                 EnableUI(False)
                                 StatusLabel.Content = Application.Language.GetString("Moving backups...")
                                 SetTitle(Application.Language.GetString("Moving backups..."))
                                 Progress.IsIndeterminate = True
                             End Sub)

        For Each Item As ListViewBackupItem In SelectedItems
            Log.Info("Rewriting info.json file For backup '{0}'.", Item.Name)

            Try
                Dim InfoJson As JObject

                Using SR As New StreamReader(My.Settings.BackupsFolderLocation & "\" & Item.Name & "\info.json")
                    InfoJson = JsonConvert.DeserializeObject(SR.ReadToEnd)
                End Using

                If InfoJson("Group") <> Group Then
                    InfoJson.Remove("Group")

                    InfoJson.Add(New JProperty("Group", Group))

                    Using SW As New StreamWriter(My.Settings.BackupsFolderLocation & "\" & Item.Name & "\info.json")
                        SW.Write(JsonConvert.SerializeObject(InfoJson))
                        SW.Dispose()
                    End Using
                End If
            Catch ex As Exception
                Log.Severe("An error occured while trying to rewrite JSON data: " & ex.Message)
            End Try
        Next

        Await Dispatcher.Invoke(Async Sub()
                                    EnableUI(True)
                                    StatusLabel.Content = Application.Language.GetString("Ready")
                                    ResetTitle()
                                    Progress.IsIndeterminate = False
                                    Await RefreshBackupsList()
                                End Sub)
    End Sub

    Private Sub OpenInExplorerItem_Click(sender As Object, e As RoutedEventArgs) Handles ListViewOpenInExplorerItem.Click
        Process.Start(My.Settings.BackupsFolderLocation & "\" & ListView.SelectedItem.Name)
    End Sub

    Private Sub Window_StateChanged(sender As Object, e As EventArgs) Handles Window.StateChanged
        If Not Me.WindowState = Windows.WindowState.Maximized Then My.Settings.WindowSize = New Size(Me.Width, Me.Height)
    End Sub

    Public Shared Function GetBackupTimeStamp()
        Return Date.Now.ToString("yyyy-MM-dd (hh\hmm\mss\s)")
    End Function

    Public Sub AdjustBackground(Optional UpdateMainWindow As Boolean = True)
        If Not (String.IsNullOrEmpty(My.Settings.BackgroundImageLocation)) And File.Exists(My.Settings.BackgroundImageLocation) Then
            If AutoBackupWindow.IsVisible Then
                Dim MainWindowPercentage = Me.Width / (Me.AutoBackupWindow.Width + Me.Width)

                Dim MainBrush As ImageBrush = New ImageBrush(New CroppedBitmap(BackgroundImageBitmap, New Int32Rect(0, 0, BackgroundImageBitmap.PixelWidth * MainWindowPercentage, BackgroundImageBitmap.PixelHeight)))
                Dim AutoBackupBrush As ImageBrush = New ImageBrush(New CroppedBitmap(BackgroundImageBitmap, New Int32Rect(BackgroundImageBitmap.PixelWidth * MainWindowPercentage, 0, BackgroundImageBitmap.PixelWidth - BackgroundImageBitmap.PixelWidth * MainWindowPercentage, BackgroundImageBitmap.PixelHeight)))

                MainBrush.AlignmentX = AlignmentX.Right
                AutoBackupBrush.AlignmentX = AlignmentX.Left

                MainBrush.Stretch = My.Settings.BackgroundImageStretch
                AutoBackupBrush.Stretch = My.Settings.BackgroundImageStretch

                MainBrush.AlignmentY = My.Settings.BackgroundImageYAlign
                AutoBackupBrush.AlignmentY = My.Settings.BackgroundImageYAlign

                Me.Background = MainBrush
                AutoBackupWindow.Background = AutoBackupBrush
            Else
                If UpdateMainWindow Then
                    Dim Brush As New ImageBrush(BackgroundImageBitmap)
                    Brush.Stretch = My.Settings.BackgroundImageStretch
                    'Brush.AlignmentX = My.Settings.BackgroundImageXAlign
                    Brush.AlignmentY = My.Settings.BackgroundImageYAlign
                    Me.Background = Brush
                End If
            End If
        End If
    End Sub

    Public Sub UpdateTheme()
        ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent(My.Settings.Theme), ThemeManager.GetAppTheme(My.Settings.ThemeShade))

        Dim DefaultBackground As SolidColorBrush = DirectCast(FindResource("ControlBackgroundBrush"), SolidColorBrush)
        Dim InterfaceOpacityBackground As New SolidColorBrush(Color.FromArgb(My.Settings.InterfaceOpacity * 2.55, DefaultBackground.Color.R, DefaultBackground.Color.G, DefaultBackground.Color.B))

        Me.ListView.Background = InterfaceOpacityBackground
        Me.Sidebar.Background = InterfaceOpacityBackground
        AutoBackupWindow.MinutesNumUpDown.Background = InterfaceOpacityBackground
        AutoBackupWindow.SavesListView.Background = InterfaceOpacityBackground
    End Sub

    Public Sub ResetTitle()
        Title = "MCBackup " + ApplicationVersion.ToString()
    End Sub

    Public Sub SetTitle(details As String)
        Title = "MCBackup " + ApplicationVersion.ToString() + " – " + details
    End Sub

    Public Sub SetTitle(details As String, ParamArray args As Object())
        Title = "MCBackup " + ApplicationVersion.ToString() + " – " + String.Format(details, args)
    End Sub
End Class

Public Class TaggedTabItem
    Private _Text As String
    Public Property Text() As String
        Get
            Return _Text
        End Get
        Set(value As String)
            _Text = value
        End Set
    End Property

    Private _Tag As Object
    Public Property Tag() As Object
        Get
            Return _Tag
        End Get
        Set(value As Object)
            _Tag = value
        End Set
    End Property

    Sub New(Text As String, Tag As Object)
        Me.Text = Text
        Me.Tag = Tag
    End Sub
End Class