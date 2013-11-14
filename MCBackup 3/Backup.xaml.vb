'   ╔═══════════════════════════════════════════════════════════════════════════╗
'   ║                         Copyright 2013 nicoco007                          ║
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

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        CustomNameTextBox.Text = ""
        DescriptionTextBox.Text = ""
        DateAndTimeRadioButton.IsChecked = True
        SaveRadioButton.IsChecked = True

        ' Look at the RefreshBackupsList() sub, it's very similar

        SavesListView.Items.Clear()
        Dim SavesDirectory As New IO.DirectoryInfo(My.Settings.SavesFolderLocation)
        Dim SavesFolders As IO.DirectoryInfo() = SavesDirectory.GetDirectories()
        Dim SavesFolder As IO.DirectoryInfo

        For Each SavesFolder In SavesFolders
            SavesListView.Items.Add(SavesFolder.ToString)
        Next

        VersionsListView.Items.Clear()
        Dim VersionsDirectory As New IO.DirectoryInfo(My.Settings.MinecraftFolderLocation & "\versions")
        Dim VersionsFolders As IO.DirectoryInfo() = VersionsDirectory.GetDirectories()
        Dim VersionsFolder As IO.DirectoryInfo

        For Each VersionsFolder In VersionsFolders
            VersionsListView.Items.Add(VersionsFolder.ToString)
        Next

        Name_CheckChanged(sender, e)
        BackupType_CheckChanged(sender, e)
    End Sub

    Private Sub Name_CheckChanged(sender As Object, e As RoutedEventArgs) Handles DateAndTimeRadioButton.Checked, CustomNameRadioButton.Checked
        If Me.IsLoaded Then
            CustomNameTextBox.IsEnabled = CustomNameRadioButton.IsChecked
        End If
    End Sub

    Private Sub BackupType_CheckChanged(sender As Object, e As RoutedEventArgs) Handles SaveRadioButton.Checked, EverythingRadioButton.Checked, VersionRadioButton.Checked
        If Me.IsLoaded Then ' This is called before the form loads, so check if form is loaded to avoid errors
            SavesListView.IsEnabled = SaveRadioButton.IsChecked
            VersionsListView.IsEnabled = VersionRadioButton.IsChecked
        End If
    End Sub

    Private Sub StartButton_Click(sender As Object, e As EventArgs) Handles StartButton.Click
        If SavesListView.SelectedItems.Count = 0 And SaveRadioButton.IsChecked Then
            MessageBox.Show("Please choose a save to back up.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK)
            Exit Sub
        End If

        If VersionsListView.SelectedItems.Count = 0 And VersionRadioButton.IsChecked Then
            MessageBox.Show("Please choose a version to back up.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK)
            Exit Sub
        End If

        If DateAndTimeRadioButton.IsChecked Then
            Main.BackupInfo(0) = GetTimeAndDate()
        ElseIf Not CustomNameTextBox.Text = "" Then
            Main.BackupInfo(0) = CustomNameTextBox.Text
        Else
            MessageBox.Show("Please enter a valid name.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK)
            Exit Sub
        End If

        Main.BackupInfo(1) = DescriptionTextBox.Text

        If SaveRadioButton.IsChecked Then
            Main.BackupInfo(2) = My.Settings.SavesFolderLocation & "\" & SavesListView.SelectedItem
            Main.BackupInfo(3) = "save"
        ElseIf VersionRadioButton.IsChecked Then
            Main.BackupInfo(2) = My.Settings.MinecraftFolderLocation & "\versions\" & VersionsListView.SelectedItem
            Main.BackupInfo(3) = "version"
        Else
            Main.BackupInfo(2) = My.Settings.MinecraftFolderLocation
            Main.BackupInfo(3) = "everything"
        End If

        Me.Close()
        Main.StartBackup()
    End Sub

    Private Function GetTimeAndDate()
        Dim Day As String = Format(Now(), "dd")
        Dim Month As String = Format(Now(), "MM")
        Dim Year As String = Format(Now(), "yyyy")
        Dim Hours As String = Format(Now(), "hh")
        Dim Minutes As String = Format(Now(), "mm")
        Dim Seconds As String = Format(Now(), "ss")

        Return Year & "-" & Month & "-" & Day & " (" & Hours & "h" & Minutes & "m" & Seconds & "s)"
    End Function
End Class
