Imports System.IO

Public Class Language
    Public Shared Main As MainWindow = DirectCast(Application.Current.MainWindow, MainWindow)

    Public Shared Dictionnary As New Dictionary(Of String, String)

    Public Shared Sub Load(FileName As String)
        Dictionnary.Clear()

        ' = Main Window =
        Dictionnary.Add("MainWindow.BackupButton.Content", FindString("MainWindow.BackupButton.Content", FileName))
        Dictionnary.Add("MainWindow.RestoreButton.Content", FindString("MainWindow.RestoreButton.Content", FileName))
        Dictionnary.Add("MainWindow.DeleteButton.Content", FindString("MainWindow.DeleteButton.Content", FileName))
        Dictionnary.Add("MainWindow.RenameButton.Content", FindString("MainWindow.RenameButton.Content", FileName))

        Dictionnary.Add("MainWindow.ListView.Columns(0).Header", FindString("MainWindow.ListView.Columns(0).Header", FileName))
        Dictionnary.Add("MainWindow.ListView.Columns(1).Header", FindString("MainWindow.ListView.Columns(1).Header", FileName))
        Dictionnary.Add("MainWindow.ListView.Columns(2).Header", FindString("MainWindow.ListView.Columns(2).Header", FileName))
        Dictionnary.Add("MainWindow.OriginalNameLabel.Text", FindString("MainWindow.OriginalNameLabel.Text", FileName))
        Dictionnary.Add("MainWindow.TypeLabel.Text", FindString("MainWindow.TypeLabel.Text", FileName))

        Dictionnary.Add("MainWindow.MenuBar.Items(0).Header", FindString("MainWindow.MenuBar.Items(0).Header", FileName))
        Dictionnary.Add("MainWindow.MenuBar.Items(0).Items(0).Header", FindString("MainWindow.MenuBar.Items(0).Items(0).Header", FileName))
        Dictionnary.Add("MainWindow.MenuBar.Items(1).Header", FindString("MainWindow.MenuBar.Items(1).Header", FileName))
        Dictionnary.Add("MainWindow.MenuBar.Items(1).Items(0).Header", FindString("MainWindow.MenuBar.Items(1).Items(0).Header", FileName))
        Dictionnary.Add("MainWindow.MenuBar.Items(1).Items(1).Header", FindString("MainWindow.MenuBar.Items(1).Items(1).Header", FileName))
        Dictionnary.Add("MainWindow.MenuBar.Items(2).Header", FindString("MainWindow.MenuBar.Items(2).Header", FileName))
        Dictionnary.Add("MainWindow.MenuBar.Items(2).Items(0).Header", FindString("MainWindow.MenuBar.Items(2).Items(0).Header", FileName))
        Dictionnary.Add("MainWindow.MenuBar.Items(3).Header", FindString("MainWindow.MenuBar.Items(3).Header", FileName))
        Dictionnary.Add("MainWindow.MenuBar.Items(3).Items(0).Header", FindString("MainWindow.MenuBar.Items(3).Items(0).Header", FileName))
        Dictionnary.Add("MainWindow.MenuBar.Items(3).Items(2).Header", FindString("MainWindow.MenuBar.Items(3).Items(2).Header", FileName))
        Dictionnary.Add("MainWindow.MenuBar.Items(3).Items(3).Header", FindString("MainWindow.MenuBar.Items(3).Items(3).Header", FileName))
        Dictionnary.Add("MainWindow.AutomaticBackupButton.Content", FindString("MainWindow.AutomaticBackupButton.Content", FileName))

        Dictionnary.Add("Status.Ready", FindString("Status.Ready", FileName))
        Dictionnary.Add("Status.BackingUp", FindString("Status.BackingUp", FileName))
        Dictionnary.Add("Status.BackupComplete", FindString("Status.BackupComplete", FileName))
        Dictionnary.Add("Status.CreatingThumb", FindString("Status.CreatingThumb", FileName))
        Dictionnary.Add("Status.RemovingOldContent", FindString("Status.RemovingOldContent", FileName))
        Dictionnary.Add("Status.Restoring", FindString("Status.Restoring", FileName))
        Dictionnary.Add("Status.RestoreComplete", FindString("Status.RestoreComplete", FileName))
        Dictionnary.Add("Status.Deleting", FindString("Status.Deleting", FileName))
        Dictionnary.Add("Status.DeleteComplete", FindString("Status.DeleteComplete", FileName))

        Dictionnary.Add("Message.Caption.Information", FindString("Message.Caption.Information", FileName))
        Dictionnary.Add("Message.Caption.Error", FindString("Message.Caption.Error", FileName))
        Dictionnary.Add("Message.Caption.AreYouSure", FindString("Message.Caption.AreYouSure", FileName))
        Dictionnary.Add("Message.Error.NoMinecraftInstall", FindString("Message.Error.NoMinecraftInstall", FileName))
        Dictionnary.Add("Message.Info.MinecraftFolderSetTo", FindString("Message.Info.MinecraftFolderSetTo", FileName))
        Dictionnary.Add("Message.NotInstalledInFolder", FindString("Message.NotInstalledInFolder", FileName))
        Dictionnary.Add("Message.BackupError", FindString("Message.BackupError", FileName))
        Dictionnary.Add("Message.RestoreAreYouSure", FindString("Message.RestoreAreYouSure", FileName))
        Dictionnary.Add("Message.RestoreError", FindString("Message.RestoreError", FileName))
        Dictionnary.Add("Message.DeleteAreYouSure", FindString("Message.DeleteAreYouSure", FileName))
        Dictionnary.Add("Message.DeleteError", FindString("Message.DeleteError", FileName))

        ' = Backup Window =
        Dictionnary.Add("BackupWindow.Title", FindString("BackupWindow.Title", FileName))
        Dictionnary.Add("BackupWindow.BackupDetailsGroupBox.Header", FindString("BackupWindow.BackupDetailsGroupBox.Header", FileName))
        Dictionnary.Add("BackupWindow.BackupNameGroupBox.Header", FindString("BackupWindow.BackupNameGroupBox.Header", FileName))
        Dictionnary.Add("BackupWindow.DateAndTimeRadioButton.Content", FindString("BackupWindow.DateAndTimeRadioButton.Content", FileName))
        Dictionnary.Add("BackupWindow.CustomNameRadioButton.Content", FindString("BackupWindow.CustomNameRadioButton.Content", FileName))
        Dictionnary.Add("BackupWindow.ShortDescriptionLabel.Content", FindString("BackupWindow.ShortDescriptionLabel.Content", FileName))
        Dictionnary.Add("BackupWindow.Save", FindString("BackupWindow.Save", FileName))
        Dictionnary.Add("BackupWindow.WholeMinecraftFolder", FindString("BackupWindow.WholeMinecraftFolder", FileName))
        Dictionnary.Add("BackupWindow.Version", FindString("BackupWindow.Version", FileName))
        Dictionnary.Add("BackupWindow.ListBox.Columns(0).Header", FindString("BackupWindow.ListBox.Columns(0).Header", FileName))
        Dictionnary.Add("BackupWindow.StartButton.Content", FindString("BackupWindow.StartButton.Content", FileName))
        Dictionnary.Add("BackupWindow.CancelButton.Content", FindString("BackupWindow.CancelButton.Content", FileName))

        Main.LoadLanguage()

        ' = Automatic Backup Window =
        Dictionnary.Add("AutoBackupWindow.Title", FindString("AutoBackupWindow.Title", FileName))
        Dictionnary.Add("AutoBackupWindow.BackupEveryLabel.Content", FindString("AutoBackupWindow.BackupEveryLabel.Content", FileName))
        Dictionnary.Add("AutoBackupWindow.MinutesLabel.Content", FindString("AutoBackupWindow.MinutesLabel.Content", FileName))
        Dictionnary.Add("AutoBackupWindow.WorldToBackUpLabel.Text", FindString("AutoBackupWindow.WorldToBackUpLabel.Text", FileName))
        Dictionnary.Add("AutoBackupWindow.RefreshButton.Content", FindString("AutoBackupWindow.RefreshButton.Content", FileName))
        Dictionnary.Add("AutoBackupWindow.SaveAsLabel.Content", FindString("AutoBackupWindow.SaveAsLabel.Content", FileName))
        Dictionnary.Add("AutoBackupWindow.PrefixLabel.Content", FindString("AutoBackupWindow.PrefixLabel.Content", FileName))
        Dictionnary.Add("AutoBackupWindow.SuffixLabel.Content", FindString("AutoBackupWindow.SuffixLabel.Content", FileName))
        Dictionnary.Add("AutoBackupWindow.StartButton.Content.Start", FindString("AutoBackupWindow.StartButton.Content.Start", FileName))
        Dictionnary.Add("AutoBackupWindow.StartButton.Content.Stop", FindString("AutoBackupWindow.StartButton.Content.Stop", FileName))

        Main.AutoBackupWindow.LoadLanguage()

        ' = Options Window =
        Dictionnary.Add("OptionsWindow.ListBox.Items(0).Content", FindString("OptionsWindow.GeneralPanel.ListBox.Items(0).Content", FileName))
        Dictionnary.Add("OptionsWindow.ListBox.Items(1).Content", FindString("OptionsWindow.GeneralPanel.ListBox.Items(1).Content", FileName))
        Dictionnary.Add("OptionsWindow.ListBox.Items(2).Content", FindString("OptionsWindow.GeneralPanel.ListBox.Items(2).Content", FileName))

        Dictionnary.Add("OptionsWindow.GeneralPanel.ShowBalloonTipsCheckBox.Content", FindString("OptionsWindow.GeneralPanel.ShowBalloonTipsCheckBox.Content", FileName))
        Dictionnary.Add("OptionsWindow.GeneralPanel.ShowDeleteConfirmationCheckBox.Content", FindString("OptionsWindow.GeneralPanel.ShowDeleteConfirmationCheckBox.Content", FileName))
        Dictionnary.Add("OptionsWindow.GeneralPanel.CheckForUpdatesCheckBox.Content", FindString("OptionsWindow.GeneralPanel.CheckForUpdatesCheckBox.Content", FileName))
        Dictionnary.Add("OptionsWindow.GeneralPanel.CreateThumbOnWorldCheckBox.Content", FindString("OptionsWindow.GeneralPanel.CreateThumbOnWorldCheckBox.Content", FileName))
        Dictionnary.Add("OptionsWindow.GeneralPanel.AlwaysCloseCheckBox.Content", FindString("OptionsWindow.GeneralPanel.AlwaysCloseCheckBox.Content", FileName))
        Dictionnary.Add("OptionsWindow.GeneralPanel.CloseToTrayRadioButton.Content", FindString("OptionsWindow.GeneralPanel.CloseToTrayRadioButton.Content", FileName))
        Dictionnary.Add("OptionsWindow.GeneralPanel.CloseCompletelyRadioButton.Content", FindString("OptionsWindow.GeneralPanel.CloseCompletelyRadioButton.Content", FileName))
        Dictionnary.Add("OptionsWindow.GeneralPanel.AlwaysCloseNoteTextBlock.Text", FindString("OptionsWindow.GeneralPanel.AlwaysCloseNoteTextBlock.Text", FileName))
        Dictionnary.Add("OptionsWindow.GeneralPanel.LanguageLabel.Content", FindString("OptionsWindow.GeneralPanel.LanguageLabel.Content", FileName))

        Dictionnary.Add("OptionsWindow.AppearancePanel.ListViewOpacityLabel.Content", FindString("OptionsWindow.AppearancePanel.ListViewOpacityLabel.Content", FileName))
        Dictionnary.Add("OptionsWindow.AppearancePanel.BackgroundImageLabel.Content", FindString("OptionsWindow.AppearancePanel.BackgroundImageLabel.Content", FileName))
        Dictionnary.Add("OptionsWindow.AppearancePanel.SizeModeLabel.Content", FindString("OptionsWindow.AppearancePanel.SizeModeLabel.Content", FileName))
        Dictionnary.Add("OptionsWindow.AppearancePanel.SizeModeComboBox.Items(0).Content", FindString("OptionsWindow.AppearancePanel.SizeModeComboBox.Items(0).Content", FileName))
        Dictionnary.Add("OptionsWindow.AppearancePanel.SizeModeComboBox.Items(1).Content", FindString("OptionsWindow.AppearancePanel.SizeModeComboBox.Items(1).Content", FileName))
        Dictionnary.Add("OptionsWindow.AppearancePanel.SizeModeComboBox.Items(2).Content", FindString("OptionsWindow.AppearancePanel.SizeModeComboBox.Items(2).Content", FileName))
        Dictionnary.Add("OptionsWindow.AppearancePanel.SizeModeComboBox.Items(3).Content", FindString("OptionsWindow.AppearancePanel.SizeModeComboBox.Items(3).Content", FileName))
        Dictionnary.Add("OptionsWindow.AppearancePanel.BackgroundImageBrowseButton.Content", FindString("OptionsWindow.AppearancePanel.BackgroundImageBrowseButton.Content", FileName))
        Dictionnary.Add("OptionsWindow.AppearancePanel.BackgroundImageRemoveButton.Content", FindString("OptionsWindow.AppearancePanel.BackgroundImageRemoveButton.Content", FileName))

        Dictionnary.Add("OptionsWindow.FoldersPanel.MinecraftFolderLocationLabel.Content", FindString("OptionsWindow.FoldersPanel.MinecraftFolderLocationLabel.Content", FileName))
        Dictionnary.Add("OptionsWindow.FoldersPanel.SavesFolderLocationLabel.Content", FindString("OptionsWindow.FoldersPanel.SavesFolderLocationLabel.Content", FileName))
        Dictionnary.Add("OptionsWindow.FoldersPanel.BackupsFolderLocationLabel.Content", FindString("OptionsWindow.FoldersPanel.BackupsFolderLocationLabel.Content", FileName))
        Dictionnary.Add("OptionsWindow.FoldersPanel.BrowseButton.Content", FindString("OptionsWindow.FoldersPanel.BrowseButton.Content", FileName))
    End Sub

    Public Shared Function GetIDFromName(Name As String)
        Dim LanguageDirectory As New IO.DirectoryInfo(Main.StartupPath & "\language")
        Dim LanguageFiles As IO.FileInfo() = LanguageDirectory.GetFiles()
        Dim LanguageFile As IO.FileInfo

        For Each LanguageFile In LanguageFiles
            Using SR As New StreamReader(Main.StartupPath & "\language\" & LanguageFile.Name)
                If FindString("fullname", LanguageFile.Name) = Name Then
                    Return LanguageFile.Name.Replace(".lang", "")
                End If
            End Using
        Next
        Return Nothing
    End Function

    Public Shared Function FindString(Identifier As String, FileName As String)
        Using SR As New StreamReader(Main.StartupPath & "\language\" & FileName)
            Dim LineNumber As Integer = 0
            While SR.Peek <> -1
                LineNumber += 1
                Dim Line As String = SR.ReadLine

                If Line.StartsWith(Identifier) And Not Line.StartsWith("#") Then
                    Dim ReturnString = Line.Substring(Identifier.Length + 2)

                    If Not ReturnString.Length - 1 = ReturnString.LastIndexOf("""") Then
                        Log.Print("FORMATTING ERROR @ LINE " & LineNumber & ": Unknown string """ & ReturnString.Substring(ReturnString.LastIndexOf("""") + 1) & """", Log.Type.Severe)
                        Return "[ERROR]"
                        Exit Function
                    End If

                    If ReturnString.Length - 1 = 0 Then
                        Log.Print("FORMATTING ERROR @ LINE " & LineNumber & ": Entry is empty!")
                        Return "[ERROR]"
                    End If

                    ReturnString = ReturnString.Remove(ReturnString.LastIndexOf(""""))
                    Return ReturnString
                End If
            End While
        End Using
        Log.Print("FORMATTING ERROR: """ & Identifier & """ indentifier not found!", Log.Type.Severe)
        Using SW As New StreamWriter(Main.StartupPath & "\language\" & FileName, True)
            SW.Write(vbNewLine & Identifier & "=""""")
        End Using
        Return "[ERROR]"
    End Function
End Class
