﻿Imports System.Windows.Interop

Public Class SetMinecraftFolderWindow
    Private Sub BaseFolderBrowseButton_Click(sender As Object, e As RoutedEventArgs) Handles BaseFolderBrowseButton.Click
        Dim FSD As New FolderSelectDialog
        If FSD.ShowDialog(New WindowInteropHelper(Me).Handle) = Forms.DialogResult.OK Then
            Select Case GetInstallationTypeButtons()
                Case Game.Launcher.Minecraft
                    Debug.Print(FSD.FolderName & "\launcher.jar")
                    If Not IO.File.Exists(FSD.FolderName & "\launcher.jar") Then
                        If MetroMessageBox.Show(MCBackup.Language.Dictionary("Message.ConfirmInvalidMinecraftFolder"), MCBackup.Language.Dictionary("Message.Caption.AreYouSure"), MessageBoxButton.YesNo, MessageBoxImage.Warning) = MessageBoxResult.No Then
                            Exit Sub
                        End If
                    End If
                    My.Settings.MinecraftFolderLocation = FSD.FolderName
                    My.Settings.SavesFolderLocation = FSD.FolderName & "\saves"
                Case Game.Launcher.Technic
                    My.Settings.MinecraftFolderLocation = FSD.FolderName
                Case Game.Launcher.FeedTheBeast
                    My.Settings.MinecraftFolderLocation = FSD.FolderName
                Case Game.Launcher.ATLauncher
                    My.Settings.MinecraftFolderLocation = FSD.FolderName
            End Select
        End If
        My.Settings.Launcher = GetInstallationTypeButtons()
        BaseFolderTextBox.Text = My.Settings.MinecraftFolderLocation
    End Sub

    Private Function GetInstallationTypeButtons() As Game.Launcher
        If MinecraftInstallationRadioButton.IsChecked Then
            Return Game.Launcher.Minecraft
        ElseIf TechnicInstallationRadioButton.IsChecked Then
            Return Game.Launcher.Technic
        ElseIf FTBInstallationRadioButton.IsChecked Then
            Return Game.Launcher.FeedTheBeast
        ElseIf ATLauncherInstallationRadioButton.IsChecked Then
            Return Game.Launcher.ATLauncher
        End If
        Return Nothing
    End Function

    Private Sub SaveButton_Click(sender As Object, e As RoutedEventArgs) Handles SaveButton.Click
        If BaseFolderTextBox.Text = "" Then
            If MetroMessageBox.Show(MCBackup.Language.Dictionary("Message.PleaseSelectFolder"), MCBackup.Language.Dictionary("Message.Caption.Error"), MessageBoxButton.OKCancel, MessageBoxImage.Error) = MessageBoxResult.Cancel Then
                Application.Current.Shutdown()
            End If
        End If
        Me.Close()
    End Sub

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs) Handles Window.Loaded
        LoadLanguage()
    End Sub

    Private Sub LoadLanguage()
        Me.Title = MCBackup.Language.Dictionary("SetMinecraftFolderWindow.Title")
        MinecraftInstallationRadioButton.Content = MCBackup.Language.Dictionary("OptionsWindow.FoldersTab.MinecraftInstallationRadioButton.Text")
        TechnicInstallationRadioButton.Content = MCBackup.Language.Dictionary("OptionsWindow.FoldersTab.TechnicInstallationRadioButton.Text")
        FTBInstallationRadioButton.Content = MCBackup.Language.Dictionary("OptionsWindow.FoldersTab.FtbInstallationRadioButton.Text")
        ATLauncherInstallationRadioButton.Content = MCBackup.Language.Dictionary("OptionsWindow.FoldersTab.AtLauncherInstallationRadioButton.Text")

        BaseFolderLabel.Content = MCBackup.Language.Dictionary("OptionsWindow.FoldersTab.BaseFolderLabel.Text")
        BaseFolderBrowseButton.Content = MCBackup.Language.Dictionary("OptionsWindow.FoldersTab.BrowseButton.Text")

        SaveButton.Content = MCBackup.Language.Dictionary("SetMinecraftFolderWindow.SaveButton.Text")
    End Sub
End Class
