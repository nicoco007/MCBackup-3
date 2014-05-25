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

Imports System.IO

Public Class Language
    Private Shared Main As MainWindow = DirectCast(Application.Current.MainWindow, MainWindow)
    Private Shared ErrorOccured As Boolean

    Public Shared Dictionary As New Dictionary(Of String, String)

    Public Shared Sub Load(FileName As String)
        ErrorOccured = False
        Log.Print("Loading language from file '" & FileName & "'...")
        Dictionary.Clear()

        ' = Main Window =
        Dictionary.Add("MainWindow.BackupButton.Content", FindString("MainWindow.BackupButton.Content", FileName))
        Dictionary.Add("MainWindow.RestoreButton.Content", FindString("MainWindow.RestoreButton.Content", FileName))
        Dictionary.Add("MainWindow.DeleteButton.Content", FindString("MainWindow.DeleteButton.Content", FileName))
        Dictionary.Add("MainWindow.RenameButton.Content", FindString("MainWindow.RenameButton.Content", FileName))
        Dictionary.Add("MainWindow.CullButton.Content", FindString("MainWindow.CullButton.Content", FileName))
        Dictionary.Add("MainWindow.AutomaticBackupButton.Content", FindString("MainWindow.AutomaticBackupButton.Content", FileName))

        Dictionary.Add("MainWindow.ListView.Columns(0).Header", FindString("MainWindow.ListView.Columns(0).Header", FileName))
        Dictionary.Add("MainWindow.ListView.Columns(1).Header", FindString("MainWindow.ListView.Columns(1).Header", FileName))
        Dictionary.Add("MainWindow.ListView.Columns(2).Header", FindString("MainWindow.ListView.Columns(2).Header", FileName))
        Dictionary.Add("MainWindow.Sidebar.OriginalNameLabel.Text", FindString("MainWindow.Sidebar.OriginalNameLabel.Text", FileName))
        Dictionary.Add("MainWindow.Sidebar.TypeLabel.Text", FindString("MainWindow.Sidebar.TypeLabel.Text", FileName))
        Dictionary.Add("MainWindow.Sidebar.DescriptionLabel.Text", FindString("MainWindow.Sidebar.DescriptionLabel.Text", FileName))

        Dictionary.Add("MainWindow.Sidebar.NoBackupSelected", FindString("MainWindow.Sidebar.NoBackupSelected", FileName))
        Dictionary.Add("MainWindow.Sidebar.NumberElementsSelected", FindString("MainWindow.Sidebar.NumberElementsSelected", FileName))
        Dictionary.Add("MainWindow.Sidebar.NumberElements", FindString("MainWindow.Sidebar.NumberElements", FileName))
        Dictionary.Add("MainWindow.Sidebar.Description.NoDesc", FindString("MainWindow.Sidebar.Description.NoDesc", FileName))
        Dictionary.Add("MainWindow.Sidebar.Description.NoItem", FindString("MainWindow.Sidebar.Description.NoItem", FileName))

        Dictionary.Add("MainWindow.Search", FindString("MainWindow.Search", FileName))

        Dictionary.Add("MainWindow.ListView.ContextMenu.SortBy", FindString("MainWindow.ListView.ContextMenu.SortBy", FileName))
        Dictionary.Add("MainWindow.ListView.ContextMenu.SortBy.Name", FindString("MainWindow.ListView.ContextMenu.SortBy.Name", FileName))
        Dictionary.Add("MainWindow.ListView.ContextMenu.SortBy.DateCreated", FindString("MainWindow.ListView.ContextMenu.SortBy.DateCreated", FileName))
        Dictionary.Add("MainWindow.ListView.ContextMenu.SortBy.Type", FindString("MainWindow.ListView.ContextMenu.SortBy.Type", FileName))
        Dictionary.Add("MainWindow.ListView.ContextMenu.SortBy.Ascending", FindString("MainWindow.ListView.ContextMenu.SortBy.Ascending", FileName))
        Dictionary.Add("MainWindow.ListView.ContextMenu.SortBy.Descending", FindString("MainWindow.ListView.ContextMenu.SortBy.Descending", FileName))

        Dictionary.Add("MainWindow.ListView.ContextMenu.GroupBy", FindString("MainWindow.ListView.ContextMenu.GroupBy", FileName))
        Dictionary.Add("MainWindow.ListView.ContextMenu.GroupBy.OriginalName", FindString("MainWindow.ListView.ContextMenu.GroupBy.OriginalName", FileName))
        Dictionary.Add("MainWindow.ListView.ContextMenu.GroupBy.Type", FindString("MainWindow.ListView.ContextMenu.GroupBy.Type", FileName))
        Dictionary.Add("MainWindow.ListView.ContextMenu.GroupBy.Nothing", FindString("MainWindow.ListView.ContextMenu.GroupBy.Nothing", FileName))

        Dictionary.Add("MainWindow.Toolbar.EditButton.Text", FindString("MainWindow.Toolbar.EditButton.Text", FileName))
        Dictionary.Add("MainWindow.Toolbar.EditContextMenu.Items(0).Header", FindString("MainWindow.Toolbar.EditContextMenu.Items(0).Header", FileName))
        Dictionary.Add("MainWindow.Toolbar.EditContextMenu.Items(1).Header", FindString("MainWindow.Toolbar.EditContextMenu.Items(1).Header", FileName))
        Dictionary.Add("MainWindow.Toolbar.ToolsButton.Text", FindString("MainWindow.Toolbar.ToolsButton.Text", FileName))
        Dictionary.Add("MainWindow.Toolbar.ToolsContextMenu.Items(0).Header", FindString("MainWindow.Toolbar.ToolsContextMenu.Items(0).Header", FileName))
        Dictionary.Add("MainWindow.Toolbar.HelpButton.Text", FindString("MainWindow.Toolbar.HelpButton.Text", FileName))
        Dictionary.Add("MainWindow.Toolbar.HelpContextMenu.Items(0).Header", FindString("MainWindow.Toolbar.HelpContextMenu.Items(0).Header", FileName))
        Dictionary.Add("MainWindow.Toolbar.HelpContextMenu.Items(2).Header", FindString("MainWindow.Toolbar.HelpContextMenu.Items(2).Header", FileName))
        Dictionary.Add("MainWindow.Toolbar.HelpContextMenu.Items(3).Header", FindString("MainWindow.Toolbar.HelpContextMenu.Items(3).Header", FileName))

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
        Dictionary.Add("Status.RefreshingBackupsList", FindString("Status.RefreshingBackupsList", FileName))

        ' = Messages =
        Dictionary.Add("Message.Caption.Information", FindString("Message.Caption.Information", FileName))
        Dictionary.Add("Message.Caption.Error", FindString("Message.Caption.Error", FileName))
        Dictionary.Add("Message.Caption.AreYouSure", FindString("Message.Caption.AreYouSure", FileName))
        Dictionary.Add("Message.NoMinecraftInstallError", FindString("Message.NoMinecraftInstallError", FileName))
        Dictionary.Add("Message.Info.MinecraftFolderSetTo", FindString("Message.Info.MinecraftFolderSetTo", FileName))
        Dictionary.Add("Message.NotInstalledInFolder", FindString("Message.NotInstalledInFolder", FileName))
        Dictionary.Add("Message.RestoreAreYouSure", FindString("Message.RestoreAreYouSure", FileName))
        Dictionary.Add("Message.DeleteAreYouSure", FindString("Message.DeleteAreYouSure", FileName))
        Dictionary.Add("Message.EnterValidName", FindString("Message.EnterValidName", FileName))
        Dictionary.Add("Message.ChooseSave", FindString("Message.ChooseSave", FileName))
        Dictionary.Add("Message.ChooseVersion", FindString("Message.ChooseVersion", FileName))
        Dictionary.Add("Message.ResetSettings", FindString("Message.ResetSettings", FileName))
        Dictionary.Add("Message.BackupInProgress", FindString("Message.BackupInProgress", FileName))
        Dictionary.Add("Message.SetBackupsFolderError", FindString("Message.SetBackupsFolderError", FileName))
        Dictionary.Add("Message.SetSavesFolderWarning", FindString("Message.SetSavesFolderWarning", FileName))
        Dictionary.Add("Message.BackupNameCannotContainIllegalCharacters", FindString("Message.BackupNameCannotContainIllegalCharacters", FileName))
        Dictionary.Add("Message.CopiedToClipboard", FindString("Message.CopiedToClipboard", FileName))
        Dictionary.Add("Message.Caption.Copied", FindString("Message.Caption.Copied", FileName))
        Dictionary.Add("Message.BackupsFolderNotFound", FindString("Message.BackupsFolderNotFound", FileName))
        Dictionary.Add("Message.MinecraftNotIn", FindString("Message.MinecraftNotIn", FileName))
        Dictionary.Add("Message.FolderDoesNotExist", FindString("Message.FolderDoesNotExist", FileName))
        Dictionary.Add("Message.AreYouSureDeleteGroup", FindString("Message.AreYouSureDeleteGroup", FileName))
        Dictionary.Add("Message.BackupAlreadyExists", FindString("Message.BackupAlreadyExists", FileName))

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
        Dictionary.Add("BalloonTip.Title.AutoBackup", FindString("BalloonTip.Title.AutoBackup", FileName))
        Dictionary.Add("BalloonTip.AutoBackup", FindString("BalloonTip.AutoBackup", FileName))

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
        Dictionary.Add("BackupWindow.GroupLabel.Text", FindString("BackupWindow.GroupLabel.Text", FileName))
        Dictionary.Add("BackupWindow.Groups.None", FindString("BackupWindow.Groups.None", FileName))
        Dictionary.Add("BackupWindow.Groups.EditGroups", FindString("BackupWindow.Groups.EditGroups", FileName))

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
        Dictionary.Add("AutoBackupWindow.BackupDescription", FindString("AutoBackupWindow.BackupDescription", FileName))

        ' = Options Window =
        Dictionary.Add("OptionsWindow.Title", FindString("OptionsWindow.Title", FileName))

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
        Dictionary.Add("OptionsWindow.AppearancePanel.SampleText", FindString("OptionsWindow.AppearancePanel.SampleText", FileName))

        Dictionary.Add("OptionsWindow.AppearancePanel.Themes", FindString("OptionsWindow.AppearancePanel.Themes", FileName))
        Dictionary.Add("OptionsWindow.AppearancePanel.ThemeTags", FindString("OptionsWindow.AppearancePanel.ThemeTags", FileName))

        Dictionary.Add("OptionsWindow.FoldersPanel.GeneralFoldersGroupBox.Header", FindString("OptionsWindow.FoldersPanel.GeneralFoldersGroupBox.Header", FileName))

        Dictionary.Add("OptionsWindow.FoldersPanel.MinecraftFolderLocationLabel.Content", FindString("OptionsWindow.FoldersPanel.MinecraftFolderLocationLabel.Content", FileName))
        Dictionary.Add("OptionsWindow.FoldersPanel.SavesFolderLocationLabel.Content", FindString("OptionsWindow.FoldersPanel.SavesFolderLocationLabel.Content", FileName))
        Dictionary.Add("OptionsWindow.FoldersPanel.BackupsFolderLocationLabel.Content", FindString("OptionsWindow.FoldersPanel.BackupsFolderLocationLabel.Content", FileName))
        Dictionary.Add("OptionsWindow.FoldersPanel.BrowseButton.Content", FindString("OptionsWindow.FoldersPanel.BrowseButton.Content", FileName))

        Dictionary.Add("OptionsWindow.GroupsTab.AddNewGroupGroupBox.Header", FindString("OptionsWindow.GroupsTab.AddNewGroupGroupBox.Header", FileName))
        Dictionary.Add("OptionsWindow.GroupsTab.OtherOptionsGroupBox.Header", FindString("OptionsWindow.GroupsTab.OtherOptionsGroupBox.Header", FileName))
        Dictionary.Add("OptionsWindow.GroupsTab.DeleteGroupButton.Text", FindString("OptionsWindow.GroupsTab.DeleteGroupButton.Text", FileName))
        Dictionary.Add("OptionsWindow.GroupsTab.RenameGroupButton.Text", FindString("OptionsWindow.GroupsTab.RenameGroupButton.Text", FileName))

        Dictionary.Add("OptionsWindow.Tabs.General", FindString("OptionsWindow.Tabs.General", FileName))
        Dictionary.Add("OptionsWindow.Tabs.Appearance", FindString("OptionsWindow.Tabs.Appearance", FileName))
        Dictionary.Add("OptionsWindow.Tabs.Folders", FindString("OptionsWindow.Tabs.Folders", FileName))
        Dictionary.Add("OptionsWindow.Tabs.Groups", FindString("OptionsWindow.Tabs.Groups", FileName))

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

        ' = Error window =
        Dictionary.Add("ErrorWindow.ContinueButton.Content", FindString("ErrorWindow.ContinueButton.Content", FileName))
        Dictionary.Add("ErrorWindow.CopyToClipboardButton.Content", FindString("ErrorWindow.CopyToClipboardButton.Content", FileName))
        Dictionary.Add("ErrorWindow.ErrorAtLine", FindString("ErrorWindow.ErrorAtLine", FileName))
        Dictionary.Add("ErrorWindow.ContactMessage", FindString("ErrorWindow.ContactMessage", FileName))

        ' = Cull Window = 
        Dictionary.Add("CullWindow.Title", FindString("CullWindow.Title", FileName))
        Dictionary.Add("CullWindow.Label1.Content", FindString("CullWindow.Label1.Content", FileName))
        Dictionary.Add("CullWindow.Label2.Content", FindString("CullWindow.Label2.Content", FileName))
        Dictionary.Add("CullWindow.CullButton.Content", FindString("CullWindow.CullButton.Content", FileName))
        Dictionary.Add("CullWindow.AreYouSureMsg", FindString("CullWindow.AreYouSureMsg", FileName))

        ' = Backup Types =
        Dictionary.Add("BackupTypes.Save", FindString("BackupTypes.Save", FileName))
        Dictionary.Add("BackupTypes.Version", FindString("BackupTypes.Version", FileName))
        Dictionary.Add("BackupTypes.Everything", FindString("BackupTypes.Everything", FileName))

        ' = Metro Message Box =
        Dictionary.Add("MetroMsgBox.Button.OK", FindString("MetroMsgBox.Button.OK", FileName))
        Dictionary.Add("MetroMsgBox.Button.Yes", FindString("MetroMsgBox.Button.Yes", FileName))
        Dictionary.Add("MetroMsgBox.Button.No", FindString("MetroMsgBox.Button.No", FileName))
        Dictionary.Add("MetroMsgBox.Button.Cancel", FindString("MetroMsgBox.Button.Cancel", FileName))

        ' = Errors (exceptions) =
        Dictionary.Add("Exception.Backup", FindString("Exception.Backup", FileName))
        Dictionary.Add("Exception.Restore", FindString("Exception.Restore", FileName))
        Dictionary.Add("Exception.Delete", FindString("Exception.Delete", FileName))
        Dictionary.Add("Exception.Rename", FindString("Exception.Rename", FileName))

        ' = Time Left Strings =
        Dictionary.Add("TimeLeft.LessThanFive", FindString("TimeLeft.LessThanFive", FileName))
        Dictionary.Add("TimeLeft.Seconds", FindString("TimeLeft.Seconds", FileName))
        Dictionary.Add("TimeLeft.MinutesAndSeconds", FindString("TimeLeft.MinutesAndSeconds", FileName))

        ' = Update Dialog =
        Dictionary.Add("UpdateDialog.Title", FindString("UpdateDialog.Title", FileName))
        Dictionary.Add("UpdateDialog.Label1.Text", FindString("UpdateDialog.Label1.Text", FileName))
        Dictionary.Add("UpdateDialog.CurrentVersionLabel.Text", FindString("UpdateDialog.CurrentVersionLabel.Text", FileName))
        Dictionary.Add("UpdateDialog.LatestVersionLabel.Text", FindString("UpdateDialog.LatestVersionLabel.Text", FileName))
        Dictionary.Add("UpdateDialog.Label2.Text", FindString("UpdateDialog.Label2.Text", FileName))

        If ErrorOccured Then
            Log.Print("Language loaded with errors. Please try solving the error(s) above.", Log.Prefix.Warning)
        Else
            Log.Print("Language loaded. No errors occured.")
        End If
    End Sub

    Public Shared Function FindString(Identifier As String, FileName As String)
        Using SR As New StreamReader(Directory.GetCurrentDirectory & "\language\" & FileName)
            Dim LineNumber As Integer = 0
            While SR.Peek <> -1
                LineNumber += 1
                Dim Line As String = SR.ReadLine

                If Line.StartsWith(Identifier & "=") And Not Line.StartsWith("#") Then
                    Dim ReturnString = Line.Substring(Identifier.Length + 1)

                    If String.IsNullOrEmpty(ReturnString) Then
                        Log.Print("[Language] Error at line " & LineNumber & ": Entry is empty!", Log.Prefix.Warning)
                        ErrorOccured = True
                        Return Identifier.Split(".")(Identifier.Split(".").Count - 2) & "." & Identifier.Split(".").Last
                    End If

                    Return ReturnString.Replace("\n", vbNewLine)
                End If
            End While
        End Using
        Log.Print("[Language] Error: '" & Identifier & "' identifier not found, added automatically.", Log.Prefix.Warning)
        Using SW As New StreamWriter(Directory.GetCurrentDirectory() & "\language\" & FileName, True)
            SW.Write(vbNewLine & Identifier & "=")
        End Using
        ErrorOccured = True
        Return Identifier.Split(".")(Identifier.Split(".").Count - 2) & "." & Identifier.Split(".").Last
    End Function
End Class
