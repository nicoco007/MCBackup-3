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

Imports System.Windows.Threading
Imports System.Text.RegularExpressions
Imports System.IO
Imports System.Text
Imports System.Globalization

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
            Me.Title = MCBackup.Language.Dictionary("AutoBackupWindow.Title")
            BackupEveryLabel.Content = MCBackup.Language.Dictionary("AutoBackupWindow.BackupEveryLabel.Content")
            MinutesLabel.Content = MCBackup.Language.Dictionary("AutoBackupWindow.MinutesLabel.Content")
            WorldToBackUpLabel.Text = MCBackup.Language.Dictionary("AutoBackupWindow.WorldToBackUpLabel.Text")
            RefreshButton.Content = MCBackup.Language.Dictionary("AutoBackupWindow.RefreshButton.Content")
            SaveAsLabel.Content = MCBackup.Language.Dictionary("AutoBackupWindow.SaveAsLabel.Content")
            'PrefixLabel.Content = MCBackup.Language.Dictionary("AutoBackupWindow.PrefixLabel.Content")
            'SuffixLabel.Content = MCBackup.Language.Dictionary("AutoBackupWindow.SuffixLabel.Content")
            StartButton.Content = MCBackup.Language.Dictionary("AutoBackupWindow.StartButton.Content.Start")
            SaveNameColumn.Header = MCBackup.Language.Dictionary("BackupWindow.SaveNameColumn.Header")
            SaveLocationColumn.Header = MCBackup.Language.Dictionary("BackupWindow.SaveLocationColumn.Header")
        Catch
        End Try
    End Sub

