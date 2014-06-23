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

Public Class AutoBackupWindow
    Private Main As MainWindow = DirectCast(Application.Current.MainWindow, MainWindow)
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
            PrefixLabel.Content = MCBackup.Language.Dictionary("AutoBackupWindow.PrefixLabel.Content")
            SuffixLabel.Content = MCBackup.Language.Dictionary("AutoBackupWindow.SuffixLabel.Content")
            StartButton.Content = MCBackup.Language.Dictionary("AutoBackupWindow.StartButton.Content.Start")
        Catch
        End Try
    End Sub

#Region "Window Handles"
    Private Sub AutoBackupWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles MyBase.Loaded
        ReloadSaves()
        LoadLanguage()
        PrefixTextBox.Text = My.Settings.AutoBkpPrefix
        SuffixTextBox.Text = My.Settings.AutoBkpSuffix
    End Sub

    Private Sub AutoBackupWindow_Closing(sender As Object, e As ComponentModel.CancelEventArgs) Handles MyBase.Closing
        e.Cancel = True
        Me.Hide()
        Main.Left = Main.Left + (Me.Width / 2)
        Try
            Main.AutomaticBackupButton.Content = MCBackup.Language.Dictionary("MainWindow.AutomaticBackupButton.Content") & " >>"
        Catch
        End Try
        Main.Focus()
    End Sub

    Private Sub AutoBackupWindow_LocationChanged(sender As Object, e As EventArgs) Handles MyBase.LocationChanged
        If Not Main.IsMoving Then
            IsMoving = True
            Main.Left = Me.Left - (Main.Width + 5)
            Main.Top = Me.Top
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
            StartButton.Content = MCBackup.Language.Dictionary("AutoBackupWindow.StartButton.Content.Start")
            TimerStarted = False

            MinutesNumUpDown.IsEnabled = True
            SavesListView.IsEnabled = True
            RefreshButton.IsEnabled = True
            PrefixTextBox.IsEnabled = True
            SuffixTextBox.IsEnabled = True
        Else
            If Regex.IsMatch(PrefixTextBox.Text, "[\/:*?""<>|]") Or Regex.IsMatch(SuffixTextBox.Text, "[\/:*?""<>|]") Then
                MetroMessageBox.Show(MCBackup.Language.Dictionary("Message.BackupNameCannotContainIllegalCharacters"), MCBackup.Language.Dictionary("Message.Caption.Error"), MessageBoxButton.OK, MessageBoxImage.Error, TextAlignment.Center)
                Exit Sub
            End If

            Minutes = MinutesNumUpDown.Value
            Seconds = 0
            TimeLabel.Content = String.Format("{0:00}:00", MinutesNumUpDown.Value)
            Timer.Start()
            StartButton.Content = MCBackup.Language.Dictionary("AutoBackupWindow.StartButton.Content.Stop")
            TimerStarted = True

            MinutesNumUpDown.IsEnabled = False
            SavesListView.IsEnabled = False
            RefreshButton.IsEnabled = False
            PrefixTextBox.IsEnabled = False
            SuffixTextBox.IsEnabled = False
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

        If Minutes = 0 And Seconds = 0 Then
            Log.Print("Starting automated backup...")
            Main.BackupInfo(0) = PrefixTextBox.Text & BackupDialog.GetBackupTimeStamp() & SuffixTextBox.Text
            Main.BackupInfo(1) = String.Format(MCBackup.Language.Dictionary("AutoBackupWindow.BackupDescription"), SavesListView.SelectedItem.Name)
            Select Case My.Settings.Launcher
                Case "minecraft"
                    Main.BackupInfo(2) = My.Settings.SavesFolderLocation & "\" & CType(SavesListView.SelectedItem, SaveInfoListViewItem).Name
                Case "technic"
                    Main.BackupInfo(2) = My.Settings.MinecraftFolderLocation & "\modpacks\" & CType(SavesListView.SelectedItem, SaveInfoListViewItem).Modpack & "\saves\" & CType(SavesListView.SelectedItem, SaveInfoListViewItem).Name
                Case "ftb"
                    Main.BackupInfo(2) = My.Settings.MinecraftFolderLocation & "\" & CType(SavesListView.SelectedItem, SaveInfoListViewItem).Modpack & "\minecraft\saves\" & CType(SavesListView.SelectedItem, SaveInfoListViewItem).Name
                Case "atlauncher"
                    Main.BackupInfo(2) = My.Settings.MinecraftFolderLocation & "\Instances\" & CType(SavesListView.SelectedItem, SaveInfoListViewItem).Modpack & "\saves\" & CType(SavesListView.SelectedItem, SaveInfoListViewItem).Name
            End Select
            Main.BackupInfo(3) = "save"
            Main.BackupInfo(3) = "save"
            Main.BackupInfo(5) = SavesListView.SelectedItem.Launcher
            Main.BackupInfo(6) = SavesListView.SelectedItem.Modpack

            Main.StartBackup()

            If My.Settings.ShowBalloonTips Then Main.NotifyIcon.ShowBalloonTip(2000, MCBackup.Language.Dictionary("BalloonTip.Title.AutoBackup"), String.Format(MCBackup.Language.Dictionary("BalloonTip.AutoBackup"), WorldName), Forms.ToolTipIcon.Info)

            Minutes = MinutesNumUpDown.Value
            Seconds = 0
            TimeLabel.Content = String.Format("{0:00}:00", MinutesNumUpDown.Value)
        End If
    End Sub
