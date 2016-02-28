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

Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Text
Imports System.Globalization

Public Class BackupDialog

    Private Main As MainWindow = DirectCast(Application.Current.MainWindow, MainWindow)

    Public Sub New()
        InitializeComponent()

        LoadLanguage()
    End Sub

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs) Handles MyBase.Loaded
        CustomNameTextBox.Text = ""
        DescriptionTextBox.Text = ""
        DefaultNameRadioButton.IsChecked = True

        SavesListView.Items.Clear()
        VersionsListView.Items.Clear()

        Select Case My.Settings.Launcher
            Case Game.Launcher.Minecraft
                If Directory.Exists(My.Settings.SavesFolderLocation) Then
                    Dim SavesDirectory As New DirectoryInfo(My.Settings.SavesFolderLocation)
                    For Each Folder As DirectoryInfo In SavesDirectory.GetDirectories
                        If My.Computer.FileSystem.FileExists(Folder.FullName & "\level.dat") Then
                            SavesListView.Items.Add(New SavesListViewItem(Folder.Name, Nothing))
                        End If
                    Next
                Else
                    Log.Warn("Saves folder does not exist!")
                End If

                If Directory.Exists(My.Settings.MinecraftFolderLocation & "\versions") Then
                    Dim VersionsDirectory As New DirectoryInfo(My.Settings.MinecraftFolderLocation & "\versions")
                    For Each Version In VersionsDirectory.GetDirectories
                        VersionsListView.Items.Add(Version.Name)
                    Next
                Else
                    Log.Warn("Versions folder does not exist!")
                End If
            Case Game.Launcher.Technic
                If Directory.Exists(My.Settings.MinecraftFolderLocation & "\modpacks") Then
                    Dim Modpacks As New DirectoryInfo(My.Settings.MinecraftFolderLocation & "\modpacks")
                    For Each Modpack As DirectoryInfo In Modpacks.GetDirectories
                        If Directory.Exists(Modpack.FullName & "\saves") Then
                            Dim SavesDirectory As New DirectoryInfo(Modpack.FullName & "\saves")
                            Log.Print("Searching '{0}' for saves...", SavesDirectory.FullName)
                            For Each Folder As DirectoryInfo In SavesDirectory.GetDirectories
                                If My.Computer.FileSystem.FileExists(Folder.FullName & "\level.dat") Then
                                    SavesListView.Items.Add(New SavesListViewItem(Folder.Name, Modpack.Name))
                                End If
                            Next
                        Else
                            Log.Warn("Modpack '{0}' does not have a saves folder!", Modpack.Name)
                        End If
                        VersionsListView.Items.Add(Modpack.Name)
                    Next
                Else
                    Log.Warn("Modpacks directory does not exist!")
                End If
            Case Game.Launcher.FeedTheBeast
                If Directory.Exists(My.Settings.MinecraftFolderLocation) Then
                    Dim BaseDirectory As New DirectoryInfo(My.Settings.MinecraftFolderLocation)
                    For Each Directory As DirectoryInfo In BaseDirectory.GetDirectories
                        If My.Computer.FileSystem.DirectoryExists(Directory.FullName & "\minecraft") Then
                            If IO.Directory.Exists(Directory.FullName & "\minecraft\saves") Then
                                Dim SavesDirectory As New DirectoryInfo(Directory.FullName & "\minecraft\saves")
                                Log.Print("Searching '{0}' for saves...", SavesDirectory.FullName)
                                For Each Folder As DirectoryInfo In SavesDirectory.GetDirectories
                                    If My.Computer.FileSystem.FileExists(Folder.FullName & "\level.dat") Then
                                        SavesListView.Items.Add(New SavesListViewItem(Folder.Name, Directory.Name))
                                    End If
                                Next
                            Else
                                Log.Warn("Pack '{0}' does not have a saves folder!", Directory.Name)
                            End If
                            VersionsListView.Items.Add(Directory.Name)
                        End If
                    Next
                Else
                    Log.Warn("FeedTheBeast directory does not exist!")
                End If
            Case Game.Launcher.ATLauncher
                If Directory.Exists(My.Settings.MinecraftFolderLocation & "\Instances") Then
                    Dim Instances As New DirectoryInfo(My.Settings.MinecraftFolderLocation & "\Instances")
                    For Each Instance As DirectoryInfo In Instances.GetDirectories
                        If Directory.Exists(Instance.FullName & "\saves") Then
                            Dim SavesDirectory As New DirectoryInfo(Instance.FullName & "\saves")
                            Log.Print("Searching '{0}' for saves...", SavesDirectory.FullName)
                            For Each Folder As DirectoryInfo In SavesDirectory.GetDirectories
                                If My.Computer.FileSystem.FileExists(Folder.FullName & "\level.dat") Then
                                    SavesListView.Items.Add(New SavesListViewItem(Folder.Name, Instance.Name))
                                End If
                            Next
                        Else
                            Log.Warn("Instance '{0}' does not have a saves folder")
                        End If
                        VersionsListView.Items.Add(Instance.Name)
                    Next
                Else
                    Log.Warn("Instances directory does not exist!")
                End If
        End Select

        If My.Settings.Launcher = Game.Launcher.Minecraft Then
            SaveNameColumn.Width = 597
            SaveLocationColumn.Width = 0
        Else
            SaveNameColumn.Width = 375
            SaveLocationColumn.Width = 222
        End If

        Name_CheckChanged(sender, e)

        CustomNameTextBox.Width = 449 - CustomNameRadioButton.ActualWidth
        CustomNameOutputTextBlock.Width = 449 - CustomNameRadioButton.ActualWidth

        GroupsComboBox.Items.Add(MCBackup.Language.GetString("Groups.None"))
        For Each Group As String In My.Settings.BackupGroups
            GroupsComboBox.Items.Add(Group)
        Next
        GroupsComboBox.Items.Add(MCBackup.Language.GetString("Groups.EditGroups"))

        If GroupsComboBox.Items.Contains(My.Settings.LastBackupGroupUsed) Then
            GroupsComboBox.SelectedItem = My.Settings.LastBackupGroupUsed
        Else
            GroupsComboBox.SelectedIndex = 0
        End If
    End Sub

    Private Sub Name_CheckChanged(sender As Object, e As RoutedEventArgs) Handles DefaultNameRadioButton.Checked, CustomNameRadioButton.Checked
        If Me.IsLoaded Then
            CustomNameTextBox.IsEnabled = CustomNameRadioButton.IsChecked
        End If
    End Sub

    Private Sub StartButton_Click(sender As Object, e As EventArgs) Handles StartButton.Click
        If SavesListView.SelectedItems.Count = 0 And BackupTypeTabControl.SelectedIndex = 0 Then
            MetroMessageBox.Show(MCBackup.Language.GetString("Message.ChooseSave"), MCBackup.Language.GetString("Message.Caption.Error"), MessageBoxButton.OK, MessageBoxImage.Error)
            Exit Sub
        End If

        If VersionsListView.SelectedItems.Count = 0 And BackupTypeTabControl.SelectedIndex = 1 Then
            MetroMessageBox.Show(MCBackup.Language.GetString("Message.ChooseVersion"), MCBackup.Language.GetString("Message.Caption.Error"), MessageBoxButton.OK, MessageBoxImage.Error)
            Exit Sub
        End If

        Select Case BackupTypeTabControl.SelectedIndex
            Case 0
                Select Case My.Settings.Launcher
                    Case Game.Launcher.Minecraft
                        Main.BackupInfo.Location = My.Settings.SavesFolderLocation & "\" & CType(SavesListView.SelectedItem, SavesListViewItem).Name
                    Case Game.Launcher.Technic
                        Main.BackupInfo.Location = My.Settings.MinecraftFolderLocation & "\modpacks\" & CType(SavesListView.SelectedItem, SavesListViewItem).Location & "\saves\" & CType(SavesListView.SelectedItem, SavesListViewItem).Name
                    Case Game.Launcher.FeedTheBeast
                        Main.BackupInfo.Location = My.Settings.MinecraftFolderLocation & "\" & CType(SavesListView.SelectedItem, SavesListViewItem).Location & "\minecraft\saves\" & CType(SavesListView.SelectedItem, SavesListViewItem).Name
                    Case Game.Launcher.ATLauncher
                        Main.BackupInfo.Location = My.Settings.MinecraftFolderLocation & "\Instances\" & CType(SavesListView.SelectedItem, SavesListViewItem).Location & "\saves\" & CType(SavesListView.SelectedItem, SavesListViewItem).Name
                End Select
                Main.BackupInfo.Type = BackupManager.BackupTypes.World
            Case 1
                Select Case My.Settings.Launcher
                    Case Game.Launcher.Minecraft
                        Main.BackupInfo.Location = My.Settings.MinecraftFolderLocation & "\versions\" & VersionsListView.SelectedItem
                    Case Game.Launcher.Technic
                        Main.BackupInfo.Location = My.Settings.MinecraftFolderLocation & "\modpacks\" & VersionsListView.SelectedItem
                    Case Game.Launcher.FeedTheBeast
                        Main.BackupInfo.Location = My.Settings.MinecraftFolderLocation & "\" & VersionsListView.SelectedItem
                    Case Game.Launcher.ATLauncher
                        Main.BackupInfo.Location = My.Settings.MinecraftFolderLocation & "\Instances\" & VersionsListView.SelectedItem
                End Select
                Main.BackupInfo.Type = BackupManager.BackupTypes.Version
            Case 2
                Main.BackupInfo.Location = My.Settings.MinecraftFolderLocation
                Main.BackupInfo.Type = BackupManager.BackupTypes.Full
        End Select

        Dim OriginalFolderName As String

        Select Case BackupTypeTabControl.SelectedIndex
            Case 0
                OriginalFolderName = DirectCast(SavesListView.SelectedItem, SavesListViewItem).Name
            Case 1
                OriginalFolderName = VersionsListView.SelectedItem
            Case 2
                OriginalFolderName = My.Settings.Launcher.ToString()
            Case Else
                OriginalFolderName = "Unknown"
        End Select

        Dim Name As String

        If DefaultNameRadioButton.IsChecked Then
            Name = BackupName.Process(My.Settings.DefaultBackupName, OriginalFolderName)
        Else
            If String.IsNullOrEmpty(CustomNameTextBox.Text) Then
                MetroMessageBox.Show(MCBackup.Language.GetString("Message.EnterValidName"), MCBackup.Language.GetString("Message.Caption.Error"), MessageBoxButton.OK, MessageBoxImage.Error, TextAlignment.Center)
                Exit Sub
            Else
                Name = BackupName.Process(CustomNameTextBox.Text, OriginalFolderName)
            End If
        End If

        If Regex.IsMatch(Name, "[\/:*?""<>|]") Then
            MetroMessageBox.Show(MCBackup.Language.GetString("Message.IllegalCharacters"), MCBackup.Language.GetString("Message.Caption.Error"), MessageBoxButton.OK, MessageBoxImage.Error, TextAlignment.Center)
            Exit Sub
        End If

        Main.BackupInfo.Name = Name

        Main.BackupInfo.Description = DescriptionTextBox.Text

        Main.BackupInfo.Group = IIf(GroupsComboBox.SelectedIndex = 0, Nothing, GroupsComboBox.SelectedItem)
        Main.BackupInfo.Launcher = My.Settings.Launcher

        If My.Settings.Launcher <> Game.Launcher.Minecraft And SavesListView.SelectedItems.Count > 0 Then
            Main.BackupInfo.Modpack = CType(SavesListView.SelectedItem, SavesListViewItem).Location
        End If

        Me.Close()
        Main.StartBackup()
    End Sub

    Public Shared Function GetBackupTimeStamp()
        Return DateTime.Now.ToString("yyyy-MM-dd (hh\hmm\mss\s)")
    End Function

    Private Sub LoadLanguage()
        Me.Title = MCBackup.Language.GetString("BackupWindow.Title")
        BackupDetailsGroupBox.Header = MCBackup.Language.GetString("BackupWindow.BackupDetailsGroupBox.Header")
        BackupNameGroupBox.Header = MCBackup.Language.GetString("BackupWindow.BackupNameGroupBox.Header")
        DefaultNameRadioButton.Content = MCBackup.Language.GetString("BackupWindow.DefaultNameRadioButton.Content")
        CustomNameRadioButton.Content = MCBackup.Language.GetString("BackupWindow.CustomNameRadioButton.Content")
        CustomNameOutputTextBlock.Content = MCBackup.Language.GetString("Localization.Output")
        ShortDescriptionLabel.Content = MCBackup.Language.GetString("BackupWindow.ShortDescriptionLabel.Content")
        SavesListViewGridView.Columns(0).Header = MCBackup.Language.GetString("BackupWindow.ListBox.Columns(0).Header")
        StartButton.Content = MCBackup.Language.GetString("BackupWindow.StartButton.Content")
        CancelButton.Content = MCBackup.Language.GetString("BackupWindow.CancelButton.Content")
        GroupLabel.Content = MCBackup.Language.GetString("BackupWindow.GroupLabel.Text")
        BackupWorldTab.Header = MCBackup.Language.GetString("BackupWindow.BackupWorldTab.Header")
        SaveNameColumn.Header = MCBackup.Language.GetString("BackupWindow.SaveNameColumn.Header")
        SaveLocationColumn.Header = MCBackup.Language.GetString("BackupWindow.SaveLocationColumn.Header")
        Select Case My.Settings.Launcher
            Case Game.Launcher.Minecraft
                BackupVersionTab.Header = MCBackup.Language.GetString("BackupWindow.BackupVersionTab.Header.Minecraft")
            Case Game.Launcher.Technic
                BackupVersionTab.Header = MCBackup.Language.GetString("BackupWindow.BackupVersionTab.Header.Technic")
            Case Game.Launcher.FeedTheBeast
                BackupVersionTab.Header = MCBackup.Language.GetString("BackupWindow.BackupVersionTab.Header.FeedTheBeast")
            Case Game.Launcher.ATLauncher
                BackupVersionTab.Header = MCBackup.Language.GetString("BackupWindow.BackupVersionTab.Header.ATLauncher")
        End Select
        VersionNameColumn.Header = MCBackup.Language.GetString("BackupWindow.VersionNameColumn.Header")
        BackupEverythingTab.Header = MCBackup.Language.GetString("BackupWindow.BackupEverythingTab.Header")
    End Sub

    Private Sub CancelButton_Click(sender As Object, e As RoutedEventArgs) Handles CancelButton.Click
        Me.Close()
    End Sub

    Private Sub GroupsComboBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles GroupsComboBox.SelectionChanged
        If GroupsComboBox.SelectedIndex = GroupsComboBox.Items.Count - 1 And GroupsComboBox.Items.Count > 1 Then
            GroupsComboBox.SelectedIndex = 0
            Me.Close()
            Dim OptionsWindow As New Options
            OptionsWindow.Owner = Main
            OptionsWindow.ShowDialog(3)
        Else
            My.Settings.LastBackupGroupUsed = GroupsComboBox.SelectedItem
        End If
    End Sub

    Private Sub Window_ContentRendered(sender As Object, e As EventArgs) Handles Window.ContentRendered
        If SavesListView.Items.Count = 0 Then
            MetroMessageBox.Show(String.Format(MCBackup.Language.GetString("Message.NoSavesWarning"), My.Settings.Launcher.ToString()), MCBackup.Language.GetString("Message.Caption.Warning"), MessageBoxButton.OK, MessageBoxImage.Warning)
        End If
    End Sub

    Private Sub CustomNameTextBox_TextChanged(sender As Object, e As TextChangedEventArgs) Handles CustomNameTextBox.TextChanged
        Dim Name As String = BackupName.Process(CustomNameTextBox.Text, MCBackup.Language.GetString("Localization.DirectoryName"))

        If Regex.IsMatch(Name, "[\/:*?""<>|]") Then
            CustomNameTextBox.Background = New SolidColorBrush(Color.FromArgb(100, 255, 0, 0))
            Exit Sub
        Else
            CustomNameTextBox.Background = New SolidColorBrush(Colors.White)
        End If

        CustomNameOutputTextBlock.Content = New ViewboxEx(MCBackup.Language.GetString("Localization.Output") & Name, Stretch.Uniform, StretchDirection.DownOnly)
    End Sub