#Region "Window Handles"
    Private Sub AutoBackupWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles MyBase.Loaded
        ReloadSaves()
        LoadLanguage()
        'PrefixTextBox.Text = My.Settings.AutoBkpPrefix
        'SuffixTextBox.Text = My.Settings.AutoBkpSuffix
    End Sub

    Private Sub AutoBackupWindow_Closing(sender As Object, e As ComponentModel.CancelEventArgs) Handles MyBase.Closing
        e.Cancel = True
        Me.Hide()
        MainWindow.Left = MainWindow.Left + (Me.Width / 2)
        Try
            MainWindow.AutomaticBackupButton.Content = MCBackup.Language.Dictionary("MainWindow.AutomaticBackupButton.Content") & " >>"
        Catch
        End Try
        MainWindow.Focus()
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
            MainWindow.NotificationIconWindow.AutoBackupTimeLabel.Visibility = Windows.Visibility.Collapsed

            StartButton.Content = MCBackup.Language.Dictionary("AutoBackupWindow.StartButton.Content.Start")
            TimerStarted = False

            MinutesNumUpDown.IsEnabled = True
            SavesListView.IsEnabled = True
            RefreshButton.IsEnabled = True
            BackupNameTextBox.IsEnabled = True
        Else
            Dim BackupName As String = BackupNameTextBox.Text
            If Regex.Matches(BackupName, "%timestamp:.*%").Count > 0 Then
                For Each Match As RegularExpressions.Match In Regex.Matches(BackupName, "%timestamp:.*%")
                    Dim Format = Match.ToString.Split(":")(1)
                    Format = Format.Remove(Format.IndexOf("%"))
                    BackupName = BackupName.Replace(Match.ToString, DateTime.Now.ToString(Format, CultureInfo.InvariantCulture))
                Next
            End If

            If Regex.IsMatch(BackupName, "[\/:*?""<>|]") Then
                MetroMessageBox.Show(MCBackup.Language.Dictionary("Message.BackupNameCannotContainIllegalCharacters"), MCBackup.Language.Dictionary("Message.Caption.Error"), MessageBoxButton.OK, MessageBoxImage.Error, TextAlignment.Center)
                Exit Sub
            End If

            MainWindow.NotificationIconWindow.AutoBackupLabel.Content = "Time until next automatic backup:"
            MainWindow.NotificationIconWindow.AutoBackupTimeLabel.Visibility = Windows.Visibility.Visible

            Minutes = MinutesNumUpDown.Value
            Seconds = 0
            TimeLabel.Content = String.Format("{0:00}:00", MinutesNumUpDown.Value)
            If MainWindow.NotificationIconWindow.IsVisible Then MainWindow.NotificationIconWindow.AutoBackupTimeLabel.Content = String.Format("{0:00}:00", MinutesNumUpDown.Value)
            Timer.Start()
            StartButton.Content = MCBackup.Language.Dictionary("AutoBackupWindow.StartButton.Content.Stop")
            TimerStarted = True

            MinutesNumUpDown.IsEnabled = False
            SavesListView.IsEnabled = False
            RefreshButton.IsEnabled = False
            BackupNameTextBox.IsEnabled = False
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
            Dim BackupName As String = BackupNameTextBox.Text
            If Regex.Matches(BackupName, "%timestamp:.*%").Count > 0 Then
                For Each Match As RegularExpressions.Match In Regex.Matches(BackupName, "%timestamp:.*%")
                    Dim Format = Match.ToString.Split(":")(1)
                    Format = Format.Remove(Format.IndexOf("%"))
                    BackupName = BackupName.Replace(Match.ToString, DateTime.Now.ToString(Format, CultureInfo.InvariantCulture))
                Next
            End If
            BackupName = BackupName.Replace("%worldname%", SavesListView.SelectedItem.Name)
            MainWindow.BackupInfo(0) = BackupName

            MainWindow.BackupInfo(1) = String.Format(MCBackup.Language.Dictionary("AutoBackupWindow.BackupDescription"), SavesListView.SelectedItem.Name)
            Select Case My.Settings.Launcher
                Case Game.Launcher.Minecraft
                    MainWindow.BackupInfo(2) = My.Settings.SavesFolderLocation & "\" & CType(SavesListView.SelectedItem, SaveInfoListViewItem).Name
                Case Game.Launcher.Technic
                    MainWindow.BackupInfo(2) = My.Settings.MinecraftFolderLocation & "\modpacks\" & CType(SavesListView.SelectedItem, SaveInfoListViewItem).Modpack & "\saves\" & CType(SavesListView.SelectedItem, SaveInfoListViewItem).Name
                Case Game.Launcher.FeedTheBeast
                    MainWindow.BackupInfo(2) = My.Settings.MinecraftFolderLocation & "\" & CType(SavesListView.SelectedItem, SaveInfoListViewItem).Modpack & "\minecraft\saves\" & CType(SavesListView.SelectedItem, SaveInfoListViewItem).Name
                Case Game.Launcher.ATLauncher
                    MainWindow.BackupInfo(2) = My.Settings.MinecraftFolderLocation & "\Instances\" & CType(SavesListView.SelectedItem, SaveInfoListViewItem).Modpack & "\saves\" & CType(SavesListView.SelectedItem, SaveInfoListViewItem).Name
            End Select

            MainWindow.BackupInfo(3) = "save"

            MainWindow.BackupInfo(5) = SavesListView.SelectedItem.Launcher
            MainWindow.BackupInfo(6) = SavesListView.SelectedItem.Modpack

            MainWindow.BackupInfo(4) = "Auto Backups"

            MainWindow.StartBackup()

            If My.Settings.ShowBalloonTips Then MainWindow.NotifyIcon.ShowBalloonTip(2000, MCBackup.Language.Dictionary("BalloonTip.Title.AutoBackup"), String.Format(MCBackup.Language.Dictionary("BalloonTip.AutoBackup"), CType(SavesListView.SelectedItem, SaveInfoListViewItem).Name), Forms.ToolTipIcon.Info)

            If Not My.Settings.BackupGroups.Contains("Auto Backups") Then
                My.Settings.BackupGroups.Add("Auto Backups")
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
            Case Game.Launcher.Minecraft
                If Directory.Exists(My.Settings.SavesFolderLocation) Then
                    Dim SavesDirectory As New DirectoryInfo(My.Settings.SavesFolderLocation)
                    For Each Folder As DirectoryInfo In SavesDirectory.GetDirectories
                        If File.Exists(Folder.FullName & "\level.dat") Then
                            SavesListView.Items.Add(New SaveInfoListViewItem(Folder.Name, Game.Launcher.Minecraft))
                        End If
                    Next
                Else
                    Log.Print("Saves directory does not exist!", Log.Level.Warning)
                End If
            Case Game.Launcher.Technic
                If Directory.Exists(My.Settings.MinecraftFolderLocation & "\modpacks") Then
                    Dim Modpacks As New DirectoryInfo(My.Settings.MinecraftFolderLocation & "\modpacks")
                    For Each Modpack As DirectoryInfo In Modpacks.GetDirectories
                        If Directory.Exists(Modpack.FullName & "\saves") Then
                            Dim SavesDirectory As New DirectoryInfo(Modpack.FullName & "\saves")
                            For Each Folder As DirectoryInfo In SavesDirectory.GetDirectories
                                If File.Exists(Folder.FullName & "\level.dat") Then
                                    SavesListView.Items.Add(New SaveInfoListViewItem(Folder.Name, Game.Launcher.Technic, Modpack.Name))
                                End If
                            Next
                        Else
                            Log.Print("Modpack '{0}' does not contain saves.", Modpack.Name)
                        End If
                    Next
                Else
                    Log.Print("Modpacks directory does not exist!", Log.Level.Warning)
                End If
            Case Game.Launcher.FeedTheBeast
                If Directory.Exists(My.Settings.MinecraftFolderLocation) Then
                    Dim BaseDirectory As New DirectoryInfo(My.Settings.MinecraftFolderLocation)
                    For Each Directory As DirectoryInfo In BaseDirectory.GetDirectories
                        If IO.Directory.Exists(Directory.FullName & "\natives") And IO.Directory.Exists(Directory.FullName & "\minecraft") Then
                            If IO.Directory.Exists(Directory.FullName & "\minecraft\saves") Then
                                Dim SavesDirectory As New DirectoryInfo(Directory.FullName & "\minecraft\saves")
                                For Each Folder As DirectoryInfo In SavesDirectory.GetDirectories
                                    If File.Exists(Folder.FullName & "\level.dat") Then
                                        SavesListView.Items.Add(New SaveInfoListViewItem(Folder.Name, Game.Launcher.FeedTheBeast, Directory.Name))
                                    End If
                                Next
                            End If
                            Log.Print("Pack '{0}' does not contain saves.", Directory.Name)
                        End If
                    Next
                Else
                    Log.Print("FeedTheBeast directory does not exist!", Log.Level.Severe)
                End If
            Case Game.Launcher.ATLauncher
                If Directory.Exists(My.Settings.MinecraftFolderLocation & "\Instances") Then
                    Dim Instances As New DirectoryInfo(My.Settings.MinecraftFolderLocation & "\Instances")
                    For Each Instance As DirectoryInfo In Instances.GetDirectories
                        If Directory.Exists(Instance.FullName & "\saves") Then
                            Dim SavesDirectory As New DirectoryInfo(Instance.FullName & "\saves")
                            For Each Folder As DirectoryInfo In SavesDirectory.GetDirectories
                                If File.Exists(Folder.FullName & "\level.dat") Then
                                    SavesListView.Items.Add(New SaveInfoListViewItem(Folder.Name, Game.Launcher.ATLauncher, Instance.Name))
                                End If
                            Next
                        Else
                            Log.Print("Instance '{0}' does not contain saves.", Instance.Name)
                        End If
                    Next
                Else
                    Log.Print("Instances directory does not exist!", Log.Level.Warning)
                End If
        End Select

        If My.Settings.Launcher = Game.Launcher.Minecraft Then
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

    Private _Launcher As Game.Launcher
    Public Property Launcher As Game.Launcher
        Get
            Return _Launcher
        End Get
        Set(value As Game.Launcher)
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

    Sub New(Name As String, Launcher As Game.Launcher)
        Me.Name = Name
        Me.Launcher = Launcher
        Me.Modpack = Modpack
    End Sub

    Sub New(Name As String, Launcher As Game.Launcher, Modpack As String)
        Me.Name = Name
        Me.Launcher = Launcher
        Me.Modpack = Modpack
    End Sub
End Class