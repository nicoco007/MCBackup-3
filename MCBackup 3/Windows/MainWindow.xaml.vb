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

Imports System.ComponentModel
Imports System.Configuration
Imports System.Globalization
Imports System.IO
Imports System.Net
Imports System.Text
Imports System.Threading
Imports System.Windows.Threading
Imports MahApps.Metro
Imports MCBackup.BackupManager
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports Substrate

Partial Class MainWindow

#Region "Variables"
    Private AppData As String = Environ("APPDATA")

    Public BackupInfo As New BackupInfo
    Public RestoreInfo As New RestoreInfo

    Private DeleteForRestoreThread As Thread
    Private RestoreThread As Thread
    Private DeleteThread As Thread

    Private FolderBrowserDialog As New Forms.FolderBrowserDialog
    Public StartupPath As String = Directory.GetCurrentDirectory()
    Public ApplicationVersion As String = Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()
    Public LatestVersion As String

    Public WithEvents NotifyIcon As New Forms.NotifyIcon

    Public AutoBackupWindow As New AutoBackupWindow
    Public NotificationIconWindow As New NotificationIconWindow
    Private Splash As New Splash

    Public AutoBackupWindowWasShown As Boolean = False

    Public BackgroundImageBitmap As BitmapImage

    Private Manager As New BackupManager()
#End Region

