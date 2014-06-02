Imports System.IO

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

Public Class Backup

    Private Main As MainWindow = DirectCast(Application.Current.MainWindow, MainWindow)
    Private Backup As Backup = DirectCast(Application.Current.Windows.OfType(Of Backup).First, Backup)

    Public Sub New()
        InitializeComponent()

        LoadLanguage()
    End Sub

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        CustomNameTextBox.Text = ""
        DescriptionTextBox.Text = ""
        DateAndTimeRadioButton.IsChecked = True

        SavesListView.Items.Clear()
        VersionsListView.Items.Clear()

        Select Case My.Settings.Launcher
            Case "minecraft"
                My.Computer.FileSystem.CreateDirectory(My.Settings.SavesFolderLocation)
                Dim SavesDirectory As New DirectoryInfo(My.Settings.SavesFolderLocation)
                For Each Folder As DirectoryInfo In SavesDirectory.GetDirectories
                    If My.Computer.FileSystem.FileExists(Folder.FullName & "\level.dat") Then
                        SavesListView.Items.Add(New SavesListViewItem(Folder.Name, Nothing))
                    End If
                Next

                My.Computer.FileSystem.CreateDirectory(My.Settings.MinecraftFolderLocation & "\versions")
                Dim VersionsDirectory As New DirectoryInfo(My.Settings.MinecraftFolderLocation & "\versions")
                For Each Version In VersionsDirectory.GetDirectories
                    VersionsListView.Items.Add(Version.Name)
                Next
            Case "technic"
                Dim Modpacks As New DirectoryInfo(My.Settings.MinecraftFolderLocation & "\modpacks")
                For Each Modpack As DirectoryInfo In Modpacks.GetDirectories
                    My.Computer.FileSystem.CreateDirectory(Modpack.FullName & "\saves")
                    Dim SavesDirectory As New DirectoryInfo(Modpack.FullName & "\saves")
                    Log.Print("Searching {0} for saves...", SavesDirectory.FullName)
                    For Each Folder As DirectoryInfo In SavesDirectory.GetDirectories
                        If My.Computer.FileSystem.FileExists(Folder.FullName & "\level.dat") Then
                            SavesListView.Items.Add(New SavesListViewItem(Folder.Name, Modpack.Name))
                        End If
                    Next
                    VersionsListView.Items.Add(Modpack.Name)
                Next
            Case "ftb"
                Dim BaseDirectory As New DirectoryInfo(My.Settings.MinecraftFolderLocation)
                For Each Directory As DirectoryInfo In BaseDirectory.GetDirectories
                    If My.Computer.FileSystem.DirectoryExists(Directory.FullName & "\natives") And My.Computer.FileSystem.DirectoryExists(Directory.FullName & "\minecraft") Then
                        My.Computer.FileSystem.CreateDirectory(Directory.FullName & "\minecraft\saves")
                        Dim SavesDirectory As New DirectoryInfo(Directory.FullName & "\minecraft\saves")
                        Log.Print("Searching {0} for saves...", SavesDirectory.FullName)
                        For Each Folder As DirectoryInfo In SavesDirectory.GetDirectories
                            If My.Computer.FileSystem.FileExists(Folder.FullName & "\level.dat") Then
                                SavesListView.Items.Add(New SavesListViewItem(Folder.Name, Directory.Name))
                            End If
                        Next
                        VersionsListView.Items.Add(Directory.Name)
                    End If
                Next
            Case "atlauncher"
                Dim Instances As New DirectoryInfo(My.Settings.MinecraftFolderLocation & "\Instances")
                For Each Instance As DirectoryInfo In Instances.GetDirectories
                    My.Computer.FileSystem.CreateDirectory(Instance.FullName & "\saves")
                    Dim SavesDirectory As New DirectoryInfo(Instance.FullName & "\saves")
                    Log.Print("Searching {0} for saves...", SavesDirectory.FullName)
                    For Each Folder As DirectoryInfo In SavesDirectory.GetDirectories
                        If My.Computer.FileSystem.FileExists(Folder.FullName & "\level.dat") Then
                            SavesListView.Items.Add(New SavesListViewItem(Folder.Name, Instance.Name))
                        End If
                    Next
                    VersionsListView.Items.Add(Instance.Name)
                Next
        End Select

        Name_CheckChanged(sender, e)

        CustomNameTextBox.Width = 449 - CustomNameRadioButton.ActualWidth

        GroupsComboBox.Items.Add(MCBackup.Language.Dictionary("BackupWindow.Groups.None"))
        For Each Group As String In My.Settings.BackupGroups
            GroupsComboBox.Items.Add(Group)
        Next
        GroupsComboBox.Items.Add(MCBackup.Language.Dictionary("BackupWindow.Groups.EditGroups"))

        GroupsComboBox.SelectedIndex = 0
    End Sub

    Private Sub Name_CheckChanged(sender As Object, e As RoutedEventArgs) Handles DateAndTimeRadioButton.Checked, CustomNameRadioButton.Checked
        If Me.IsLoaded Then
            CustomNameTextBox.IsEnabled = CustomNameRadioButton.IsChecked
        End If
    End Sub

    Private Sub StartButton_Click(sender As Object, e As EventArgs) Handles StartButton.Click
        If DateAndTimeRadioButton.IsChecked Then
            Select Case BackupTypeTabControl.SelectedIndex
                Case 0
                    Main.BackupInfo(0) = CType(SavesListView.SelectedItem, SavesListViewItem).Name & " " & GetBackupTimeStamp()
                Case 1
                    If My.Settings.Launcher = "minecraft" Then
                        Main.BackupInfo(0) = "Version " & VersionsListView.SelectedItem & " " & GetBackupTimeStamp()
                    Else
                        Main.BackupInfo(0) = VersionsListView.SelectedItem & " " & GetBackupTimeStamp()
                    End If
                Case 2
                    Select Case My.Settings.Launcher
                        Case "minecraft"
                            Main.BackupInfo(0) = "Minecraft " & GetBackupTimeStamp()
                        Case "technic"
                            Main.BackupInfo(0) = "Technic " & GetBackupTimeStamp()
                        Case "ftb"
                            Main.BackupInfo(0) = "Feed the Beast " & GetBackupTimeStamp()
                        Case "atlauncher"
                            Main.BackupInfo(0) = "ATLauncher " & GetBackupTimeStamp()
                    End Select
            End Select
        ElseIf Not CustomNameTextBox.Text = "" Then
            Main.BackupInfo(0) = CustomNameTextBox.Text
        Else
            MetroMessageBox.Show(MCBackup.Language.Dictionary("Message.EnterValidName"), MCBackup.Language.Dictionary("Message.Caption.Error"), MessageBoxButton.OK, MessageBoxImage.Error)
            Exit Sub
        End If

        If SavesListView.SelectedItems.Count = 0 And BackupTypeTabControl.SelectedIndex = 0 Then
            MetroMessageBox.Show(MCBackup.Language.Dictionary("Message.ChooseSave"), MCBackup.Language.Dictionary("Message.Caption.Error"), MessageBoxButton.OK, MessageBoxImage.Error)
            Exit Sub
        End If

        If VersionsListView.SelectedItems.Count = 0 And BackupTypeTabControl.SelectedIndex = 1 Then
            MetroMessageBox.Show(MCBackup.Language.Dictionary("Message.ChooseVersion"), MCBackup.Language.Dictionary("Message.Caption.Error"), MessageBoxButton.OK, MessageBoxImage.Error)
            Exit Sub
        End If

        Main.BackupInfo(1) = DescriptionTextBox.Text

        Select Case BackupTypeTabControl.SelectedIndex
            Case 0
                Select Case My.Settings.Launcher
                    Case "minecraft"
                        Main.BackupInfo(2) = My.Settings.SavesFolderLocation & "\" & CType(SavesListView.SelectedItem, SavesListViewItem).Name
                    Case "technic"
                        Main.BackupInfo(2) = My.Settings.MinecraftFolderLocation & "\modpacks\" & CType(SavesListView.SelectedItem, SavesListViewItem).Location & "\saves\" & CType(SavesListView.SelectedItem, SavesListViewItem).Name
                    Case "ftb"
                        Main.BackupInfo(2) = My.Settings.MinecraftFolderLocation & "\" & CType(SavesListView.SelectedItem, SavesListViewItem).Location & "\minecraft\saves\" & CType(SavesListView.SelectedItem, SavesListViewItem).Name
                    Case "atlauncher"
                        Main.BackupInfo(2) = My.Settings.MinecraftFolderLocation & "\Instances\" & CType(SavesListView.SelectedItem, SavesListViewItem).Location & "\saves\" & CType(SavesListView.SelectedItem, SavesListViewItem).Name
                End Select
                Main.BackupInfo(3) = "save"
            Case 1
                Select Case My.Settings.Launcher
                    Case "minecraft"
                        Main.BackupInfo(2) = My.Settings.MinecraftFolderLocation & "\versions\" & VersionsListView.SelectedItem
                    Case "technic"
                        Main.BackupInfo(2) = My.Settings.MinecraftFolderLocation & "\modpacks\" & VersionsListView.SelectedItem
                    Case "ftb"
                        Main.BackupInfo(2) = My.Settings.MinecraftFolderLocation & "\" & VersionsListView.SelectedItem
                    Case "atlauncher"
                        Main.BackupInfo(2) = My.Settings.MinecraftFolderLocation & "\Instances\" & VersionsListView.SelectedItem
                End Select
                Main.BackupInfo(3) = "version"
            Case 2
                Main.BackupInfo(2) = My.Settings.MinecraftFolderLocation
                Main.BackupInfo(3) = "everything"
        End Select

        Main.BackupInfo(4) = IIf(GroupsComboBox.SelectedIndex = 0, Nothing, GroupsComboBox.SelectedItem)
        Main.BackupInfo(5) = My.Settings.Launcher

        If My.Settings.Launcher <> "minecraft" And SavesListView.SelectedItems.Count > 0 Then
            Main.BackupInfo(6) = CType(SavesListView.SelectedItem, SavesListViewItem).Location
        End If

        Me.Close()
        Main.StartBackup()
    End Sub

    Public Shared Function GetBackupTimeStamp()
        Return DateTime.Now.ToString("yyyy-MM-dd (hh\hmm\mss\s)")
    End Function

    Private Sub LoadLanguage()
        Me.Title = MCBackup.Language.Dictionary("BackupWindow.Title")
        BackupDetailsGroupBox.Header = MCBackup.Language.Dictionary("BackupWindow.BackupDetailsGroupBox.Header")
        BackupNameGroupBox.Header = MCBackup.Language.Dictionary("BackupWindow.BackupNameGroupBox.Header")
        DateAndTimeRadioButton.Content = MCBackup.Language.Dictionary("BackupWindow.DateAndTimeRadioButton.Content")
        CustomNameRadioButton.Content = MCBackup.Language.Dictionary("BackupWindow.CustomNameRadioButton.Content")
        ShortDescriptionLabel.Content = MCBackup.Language.Dictionary("BackupWindow.ShortDescriptionLabel.Content")
        SavesListViewGridView.Columns(0).Header = MCBackup.Language.Dictionary("BackupWindow.ListBox.Columns(0).Header")
        StartButton.Content = MCBackup.Language.Dictionary("BackupWindow.StartButton.Content")
        CancelButton.Content = MCBackup.Language.Dictionary("BackupWindow.CancelButton.Content")
        GroupLabel.Content = MCBackup.Language.Dictionary("BackupWindow.GroupLabel.Text")
        Select Case My.Settings.Launcher
            Case "minecraft"
                BackupTypeTabControl.Items(1).Header = "Version"
            Case "technic"
                BackupTypeTabControl.Items(1).Header = "Modpack"
            Case "ftb"
                BackupTypeTabControl.Items(1).Header = "Modpack"
            Case "atlauncher"
                BackupTypeTabControl.Items(1).Header = "Instance"
        End Select
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
        End If
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