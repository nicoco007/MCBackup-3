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

Imports System.ComponentModel
Imports System.IO
Imports System.Windows.Interop

Public Class SetMinecraftFolderWindow
    Private Sub BaseFolderBrowseButton_Click(sender As Object, e As RoutedEventArgs) Handles BaseFolderBrowseButton.Click
        Dim FSD As New FolderSelectDialog
        If FSD.ShowDialog(New WindowInteropHelper(Me).Handle) = Forms.DialogResult.OK Then
            Select Case GetInstallationTypeButtons()
                Case Launcher.Minecraft
                    My.Settings.MinecraftFolderLocation = FSD.FolderName
                    My.Settings.SavesFolderLocation = FSD.FolderName & "\saves"
                Case Else
                    My.Settings.MinecraftFolderLocation = FSD.FolderName
                    My.Settings.SavesFolderLocation = Nothing
            End Select

            My.Settings.Launcher = GetInstallationTypeButtons()
            BaseFolderTextBox.Text = My.Settings.MinecraftFolderLocation
            SavesFolderTextBox.Text = My.Settings.SavesFolderLocation
        End If
    End Sub

    Private Function GetInstallationTypeButtons() As Launcher
        If MinecraftInstallationRadioButton.IsChecked Then
            Return Launcher.Minecraft
        ElseIf TechnicInstallationRadioButton.IsChecked Then
            Return Launcher.Technic
        ElseIf FTBInstallationRadioButton.IsChecked Then
            Return Launcher.FeedTheBeast
        ElseIf ATLauncherInstallationRadioButton.IsChecked Then
            Return Launcher.ATLauncher
        End If
        Return Nothing
    End Function

    Private Sub LauncherTypeChanged(sender As RadioButton, e As EventArgs) Handles MinecraftInstallationRadioButton.Checked, TechnicInstallationRadioButton.Checked, FTBInstallationRadioButton.Checked, ATLauncherInstallationRadioButton.Checked

        If Me.IsLoaded Then

            If sender Is MinecraftInstallationRadioButton Then

                SavesFolderTextBox.IsEnabled = True
                SavesFolderBrowseButton.IsEnabled = True

                If Not String.IsNullOrEmpty(BaseFolderTextBox.Text) Then

                    SavesFolderTextBox.Text = BaseFolderTextBox.Text + "\saves"
                    My.Settings.SavesFolderLocation = BaseFolderTextBox.Text + "\saves"

                End If

            Else

                SavesFolderTextBox.IsEnabled = False
                SavesFolderBrowseButton.IsEnabled = False

                SavesFolderTextBox.Text = ""
                My.Settings.SavesFolderLocation = ""

            End If

        End If

    End Sub

    Private Sub SaveButton_Click(sender As Object, e As RoutedEventArgs) Handles SaveButton.Click

        Me.Close()

    End Sub

    Private Sub Window_Closing(sender As Object, e As CancelEventArgs) Handles Window.Closing

        If BaseFolderTextBox.Text = "" OrElse Not Directory.Exists(BaseFolderTextBox.Text) Then

            If MetroMessageBox.Show(MCBackup.Language.GetString("Message.PleaseSelectFolder"), MCBackup.Language.GetString("Message.Caption.Error"), MessageBoxButton.OKCancel, MessageBoxImage.Error) = MessageBoxResult.OK Then

                e.Cancel = True

            End If

        Else

            If My.Settings.Launcher = Launcher.Minecraft And String.IsNullOrEmpty(My.Settings.SavesFolderLocation) And Not String.IsNullOrEmpty(My.Settings.MinecraftFolderLocation) Then

                My.Settings.SavesFolderLocation = My.Settings.MinecraftFolderLocation + "\saves"

            End If

        End If

    End Sub

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs) Handles Window.Loaded
        LoadLanguage()
    End Sub

    Private Sub LoadLanguage()
        Me.Title = MCBackup.Language.GetString("SetMinecraftFolderWindow.Title")
        MinecraftInstallationRadioButton.Content = MCBackup.Language.GetString("OptionsWindow.FoldersTab.MinecraftInstallationRadioButton.Text")
        TechnicInstallationRadioButton.Content = MCBackup.Language.GetString("OptionsWindow.FoldersTab.TechnicInstallationRadioButton.Text")
        FTBInstallationRadioButton.Content = MCBackup.Language.GetString("OptionsWindow.FoldersTab.FtbInstallationRadioButton.Text")
        ATLauncherInstallationRadioButton.Content = MCBackup.Language.GetString("OptionsWindow.FoldersTab.AtLauncherInstallationRadioButton.Text")

        BaseFolderLabel.Content = MCBackup.Language.GetString("OptionsWindow.FoldersTab.BaseFolderLabel.Text")
        BaseFolderBrowseButton.Content = MCBackup.Language.GetString("OptionsWindow.FoldersTab.BrowseButton.Text")

        SavesFolderLabel.Content = MCBackup.Language.GetString("OptionsWindow.FoldersTab.SavesFolderLabel.Text")
        SavesFolderBrowseButton.Content = MCBackup.Language.GetString("OptionsWindow.FoldersTab.BrowseButton.Text")

        SaveButton.Content = MCBackup.Language.GetString("SetMinecraftFolderWindow.SaveButton.Text")
    End Sub
End Class