#Region "Load"
    Public Sub New()
        InitializeComponent()

        AddHandler Manager.BackupProgressChanged, AddressOf BackupManager_BackupProgressChanged
        AddHandler Manager.BackupCompleted, AddressOf BackupManager_BackupCompleted
        AddHandler Manager.RestoreProgressChanged, AddressOf BackupManager_RestoreProgressChanged
        AddHandler Manager.RestoreCompleted, AddressOf BackupManager_RestoreCompleted
    End Sub

    Private Sub Window_Loaded(sender As Object, e As EventArgs) Handles Window.Loaded
        Application.CloseAction = Application.AppCloseAction.Force

        UpdateTheme()

        Splash.Show()
        Splash.ShowStatus("Splash.Status.Starting", "Starting...")

        Log.SPrint("")
        Log.SPrint("---------- Starting MCBackup v{0} @ {1} ----------", ApplicationVersion, Log.DebugTimeStamp())
        Log.Print(Game.Launcher.Minecraft.GetStringValue())
        Log.Print(Game.Launcher.FeedTheBeast.GetStringValue())
        Log.Print("OS Name: " & Log.GetWindowsVersion())
        Log.Print("OS Version: " & Environment.OSVersion.Version.Major & "." & Environment.OSVersion.Version.Minor)
        Log.Print("Architecture: " & Log.GetWindowsArch())
        Log.Print(".NET Framework Version: " & Environment.Version.Major & "." & Environment.Version.Minor)

        Dim SettingsUpgraded As Boolean = False
        Dim ConfigurationFile As String = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath
        Debug.Print("Configuration file: " + ConfigurationFile)

        If My.Settings.CallUpgrade Then
            ' Check if other configuration files exist
            Dim ConfigurationDirectory As DirectoryInfo = New FileInfo(ConfigurationFile).Directory.Parent
            If ConfigurationDirectory.Exists() Then
                For Each VersionDirectory As DirectoryInfo In ConfigurationDirectory.GetDirectories()
                    If File.Exists(Path.Combine(VersionDirectory.FullName, "user.config")) And VersionDirectory.Name <> ApplicationVersion.ToString() Then
                        SettingsUpgraded = True
                        Exit For
                    End If
                Next
            End If

            Log.Print("Upgrading settings")
            My.Settings.Upgrade()
            My.Settings.CallUpgrade = False
        End If

        Splash.StepProgress()

        Me.Title = "MCBackup " & ApplicationVersion

        Splash.ShowStatus("Splash.Status.LoadingLang", "Loading Language...")
        Splash.StepProgress()

        Dim DefaultLanguage As String = "en_US"

        Log.Print("System language: " & CultureInfo.CurrentCulture.EnglishName, Log.Level.Debug)

        Select Case CultureInfo.CurrentCulture.ThreeLetterISOLanguageName
            Case "eng"
                DefaultLanguage = "en_US"
            Case "fra"
                DefaultLanguage = "fr_FR"
        End Select

        Splash.StepProgress()

        Try
            If String.IsNullOrEmpty(My.Settings.Language) Then
                My.Settings.Language = DefaultLanguage
            End If
            MCBackup.Language.Load(My.Settings.Language & ".lang")
            If String.IsNullOrEmpty(My.Settings.DefaultBackupName) Then My.Settings.DefaultBackupName = MCBackup.Language.GetString("Localization.DefaultBackupName")
            If String.IsNullOrEmpty(My.Settings.DefaultAutoBackupName) Then My.Settings.DefaultAutoBackupName = MCBackup.Language.GetString("Localization.DefaultAutoBackupName")
        Catch ex As Exception
            ErrorReportDialog.Show("Could not load language file " & My.Settings.Language & ".lang! MCBackup will now exit.", ex)
            My.Settings.Language = DefaultLanguage
            My.Settings.Save()
            Me.Close()
            Exit Sub
        End Try

        Splash.StepProgress()

        NotifyIcon.Text = "MCBackup " & ApplicationVersion
        NotifyIcon.Icon = New System.Drawing.Icon(Application.GetResourceStream(New Uri("pack://application:,,,/Resources/icon.ico")).Stream)
        Dim ContextMenu As New Forms.ContextMenu
        Dim ExitToolbarMenuItem As New Forms.MenuItem
        ExitToolbarMenuItem.Text = MCBackup.Language.FindString("NotifyIcon.ContextMenu.ExitItem.Text", My.Settings.Language & ".lang")
        AddHandler ExitToolbarMenuItem.Click, AddressOf ExitToolbarMenuItem_Click

        ContextMenu.MenuItems.Add(ExitToolbarMenuItem)
        NotifyIcon.ContextMenu = ContextMenu
        NotifyIcon.Visible = True

        Splash.StepProgress()

        Splash.ShowStatus("Splash.Status.LoadingProps", "Loading Settings...")

        If SettingsUpgraded Then MetroMessageBox.Show(MCBackup.Language.GetString("Message.SettingsUpgrade"), MCBackup.Language.GetString("Message.Caption.Information"), MessageBoxButton.OK, MessageBoxImage.Information)

        Log.Print(String.Format("Current Launcher: '{0}'", My.Settings.Launcher.GetStringValue()))
        Splash.StepProgress()

        StatusLabel.Foreground = New SolidColorBrush(My.Settings.StatusLabelColor)

        Splash.StepProgress()

        If Not My.Settings.BackgroundImageLocation = "" And My.Computer.FileSystem.FileExists(My.Settings.BackgroundImageLocation) Then
            BackgroundImageBitmap = New BitmapImage(New Uri(My.Settings.BackgroundImageLocation))
            AdjustBackground()
        End If

        Splash.StepProgress()

        Me.Width = My.Settings.WindowSize.Width
        Me.Height = My.Settings.WindowSize.Height

        Me.WindowState = IIf(My.Settings.IsWindowMaximized, WindowState.Maximized, WindowState.Normal)

        If My.Settings.BackupsFolderLocation = "" Then
            My.Settings.BackupsFolderLocation = StartupPath & "\backups"
            Directory.CreateDirectory(My.Settings.BackupsFolderLocation)
        End If

        If Not My.Computer.FileSystem.DirectoryExists(My.Settings.BackupsFolderLocation) Then
            If MetroMessageBox.Show(MCBackup.Language.GetString("Message.BackupsFolderNotFound"), MCBackup.Language.GetString("Message.Caption.Error"), MessageBoxButton.OKCancel) = MessageBoxResult.OK Then
                My.Settings.BackupsFolderLocation = StartupPath & "\backups"
                My.Computer.FileSystem.CreateDirectory(My.Settings.BackupsFolderLocation)
            Else
                Me.Close()
                Exit Sub
            End If
        End If

        Log.Print("Backups folder location set to '" & My.Settings.BackupsFolderLocation & "'")

        GridSidebarColumn.Width = New GridLength(My.Settings.SidebarWidth.Value, GridUnitType.Star)
        GridListViewColumn.Width = New GridLength(My.Settings.ListViewWidth.Value, GridUnitType.Star)

        Splash.ShowStatus("Splash.Status.FindingMinecraft", "Finding Minecraft...")
        Splash.StepProgress()

        If String.IsNullOrEmpty(My.Settings.MinecraftFolderLocation) Then
            If IO.Directory.Exists(AppData & "\.minecraft\saves") And IO.Directory.Exists(AppData & "\.minecraft\versions") Then
                My.Settings.MinecraftFolderLocation = AppData & "\.minecraft"
                My.Settings.SavesFolderLocation = My.Settings.MinecraftFolderLocation & "\saves"
            End If
        End If

        If Not Directory.Exists(My.Settings.MinecraftFolderLocation) Then
            If MetroMessageBox.Show(MCBackup.Language.GetString("Message.NoMinecraftInstallError"), MCBackup.Language.GetString("Message.Caption.Error"), MessageBoxButton.OKCancel, MessageBoxImage.Error) = MessageBoxResult.OK Then
                Dim SetMinecraftFolderWindow As New SetMinecraftFolderWindow
                SetMinecraftFolderWindow.ShowDialog()
            Else
                Me.Close()
            End If
        End If

        Splash.StepProgress()

        Log.Print("Minecraft folder set to '" & My.Settings.MinecraftFolderLocation & "'")
        If My.Settings.Launcher = Game.Launcher.Minecraft Then Log.Print("Saves folder set to '" & My.Settings.SavesFolderLocation & "'")

        Splash.StepProgress()

        If My.Settings.CheckForUpdates Then
            Log.Print("Searching for updates...")
            Splash.ShowStatus("Splash.Status.CheckingUpdates", "Checking for Updates...")

            Log.Print("Connecting to http://content.nicoco007.com...")
            Try
                My.Computer.Network.Ping("content.nicoco007.com", 1000)
                Log.Print("Successfully connected.")

                Dim WebClient As New WebClient
                AddHandler WebClient.DownloadStringCompleted, AddressOf WebClient_DownloadedStringAsync
                WebClient.DownloadStringAsync(New Uri("http://content.nicoco007.com/downloads/mcbackup-3/version"))
            Catch ex As Exception
                Log.Print("Could not connect to http://content.nicoco007.com, skipping update check...", Log.Level.Warning)
            End Try
        Else
            Log.Print("Update checking disabled, skipping...")
            Splash.StepProgress()
        End If

        Splash.ShowStatus("Splash.Status.Done", "Done.")
        Splash.StepProgress()

        LoadLanguage()

        Splash.Hide()

        Application.CloseAction = Application.AppCloseAction.Ask

        ' Send +1 to StatCounter
        If My.Settings.SendAnonymousData Then
            Dim WebClient As New WebClient
            WebClient.DownloadDataAsync(New Uri("http://c.statcounter.com/10065404/0/6bad5aa6/1/"))
        End If
    End Sub

    Private Sub Window_ContentRendered(sender As Object, e As EventArgs) Handles Window.ContentRendered
        RefreshBackupsList()
        ReloadBackupGroups()
    End Sub

    Private Sub WebClient_DownloadedStringAsync(sender As Object, e As DownloadStringCompletedEventArgs)
        If e.Error Is Nothing Then
            LatestVersion = e.Result
            Dim ApplicationVersionInt = ApplicationVersion.Replace(".", "")
            Dim LatestVersionInt = LatestVersion.Replace(".", "")
            If ApplicationVersionInt < LatestVersionInt Then
                Log.Print("A new version is available (version " & LatestVersion & ")!")
                Dim UpdateDialog As New UpdateDialog
                UpdateDialog.Owner = Me
                UpdateDialog.Show()
            ElseIf ApplicationVersionInt > LatestVersionInt Then
                Log.Print("MCBackup seems to be running a beta version (version " & ApplicationVersion & ")!")
                Me.Title += " Beta"
            ElseIf ApplicationVersionInt = LatestVersionInt Then
                Log.Print("MCBackup is up-to-date (version " & ApplicationVersion & ").")
            End If
        Else
            Log.Print("An error occured while trying to retrieve the latest version: " & e.Error.Message)
            LatestVersion = "Unknown"
        End If
    End Sub

    Private WithEvents BGW As New BackgroundWorker
    Private Items

    Public Sub RefreshBackupsList()
        If Me.IsLoaded And GroupsTabControl.SelectedIndex <> -1 And Not BGW.IsBusy Then
            If Dispatcher.CheckAccess Then
                Items = New List(Of ListViewBackupItem)
                EnableUI(False)
                Progress.IsIndeterminate = True
                StatusLabel.Content = MCBackup.Language.GetString("Status.RefreshingBackupsList")
                Me.Title = "MCBackup " + ApplicationVersion + " - " + MCBackup.Language.GetString("MainWindow.Title.RefreshingBackupsList", ApplicationVersion)
                BGW.RunWorkerAsync()
            Else
                Dispatcher.Invoke(Sub() RefreshBackupsList())
            End If
        End If
    End Sub

    Private Sub BGW_DoWork() Handles BGW.DoWork
        Dim Search As String = "", Group As String = ""
        Dispatcher.Invoke(Sub()
                              If Not SearchTextBox.Text = MCBackup.Language.GetString("MainWindow.Search") Then
                                  Search = SearchTextBox.Text
                              End If
                              Group = TryCast(GroupsTabControl.SelectedItem, TaggedTabItem).Tag
                          End Sub)
        Dim Directory As New IO.DirectoryInfo(My.Settings.BackupsFolderLocation) ' Create a DirectoryInfo variable for the backups folder

        Dim DirectoriesToDelete As New ArrayList

        For Each Folder As DirectoryInfo In Directory.GetDirectories ' For each folder in the backups folder
            Dim Type As String = "[ERROR]"                 ' Create variables with default value [ERROR], in case one of the values doesn't exist

            Try
                If File.Exists(Folder.FullName & "\info.mcb") Then ' Convert info.mcb to info.json
                    Log.Print("Converting info.mcb to JSON in backup '{0}'", Log.Level.Info, Folder.Name)

                    Dim Json As New JObject

                    Using SR As New StreamReader(Folder.FullName & "\info.mcb")
                        Do While SR.Peek <> -1
                            Dim Line As String = SR.ReadLine
                            If Not Line.StartsWith("#") Then
                                If Line.StartsWith("baseFolderName=") Then
                                    Json.Add(New JProperty("OriginalName", Line.Substring(15)))
                                ElseIf Line.StartsWith("type=") Then
                                    Json.Add(New JProperty("Type", Line.Substring(5)))
                                ElseIf Line.StartsWith("desc=") Then
                                    Json.Add(New JProperty("Description", Line.Substring(5)))
                                ElseIf Line.StartsWith("groupName=") Then
                                    Json.Add(New JProperty("Group", Line.Substring(10)))
                                ElseIf Line.StartsWith("launcher=") Then
                                    Json.Add(New JProperty("Launcher", Line.Substring(9)))
                                ElseIf Line.StartsWith("modpack=") Then
                                    Json.Add(New JProperty("Modpack", Line.Substring(8)))
                                End If
                            End If
                        Loop
                    End Using

                    Using SR As New StreamWriter(Folder.FullName & "\info.json")
                        SR.Write(JsonConvert.SerializeObject(Json, Formatting.Indented))
                    End Using

                    File.Delete(Folder.FullName & "\info.mcb")
                End If

                If Not My.Computer.FileSystem.FileExists(Folder.FullName & "\info.json") Then
                    Log.Print("'info.json' does not exist in folder '{0}'. This folder will not be considered as a backup.", Log.Level.Warning, Folder.Name)
                    DirectoriesToDelete.Add(Folder.Name)
                    Exit Try
                End If

                Dim InfoJson As JObject

                Using SR As New StreamReader(Folder.FullName & "\info.json")
                    InfoJson = JsonConvert.DeserializeObject(SR.ReadToEnd)
                End Using

                If Not IsNumeric(InfoJson("Type")) Then
                    Select Case InfoJson("Type")
                        Case "save"
                            InfoJson("Type") = BackupTypes.World
                        Case "version"
                            InfoJson("Type") = BackupTypes.Version
                        Case "everything"
                            InfoJson("Type") = BackupTypes.Full
                        Case Else
                            InfoJson("Type") = BackupTypes.World
                    End Select

                    Using SW As New StreamWriter(Folder.FullName + "\info.json")
                        SW.Write(InfoJson)
                    End Using
                End If

                Select Case CInt(InfoJson("Type"))
                    Case BackupTypes.World
                        Type = MCBackup.Language.GetString("BackupTypes.Save")
                    Case BackupTypes.Version
                        Type = MCBackup.Language.GetString("BackupTypes.Version")
                    Case BackupTypes.Full
                        Type = MCBackup.Language.GetString("BackupTypes.Everything")
                End Select

                Dim BackupDateCreated As DateTime = GetFolderDateCreated(Directory.ToString & "\" & Folder.ToString)


                If Group = "" And Folder.Name.IndexOf(Search, 0, StringComparison.CurrentCultureIgnoreCase) <> -1 Then
                    If BackupDateCreated.AddDays(14) < DateTime.Today Then
                        Dispatcher.Invoke(Sub()
                                              Items.Add(New ListViewBackupItem(Folder.ToString, BackupDateCreated.ToString(MCBackup.Language.GetString("Localization.DefaultDateFormat"), CultureInfo.InvariantCulture), New SolidColorBrush(Color.FromRgb(My.Settings.ListViewTextColorIntensity, 0, 0)), InfoJson("OriginalName"), Type))
                                          End Sub)
                    ElseIf BackupDateCreated.AddDays(7) < DateTime.Today Then
                        Dispatcher.Invoke(Sub()
                                              Items.Add(New ListViewBackupItem(Folder.ToString, BackupDateCreated.ToString(MCBackup.Language.GetString("Localization.DefaultDateFormat"), CultureInfo.InvariantCulture), New SolidColorBrush(Color.FromRgb(My.Settings.ListViewTextColorIntensity, My.Settings.ListViewTextColorIntensity, 0)), InfoJson("OriginalName"), Type))
                                          End Sub)
                    Else
                        Dispatcher.Invoke(Sub()
                                              Items.Add(New ListViewBackupItem(Folder.ToString, BackupDateCreated.ToString(MCBackup.Language.GetString("Localization.DefaultDateFormat"), CultureInfo.InvariantCulture), New SolidColorBrush(Color.FromRgb(0, My.Settings.ListViewTextColorIntensity, 0)), InfoJson("OriginalName"), Type))
                                          End Sub)
                    End If
                ElseIf InfoJson("Group") = Group And Folder.Name.IndexOf(Search, 0, StringComparison.CurrentCultureIgnoreCase) <> -1 Then
                    If BackupDateCreated.AddDays(14) < DateTime.Today Then
                        Dispatcher.Invoke(Sub()
                                              Items.Add(New ListViewBackupItem(Folder.ToString, BackupDateCreated.ToString(MCBackup.Language.GetString("Localization.DefaultDateFormat"), CultureInfo.InvariantCulture), New SolidColorBrush(Color.FromRgb(My.Settings.ListViewTextColorIntensity, 0, 0)), InfoJson("OriginalName"), Type))
                                          End Sub)
                    ElseIf BackupDateCreated.AddDays(7) < DateTime.Today Then
                        Dispatcher.Invoke(Sub()
                                              Items.Add(New ListViewBackupItem(Folder.ToString, BackupDateCreated.ToString(MCBackup.Language.GetString("Localization.DefaultDateFormat"), CultureInfo.InvariantCulture), New SolidColorBrush(Color.FromRgb(My.Settings.ListViewTextColorIntensity, My.Settings.ListViewTextColorIntensity, 0)), InfoJson("OriginalName"), Type))
                                          End Sub)
                    Else
                        Dispatcher.Invoke(Sub()
                                              Items.Add(New ListViewBackupItem(Folder.ToString, BackupDateCreated.ToString(MCBackup.Language.GetString("Localization.DefaultDateFormat"), CultureInfo.InvariantCulture), New SolidColorBrush(Color.FromRgb(0, My.Settings.ListViewTextColorIntensity, 0)), InfoJson("OriginalName"), Type))
                                          End Sub)
                    End If
                End If
            Catch ex As Exception
                Log.Print("An error occured during the backup: " & ex.Message, Log.Level.Severe)
            End Try
        Next

        If DirectoriesToDelete.Count > 0 Then
            Dim SB As New StringBuilder
            For Each Folder As String In DirectoriesToDelete
                SB.Append(vbNewLine & "> " & Folder)
            Next
            Dim Result As MessageBoxResult
            Dispatcher.Invoke(Sub()
                                  Result = MetroMessageBox.Show(MCBackup.Language.GetString("Message.DeleteInvalidBackups", SB.ToString), MCBackup.Language.GetString("Message.Caption.InvalidBackups"), MessageBoxButton.YesNo, MessageBoxImage.Question)
                              End Sub)
            If Result = MessageBoxResult.Yes Then
                Dispatcher.Invoke(Sub()
                                      Progress.IsIndeterminate = False
                                      EnableUI(False)
                                      StartDelete(DirectoriesToDelete)
                                  End Sub)
            End If
        End If
    End Sub

    Private Sub BGW_RunWorkerCompleted() Handles BGW.RunWorkerCompleted
        ListView.ItemsSource = Items
        ListView.SelectedIndex = -1
        SidebarTitle.Text = MCBackup.Language.GetString("MainWindow.Sidebar.NumberElements", Items.Count)

        If ListView.Items.Count = 0 Then
            NoBackupsOverlay.Visibility = Visibility.Visible
        Else
            NoBackupsOverlay.Visibility = Visibility.Collapsed
        End If

        Select Case My.Settings.ListViewGroupBy
            Case BackupsListView.GroupBy.OriginalName
                ListViewGroupByNameItem_Click(Nothing, Nothing)
            Case BackupsListView.GroupBy.Type
                ListViewGroupByTypeItem_Click(Nothing, Nothing)
            Case Else
                ListViewGroupByNothingItem_Click(Nothing, Nothing)
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
        StatusLabel.Content = MCBackup.Language.GetString("Status.Ready")
        Me.Title = String.Format("MCBackup {0}", ApplicationVersion)
        EnableUI(True)
    End Sub

    Private Sub ListView_SelectionChanged(sender As Object, e As EventArgs) Handles ListView.SelectionChanged
        Select Case ListView.SelectedItems.Count
            Case 0
                RestoreButton.IsEnabled = False
                RenameButton.IsEnabled = False ' Don't allow anything when no items are selected
                DeleteButton.IsEnabled = False

                SidebarTitle.Text = MCBackup.Language.GetString("MainWindow.Sidebar.NumberElements", ListView.Items.Count)        'Show total number of elements
                SidebarTitle.ToolTip = MCBackup.Language.GetString("MainWindow.Sidebar.NumberElements", ListView.Items.Count)

                ListViewRestoreItem.IsEnabled = False
                ListViewDeleteItem.IsEnabled = False         'Disable ContextMenu items
                ListViewRenameItem.IsEnabled = False
                ListViewMoveToGroupItem.IsEnabled = False

                ThumbnailImage.Source = New BitmapImage(New Uri("pack://application:,,,/Resources/nothumb.png"))
                SidebarOriginalNameContent.Text = "-"
                SidebarOriginalNameContent.ToolTip = MCBackup.Language.GetString("MainWindow.Sidebar.NoBackupSelected")
                SidebarTypeContent.Text = "-"
                SidebarTypeContent.ToolTip = MCBackup.Language.GetString("MainWindow.Sidebar.NoBackupSelected")

                DescriptionTextBox.Text = MCBackup.Language.GetString("MainWindow.Sidebar.Description.NoItem")

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

                SidebarTitle.Text = MCBackup.Language.GetString("MainWindow.Sidebar.NumberElementsSelected", ListView.SelectedItems.Count)   'Set sidebar title to number of selected items
                SidebarTitle.ToolTip = MCBackup.Language.GetString("MainWindow.Sidebar.NumberElementsSelected", ListView.SelectedItems.Count)

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
                                      Dispatcher.Invoke(Sub() ErrorReportDialog.Show("An error occured while trying to load the backup's thumbnail", ex))
                                  End Try
                              Else
                                  ThumbnailImage.Source = New BitmapImage(New Uri("pack://application:,,,/Resources/nothumb.png"))
                              End If
                          End Sub)

        Dim Type As BackupTypes = BackupTypes.World, OriginalFolderName As String = "-", Description As String = ""

        Try
            Dim InfoJson As JObject
            Using SR As New StreamReader(My.Settings.BackupsFolderLocation & "\" & SelectedItem.Name & "\info.json")
                InfoJson = JsonConvert.DeserializeObject(SR.ReadToEnd)
            End Using
            Type = CInt(InfoJson("Type"))
            OriginalFolderName = InfoJson("OriginalName")
            Description = InfoJson("Description")
        Catch ex As Exception
            Log.Print("Could not read info.json! " + ex.Message, Log.Level.Severe)
        End Try

        Dispatcher.Invoke(Sub()
                              SidebarOriginalNameContent.Text = OriginalFolderName
                              SidebarOriginalNameContent.ToolTip = OriginalFolderName

                              Select Case Type
                                  Case BackupTypes.World
                                      SidebarTypeContent.Text = MCBackup.Language.GetString("BackupTypes.Save")
                                      SidebarTypeContent.ToolTip = MCBackup.Language.GetString("BackupTypes.Save")
                                  Case BackupTypes.Version
                                      SidebarTypeContent.Text = MCBackup.Language.GetString("BackupTypes.Version")
                                      SidebarTypeContent.ToolTip = MCBackup.Language.GetString("BackupTypes.Version")
                                  Case BackupTypes.Full
                                      SidebarTypeContent.Text = MCBackup.Language.GetString("BackupTypes.Everything")
                                      SidebarTypeContent.ToolTip = MCBackup.Language.GetString("BackupTypes.Everything")
                              End Select

                              DescriptionTextBox.Text = IIf(String.IsNullOrEmpty(Description), MCBackup.Language.GetString("MainWindow.Sidebar.Description.NoDesc"), Description)
                          End Sub)

        If Type = BackupTypes.World AndAlso SelectedItem IsNot Nothing AndAlso File.Exists(Path.Combine(My.Settings.BackupsFolderLocation, SelectedItem.Name, "level.dat")) Then
            Dispatcher.Invoke(Sub()
                                  SidebarPlayerHealthGrid.Children.Clear()
                                  SidebarPlayerHungerGrid.Children.Clear()

                                  Try
                                      Dim World As NbtWorld = NbtWorld.Open(My.Settings.BackupsFolderLocation & "\" & SelectedItem.Name)

                                      ' TODO: find a way to read from playername.dat file
                                      If World IsNot Nothing AndAlso World.Level IsNot Nothing AndAlso World.Level.Player IsNot Nothing Then
                                          SidebarPlayerHealthGrid.ToolTip = World.Level.Player.Health.ToString() + "/20"
                                          SidebarPlayerHungerGrid.ToolTip = World.Level.Player.HungerLevel.ToString() + "/20"

                                          For i As Integer = 0 To World.Level.Player.Health \ 2 - 1
                                              SidebarPlayerHealthGrid.Children.Add(New Game.Images.Health(New Thickness(SidebarPlayerHealthGrid.Children.Count * 10, 0, 0, 0), Game.Images.State.Full))
                                          Next
                                          If World.Level.Player.Health Mod 2 <> 0 Then
                                              SidebarPlayerHealthGrid.Children.Add(New Game.Images.Health(New Thickness(SidebarPlayerHealthGrid.Children.Count * 10, 0, 0, 0), Game.Images.State.Half))
                                          End If
                                          For i As Integer = 0 To (20 - World.Level.Player.Health) \ 2 - 1
                                              SidebarPlayerHealthGrid.Children.Add(New Game.Images.Health(New Thickness(SidebarPlayerHealthGrid.Children.Count * 10, 0, 0, 0), Game.Images.State.Empty))
                                          Next

                                          For i As Integer = 0 To World.Level.Player.HungerLevel \ 2 - 1
                                              SidebarPlayerHungerGrid.Children.Add(New Game.Images.Hunger(New Thickness(90 - SidebarPlayerHungerGrid.Children.Count * 10, 0, 0, 0), Game.Images.State.Full))
                                          Next
                                          If World.Level.Player.HungerLevel Mod 2 <> 0 Then
                                              SidebarPlayerHungerGrid.Children.Add(New Game.Images.Hunger(New Thickness(90 - SidebarPlayerHungerGrid.Children.Count * 10, 0, 0, 0), Game.Images.State.Half))
                                          End If
                                          For i As Integer = 0 To (20 - World.Level.Player.HungerLevel) \ 2 - 1
                                              SidebarPlayerHungerGrid.Children.Add(New Game.Images.Hunger(New Thickness(90 - SidebarPlayerHungerGrid.Children.Count * 10, 0, 0, 0), Game.Images.State.Empty))
                                          Next

                                          SidebarPlayerHealth.Visibility = Visibility.Visible
                                          SidebarPlayerHunger.Visibility = Visibility.Visible
                                      End If
                                  Catch ex As Exception
                                      Dispatcher.Invoke(Sub() ErrorReportDialog.Show("An error occured while trying to load world info.", ex))
                                  End Try
                              End Sub)
        Else
            Dispatcher.Invoke(Sub()
                                  SidebarPlayerHealth.Visibility = Visibility.Collapsed
                                  SidebarPlayerHunger.Visibility = Visibility.Collapsed
                              End Sub)
        End If
    End Sub

    Public Sub LoadLanguage()
        Try
            BackupButton.Content = MCBackup.Language.GetString("MainWindow.BackupButton.Content")
            RestoreButton.Content = MCBackup.Language.GetString("MainWindow.RestoreButton.Content")
            DeleteButton.Content = MCBackup.Language.GetString("MainWindow.DeleteButton.Content")
            RenameButton.Content = MCBackup.Language.GetString("MainWindow.RenameButton.Content")
            CullButton.Content = MCBackup.Language.GetString("MainWindow.CullButton.Content")
            ListViewMoveToGroupItem.Header = MCBackup.Language.GetString("MainWindow.MoveToGroupButton.Text")
            AutomaticBackupButton.Content = MCBackup.Language.GetString("MainWindow.AutomaticBackupButton.Content") & IIf(AutoBackupWindow.IsVisible, " <<", " >>")

            NameColumnHeader.Content = MCBackup.Language.GetString("MainWindow.ListView.Columns(0).Header")
            DateCreatedColumnHeader.Content = MCBackup.Language.GetString("MainWindow.ListView.Columns(1).Header")
            TypeColumnHeader.Content = MCBackup.Language.GetString("MainWindow.ListView.Columns(2).Header")

            SidebarOriginalNameLabel.Text = MCBackup.Language.GetString("MainWindow.Sidebar.OriginalNameLabel.Text")
            SidebarTypeLabel.Text = MCBackup.Language.GetString("MainWindow.Sidebar.TypeLabel.Text")
            SidebarDescriptionLabel.Text = MCBackup.Language.GetString("MainWindow.Sidebar.DescriptionLabel.Text")
            SidebarPlayerHealthLabel.Text = MCBackup.Language.GetString("MainWindow.Sidebar.PlayerHealthLabel.Text")
            SidebarPlayerHungerLabel.Text = MCBackup.Language.GetString("MainWindow.Sidebar.PlayerHungerLabel.Text")

            FileToolbarButton.Content = MCBackup.Language.GetString("MainWindow.Toolbar.FileButton.Text")
            FileContextMenu.Items(0).Header = MCBackup.Language.GetString("MainWindow.Toolbar.FileContextMenu.Items(0).Header")
            EditToolbarButton.Content = MCBackup.Language.GetString("MainWindow.Toolbar.EditButton.Text")
            EditContextMenu.Items(0).Header = MCBackup.Language.GetString("MainWindow.Toolbar.EditContextMenu.Items(0).Header")
            EditContextMenu.Items(1).Header = MCBackup.Language.GetString("MainWindow.Toolbar.EditContextMenu.Items(1).Header")
            ToolsToolbarButton.Content = MCBackup.Language.GetString("MainWindow.Toolbar.ToolsButton.Text")
            ToolsContextMenu.Items(0).Header = MCBackup.Language.GetString("MainWindow.Toolbar.ToolsContextMenu.Items(0).Header")
            HelpToolbarButton.Content = MCBackup.Language.GetString("MainWindow.Toolbar.HelpButton.Text")
            HelpContextMenu.Items(0).Header = MCBackup.Language.GetString("MainWindow.Toolbar.HelpContextMenu.Items(0).Header")
            HelpContextMenu.Items(2).Header = MCBackup.Language.GetString("MainWindow.Toolbar.HelpContextMenu.Items(2).Header")
            HelpContextMenu.Items(3).Header = MCBackup.Language.GetString("MainWindow.Toolbar.HelpContextMenu.Items(3).Header")

            StatusLabel.Content = MCBackup.Language.GetString("Status.Ready")
            Me.Title = String.Format("MCBackup {0}", ApplicationVersion)

            SearchTextBox.Text = MCBackup.Language.GetString("MainWindow.Search")
            SearchTextBox.Foreground = New SolidColorBrush(Colors.Gray)

            ListViewSortByItem.Header = MCBackup.Language.GetString("MainWindow.ListView.ContextMenu.SortBy")
            ListViewSortByNameItem.Header = MCBackup.Language.GetString("MainWindow.ListView.ContextMenu.SortBy.Name")
            ListViewSortByDateCreatedItem.Header = MCBackup.Language.GetString("MainWindow.ListView.ContextMenu.SortBy.DateCreated")
            ListViewSortByTypeItem.Header = MCBackup.Language.GetString("MainWindow.ListView.ContextMenu.SortBy.Type")
            ListViewSortAscendingItem.Header = MCBackup.Language.GetString("MainWindow.ListView.ContextMenu.SortBy.Ascending")
            ListViewSortDescendingItem.Header = MCBackup.Language.GetString("MainWindow.ListView.ContextMenu.SortBy.Descending")

            ListViewGroupByItem.Header = MCBackup.Language.GetString("MainWindow.ListView.ContextMenu.GroupBy")
            ListViewGroupByNameItem.Header = MCBackup.Language.GetString("MainWindow.ListView.ContextMenu.GroupBy.OriginalName")
            ListViewGroupByTypeItem.Header = MCBackup.Language.GetString("MainWindow.ListView.ContextMenu.GroupBy.Type")
            ListViewGroupByNothingItem.Header = MCBackup.Language.GetString("MainWindow.ListView.ContextMenu.GroupBy.Nothing")

            ListViewRestoreItem.Header = MCBackup.Language.GetString("MainWindow.RestoreButton.Content")
            ListViewDeleteItem.Header = MCBackup.Language.GetString("MainWindow.DeleteButton.Content")
            ListViewRenameItem.Header = MCBackup.Language.GetString("MainWindow.RenameButton.Content")

            ListViewOpenInExplorerItem.Header = MCBackup.Language.GetString("MainWindow.ListView.ContextMenu.OpenInExplorer")

            NoBackupsOverlay.Text = MCBackup.Language.GetString("MainWindow.NoBackupsOverlay.Text")

            CancelButton.Content = MCBackup.Language.GetString("MainWindow.CancelButton.Text")
        Catch ex As Exception
            Dispatcher.Invoke(Sub() ErrorReportDialog.Show("Could not load language.", ex))
        End Try
    End Sub

    Public Sub ReloadBackupGroups()
        GroupsTabControl.Items.Clear()
        GroupsTabControl.Items.Add(New TaggedTabItem(MCBackup.Language.GetString("MainWindow.Groups.All"), ""))
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

    Public Sub StartBackup()
        ' Disable UI buttons
        EnableUI(False)

        ' Start backup using BackupManager
        Manager.BackupAsync(BackupInfo.Name, BackupInfo.Location, BackupInfo.Type, BackupInfo.Description, BackupInfo.Group, BackupInfo.Launcher, BackupInfo.Modpack)
    End Sub

    Private Sub BackupManager_BackupProgressChanged(sender As Object, e As BackupProgressChangedEventArgs)
        Progress.Maximum = 100

        ' Report progress depending on status
        Select Case e.Status
            Case BackupStatus.Starting

                Progress.IsIndeterminate = True
                Progress.Value = 0

                StatusLabel.Content = MCBackup.Language.GetString("Status.StartingBackup")

            Case BackupStatus.Running

                ' Set progress style to indeterminate and value to current progress percentage
                Progress.IsIndeterminate = False
                Progress.Value = e.ProgressPercentage

                ' Set status label & window title text to reflect status & progress
                StatusLabel.Content = MCBackup.Language.GetString("Status.BackingUp", e.ProgressPercentage, IIf(Single.IsNaN(e.TransferRate), 0, e.TransferRate / 1048576), Manager.EstimatedTimeSpanToString(e.EstimatedTimeRemaining))
                Me.Title = "MCBackup " + ApplicationVersion + " - " + MCBackup.Language.GetString("MainWindow.Title.Backup", ApplicationVersion, e.ProgressPercentage)

            Case BackupStatus.RevertingChanges

                ' Set progress style to indeterminate and value to current progress percentage
                Progress.IsIndeterminate = True
                Progress.Value = 0

                ' Set status label & window title text to reflect status
                StatusLabel.Content = MCBackup.Language.GetString("Status.RevertingChanges")
                Me.Title = "MCBackup " + ApplicationVersion + " - " + MCBackup.Language.GetString("MainWindow.Title.RevertingChanges", ApplicationVersion)

            Case BackupStatus.CreatingThumbnail

                ' Set progress style to indeterminate and value to current progress percentage
                Progress.IsIndeterminate = False
                Progress.Value = e.ProgressPercentage

                ' Set status label & window title text to reflect status & progress
                StatusLabel.Content = MCBackup.Language.GetString("Status.CreatingThumb", e.ProgressPercentage)
                Me.Title = "MCBackup " + ApplicationVersion + " - " + MCBackup.Language.GetString("MainWindow.Title.CreatingThumb", ApplicationVersion, e.ProgressPercentage)

        End Select
    End Sub

    Private Sub BackupManager_BackupCompleted(sender As Object, e As BackupCompletedEventArgs)
        ProgressBar.Value = 100

        ' Check if an error occured
        If e.Error IsNot Nothing Then

            ' Show error balloon tip
            If My.Settings.ShowBalloonTips Then NotifyIcon.ShowBalloonTip(2000, MCBackup.Language.GetString("BalloonTip.Title.BackupError"), MCBackup.Language.GetString("BalloonTip.BackupError"), System.Windows.Forms.ToolTipIcon.Error)

            ' Show error report dialog
            ErrorReportDialog.Show(MCBackup.Language.GetString("Exception.Backup"), e.Error)

        Else

            ' Show backup completed balloon tip
            If My.Settings.ShowBalloonTips Then NotifyIcon.ShowBalloonTip(2000, MCBackup.Language.GetString("BalloonTip.Title.BackupComplete"), MCBackup.Language.GetString("BalloonTip.BackupComplete"), System.Windows.Forms.ToolTipIcon.Info)

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
        RefreshBackupsList()

        ' Set status to ready
        StatusLabel.Content = MCBackup.Language.GetString("Status.Ready")
        Me.Title = "MCBackup " + ApplicationVersion

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
        If MetroMessageBox.Show(MCBackup.Language.GetString("Message.RestoreAreYouSure"), MCBackup.Language.GetString("Message.Caption.AreYouSure"), MessageBoxButton.YesNo, MessageBoxImage.Question) = Forms.DialogResult.Yes Then
            EnableUI(False)
            'Cancel = False
            Log.Print("Starting Restore")
            RestoreInfo.BackupName = ListView.SelectedItems(0).Name

            ListView.SelectedIndex = -1

            Dim BaseFolderName As String = "",
                Launcher As Game.Launcher = Game.Launcher.Minecraft,
                Modpack As String = "",
                InfoJson As JObject

            If Not File.Exists(My.Settings.BackupsFolderLocation & "\" & RestoreInfo.BackupName & "\info.json") Then
                MetroMessageBox.Show(MCBackup.Language.GetString(""))
                Exit Sub
            End If

            Using SR As New StreamReader(My.Settings.BackupsFolderLocation & "\" & RestoreInfo.BackupName & "\info.json")
                InfoJson = JsonConvert.DeserializeObject(SR.ReadToEnd)
                BaseFolderName = InfoJson("OriginalName")
                RestoreInfo.BackupType = CInt(InfoJson("Type"))

                Dim Temp As Object = InfoJson("Launcher")
                If IsNumeric(Temp) Then
                    If Temp > [Enum].GetValues(GetType(Game.Launcher)).Cast(Of Game.Launcher).Last() Or Temp < 0 Then
                        Launcher = Game.Launcher.Minecraft
                    Else
                        Launcher = Temp
                    End If
                ElseIf Not String.IsNullOrEmpty(Temp)
                    Select Case Temp.ToString().ToLower()
                        Case "minecraft"
                            Launcher = Game.Launcher.Minecraft
                        Case "technic"
                            Launcher = Game.Launcher.Technic
                        Case "ftb"
                            Launcher = Game.Launcher.FeedTheBeast
                        Case "feedthebeast"
                            Launcher = Game.Launcher.FeedTheBeast
                        Case "atlauncher"
                            Launcher = Game.Launcher.ATLauncher
                        Case Else
                            Launcher = Game.Launcher.Minecraft
                    End Select
                Else
                    Launcher = Game.Launcher.Minecraft
                End If

                Modpack = InfoJson("Modpack")
            End Using

            If Launcher <> My.Settings.Launcher Then
                MetroMessageBox.Show(MCBackup.Language.GetString("Message.IncompatibleBackupConfig", Launcher.GetStringValue()), MCBackup.Language.GetString("Message.Caption.Error"), MessageBoxButton.OK, MessageBoxImage.Error)
                EnableUI(True)
                Exit Sub
            End If

            Select Case RestoreInfo.BackupType
                Case BackupTypes.World
                    Select Case My.Settings.Launcher
                        Case Game.Launcher.Minecraft
                            RestoreInfo.RestoreLocation = My.Settings.SavesFolderLocation & "\" & BaseFolderName
                        Case Game.Launcher.Technic
                            RestoreInfo.RestoreLocation = My.Settings.MinecraftFolderLocation & "\modpacks\" & Modpack & "\saves\" & BaseFolderName
                        Case Game.Launcher.FeedTheBeast
                            RestoreInfo.RestoreLocation = My.Settings.MinecraftFolderLocation & "\" & Modpack & "\minecraft\saves\" & BaseFolderName
                        Case Game.Launcher.ATLauncher
                            RestoreInfo.RestoreLocation = My.Settings.MinecraftFolderLocation & "\Instances\" & Modpack & "\saves\" & BaseFolderName
                    End Select
                Case BackupTypes.Version
                    Select Case My.Settings.Launcher
                        Case Game.Launcher.Minecraft
                            RestoreInfo.RestoreLocation = My.Settings.MinecraftFolderLocation & "\versions\" & BaseFolderName
                        Case Game.Launcher.Technic
                            RestoreInfo.RestoreLocation = My.Settings.MinecraftFolderLocation & "\modpacks\" & BaseFolderName
                        Case Game.Launcher.FeedTheBeast
                            RestoreInfo.RestoreLocation = My.Settings.MinecraftFolderLocation & "\" & BaseFolderName
                        Case Game.Launcher.ATLauncher
                            RestoreInfo.RestoreLocation = My.Settings.MinecraftFolderLocation & "\Instances\" & BaseFolderName
                    End Select
                Case BackupTypes.Full
                    RestoreInfo.RestoreLocation = My.Settings.MinecraftFolderLocation
            End Select

            Manager.RestoreAsync(RestoreInfo.BackupName, RestoreInfo.RestoreLocation, RestoreInfo.BackupType)
        End If
    End Sub

    Private Sub BackupManager_RestoreProgressChanged(sender As Object, e As RestoreProgressChangedEventArgs)

        Progress.Maximum = 100

        Select Case e.RestoreStatus

            Case RestoreStatus.Starting

                Progress.IsIndeterminate = True
                Progress.Value = 0

                StatusLabel.Content = MCBackup.Language.GetString("Status.StartingRestore")

            Case RestoreStatus.RemovingOldFiles

                Progress.IsIndeterminate = False
                Progress.Value = e.ProgressPercentage

                StatusLabel.Content = MCBackup.Language.GetString("Status.RemovingOldContent", e.ProgressPercentage)
                Me.Title = "MCBackup {0} - " + MCBackup.Language.GetString("MainWindow.Title.RemovingOldContent", ApplicationVersion, e.ProgressPercentage)

            Case RestoreStatus.Restoring

                Progress.IsIndeterminate = False
                Progress.Value = e.ProgressPercentage

                StatusLabel.Content = MCBackup.Language.GetString("Status.Restoring", e.ProgressPercentage, e.TransferRate / 1048576, Manager.EstimatedTimeSpanToString(e.EstimatedTimeRemaining))
                Me.Title = "MCBackup {0} - " + MCBackup.Language.GetString("MainWindow.Title.Restore", ApplicationVersion, e.ProgressPercentage)

        End Select

    End Sub

    Private Sub BackupManager_RestoreCompleted(sender As Object, e As RestoreCompletedEventArgs)

        If e.Error IsNot Nothing Then

            ErrorReportDialog.Show(e.Error.Message, e.Error)

        End If

        StatusLabel.Content = MCBackup.Language.GetString("Status.Ready")
        Me.Title = String.Format("MCBackup {0}", ApplicationVersion)
        Progress.Maximum = 100
        Progress.Value = 100
        EnableUI(True)
        RefreshBackupsList()
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
            Dispatcher.Invoke(Sub() ErrorReportDialog.Show(String.Format("Could not find size of '{0}'", FolderPath), ex))

            Return 0
        End Try
    End Function

    Public Function GetFolderDateCreated(FolderPath As String) As DateTime
        Try
            Dim FSO As New Scripting.FileSystemObject
            Return FSO.GetFolder(FolderPath).DateCreated ' Get FolderPath's date of creation
        Catch ex As Exception
            Dispatcher.Invoke(Sub() ErrorReportDialog.Show(String.Format("Could not find creation date of '{0}'", FolderPath), ex))
        End Try
        Return Nothing
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
            Dispatcher.Invoke(Sub() ErrorReportDialog.Show(String.Format("Could not convert source {0} to bitmap:", Source), ex))
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
        Dim OptionsWindow As New Options
        OptionsWindow.Owner = Me
        OptionsWindow.ShowDialog()
    End Sub

    Private Sub BackupsFolderMenuItem_Click(sender As Object, e As RoutedEventArgs)
        Process.Start(My.Settings.BackupsFolderLocation)
    End Sub

    Private Sub WebsiteMenuItem_Click(sender As Object, e As RoutedEventArgs)
        Process.Start("http://go.nicoco007.com/fwlink/?LinkID=1000")
    End Sub

    Private Sub AboutMenuItem_Click(sender As Object, e As RoutedEventArgs)
        Dim AboutDialog As New AboutDialog
        AboutDialog.Owner = Me
        AboutDialog.ShowDialog()
    End Sub

    Private Sub ReportBugMenuItem_Click(sender As Object, e As RoutedEventArgs)
        Process.Start("http://go.nicoco007.com/fwlink/?LinkID=5000")
    End Sub

    Private Sub RefreshBackupsList_Click(sender As Object, e As RoutedEventArgs)
        RefreshBackupsList()
    End Sub
#End Region

#Region "Delete"
    Private Sub DeleteButton_Click(sender As Object, e As EventArgs) Handles DeleteButton.Click, ListViewDeleteItem.Click
        If My.Settings.ShowDeleteDialog Then
            If DeleteDialog.Show(Me) = Windows.Forms.DialogResult.Yes Then
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
                                  StatusLabel.Content = MCBackup.Language.GetString("Status.Deleting", 100 - PercentComplete)
                                  Me.Title = "MCBackup " + ApplicationVersion + " - " & MCBackup.Language.GetString("MainWindow.Title.Delete", 100 - PercentComplete)
                                  MCBackup.Progress.Value = CurrentSize
                              End Sub)
            Thread.Sleep(200)
        Loop

        Dispatcher.Invoke(Sub()
                              StatusLabel.Content = MCBackup.Language.GetString("Status.Ready")
                              Me.Title = "MCBackup " + ApplicationVersion
                              MCBackup.Progress.Value = 0
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
                Log.Print("Delete thread aborted!", Log.Level.Severe)
                Me.Dispatcher.Invoke(Sub()
                                         Progress.Value = 0
                                         StatusLabel.Content = MCBackup.Language.GetString("Status.CanceledAndReady")
                                         Me.Title = String.Format("MCBackup {0}", ApplicationVersion)
                                     End Sub)
            Else
                Dispatcher.Invoke(Sub() ErrorReportDialog.Show(MCBackup.Language.GetString("Exception.Delete"), ex))
            End If
        End Try

        Log.Print("Done.")
    End Sub

    Private Sub DeleteBackgroundWorker_RunWorkerCompleted()
        EnableUI(True)
        RefreshBackupsList()
        ReloadBackupGroups()
        StatusLabel.Content = MCBackup.Language.GetString("Status.Ready")
        Me.Title = String.Format("MCBackup {0}", ApplicationVersion)
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
            AutomaticBackupButton.Content = MCBackup.Language.GetString("MainWindow.AutomaticBackupButton.Content") & " >>"
            AdjustBackground()
        Else
            Me.Left = Me.Left - (AutoBackupWindow.Width / 2)
            AutomaticBackupButton.Content = MCBackup.Language.GetString("MainWindow.AutomaticBackupButton.Content") & " <<"
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

    Private Sub ListViewGroupByNameItem_Click(sender As Object, e As RoutedEventArgs) Handles ListViewGroupByNameItem.Click
        ListViewGroupByNameItem.IsChecked = True
        ListViewGroupByTypeItem.IsChecked = False
        ListViewGroupByNothingItem.IsChecked = False
        Dim View As CollectionView = DirectCast(CollectionViewSource.GetDefaultView(ListView.ItemsSource), CollectionView)
        View.GroupDescriptions.Clear()
        View.GroupDescriptions.Add(New PropertyGroupDescription("OriginalName"))
        My.Settings.ListViewGroupBy = BackupsListView.GroupBy.OriginalName
    End Sub

    Private Sub ListViewGroupByTypeItem_Click(sender As Object, e As RoutedEventArgs) Handles ListViewGroupByTypeItem.Click
        ListViewGroupByNameItem.IsChecked = False
        ListViewGroupByTypeItem.IsChecked = True
        ListViewGroupByNothingItem.IsChecked = False
        Dim View As CollectionView = DirectCast(CollectionViewSource.GetDefaultView(ListView.ItemsSource), CollectionView)
        View.GroupDescriptions.Clear()
        View.GroupDescriptions.Add(New PropertyGroupDescription("Type"))
        My.Settings.ListViewGroupBy = BackupsListView.GroupBy.Type
    End Sub

    Private Sub ListViewGroupByNothingItem_Click(sender As Object, e As RoutedEventArgs) Handles ListViewGroupByNothingItem.Click
        ListViewGroupByNameItem.IsChecked = False
        ListViewGroupByTypeItem.IsChecked = False
        ListViewGroupByNothingItem.IsChecked = True
        Dim View As CollectionView = DirectCast(CollectionViewSource.GetDefaultView(ListView.ItemsSource), CollectionView)
        View.GroupDescriptions.Clear()
        My.Settings.ListViewGroupBy = BackupsListView.GroupBy.Nothing
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
                                NotifyIcon.ShowBalloonTip(2000, MCBackup.Language.GetString("BalloonTip.Title.RunningBackground"), MCBackup.Language.GetString("BalloonTip.RunningBackground"), System.Windows.Forms.ToolTipIcon.Info)
                                My.Settings.FirstCloseToTray = False
                            End If

                            Log.Print("Closing To tray")

                            My.Settings.CloseToTray = True

                            e.Cancel = True
                        Case Forms.DialogResult.No
                            If Manager.IsBusy Or ThreadIsNotNothingAndAlive(DeleteThread) Then
                                MetroMessageBox.Show(MCBackup.Language.GetString("Message.MCBackupIsWorking"), MCBackup.Language.GetString("Message.Caption.MCBackupIsWorking"), MessageBoxButton.OK, MessageBoxImage.Question)
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
                            NotifyIcon.ShowBalloonTip(2000, MCBackup.Language.GetString("BalloonTip.Title.RunningBackground"), MCBackup.Language.GetString("BalloonTip.RunningBackground"), System.Windows.Forms.ToolTipIcon.Info)
                            My.Settings.FirstCloseToTray = False
                        End If

                        Log.Print("Closing To tray")

                        e.Cancel = True
                    Else
                        If Manager.IsBusy Or ThreadIsNotNothingAndAlive(DeleteThread) Then
                            MetroMessageBox.Show(MCBackup.Language.GetString("Message.MCBackupIsWorking"), MCBackup.Language.GetString("Message.Caption.MCBackupIsWorking"), MessageBoxButton.OK, MessageBoxImage.Question)
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

            Log.Print("Someone Is closing me!")
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

    Private Sub SearchTextBox_KeyDown(sender As Object, e As KeyEventArgs) Handles SearchTextBox.KeyDown
        If e.Key = Key.Enter Then
            RefreshBackupsList()
            FilterTimer.Stop()
        End If
    End Sub

    Private Sub SearchTextBox_LostFocus(sender As Object, e As RoutedEventArgs) Handles SearchTextBox.LostFocus
        If String.IsNullOrEmpty(SearchTextBox.Text) Then
            PreventUpdate = True
            SearchTextBox.Text = MCBackup.Language.GetString("MainWindow.Search")
            SearchTextBox.Foreground = New SolidColorBrush(Colors.Gray)
            PreventUpdate = False
        End If
    End Sub

    Private Sub SearchTextBox_GotFocus(sender As Object, e As RoutedEventArgs) Handles SearchTextBox.GotFocus
        If SearchTextBox.Text = MCBackup.Language.GetString("MainWindow.Search") Then
            PreventUpdate = True
            SearchTextBox.Text = ""
            SearchTextBox.Foreground = New SolidColorBrush(Colors.Black)
            PreventUpdate = False
        End If
    End Sub

    Private Sub FilterTimer_Tick(sender As Object, e As EventArgs) Handles FilterTimer.Tick
        RefreshBackupsList()
        FilterTimer.Stop()
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

    Private Sub TabControl_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles GroupsTabControl.SelectionChanged
        RefreshBackupsList()
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
        Dim OptionsWindow As New Options
        OptionsWindow.Owner = Me
        OptionsWindow.ShowDialog(3)
    End Sub

    Private Sub CancelButton_Click(sender As Object, e As RoutedEventArgs) Handles CancelButton.Click
        Manager.Cancel()

        If DeleteThread IsNot Nothing Then
            If DeleteThread.IsAlive Then
                If MetroMessageBox.Show(MCBackup.Language.GetString("Message.CancelDelete"), MCBackup.Language.GetString("Message.Caption.AreYouSure"), MessageBoxButton.YesNo, MessageBoxImage.Exclamation) = MessageBoxResult.Yes Then
                    If DeleteThread.IsAlive Then
                        DeleteThread.Abort()
                    End If
                End If
            End If
        End If
    End Sub

    Private Sub EnableUI(IsEnabled As Boolean)
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

    Public Sub MoveToGroup(SelectedItems As List(Of ListViewBackupItem), Group As String)
        Me.Dispatcher.Invoke(Sub()
                                 EnableUI(False)
                                 StatusLabel.Content = MCBackup.Language.GetString("Status.MovingBackups")
                                 Me.Title = "MCBackup " + ApplicationVersion + " - " + MCBackup.Language.GetString("MainWindow.Title.MovingBackups", ApplicationVersion)
                                 Progress.IsIndeterminate = True
                             End Sub)

        For Each Item As ListViewBackupItem In SelectedItems
            Log.Print("Rewriting info.json file For backup '{0}'.", Log.Level.Info, Item.Name)

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
                Log.Print("An error occured while trying to rewrite JSON data: " & ex.Message, Log.Level.Severe)
            End Try
        Next

        Me.Dispatcher.Invoke(Sub()
                                 EnableUI(True)
                                 StatusLabel.Content = MCBackup.Language.GetString("Status.Ready")
                                 Me.Title = String.Format("MCBackup {0}", ApplicationVersion)
                                 Progress.IsIndeterminate = False
                                 RefreshBackupsList()
                             End Sub)
    End Sub

    Private Sub OpenInExplorerItem_Click(sender As Object, e As RoutedEventArgs) Handles ListViewOpenInExplorerItem.Click
        Process.Start(My.Settings.BackupsFolderLocation & "\" & ListView.SelectedItem.Name)
    End Sub

    Private Sub Window_StateChanged(sender As Object, e As EventArgs) Handles Window.StateChanged
        If Not Me.WindowState = Windows.WindowState.Maximized Then My.Settings.WindowSize = New Size(Me.Width, Me.Height)
    End Sub

    Public Shared Function GetBackupTimeStamp()
        Return DateTime.Now.ToString("yyyy-MM-dd (hh\hmm\mss\s)")
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