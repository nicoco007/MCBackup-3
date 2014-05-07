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

Imports MCBackup.CloseAction
Imports System.Globalization
Imports MahApps.Metro
Imports System.Threading

Partial Class MainWindow
#Region "Variables"
    Private AppData As String = Environ("APPDATA")
    Public BackupInfo(4) As String
    Public RestoreInfo(2) As String

    Private FolderBrowserDialog As New System.Windows.Forms.FolderBrowserDialog

    Private BackupBackgroundWorker As New BackgroundWorker()
    Private DeleteForRestoreBackgroundWorker As New BackgroundWorker()
    Private RestoreBackgroundWorker As New BackgroundWorker()
    Private DeleteBackgroundWorker As New BackgroundWorker()

    Public StartupPath As String = Directory.GetCurrentDirectory()
    Public ApplicationVersion As String = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()
    Public LatestVersion As String

    Public WithEvents NotifyIcon As New System.Windows.Forms.NotifyIcon

    Public AutoBackupWindow As New AutoBackup
    Private Splash As New Splash

    Public Shared ListViewItems As New ArrayList
#End Region

#Region "Load"
    Public Sub New()
        Splash.Show()

        Splash.ShowStatus("Splash.Status.Starting", "Starting...")

        Log.StartNew()
        Log.Print("Starting MCBackup")

        ThemeManager.ChangeTheme(My.Application, New Accent(My.Settings.Theme, New Uri("pack://application:,,,/MahApps.Metro;component/Styles/Accents/" & My.Settings.Theme & ".xaml")), Theme.Light)

        Splash.StepProgress()

        InitializeComponent()

        AddHandler BackupBackgroundWorker.DoWork, New DoWorkEventHandler(AddressOf BackupBackgroundWorker_DoWork)
        AddHandler BackupBackgroundWorker.RunWorkerCompleted, New RunWorkerCompletedEventHandler(AddressOf BackupBackgroundWorker_RunWorkerCompleted)
        AddHandler DeleteForRestoreBackgroundWorker.DoWork, New DoWorkEventHandler(AddressOf DeleteForRestoreBackgroundWorker_DoWork)
        AddHandler DeleteForRestoreBackgroundWorker.RunWorkerCompleted, New RunWorkerCompletedEventHandler(AddressOf DeleteForRestoreBackgroundWorker_RunWorkerCompleted)
        AddHandler RestoreBackgroundWorker.DoWork, New DoWorkEventHandler(AddressOf RestoreBackgroundWorker_DoWork)
        AddHandler RestoreBackgroundWorker.RunWorkerCompleted, New RunWorkerCompletedEventHandler(AddressOf RestoreBackgroundWorker_RunWorkerCompleted)
        AddHandler DeleteBackgroundWorker.DoWork, New DoWorkEventHandler(AddressOf DeleteBackgroundWorker_DoWork)
        AddHandler DeleteBackgroundWorker.RunWorkerCompleted, New RunWorkerCompletedEventHandler(AddressOf DeleteBackgroundWorker_RunWorkerCompleted)

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
            Log.Print("Could not load language file '" & My.Settings.Language & "': " & ex.Message, Log.Type.Severe)
            ErrorWindow.Show("Error: Could not load language file (" & My.Settings.Language & ")! MCBackup will now exit.", ex)
            My.Settings.Language = DefaultLanguage
            My.Settings.Save()
            Me.ClsType = CloseType.ForceClose
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

        Main.ListView.Opacity = My.Settings.InterfaceOpacity / 100
        Main.Sidebar.Background = New SolidColorBrush(Color.FromArgb(My.Settings.InterfaceOpacity * 2.55, 255, 255, 255))

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
                Me.ClsType = CloseType.ForceClose
                Me.Close()
                Exit Sub
            End If
        End If

        Log.Print("Set Backups folder location to '" & My.Settings.BackupsFolderLocation & "'")

        GridSidebarColumn.Width = New GridLength(My.Settings.SidebarWidth.Value, GridUnitType.Star)
        GridListViewColumn.Width = New GridLength(My.Settings.ListViewWidth.Value, GridUnitType.Star)

        Splash.StepProgress()
    End Sub

    Private Sub Main_Loaded(sender As Object, e As RoutedEventArgs) Handles MyBase.Loaded
        Me.Hide()

        If My.Settings.CheckForUpdates Then
            Log.Print("Searching for updates...")
            Splash.ShowStatus("Splash.Status.CheckingUpdates", "Checking for Updates...")
            Splash.StepProgress()

            Log.Print("Connecting to http://content.nicoco007.com...")
            Try
                My.Computer.Network.Ping("content.nicoco007.com", 1000)
                Log.Print("Successfully connected.")
            Catch ex As Exception
                Log.Print("Could not connect to content.nicoco007.com, skipping update check...", Log.Type.Warning)
                Load2()
                Exit Sub
            End Try

            Splash.StepProgress()

            Dim WebClient As New WebClient
            AddHandler WebClient.DownloadStringCompleted, AddressOf WebClient_DownloadedStringAsync
            WebClient.DownloadStringAsync(New Uri("http://content.nicoco007.com/downloads/mcbackup-3/version"))
        Else
            Log.Print("Update checking disabled, skipping...")
            Splash.StepProgress()
            Splash.StepProgress()
            Load2()
        End If
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
        Load2()
    End Sub

    Private Sub Load2()
        Splash.ShowStatus("Splash.Status.FindingMinecraft", "Finding Minecraft...")
        Splash.StepProgress()

        If Not My.Computer.FileSystem.FileExists(My.Settings.MinecraftFolderLocation & "\launcher.jar") Then ' Check if saved directory exists AND still has Minecraft installed in it
            If My.Computer.FileSystem.FileExists(AppData & "\.minecraft\launcher.jar") Then ' If not, check for the usual Minecraft folder location
                My.Settings.MinecraftFolderLocation = AppData & "\.minecraft" ' Set folder location to default Minecraft folder location
                My.Settings.SavesFolderLocation = My.Settings.MinecraftFolderLocation & "\saves"
            Else
                MetroMessageBox.Show(MCBackup.Language.Dictionary("Message.NoMinecraftInstallError"), MCBackup.Language.Dictionary("Message.Caption.Error"), MessageBoxButton.OK, MessageBoxImage.Error)
                Log.Print("Minecraft folder not found", Log.Type.Warning)
                MinecraftFolderSearch()
                Exit Sub
            End If
        End If

        Splash.StepProgress()

        My.Computer.FileSystem.CreateDirectory(My.Settings.SavesFolderLocation)

        Log.Print("Minecraft folder set to '" & My.Settings.MinecraftFolderLocation & "'")
        Log.Print("Saves folder set to '" & My.Settings.SavesFolderLocation & "'")

        RefreshBackupsList()
        ReloadBackupGroups()

        Splash.ShowStatus("Splash.Status.Done", "Done.")
        Splash.StepProgress()

        LoadLanguage()

        Splash.Hide()
        Me.Show()
    End Sub

    Private Sub MinecraftFolderSearch()
        If FolderBrowserDialog.ShowDialog = Windows.Forms.DialogResult.OK Then
            If My.Computer.FileSystem.FileExists(FolderBrowserDialog.SelectedPath & "\launcher.jar") Then ' Check if Minecraft exists in that folder
                MetroMessageBox.Show(String.Format(MCBackup.Language.Dictionary("Message.Info.MinecraftFolderSetTo"), FolderBrowserDialog.SelectedPath), MCBackup.Language.Dictionary("Message.Caption.Information"), MessageBoxButton.OK, MessageBoxImage.Error) ' Tell user that folder has been selected successfully
                My.Settings.MinecraftFolderLocation = FolderBrowserDialog.SelectedPath
                My.Settings.SavesFolderLocation = My.Settings.MinecraftFolderLocation & "\saves"
                Log.Print("Minecraft folder set to '" & My.Settings.MinecraftFolderLocation & "'")
                Log.Print("Saves folder set to '" & My.Settings.SavesFolderLocation & "'")
                Exit Sub
            Else
                If MetroMessageBox.Show(MCBackup.Language.Dictionary("Message.NotInstalledInFolder"), MCBackup.Language.Dictionary("Message.Caption.Error"), MessageBoxButton.YesNo, MessageBoxImage.Error) = Windows.Forms.DialogResult.Yes Then ' Ask if user wants to try finding folder again
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
        If Me.IsLoaded Then
            ListView.IsEnabled = False
            GroupsTabControl.IsEnabled = False
            ProgressBar.IsIndeterminate = True
            StatusLabel.Content = MCBackup.Language.Dictionary("Status.RefreshingBackupsList")
            Dim Thread = New Thread(AddressOf RefreshBackupsList_Thread)
            Thread.Start()
        End If
    End Sub

    Private Sub RefreshBackupsList_Thread()
        If ListView.Dispatcher.CheckAccess() Then
            Dim Group As String = GroupsTabControl.SelectedItem
            Dim Search As String = ""
            If SearchTextBox.Text <> MCBackup.Language.Dictionary("MainWindow.Search") Then
                Search = SearchTextBox.Text
            End If

            Dim Directory As New IO.DirectoryInfo(My.Settings.BackupsFolderLocation) ' Create a DirectoryInfo variable for the backups folder

            Dim Items As New List(Of ListViewBackupItem)()

            For Each Folder As DirectoryInfo In Directory.GetDirectories ' For each folder in the backups folder
                Dim Type As String = "[ERROR]"                  ' Create variables with default value [ERROR], in case one of the values doesn't exist
                Dim OriginalFolderName As String = "[ERROR]"    ' 

                Try
                    If Not My.Computer.FileSystem.FileExists(Folder.FullName & "\info.mcb") Then
                        Log.Print(String.Format("'info.mcb' does not exist in folder '{0}'. This folder will not be considered as a backup.", Folder.Name), Log.Type.Warning)
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
                                ElseIf Line = "groupName=" & Group And Not (Group = "All") And Folder.Name.IndexOf(Search, 0, StringComparison.CurrentCultureIgnoreCase) <> -1 Then
                                    If GetFolderDateCreated(Directory.ToString & "\" & Folder.ToString).AddDays(14) < DateTime.Today Then
                                        Items.Add(New ListViewBackupItem(Folder.ToString, GetFolderDateCreated(Directory.ToString & "\" & Folder.ToString), New SolidColorBrush(Color.FromRgb(My.Settings.ListViewTextColorIntensity, 0, 0)), OriginalFolderName, Type))
                                    ElseIf GetFolderDateCreated(Directory.ToString & "\" & Folder.ToString).AddDays(7) < DateTime.Today Then
                                        Items.Add(New ListViewBackupItem(Folder.ToString, GetFolderDateCreated(Directory.ToString & "\" & Folder.ToString), New SolidColorBrush(Color.FromRgb(My.Settings.ListViewTextColorIntensity, My.Settings.ListViewTextColorIntensity, 0)), OriginalFolderName, Type))
                                    Else
                                        Items.Add(New ListViewBackupItem(Folder.ToString, GetFolderDateCreated(Directory.ToString & "\" & Folder.ToString), New SolidColorBrush(Color.FromRgb(0, My.Settings.ListViewTextColorIntensity, 0)), OriginalFolderName, Type))
                                    End If
                                End If
                            End If
                        Loop
                    End Using

                    If Group = "All" And Folder.Name.IndexOf(Search, 0, StringComparison.CurrentCultureIgnoreCase) <> -1 Then
                        If GetFolderDateCreated(Directory.ToString & "\" & Folder.ToString).AddDays(14) < DateTime.Today Then
                            Items.Add(New ListViewBackupItem(Folder.ToString, GetFolderDateCreated(Directory.ToString & "\" & Folder.ToString), New SolidColorBrush(Color.FromRgb(My.Settings.ListViewTextColorIntensity, 0, 0)), OriginalFolderName, Type))
                        ElseIf GetFolderDateCreated(Directory.ToString & "\" & Folder.ToString).AddDays(7) < DateTime.Today Then
                            Items.Add(New ListViewBackupItem(Folder.ToString, GetFolderDateCreated(Directory.ToString & "\" & Folder.ToString), New SolidColorBrush(Color.FromRgb(My.Settings.ListViewTextColorIntensity, My.Settings.ListViewTextColorIntensity, 0)), OriginalFolderName, Type))
                        Else
                            Items.Add(New ListViewBackupItem(Folder.ToString, GetFolderDateCreated(Directory.ToString & "\" & Folder.ToString), New SolidColorBrush(Color.FromRgb(0, My.Settings.ListViewTextColorIntensity, 0)), OriginalFolderName, Type))
                        End If
                    End If
                Catch ex As Exception
                    Log.Print(ex.Message, Log.Type.Severe)
                End Try
            Next

            ListView.ItemsSource = Items
            ListView.SelectedIndex = -1
            SidebarTitle.Text = String.Format(MCBackup.Language.Dictionary("MainWindow.Sidebar.NumberElements"), Items.Count)

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
            ListView.IsEnabled = True
            GroupsTabControl.IsEnabled = True
        Else
            ListView.Dispatcher.Invoke(Sub() RefreshBackupsList_Thread())
        End If
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
                RestoreButton.IsEnabled = True
                RenameButton.IsEnabled = True ' Allow anything if only 1 item is selected
                DeleteButton.IsEnabled = True

                SidebarTitle.Text = ListView.SelectedItem.Name      'Set sidebar title to backup name
                SidebarTitle.ToolTip = ListView.SelectedItem.Name

                ListViewRestoreItem.IsEnabled = True
                ListViewDeleteItem.IsEnabled = True     'Enable ContextMenu items
                ListViewRenameItem.IsEnabled = True

                If My.Computer.FileSystem.FileExists(My.Settings.BackupsFolderLocation & "\" & ListView.SelectedItem.Name & "\thumb.png") Then
                    Try
                        ThumbnailImage.Source = BitmapFromUri(New Uri(My.Settings.BackupsFolderLocation & "\" & ListView.SelectedItem.Name & "\thumb.png"))
                    Catch ex As Exception
                        ErrorWindow.Show("An error occured while trying to load the backup's thumbnail", ex)
                    End Try
                Else
                    ThumbnailImage.Source = New BitmapImage(New Uri("pack://application:,,,/Resources/nothumb.png"))
                End If

                Dim Type As String = "-"
                Dim OriginalFolderName As String = "-"

                Try
                    Using SR As New StreamReader(My.Settings.BackupsFolderLocation & "\" & ListView.SelectedItem.Name.ToString & "\info.mcb")
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
                                ElseIf Line.StartsWith("desc=") Then
                                    DescriptionTextBox.Text = IIf(String.IsNullOrEmpty(Line.Substring(5)), MCBackup.Language.Dictionary("MainWindow.Sidebar.Description.NoDesc"), Line.Substring(5))
                                End If
                            End If
                        Loop
                    End Using
                Catch ex As Exception
                    Log.Print(ex.Message, Log.Type.Severe)
                End Try

                SidebarOriginalNameContent.Text = OriginalFolderName
                SidebarOriginalNameContent.ToolTip = OriginalFolderName
                SidebarTypeContent.Text = Type
                SidebarTypeContent.ToolTip = Type
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
            ErrorWindow.Show("", ex)
        End Try
    End Sub

    Public Sub ReloadBackupGroups()
        GroupsTabControl.Items.Clear()
        GroupsTabControl.Items.Add("All")
        For Each Group As String In My.Settings.BackupGroups
            GroupsTabControl.Items.Add(Group)
        Next
        GroupsTabControl.SelectedIndex = 0
    End Sub
#End Region

#Region "Backup"
    Private Delegate Sub UpdateProgressBarDelegate(ByVal dp As System.Windows.DependencyProperty, ByVal value As Object)
    Private BackupStopwatch As New Stopwatch

    Private Sub BackupButton_Click(sender As Object, e As EventArgs) Handles BackupButton.Click
        Dim BackupWindow As New Backup
        BackupWindow.Owner = Me
        BackupWindow.ShowDialog()
    End Sub

    Public Sub StartBackup()
        If BackupBackgroundWorker.IsBusy Then
            MetroMessageBox.Show(MCBackup.Language.Dictionary("Message.BackupInProgress"), MCBackup.Language.Dictionary("Message.Caption.Error"), MessageBoxButton.OK, MessageBoxImage.Error)
            Exit Sub
        End If
        Log.Print("Starting new backup (Name: '" & BackupInfo(0) & "'; Description: '" & BackupInfo(1) & "'; Path: '" & BackupInfo(2) & "'; Type: '" & BackupInfo(3) & "')")
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
                SW.WriteLine("######## WARNING!  DO NOT EDIT THIS FILE! ########")
                SW.WriteLine("## YOU COULD DAMAGE YOUR MINECRAFT INSTALLATION ##")
                SW.WriteLine("baseFolderName=" & BackupInfo(2).Split("\").Last) ' Write save/version folder name
                SW.WriteLine("type=" & BackupInfo(3)) ' Write type in file
                SW.WriteLine("desc=" & BackupInfo(1)) ' Write description if file
                SW.Write("groupName=" & BackupInfo(4))
            End Using
        Catch ex As Exception
            ErrorWindow.Show(MCBackup.Language.Dictionary("Exception.Backup"), ex)
            If My.Settings.ShowBalloonTips Then NotifyIcon.ShowBalloonTip(2000, MCBackup.Language.Dictionary("BalloonTip.Title.BackupError"), MCBackup.Language.Dictionary("BalloonTip.BackupError"), System.Windows.Forms.ToolTipIcon.Error)
            Log.Print(ex.Message, Log.Type.Severe)
        End Try
    End Sub

    Private Sub UpdateBackupProgress()
        If Not BackupStopwatch.IsRunning Then
            BackupStopwatch.Reset()
            BackupStopwatch.Start()
        End If

        My.Computer.FileSystem.CreateDirectory(My.Settings.BackupsFolderLocation & "\" & BackupInfo(0))
        Dim PercentComplete As Double = 0

        Dim UpdateProgressBarDelegate As New UpdateProgressBarDelegate(AddressOf ProgressBar.SetValue)

        Do Until Int(PercentComplete) = 100
            PercentComplete = GetFolderSize(My.Settings.BackupsFolderLocation & "\" & BackupInfo(0)) / GetFolderSize(BackupInfo(2)) * 100

            Dim Copied As Double = GetFolderSize(My.Settings.BackupsFolderLocation & "\" & BackupInfo(0))
            Dim ToCopy As Double = GetFolderSize(BackupInfo(2))
            Dim Speed As Double = Copied / (BackupStopwatch.ElapsedMilliseconds / 1000 * 1024)
            Dim TimeLeft As Double = (100 - PercentComplete) * BackupStopwatch.ElapsedMilliseconds / PercentComplete / 1000

            If PercentComplete < 1 Then
                StatusLabel.Content = String.Format(MCBackup.Language.Dictionary("Status.BackingUp"), PercentComplete, Speed / 1024, "?")
            Else
                StatusLabel.Content = String.Format(MCBackup.Language.Dictionary("Status.BackingUp"), PercentComplete, Speed / 1024, TimeLeft)
            End If

            Dispatcher.Invoke(UpdateProgressBarDelegate, System.Windows.Threading.DispatcherPriority.Background, New Object() {ProgressBar.ValueProperty, PercentComplete})
            If Environment.OSVersion.Version.Major > 5 Then
                TaskbarManager.Instance.SetProgressValue(PercentComplete, 100)
            End If
        Loop

        If Environment.OSVersion.Version.Major > 5 Then
            TaskbarManager.Instance.SetProgressValue(100, 100)
        End If

        If BackupInfo(3) = "save" And My.Settings.CreateThumbOnWorld Then
            StatusLabel.Content = String.Format(MCBackup.Language.Dictionary("Status.CreatingThumb"), "0")
            Log.Print("Creating thumbnail")
            CreateThumb(BackupInfo(2))
        Else
            RefreshBackupsList()
            If My.Settings.ShowBalloonTips Then NotifyIcon.ShowBalloonTip(2000, MCBackup.Language.Dictionary("BalloonTip.Title.BackupComplete"), MCBackup.Language.Dictionary("BalloonTip.BackupComplete"), System.Windows.Forms.ToolTipIcon.Info)
            StatusLabel.Content = MCBackup.Language.Dictionary("Status.BackupComplete")
            StatusLabel.Refresh()
            Log.Print("Backup Complete")
            ListView.IsEnabled = True
            BackupButton.IsEnabled = True
        End If
    End Sub

    Private Sub BackupBackgroundWorker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs)
        BackupStopwatch.Stop()
        ProgressBar.Value = 100
    End Sub

    Private WorldPath As String = ""

    Private Sub CreateThumb(Path As String)
        StatusLabel.Content = String.Format(MCBackup.Language.Dictionary("Status.CreatingThumb"), 0)
        Dim Thread As New Thread(AddressOf ThumbnailBackgroundWorker_DoWork)
        Thread.Start()
        WorldPath = Path
    End Sub

    Private Sub ThumbnailBackgroundWorker_DoWork()
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
            StatusLabel_Content(String.Format(MCBackup.Language.Dictionary("Status.CreatingThumb"), Int(PercentComplete)))
        ElseIf e.Data = "Job complete." Then
            Dispatcher.Invoke(Sub() ThumbnailGenerationComplete())
        End If
    End Sub

    Private Sub ThumbnailGenerationComplete()
        UpdateProgress(100)
        RefreshBackupsList()
        StatusLabel.Content = MCBackup.Language.Dictionary("Status.BackupComplete")
        StatusLabel.Refresh()
        If My.Settings.ShowBalloonTips Then NotifyIcon.ShowBalloonTip(2000, MCBackup.Language.Dictionary("BalloonTip.Title.BackupComplete"), MCBackup.Language.Dictionary("BalloonTip.BackupComplete"), System.Windows.Forms.ToolTipIcon.Info)
        Log.Print("Backup Complete")
        ListView.IsEnabled = True
        BackupButton.IsEnabled = True
    End Sub
#End Region

#Region "Restore"
    Private RestoreStopWatch As New Stopwatch

    Private Sub RestoreButton_Click(sender As Object, e As EventArgs) Handles RestoreButton.Click, ListViewRestoreItem.Click
        If MetroMessageBox.Show(MCBackup.Language.Dictionary("Message.RestoreAreYouSure"), MCBackup.Language.Dictionary("Message.Caption.AreYouSure"), MessageBoxButton.YesNo, MessageBoxImage.Question) = Forms.DialogResult.Yes Then
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
                    RestoreInfo(1) = My.Settings.SavesFolderLocation & "\" & BaseFolderName
                Case "version"
                    RestoreInfo(1) = My.Settings.MinecraftFolderLocation & "\versions\" & BaseFolderName
                Case "everything"
                    RestoreInfo(1) = My.Settings.MinecraftFolderLocation
            End Select

            DeleteForRestoreBackgroundWorker.RunWorkerAsync()
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
        If Environment.OSVersion.Version.Major > 5 Then
            TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Normal)
        End If
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
            ErrorWindow.Show(MCBackup.Language.Dictionary("Exception.Restore"), ex)
            If My.Settings.ShowBalloonTips Then NotifyIcon.ShowBalloonTip(2000, MCBackup.Language.Dictionary("BalloonTip.Title.RestoreError"), MCBackup.Language.Dictionary("BalloonTip.RestoreError"), System.Windows.Forms.ToolTipIcon.Error)
            Log.Print(ex.Message, Log.Type.Severe)
        End Try
    End Sub

    Private Sub UpdateRestoreProgress()
        If Not RestoreStopWatch.IsRunning Then
            RestoreStopWatch.Reset()
            RestoreStopWatch.Start()
        End If

        Dim PercentComplete As Integer = 0

        Dim UpdateRestoreProgressBarDelegate As New UpdateProgressBarDelegate(AddressOf ProgressBar.SetValue)

        Do Until PercentComplete = 100
            If My.Computer.FileSystem.DirectoryExists(RestoreInfo(1)) Then
                PercentComplete = GetFolderSize(RestoreInfo(1)) / GetFolderSize(My.Settings.BackupsFolderLocation & "\" & RestoreInfo(0)) * 100

                Dim Copied As Double = GetFolderSize(RestoreInfo(1))
                Dim Speed As Double = Copied / (RestoreStopWatch.ElapsedMilliseconds / 1000 * 1024)
                Dim TimeLeft As Double = (100 - PercentComplete) * RestoreStopWatch.ElapsedMilliseconds / PercentComplete / 1000

                If PercentComplete < 1 Then
                    StatusLabel.Content = String.Format(MCBackup.Language.Dictionary("Status.Restoring"), PercentComplete, Speed / 1024, "?")
                Else
                    StatusLabel.Content = String.Format(MCBackup.Language.Dictionary("Status.Restoring"), PercentComplete, Speed / 1024, TimeLeft)
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

    Private Sub RestoreBackgroundWorker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs)
        RestoreStopWatch.Stop()
        ProgressBar.Value = 100
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

    Public Function GetFolderDateCreated(FolderPath As String)
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
    Private Sub DeleteButton_Click(sender As Object, e As EventArgs) Handles DeleteButton.Click, ListViewDeleteItem.Click
        If MetroMessageBox.Show(MCBackup.Language.Dictionary("Message.DeleteAreYouSure"), MCBackup.Language.Dictionary("Message.Caption.AreYouSure"), MessageBoxButton.YesNo, MessageBoxImage.Question) = Windows.Forms.DialogResult.Yes Then
            ListViewItems.Clear()
            For Each Item In ListView.SelectedItems
                ListViewItems.Add(Item.Name)
            Next
            ListView.SelectedIndex = -1
            DeleteBackgroundWorker.RunWorkerAsync()
            StatusLabel.Content = MCBackup.Language.Dictionary("Status.Deleting")
            ProgressBar.IsIndeterminate = True
            If Environment.OSVersion.Version.Major > 5 Then
                TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Indeterminate)
            End If
        End If
    End Sub

    Private Sub DeleteBackgroundWorker_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs)
        For Each Item As String In ListViewItems
            Try
                My.Computer.FileSystem.DeleteDirectory(My.Settings.BackupsFolderLocation & "\" & Item, FileIO.DeleteDirectoryOption.DeleteAllContents)
            Catch ex As Exception
                ErrorWindow.Show(MCBackup.Language.Dictionary("Exception.Delete"), ex)
            End Try
        Next
    End Sub

    Private Sub DeleteBackgroundWorker_RunWorkerCompleted(sender As Object, e As System.ComponentModel.RunWorkerCompletedEventArgs)
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
        Dim RenameWindow As New Rename
        RenameWindow.Owner = Me
        RenameWindow.ShowDialog()
    End Sub

    Private Sub CullButton_Click(sender As Object, e As RoutedEventArgs) Handles CullButton.Click
        Dim CullWindow As New CullWindow
        CullWindow.Owner = Me
        CullWindow.Show()
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
            AutoBackupWindow.Show()
            Me.Left = Me.Left - (AutoBackupWindow.Width / 2)
            AutomaticBackupButton.Content = MCBackup.Language.Dictionary("MainWindow.AutomaticBackupButton.Content") & " <<"
        End If
    End Sub

    Private Sub Main_Activated(sender As Object, e As EventArgs) Handles MyBase.Activated
        AutoBackupWindow.Focus()
        Me.Focus()
    End Sub

    Private Sub Main_SizeChanged(sender As Object, e As SizeChangedEventArgs) Handles Main.SizeChanged
        Main_LocationChanged(sender, Nothing)
        InternalGrid.MaxWidth = Me.ActualWidth - 20
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

#Region "-- Group By --"
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
#End Region

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

    'asc/desc
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

    Private Sub Main_Closing(sender As Object, e As CancelEventArgs) Handles MyBase.Closing
        Me.Focus()
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
End Class

Public Class CloseAction
    Public Enum CloseType As Integer
        CloseToTray
        CloseCompletely
        Cancel
        ForceClose
    End Enum
End Class

