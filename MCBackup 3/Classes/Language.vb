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

Public Class Language
    Private Shared Main As MainWindow = DirectCast(Application.Current.MainWindow, MainWindow)
    Private Shared ErrorOccured As Boolean

    Private Shared _IsLoaded As Boolean = False
    Public Shared Property IsLoaded() As Boolean
        Get
            Return _IsLoaded
        End Get
        Set(value As Boolean)
            _IsLoaded = value
        End Set
    End Property

    Public Shared Dictionary As New Dictionary(Of String, String)
    Private Shared LanguageFile As String

    Public Shared Sub Load(FileName As String)
        ErrorOccured = False
        Log.Print("Loading language from file '" & FileName & "'...")
        Dictionary.Clear()

        LanguageFile = FileName

        ' = Main Window =
        Dictionary.Add("MainWindow.Title.Backup", FindString("MainWindow.Title.Backup"))
        Dictionary.Add("MainWindow.Title.Restore", FindString("MainWindow.Title.Restore"))
        Dictionary.Add("MainWindow.Title.RemovingOldContent", FindString("MainWindow.Title.RemovingOldContent"))
        Dictionary.Add("MainWindow.Title.Delete", FindString("MainWindow.Title.Delete"))
        Dictionary.Add("MainWindow.Title.RefreshingBackupsList", FindString("MainWindow.Title.RefreshingBackupsList"))
        Dictionary.Add("MainWindow.Title.RevertingChanges", FindString("MainWindow.Title.RevertingChanges"))
        Dictionary.Add("MainWindow.Title.CreatingThumb", FindString("MainWindow.Title.CreatingThumb"))
        Dictionary.Add("MainWindow.Title.MovingBackups", FindString("MainWindow.Title.MovingBackups"))

        Dictionary.Add("MainWindow.BackupButton.Content", FindString("MainWindow.BackupButton.Content"))
        Dictionary.Add("MainWindow.RestoreButton.Content", FindString("MainWindow.RestoreButton.Content"))
        Dictionary.Add("MainWindow.DeleteButton.Content", FindString("MainWindow.DeleteButton.Content"))
        Dictionary.Add("MainWindow.RenameButton.Content", FindString("MainWindow.RenameButton.Content"))
        Dictionary.Add("MainWindow.CullButton.Content", FindString("MainWindow.CullButton.Content"))
        Dictionary.Add("MainWindow.MoveToGroupButton.Text", FindString("MainWindow.MoveToGroupButton.Text"))
        Dictionary.Add("MainWindow.AutomaticBackupButton.Content", FindString("MainWindow.AutomaticBackupButton.Content"))

        Dictionary.Add("MainWindow.ListView.Columns(0).Header", FindString("MainWindow.ListView.Columns(0).Header"))
        Dictionary.Add("MainWindow.ListView.Columns(1).Header", FindString("MainWindow.ListView.Columns(1).Header"))
        Dictionary.Add("MainWindow.ListView.Columns(2).Header", FindString("MainWindow.ListView.Columns(2).Header"))

        Dictionary.Add("MainWindow.Sidebar.OriginalNameLabel.Text", FindString("MainWindow.Sidebar.OriginalNameLabel.Text"))
        Dictionary.Add("MainWindow.Sidebar.TypeLabel.Text", FindString("MainWindow.Sidebar.TypeLabel.Text"))
        Dictionary.Add("MainWindow.Sidebar.DescriptionLabel.Text", FindString("MainWindow.Sidebar.DescriptionLabel.Text"))
        Dictionary.Add("MainWindow.Sidebar.PlayerHealthLabel.Text", FindString("MainWindow.Sidebar.PlayerHealthLabel.Text"))
        Dictionary.Add("MainWindow.Sidebar.PlayerHungerLabel.Text", FindString("MainWindow.Sidebar.PlayerHungerLabel.Text"))
        Dictionary.Add("MainWindow.Sidebar.NoBackupSelected", FindString("MainWindow.Sidebar.NoBackupSelected"))
        Dictionary.Add("MainWindow.Sidebar.NumberElementsSelected", FindString("MainWindow.Sidebar.NumberElementsSelected"))
        Dictionary.Add("MainWindow.Sidebar.NumberElements", FindString("MainWindow.Sidebar.NumberElements"))
        Dictionary.Add("MainWindow.Sidebar.Description.NoDesc", FindString("MainWindow.Sidebar.Description.NoDesc"))
        Dictionary.Add("MainWindow.Sidebar.Description.NoItem", FindString("MainWindow.Sidebar.Description.NoItem"))

        Dictionary.Add("MainWindow.Search", FindString("MainWindow.Search"))

        Dictionary.Add("MainWindow.ListView.ContextMenu.OpenInExplorer", FindString("MainWindow.ListView.ContextMenu.OpenInExplorer"))

        Dictionary.Add("MainWindow.ListView.ContextMenu.SortBy", FindString("MainWindow.ListView.ContextMenu.SortBy"))
        Dictionary.Add("MainWindow.ListView.ContextMenu.SortBy.Name", FindString("MainWindow.ListView.ContextMenu.SortBy.Name"))
        Dictionary.Add("MainWindow.ListView.ContextMenu.SortBy.DateCreated", FindString("MainWindow.ListView.ContextMenu.SortBy.DateCreated"))
        Dictionary.Add("MainWindow.ListView.ContextMenu.SortBy.Type", FindString("MainWindow.ListView.ContextMenu.SortBy.Type"))
        Dictionary.Add("MainWindow.ListView.ContextMenu.SortBy.Ascending", FindString("MainWindow.ListView.ContextMenu.SortBy.Ascending"))
        Dictionary.Add("MainWindow.ListView.ContextMenu.SortBy.Descending", FindString("MainWindow.ListView.ContextMenu.SortBy.Descending"))

        Dictionary.Add("MainWindow.ListView.ContextMenu.GroupBy", FindString("MainWindow.ListView.ContextMenu.GroupBy"))
        Dictionary.Add("MainWindow.ListView.ContextMenu.GroupBy.OriginalName", FindString("MainWindow.ListView.ContextMenu.GroupBy.OriginalName"))
        Dictionary.Add("MainWindow.ListView.ContextMenu.GroupBy.Type", FindString("MainWindow.ListView.ContextMenu.GroupBy.Type"))
        Dictionary.Add("MainWindow.ListView.ContextMenu.GroupBy.Nothing", FindString("MainWindow.ListView.ContextMenu.GroupBy.Nothing"))

        Dictionary.Add("MainWindow.Groups.All", FindString("MainWindow.Groups.All"))

        Dictionary.Add("MainWindow.Toolbar.FileButton.Text", FindString("MainWindow.Toolbar.FileButton.Text"))
        Dictionary.Add("MainWindow.Toolbar.FileContextMenu.Items(0).Header", FindString("MainWindow.Toolbar.FileContextMenu.Items(0).Header"))
        Dictionary.Add("MainWindow.Toolbar.EditButton.Text", FindString("MainWindow.Toolbar.EditButton.Text"))
        Dictionary.Add("MainWindow.Toolbar.EditContextMenu.Items(0).Header", FindString("MainWindow.Toolbar.EditContextMenu.Items(0).Header"))
        Dictionary.Add("MainWindow.Toolbar.EditContextMenu.Items(1).Header", FindString("MainWindow.Toolbar.EditContextMenu.Items(1).Header"))
        Dictionary.Add("MainWindow.Toolbar.ToolsButton.Text", FindString("MainWindow.Toolbar.ToolsButton.Text"))
        Dictionary.Add("MainWindow.Toolbar.ToolsContextMenu.Items(0).Header", FindString("MainWindow.Toolbar.ToolsContextMenu.Items(0).Header"))
        Dictionary.Add("MainWindow.Toolbar.HelpButton.Text", FindString("MainWindow.Toolbar.HelpButton.Text"))
        Dictionary.Add("MainWindow.Toolbar.HelpContextMenu.Items(0).Header", FindString("MainWindow.Toolbar.HelpContextMenu.Items(0).Header"))
        Dictionary.Add("MainWindow.Toolbar.HelpContextMenu.Items(2).Header", FindString("MainWindow.Toolbar.HelpContextMenu.Items(2).Header"))
        Dictionary.Add("MainWindow.Toolbar.HelpContextMenu.Items(3).Header", FindString("MainWindow.Toolbar.HelpContextMenu.Items(3).Header"))

        Dictionary.Add("MainWindow.NoBackupsOverlay.Text", FindString("MainWindow.NoBackupsOverlay.Text"))

        Dictionary.Add("MainWindow.CancelButton.Text", FindString("MainWindow.CancelButton.Text"))

        ' = Statuses =
        Dictionary.Add("Status.Ready", FindString("Status.Ready"))
        Dictionary.Add("Status.CanceledAndReady", FindString("Status.CanceledAndReady"))
        Dictionary.Add("Status.BackingUp", FindString("Status.BackingUp"))
        Dictionary.Add("Status.BackupComplete", FindString("Status.BackupComplete"))
        Dictionary.Add("Status.CreatingThumb", FindString("Status.CreatingThumb"))
        Dictionary.Add("Status.RemovingOldContent", FindString("Status.RemovingOldContent"))
        Dictionary.Add("Status.Restoring", FindString("Status.Restoring"))
        Dictionary.Add("Status.RestoreComplete", FindString("Status.RestoreComplete"))
        Dictionary.Add("Status.Deleting", FindString("Status.Deleting"))
        Dictionary.Add("Status.DeleteComplete", FindString("Status.DeleteComplete"))
        Dictionary.Add("Status.RefreshingBackupsList", FindString("Status.RefreshingBackupsList"))
        Dictionary.Add("Status.RevertingChanges", FindString("Status.RevertingChanges"))
        Dictionary.Add("Status.MovingBackups", FindString("Status.MovingBackups"))
        Dictionary.Add("Status.StartingBackup", FindString("Status.StartingBackup"))

        ' = Messages =
        Dictionary.Add("Message.Caption.Information", FindString("Message.Caption.Information"))
        Dictionary.Add("Message.Caption.Warning", FindString("Message.Caption.Warning"))
        Dictionary.Add("Message.Caption.Error", FindString("Message.Caption.Error"))
        Dictionary.Add("Message.Caption.AreYouSure", FindString("Message.Caption.AreYouSure"))
        Dictionary.Add("Message.Caption.InvalidBackups", FindString("Message.Caption.InvalidBackups"))
        Dictionary.Add("Message.Caption.MCBackupIsWorking", FindString("Message.Caption.MCBackupIsWorking"))
        Dictionary.Add("Message.Caption.Copied", FindString("Message.Caption.Copied"))
        Dictionary.Add("Message.Caption.RestartRequired", FindString("Message.Caption.RestartRequired"))

        Dictionary.Add("Message.NoMinecraftInstallError", FindString("Message.NoMinecraftInstallError"))
        Dictionary.Add("Message.RestoreAreYouSure", FindString("Message.RestoreAreYouSure"))
        Dictionary.Add("Message.DeleteAreYouSure", FindString("Message.DeleteAreYouSure"))
        Dictionary.Add("Message.EnterValidName", FindString("Message.EnterValidName"))
        Dictionary.Add("Message.ChooseSave", FindString("Message.ChooseSave"))
        Dictionary.Add("Message.ChooseVersion", FindString("Message.ChooseVersion"))
        Dictionary.Add("Message.ResetSettings", FindString("Message.ResetSettings"))
        Dictionary.Add("Message.BackupInProgress", FindString("Message.BackupInProgress"))
        Dictionary.Add("Message.CopiedToClipboard", FindString("Message.CopiedToClipboard"))
        Dictionary.Add("Message.BackupsFolderNotFound", FindString("Message.BackupsFolderNotFound"))
        Dictionary.Add("Message.AreYouSureDeleteGroup", FindString("Message.AreYouSureDeleteGroup"))
        Dictionary.Add("Message.BackupAlreadyExists", FindString("Message.BackupAlreadyExists"))
        Dictionary.Add("Message.NoSavesWarning", FindString("Message.NoSavesWarning"))
        Dictionary.Add("Message.DisableAnonymousStats", FindString("Message.DisableAnonymousStats"))
        Dictionary.Add("Message.InvalidSavesFolder", FindString("Message.InvalidSavesFolder"))
        Dictionary.Add("Message.CouldNotSetBackupsFolder", FindString("Message.CouldNotSetBackupsFolder"))
        Dictionary.Add("Message.ConfirmInvalidMinecraftFolder", FindString("Message.ConfirmInvalidMinecraftFolder"))
        Dictionary.Add("Message.PleaseSelectFolder", FindString("Message.PleaseSelectFolder"))
        Dictionary.Add("Message.DeleteInvalidBackups", FindString("Message.DeleteInvalidBackups"))
        Dictionary.Add("Message.IncompatibleBackupConfig", FindString("Message.IncompatibleBackupConfig"))
        Dictionary.Add("Message.MCBackupIsWorking", FindString("Message.MCBackupIsWorking"))
        Dictionary.Add("Message.CancelBackup", FindString("Message.CancelBackup"))
        Dictionary.Add("Message.CancelRestore", FindString("Message.CancelRestore"))
        Dictionary.Add("Message.CancelDelete", FindString("Message.CancelDelete"))
        Dictionary.Add("Message.OpenWebpage", FindString("Message.OpenWebpage"))
        Dictionary.Add("Message.ExceptionOccured", FindString("Message.ExceptionOccured"))
        Dictionary.Add("Message.IllegalCharacters", FindString("Message.IllegalCharacters"))
        Dictionary.Add("Message.SettingsUpgrade", FindString("Message.SettingsUpgrade"))
        Dictionary.Add("Message.RestartAfterThemeChange", FindString("Message.RestartAfterThemeChange"))

        ' = Balloon Tips =
        Dictionary.Add("BalloonTip.Title.BackupError", FindString("BalloonTip.Title.BackupError"))
        Dictionary.Add("BalloonTip.BackupError", FindString("BalloonTip.BackupError"))
        Dictionary.Add("BalloonTip.Title.BackupComplete", FindString("BalloonTip.Title.BackupComplete"))
        Dictionary.Add("BalloonTip.BackupComplete", FindString("BalloonTip.BackupComplete"))
        Dictionary.Add("BalloonTip.Title.RestoreError", FindString("BalloonTip.Title.RestoreError"))
        Dictionary.Add("BalloonTip.RestoreError", FindString("BalloonTip.RestoreError"))
        Dictionary.Add("BalloonTip.Title.RestoreComplete", FindString("BalloonTip.Title.RestoreComplete"))
        Dictionary.Add("BalloonTip.RestoreComplete", FindString("BalloonTip.RestoreComplete"))
        Dictionary.Add("BalloonTip.Title.RunningBackground", FindString("BalloonTip.Title.RunningBackground"))
        Dictionary.Add("BalloonTip.RunningBackground", FindString("BalloonTip.RunningBackground"))
        Dictionary.Add("BalloonTip.Title.AutoBackup", FindString("BalloonTip.Title.AutoBackup"))
        Dictionary.Add("BalloonTip.AutoBackup", FindString("BalloonTip.AutoBackup"))

        ' = Backup Window =
        Dictionary.Add("BackupWindow.Title", FindString("BackupWindow.Title"))
        Dictionary.Add("BackupWindow.BackupDetailsGroupBox.Header", FindString("BackupWindow.BackupDetailsGroupBox.Header"))
        Dictionary.Add("BackupWindow.BackupNameGroupBox.Header", FindString("BackupWindow.BackupNameGroupBox.Header"))
        Dictionary.Add("BackupWindow.DefaultNameRadioButton.Content", FindString("BackupWindow.DefaultNameRadioButton.Content"))
        Dictionary.Add("BackupWindow.CustomNameRadioButton.Content", FindString("BackupWindow.CustomNameRadioButton.Content"))
        Dictionary.Add("BackupWindow.ShortDescriptionLabel.Content", FindString("BackupWindow.ShortDescriptionLabel.Content"))
        Dictionary.Add("BackupWindow.ListBox.Columns(0).Header", FindString("BackupWindow.ListBox.Columns(0).Header"))
        Dictionary.Add("BackupWindow.StartButton.Content", FindString("BackupWindow.StartButton.Content"))
        Dictionary.Add("BackupWindow.CancelButton.Content", FindString("BackupWindow.CancelButton.Content"))
        Dictionary.Add("BackupWindow.GroupLabel.Text", FindString("BackupWindow.GroupLabel.Text"))
        Dictionary.Add("BackupWindow.BackupWorldTab.Header", FindString("BackupWindow.BackupWorldTab.Header"))
        Dictionary.Add("BackupWindow.SaveNameColumn.Header", FindString("BackupWindow.SaveNameColumn.Header"))
        Dictionary.Add("BackupWindow.SaveLocationColumn.Header", FindString("BackupWindow.SaveLocationColumn.Header"))
        Dictionary.Add("BackupWindow.BackupVersionTab.Header.Minecraft", FindString("BackupWindow.BackupVersionTab.Header.Minecraft"))
        Dictionary.Add("BackupWindow.BackupVersionTab.Header.Technic", FindString("BackupWindow.BackupVersionTab.Header.Technic"))
        Dictionary.Add("BackupWindow.BackupVersionTab.Header.FeedTheBeast", FindString("BackupWindow.BackupVersionTab.Header.FeedTheBeast"))
        Dictionary.Add("BackupWindow.BackupVersionTab.Header.ATLauncher", FindString("BackupWindow.BackupVersionTab.Header.ATLauncher"))
        Dictionary.Add("BackupWindow.VersionNameColumn.Header", FindString("BackupWindow.VersionNameColumn.Header"))
        Dictionary.Add("BackupWindow.BackupEverythingTab.Header", FindString("BackupWindow.BackupEverythingTab.Header"))

        ' = Automatic Backup Window =
        Dictionary.Add("AutoBackupWindow.Title", FindString("AutoBackupWindow.Title"))
        Dictionary.Add("AutoBackupWindow.BackupEveryLabel.Content", FindString("AutoBackupWindow.BackupEveryLabel.Content"))
        Dictionary.Add("AutoBackupWindow.MinutesLabel.Content", FindString("AutoBackupWindow.MinutesLabel.Content"))
        Dictionary.Add("AutoBackupWindow.WorldToBackUpLabel.Text", FindString("AutoBackupWindow.WorldToBackUpLabel.Text"))
        Dictionary.Add("AutoBackupWindow.RefreshButton.Content", FindString("AutoBackupWindow.RefreshButton.Content"))
        Dictionary.Add("AutoBackupWindow.StartButton.Content.Start", FindString("AutoBackupWindow.StartButton.Content.Start"))
        Dictionary.Add("AutoBackupWindow.StartButton.Content.Stop", FindString("AutoBackupWindow.StartButton.Content.Stop"))
        Dictionary.Add("AutoBackupWindow.BackupDescription", FindString("AutoBackupWindow.BackupDescription"))

        ' = Options Window =
        Dictionary.Add("OptionsWindow.Title", FindString("OptionsWindow.Title"))

        Dictionary.Add("OptionsWindow.CloseButton.Content", FindString("OptionsWindow.CloseButton.Content"))
        Dictionary.Add("OptionsWindow.ResetButton.Content", FindString("OptionsWindow.ResetButton.Content"))

        Dictionary.Add("OptionsWindow.AllSupportedImages", FindString("OptionsWindow.AllSupportedImages"))

        Dictionary.Add("OptionsWindow.GeneralPanel.GeneralOptionsGroupBox.Header", FindString("OptionsWindow.GeneralPanel.GeneralOptionsGroupBox.Header"))
        Dictionary.Add("OptionsWindow.GeneralPanel.CloseToTrayOptionsGroupBox.Header", FindString("OptionsWindow.GeneralPanel.CloseToTrayOptionsGroupBox.Header"))
        Dictionary.Add("OptionsWindow.GeneralPanel.LanguageGroupBox.Header", FindString("OptionsWindow.GeneralPanel.LanguageGroupBox.Header"))

        Dictionary.Add("OptionsWindow.GeneralPanel.ShowBalloonTipsCheckBox.Content", FindString("OptionsWindow.GeneralPanel.ShowBalloonTipsCheckBox.Content"))
        Dictionary.Add("OptionsWindow.GeneralPanel.ShowDeleteConfirmationCheckBox.Content", FindString("OptionsWindow.GeneralPanel.ShowDeleteConfirmationCheckBox.Content"))
        Dictionary.Add("OptionsWindow.GeneralPanel.CheckForUpdatesCheckBox.Content", FindString("OptionsWindow.GeneralPanel.CheckForUpdatesCheckBox.Content"))
        Dictionary.Add("OptionsWindow.GeneralPanel.CreateThumbOnWorldCheckBox.Content", FindString("OptionsWindow.GeneralPanel.CreateThumbOnWorldCheckBox.Content"))
        Dictionary.Add("OptionsWindow.GeneralPanel.AlwaysCloseCheckBox.Content", FindString("OptionsWindow.GeneralPanel.AlwaysCloseCheckBox.Content"))
        Dictionary.Add("OptionsWindow.GeneralPanel.CloseToTrayRadioButton.Content", FindString("OptionsWindow.GeneralPanel.CloseToTrayRadioButton.Content"))
        Dictionary.Add("OptionsWindow.GeneralPanel.CloseCompletelyRadioButton.Content", FindString("OptionsWindow.GeneralPanel.CloseCompletelyRadioButton.Content"))
        Dictionary.Add("OptionsWindow.GeneralPanel.AlwaysCloseNoteTextBlock.Text", FindString("OptionsWindow.GeneralPanel.AlwaysCloseNoteTextBlock.Text"))
        Dictionary.Add("OptionsWindow.GeneralPanel.SendAnonymousDataCheckBox.Content", FindString("OptionsWindow.GeneralPanel.SendAnonymousDataCheckBox.Content"))

        Dictionary.Add("OptionsWindow.AppearancePanel.GeneralAppearanceGroupBox.Header", FindString("OptionsWindow.AppearancePanel.GeneralAppearanceGroupBox.Header"))
        Dictionary.Add("OptionsWindow.AppearancePanel.StatusTextColorGroupBox.Header", FindString("OptionsWindow.AppearancePanel.StatusTextColorGroupBox.Header"))
        Dictionary.Add("OptionsWindow.AppearancePanel.ListViewTextColorIntensityGroupBox.Header", FindString("OptionsWindow.AppearancePanel.ListViewTextColorIntensityGroupBox.Header"))

        Dictionary.Add("OptionsWindow.AppearancePanel.ListViewOpacityLabel.Content", FindString("OptionsWindow.AppearancePanel.ListViewOpacityLabel.Content"))
        Dictionary.Add("OptionsWindow.AppearancePanel.BackgroundImageLabel.Content", FindString("OptionsWindow.AppearancePanel.BackgroundImageLabel.Content"))
        Dictionary.Add("OptionsWindow.AppearancePanel.SizeModeLabel.Content", FindString("OptionsWindow.AppearancePanel.SizeModeLabel.Content"))
        Dictionary.Add("OptionsWindow.AppearancePanel.SizeModeComboBox.Items(0).Content", FindString("OptionsWindow.AppearancePanel.SizeModeComboBox.Items(0).Content"))
        Dictionary.Add("OptionsWindow.AppearancePanel.SizeModeComboBox.Items(1).Content", FindString("OptionsWindow.AppearancePanel.SizeModeComboBox.Items(1).Content"))
        Dictionary.Add("OptionsWindow.AppearancePanel.SizeModeComboBox.Items(2).Content", FindString("OptionsWindow.AppearancePanel.SizeModeComboBox.Items(2).Content"))
        Dictionary.Add("OptionsWindow.AppearancePanel.SizeModeComboBox.Items(3).Content", FindString("OptionsWindow.AppearancePanel.SizeModeComboBox.Items(3).Content"))
        Dictionary.Add("OptionsWindow.AppearancePanel.BackgroundImageBrowseButton.Content", FindString("OptionsWindow.AppearancePanel.BackgroundImageBrowseButton.Content"))
        Dictionary.Add("OptionsWindow.AppearancePanel.BackgroundImageRemoveButton.Content", FindString("OptionsWindow.AppearancePanel.BackgroundImageRemoveButton.Content"))
        Dictionary.Add("OptionsWindow.AppearancePanel.ThemeLabel.Content", FindString("OptionsWindow.AppearancePanel.ThemeLabel.Content"))
        Dictionary.Add("OptionsWindow.AppearancePanel.SampleText", FindString("OptionsWindow.AppearancePanel.SampleText"))

        Dictionary.Add("OptionsWindow.AppearancePanel.Themes", FindString("OptionsWindow.AppearancePanel.Themes"))
        Dictionary.Add("OptionsWindow.AppearancePanel.ThemeTags", FindString("OptionsWindow.AppearancePanel.ThemeTags"))
        Dictionary.Add("OptionsWindow.AppearancePanel.ThemeShades.Light", FindString("OptionsWindow.AppearancePanel.ThemeShades.Light"))
        Dictionary.Add("OptionsWindow.AppearancePanel.ThemeShades.Dark", FindString("OptionsWindow.AppearancePanel.ThemeShades.Dark"))

        Dictionary.Add("OptionsWindow.FoldersTab.InstallTypeGroupBox.Header", FindString("OptionsWindow.FoldersTab.InstallTypeGroupBox.Header"))
        Dictionary.Add("OptionsWindow.FoldersTab.MinecraftInstallationRadioButton.Text", FindString("OptionsWindow.FoldersTab.MinecraftInstallationRadioButton.Text"))
        Dictionary.Add("OptionsWindow.FoldersTab.TechnicInstallationRadioButton.Text", FindString("OptionsWindow.FoldersTab.TechnicInstallationRadioButton.Text"))
        Dictionary.Add("OptionsWindow.FoldersTab.FtbInstallationRadioButton.Text", FindString("OptionsWindow.FoldersTab.FtbInstallationRadioButton.Text"))
        Dictionary.Add("OptionsWindow.FoldersTab.AtLauncherInstallationRadioButton.Text", FindString("OptionsWindow.FoldersTab.AtLauncherInstallationRadioButton.Text"))
        Dictionary.Add("OptionsWindow.FoldersTab.BaseFolderLabel.Text", FindString("OptionsWindow.FoldersTab.BaseFolderLabel.Text"))
        Dictionary.Add("OptionsWindow.FoldersTab.BrowseButton.Text", FindString("OptionsWindow.FoldersTab.BrowseButton.Text"))
        Dictionary.Add("OptionsWindow.FoldersTab.GeneralFoldersGroupBox.Header", FindString("OptionsWindow.FoldersTab.GeneralFoldersGroupBox.Header"))
        Dictionary.Add("OptionsWindow.FoldersTab.SavesFolderLabel.Text", FindString("OptionsWindow.FoldersTab.SavesFolderLabel.Text"))
        Dictionary.Add("OptionsWindow.FoldersTab.BackupsFolderLabel.Text", FindString("OptionsWindow.FoldersTab.BackupsFolderLabel.Text"))

        Dictionary.Add("OptionsWindow.GroupsTab.AddNewGroupGroupBox.Header", FindString("OptionsWindow.GroupsTab.AddNewGroupGroupBox.Header"))
        Dictionary.Add("OptionsWindow.GroupsTab.OtherOptionsGroupBox.Header", FindString("OptionsWindow.GroupsTab.OtherOptionsGroupBox.Header"))
        Dictionary.Add("OptionsWindow.GroupsTab.DeleteGroupButton.Text", FindString("OptionsWindow.GroupsTab.DeleteGroupButton.Text"))
        Dictionary.Add("OptionsWindow.GroupsTab.RenameGroupButton.Text", FindString("OptionsWindow.GroupsTab.RenameGroupButton.Text"))
        Dictionary.Add("OptionsWindow.GroupsTab.MoveGroupUpButton.Text", FindString("OptionsWindow.GroupsTab.MoveGroupUpButton.Text"))
        Dictionary.Add("OptionsWindow.GroupsTab.MoveGroupDownButton.Text", FindString("OptionsWindow.GroupsTab.MoveGroupDownButton.Text"))

        Dictionary.Add("OptionsWindow.AdvancedTab.DefaultBackupNameLabel.Text", FindString("OptionsWindow.AdvancedTab.DefaultBackupNameLabel.Text"))
        Dictionary.Add("OptionsWindow.AdvancedTab.DefaultAutoBackupNameLabel.Text", FindString("OptionsWindow.AdvancedTab.DefaultAutoBackupNameLabel.Text"))
        Dictionary.Add("OptionsWindow.AdvancedTab.IgnoreSystemLocalizationCheckBox.Text", FindString("OptionsWindow.AdvancedTab.IgnoreSystemLocalizationCheckBox.Text"))
        Dictionary.Add("OptionsWindow.AdvancedTab.PlaceholdersLink.Text", FindString("OptionsWindow.AdvancedTab.PlaceholdersLink.Text"))

        Dictionary.Add("OptionsWindow.Tabs.General", FindString("OptionsWindow.Tabs.General"))
        Dictionary.Add("OptionsWindow.Tabs.Appearance", FindString("OptionsWindow.Tabs.Appearance"))
        Dictionary.Add("OptionsWindow.Tabs.Folders", FindString("OptionsWindow.Tabs.Folders"))
        Dictionary.Add("OptionsWindow.Tabs.Groups", FindString("OptionsWindow.Tabs.Groups"))
        Dictionary.Add("OptionsWindow.Tabs.Advanced", FindString("OptionsWindow.Tabs.Advanced"))

        ' = Rename Window =
        Dictionary.Add("RenameWindow.Title", FindString("RenameWindow.Title"))
        Dictionary.Add("RenameWindow.RenameButton.Content", FindString("RenameWindow.RenameButton.Content"))
        Dictionary.Add("RenameWindow.CancelButton.Content", FindString("RenameWindow.CancelButton.Content"))

        ' = About Window =
        Dictionary.Add("AboutWindow.Title", FindString("AboutWindow.Title"))
        Dictionary.Add("AboutWindow.Text", FindString("AboutWindow.Text"))

        ' = Close to tray window =
        Dictionary.Add("CloseToTrayWindow.Title", FindString("CloseToTrayWindow.Title"))
        Dictionary.Add("CloseToTrayWindow.MessageLabel.Content", FindString("CloseToTrayWindow.MessageLabel.Content"))
        Dictionary.Add("CloseToTrayWindow.YesButton.Content", FindString("CloseToTrayWindow.YesButton.Content"))
        Dictionary.Add("CloseToTrayWindow.NoButton.Content", FindString("CloseToTrayWindow.NoButton.Content"))
        Dictionary.Add("CloseToTrayWindow.CancelButton.Content", FindString("CloseToTrayWindow.CancelButton.Content"))
        Dictionary.Add("CloseToTrayWindow.SaveCheckBox.Content", FindString("CloseToTrayWindow.SaveCheckBox.Content"))
        Dictionary.Add("CloseToTrayWindow.RevertLabel.Content", FindString("CloseToTrayWindow.RevertLabel.Content"))

        ' = Error window =
        Dictionary.Add("ErrorWindow.ContinueButton.Content", FindString("ErrorWindow.ContinueButton.Content"))
        Dictionary.Add("ErrorWindow.CopyToClipboardButton.Content", FindString("ErrorWindow.CopyToClipboardButton.Content"))
        Dictionary.Add("ErrorWindow.ContactMessage", FindString("ErrorWindow.ContactMessage"))

        ' = Cull Window = 
        Dictionary.Add("CullWindow.Title", FindString("CullWindow.Title"))
        Dictionary.Add("CullWindow.Label1.Content", FindString("CullWindow.Label1.Content"))
        Dictionary.Add("CullWindow.Label2.Content", FindString("CullWindow.Label2.Content"))
        Dictionary.Add("CullWindow.CullButton.Content", FindString("CullWindow.CullButton.Content"))
        Dictionary.Add("CullWindow.AreYouSureMsg", FindString("CullWindow.AreYouSureMsg"))

        ' = Backup Types =
        Dictionary.Add("BackupTypes.Save", FindString("BackupTypes.Save"))
        Dictionary.Add("BackupTypes.Version", FindString("BackupTypes.Version"))
        Dictionary.Add("BackupTypes.Everything", FindString("BackupTypes.Everything"))

        ' = Metro Message Box =
        Dictionary.Add("MetroMsgBox.Button.OK", FindString("MetroMsgBox.Button.OK"))
        Dictionary.Add("MetroMsgBox.Button.Yes", FindString("MetroMsgBox.Button.Yes"))
        Dictionary.Add("MetroMsgBox.Button.No", FindString("MetroMsgBox.Button.No"))
        Dictionary.Add("MetroMsgBox.Button.Cancel", FindString("MetroMsgBox.Button.Cancel"))

        ' = Errors (exceptions) =
        Dictionary.Add("Exception.Backup", FindString("Exception.Backup"))
        Dictionary.Add("Exception.Restore", FindString("Exception.Restore"))
        Dictionary.Add("Exception.Delete", FindString("Exception.Delete"))
        Dictionary.Add("Exception.Rename", FindString("Exception.Rename"))

        ' = Time Left Strings =
        Dictionary.Add("TimeLeft.LessThanFive", FindString("TimeLeft.LessThanFive"))
        Dictionary.Add("TimeLeft.Seconds", FindString("TimeLeft.Seconds"))
        Dictionary.Add("TimeLeft.MinutesSeconds", FindString("TimeLeft.MinutesSeconds"))

        ' = Update Dialog =
        Dictionary.Add("UpdateDialog.Title", FindString("UpdateDialog.Title"))
        Dictionary.Add("UpdateDialog.Label1.Text", FindString("UpdateDialog.Label1.Text"))
        Dictionary.Add("UpdateDialog.CurrentVersionLabel.Text", FindString("UpdateDialog.CurrentVersionLabel.Text"))
        Dictionary.Add("UpdateDialog.LatestVersionLabel.Text", FindString("UpdateDialog.LatestVersionLabel.Text"))
        Dictionary.Add("UpdateDialog.Label2.Text", FindString("UpdateDialog.Label2.Text"))
        Dictionary.Add("UpdateDialog.ShowChangelogButton.Text", FindString("UpdateDialog.ShowChangelogButton.Text"))

        ' = Move to Group Dialog =
        Dictionary.Add("MoveToGroupDialog.Title", FindString("MoveToGroupDialog.Title"))
        Dictionary.Add("MoveToGroupDialog.MoveButton.Text", FindString("MoveToGroupDialog.MoveButton.Text"))

        ' = Set Minecraft Folder Window =
        Dictionary.Add("SetMinecraftFolderWindow.Title", FindString("SetMinecraftFolderWindow.Title"))
        Dictionary.Add("SetMinecraftFolderWindow.SaveButton.Text", FindString("SetMinecraftFolderWindow.SaveButton.Text"))

        Dictionary.Add("DeleteDialog.DoNotAskAgain.Text", FindString("DeleteDialog.DoNotAskAgain.Text"))

        Dictionary.Add("Localization.DefaultDateFormat", FindString("Localization.DefaultDateFormat"))
        Dictionary.Add("Localization.DefaultBackupName", FindString("Localization.DefaultBackupName"))
        Dictionary.Add("Localization.DefaultAutoBackupName", FindString("Localization.DefaultAutoBackupName"))
        Dictionary.Add("Localization.Output", FindString("Localization.Output"))
        Dictionary.Add("Localization.DirectoryName", FindString("Localization.DirectoryName"))

        Dictionary.Add("Groups.None", FindString("Groups.None"))
        Dictionary.Add("Groups.AutoBackups", FindString("Groups.AutoBackups"))
        Dictionary.Add("Groups.EditGroups", FindString("Groups.EditGroups"))

        If ErrorOccured Then
            Log.Print("Language loaded with errors. Please try solving the error(s) above.", Log.Level.Warning)
        Else
            Log.Print("Language loaded. No errors occured.")
        End If

        IsLoaded = True
    End Sub

    Private Shared Function FindString(Identifier As String)
        Using SR As New StreamReader(Directory.GetCurrentDirectory & "\language\" & LanguageFile)
            Dim LineNumber As Integer = 0
            While SR.Peek <> -1
                LineNumber += 1
                Dim Line As String = SR.ReadLine

                If Line.StartsWith(Identifier & "=") And Not Line.StartsWith("#") Then
                    Dim ReturnString = Line.Substring(Identifier.Length + 1)

                    If String.IsNullOrEmpty(ReturnString) Then
                        Log.Print("Language Error at line " & LineNumber & ": Entry is empty!", Log.Level.Warning)
                        ErrorOccured = True
                        Return Identifier.Split(".")(Identifier.Split(".").Count - 2) & "." & Identifier.Split(".").Last
                    End If

                    Return ReturnString.Replace("\n", vbNewLine)
                End If
            End While
        End Using
        Log.Print("Language Error: '" & Identifier & "' identifier not found, added automatically.", Log.Level.Warning)
        Using SW As New StreamWriter(Directory.GetCurrentDirectory() & "\language\" & LanguageFile, True)
            SW.Write(vbNewLine & Identifier & "=")
        End Using
        ErrorOccured = True
        If Not String.IsNullOrEmpty(Identifier) Then
            Return Identifier.Split(".")(Identifier.Split(".").Count - 2) & "." & Identifier.Split(".").Last
        Else
            Return "Error"
        End If
    End Function

    Public Shared Function FindString(Identifier As String, LanguageFile As String)
        Using SR As New StreamReader(Directory.GetCurrentDirectory & "\language\" & LanguageFile)
            Dim LineNumber As Integer = 0
            While SR.Peek <> -1
                LineNumber += 1
                Dim Line As String = SR.ReadLine
                If Line.StartsWith(Identifier & "=") And Not Line.StartsWith("#") Then
                    Dim ReturnString = Line.Substring(Identifier.Length + 1)

                    If String.IsNullOrEmpty(ReturnString) Then
                        Log.Print("[Language] Error at line " & LineNumber & ": Entry is empty!", Log.Level.Warning)
                        ErrorOccured = True
                        Exit While
                    End If

                    Return ReturnString.Replace("\n", vbNewLine)
                End If
            End While
        End Using
        Log.Print("[Language] Error: '" & Identifier & "' identifier not found, added automatically.", Log.Level.Warning)
        Using SW As New StreamWriter(Directory.GetCurrentDirectory() & "\language\" & LanguageFile, True)
            SW.Write(vbNewLine & Identifier & "=")
        End Using
        ErrorOccured = True
        If Regex.Matches(Identifier, "\.").Count > 1 Then
            Return Identifier.Split(".")(Identifier.Split(".").Count - 2) & "." & Identifier.Split(".").Last
        Else
            Return Identifier
        End If
    End Function
End Class
