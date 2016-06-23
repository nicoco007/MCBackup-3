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

Imports System.Windows.Threading
Imports System.Text.RegularExpressions
Imports System.IO

Public Class AutoBackupWindow
    Private MainWindow As MainWindow = DirectCast(Application.Current.MainWindow, MainWindow)
    Public IsMoving As Boolean

    Private Timer As New DispatcherTimer

    Private WorldName As String = ""

    Sub New()
        InitializeComponent()

        Timer.Interval = TimeSpan.FromSeconds(1)

        AddHandler Timer.Tick, New EventHandler(AddressOf Timer_Tick)
    End Sub

    Public Sub LoadLanguage()
        Try
            Me.Title = MCBackup.Language.GetString("AutoBackupWindow.Title")
            BackupEveryLabel.Content = MCBackup.Language.GetString("AutoBackupWindow.BackupEveryLabel.Content")
            MinutesLabel.Content = MCBackup.Language.GetString("AutoBackupWindow.MinutesLabel.Content")
            WorldToBackUpLabel.Text = MCBackup.Language.GetString("AutoBackupWindow.WorldToBackUpLabel.Text")
            RefreshButton.Content = MCBackup.Language.GetString("AutoBackupWindow.RefreshButton.Content")
            StartButton.Content = MCBackup.Language.GetString("AutoBackupWindow.StartButton.Content.Start")
            SaveNameColumn.Header = MCBackup.Language.GetString("BackupWindow.SaveNameColumn.Header")
            SaveLocationColumn.Header = MCBackup.Language.GetString("BackupWindow.SaveLocationColumn.Header")
        Catch
        End Try
    End Sub

#Region "Window Handles"
    Private Sub AutoBackupWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles MyBase.Loaded
        ReloadSaves()
        LoadLanguage()
    End Sub

    Private Sub AutoBackupWindow_Closing(sender As Object, e As ComponentModel.CancelEventArgs) Handles MyBase.Closing
        e.Cancel = True
        Me.Hide()
        MainWindow.Left = MainWindow.Left + (Me.Width / 2)
        Try
            MainWindow.AutomaticBackupButton.Content = MCBackup.Language.GetString("MainWindow.AutomaticBackupButton.Content") & " >>"
        Catch
        End Try
        MainWindow.Focus()
        MainWindow.AdjustBackground()
    End Sub

    Private Sub AutoBackupWindow_LocationChanged(sender As Object, e As EventArgs) Handles MyBase.LocationChanged
        If Not MainWindow.IsMoving Then
            IsMoving = True
            MainWindow.Left = Me.Left - (MainWindow.Width + 5)
            MainWindow.Top = Me.Top
            IsMoving = False
        End If
    End Sub
#End Region