End Class

Public Class SavesListViewItem
    Private _Name As String
    Public Property Name() As String
        Get
            Return _Name
        End Get
        Set(value As String)
            _Name = value
        End Set
    End Property

    Private _Location As String
    Public Property Location() As String
        Get
            Return _Location
        End Get
        Set(value As String)
            _Location = value
        End Set
    End Property

    Public Sub New(Name As String, Location As String)
        Me.Name = Name
        Me.Location = Location
    End Sub
End Class

Public Class BackupName
    Public Shared Function Process(BackupName As String, OriginalFolderName As String) As String
        If Regex.Matches(BackupName, "%timestamp:.+?%").Count > 0 Then
            For Each Match As RegularExpressions.Match In Regex.Matches(BackupName, "%timestamp:.+?%")
                Dim Format = Match.Value.Substring(Match.Value.IndexOf(":") + 1)
                Format = Format.Remove(Format.IndexOf("%"))
                Try
                    BackupName = BackupName.Replace(Match.ToString, DateTime.Now.ToString(Format, IIf(My.Settings.IgnoreSystemLocalizationWhenFormatting, CultureInfo.InvariantCulture, CultureInfo.CurrentCulture)))
                Catch
                End Try
            Next
        End If
        BackupName = BackupName.Replace("%worldname%", OriginalFolderName)
        Return BackupName
    End Function
End Class