#End Region

#Region "Functions"
    Public Sub ReloadSaves()
        SavesListView.Items.Clear()
        Select Case My.Settings.Launcher
            Case "minecraft"
                My.Computer.FileSystem.CreateDirectory(My.Settings.SavesFolderLocation)
                Dim SavesDirectory As New DirectoryInfo(My.Settings.SavesFolderLocation)
                For Each Folder As DirectoryInfo In SavesDirectory.GetDirectories
                    If My.Computer.FileSystem.FileExists(Folder.FullName & "\level.dat") Then
                        SavesListView.Items.Add(New SaveInfoListViewItem(Folder.Name, "minecraft"))
                    End If
                Next
            Case "technic"
                Dim Modpacks As New DirectoryInfo(My.Settings.MinecraftFolderLocation & "\modpacks")
                For Each Modpack As DirectoryInfo In Modpacks.GetDirectories
                    My.Computer.FileSystem.CreateDirectory(Modpack.FullName & "\saves")
                    Dim SavesDirectory As New DirectoryInfo(Modpack.FullName & "\saves")
                    For Each Folder As DirectoryInfo In SavesDirectory.GetDirectories
                        If My.Computer.FileSystem.FileExists(Folder.FullName & "\level.dat") Then
                            SavesListView.Items.Add(New SaveInfoListViewItem(Folder.Name, "technic", Modpack.Name))
                        End If
                    Next
                Next
            Case "ftb"
                Dim BaseDirectory As New DirectoryInfo(My.Settings.MinecraftFolderLocation)
                For Each Directory As DirectoryInfo In BaseDirectory.GetDirectories
                    If My.Computer.FileSystem.DirectoryExists(Directory.FullName & "\natives") And My.Computer.FileSystem.DirectoryExists(Directory.FullName & "\minecraft") Then
                        My.Computer.FileSystem.CreateDirectory(Directory.FullName & "\minecraft\saves")
                        Dim SavesDirectory As New DirectoryInfo(Directory.FullName & "\minecraft\saves")
                        For Each Folder As DirectoryInfo In SavesDirectory.GetDirectories
                            If My.Computer.FileSystem.FileExists(Folder.FullName & "\level.dat") Then
                                SavesListView.Items.Add(New SaveInfoListViewItem(Folder.Name, "ftb", Directory.Name))
                            End If
                        Next
                    End If
                Next
            Case "atlauncher"
                Dim Instances As New DirectoryInfo(My.Settings.MinecraftFolderLocation & "\Instances")
                For Each Instance As DirectoryInfo In Instances.GetDirectories
                    My.Computer.FileSystem.CreateDirectory(Instance.FullName & "\saves")
                    Dim SavesDirectory As New DirectoryInfo(Instance.FullName & "\saves")
                    For Each Folder As DirectoryInfo In SavesDirectory.GetDirectories
                        If My.Computer.FileSystem.FileExists(Folder.FullName & "\level.dat") Then
                            SavesListView.Items.Add(New SaveInfoListViewItem(Folder.Name, "atlauncher", Instance.Name))
                        End If
                    Next
                Next
        End Select

        If My.Settings.Launcher = "minecraft" Then
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
        If SavesListView.SelectedItems.Count = 1 Then StartButton.IsEnabled = True Else StartButton.IsEnabled = False
    End Sub

    Private Sub AutoBackupWindow_Activated(sender As Object, e As EventArgs) Handles MyBase.Activated
        Main.Focus()
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

    Private _Launcher As String
    Public Property Launcher As String
        Get
            Return _Launcher
        End Get
        Set(value As String)
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

    Sub New(Name As String, Launcher As String)
        Me.Name = Name
        Me.Launcher = Launcher
        Me.Modpack = Modpack
    End Sub

    Sub New(Name As String, Launcher As String, Modpack As String)
        Me.Name = Name
        Me.Launcher = Launcher
        Me.Modpack = Modpack
    End Sub
End Class