#Region "Timer"
    Private TimerStarted As Boolean

    Private Sub StartButton_Click(sender As Object, e As RoutedEventArgs) Handles StartButton.Click
        If TimerStarted Then
            Timer.Stop()
            TimeLabel.Content = "00:00"
            MainWindow.NotificationIconWindow.AutoBackupLabel.Content = "Automatic Backup is not running."
            MainWindow.NotificationIconWindow.AutoBackupTimeLabel.Visibility = Visibility.Collapsed

            StartButton.Content = MCBackup.Language.GetString("AutoBackupWindow.StartButton.Content.Start")
            TimerStarted = False

            MinutesNumUpDown.IsEnabled = True
            SavesListView.IsEnabled = True
            RefreshButton.IsEnabled = True
        Else
            MainWindow.NotificationIconWindow.AutoBackupLabel.Content = "Time until next automatic backup:"
            MainWindow.NotificationIconWindow.AutoBackupTimeLabel.Visibility = Visibility.Visible

            Minutes = MinutesNumUpDown.Value
            Seconds = 0
            TimeLabel.Content = String.Format("{0:00}:00", MinutesNumUpDown.Value)
            If MainWindow.NotificationIconWindow.IsVisible Then MainWindow.NotificationIconWindow.AutoBackupTimeLabel.Content = String.Format("{0:00}:00", MinutesNumUpDown.Value)
            Timer.Start()
            StartButton.Content = MCBackup.Language.GetString("AutoBackupWindow.StartButton.Content.Stop")
            TimerStarted = True

            MinutesNumUpDown.IsEnabled = False
            SavesListView.IsEnabled = False
            RefreshButton.IsEnabled = False
        End If
    End Sub

    Private Minutes, Seconds As Integer

    Private Sub Timer_Tick(sender As Object, e As EventArgs)
        If Seconds > 0 Then
            Seconds -= 1
        Else
            Seconds = 59
            Minutes -= 1
        End If

        TimeLabel.Content = String.Format("{0:00}:{1:00}", Minutes, Seconds)
        If MainWindow.NotificationIconWindow.IsVisible Then MainWindow.NotificationIconWindow.AutoBackupTimeLabel.Content = String.Format("{0:00}:{1:00}", Minutes, Seconds)

        If Minutes = 0 And Seconds = 0 Then
            Log.Print("Starting automated backup...")

            Dim Name As String = BackupName.Process(My.Settings.DefaultAutoBackupName, DirectCast(SavesListView.SelectedItem, SaveInfoListViewItem).Name)

            If Regex.IsMatch(Name, "[\/:*?""<>|]") Then
                MetroMessageBox.Show(MCBackup.Language.GetString("Message.IllegalCharacters"), MCBackup.Language.GetString("Message.Caption.Error"), MessageBoxButton.OK, MessageBoxImage.Error, TextAlignment.Center)
                Timer.Stop()
                Exit Sub
            End If

            Dim location As String = ""

            Select Case My.Settings.Launcher
                Case Launcher.Minecraft
                    location = My.Settings.SavesFolderLocation & "\" & CType(SavesListView.SelectedItem, SaveInfoListViewItem).Name
                Case Launcher.Technic
                    location = My.Settings.MinecraftFolderLocation & "\modpacks\" & CType(SavesListView.SelectedItem, SaveInfoListViewItem).Modpack & "\saves\" & CType(SavesListView.SelectedItem, SaveInfoListViewItem).Name
                Case Launcher.FeedTheBeast
                    location = My.Settings.MinecraftFolderLocation & "\" & CType(SavesListView.SelectedItem, SaveInfoListViewItem).Modpack & "\minecraft\saves\" & CType(SavesListView.SelectedItem, SaveInfoListViewItem).Name
                Case Launcher.ATLauncher
                    location = My.Settings.MinecraftFolderLocation & "\Instances\" & CType(SavesListView.SelectedItem, SaveInfoListViewItem).Modpack & "\saves\" & CType(SavesListView.SelectedItem, SaveInfoListViewItem).Name
            End Select

            MainWindow.StartBackup(
                Name,
                location,
                BackupType.World,
                String.Format(MCBackup.Language.GetString("AutoBackupWindow.BackupDescription"), SavesListView.SelectedItem.Name),
                MCBackup.Language.GetString("Groups.AutoBackups"),
                SavesListView.SelectedItem.Launcher,
                SavesListView.SelectedItem.Modpack
            )

            If My.Settings.ShowBalloonTips Then MainWindow.NotifyIcon.ShowBalloonTip(2000, MCBackup.Language.GetString("BalloonTip.Title.AutoBackup"), String.Format(MCBackup.Language.GetString("BalloonTip.AutoBackup"), CType(SavesListView.SelectedItem, SaveInfoListViewItem).Name), Forms.ToolTipIcon.Info)

            If Not My.Settings.BackupGroups.Contains(MCBackup.Language.GetString("Groups.AutoBackups")) Then
                My.Settings.BackupGroups.Add(MCBackup.Language.GetString("Groups.AutoBackups"))
            End If

            Minutes = MinutesNumUpDown.Value
            Seconds = 0
            TimeLabel.Content = String.Format("{0:00}:00", MinutesNumUpDown.Value)
            If MainWindow.NotificationIconWindow.IsVisible Then MainWindow.NotificationIconWindow.AutoBackupTimeLabel.Content = String.Format("{0:00}:00", MinutesNumUpDown.Value)
        End If
    End Sub
#End Region

