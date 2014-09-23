'   ╔═══════════════════════════════════════════════════════════════════════════╗
'   ║                        Copyright © 2014 nicoco007                         ║
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
Imports System.ComponentModel
Imports System.Windows.Interop.Imaging
Imports Microsoft.WindowsAPICodePack
Imports Microsoft.WindowsAPICodePack.Taskbar
Imports System.Globalization
Imports System.Threading
Imports System.Windows.Interop
Imports Substrate
Imports MahApps.Metro
Imports System.Text
Imports Newtonsoft.Json.Linq
Imports Newtonsoft.Json
Imports System.Security

Partial Class MainWindow

#Region "Variables"
    Private AppData As String = Environ("APPDATA")

    ' BackupInfo(0) = Backup name
    ' BackupInfo(1) = Backup description
    ' BackupInfo(2) = Location
    ' BackupInfo(3) = Backup type
    ' BackupInfo(4) = Backup group
    ' BackupInfo(5) = Launcher
    ' BackupInfo(6) = Modpack
    Public BackupInfo(6) As String
    Public RestoreInfo(2) As String

    Private FolderBrowserDialog As New System.Windows.Forms.FolderBrowserDialog

    Private BackupThread As Thread
    Private DeleteForRestoreThread As Thread
    Private RestoreThread As Thread
    Private DeleteThread As Thread

    Public StartupPath As String = Directory.GetCurrentDirectory()
    Public ApplicationVersion As String = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()
    Public LatestVersion As String

    Public WithEvents NotifyIcon As New System.Windows.Forms.NotifyIcon

    Public AutoBackupWindow As New AutoBackupWindow
    Private Splash As New Splash

    Private Cancel As Boolean = False
#End Region

