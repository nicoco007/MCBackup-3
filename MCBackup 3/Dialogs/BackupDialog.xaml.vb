'   ╔═══════════════════════════════════════════════════════4710════════════════════╗
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

Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Globalization

Public Class BackupDialog

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
            Case Launcher.Minecraft
                If Directory.Exists(My.Settings.SavesFolderLocation) Then
                    Dim SavesDirectory As New DirectoryInfo(My.Settings.SavesFolderLocation)
                    For Each Folder As DirectoryInfo In SavesDirectory.GetDirectories
                        If My.Computer.FileSystem.FileExists(Folder.FullName & "\level.dat") Then
                            SavesListView.Items.Add(New SavesListViewItem(Folder.FullName, Folder.Name, Nothing))
                        End If
                    Next
                Else
                    Log.Warn("Saves folder does not exist!")
                End If

                If Directory.Exists(My.Settings.MinecraftFolderLocation & "\versions") Then
                    Dim VersionsDirectory As New DirectoryInfo(My.Settings.MinecraftFolderLocation & "\versions")
                    For Each Version In VersionsDirectory.GetDirectories
                        VersionsListView.Items.Add(New BackupsDialogListViewItem(Version.FullName, Version.Name))
                    Next
                Else
                    Log.Warn("Versions folder does not exist!")
                End If
            Case Launcher.Technic
                If Directory.Exists(My.Settings.MinecraftFolderLocation & "\modpacks") Then
                    Dim Modpacks As New DirectoryInfo(My.Settings.MinecraftFolderLocation & "\modpacks")
                    For Each Modpack As DirectoryInfo In Modpacks.GetDirectories
                        If Directory.Exists(Modpack.FullName & "\saves") Then
                            Dim SavesDirectory As New DirectoryInfo(Modpack.FullName & "\saves")
                            Log.Print("Searching '{0}' for saves...", SavesDirectory.FullName)
                            For Each Folder As DirectoryInfo In SavesDirectory.GetDirectories
                                If My.Computer.FileSystem.FileExists(Folder.FullName & "\level.dat") Then
                                    SavesListView.Items.Add(New SavesListViewItem(Folder.FullName, Folder.Name, Modpack.Name))
                                End If
                            Next
                        Else
                            Log.Warn("Modpack '{0}' does not have a saves folder!", Modpack.Name)
                        End If
                        VersionsListView.Items.Add(New BackupsDialogListViewItem(Modpack.FullName, Modpack.Name))
                    Next
                Else
                    Log.Warn("Modpacks directory does not exist!")
                End If
            Case Launcher.FeedTheBeast
                If Directory.Exists(My.Settings.MinecraftFolderLocation) Then
                    Dim BaseDirectory As New DirectoryInfo(My.Settings.MinecraftFolderLocation)
                    For Each Directory As DirectoryInfo In BaseDirectory.GetDirectories
                        If My.Computer.FileSystem.DirectoryExists(Directory.FullName & "\minecraft") Then
                            If IO.Directory.Exists(Directory.FullName & "\minecraft\saves") Then
                                Dim SavesDirectory As New DirectoryInfo(Directory.FullName & "\minecraft\saves")
                                Log.Print("Searching '{0}' for saves...", SavesDirectory.FullName)
                                For Each Folder As DirectoryInfo In SavesDirectory.GetDirectories
                                    If My.Computer.FileSystem.FileExists(Folder.FullName & "\level.dat") Then
                                        SavesListView.Items.Add(New SavesListViewItem(Folder.FullName, Folder.Name, Directory.Name))
                                    End If
                                Next
                            Else
                                Log.Warn("Pack '{0}' does not have a saves folder!", Directory.Name)
                            End If
                            VersionsListView.Items.Add(New BackupsDialogListViewItem(Directory.FullName, Directory.Name))
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
                            Log.Print("Searching '{0}' for saves...", SavesDirectory.FullName)
                            For Each Folder As DirectoryInfo In SavesDirectory.GetDirectories
                                If My.Computer.FileSystem.FileExists(Folder.FullName & "\level.dat") Then
                                    SavesListView.Items.Add(New SavesListViewItem(Folder.FullName, Folder.Name, Instance.Name))
                                End If
                            Next
                        Else
                            Log.Warn("Instance '{0}' does not have a saves folder")
                        End If
                        VersionsListView.Items.Add(New BackupsDialogListViewItem(Instance.FullName, Instance.Name))
                    Next
                Else
                    Log.Warn("Instances directory does not exist!")
                End If
        End Select

        If My.Settings.Launcher = Launcher.Minecraft Then
            SaveNameColumn.Width = 597
            SaveLocationColumn.Width = 0
        Else
            SaveNameColumn.Width = 375
            SaveLocationColumn.Width = 222
        End If

        Name_CheckChanged(sender, e)

        CustomNameTextBox.Width = 449 - CustomNameRadioButton.ActualWidth
        CustomNameOutputTextBlock.Width = 449 - CustomNameRadioButton.ActualWidth

        GroupsComboBox.Items.Add(Application.Language.GetString("None"))
        For Each Group As String In My.Settings.BackupGroups
            GroupsComboBox.Items.Add(Group)
        Next
        GroupsComboBox.Items.Add(Application.Language.GetString("Edit groups..."))

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
            MetroMessageBox.Show(Application.Language.GetString("Please select a world to back up."), Application.Language.GetString("Error"), MessageBoxButton.OK, MessageBoxImage.Error)
            Exit Sub
        End If

        If VersionsListView.SelectedItems.Count = 0 And BackupTypeTabControl.SelectedIndex = 1 Then
            MetroMessageBox.Show(Application.Language.GetString("Please select a version/modpack to back up"), Application.Language.GetString("Error"), MessageBoxButton.OK, MessageBoxImage.Error)
            Exit Sub
        End If

        Dim name As String = ""
        Dim location As String = ""
        Dim type As BackupType = BackupTypeTabControl.SelectedIndex
        Dim OriginalFolderName As String = ""

        Select Case type
            Case BackupType.World
                location = DirectCast(SavesListView.SelectedItem, SavesListViewItem).FullPath
                OriginalFolderName = DirectCast(SavesListView.SelectedItem, SavesListViewItem).Name
            Case BackupType.Version
                location = DirectCast(VersionsListView.SelectedItem, BackupsDialogListViewItem).FullPath
                OriginalFolderName = DirectCast(VersionsListView.SelectedItem, BackupsDialogListViewItem).Name
            Case BackupType.Full
                location = My.Settings.MinecraftFolderLocation
                OriginalFolderName = My.Settings.Launcher.ToString()
        End Select

        If DefaultNameRadioButton.IsChecked Then
            name = BackupName.Process(My.Settings.DefaultBackupName, OriginalFolderName)
        Else
            If String.IsNullOrEmpty(CustomNameTextBox.Text) Then
                MetroMessageBox.Show(Application.Language.GetString("Please enter a valid backup name."), Application.Language.GetString("Error"), MessageBoxButton.OK, MessageBoxImage.Error, TextAlignment.Center)
                Exit Sub
            Else
                name = BackupName.Process(CustomNameTextBox.Text, OriginalFolderName)
            End If
        End If

        If Regex.IsMatch(name, "[\/:*?""<>|]") Then
            MetroMessageBox.Show(Application.Language.GetString("A backup name cannot contain the following characters:\n\ / : * ? "" < > |\nPlease check your settings and try again."), Application.Language.GetString("Error"), MessageBoxButton.OK, MessageBoxImage.Error, TextAlignment.Center)
            Exit Sub
        End If

        Dim modpack = Nothing

        If My.Settings.Launcher <> Launcher.Minecraft And SavesListView.SelectedItems.Count > 0 Then
            modpack = CType(SavesListView.SelectedItem, BackupsDialogListViewItem).Name
        End If

        Me.Close()
        BackupManager.BackupAsync(name, location, type, DescriptionTextBox.Text, IIf(GroupsComboBox.SelectedIndex = 0, Nothing, GroupsComboBox.SelectedItem), My.Settings.Launcher, modpack)
    End Sub

    Private Sub LoadLanguage()
        Me.Title = Application.Language.GetStringWithContext("noun", "Backup")
        BackupDetailsGroupBox.Header = Application.Language.GetString("Backup Details")
        BackupNameGroupBox.Header = Application.Language.GetString("Backup Name")
        DefaultNameRadioButton.Content = Application.Language.GetString("Default Name")
        CustomNameRadioButton.Content = Application.Language.GetString("Custom Name")
        CustomNameOutputTextBlock.Content = Application.Language.GetString("Output")
        ShortDescriptionLabel.Content = Application.Language.GetString("Description")
        SavesListViewGridView.Columns(0).Header = Application.Language.GetString("Name")
        StartButton.Content = Application.Language.GetString("Start")
        CancelButton.Content = Application.Language.GetString("Cancel")
        GroupLabel.Content = Application.Language.GetString("Group")
        BackupWorldTab.Header = Application.Language.GetString("World")
        SaveNameColumn.Header = Application.Language.GetString("Name")
        SaveLocationColumn.Header = Application.Language.GetString("Location")
        VersionNameColumn.Header = Application.Language.GetString("Name")
        BackupEverythingTab.Header = Application.Language.GetString("Everything")

        Select Case My.Settings.Launcher
            Case Launcher.Minecraft
                BackupVersionTab.Header = Application.Language.GetString("Version")
            Case Launcher.Technic
                BackupVersionTab.Header = Application.Language.GetString("Modpack")
            Case Launcher.FeedTheBeast
                BackupVersionTab.Header = Application.Language.GetString("Modpack")
            Case Launcher.ATLauncher
                BackupVersionTab.Header = Application.Language.GetString("Instance")
        End Select
    End Sub

    Private Sub CancelButton_Click(sender As Object, e As RoutedEventArgs) Handles CancelButton.Click
        Me.Close()
    End Sub

    Private Sub GroupsComboBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles GroupsComboBox.SelectionChanged
        If GroupsComboBox.SelectedIndex = GroupsComboBox.Items.Count - 1 And GroupsComboBox.Items.Count > 1 Then
            GroupsComboBox.SelectedIndex = 0
            Me.Close()
            Dim OptionsWindow As New OptionsDialog()
            OptionsWindow.Owner = Owner
            OptionsWindow.ShowDialog(3)
        Else
            My.Settings.LastBackupGroupUsed = GroupsComboBox.SelectedItem
        End If
    End Sub

    Private Sub Window_ContentRendered(sender As Object, e As EventArgs) Handles Window.ContentRendered
        If SavesListView.Items.Count = 0 Then
            MetroMessageBox.Show(String.Format(Application.Language.GetString("There seem to be no saves in your {0} installation. This is either because you have never started the game, or because the folder you selected as base folder is incorrect. Please check your settings if you have already ran Minecraft at least once."), My.Settings.Launcher.ToString()), Application.Language.GetString("Warning"), MessageBoxButton.OK, MessageBoxImage.Warning)
        End If
    End Sub

    Private Sub CustomNameTextBox_TextChanged(sender As Object, e As TextChangedEventArgs) Handles CustomNameTextBox.TextChanged
        Dim Name As String = BackupName.Process(CustomNameTextBox.Text, Application.Language.GetString("[Folder Name]"))

        If Regex.IsMatch(Name, "[\/:*?""<>|]") Then
            CustomNameTextBox.Background = New SolidColorBrush(Color.FromArgb(100, 255, 0, 0))
            Exit Sub
        Else
            CustomNameTextBox.Background = New SolidColorBrush(Colors.White)
        End If

        CustomNameOutputTextBlock.Content = New Viewbox() With {.Child = New ContentControl() With {.Content = Application.Language.GetString("Will back up as: ") + Name}, .Stretch = Stretch.Uniform, .StretchDirection = StretchDirection.DownOnly}
    End Sub