#Region "Functions"
    Public Sub ReloadSaves()
        SavesListView.Items.Clear()
        Select Case My.Settings.Launcher
            Case Launcher.Minecraft
                If Directory.Exists(My.Settings.SavesFolderLocation) Then
                    Dim SavesDirectory As New DirectoryInfo(My.Settings.SavesFolderLocation)
                    For Each Folder As DirectoryInfo In SavesDirectory.GetDirectories
                        If File.Exists(Folder.FullName & "\level.dat") Then
                            SavesListView.Items.Add(New SaveInfoListViewItem(Folder.Name, Launcher.Minecraft))
                        End If
                    Next
                Else
                    Log.Warn("Saves directory does not exist!")
                End If
            Case Launcher.Technic
                If Directory.Exists(My.Settings.MinecraftFolderLocation & "\modpacks") Then
                    Dim Modpacks As New DirectoryInfo(My.Settings.MinecraftFolderLocation & "\modpacks")
                    For Each Modpack As DirectoryInfo In Modpacks.GetDirectories
                        If Directory.Exists(Modpack.FullName & "\saves") Then
                            Dim SavesDirectory As New DirectoryInfo(Modpack.FullName & "\saves")
                            For Each Folder As DirectoryInfo In SavesDirectory.GetDirectories
                                If File.Exists(Folder.FullName & "\level.dat") Then
                                    SavesListView.Items.Add(New SaveInfoListViewItem(Folder.Name, Launcher.Technic, Modpack.Name))
                                End If
                            Next
                        Else
                            Log.Print("Modpack '{0}' does not contain saves.", Modpack.Name)
                        End If
                    Next
                Else
                    Log.Warn("Modpacks directory does not exist!")
                End If
            Case Launcher.FeedTheBeast
                If Directory.Exists(My.Settings.MinecraftFolderLocation) Then
                    Dim BaseDirectory As New DirectoryInfo(My.Settings.MinecraftFolderLocation)
                    For Each Directory As DirectoryInfo In BaseDirectory.GetDirectories
                        If IO.Directory.Exists(Directory.FullName & "\natives") And IO.Directory.Exists(Directory.FullName & "\minecraft") Then
                            If IO.Directory.Exists(Directory.FullName & "\minecraft\saves") Then
                                Dim SavesDirectory As New DirectoryInfo(Directory.FullName & "\minecraft\saves")
                                For Each Folder As DirectoryInfo In SavesDirectory.GetDirectories
                                    If File.Exists(Folder.FullName & "\level.dat") Then
                                        SavesListView.Items.Add(New SaveInfoListViewItem(Folder.Name, Launcher.FeedTheBeast, Directory.Name))
                                    End If
                                Next
                            End If
                            Log.Print("Pack '{0}' does not contain saves.", Directory.Name)
                        End If
                    Next
                Else
                    Log.Warn("FeedTheBeast directory does not exist!")
                End If
            Case Launcher.ATLauncher
                If Directory.Exists(My.Settings.MinecraftFolderLocation & "\Instances") Then
                    Dim Instances As New DirectoryInfo(My.Settings.MinecraftFolderLocation & "\Instances")
                    For Each Instance As DirectoryInfo In Instances.GetDirectories
                        If Directory.Exists(Instance.FullName & "\saves") Then
                            Dim SavesDirectory As New DirectoryInfo(Instance.FullName & "\saves")
                            For Each Folder As DirectoryInfo In SavesDirectory.GetDirectories
                                If File.Exists(Folder.FullName & "\level.dat") Then
                                    SavesListView.Items.Add(New SaveInfoListViewItem(Folder.Name, Launcher.ATLauncher, Instance.Name))
                                End If
                            Next
                        Else
                            Log.Print("Instance '{0}' does not contain saves.", Instance.Name)
                        End If
                    Next
                Else
                    Log.Warn("Instances directory does not exist!")
                End If
        End Select

        If My.Settings.Launcher = Launcher.Minecraft Then
            SaveNameColumn.Width = 310
            SaveLocationColumn.Width = 0
        Else
            SaveNameColumn.Width = 180
            SaveLocationColumn.Width = 130
        End If
    End Sub
#End Region

    Private Sub RefreshButton_Click(sender As Object, e As RoutedEventArgs) Handles RefreshButton.Click
        ReloadSaves()
    End Sub

    Private Sub SaveListBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles SavesListView.SelectionChanged
        StartButton.IsEnabled = (SavesListView.SelectedItems.Count = 1)
    End Sub

    Private Sub AutoBackupWindow_Activated(sender As Object, e As EventArgs) Handles MyBase.Activated
        MainWindow.Focus()
    End Sub

    Private Sub SaveListBox_PreviewMouseDown(sender As Object, e As MouseButtonEventArgs) Handles SavesListView.PreviewMouseDown
        SavesListView.SelectedIndex = -1
    End Sub
End Class

Public Class SaveInfoListViewItem
    Private _Name As String
    Public Property Name As String
        Get
            Return _Name
        End Get
        Set(value As String)
            _Name = value
        End Set
    End Property

    Private _Launcher As Launcher
    Public Property Launcher As Launcher
        Get
            Return _Launcher
        End Get
        Set(value As Launcher)
            _Launcher = value
        End Set
    End Property

    Private _Modpack As String
    Public Property Modpack As String
        Get
            Return _Modpack
        End Get
        Set(value As String)
            _Modpack = value
        End Set
    End Property

    Sub New(Name As String, Launcher As Launcher)
        Me.Name = Name
        Me.Launcher = Launcher
        Me.Modpack = Modpack
    End Sub

    Sub New(Name As String, Launcher As Launcher, Modpack As String)
        Me.Name = Name
        Me.Launcher = Launcher
        Me.Modpack = Modpack
    End Sub
End Class