#Region "Load"
    Public Sub New()
        Application.CloseAction = Application.AppCloseAction.Force

        InitializeComponent()

        ThemeManager.ChangeAppStyle(My.Application, ThemeManager.GetAccent(My.Settings.Theme), ThemeManager.GetAppTheme("BaseLight"))

        Splash.Show()
        Splash.ShowStatus("Splash.Status.Starting", "Starting...")

        Log.SPrint("")
        Log.SPrint("---------- Starting MCBackup v{0} @ {1} ----------", ApplicationVersion, Log.DebugTimeStamp())
        Log.Print("OS Name: " & Log.GetWindowsVersion())
        Log.Print("OS Version: " & Environment.OSVersion.Version.Major & "." & Environment.OSVersion.Version.Minor)
        Log.Print("Architecture: " & Log.GetWindowsArch())
        Log.Print(".NET Framework Version: " & Environment.Version.Major & "." & Environment.Version.Minor)

        Log.Print(String.Format("Current Launcher: '{0}'", My.Settings.Launcher))

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
                MCBackup.Language.Load(My.Settings.Language & ".lang")
            Else
                MCBackup.Language.Load(My.Settings.Language & ".lang")
            End If
        Catch ex As Exception
            ErrorReportDialog.Show("Error: Could not load language file (" & My.Settings.Language & ")! MCBackup will now exit.", ex)
            My.Settings.Language = DefaultLanguage
            My.Settings.Save()
            Me.Close()
            Exit Sub
        End Try

        Splash.StepProgress()

        NotifyIcon.Text = "MCBackup " & ApplicationVersion
        NotifyIcon.Icon = New System.Drawing.Icon(Application.GetResourceStream(New Uri("pack://application:,,,/Resources/icon.ico")).Stream)
        Dim ContextMenu As New System.Windows.Forms.ContextMenu
        Dim ExitToolbarMenuItem As New System.Windows.Forms.MenuItem
        ExitToolbarMenuItem.Text = MCBackup.Language.FindString("NotifyIcon.ContextMenu.ExitItem.Text", My.Settings.Language & ".lang")
        AddHandler ExitToolbarMenuItem.Click, AddressOf ExitToolbarMenuItem_Click
        ContextMenu.MenuItems.Add(ExitToolbarMenuItem)
        NotifyIcon.ContextMenu = ContextMenu
        NotifyIcon.Visible = True

        Splash.StepProgress()

        Splash.ShowStatus("Splash.Status.LoadingProps", "Loading Properties...")

        Splash.StepProgress()

        Me.ListView.Background = New SolidColorBrush(Color.FromArgb(My.Settings.InterfaceOpacity * 2.55, 255, 255, 255))
        Me.Sidebar.Background = New SolidColorBrush(Color.FromArgb(My.Settings.InterfaceOpacity * 2.55, 255, 255, 255))

        StatusLabel.Foreground = New SolidColorBrush(My.Settings.StatusLabelColor)

        Splash.StepProgress()

        If Not My.Settings.BackgroundImageLocation = "" And My.Computer.FileSystem.FileExists(My.Settings.BackgroundImageLocation) Then
            Dim Brush As New ImageBrush(New BitmapImage(New Uri(My.Settings.BackgroundImageLocation)))
            Brush.Stretch = My.Settings.BackgroundImageStretch
            Me.Background = Brush
        End If

        Splash.StepProgress()

        Me.Width = My.Settings.WindowSize.Width
        Me.Height = My.Settings.WindowSize.Height

        If My.Settings.IsWindowMaximized Then Me.WindowState = WindowState.Maximized

        If My.Settings.BackupsFolderLocation = "" Then
            My.Settings.BackupsFolderLocation = StartupPath & "\backups"
        End If

        If Not My.Computer.FileSystem.DirectoryExists(My.Settings.BackupsFolderLocation) Then
            If MetroMessageBox.Show(MCBackup.Language.Dictionary("Message.BackupsFolderNotFound"), MCBackup.Language.Dictionary("Message.Caption.Error"), MessageBoxButton.OKCancel) = MessageBoxResult.OK Then
                My.Settings.BackupsFolderLocation = StartupPath & "\backups"
                My.Computer.FileSystem.CreateDirectory(My.Settings.BackupsFolderLocation)
            Else
                Me.Close()
                Exit Sub
            End If
        End If

        Log.Print("Set Backups folder location to '" & My.Settings.BackupsFolderLocation & "'")

        GridSidebarColumn.Width = New GridLength(My.Settings.SidebarWidth.Value, GridUnitType.Star)
        GridListViewColumn.Width = New GridLength(My.Settings.ListViewWidth.Value, GridUnitType.Star)

        Splash.ShowStatus("Splash.Status.FindingMinecraft", "Finding Minecraft...")
        Splash.StepProgress()

        If String.IsNullOrEmpty(My.Settings.MinecraftFolderLocation) Then
            If IO.File.Exists(AppData & "\.minecraft\launcher.jar") Then
                My.Settings.MinecraftFolderLocation = AppData & "\.minecraft"
            End If
        End If

        If Not Directory.Exists(My.Settings.MinecraftFolderLocation) Then
            If MetroMessageBox.Show(MCBackup.Language.Dictionary("Message.NoMinecraftInstallError"), MCBackup.Language.Dictionary("Message.Caption.Error"), MessageBoxButton.OKCancel, MessageBoxImage.Error) = MessageBoxResult.OK Then
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
    End Sub

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs) Handles MyBase.Loaded
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
                Log.Print("MCBackup is running in beta mode (version " & ApplicationVersion & ")!")
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
        If Not BGW.IsBusy Then
            If Dispatcher.CheckAccess Then
                Items = New List(Of ListViewBackupItem)
                EnableUI(False)
                ProgressBar.IsIndeterminate = True
                StatusLabel.Content = MCBackup.Language.Dictionary("Status.RefreshingBackupsList")
                BGW.RunWorkerAsync()
            Else
                Dispatcher.Invoke(Sub() RefreshBackupsList())
            End If
        End If
    End Sub

    Private Sub BGW_DoWork() Handles BGW.DoWork
        Dim Search As String = "", Group As String = ""
        Dispatcher.Invoke(Sub()
                              If Not SearchTextBox.Text = MCBackup.Language.Dictionary("MainWindow.Search") Then
                                  Search = SearchTextBox.Text
                              End If
                              Group = DirectCast(GroupsTabControl.SelectedItem, TaggedTabItem).Tag
                          End Sub)
        Dim Directory As New IO.DirectoryInfo(My.Settings.BackupsFolderLocation) ' Create a DirectoryInfo variable for the backups folder

        Dim DirectoriesToDelete As New ArrayList

        For Each Folder As DirectoryInfo In Directory.GetDirectories ' For each folder in the backups folder
            Dim Type As String = "[ERROR]"                  ' Create variables with default value [ERROR], in case one of the values doesn't exist

            Try
                If IO.File.Exists(Folder.FullName & "\info.mcb") Then ' Convert info.mcb to info.json
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

                    IO.File.Delete(Folder.FullName & "\info.mcb")
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

                Select Case InfoJson("Type")
                    Case "save"
                        Type = MCBackup.Language.Dictionary("BackupTypes.Save")
                    Case "version"
                        Type = MCBackup.Language.Dictionary("BackupTypes.Version")
                    Case "everything"
                        Type = MCBackup.Language.Dictionary("BackupTypes.Everything")
                End Select

                If Group = "" And Folder.Name.IndexOf(Search, 0, StringComparison.CurrentCultureIgnoreCase) <> -1 Then
                    If GetFolderDateCreated(Directory.ToString & "\" & Folder.ToString).AddDays(14) < DateTime.Today Then
                        Dispatcher.Invoke(Sub()
                                              Items.Add(New ListViewBackupItem(Folder.ToString, GetFolderDateCreated(Directory.ToString & "\" & Folder.ToString), New SolidColorBrush(Color.FromRgb(My.Settings.ListViewTextColorIntensity, 0, 0)), InfoJson("OriginalName"), Type))
                                          End Sub)
                    ElseIf GetFolderDateCreated(Directory.ToString & "\" & Folder.ToString).AddDays(7) < DateTime.Today Then
                        Dispatcher.Invoke(Sub()
                                              Items.Add(New ListViewBackupItem(Folder.ToString, GetFolderDateCreated(Directory.ToString & "\" & Folder.ToString), New SolidColorBrush(Color.FromRgb(My.Settings.ListViewTextColorIntensity, My.Settings.ListViewTextColorIntensity, 0)), InfoJson("OriginalName"), Type))
                                          End Sub)
                    Else
                        Dispatcher.Invoke(Sub()
                                              Items.Add(New ListViewBackupItem(Folder.ToString, GetFolderDateCreated(Directory.ToString & "\" & Folder.ToString), New SolidColorBrush(Color.FromRgb(0, My.Settings.ListViewTextColorIntensity, 0)), InfoJson("OriginalName"), Type))
                                          End Sub)
                    End If
                ElseIf InfoJson("Group") = Group And Not (Group = "") And Folder.Name.IndexOf(Search, 0, StringComparison.CurrentCultureIgnoreCase) <> -1 Then
                    If GetFolderDateCreated(Directory.ToString & "\" & Folder.ToString).AddDays(14) < DateTime.Today Then
                        Dispatcher.Invoke(Sub()
                                              Items.Add(New ListViewBackupItem(Folder.ToString, GetFolderDateCreated(Directory.ToString & "\" & Folder.ToString), New SolidColorBrush(Color.FromRgb(My.Settings.ListViewTextColorIntensity, 0, 0)), InfoJson("OriginalName"), Type))
                                          End Sub)
                    ElseIf GetFolderDateCreated(Directory.ToString & "\" & Folder.ToString).AddDays(7) < DateTime.Today Then
                        Dispatcher.Invoke(Sub()
                                              Items.Add(New ListViewBackupItem(Folder.ToString, GetFolderDateCreated(Directory.ToString & "\" & Folder.ToString), New SolidColorBrush(Color.FromRgb(My.Settings.ListViewTextColorIntensity, My.Settings.ListViewTextColorIntensity, 0)), InfoJson("OriginalName"), Type))
                                          End Sub)
                    Else
                        Dispatcher.Invoke(Sub()
                                              Items.Add(New ListViewBackupItem(Folder.ToString, GetFolderDateCreated(Directory.ToString & "\" & Folder.ToString), New SolidColorBrush(Color.FromRgb(0, My.Settings.ListViewTextColorIntensity, 0)), InfoJson("OriginalName"), Type))
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
                                  Result = MetroMessageBox.Show(String.Format(MCBackup.Language.Dictionary("Message.DeleteInvalidBackups"), SB.ToString), MCBackup.Language.Dictionary("Message.Caption.InvalidBackups"), MessageBoxButton.YesNo, MessageBoxImage.Question)
                              End Sub)
            If Result = MessageBoxResult.Yes Then
                Dispatcher.Invoke(Sub()
                                      Delete(DirectoriesToDelete)
                                  End Sub)
            End If
        End If
    End Sub

    Private Sub BGW_RunWorkerCompleted() Handles BGW.RunWorkerCompleted
        ListView.ItemsSource = Items
        ListView.SelectedIndex = -1
        SidebarTitle.Text = String.Format(MCBackup.Language.Dictionary("MainWindow.Sidebar.NumberElements"), Items.Count)

        If ListView.Items.Count = 0 Then
            NoBackupsOverlay.Visibility = Visibility.Visible
        Else
            NoBackupsOverlay.Visibility = Visibility.Collapsed
        End If

        Select Case My.Settings.ListViewGroupBy
            Case "OriginalName"
                ListViewGroupByNameItem_Click(Nothing, Nothing)
            Case "Type"
                ListViewGroupByTypeItem_Click(Nothing, Nothing)
            Case "Nothing"
                ListViewGroupByNothingItem_Click(Nothing, Nothing)
            Case Else
                ListViewGroupByNothingItem_Click(Nothing, Nothing)
        End Select

        Select Case My.Settings.ListViewSortBy
            Case "Name"
                ListViewSortByNameItem_Click(Nothing, Nothing)
            Case "DateCreated"
                ListViewSortByDateCreatedItem_Click(Nothing, Nothing)
            Case "Type"
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

        ProgressBar.IsIndeterminate = False
        StatusLabel.Content = MCBackup.Language.Dictionary("Status.Ready")
        EnableUI(True)
    End Sub

    Private Sub ListView_SelectionChanged(sender As Object, e As EventArgs) Handles ListView.SelectionChanged
        Select Case ListView.SelectedItems.Count
            Case 0
                RestoreButton.IsEnabled = False
                RenameButton.IsEnabled = False ' Don't allow anything when no items are selected
                DeleteButton.IsEnabled = False

                SidebarTitle.Text = String.Format(MCBackup.Language.Dictionary("MainWindow.Sidebar.NumberElements"), ListView.Items.Count)        'Show total number of elements
                SidebarTitle.ToolTip = String.Format(MCBackup.Language.Dictionary("MainWindow.Sidebar.NumberElements"), ListView.Items.Count)

                ListViewRestoreItem.IsEnabled = False
                ListViewDeleteItem.IsEnabled = False         'Disable ContextMenu items
                ListViewRenameItem.IsEnabled = False
                ListViewMoveToGroupItem.IsEnabled = False

                ThumbnailImage.Source = New BitmapImage(New Uri("pack://application:,,,/Resources/nothumb.png"))
                SidebarOriginalNameContent.Text = "-"
                SidebarOriginalNameContent.ToolTip = MCBackup.Language.Dictionary("MainWindow.Sidebar.NoBackupSelected")
                SidebarTypeContent.Text = "-"
                SidebarTypeContent.ToolTip = MCBackup.Language.Dictionary("MainWindow.Sidebar.NoBackupSelected")

                DescriptionTextBox.Text = MCBackup.Language.Dictionary("MainWindow.Sidebar.Description.NoItem")

                SidebarPlayerHealth.Visibility = Windows.Visibility.Collapsed
                SidebarPlayerHunger.Visibility = Windows.Visibility.Collapsed

                SidebarTypeLabel.Visibility = Windows.Visibility.Collapsed
                SidebarTypeContent.Visibility = Windows.Visibility.Collapsed
                SidebarOriginalNameLabel.Visibility = Windows.Visibility.Collapsed
                SidebarOriginalNameContent.Visibility = Windows.Visibility.Collapsed
                SidebarDescriptionLabel.Visibility = Windows.Visibility.Collapsed
                DescriptionTextBox.Visibility = Windows.Visibility.Collapsed
            Case 1
                Dim Thread As New Thread(AddressOf LoadBackupInfo)
                Thread.Start()
            Case Else
                RestoreButton.IsEnabled = False
                RenameButton.IsEnabled = False ' Only allow deletion if more than 1 item is selected
                DeleteButton.IsEnabled = True

                SidebarTitle.Text = String.Format(MCBackup.Language.Dictionary("MainWindow.Sidebar.NumberElementsSelected"), ListView.SelectedItems.Count)   'Set sidebar title to number of selected items
                SidebarTitle.ToolTip = String.Format(MCBackup.Language.Dictionary("MainWindow.Sidebar.NumberElementsSelected"), ListView.SelectedItems.Count)

                ListViewRestoreItem.IsEnabled = False
                ListViewDeleteItem.IsEnabled = True
                ListViewRenameItem.IsEnabled = False
                ListViewMoveToGroupItem.IsEnabled = True

                ThumbnailImage.Source = New BitmapImage(New Uri("pack://application:,,,/Resources/nothumb.png"))

                SidebarPlayerHealth.Visibility = Windows.Visibility.Collapsed
                SidebarPlayerHunger.Visibility = Windows.Visibility.Collapsed

                SidebarTypeLabel.Visibility = Windows.Visibility.Collapsed
                SidebarTypeContent.Visibility = Windows.Visibility.Collapsed
                SidebarOriginalNameLabel.Visibility = Windows.Visibility.Collapsed
                SidebarOriginalNameContent.Visibility = Windows.Visibility.Collapsed
                SidebarDescriptionLabel.Visibility = Windows.Visibility.Collapsed
                DescriptionTextBox.Visibility = Windows.Visibility.Collapsed
        End Select
    End Sub

    Private Sub LoadBackupInfo()
        Dim SelectedItem As ListViewBackupItem = Nothing
        Dispatcher.Invoke(Sub()
                              SelectedItem = ListView.SelectedItem
                              RestoreButton.IsEnabled = True
                              RenameButton.IsEnabled = True ' Allow anything if only 1 item is selected
                              DeleteButton.IsEnabled = True

                              SidebarTitle.Text = SelectedItem.Name     'Set sidebar title to backup name
                              SidebarTitle.ToolTip = SelectedItem

                              ListViewRestoreItem.IsEnabled = True
                              ListViewDeleteItem.IsEnabled = True     'Enable ContextMenu items
                              ListViewRenameItem.IsEnabled = True
                              ListViewMoveToGroupItem.IsEnabled = True

                              SidebarPlayerHealth.Visibility = Windows.Visibility.Collapsed
                              SidebarPlayerHunger.Visibility = Windows.Visibility.Collapsed

                              SidebarTypeLabel.Visibility = Windows.Visibility.Visible
                              SidebarTypeContent.Visibility = Windows.Visibility.Visible
                              SidebarOriginalNameLabel.Visibility = Windows.Visibility.Visible
                              SidebarOriginalNameContent.Visibility = Windows.Visibility.Visible
                              SidebarDescriptionLabel.Visibility = Windows.Visibility.Visible
                              DescriptionTextBox.Visibility = Windows.Visibility.Visible

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

        Dim Type As String = "-", OriginalFolderName As String = "-", Description As String = ""

        Try
            Dim InfoJson As JObject
            Using SR As New StreamReader(My.Settings.BackupsFolderLocation & "\" & SelectedItem.Name & "\info.json")
                InfoJson = JsonConvert.DeserializeObject(SR.ReadToEnd)
            End Using
            Type = InfoJson("Type")
            OriginalFolderName = InfoJson("OriginalName")
            Description = InfoJson("Description")
        Catch ex As Exception
            Log.Print(ex.Message, Log.Level.Severe)
        End Try
        Dispatcher.Invoke(Sub()
                              SidebarOriginalNameContent.Text = OriginalFolderName
                              SidebarOriginalNameContent.ToolTip = OriginalFolderName

                              Select Case Type
                                  Case "save"
                                      SidebarTypeContent.Text = MCBackup.Language.Dictionary("BackupTypes.Save")
                                      SidebarTypeContent.ToolTip = MCBackup.Language.Dictionary("BackupTypes.Save")
                                  Case "version"
                                      SidebarTypeContent.Text = MCBackup.Language.Dictionary("BackupTypes.Version")
                                      SidebarTypeContent.ToolTip = MCBackup.Language.Dictionary("BackupTypes.Version")
                                  Case "everything"
                                      SidebarTypeContent.Text = MCBackup.Language.Dictionary("BackupTypes.Everything")
                                      SidebarTypeContent.ToolTip = MCBackup.Language.Dictionary("BackupTypes.Everything")
                              End Select

                              DescriptionTextBox.Text = IIf(String.IsNullOrEmpty(Description), MCBackup.Language.Dictionary("MainWindow.Sidebar.Description.NoDesc"), Description)
                          End Sub)

        ' Removed until fix is found
        'If Type = "save" Then
        '    Dispatcher.Invoke(Sub()
        '                          SidebarPlayerHealth.Visibility = Windows.Visibility.Visible
        '                          SidebarPlayerHunger.Visibility = Windows.Visibility.Visible
        '                          SidebarPlayerHealthGrid.Children.Clear()
        '                          SidebarPlayerHungerGrid.Children.Clear()
        '                      End Sub)
        '    Try
        '        Dim World As NbtWorld = NbtWorld.Open(My.Settings.BackupsFolderLocation & "\" & SelectedItem.Name)

        '        Dispatcher.Invoke(Sub()
        '                              For i As Integer = 0 To World.Level.Player.Health \ 2 - 1
        '                                  SidebarPlayerHealthGrid.Children.Add(New Game.Images.Health(New Thickness(SidebarPlayerHealthGrid.Children.Count * 10, 0, 0, 0), Game.Images.State.Full))
        '                              Next
        '                              If World.Level.Player.Health Mod 2 <> 0 Then
        '                                  SidebarPlayerHealthGrid.Children.Add(New Game.Images.Health(New Thickness(SidebarPlayerHealthGrid.Children.Count * 10, 0, 0, 0), Game.Images.State.Half))
        '                              End If
        '                              For i As Integer = 0 To (20 - World.Level.Player.Health) \ 2 - 1
        '                                  SidebarPlayerHealthGrid.Children.Add(New Game.Images.Health(New Thickness(SidebarPlayerHealthGrid.Children.Count * 10, 0, 0, 0), Game.Images.State.Empty))
        '                              Next

        '                              For i As Integer = 0 To World.Level.Player.HungerLevel \ 2 - 1
        '                                  SidebarPlayerHungerGrid.Children.Add(New Game.Images.Hunger(New Thickness(90 - SidebarPlayerHungerGrid.Children.Count * 10, 0, 0, 0), Game.Images.State.Full))
        '                              Next
        '                              If World.Level.Player.HungerLevel Mod 2 <> 0 Then
        '                                  SidebarPlayerHungerGrid.Children.Add(New Game.Images.Hunger(New Thickness(90 - SidebarPlayerHungerGrid.Children.Count * 10, 0, 0, 0), Game.Images.State.Half))
        '                              End If
        '                              For i As Integer = 0 To (20 - World.Level.Player.HungerLevel) \ 2 - 1
        '                                  SidebarPlayerHungerGrid.Children.Add(New Game.Images.Hunger(New Thickness(90 - SidebarPlayerHungerGrid.Children.Count * 10, 0, 0, 0), Game.Images.State.Empty))
        '                              Next
        '                          End Sub)
        '    Catch ex As Exception
        '        Dispatcher.Invoke(Sub() ErrorReportDialog.Show("An error occured while trying to load world info.", ex))
        '    End Try
        'Else
        '    Dispatcher.Invoke(Sub()
        '                          SidebarPlayerHealth.Visibility = Windows.Visibility.Collapsed
        '                          SidebarPlayerHunger.Visibility = Windows.Visibility.Collapsed
        '                      End Sub)
        'End If
    End Sub

    Public Sub LoadLanguage()
        Try
            BackupButton.Content = MCBackup.Language.Dictionary("MainWindow.BackupButton.Content")
            RestoreButton.Content = MCBackup.Language.Dictionary("MainWindow.RestoreButton.Content")
            DeleteButton.Content = MCBackup.Language.Dictionary("MainWindow.DeleteButton.Content")
            RenameButton.Content = MCBackup.Language.Dictionary("MainWindow.RenameButton.Content")
            CullButton.Content = MCBackup.Language.Dictionary("MainWindow.CullButton.Content")
            ListViewMoveToGroupItem.Header = MCBackup.Language.Dictionary("MainWindow.MoveToGroupButton.Text")
            AutomaticBackupButton.Content = MCBackup.Language.Dictionary("MainWindow.AutomaticBackupButton.Content") & " >>"

            NameColumnHeader.Content = MCBackup.Language.Dictionary("MainWindow.ListView.Columns(0).Header")
            DateCreatedColumnHeader.Content = MCBackup.Language.Dictionary("MainWindow.ListView.Columns(1).Header")
            TypeColumnHeader.Content = MCBackup.Language.Dictionary("MainWindow.ListView.Columns(2).Header")

            SidebarOriginalNameLabel.Text = MCBackup.Language.Dictionary("MainWindow.Sidebar.OriginalNameLabel.Text")
            SidebarTypeLabel.Text = MCBackup.Language.Dictionary("MainWindow.Sidebar.TypeLabel.Text")
            SidebarDescriptionLabel.Text = MCBackup.Language.Dictionary("MainWindow.Sidebar.DescriptionLabel.Text")
            SidebarPlayerHealthLabel.Text = MCBackup.Language.Dictionary("MainWindow.Sidebar.PlayerHealthLabel.Text")
            SidebarPlayerHungerLabel.Text = MCBackup.Language.Dictionary("MainWindow.Sidebar.PlayerHungerLabel.Text")

            FileToolbarButton.Content = MCBackup.Language.Dictionary("MainWindow.Toolbar.FileButton.Text")
            FileContextMenu.Items(0).Header = MCBackup.Language.Dictionary("MainWindow.Toolbar.FileContextMenu.Items(0).Header")
            EditToolbarButton.Content = MCBackup.Language.Dictionary("MainWindow.Toolbar.EditButton.Text")
            EditContextMenu.Items(0).Header = MCBackup.Language.Dictionary("MainWindow.Toolbar.EditContextMenu.Items(0).Header")
            EditContextMenu.Items(1).Header = MCBackup.Language.Dictionary("MainWindow.Toolbar.EditContextMenu.Items(1).Header")
            ToolsToolbarButton.Content = MCBackup.Language.Dictionary("MainWindow.Toolbar.ToolsButton.Text")
            ToolsContextMenu.Items(0).Header = MCBackup.Language.Dictionary("MainWindow.Toolbar.ToolsContextMenu.Items(0).Header")
            HelpToolbarButton.Content = MCBackup.Language.Dictionary("MainWindow.Toolbar.HelpButton.Text")
            HelpContextMenu.Items(0).Header = MCBackup.Language.Dictionary("MainWindow.Toolbar.HelpContextMenu.Items(0).Header")
            HelpContextMenu.Items(2).Header = MCBackup.Language.Dictionary("MainWindow.Toolbar.HelpContextMenu.Items(2).Header")
            HelpContextMenu.Items(3).Header = MCBackup.Language.Dictionary("MainWindow.Toolbar.HelpContextMenu.Items(3).Header")

            StatusLabel.Content = MCBackup.Language.Dictionary("Status.Ready")

            SearchTextBox.Text = MCBackup.Language.Dictionary("MainWindow.Search")
            SearchTextBox.Foreground = New SolidColorBrush(Colors.Gray)

            ListViewSortByItem.Header = MCBackup.Language.Dictionary("MainWindow.ListView.ContextMenu.SortBy")
            ListViewSortByNameItem.Header = MCBackup.Language.Dictionary("MainWindow.ListView.ContextMenu.SortBy.Name")
            ListViewSortByDateCreatedItem.Header = MCBackup.Language.Dictionary("MainWindow.ListView.ContextMenu.SortBy.DateCreated")
            ListViewSortByTypeItem.Header = MCBackup.Language.Dictionary("MainWindow.ListView.ContextMenu.SortBy.Type")
            ListViewSortAscendingItem.Header = MCBackup.Language.Dictionary("MainWindow.ListView.ContextMenu.SortBy.Ascending")
            ListViewSortDescendingItem.Header = MCBackup.Language.Dictionary("MainWindow.ListView.ContextMenu.SortBy.Descending")

            ListViewGroupByItem.Header = MCBackup.Language.Dictionary("MainWindow.ListView.ContextMenu.GroupBy")
            ListViewGroupByNameItem.Header = MCBackup.Language.Dictionary("MainWindow.ListView.ContextMenu.GroupBy.OriginalName")
            ListViewGroupByTypeItem.Header = MCBackup.Language.Dictionary("MainWindow.ListView.ContextMenu.GroupBy.Type")
            ListViewGroupByNothingItem.Header = MCBackup.Language.Dictionary("MainWindow.ListView.ContextMenu.GroupBy.Nothing")

            ListViewRestoreItem.Header = MCBackup.Language.Dictionary("MainWindow.RestoreButton.Content")
            ListViewDeleteItem.Header = MCBackup.Language.Dictionary("MainWindow.DeleteButton.Content")
            ListViewRenameItem.Header = MCBackup.Language.Dictionary("MainWindow.RenameButton.Content")

            NoBackupsOverlay.Text = MCBackup.Language.Dictionary("MainWindow.NoBackupsOverlay.Text")

            CancelButton.Content = MCBackup.Language.Dictionary("MainWindow.CancelButton.Text")
        Catch ex As Exception
            Dispatcher.Invoke(Sub() ErrorReportDialog.Show("Could not load language.", ex))
        End Try
    End Sub

    Public Sub ReloadBackupGroups()
        GroupsTabControl.Items.Clear()
        GroupsTabControl.Items.Add(New TaggedTabItem(MCBackup.Language.Dictionary("MainWindow.Groups.All"), ""))
        For Each Group As String In My.Settings.BackupGroups
            GroupsTabControl.Items.Add(New TaggedTabItem(Group, Group))
        Next
        GroupsTabControl.SelectedIndex = 0
    End Sub
#End Region

#Region "Backup"
    Private Delegate Sub UpdateProgressBarDelegate(ByVal dp As System.Windows.DependencyProperty, ByVal value As Object)
    Private BackupStopwatch As New Stopwatch

    Private Sub BackupButton_Click(sender As Object, e As EventArgs) Handles BackupButton.Click
        Dim BackupDialog As New BackupDialog
        BackupDialog.Owner = Me
        BackupDialog.ShowDialog()
    End Sub

    Public Sub StartBackup()
        If Not BackupThread Is Nothing Then
            If BackupThread.IsAlive Then
                MetroMessageBox.Show(MCBackup.Language.Dictionary("Message.BackupInProgress"), MCBackup.Language.Dictionary("Message.Caption.Error"), MessageBoxButton.OK, MessageBoxImage.Error)
                Exit Sub
            End If
        End If
        Log.Print("Starting new backup (Name: '{0}'; Description: '{1}'; Path: '{2}'; Type: '{3}'", BackupInfo(0), BackupInfo(1), BackupInfo(2), BackupInfo(3))
        EnableUI(False)
        Cancel = False
        Dim t As New Thread(AddressOf Backup)
        t.Start()
    End Sub

    Private MCMapProcess As Process

    Private Sub Backup()
        Try
            ' Create the target directory to prevent exceptions while getting the completion percentage
            My.Computer.FileSystem.CreateDirectory(My.Settings.BackupsFolderLocation & "\" & BackupInfo(0))

            ' Start copying the source directory asynchronously to the target directory
            BackupThread = FileSystemOperations.Directory.CopyAsync(BackupInfo(2), My.Settings.BackupsFolderLocation & "\" & BackupInfo(0), True)

            ' Reset & start the backup stopwatch
            BackupStopwatch.Reset()
            BackupStopwatch.Start()

            ' Set variables
            Dim PercentComplete As Double = 0

            ' Do until percent complete is equal to or over 100
            Do Until Int(PercentComplete) = 100
                ' Calculate percent complete by dividing target location by source location, and multiply by 100
                PercentComplete = GetFolderSize(My.Settings.BackupsFolderLocation & "\" & BackupInfo(0)) / GetFolderSize(BackupInfo(2)) * 100

                ' Determine speed in megabytes per second (MB/s) by dividing bytes copied by seconds elapsed (in decimal for more accuracy), and dividing by 1048576.
                Dim Total As Double = GetFolderSize(BackupInfo(2))
                Dim Copied As Double = GetFolderSize(My.Settings.BackupsFolderLocation & "\" & BackupInfo(0)) ' 1024 (1K bytes) × 1024 = 1048576 (1M bytes)
                Dim Speed As Double = Math.Round((Copied / 1048576) / (BackupStopwatch.ElapsedMilliseconds / 1000), 2)

                Dim TimeLeft As New TimeSpan(0)

                ' Determine time remaining using (TimeElapsed / BytesCopied) * BytesRemaining and round to the nearest 5
                If Copied > 0 Then
                    TimeLeft = TimeSpan.FromSeconds(Math.Round((BackupStopwatch.ElapsedMilliseconds / 1000) / Copied * (Total - Copied) / 5) * 5)
                End If

                Dispatcher.Invoke(Sub()
                                      StatusLabel.Content = String.Format(MCBackup.Language.Dictionary("Status.BackingUp"), PercentComplete, Speed, TimeLeft.TotalSeconds)
                                  End Sub)

                Me.Dispatcher.Invoke(Sub()
                                         Progress.Value = PercentComplete
                                     End Sub)

                If Cancel = True And BackupThread.IsAlive = False Then
                    Dispatcher.Invoke(Sub()
                                          StatusLabel.Content = "Reverting changes..."
                                          ProgressBar.IsIndeterminate = True
                                      End Sub)
                    My.Computer.FileSystem.DeleteDirectory(My.Settings.BackupsFolderLocation & "\" & BackupInfo(0), FileIO.DeleteDirectoryOption.DeleteAllContents)
                    Dispatcher.Invoke(Sub()
                                          BackupStopwatch.Stop()
                                          Progress.Value = 0
                                          ProgressBar.IsIndeterminate = False
                                          StatusLabel.Content = "Operation cancelled - Ready"
                                          EnableUI(True)
                                          RefreshBackupsList()
                                          ReloadBackupGroups()
                                      End Sub)
                    Exit Sub
                End If
            Loop

            ' Stop backup stopwatch
            BackupStopwatch.Stop()

            Dim InfoJson As New JObject

            InfoJson.Add(New JProperty("OriginalName", New DirectoryInfo(BackupInfo(2)).Name))
            InfoJson.Add(New JProperty("Type", BackupInfo(3)))
            InfoJson.Add(New JProperty("Description", BackupInfo(1)))
            InfoJson.Add(New JProperty("Group", BackupInfo(4)))
            InfoJson.Add(New JProperty("Launcher", BackupInfo(5)))
            InfoJson.Add(New JProperty("Modpack", BackupInfo(6)))

            Using SW As New StreamWriter(My.Settings.BackupsFolderLocation & "\" & BackupInfo(0) & "\info.json") ' Create information file (stores description, type, folder name, group name, launcher and modpack)
                SW.Write(JsonConvert.SerializeObject(InfoJson, Formatting.Indented))
            End Using

            ' Send +1 to StatCounter
            If My.Settings.SendAnonymousData Then
                Dim WebClient As New WebClient
                WebClient.DownloadDataAsync(New Uri("http://c.statcounter.com/9820848/0/90ee98bc/1/"))
            End If

            If BackupInfo(3) = "save" And My.Settings.CreateThumbOnWorld Then
                ' Create thumbnail if backup type is save
                Log.Print("Creating thumbnail")

                Dispatcher.Invoke(Sub()
                                      StatusLabel.Content = String.Format(MCBackup.Language.Dictionary("Status.CreatingThumb"), 0)

                                      MCMapProcess = New Process

                                      With MCMapProcess.StartInfo
                                          .FileName = Chr(34) & StartupPath & "\mcmap\mcmap.exe" & Chr(34)
                                          .WorkingDirectory = StartupPath & "\mcmap\"
                                          .Arguments = String.Format(" -from -15 -15 -to 15 15 -file ""{0}\{1}\thumb.png"" ""{2}""", My.Settings.BackupsFolderLocation, BackupInfo(0), BackupInfo(2))
                                          .CreateNoWindow = True
                                          .UseShellExecute = False
                                          .RedirectStandardError = True
                                          .RedirectStandardOutput = True
                                      End With

                                      AddHandler MCMapProcess.OutputDataReceived, AddressOf MCMap_DataReceived
                                      AddHandler MCMapProcess.ErrorDataReceived, AddressOf MCMap_DataReceived

                                      With MCMapProcess
                                          .Start()
                                          .BeginOutputReadLine()
                                          .BeginErrorReadLine()
                                      End With
                                  End Sub)
            Else
                ' Refresh backups list
                Dispatcher.Invoke(Sub()
                                      RefreshBackupsList()
                                  End Sub)

                Log.Print("Backup Complete")

                ' Re-enable the UI and tell the user the backup is complete
                Dispatcher.Invoke(Sub()
                                      EnableUI(True)
                                      RefreshBackupsList()
                                      ReloadBackupGroups()
                                      StatusLabel.Content = MCBackup.Language.Dictionary("Status.BackupComplete")
                                      StatusLabel.Refresh()
                                      Progress.Value = 100
                                  End Sub)

                If My.Settings.ShowBalloonTips Then NotifyIcon.ShowBalloonTip(2000, MCBackup.Language.Dictionary("BalloonTip.Title.BackupComplete"), MCBackup.Language.Dictionary("BalloonTip.BackupComplete"), System.Windows.Forms.ToolTipIcon.Info)
            End If
        Catch ex As Exception
            If My.Settings.ShowBalloonTips Then NotifyIcon.ShowBalloonTip(2000, MCBackup.Language.Dictionary("BalloonTip.Title.BackupError"), MCBackup.Language.Dictionary("BalloonTip.BackupError"), System.Windows.Forms.ToolTipIcon.Error)
            Dispatcher.Invoke(Sub() ErrorReportDialog.Show(MCBackup.Language.Dictionary("Exception.Backup"), ex))
        End Try
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

        If Me.Cancel Then
            Me.Cancel = False
            Exit Sub
        End If

        If e.Data.Contains("[") And e.Data.Contains("]") Then
            Dim PercentComplete As Double = (e.Data.Substring(1).Remove(e.Data.IndexOf(".") - 1) / 4) + (StepNumber * 25)

            Dispatcher.Invoke(Sub()
                                  Progress.Value = PercentComplete
                                  StatusLabel.Content = String.Format(MCBackup.Language.Dictionary("Status.CreatingThumb"), Int(PercentComplete))
                              End Sub)
        ElseIf e.Data = "Job complete." Then
            Dispatcher.Invoke(Sub()
                                  EnableUI(True)
                                  RefreshBackupsList()
                                  ReloadBackupGroups()
                                  Progress.Value = 100
                                  StatusLabel.Content = MCBackup.Language.Dictionary("Status.BackupComplete")
                                  StatusLabel.Refresh()
                                  If My.Settings.ShowBalloonTips Then NotifyIcon.ShowBalloonTip(2000, MCBackup.Language.Dictionary("BalloonTip.Title.BackupComplete"), MCBackup.Language.Dictionary("BalloonTip.BackupComplete"), System.Windows.Forms.ToolTipIcon.Info)
                                  Log.Print("Backup Complete")
                              End Sub)
        End If
    End Sub
#End Region

#Region "Restore"
    Private RestoreStopWatch As New Stopwatch

    Private Sub RestoreButton_Click(sender As Object, e As EventArgs) Handles RestoreButton.Click, ListViewRestoreItem.Click
        If MetroMessageBox.Show(MCBackup.Language.Dictionary("Message.RestoreAreYouSure"), MCBackup.Language.Dictionary("Message.Caption.AreYouSure"), MessageBoxButton.YesNo, MessageBoxImage.Question) = Forms.DialogResult.Yes Then
            EnableUI(False)
            Cancel = False
            Log.Print("Starting Restore")
            RestoreInfo(0) = ListView.SelectedItems(0).Name ' Set place 0 of RestoreInfo array to the backup name

            Dim BaseFolderName As String = "", Launcher As Game.Launcher = Game.Launcher.Minecraft, Modpack As String = ""

            Dim InfoJson As JObject

            Using SR As New StreamReader(My.Settings.BackupsFolderLocation & "\" & RestoreInfo(0) & "\info.json")
                'Do While SR.Peek <> -1
                '    Dim Line As String = SR.ReadLine
                '    If Not Line.StartsWith("#") Then
                '        If Line.StartsWith("baseFolderName=") Then
                '            BaseFolderName = Line.Substring(15)
                '        ElseIf Line.StartsWith("type=") Then
                '            RestoreInfo(2) = Line.Substring(5)
                '        ElseIf Line.StartsWith("launcher=") Then
                '            Dim Temp As Object = Line.Substring(9)
                '            If IsNumeric(Temp) Then
                '                If Temp > [Enum].GetValues(GetType(Game.Launcher)).Cast(Of Game.Launcher).Last() Or Temp < 0 Then
                '                    Launcher = Game.Launcher.Minecraft
                '                Else
                '                    Launcher = Temp
                '                End If
                '            Else
                '                Select Case Temp
                '                    Case "minecraft"
                '                        Launcher = Game.Launcher.Minecraft
                '                    Case "technic"
                '                        Launcher = Game.Launcher.Technic
                '                    Case "ftb"
                '                        Launcher = Game.Launcher.FeedTheBeast
                '                    Case "atlauncher"
                '                        Launcher = Game.Launcher.ATLauncher
                '                    Case Else
                '                        Launcher = Game.Launcher.Minecraft
                '                End Select
                '            End If
                '        ElseIf Line.StartsWith("modpack=") Then
                '            Modpack = Line.Substring(8)
                '        End If
                '    End If
                'Loop
                InfoJson = JsonConvert.DeserializeObject(SR.ReadToEnd)
                BaseFolderName = InfoJson("OriginalName")
                RestoreInfo(2) = InfoJson("Type")

                Dim Temp As Object = InfoJson("Launcher")
                If IsNumeric(Temp) Then
                    If Temp > [Enum].GetValues(GetType(Game.Launcher)).Cast(Of Game.Launcher).Last() Or Temp < 0 Then
                        Launcher = Game.Launcher.Minecraft
                    Else
                        Launcher = Temp
                    End If
                Else
                    Select Case Temp
                        Case "minecraft"
                            Launcher = Game.Launcher.Minecraft
                        Case "technic"
                            Launcher = Game.Launcher.Technic
                        Case "ftb"
                            Launcher = Game.Launcher.FeedTheBeast
                        Case "atlauncher"
                            Launcher = Game.Launcher.ATLauncher
                        Case Else
                            Launcher = Game.Launcher.Minecraft
                    End Select
                End If

                Modpack = InfoJson("Modpack")
            End Using

            If Launcher <> My.Settings.Launcher Then
                MetroMessageBox.Show(String.Format(MCBackup.Language.Dictionary("Message.IncompatibleBackupConfig"), Game.LauncherToString(Launcher)), MCBackup.Language.Dictionary("Message.Caption.Error"), MessageBoxButton.OK, MessageBoxImage.Error)
                EnableUI(True)
                Exit Sub
            End If

            Select Case RestoreInfo(2)
                Case "save"
                    Select Case My.Settings.Launcher
                        Case Game.Launcher.Minecraft
                            RestoreInfo(1) = My.Settings.SavesFolderLocation & "\" & BaseFolderName
                        Case Game.Launcher.Technic
                            RestoreInfo(1) = My.Settings.MinecraftFolderLocation & "\modpacks\" & Modpack & "\saves\" & BaseFolderName
                        Case Game.Launcher.FeedTheBeast
                            RestoreInfo(1) = My.Settings.MinecraftFolderLocation & "\" & Modpack & "\minecraft\saves\" & BaseFolderName
                        Case Game.Launcher.ATLauncher
                            RestoreInfo(1) = My.Settings.MinecraftFolderLocation & "\Instances\" & Modpack & "\saves\" & BaseFolderName
                    End Select
                Case "version"
                    Select Case My.Settings.Launcher
                        Case Game.Launcher.Minecraft
                            RestoreInfo(1) = My.Settings.MinecraftFolderLocation & "\versions\" & BaseFolderName
                        Case Game.Launcher.Technic
                            RestoreInfo(1) = My.Settings.MinecraftFolderLocation & "\modpacks\" & BaseFolderName
                        Case Game.Launcher.FeedTheBeast
                            RestoreInfo(1) = My.Settings.MinecraftFolderLocation & "\" & BaseFolderName
                        Case Game.Launcher.ATLauncher
                            RestoreInfo(1) = My.Settings.MinecraftFolderLocation & "\Instances\" & BaseFolderName
                    End Select
                Case "everything"
                    RestoreInfo(1) = My.Settings.MinecraftFolderLocation
            End Select

            Dim t As New Thread(AddressOf Restore)
            t.Start()
        End If
    End Sub

    Private Sub Restore()
        Try
            ' Only delete folder contents if folder exists AND it's size is not zero.
            If Directory.Exists(RestoreInfo(1)) Then
                If GetFolderSize(RestoreInfo(1)) <> 0 Then
                    ' Set initial size variable
                    Dim InitialSize As Double = GetFolderSize(RestoreInfo(1))

                    ' Start removal async
                    DeleteForRestoreThread = New DirectoryInfo(RestoreInfo(1)).DeleteContentsAsync

                    Try
                        Do Until DeleteForRestoreThread.IsAlive = False
                            ' Set bytes remaining to current folder size
                            Dim BytesRemaining As Double = 0

                            BytesRemaining = GetFolderSize(RestoreInfo(1))

                            ' Determine percent removed (inverted) by dividing bytes remaining by initial size, and multiplying by 100
                            Dim PercentRemoved As Decimal = BytesRemaining / InitialSize * 100

                            Dispatcher.Invoke(Sub()
                                                  ' Show percent complete and message
                                                  StatusLabel.Content = String.Format(MCBackup.Language.Dictionary("Status.RemovingOldContent"), 100 - PercentRemoved)
                                                  Progress.Value = PercentRemoved
                                              End Sub)

                            If Cancel And DeleteForRestoreThread.IsAlive = False Then
                                Dispatcher.Invoke(Sub()
                                                      RestoreStopWatch.Stop()
                                                      Progress.Value = 0
                                                      ProgressBar.IsIndeterminate = False
                                                      StatusLabel.Content = "Operation cancelled - Ready"
                                                      EnableUI(True)
                                                      RefreshBackupsList()
                                                      ReloadBackupGroups()
                                                  End Sub)
                                Exit Sub
                            End If
                        Loop
                    Catch ex As DirectoryNotFoundException ' HACK Find better way to do this!
                        Log.Print("Directory not found exception occured during removal for restore. This often happens and shouldn't be considered an issue, but may be the source of an occurring problem.", Log.Level.Warning)
                        Exit Try
                    Catch ex As Exception
                        Me.Dispatcher.Invoke(Sub() ErrorReportDialog.Show("An error occured while trying to delete the folder.", ex))
                    End Try
                End If
            End If

            'Exit Sub

            ' Create the target directory to prevent exceptions while getting the completion percentage
            My.Computer.FileSystem.CreateDirectory(RestoreInfo(1))

            ' Start copying the source directory asynchronously to the target directory
            RestoreThread = FileSystemOperations.Directory.CopyAsync(My.Settings.BackupsFolderLocation & "\" & RestoreInfo(0), RestoreInfo(1), True)

            ' Reset & start the backup stopwatch
            RestoreStopWatch.Reset()
            RestoreStopWatch.Start()

            ' Set variables
            Dim PercentComplete As Double = 0

            ' Do until percent complete is equal to or over 100
            Do Until Int(PercentComplete) >= 100
                ' Calculate percent complete by dividing target location by source location, and multiply by 100
                PercentComplete = GetFolderSize(RestoreInfo(1)) / GetFolderSize(My.Settings.BackupsFolderLocation & "\" & RestoreInfo(0)) * 100

                ' Determine speed in megabytes per second (MB/s) by dividing bytes copied by seconds elapsed (in decimal for more accuracy), and dividing by 1048576.
                Dim Total As Double = GetFolderSize(My.Settings.BackupsFolderLocation & "\" & RestoreInfo(0))
                Dim Copied As Double = GetFolderSize(RestoreInfo(1))
                Dim Speed As Double = Math.Round((Copied / 1048576) / (RestoreStopWatch.ElapsedMilliseconds / 1000), 2) ' 1024 (1K bytes) × 1024 = 1048576 (1M bytes)

                Dim TimeLeft As New TimeSpan(0)

                ' Determine time remaining using (TimeElapsed / BytesCopied) * BytesRemaining and round to the nearest 5
                If Copied > 0 Then
                    TimeLeft = TimeSpan.FromSeconds(Math.Round((RestoreStopWatch.ElapsedMilliseconds / 1000) / Copied * (Total - Copied) / 5) * 5)
                End If

                Dispatcher.Invoke(Sub()
                                      ' Display percent complete on progress bar and restoring message
                                      StatusLabel.Content = String.Format(MCBackup.Language.Dictionary("Status.Restoring"), PercentComplete, Speed, TimeLeft.TotalSeconds)
                                      Progress.Value = PercentComplete
                                      ProgressBar.Refresh()
                                  End Sub)

                ' Cancel if cancel variable is true and backup thread has been killed
                If Cancel And RestoreThread.IsAlive = False Then
                    Dispatcher.Invoke(Sub()
                                          RestoreStopWatch.Stop()
                                          Progress.Value = 0
                                          ProgressBar.IsIndeterminate = False
                                          StatusLabel.Content = "Operation cancelled - Ready"
                                          EnableUI(True)
                                          RefreshBackupsList()
                                          ReloadBackupGroups()
                                      End Sub)
                    Exit Sub
                End If
            Loop

            ' Stop backup stopwatch
            RestoreStopWatch.Stop()

            ' Delete info/thumb files from restored backup
            If My.Computer.FileSystem.FileExists(RestoreInfo(1) & "\info.json") Then My.Computer.FileSystem.DeleteFile(RestoreInfo(1) & "\info.json")
            If My.Computer.FileSystem.FileExists(RestoreInfo(1) & "\thumb.png") Then My.Computer.FileSystem.DeleteFile(RestoreInfo(1) & "\thumb.png")

            Dispatcher.Invoke(Sub()
                                  StatusLabel.Content = MCBackup.Language.Dictionary("Status.RestoreComplete")
                                  Progress.Value = 100
                                  EnableUI(True)
                                  RefreshBackupsList()
                                  ReloadBackupGroups()
                              End Sub)

            If My.Settings.ShowBalloonTips Then NotifyIcon.ShowBalloonTip(2000, MCBackup.Language.Dictionary("BalloonTip.Title.RestoreComplete"), MCBackup.Language.Dictionary("BalloonTip.RestoreComplete"), System.Windows.Forms.ToolTipIcon.Info)
            Log.Print("Restore Complete")
            RefreshBackupsList()
        Catch ex As Exception
            Log.Print(ex.Message, Log.Level.Severe)
            If My.Settings.ShowBalloonTips Then NotifyIcon.ShowBalloonTip(2000, MCBackup.Language.Dictionary("BalloonTip.Title.RestoreError"), MCBackup.Language.Dictionary("BalloonTip.RestoreError"), System.Windows.Forms.ToolTipIcon.Error)
            Me.Dispatcher.Invoke(Sub() ErrorReportDialog.Show(MCBackup.Language.Dictionary("Exception.Restore"), ex))
        End Try
    End Sub
#End Region

#Region "Functions"
    Private Function GetFolderSize(FolderPath As String)
        Dim FSO As FileSystemObject = New FileSystemObject
        If Not Directory.Exists(FolderPath) Then
            Throw New DirectoryNotFoundException(String.Format("Directory '{0}' does not exist.", FolderPath))
        End If
        Dim Size = FSO.GetFolder(FolderPath).Size ' Get FolderPath's size
        If Size <> Nothing Then
            Return Size
        Else
            Return 0
        End If
        Return 0
    End Function

    Public Function GetFolderDateCreated(FolderPath As String)
        Try
            Dim FSO As FileSystemObject = New FileSystemObject
            Return FSO.GetFolder(FolderPath).DateCreated ' Get FolderPath's date of creation
        Catch ex As Exception
            Dispatcher.Invoke(Sub() ErrorReportDialog.Show(String.Format("Could not find {0}'s creation date:", FolderPath), ex))
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
        Process.Start("http://www.nicoco007.com/minecraft/applications/mcbackup-3")
    End Sub

    Private Sub AboutMenuItem_Click(sender As Object, e As RoutedEventArgs)
        Dim AboutDialog As New AboutDialog
        AboutDialog.Owner = Me
        AboutDialog.ShowDialog()
    End Sub

    Private Sub ReportBugMenuItem_Click(sender As Object, e As RoutedEventArgs)
        Process.Start("http://bugtracker.nicoco007.com/index.php?do=newtask&project=2")
    End Sub

    Private Sub RefreshBackupsList_Click(sender As Object, e As RoutedEventArgs)
        RefreshBackupsList()
    End Sub
#End Region

#Region "Delete"
    Private Sub DeleteButton_Click(sender As Object, e As EventArgs) Handles DeleteButton.Click, ListViewDeleteItem.Click
        If MetroMessageBox.Show(MCBackup.Language.Dictionary("Message.DeleteAreYouSure"), MCBackup.Language.Dictionary("Message.Caption.AreYouSure"), MessageBoxButton.YesNo, MessageBoxImage.Question) = Windows.Forms.DialogResult.Yes Then
            Dim ListViewItems As New ArrayList
            For Each Item In ListView.SelectedItems
                ListViewItems.Add(Item.Name)
            Next
            Delete(ListViewItems)
        End If
    End Sub

    Private Sub Delete(ItemsToDelete As ArrayList)
        EnableUI(False)
        ListView.SelectedIndex = -1
        DeleteThread = New Thread(Sub() DeleteBackgroundWorker_DoWork(ItemsToDelete))
        DeleteThread.Start()
        StatusLabel.Content = MCBackup.Language.Dictionary("Status.Deleting")
        ProgressBar.IsIndeterminate = True
        If Environment.OSVersion.Version.Major > 5 Then
            TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Indeterminate)
        End If
    End Sub

    Private Sub DeleteBackgroundWorker_DoWork(ItemsToDelete As ArrayList)
        Try
            For Each Item As String In ItemsToDelete
                My.Computer.FileSystem.DeleteDirectory(My.Settings.BackupsFolderLocation & "\" & Item, FileIO.DeleteDirectoryOption.DeleteAllContents)
            Next
            Me.Dispatcher.Invoke(Sub()
                                     DeleteBackgroundWorker_RunWorkerCompleted()
                                 End Sub)
        Catch ex As Exception
            If TypeOf ex Is ThreadAbortException Then
                Log.Print("Delete thread aborted!", Log.Level.Severe)
                Me.Dispatcher.Invoke(Sub()
                                         BackupStopwatch.Stop()
                                         Progress.Value = 0
                                         StatusLabel.Content = "Delete cancelled."
                                     End Sub)
            Else
                Dispatcher.Invoke(Sub() ErrorReportDialog.Show(MCBackup.Language.Dictionary("Exception.Delete"), ex))
            End If
        End Try
    End Sub

    Private Sub DeleteBackgroundWorker_RunWorkerCompleted()
        EnableUI(True)
        RefreshBackupsList()
        ReloadBackupGroups()
        StatusLabel.Content = MCBackup.Language.Dictionary("Status.DeleteComplete")
        ProgressBar.IsIndeterminate = False
        If Environment.OSVersion.Version.Major > 5 Then
            TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress)
        End If
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
            AutomaticBackupButton.Content = MCBackup.Language.Dictionary("MainWindow.AutomaticBackupButton.Content") & " >>"
        Else
            Me.Left = Me.Left - (AutoBackupWindow.Width / 2)
            AutomaticBackupButton.Content = MCBackup.Language.Dictionary("MainWindow.AutomaticBackupButton.Content") & " <<"
            AutoBackupWindow.Top = Me.Top
            AutoBackupWindow.Left = Me.Left + Me.Width + 5
            AutoBackupWindow.Show()
        End If
    End Sub

    Private Sub Main_Activated(sender As Object, e As EventArgs) Handles MyBase.Activated
        AutoBackupWindow.Focus()
        Me.Focus()
    End Sub

    Private Sub Main_SizeChanged(sender As Object, e As SizeChangedEventArgs) Handles MyBase.SizeChanged
        Main_LocationChanged(sender, Nothing)
        AutoBackupWindow.Height = Me.Height
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
        ElseIf ClickedColumn Is DateCreatedColumnHeader Then
            ListViewSortByNameItem.IsChecked = False
            ListViewSortByDateCreatedItem.IsChecked = True
            ListViewSortByTypeItem.IsChecked = False
        ElseIf ClickedColumn Is TypeColumnHeader Then
            ListViewSortByNameItem.IsChecked = False
            ListViewSortByDateCreatedItem.IsChecked = False
            ListViewSortByTypeItem.IsChecked = True
        End If
    End Sub

    Private Sub ListViewGroupByNameItem_Click(sender As Object, e As RoutedEventArgs) Handles ListViewGroupByNameItem.Click
        ListViewGroupByNameItem.IsChecked = True
        ListViewGroupByTypeItem.IsChecked = False
        ListViewGroupByNothingItem.IsChecked = False
        Dim View As CollectionView = DirectCast(CollectionViewSource.GetDefaultView(ListView.ItemsSource), CollectionView)
        View.GroupDescriptions.Clear()
        View.GroupDescriptions.Add(New PropertyGroupDescription("OriginalName"))
        My.Settings.ListViewGroupBy = "OriginalName"
    End Sub

    Private Sub ListViewGroupByTypeItem_Click(sender As Object, e As RoutedEventArgs) Handles ListViewGroupByTypeItem.Click
        ListViewGroupByNameItem.IsChecked = False
        ListViewGroupByTypeItem.IsChecked = True
        ListViewGroupByNothingItem.IsChecked = False
        Dim View As CollectionView = DirectCast(CollectionViewSource.GetDefaultView(ListView.ItemsSource), CollectionView)
        View.GroupDescriptions.Clear()
        View.GroupDescriptions.Add(New PropertyGroupDescription("Type"))
        My.Settings.ListViewGroupBy = "Type"
    End Sub

    Private Sub ListViewGroupByNothingItem_Click(sender As Object, e As RoutedEventArgs) Handles ListViewGroupByNothingItem.Click
        ListViewGroupByNameItem.IsChecked = False
        ListViewGroupByTypeItem.IsChecked = False
        ListViewGroupByNothingItem.IsChecked = True
        Dim View As CollectionView = DirectCast(CollectionViewSource.GetDefaultView(ListView.ItemsSource), CollectionView)
        View.GroupDescriptions.Clear()
        My.Settings.ListViewGroupBy = "Nothing"
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

#Region "Tray Icon"
    Private Sub ExitToolbarMenuItem_Click(sender As Object, e As EventArgs)
        Application.CloseAction = Application.AppCloseAction.Close
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
                            If My.Settings.FirstCloseToTray Then
                                NotifyIcon.ShowBalloonTip(2000, MCBackup.Language.Dictionary("BalloonTip.Title.RunningBackground"), MCBackup.Language.Dictionary("BalloonTip.RunningBackground"), System.Windows.Forms.ToolTipIcon.Info)
                                My.Settings.FirstCloseToTray = False
                            End If

                            Log.Print("Closing to tray")

                            My.Settings.CloseToTray = True

                            e.Cancel = True
                        Case Forms.DialogResult.No
                            ' Do nothing
                        Case Forms.DialogResult.Cancel
                            e.Cancel = True
                        Case Else
                            e.Cancel = True
                    End Select
                Else
                    If My.Settings.CloseToTray Then
                        Me.Hide()
                        If My.Settings.FirstCloseToTray Then
                            NotifyIcon.ShowBalloonTip(2000, MCBackup.Language.Dictionary("BalloonTip.Title.RunningBackground"), MCBackup.Language.Dictionary("BalloonTip.RunningBackground"), System.Windows.Forms.ToolTipIcon.Info)
                            My.Settings.FirstCloseToTray = False
                        End If

                        Log.Print("Closing to tray")

                        My.Settings.CloseToTray = True

                        e.Cancel = True
                    End If
                End If
            End If

            If ThreadIsNotNothingAndAlive(BackupThread) Or ProcessIsNotNothingAndRunning(MCMapProcess) Or ThreadIsNotNothingAndAlive(DeleteForRestoreThread) Or ThreadIsNotNothingAndAlive(RestoreThread) Or ThreadIsNotNothingAndAlive(DeleteThread) Then
                MetroMessageBox.Show(MCBackup.Language.Dictionary("Message.MCBackupIsWorking"), MCBackup.Language.Dictionary("Message.Caption.MCBackupIsWorking"), MessageBoxButton.OK, MessageBoxImage.Question)
                e.Cancel = True
            End If

            If e.Cancel Then Exit Sub

            NotifyIcon.Visible = False
            NotifyIcon.Dispose()

            My.Settings.SidebarWidth = GridSidebarColumn.Width
            My.Settings.ListViewWidth = GridListViewColumn.Width

            My.Settings.AutoBkpPrefix = AutoBackupWindow.PrefixTextBox.Text
            My.Settings.AutoBkpSuffix = AutoBackupWindow.SuffixTextBox.Text

            Dim View As CollectionView = DirectCast(CollectionViewSource.GetDefaultView(ListView.ItemsSource), CollectionView)
            My.Settings.ListViewSortBy = View.SortDescriptions(0).PropertyName
            My.Settings.ListViewSortByDirection = View.SortDescriptions(0).Direction

            My.Settings.WindowSize = New Size(Me.Width, Me.Height)
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

#Region "Search Text Box"
    Private Sub SearchTextBox_TextChanged(sender As Object, e As TextChangedEventArgs) Handles SearchTextBox.TextChanged
        If Me.IsLoaded Then
            RefreshBackupsList()
        End If
    End Sub

    Private Sub SearchTextBox_LostFocus(sender As Object, e As RoutedEventArgs) Handles SearchTextBox.LostFocus
        If String.IsNullOrEmpty(SearchTextBox.Text) Then
            SearchTextBox.Text = MCBackup.Language.Dictionary("MainWindow.Search")
            SearchTextBox.Foreground = New SolidColorBrush(Colors.Gray)
        End If
    End Sub

    Private Sub SearchTextBox_GotFocus(sender As Object, e As RoutedEventArgs) Handles SearchTextBox.GotFocus
        If SearchTextBox.Text = MCBackup.Language.Dictionary("MainWindow.Search") Then
            SearchTextBox.Text = ""
            SearchTextBox.Foreground = New SolidColorBrush(Colors.Black)
        End If
    End Sub
#End Region

#Region "ListView"
    Private Sub ListView_ContextMenuOpening(sender As Object, e As ContextMenuEventArgs) Handles ListView.ContextMenuOpening
        Select Case ListView.SelectedItems.Count
            Case Is > 1
                ListViewRestoreItem.IsEnabled = False
                ListViewDeleteItem.IsEnabled = True
                ListViewRenameItem.IsEnabled = False
            Case 1
                ListViewRestoreItem.IsEnabled = True
                ListViewDeleteItem.IsEnabled = True
                ListViewRenameItem.IsEnabled = True
            Case 0
                ListViewRestoreItem.IsEnabled = False
                ListViewDeleteItem.IsEnabled = False
                ListViewRenameItem.IsEnabled = False
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
        If BackupThread IsNot Nothing Then
            If BackupThread.IsAlive Then
                If MetroMessageBox.Show(MCBackup.Language.Dictionary("Message.CancelBackup"), MCBackup.Language.Dictionary("Message.Caption.AreYouSure"), MessageBoxButton.YesNo, MessageBoxImage.Exclamation) = MessageBoxResult.Yes Then
                    If BackupThread.IsAlive Then
                        BackupThread.Abort()
                        Cancel = True
                    End If
                End If
            End If
        End If

        If MCMapProcess IsNot Nothing Then
            If MCMapProcess.HasExited = False Then
                If MetroMessageBox.Show(MCBackup.Language.Dictionary("Message.CancelBackup"), MCBackup.Language.Dictionary("Message.Caption.AreYouSure"), MessageBoxButton.YesNo, MessageBoxImage.Exclamation) = MessageBoxResult.Yes Then
                    If Not MCMapProcess.HasExited Then
                        MCMapProcess.Kill()
                        If IO.File.Exists(My.Settings.BackupsFolderLocation & "\" & BackupInfo(0) & "\thumb.png") Then IO.File.Delete(My.Settings.BackupsFolderLocation & "\" & BackupInfo(0) & "\thumb.png")
                        EnableUI(True)
                        RefreshBackupsList()
                        ReloadBackupGroups()
                        Progress.Value = 0
                        StatusLabel.Content = "Thumbnail Creation Cancelled - Ready"
                        StatusLabel.Refresh()
                        Log.Print("Thumbnail creation cancelled")
                    End If
                End If
            End If
        End If

        If DeleteForRestoreThread IsNot Nothing Then
            If DeleteForRestoreThread.IsAlive Then
                If MetroMessageBox.Show(MCBackup.Language.Dictionary("Message.CancelRestore"), MCBackup.Language.Dictionary("Message.Caption.Warning"), MessageBoxButton.YesNo, MessageBoxImage.Exclamation) = MessageBoxResult.Yes Then
                    If DeleteForRestoreThread.IsAlive Then
                        DeleteForRestoreThread.Abort()
                        Cancel = True
                    End If
                End If
            End If
        End If

        If RestoreThread IsNot Nothing Then
            If RestoreThread.IsAlive Then
                If MetroMessageBox.Show(MCBackup.Language.Dictionary("Message.CancelRestore"), MCBackup.Language.Dictionary("Message.Caption.Warning"), MessageBoxButton.YesNo, MessageBoxImage.Exclamation) = MessageBoxResult.Yes Then
                    If RestoreThread.IsAlive Then
                        RestoreThread.Abort()
                        Cancel = True
                    End If
                End If
            End If
        End If

        If DeleteThread IsNot Nothing Then
            If DeleteThread.IsAlive Then
                If MetroMessageBox.Show(MCBackup.Language.Dictionary("Message.CancelDelete"), MCBackup.Language.Dictionary("Message.Caption.AreYouSure"), MessageBoxButton.YesNo, MessageBoxImage.Exclamation) = MessageBoxResult.Yes Then
                    If DeleteThread.IsAlive Then
                        DeleteThread.Abort()
                        Cancel = True
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
                                 StatusLabel.Content = "Moving backups, please wait..."
                                 ProgressBar.IsIndeterminate = True
                             End Sub)

        For Each Item As ListViewBackupItem In SelectedItems
            Log.Print("Rewriting info.json file for backup '{0}'.", Log.Level.Info, Item.Name)

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
        Next

        Me.Dispatcher.Invoke(Sub()
                                 EnableUI(True)
                                 StatusLabel.Content = "Ready"
                                 ProgressBar.IsIndeterminate = False
                                 RefreshBackupsList()
                             End Sub)
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