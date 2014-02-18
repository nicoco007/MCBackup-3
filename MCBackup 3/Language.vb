Imports System.IO

Public Class Language
    Private Shared Main As MainWindow = DirectCast(Application.Current.MainWindow, MainWindow)
    Private Shared ErrorOccured As Boolean

    Public Shared Dictionary As New Dictionary(Of String, String)

    Public Shared Sub Load(FileName As String)
        ErrorOccured = False
        Log.Print("Loading language from file '" & FileName & "'...")
        Dictionary.Clear()
        Dictionary.Add("Splash.Status.Starting", FindString("Splash.Status.Starting", FileName))
        Dictionary.Add("Splash.Status.LoadingLang", FindString("Splash.Status.LoadingLang", FileName))
        Dictionary.Add("Splash.Status.LoadingProps", FindString("Splash.Status.LoadingProps", FileName))
        Dictionary.Add("Splash.Status.CheckingUpdates", FindString("Splash.Status.CheckingUpdates", FileName))
        Dictionary.Add("Splash.Status.FindingMinecraft", FindString("Splash.Status.FindingMinecraft", FileName))
        Dictionary.Add("Splash.Status.Done", FindString("Splash.Status.Done", FileName))

        ' = Main Window =
        Dictionary.Add("MainWindow.BackupButton.Content", FindString("MainWindow.BackupButton.Content", FileName))
        Dictionary.Add("MainWindow.RestoreButton.Content", FindString("MainWindow.RestoreButton.Content", FileName))
        Dictionary.Add("MainWindow.DeleteButton.Content", FindString("MainWindow.DeleteButton.Content", FileName))
        Dictionary.Add("MainWindow.RenameButton.Content", FindString("MainWindow.RenameButton.Content", FileName))
        Dictionary.Add("MainWindow.CullButton.Content", FindString("MainWindow.CullButton.Content", FileName))

        Dictionary.Add("MainWindow.ListView.Columns(0).Header", FindString("MainWindow.ListView.Columns(0).Header", FileName))
        Dictionary.Add("MainWindow.ListView.Columns(1).Header", FindString("MainWindow.ListView.Columns(1).Header", FileName))
        Dictionary.Add("MainWindow.ListView.Columns(2).Header", FindString("MainWindow.ListView.Columns(2).Header", FileName))
        Dictionary.Add("MainWindow.OriginalNameLabel.Text", FindString("MainWindow.OriginalNameLabel.Text", FileName))
        Dictionary.Add("MainWindow.TypeLabel.Text", FindString("MainWindow.TypeLabel.Text", FileName))

        Dictionary.Add("MainWindow.MenuBar.Items(0).Header", FindString("MainWindow.MenuBar.Items(0).Header", FileName))
        Dictionary.Add("MainWindow.MenuBar.Items(0).Items(0).Header", FindString("MainWindow.MenuBar.Items(0).Items(0).Header", FileName))
        Dictionary.Add("MainWindow.MenuBar.Items(1).Header", FindString("MainWindow.MenuBar.Items(1).Header", FileName))
        Dictionary.Add("MainWindow.MenuBar.Items(1).Items(0).Header", FindString("MainWindow.MenuBar.Items(1).Items(0).Header", FileName))
        Dictionary.Add("MainWindow.MenuBar.Items(1).Items(1).Header", FindString("MainWindow.MenuBar.Items(1).Items(1).Header", FileName))
        Dictionary.Add("MainWindow.MenuBar.Items(2).Header", FindString("MainWindow.MenuBar.Items(2).Header", FileName))
        Dictionary.Add("MainWindow.MenuBar.Items(2).Items(0).Header", FindString("MainWindow.MenuBar.Items(2).Items(0).Header", FileName))
        Dictionary.Add("MainWindow.MenuBar.Items(3).Header", FindString("MainWindow.MenuBar.Items(3).Header", FileName))
        Dictionary.Add("MainWindow.MenuBar.Items(3).Items(0).Header", FindString("MainWindow.MenuBar.Items(3).Items(0).Header", FileName))
        Dictionary.Add("MainWindow.MenuBar.Items(3).Items(2).Header", FindString("MainWindow.MenuBar.Items(3).Items(2).Header", FileName))
        Dictionary.Add("MainWindow.MenuBar.Items(3).Items(3).Header", FindString("MainWindow.MenuBar.Items(3).Items(3).Header", FileName))
        Dictionary.Add("MainWindow.AutomaticBackupButton.Content", FindString("MainWindow.AutomaticBackupButton.Content", FileName))

        ' = Statuses =
        Dictionary.Add("Status.Ready", FindString("Status.Ready", FileName))
        Dictionary.Add("Status.BackingUp", FindString("Status.BackingUp", FileName))
        Dictionary.Add("Status.BackupComplete", FindString("Status.BackupComplete", FileName))
        Dictionary.Add("Status.CreatingThumb", FindString("Status.CreatingThumb", FileName))
        Dictionary.Add("Status.RemovingOldContent", FindString("Status.RemovingOldContent", FileName))
        Dictionary.Add("Status.Restoring", FindString("Status.Restoring", FileName))
        Dictionary.Add("Status.RestoreComplete", FindString("Status.RestoreComplete", FileName))
        Dictionary.Add("Status.Deleting", FindString("Status.Deleting", FileName))
        Dictionary.Add("Status.DeleteComplete", FindString("Status.DeleteComplete", FileName))

        ' = Messages =
        Dictionary.Add("Message.Caption.Information", FindString("Message.Caption.Information", FileName))
        Dictionary.Add("Message.Caption.Error", FindString("Message.Caption.Error", FileName))
        Dictionary.Add("Message.Caption.AreYouSure", FindString("Message.Caption.AreYouSure", FileName))
        Dictionary.Add("Message.Error.NoMinecraftInstall", FindString("Message.Error.NoMinecraftInstall", FileName))
        Dictionary.Add("Message.Info.MinecraftFolderSetTo", FindString("Message.Info.MinecraftFolderSetTo", FileName))
        Dictionary.Add("Message.NotInstalledInFolder", FindString("Message.NotInstalledInFolder", FileName))
        Dictionary.Add("Message.BackupError", FindString("Message.BackupError", FileName))
        Dictionary.Add("Message.RestoreAreYouSure", FindString("Message.RestoreAreYouSure", FileName))
        Dictionary.Add("Message.RestoreError", FindString("Message.RestoreError", FileName))
        Dictionary.Add("Message.DeleteAreYouSure", FindString("Message.DeleteAreYouSure", FileName))
        Dictionary.Add("Message.DeleteError", FindString("Message.DeleteError", FileName))
        Dictionary.Add("Message.EnterValidName", FindString("Message.EnterValidName", FileName))
        Dictionary.Add("Message.ChooseSave", FindString("Message.ChooseSave", FileName))
        Dictionary.Add("Message.ChooseVersion", FindString("Message.ChooseVersion", FileName))
        Dictionary.Add("Message.ResetSettings", FindString("Message.ResetSettings", FileName))

        ' = Balloon Tips =
        Dictionary.Add("BalloonTip.Title.BackupError", FindString("BalloonTip.Title.BackupError", FileName))
        Dictionary.Add("BalloonTip.BackupError", FindString("BalloonTip.BackupError", FileName))
        Dictionary.Add("BalloonTip.Title.BackupComplete", FindString("BalloonTip.Title.BackupComplete", FileName))
        Dictionary.Add("BalloonTip.BackupComplete", FindString("BalloonTip.BackupComplete", FileName))
        Dictionary.Add("BalloonTip.Title.RestoreError", FindString("BalloonTip.Title.RestoreError", FileName))
        Dictionary.Add("BalloonTip.RestoreError", FindString("BalloonTip.RestoreError", FileName))
        Dictionary.Add("BalloonTip.Title.RestoreComplete", FindString("BalloonTip.Title.RestoreComplete", FileName))
        Dictionary.Add("BalloonTip.RestoreComplete", FindString("BalloonTip.RestoreComplete", FileName))
        Dictionary.Add("BalloonTip.Title.RunningBackground", FindString("BalloonTip.Title.RunningBackground", FileName))
        Dictionary.Add("BalloonTip.RunningBackground", FindString("BalloonTip.RunningBackground", FileName))


        ' = Backup Window =
        Dictionary.Add("BackupWindow.Title", FindString("BackupWindow.Title", FileName))
        Dictionary.Add("BackupWindow.BackupDetailsGroupBox.Header", FindString("BackupWindow.BackupDetailsGroupBox.Header", FileName))
        Dictionary.Add("BackupWindow.BackupNameGroupBox.Header", FindString("BackupWindow.BackupNameGroupBox.Header", FileName))
        Dictionary.Add("BackupWindow.DateAndTimeRadioButton.Content", FindString("BackupWindow.DateAndTimeRadioButton.Content", FileName))
        Dictionary.Add("BackupWindow.CustomNameRadioButton.Content", FindString("BackupWindow.CustomNameRadioButton.Content", FileName))
        Dictionary.Add("BackupWindow.ShortDescriptionLabel.Content", FindString("BackupWindow.ShortDescriptionLabel.Content", FileName))
        Dictionary.Add("BackupWindow.Save", FindString("BackupWindow.Save", FileName))
        Dictionary.Add("BackupWindow.WholeMinecraftFolder", FindString("BackupWindow.WholeMinecraftFolder", FileName))
        Dictionary.Add("BackupWindow.Version", FindString("BackupWindow.Version", FileName))
        Dictionary.Add("BackupWindow.ListBox.Columns(0).Header", FindString("BackupWindow.ListBox.Columns(0).Header", FileName))
        Dictionary.Add("BackupWindow.StartButton.Content", FindString("BackupWindow.StartButton.Content", FileName))
        Dictionary.Add("BackupWindow.CancelButton.Content", FindString("BackupWindow.CancelButton.Content", FileName))

        Main.LoadLanguage()

        ' = Automatic Backup Window =
        Dictionary.Add("AutoBackupWindow.Title", FindString("AutoBackupWindow.Title", FileName))
        Dictionary.Add("AutoBackupWindow.BackupEveryLabel.Content", FindString("AutoBackupWindow.BackupEveryLabel.Content", FileName))
        Dictionary.Add("AutoBackupWindow.MinutesLabel.Content", FindString("AutoBackupWindow.MinutesLabel.Content", FileName))
        Dictionary.Add("AutoBackupWindow.WorldToBackUpLabel.Text", FindString("AutoBackupWindow.WorldToBackUpLabel.Text", FileName))
        Dictionary.Add("AutoBackupWindow.RefreshButton.Content", FindString("AutoBackupWindow.RefreshButton.Content", FileName))
        Dictionary.Add("AutoBackupWindow.SaveAsLabel.Content", FindString("AutoBackupWindow.SaveAsLabel.Content", FileName))
        Dictionary.Add("AutoBackupWindow.PrefixLabel.Content", FindString("AutoBackupWindow.PrefixLabel.Content", FileName))
        Dictionary.Add("AutoBackupWindow.SuffixLabel.Content", FindString("AutoBackupWindow.SuffixLabel.Content", FileName))
        Dictionary.Add("AutoBackupWindow.StartButton.Content.Start", FindString("AutoBackupWindow.StartButton.Content.Start", FileName))
        Dictionary.Add("AutoBackupWindow.StartButton.Content.Stop", FindString("AutoBackupWindow.StartButton.Content.Stop", FileName))

        Main.AutoBackupWindow.LoadLanguage()

        ' = Options Window =
        Dictionary.Add("OptionsWindow.Title", FindString("OptionsWindow.Title", FileName))
        Dictionary.Add("OptionsWindow.ListBox.Items(0).Content", FindString("OptionsWindow.GeneralPanel.ListBox.Items(0).Content", FileName))
        Dictionary.Add("OptionsWindow.ListBox.Items(1).Content", FindString("OptionsWindow.GeneralPanel.ListBox.Items(1).Content", FileName))
        Dictionary.Add("OptionsWindow.ListBox.Items(2).Content", FindString("OptionsWindow.GeneralPanel.ListBox.Items(2).Content", FileName))

        Dictionary.Add("OptionsWindow.CloseButton.Content", FindString("OptionsWindow.CloseButton.Content", FileName))
        Dictionary.Add("OptionsWindow.ResetButton.Content", FindString("OptionsWindow.ResetButton.Content", FileName))

        Dictionary.Add("OptionsWindow.AllSupportedImages", FindString("OptionsWindow.AllSupportedImages", FileName))

        Dictionary.Add("OptionsWindow.GeneralPanel.GeneralOptionsGroupBox.Header", FindString("OptionsWindow.GeneralPanel.GeneralOptionsGroupBox.Header", FileName))
        Dictionary.Add("OptionsWindow.GeneralPanel.CloseToTrayOptionsGroupBox.Header", FindString("OptionsWindow.GeneralPanel.CloseToTrayOptionsGroupBox.Header", FileName))
        Dictionary.Add("OptionsWindow.GeneralPanel.LanguageGroupBox.Header", FindString("OptionsWindow.GeneralPanel.LanguageGroupBox.Header", FileName))

        Dictionary.Add("OptionsWindow.GeneralPanel.ShowBalloonTipsCheckBox.Content", FindString("OptionsWindow.GeneralPanel.ShowBalloonTipsCheckBox.Content", FileName))
        Dictionary.Add("OptionsWindow.GeneralPanel.ShowDeleteConfirmationCheckBox.Content", FindString("OptionsWindow.GeneralPanel.ShowDeleteConfirmationCheckBox.Content", FileName))
        Dictionary.Add("OptionsWindow.GeneralPanel.CheckForUpdatesCheckBox.Content", FindString("OptionsWindow.GeneralPanel.CheckForUpdatesCheckBox.Content", FileName))
        Dictionary.Add("OptionsWindow.GeneralPanel.CreateThumbOnWorldCheckBox.Content", FindString("OptionsWindow.GeneralPanel.CreateThumbOnWorldCheckBox.Content", FileName))
        Dictionary.Add("OptionsWindow.GeneralPanel.AlwaysCloseCheckBox.Content", FindString("OptionsWindow.GeneralPanel.AlwaysCloseCheckBox.Content", FileName))
        Dictionary.Add("OptionsWindow.GeneralPanel.CloseToTrayRadioButton.Content", FindString("OptionsWindow.GeneralPanel.CloseToTrayRadioButton.Content", FileName))
        Dictionary.Add("OptionsWindow.GeneralPanel.CloseCompletelyRadioButton.Content", FindString("OptionsWindow.GeneralPanel.CloseCompletelyRadioButton.Content", FileName))
        Dictionary.Add("OptionsWindow.GeneralPanel.AlwaysCloseNoteTextBlock.Text", FindString("OptionsWindow.GeneralPanel.AlwaysCloseNoteTextBlock.Text", FileName))

        Dictionary.Add("OptionsWindow.AppearancePanel.GeneralAppearanceGroupBox.Header", FindString("OptionsWindow.AppearancePanel.GeneralAppearanceGroupBox.Header", FileName))
        Dictionary.Add("OptionsWindow.AppearancePanel.StatusTextColorGroupBox.Header", FindString("OptionsWindow.AppearancePanel.StatusTextColorGroupBox.Header", FileName))
        Dictionary.Add("OptionsWindow.AppearancePanel.ListViewTextColorIntensityGroupBox.Header", FindString("OptionsWindow.AppearancePanel.ListViewTextColorIntensityGroupBox.Header", FileName))

        Dictionary.Add("OptionsWindow.AppearancePanel.ListViewOpacityLabel.Content", FindString("OptionsWindow.AppearancePanel.ListViewOpacityLabel.Content", FileName))
        Dictionary.Add("OptionsWindow.AppearancePanel.BackgroundImageLabel.Content", FindString("OptionsWindow.AppearancePanel.BackgroundImageLabel.Content", FileName))
        Dictionary.Add("OptionsWindow.AppearancePanel.SizeModeLabel.Content", FindString("OptionsWindow.AppearancePanel.SizeModeLabel.Content", FileName))
        Dictionary.Add("OptionsWindow.AppearancePanel.SizeModeComboBox.Items(0).Content", FindString("OptionsWindow.AppearancePanel.SizeModeComboBox.Items(0).Content", FileName))
        Dictionary.Add("OptionsWindow.AppearancePanel.SizeModeComboBox.Items(1).Content", FindString("OptionsWindow.AppearancePanel.SizeModeComboBox.Items(1).Content", FileName))
        Dictionary.Add("OptionsWindow.AppearancePanel.SizeModeComboBox.Items(2).Content", FindString("OptionsWindow.AppearancePanel.SizeModeComboBox.Items(2).Content", FileName))
        Dictionary.Add("OptionsWindow.AppearancePanel.SizeModeComboBox.Items(3).Content", FindString("OptionsWindow.AppearancePanel.SizeModeComboBox.Items(3).Content", FileName))
        Dictionary.Add("OptionsWindow.AppearancePanel.BackgroundImageBrowseButton.Content", FindString("OptionsWindow.AppearancePanel.BackgroundImageBrowseButton.Content", FileName))
        Dictionary.Add("OptionsWindow.AppearancePanel.BackgroundImageRemoveButton.Content", FindString("OptionsWindow.AppearancePanel.BackgroundImageRemoveButton.Content", FileName))
        Dictionary.Add("OptionsWindow.AppearancePanel.ThemeLabel.Content", FindString("OptionsWindow.AppearancePanel.ThemeLabel.Content", FileName))

        Dictionary.Add("OptionsWindow.AppearancePanel.Themes", FindString("OptionsWindow.AppearancePanel.Themes", FileName))
        Dictionary.Add("OptionsWindow.AppearancePanel.ThemeTags", FindString("OptionsWindow.AppearancePanel.ThemeTags", FileName))

        Dictionary.Add("OptionsWindow.FoldersPanel.GeneralFoldersGroupBox.Header", FindString("OptionsWindow.FoldersPanel.GeneralFoldersGroupBox.Header", FileName))

        Dictionary.Add("OptionsWindow.FoldersPanel.MinecraftFolderLocationLabel.Content", FindString("OptionsWindow.FoldersPanel.MinecraftFolderLocationLabel.Content", FileName))
        Dictionary.Add("OptionsWindow.FoldersPanel.SavesFolderLocationLabel.Content", FindString("OptionsWindow.FoldersPanel.SavesFolderLocationLabel.Content", FileName))
        Dictionary.Add("OptionsWindow.FoldersPanel.BackupsFolderLocationLabel.Content", FindString("OptionsWindow.FoldersPanel.BackupsFolderLocationLabel.Content", FileName))
        Dictionary.Add("OptionsWindow.FoldersPanel.BrowseButton.Content", FindString("OptionsWindow.FoldersPanel.BrowseButton.Content", FileName))

        ' = Rename Window =
        Dictionary.Add("RenameWindow.Title", FindString("RenameWindow.Title", FileName))
        Dictionary.Add("RenameWindow.RenameButton.Content", FindString("RenameWindow.RenameButton.Content", FileName))
        Dictionary.Add("RenameWindow.CancelButton.Content", FindString("RenameWindow.CancelButton.Content", FileName))

        ' = About Window =
        Dictionary.Add("AboutWindow.Title", FindString("AboutWindow.Title", FileName))
        Dictionary.Add("AboutWindow.Text", FindString("AboutWindow.Text", FileName))

        ' = Close to tray window =
        Dictionary.Add("CloseToTrayWindow.Title", FindString("CloseToTrayWindow.Title", FileName))
        Dictionary.Add("CloseToTrayWindow.MessageLabel.Content", FindString("CloseToTrayWindow.MessageLabel.Content", FileName))
        Dictionary.Add("CloseToTrayWindow.YesButton.Content", FindString("CloseToTrayWindow.YesButton.Content", FileName))
        Dictionary.Add("CloseToTrayWindow.NoButton.Content", FindString("CloseToTrayWindow.NoButton.Content", FileName))
        Dictionary.Add("CloseToTrayWindow.CancelButton.Content", FindString("CloseToTrayWindow.CancelButton.Content", FileName))
        Dictionary.Add("CloseToTrayWindow.SaveCheckBox.Content", FindString("CloseToTrayWindow.SaveCheckBox.Content", FileName))
        Dictionary.Add("CloseToTrayWindow.RevertLabel.Content", FindString("CloseToTrayWindow.RevertLabel.Content", FileName))

        ' = Error form =
        Dictionary.Add("ErrorForm.ContinueButton.Content", FindString("ErrorForm.ContinueButton.Content", FileName))
        Dictionary.Add("ErrorForm.CopyToClipboardButton.Content", FindString("ErrorForm.CopyToClipboardButton.Content", FileName))

        ' = Cull Window = 
        Dictionary.Add("CullWindow.Title", FindString("CullWindow.Title", FileName))
        Dictionary.Add("CullWindow.Label1.Content", FindString("CullWindow.Label1.Content", FileName))
        Dictionary.Add("CullWindow.Label2.Content", FindString("CullWindow.Label2.Content", FileName))
        Dictionary.Add("CullWindow.CullButton.Content", FindString("CullWindow.CullButton.Content", FileName))
        Dictionary.Add("CullWindow.AreYouSureMsg", FindString("CullWindow.AreYouSureMsg", FileName))

        If ErrorOccured Then
            Log.Print("Language loaded with errors. Try resolving error(s) above.")
        Else
            Log.Print("Language loaded. No errors occured.")
        End If
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
        Using SR As New StreamReader(Directory.GetCurrentDirectory & "\language\" & FileName)
            Dim LineNumber As Integer = 0
            While SR.Peek <> -1
                LineNumber += 1
                Dim Line As String = SR.ReadLine

                If Line.StartsWith(Identifier) And Not Line.StartsWith("#") Then
                    Dim ReturnString = Line.Substring(Identifier.Length + 2)

                    If Not ReturnString.Length - 1 = ReturnString.LastIndexOf("""") Then
                        Log.Print("FORMATTING ERROR @ LINE " & LineNumber & ": Look around '" & ReturnString.Substring(ReturnString.LastIndexOf("""") + 1) & "'", Log.Type.Warning)
                        ErrorOccured = True
                        Return "[ERROR]"
                        Exit Function
                    End If

                    If ReturnString.Length - 1 = 0 Then
                        Log.Print("FORMATTING ERROR @ LINE " & LineNumber & ": Entry is empty!", Log.Type.Warning)
                        ErrorOccured = True
                        Return "[ERROR]"
                    End If

                    ReturnString = ReturnString.Remove(ReturnString.LastIndexOf(""""))
                    Return ReturnString
                End If
            End While
        End Using
        Log.Print("FORMATTING ERROR: """ & Identifier & """ indentifier not found!", Log.Type.Warning)
        Using SW As New StreamWriter(Main.StartupPath & "\language\" & FileName, True)
            SW.Write(vbNewLine & Identifier & "=""""")
        End Using
        ErrorOccured = True
        Return "[ERROR]"
    End Function
End Class