End Class

Public Class BackupsDialogListViewItem
    Public Property Name As String
    Public Property FullPath As String

    Public Sub New(FullPath As String, Name As String)
        Me.Name = Name
        Me.FullPath = FullPath
    End Sub
End Class

Public Class SavesListViewItem
    Inherits BackupsDialogListViewItem
    Public Property Location As String

    Public Sub New(FullPath As String, Name As String, Location As String)
        MyBase.New(FullPath, Name)
        Me.Location = Location
    End Sub
End Class

Public Class BackupName
    Public Shared Function Process(BackupName As String, OriginalFolderName As String) As String
        If Regex.Matches(BackupName, "%timestamp:.+?%").Count > 0 Then
            For Each Match As Match In Regex.Matches(BackupName, "%timestamp:.+?%")
                Dim Format = Match.Value.Substring(Match.Value.IndexOf(":") + 1)
                Format = Format.Remove(Format.IndexOf("%"))
                Try
                    BackupName = BackupName.Replace(Match.ToString, Date.Now.ToString(Format, IIf(My.Settings.IgnoreSystemLocalizationWhenFormatting, CultureInfo.InvariantCulture, CultureInfo.CurrentCulture)))
                Catch
                End Try
            Next
        End If
        BackupName = BackupName.Replace("%worldname%", OriginalFolderName)
        Return BackupName
    End Function
End Class