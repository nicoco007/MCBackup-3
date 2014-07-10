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

Imports MCBackup.CloseAction

Imports MahApps.Metro
Imports System.Text

Partial Class MainWindow

    Private _CloseType As CloseType
    Public Property CloseType() As CloseType
        Get
            Return _CloseType
        End Get
        Set(value As CloseType)
            _CloseType = value
        End Set
    End Property


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

    Private BackupThread As New Thread(AddressOf Backup_NEW)
    Private DeleteForRestoreThread As New Thread(AddressOf DeleteForRestoreBackgroundWorker_DoWork)
    Private RestoreThread As New Thread(AddressOf RestoreBackgroundWorker_DoWork)
    Private DeleteThread As New Thread(AddressOf DeleteBackgroundWorker_DoWork)
    Private UpdateProgress As Thread

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
        InitializeComponent()

        ThemeManager.ChangeAppStyle(My.Application, ThemeManager.GetAccent(My.Settings.Theme), ThemeManager.GetAppTheme("BaseLight"))

        Splash.Show()

        Splash.ShowStatus("Splash.Status.Starting", "Starting...")

        Log.SPrint("")
        Log.SPrint("---------- Starting MCBackup v{0} @ {1} ----------", Me.ApplicationVersion, Log.DebugTimeStamp())
        Log.Print("OS Name: " & Log.GetWindowsVersion())
        Log.Print("OS Version: " & Environment.OSVersion.Version.Major & "." & Environment.OSVersion.Version.Minor)
        Log.Print("Architecture: " & Log.GetWindowsArch())
        Log.Print(".NET Framework Version: " & Environment.Version.Major & "." & Environment.Version.Minor)
        Log.Print(String.Format("Current Launcher: '{0}'", My.Settings.Launcher))

        Splash.StepProgress()

        Splash.Progress.Value += 1
        Splash.Progress.Refresh()

        Me.Title = "MCBackup " & ApplicationVersion

        Splash.ShowStatus("Splash.Status.LoadingLang", "Loading Language...")
        Splash.StepProgress()

        Dim DefaultLanguage As String = "en_US"

        Select Case CultureInfo.CurrentCulture.ThreeLetterISOLanguageName
            Case "eng"
                DefaultLanguage = "en_US"
            Case "fra"
                DefaultLanguage = "fr_FR"
        End Select

        Splash.StepProgress()
        Try
            If My.Settings.Language = "" Or My.Settings.Language Is Nothing Then
                My.Settings.Language = DefaultLanguage
                MCBackup.Language.Load(My.Settings.Language & ".lang")
            Else
                MCBackup.Language.Load(My.Settings.Language & ".lang")
            End If
        Catch ex As Exception
            ErrorReportDialog.Show("Error: Could not load language file (" & My.Settings.Language & ")! MCBackup will now exit.", ex)
            My.Settings.Language = DefaultLanguage
            My.Settings.Save()
            Me.CloseType = CloseType.ForceClose
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
                Me.CloseType = CloseType.ForceClose
                Me.Close()
                Exit Sub
            End If
        End If

        Log.Print("Set Backups folder location to '" & My.Settings.BackupsFolderLocation & "'")

        GridSidebarColumn.Width = New GridLength(My.Settings.SidebarWidth.Value, GridUnitType.Star)
        GridListViewColumn.Width = New GridLength(My.Settings.ListViewWidth.Value, GridUnitType.Star)

        Splash.ShowStatus("Splash.Status.FindingMinecraft", "Finding Minecraft...")
        Splash.StepProgress()

        If Not IsValidInstallation() Then
            If My.Computer.FileSystem.FileExists(AppData & "\.minecraft\launcher.jar") Then
                My.Settings.MinecraftFolderLocation = AppData & "\.minecraft"
                My.Settings.SavesFolderLocation = AppData & "\.minecraft\saves"
            Else
                Log.Print("Minecraft folder is not valid!", Log.Level.Warning)
                MetroMessageBox.Show("Could not find your Minecraft installation! Please select it using the next dialog.", MCBackup.Language.Dictionary("Message.Caption.Error"), MessageBoxButton.OK, MessageBoxImage.Error)
                Dim SetMinecraftFolderWindow As New SetMinecraftFolderWindow
                SetMinecraftFolderWindow.ShowDialog()
            End If
        End If

        If Not My.Computer.FileSystem.DirectoryExists(My.Settings.MinecraftFolderLocation) Then
            MetroMessageBox.Show("Could not find your Minecraft installation! Please select it using the next dialog.", MCBackup.Language.Dictionary("Message.Caption.Error"), MessageBoxButton.OK, MessageBoxImage.Error)
            Dim SetMinecraftFolderWindow As New SetMinecraftFolderWindow
            SetMinecraftFolderWindow.ShowDialog()
        End If

        Splash.StepProgress()

        If Not My.Settings.SavesFolderLocation = "" Then
            My.Computer.FileSystem.CreateDirectory(My.Settings.SavesFolderLocation)
        End If

        Log.Print("Minecraft folder set to '" & My.Settings.MinecraftFolderLocation & "'")
        Log.Print("Saves folder set to '" & My.Settings.SavesFolderLocation & "'")

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
                Log.Print("Could not connect to content.nicoco007.com, skipping update check...", Log.Level.Warning)
            End Try
        Else
            Log.Print("Update checking disabled, skipping...")
            Splash.StepProgress()
        End If

        Splash.ShowStatus("Splash.Status.Done", "Done.")
        Splash.StepProgress()

        LoadLanguage()

        Splash.Hide()
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
            Items = New List(Of ListViewBackupItem)
            EnableUI(False)
            ProgressBar.IsIndeterminate = True
            StatusLabel.Content = MCBackup.Language.Dictionary("Status.RefreshingBackupsList")
            BGW.RunWorkerAsync()
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
            Dim OriginalFolderName As String = "[ERROR]"    ' 

            Try
                If Not My.Computer.FileSystem.FileExists(Folder.FullName & "\info.mcb") Then
                    Log.Print("'info.mcb' does not exist in folder '{0}'. This folder will not be considered as a backup.", Log.Level.Warning, Folder.Name)
                    DirectoriesToDelete.Add(Folder.Name)
                    Exit Try
                End If

                Using SR As New StreamReader(Folder.FullName & "\info.mcb")
                    Do While SR.Peek <> -1
                        Dim Line As String = SR.ReadLine
                        If Not Line.StartsWith("#") Then
                            If Line.StartsWith("type=") Then
                                Select Case Line.Substring(5)
                                    Case "save"
                                        Type = MCBackup.Language.Dictionary("BackupTypes.Save")
                                    Case "version"
                                        Type = MCBackup.Language.Dictionary("BackupTypes.Version")
                                    Case "everything"
                                        Type = MCBackup.Language.Dictionary("BackupTypes.Everything")
                                End Select
                            ElseIf Line.StartsWith("baseFolderName=") Then
                                OriginalFolderName = Line.Substring(15) ' Set original folder name to "baseFolderName=" line
                            ElseIf Line = "groupName=" & Group And Not (Group = "") And Folder.Name.IndexOf(Search, 0, StringComparison.CurrentCultureIgnoreCase) <> -1 Then
                                If GetFolderDateCreated(Directory.ToString & "\" & Folder.ToString).AddDays(14) < DateTime.Today Then
                                    Dispatcher.Invoke(Sub()
                                                          Items.Add(New ListViewBackupItem(Folder.ToString, GetFolderDateCreated(Directory.ToString & "\" & Folder.ToString), New SolidColorBrush(Color.FromRgb(My.Settings.ListViewTextColorIntensity, 0, 0)), OriginalFolderName, Type))
                                                      End Sub)
                                ElseIf GetFolderDateCreated(Directory.ToString & "\" & Folder.ToString).AddDays(7) < DateTime.Today Then
                                    Dispatcher.Invoke(Sub()
                                                          Items.Add(New ListViewBackupItem(Folder.ToString, GetFolderDateCreated(Directory.ToString & "\" & Folder.ToString), New SolidColorBrush(Color.FromRgb(My.Settings.ListViewTextColorIntensity, My.Settings.ListViewTextColorIntensity, 0)), OriginalFolderName, Type))
                                                      End Sub)
                                Else
                                    Dispatcher.Invoke(Sub()
                                                          Items.Add(New ListViewBackupItem(Folder.ToString, GetFolderDateCreated(Directory.ToString & "\" & Folder.ToString), New SolidColorBrush(Color.FromRgb(0, My.Settings.ListViewTextColorIntensity, 0)), OriginalFolderName, Type))
                                                      End Sub)
                                End If
                            End If
                        End If
                    Loop
                End Using

                If Group = "" And Folder.Name.IndexOf(Search, 0, StringComparison.CurrentCultureIgnoreCase) <> -1 Then
                    If GetFolderDateCreated(Directory.ToString & "\" & Folder.ToString).AddDays(14) < DateTime.Today Then
                        Dispatcher.Invoke(Sub()
                                              Items.Add(New ListViewBackupItem(Folder.ToString, GetFolderDateCreated(Directory.ToString & "\" & Folder.ToString), New SolidColorBrush(Color.FromRgb(My.Settings.ListViewTextColorIntensity, 0, 0)), OriginalFolderName, Type))
                                          End Sub)
                    ElseIf GetFolderDateCreated(Directory.ToString & "\" & Folder.ToString).AddDays(7) < DateTime.Today Then
                        Dispatcher.Invoke(Sub()
                                              Items.Add(New ListViewBackupItem(Folder.ToString, GetFolderDateCreated(Directory.ToString & "\" & Folder.ToString), New SolidColorBrush(Color.FromRgb(My.Settings.ListViewTextColorIntensity, My.Settings.ListViewTextColorIntensity, 0)), OriginalFolderName, Type))
                                          End Sub)
                    Else
                        Dispatcher.Invoke(Sub()
                                              Items.Add(New ListViewBackupItem(Folder.ToString, GetFolderDateCreated(Directory.ToString & "\" & Folder.ToString), New SolidColorBrush(Color.FromRgb(0, My.Settings.ListViewTextColorIntensity, 0)), OriginalFolderName, Type))
                                          End Sub)
                    End If
                End If
            Catch ex As Exception
                Log.Print("An error occured during the backup: " & ex.Message, Log.Level.Severe)
            End Try
        Next

        If DirectoriesToDelete.Count > 0 Then
            Dim SB As New StringBuilder
            SB.AppendLine("The following directories do not seem to be backups:")
            For Each Folder As String In DirectoriesToDelete
                SB.AppendLine("  " & Folder)
            Next
            SB.AppendLine("Would you like to delete them?")
            Dim Result As MessageBoxResult
            Dispatcher.Invoke(Sub()
                                  Result = MetroMessageBox.Show(SB.ToString, "Invalid backups", MessageBoxButton.YesNo, MessageBoxImage.Question)
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

                ThumbnailImage.Source = New BitmapImage(New Uri("pack://application:,,,/Resources/nothumb.png"))
                SidebarOriginalNameContent.Text = "-"
                SidebarOriginalNameContent.ToolTip = MCBackup.Language.Dictionary("MainWindow.Sidebar.NoBackupSelected")
                SidebarTypeContent.Text = "-"
                SidebarTypeContent.ToolTip = MCBackup.Language.Dictionary("MainWindow.Sidebar.NoBackupSelected")

                DescriptionTextBox.Text = MCBackup.Language.Dictionary("MainWindow.Sidebar.Description.NoItem")
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

                DescriptionTextBox.Text = MCBackup.Language.Dictionary("MainWindow.Sidebar.Description.NoItem")
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

                              SidebarPlayerHealth.Visibility = Windows.Visibility.Collapsed
                              SidebarPlayerHunger.Visibility = Windows.Visibility.Collapsed

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
            Using SR As New StreamReader(My.Settings.BackupsFolderLocation & "\" & SelectedItem.Name & "\info.mcb")
                Do While SR.Peek <> -1
                    Dim Line As String = SR.ReadLine
                    If Not Line.StartsWith("#") Then
                        If Line.StartsWith("type=") Then
                            Type = Line.Substring(5)
                        ElseIf Line.StartsWith("baseFolderName=") Then
                            OriginalFolderName = Line.Substring(15) ' Set original folder name to "baseFolderName=" line
                        ElseIf Line.StartsWith("desc=") Then
                            Description = Line.Substring(5)
                        End If
                    End If
                Loop
            End Using
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

        If Type = "save" Then
            Dispatcher.Invoke(Sub()
                                  SidebarPlayerHealth.Visibility = Windows.Visibility.Visible
                                  SidebarPlayerHunger.Visibility = Windows.Visibility.Visible
                                  SidebarPlayerHealthGrid.Children.Clear()
                                  SidebarPlayerHungerGrid.Children.Clear()
                              End Sub)
            Try
                Dim World As NbtWorld = AnvilWorld.Open(My.Settings.BackupsFolderLocation & "\" & SelectedItem.Name)

                Dispatcher.Invoke(Sub()
                                      For i As Integer = 0 To World.Level.Player.Health \ 2 - 1
                                          SidebarPlayerHealthGrid.Children.Add(New MinecraftIcons.PlayerStats.Heart(New Thickness(SidebarPlayerHealthGrid.Children.Count * 10, 0, 0, 0), MinecraftIcons.PlayerStats.State.Full))
                                      Next
                                      If World.Level.Player.Health Mod 2 <> 0 Then
                                          SidebarPlayerHealthGrid.Children.Add(New MinecraftIcons.PlayerStats.Heart(New Thickness(SidebarPlayerHealthGrid.Children.Count * 10, 0, 0, 0), MinecraftIcons.PlayerStats.State.Half))
                                      End If
                                      For i As Integer = 0 To (20 - World.Level.Player.Health) \ 2 - 1
                                          SidebarPlayerHealthGrid.Children.Add(New MinecraftIcons.PlayerStats.Heart(New Thickness(SidebarPlayerHealthGrid.Children.Count * 10, 0, 0, 0), MinecraftIcons.PlayerStats.State.Empty))
                                      Next

                                      For i As Integer = 0 To World.Level.Player.HungerLevel \ 2 - 1
                                          SidebarPlayerHungerGrid.Children.Add(New MinecraftIcons.PlayerStats.Hunger(New Thickness(90 - SidebarPlayerHungerGrid.Children.Count * 10, 0, 0, 0), MinecraftIcons.PlayerStats.State.Full))
                                      Next
                                      If World.Level.Player.HungerLevel Mod 2 <> 0 Then
                                          SidebarPlayerHungerGrid.Children.Add(New MinecraftIcons.PlayerStats.Hunger(New Thickness(90 - SidebarPlayerHungerGrid.Children.Count * 10, 0, 0, 0), MinecraftIcons.PlayerStats.State.Half))
                                      End If
                                      For i As Integer = 0 To (20 - World.Level.Player.HungerLevel) \ 2 - 1
                                          SidebarPlayerHungerGrid.Children.Add(New MinecraftIcons.PlayerStats.Hunger(New Thickness(90 - SidebarPlayerHungerGrid.Children.Count * 10, 0, 0, 0), MinecraftIcons.PlayerStats.State.Empty))
                                      Next
                                  End Sub)
            Catch ex As Exception
                Dispatcher.Invoke(Sub() ErrorReportDialog.Show("An error occured while trying to load world info.", ex))
            End Try
        Else
            Dispatcher.Invoke(Sub()
                                  SidebarPlayerHealth.Visibility = Windows.Visibility.Collapsed
                                  SidebarPlayerHunger.Visibility = Windows.Visibility.Collapsed
                              End Sub)
        End If
    End Sub

    Public Sub LoadLanguage()
        Try
            BackupButton.Content = MCBackup.Language.Dictionary("MainWindow.BackupButton.Content")
            RestoreButton.Content = MCBackup.Language.Dictionary("MainWindow.RestoreButton.Content")
            DeleteButton.Content = MCBackup.Language.Dictionary("MainWindow.DeleteButton.Content")
            RenameButton.Content = MCBackup.Language.Dictionary("MainWindow.RenameButton.Content")
            CullButton.Content = MCBackup.Language.Dictionary("MainWindow.CullButton.Content")

            AutomaticBackupButton.Content = MCBackup.Language.Dictionary("MainWindow.AutomaticBackupButton.Content") & " >>"

            NameColumnHeader.Content = MCBackup.Language.Dictionary("MainWindow.ListView.Columns(0).Header")
            DateCreatedColumnHeader.Content = MCBackup.Language.Dictionary("MainWindow.ListView.Columns(1).Header")
            TypeColumnHeader.Content = MCBackup.Language.Dictionary("MainWindow.ListView.Columns(2).Header")

            SidebarOriginalNameLabel.Text = MCBackup.Language.Dictionary("MainWindow.Sidebar.OriginalNameLabel.Text")
            SidebarTypeLabel.Text = MCBackup.Language.Dictionary("MainWindow.Sidebar.TypeLabel.Text")
            SidebarDescriptionLabel.Text = MCBackup.Language.Dictionary("MainWindow.Sidebar.DescriptionLabel.Text")

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
        Catch ex As Exception
            ErrorReportDialog.Show("Could not load language.", ex)
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
        Dim t As New Thread(AddressOf Backup_NEW)
        t.Start()
    End Sub

    Private Sub Backup_NEW()
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

            ' Do until percent complete is equal to 100
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
                                         ProgressBar.Value = PercentComplete
                                         ProgressBar.Refresh()
                                     End Sub)

                If Environment.OSVersion.Version.Major > 5 Then
                    TaskbarManager.Instance.SetProgressValue(PercentComplete, 100)
                End If

                If Cancel And BackupThread.IsAlive = False Then
                    Dispatcher.Invoke(Sub()
                                          StatusLabel.Content = "Reverting changes..."
                                          ProgressBar.IsIndeterminate = True
                                      End Sub)
                    My.Computer.FileSystem.DeleteDirectory(My.Settings.BackupsFolderLocation & "\" & BackupInfo(0), FileIO.DeleteDirectoryOption.DeleteAllContents)
                    Dispatcher.Invoke(Sub()
                                          Cancelled()
                                      End Sub)
                    Exit Sub
                End If
            Loop

            ' Stop backup stopwatch
            BackupStopwatch.Stop()

            Using SW As New StreamWriter(My.Settings.BackupsFolderLocation & "\" & BackupInfo(0) & "\info.mcb") ' Create information file (stores description, type, folder name, group name, launcher and modpack)
                SW.WriteLine("######## WARNING!  DO NOT EDIT THIS FILE! ########")  ' Write warning in case a snooping snooper snoops in the file
                SW.WriteLine("## YOU COULD DAMAGE YOUR MINECRAFT INSTALLATION ##")
                SW.WriteLine("baseFolderName=" & BackupInfo(2).Split("\").Last)     ' Write save/version folder name
                SW.WriteLine("type=" & BackupInfo(3))                               ' Write type
                SW.WriteLine("desc=" & BackupInfo(1))                               ' Write description
                SW.WriteLine("groupName=" & BackupInfo(4))                          ' Write group name
                SW.WriteLine("launcher=" & BackupInfo(5))                           ' Write launcher ID
                SW.WriteLine("modpack=" & BackupInfo(6))                            ' Write modpack name
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
                                      CreateThumb(BackupInfo(2))
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
                                      StatusLabel.Content = MCBackup.Language.Dictionary("Status.BackupComplete")
                                      StatusLabel.Refresh()
                                      ProgressBar.Value = 100
                                  End Sub)

                If My.Settings.ShowBalloonTips Then NotifyIcon.ShowBalloonTip(2000, MCBackup.Language.Dictionary("BalloonTip.Title.BackupComplete"), MCBackup.Language.Dictionary("BalloonTip.BackupComplete"), System.Windows.Forms.ToolTipIcon.Info)
            End If
        Catch ex As Exception
            If My.Settings.ShowBalloonTips Then NotifyIcon.ShowBalloonTip(2000, MCBackup.Language.Dictionary("BalloonTip.Title.BackupError"), MCBackup.Language.Dictionary("BalloonTip.BackupError"), System.Windows.Forms.ToolTipIcon.Error)
            ErrorReportDialog.Show(MCBackup.Language.Dictionary("Exception.Backup"), ex)
        End Try
    End Sub

    Private ThumbnailThread As New Thread(Sub() ThumbnailBackgroundWorker_DoWork(Nothing))

    Private Sub CreateThumb(Path As String)
        StatusLabel.Content = String.Format(MCBackup.Language.Dictionary("Status.CreatingThumb"), 0)
        ThumbnailThread = New Thread(Sub() ThumbnailBackgroundWorker_DoWork(Path))
        ThumbnailThread.Start()
    End Sub

    Private Sub ThumbnailBackgroundWorker_DoWork(WorldPath As String)
        Try
            Dim MCMap As New Process

            With MCMap.StartInfo
                .FileName = Chr(34) & StartupPath & "\mcmap\mcmap.exe" & Chr(34)
                .WorkingDirectory = StartupPath & "\mcmap\"
                .Arguments = String.Format(" -from -15 -15 -to 15 15 -file ""{0}\{1}\thumb.png"" ""{2}""", My.Settings.BackupsFolderLocation, BackupInfo(0), WorldPath)
                .CreateNoWindow = True
                .UseShellExecute = False
                .RedirectStandardError = True
                .RedirectStandardOutput = True
            End With

            AddHandler MCMap.OutputDataReceived, AddressOf MCMap_DataReceived
            AddHandler MCMap.ErrorDataReceived, AddressOf MCMap_DataReceived

            With MCMap
                .Start()
                .BeginOutputReadLine()
                .BeginErrorReadLine()
                .WaitForExit()
            End With
        Catch ex As Exception
            If TypeOf ex Is ThreadAbortException Then
                Log.Print("Thumbnail creation aborted!")
            Else
                Dispatcher.Invoke(Sub() ErrorReportDialog.Show("An error occured while trying to create the thumbnail.", ex))
            End If
        End Try
    End Sub

    Private Sub MCMap_DataReceived(sender As Object, e As DataReceivedEventArgs)
        Dim StepNumber As Integer
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
            UpdateThumbProgress(PercentComplete)
            StatusLabel_Content(String.Format(MCBackup.Language.Dictionary("Status.CreatingThumb"), Int(PercentComplete)))
        ElseIf e.Data = "Job complete." Then
            Dispatcher.Invoke(Sub() ThumbnailGenerationComplete())
        End If
    End Sub

    Private Sub ThumbnailGenerationComplete()
        EnableUI(True)
        UpdateThumbProgress(100)
        RefreshBackupsList()
        StatusLabel.Content = MCBackup.Language.Dictionary("Status.BackupComplete")
        StatusLabel.Refresh()
        If My.Settings.ShowBalloonTips Then NotifyIcon.ShowBalloonTip(2000, MCBackup.Language.Dictionary("BalloonTip.Title.BackupComplete"), MCBackup.Language.Dictionary("BalloonTip.BackupComplete"), System.Windows.Forms.ToolTipIcon.Info)
        Log.Print("Backup Complete")
    End Sub
#End Region

#Region "Restore"
    Private RestoreStopWatch As New Stopwatch

    Private Sub RestoreButton_Click(sender As Object, e As EventArgs) Handles RestoreButton.Click, ListViewRestoreItem.Click
        If MetroMessageBox.Show(MCBackup.Language.Dictionary("Message.RestoreAreYouSure"), MCBackup.Language.Dictionary("Message.Caption.AreYouSure"), MessageBoxButton.YesNo, MessageBoxImage.Question) = Forms.DialogResult.Yes Then
            EnableUI(False)
            Log.Print("Starting Restore")
            RestoreInfo(0) = ListView.SelectedItems(0).Name ' Set place 0 of RestoreInfo array to the backup name

            Dim BaseFolderName As String = "", Launcher As String = "", Modpack As String = ""

            Using SR As New StreamReader(My.Settings.BackupsFolderLocation & "\" & RestoreInfo(0) & "\info.mcb")
                Do While SR.Peek <> -1
                    Dim Line As String = SR.ReadLine
                    If Not Line.StartsWith("#") Then
                        If Line.StartsWith("baseFolderName=") Then
                            BaseFolderName = Line.Substring(15)
                        ElseIf Line.StartsWith("type=") Then
                            RestoreInfo(2) = Line.Substring(5)
                        ElseIf Line.StartsWith("launcher=") Then
                            Launcher = Line.Substring(9)
                        ElseIf Line.StartsWith("modpack=") Then
                            Modpack = Line.Substring(8)
                        End If
                    End If
                Loop
            End Using

            If Launcher = "" Then Launcher = "minecraft"

            If Launcher <> My.Settings.Launcher Then
                MetroMessageBox.Show(String.Format("This backup is not compatible with your current configuration! It is designed for '{0}' installations.", Launcher), MCBackup.Language.Dictionary("Message.Caption.Error"), MessageBoxButton.OK, MessageBoxImage.Error)
                Exit Sub
            End If

            Select Case RestoreInfo(2)
                Case "save"
                    Select Case My.Settings.Launcher
                        Case "minecraft"
                            RestoreInfo(1) = My.Settings.SavesFolderLocation & "\" & BaseFolderName
                        Case "technic"
                            RestoreInfo(1) = My.Settings.MinecraftFolderLocation & "\modpacks\" & Modpack & "\saves\" & BaseFolderName
                        Case "ftb"
                            RestoreInfo(1) = My.Settings.MinecraftFolderLocation & "\" & Modpack & "\minecraft\saves\" & BaseFolderName
                        Case "atlauncher"
                            RestoreInfo(1) = My.Settings.MinecraftFolderLocation & "\Instances\" & Modpack & "\saves\" & BaseFolderName
                    End Select
                Case "version"
                    Select Case My.Settings.Launcher
                        Case "minecraft"
                            RestoreInfo(1) = My.Settings.MinecraftFolderLocation & "\versions\" & BaseFolderName
                        Case "technic"
                            RestoreInfo(1) = My.Settings.MinecraftFolderLocation & "\modpacks\" & BaseFolderName
                        Case "ftb"
                            RestoreInfo(1) = My.Settings.MinecraftFolderLocation & "\" & BaseFolderName
                        Case "atlauncher"
                            RestoreInfo(1) = My.Settings.MinecraftFolderLocation & "\Instances\" & BaseFolderName
                    End Select
                Case "everything"
                    RestoreInfo(1) = My.Settings.MinecraftFolderLocation
            End Select

            DeleteForRestoreThread = New Thread(AddressOf DeleteForRestoreBackgroundWorker_DoWork)
            DeleteForRestoreThread.Start()
            ProgressBar.IsIndeterminate = True
            If Environment.OSVersion.Version.Major > 5 Then
                TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Indeterminate)
            End If
            StatusLabel.Content = MCBackup.Language.Dictionary("Status.RemovingOldContent")
            Log.Print("Removing old content")
        Else
            Exit Sub
        End If
    End Sub

    Private Sub DeleteForRestoreBackgroundWorker_DoWork()
        Try
            If My.Computer.FileSystem.DirectoryExists(RestoreInfo(1)) Then
                My.Computer.FileSystem.DeleteDirectory(RestoreInfo(1), FileIO.DeleteDirectoryOption.DeleteAllContents)
            End If

            Me.Dispatcher.Invoke(Sub()
                                     DeleteForRestoreBackgroundWorker_RunWorkerCompleted()
                                 End Sub)
        Catch ex As Exception
            If TypeOf ex Is ThreadAbortException Then
                Log.Print("Restore thread aborted!", Log.Level.Severe)
                Me.Dispatcher.Invoke(Sub()
                                         BackupStopwatch.Stop()
                                         ProgressBar.Value = 0
                                         StatusLabel.Content = "Restore cancelled."
                                     End Sub)
            Else
                If My.Settings.ShowBalloonTips Then NotifyIcon.ShowBalloonTip(2000, MCBackup.Language.Dictionary("BalloonTip.Title.RestoreError"), MCBackup.Language.Dictionary("BalloonTip.RestoreError"), System.Windows.Forms.ToolTipIcon.Error)
                ErrorReportDialog.Show(MCBackup.Language.Dictionary("Exception.Restore"), ex)
            End If
        End Try
    End Sub

    Private Sub DeleteForRestoreBackgroundWorker_RunWorkerCompleted()
        ProgressBar.IsIndeterminate = False
        If Environment.OSVersion.Version.Major > 5 Then
            TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Normal)
        End If
        Log.Print("Removed old content, restoring...")
        RestoreThread = New Thread(AddressOf RestoreBackgroundWorker_DoWork)
        RestoreThread.Start()
        UpdateRestoreProgress()
    End Sub

    Private Sub RestoreBackgroundWorker_DoWork()
        Try
            My.Computer.FileSystem.CopyDirectory(My.Settings.BackupsFolderLocation & "\" & RestoreInfo(0), RestoreInfo(1), True)
            My.Computer.FileSystem.DeleteFile(RestoreInfo(1) & "\info.mcb")
            If My.Computer.FileSystem.FileExists(RestoreInfo(1) & "\thumb.png") Then
                My.Computer.FileSystem.DeleteFile(RestoreInfo(1) & "\thumb.png")
            End If
            Me.Dispatcher.Invoke(Sub()
                                     RestoreBackgroundWorker_RunWorkerCompleted()
                                 End Sub)
        Catch ex As Exception
            If TypeOf ex Is ThreadAbortException Then
                Log.Print("Restore thread aborted!", Log.Level.Severe)
                Me.Dispatcher.Invoke(Sub()
                                         BackupStopwatch.Stop()
                                         ProgressBar.Value = 0
                                         StatusLabel.Content = "Restore cancelled."
                                     End Sub)
            Else
                If My.Settings.ShowBalloonTips Then NotifyIcon.ShowBalloonTip(2000, MCBackup.Language.Dictionary("BalloonTip.Title.RestoreError"), MCBackup.Language.Dictionary("BalloonTip.RestoreError"), System.Windows.Forms.ToolTipIcon.Error)
                ErrorReportDialog.Show(MCBackup.Language.Dictionary("Exception.Restore"), ex)
            End If
        End Try
    End Sub

    Private Sub UpdateRestoreProgress()
        If Not RestoreStopWatch.IsRunning Then
            RestoreStopWatch.Reset()
            RestoreStopWatch.Start()
        End If

        Dim PercentComplete As Integer = 0
        Dim TimeLeft As TimeSpan

        Dim UpdateRestoreProgressBarDelegate As New UpdateProgressBarDelegate(AddressOf ProgressBar.SetValue)

        Do Until PercentComplete = 100
            If My.Computer.FileSystem.DirectoryExists(RestoreInfo(1)) Then
                PercentComplete = GetFolderSize(RestoreInfo(1)) / GetFolderSize(My.Settings.BackupsFolderLocation & "\" & RestoreInfo(0)) * 100

                Dim Copied As Double = GetFolderSize(RestoreInfo(1))
                Dim Speed As Double = Copied / (RestoreStopWatch.ElapsedMilliseconds / 1000 * 1024)

                If PercentComplete < 1 Then
                    StatusLabel.Content = String.Format(MCBackup.Language.Dictionary("Status.Restoring"), PercentComplete, Speed / 1024, "estimating time left...")
                Else
                    If Math.Round(BackupStopwatch.Elapsed.TotalSeconds, 1) Mod 2 < 0.25 Then
                        TimeLeft = TimeSpan.FromSeconds(Math.Round((100 - PercentComplete) * RestoreStopWatch.ElapsedMilliseconds / PercentComplete / 1000 / 5) * 5)
                    End If

                    If TimeLeft.TotalSeconds < 5 Then
                        StatusLabel.Content = String.Format(MCBackup.Language.Dictionary("Status.Restoring"), PercentComplete, Speed / 1024, MCBackup.Language.Dictionary("TimeLeft.LessThanFive"))
                    ElseIf TimeLeft.TotalSeconds < 60 Then
                        StatusLabel.Content = String.Format(MCBackup.Language.Dictionary("Status.Restoring"), PercentComplete, Speed / 1024, String.Format(MCBackup.Language.Dictionary("TimeLeft.Seconds"), TimeLeft.Seconds))
                    Else
                        StatusLabel.Content = String.Format(MCBackup.Language.Dictionary("Status.Restoring"), PercentComplete, Speed / 1024, String.Format(MCBackup.Language.Dictionary("TimeLeft.MinutesAndSeconds"), TimeLeft.Minutes, TimeLeft.Seconds))
                    End If
                End If

                Dispatcher.Invoke(UpdateRestoreProgressBarDelegate, System.Windows.Threading.DispatcherPriority.Background, New Object() {ProgressBar.ValueProperty, Convert.ToDouble(PercentComplete)})
                If Environment.OSVersion.Version.Major > 5 Then
                    TaskbarManager.Instance.SetProgressValue(PercentComplete, 100)
                End If
            End If
        Loop

        StatusLabel.Content = MCBackup.Language.Dictionary("Status.RestoreComplete")
        If My.Settings.ShowBalloonTips Then NotifyIcon.ShowBalloonTip(2000, MCBackup.Language.Dictionary("BalloonTip.Title.RestoreComplete"), MCBackup.Language.Dictionary("BalloonTip.RestoreComplete"), System.Windows.Forms.ToolTipIcon.Info)
        Log.Print("Restore Complete")
        RefreshBackupsList()
    End Sub

    Private Sub RestoreBackgroundWorker_RunWorkerCompleted()
        RestoreStopWatch.Stop()
        EnableUI(True)
        ProgressBar.Value = 100
    End Sub
#End Region

#Region "Functions"
    Private Function GetFolderSize(FolderPath As String)
        Try
            Dim FSO As FileSystemObject = New FileSystemObject
            Return FSO.GetFolder(FolderPath).Size ' Get FolderPath's size
        Catch ex As Exception
            ErrorReportDialog.Show(String.Format("Could not find {0}'s size:", FolderPath), ex)
        End Try
        Return 0
    End Function

    Public Function GetFolderDateCreated(FolderPath As String)
        Try
            Dim FSO As FileSystemObject = New FileSystemObject
            Return FSO.GetFolder(FolderPath).DateCreated ' Get FolderPath's date of creation
        Catch ex As Exception
            ErrorReportDialog.Show(String.Format("Could not find {0}'s creation date:", FolderPath), ex)
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
            ErrorReportDialog.Show(String.Format("Could not convert source {0} to bitmap:", Source), ex)
        End Try
        Return Nothing
    End Function

    Private Sub UpdateThumbProgress(Value As Double)
        Dim UpdateProgressBarDelegate As New UpdateProgressBarDelegate(AddressOf ProgressBar.SetValue)
        Dispatcher.Invoke(UpdateProgressBarDelegate, System.Windows.Threading.DispatcherPriority.Background, New Object() {ProgressBar.ValueProperty, Value})
        If Environment.OSVersion.Version.Major > 5 Then
            TaskbarManager.Instance.SetProgressValue(Value, 100)
        End If
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
                                         ProgressBar.Value = 0
                                         StatusLabel.Content = "Delete cancelled."
                                     End Sub)
            Else
                If My.Settings.ShowBalloonTips Then NotifyIcon.ShowBalloonTip(2000, MCBackup.Language.Dictionary("BalloonTip.Title.DeleteError"), MCBackup.Language.Dictionary("BalloonTip.DeleteError"), System.Windows.Forms.ToolTipIcon.Error)
                ErrorReportDialog.Show(MCBackup.Language.Dictionary("Exception.Delete"), ex)
            End If
        End Try
    End Sub

    Private Sub DeleteBackgroundWorker_RunWorkerCompleted()
        EnableUI(True)
        StatusLabel.Content = MCBackup.Language.Dictionary("Status.DeleteComplete")
        ProgressBar.IsIndeterminate = False
        If Environment.OSVersion.Version.Major > 5 Then
            TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress)
        End If
        RefreshBackupsList()
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
        Me.CloseType = CloseType.ForceClose
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
        If Not CloseType = CloseType.ForceClose Then
            Dim CloseToTrayDialog As New CloseToTrayDialog
            CloseToTrayDialog.Owner = Me
            If My.Settings.SaveCloseState Then
                If My.Settings.CloseToTray Then
                    CloseType = CloseType.CloseToTray
                Else
                    CloseType = CloseType.CloseCompletely
                End If
            Else
                CloseToTrayDialog.ShowDialog()
            End If

            Select Case CloseType
                Case CloseType.CloseToTray
                    e.Cancel = True
                    Me.Hide()
                    If My.Settings.FirstCloseToTray Then
                        NotifyIcon.ShowBalloonTip(2000, MCBackup.Language.Dictionary("BalloonTip.Title.RunningBackground"), MCBackup.Language.Dictionary("BalloonTip.RunningBackground"), System.Windows.Forms.ToolTipIcon.Info)
                        My.Settings.FirstCloseToTray = False
                    End If
                    Log.Print("Closing to tray")
                    Exit Sub
                Case CloseType.CloseCompletely
                    Exit Select
                Case CloseType.Cancel
                    e.Cancel = True
                    Exit Sub
            End Select

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

            Log.Print("Someone is closing me!")
            My.Settings.Save()
        End If
    End Sub
#End Region

#Region "Toolbar"
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

    Private Function IsValidInstallation()
        Select Case My.Settings.Launcher
            Case "minecraft"
                If My.Computer.FileSystem.FileExists(My.Settings.MinecraftFolderLocation & "\launcher.jar") Then
                    Return True
                End If
            Case "technic"
                If My.Computer.FileSystem.FileExists(My.Settings.MinecraftFolderLocation & "\settings.json") Then
                    Return True
                End If
            Case "ftb"
                If My.Computer.FileSystem.DirectoryExists(My.Settings.MinecraftFolderLocation & "\authlib") Then
                    Return True
                End If
            Case "atlauncher"
                If My.Computer.FileSystem.DirectoryExists(My.Settings.MinecraftFolderLocation & "\Configs") Then
                    Return True
                End If
        End Select
        Return False
    End Function

    Private Sub CancelButton_Click(sender As Object, e As RoutedEventArgs) Handles CancelButton.Click
        If BackupThread.IsAlive Then
            If MetroMessageBox.Show("Are you sure you want to cancel the backup?", MCBackup.Language.Dictionary("Message.Caption.AreYouSure"), MessageBoxButton.YesNo, MessageBoxImage.Exclamation) = MessageBoxResult.Yes Then
                BackupThread.Abort()
            End If
        End If

        If ThumbnailThread.IsAlive Then
            If MetroMessageBox.Show("Are you sure you want to cancel the thumbnail creation? You can disable this feature in the settings.", MCBackup.Language.Dictionary("Message.Caption.AreYouSure"), MessageBoxButton.YesNo, MessageBoxImage.Exclamation) = MessageBoxResult.Yes Then
                ThumbnailThread.Abort()
            End If
        End If

        If DeleteForRestoreThread.IsAlive Then
            If MetroMessageBox.Show("WARNING!" & vbNewLine & "Cancelling a restore operation can seriously damage your Minecraft installlation!" & vbNewLine & vbNewLine & "Are you sure you want to cancel the restore?", "WARNING!", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) = MessageBoxResult.Yes Then
                DeleteForRestoreThread.Abort()
            End If
        End If

        If RestoreThread.IsAlive Then
            If MetroMessageBox.Show("WARNING!" & vbNewLine & "Cancelling a restore operation can seriously damage your Minecraft installlation!" & vbNewLine & vbNewLine & "Are you sure you want to cancel the restore?", "WARNING!", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) = MessageBoxResult.Yes Then
                RestoreThread.Abort()
            End If
        End If

        If DeleteThread.IsAlive Then
            If MetroMessageBox.Show("Are you sure you want to cancel the delete?", MCBackup.Language.Dictionary("Message.Caption.AreYouSure"), MessageBoxButton.YesNo, MessageBoxImage.Exclamation) = MessageBoxResult.Yes Then
                DeleteThread.Abort()
            End If
        End If

        Cancel = True
    End Sub

    Private Sub Cancelled()
        ProgressBar.Value = 0
        ProgressBar.IsIndeterminate = False
        StatusLabel.Content = "Operation cancelled - Ready"
        EnableUI(True)
    End Sub

    Private Sub EnableUI(IsEnabled As Boolean)
        BackupButton.IsEnabled = IsEnabled
        RestoreButton.IsEnabled = IsEnabled
        DeleteButton.IsEnabled = IsEnabled
        RenameButton.IsEnabled = IsEnabled
        CancelButton.IsEnabled = Not IsEnabled
        GroupsTabControl.IsEnabled = IsEnabled
        ListView.IsEnabled = IsEnabled
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

Public Class MinecraftIcons
    Public Class PlayerStats
        Public Class Heart
            Inherits Image
            Public Sub New(Margin As Thickness, State As State)
                Select Case State
                    Case PlayerStats.State.Full
                        Me.Source = New BitmapImage(New Uri("pack://application:,,,/Resources/NBTInfo/heart_full.png"))
                    Case PlayerStats.State.Half
                        Me.Source = New BitmapImage(New Uri("pack://application:,,,/Resources/NBTInfo/heart_half.png"))
                    Case PlayerStats.State.Empty
                        Me.Source = New BitmapImage(New Uri("pack://application:,,,/Resources/NBTInfo/heart_empty.png"))
                End Select

                Me.Width = 9
                Me.Height = 9
                Me.HorizontalAlignment = Windows.HorizontalAlignment.Left
                Me.VerticalAlignment = Windows.VerticalAlignment.Stretch
                Me.Stretch = Windows.Media.Stretch.None
                Me.Margin = Margin
            End Sub
        End Class

        Public Class Hunger
            Inherits Image
            Public Sub New(Margin As Thickness, State As State)
                Select Case State
                    Case PlayerStats.State.Full
                        Me.Source = New BitmapImage(New Uri("pack://application:,,,/Resources/NBTInfo/hunger_full.png"))
                    Case PlayerStats.State.Half
                        Me.Source = New BitmapImage(New Uri("pack://application:,,,/Resources/NBTInfo/hunger_half.png"))
                    Case PlayerStats.State.Empty
                        Me.Source = New BitmapImage(New Uri("pack://application:,,,/Resources/NBTInfo/hunger_empty.png"))
                End Select

                Me.Width = 9
                Me.Height = 9
                Me.HorizontalAlignment = Windows.HorizontalAlignment.Left
                Me.VerticalAlignment = Windows.VerticalAlignment.Stretch
                Me.Stretch = Windows.Media.Stretch.None
                Me.Margin = Margin
            End Sub
        End Class

        Public Enum State
            Full
            Half
            Empty
        End Enum
    End Class
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