Imports System.Windows.Interop

Public Class SetMinecraftFolderWindow
    Private Sub BaseFolderBrowseButton_Click(sender As Object, e As RoutedEventArgs) Handles BaseFolderBrowseButton.Click
        Dim FSD As New FolderSelectDialog
        If FSD.ShowDialog(New WindowInteropHelper(Me).Handle) = Forms.DialogResult.OK Then
            Select Case GetInstallationTypeButtons()
                Case "minecraft"
                    If My.Computer.FileSystem.FileExists(FSD.FileName & "\launcher.jar") Then
                        My.Settings.MinecraftFolderLocation = FSD.FileName
                        My.Settings.SavesFolderLocation = FSD.FileName & "\saves"
                        My.Settings.Launcher = "minecraft"
                    Else
                        If MetroMessageBox.Show("Minecraft is not installed in that folder! Try again?", MCBackup.Language.Dictionary("Message.Caption.Error"), MessageBoxButton.YesNo, MessageBoxImage.Error) = MessageBoxResult.Yes Then
                            BaseFolderBrowseButton_Click(sender, e)
                        End If
                    End If
                Case "technic"
                    If My.Computer.FileSystem.FileExists(FSD.FileName & "\settings.json") Then
                        My.Settings.MinecraftFolderLocation = FSD.FileName
                        My.Settings.Launcher = "technic"
                    Else
                        If MetroMessageBox.Show("Technic is not installed in that folder! Try again?", MCBackup.Language.Dictionary("Message.Caption.Error"), MessageBoxButton.YesNo, MessageBoxImage.Error) = MessageBoxResult.Yes Then
                            BaseFolderBrowseButton_Click(sender, e)
                        End If
                    End If
                Case "ftb"
                    If My.Computer.FileSystem.DirectoryExists(FSD.FileName & "\authlib") Then
                        My.Settings.MinecraftFolderLocation = FSD.FileName
                        My.Settings.Launcher = "ftb"
                    Else
                        If MetroMessageBox.Show("Feed the Beast is not installed in that folder! Try again?", MCBackup.Language.Dictionary("Message.Caption.Error"), MessageBoxButton.YesNo, MessageBoxImage.Error) = MessageBoxResult.Yes Then
                            BaseFolderBrowseButton_Click(sender, e)
                        End If
                    End If
                Case "atlauncher"
                    If My.Computer.FileSystem.DirectoryExists(FSD.FileName & "\Configs") Then
                        My.Settings.MinecraftFolderLocation = FSD.FileName
                        My.Settings.Launcher = "atlauncher"
                    Else
                        If MetroMessageBox.Show("ATLauncher is not installed in that folder! Try again?", MCBackup.Language.Dictionary("Message.Caption.Error"), MessageBoxButton.YesNo, MessageBoxImage.Error) = MessageBoxResult.Yes Then
                            BaseFolderBrowseButton_Click(sender, e)
                        End If
                    End If
            End Select
        End If
        BaseFolderTextBox.Text = My.Settings.MinecraftFolderLocation
    End Sub

    Private Function GetInstallationTypeButtons() As String
        If MinecraftInstallationRadioButton.IsChecked Then
            Return "minecraft"
        ElseIf TechnicInstallationRadioButton.IsChecked Then
            Return "technic"
        ElseIf FTBInstallationRadioButton.IsChecked Then
            Return "ftb"
        ElseIf ATLauncherInstallationRadioButton.IsChecked Then
            Return "atlauncher"
        End If
        Return Nothing
    End Function

    Private Sub SaveButton_Click(sender As Object, e As RoutedEventArgs) Handles SaveButton.Click
        If BaseFolderTextBox.Text = "" Then
            If MetroMessageBox.Show("Please select a folder before continuing.", MCBackup.Language.Dictionary("Message.Caption.Error"), MessageBoxButton.OKCancel, MessageBoxImage.Error) = MessageBoxResult.Cancel Then
                Application.Current.Shutdown()
            End If
        End If
        Me.Close()
    End Sub
End Class
