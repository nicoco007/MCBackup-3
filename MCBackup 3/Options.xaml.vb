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

Imports System.Linq
Imports System.Security.Permissions
Imports System.Security
Imports MahApps.Metro

Partial Public Class Options
    Private Main As MainWindow = DirectCast(Application.Current.MainWindow, MainWindow)
    Private FolderBrowserDialog As New System.Windows.Forms.FolderBrowserDialog
    Private OpenFileDialog As New System.Windows.Forms.OpenFileDialog

    Public Sub New()
        InitializeComponent()

        OpenFileDialog.Filter = MCBackup.Language.Dictionary("OptionsWindow.AllSupportedImages") & " (*bmp, *.jpg, *.jpeg, *.png)|*bmp;*.gif;*.png;*.jpg;*.jpeg|BMP (*.bmp)|*.bmp|JPEG (*.jpg, *.jpeg)|*.jpg;*.jpeg|PNG (*.png)|*.png"

        MinecraftFolderTextBox.Text = My.Settings.MinecraftFolderLocation
        BackupsFolderTextBox.Text = My.Settings.BackupsFolderLocation
        SavesFolderTextBox.Text = My.Settings.SavesFolderLocation
        ListViewOpacitySlider.Value = My.Settings.InterfaceOpacity
        OpacityPercentLabel.Content = Int(ListViewOpacitySlider.Value).ToString & "%"
        SizeModeComboBox.SelectedIndex = My.Settings.BackgroundImageStretch
        CheckForUpdatesCheckBox.IsChecked = My.Settings.CheckForUpdates
        ShowBalloonTipsCheckBox.IsChecked = My.Settings.ShowBalloonTips
        CreateThumbOnWorldCheckBox.IsChecked = My.Settings.CreateThumbOnWorld

        AlwaysCloseCheckBox.IsChecked = My.Settings.SaveCloseState
        CloseToTrayRadioButton.IsChecked = My.Settings.CloseToTray
        CloseCompletelyRadioButton.IsChecked = Not My.Settings.CloseToTray

        Dim StatusLabelColor = My.Settings.StatusLabelColor
        RedColorSlider.Value = StatusLabelColor.R
        GreenColorSlider.Value = StatusLabelColor.G
        BlueColorSlider.Value = StatusLabelColor.B

        ColorSlider_ValueChanged(Nothing, Nothing)

        RedColorLabel.ContextMenu = Nothing
        GreenColorLabel.ContextMenu = Nothing
        BlueColorLabel.ContextMenu = Nothing
    End Sub

    Public Overloads Sub ShowDialog(Tab As Integer)
        If Not Tab > TabControl.Items.Count - 1 Then
            TabControl.SelectedIndex = Tab
        End If
        MyBase.ShowDialog()
    End Sub

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        Dim LanguageDirectory As New IO.DirectoryInfo(Main.StartupPath & "\language")
        Dim LanguageFiles As IO.FileInfo() = LanguageDirectory.GetFiles()
        Dim LanguageFile As IO.FileInfo

        For Each LanguageFile In LanguageFiles
            LanguagesComboBox.Items.Add(New TaggedComboBoxItem(MCBackup.Language.FindString("fullname", LanguageFile.Name) & " (" & IO.Path.GetFileNameWithoutExtension(LanguageFile.Name) & ")", LanguageFile.Name))
        Next

        LanguagesComboBox.SelectedItem = LanguagesComboBox.Items.OfType(Of TaggedComboBoxItem)().FirstOrDefault(Function(Item) Item.Tag = My.Settings.Language & ".lang")

        AlwaysCloseCheckBox_Checked(sender, Nothing)

        LoadLanguage()

        ListViewTextColorIntensitySlider.Value = My.Settings.ListViewTextColorIntensity

        ReloadBackupGroups()
    End Sub

    Private Sub AlwaysCloseCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles AlwaysCloseCheckBox.Click
        CloseToTrayRadioButton.IsEnabled = AlwaysCloseCheckBox.IsChecked
        CloseCompletelyRadioButton.IsEnabled = AlwaysCloseCheckBox.IsChecked
    End Sub

    Private Sub BrowseMinecraftFolderButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseMinecraftFolderButton.Click
        If FolderBrowserDialog.ShowDialog = Forms.DialogResult.OK Then
            If My.Computer.FileSystem.FileExists(FolderBrowserDialog.SelectedPath & "\launcher.jar") Then ' Check if Minecraft exists in that folder
                MinecraftFolderTextBox.Text = FolderBrowserDialog.SelectedPath
                SavesFolderTextBox.Text = FolderBrowserDialog.SelectedPath & "\saves"
                Exit Sub
            Else
                If MetroMessageBox.Show(MCBackup.Language.Dictionary("Message.NotInstalledInFolder"), MCBackup.Language.Dictionary("Message.Caption.Error"), MessageBoxButton.YesNo, MessageBoxImage.Error) = Windows.Forms.DialogResult.Yes Then ' Ask if user wants to try finding folder again
                    BrowseMinecraftFolderButton_Click(sender, e) ' Restart from beginning if "Yes"
                End If
            End If
        End If
    End Sub

    Private Sub BrowseBackupsFolderButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseBackupsFolderButton.Click
        If FolderBrowserDialog.ShowDialog = Forms.DialogResult.OK Then
            Try
                IO.File.Create(FolderBrowserDialog.SelectedPath & "\.tmp").Dispose()
                My.Computer.FileSystem.DeleteFile(FolderBrowserDialog.SelectedPath & "\.tmp")
                BackupsFolderTextBox.Text = FolderBrowserDialog.SelectedPath
            Catch ex As Exception
                Log.Print(ex.Message, Log.Type.Severe)
                MetroMessageBox.Show(String.Format(MCBackup.Language.Dictionary("Message.SetBackupsFolderError"), FolderBrowserDialog.SelectedPath), MCBackup.Language.Dictionary("Message.Caption.Error"), MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End If
    End Sub

    Private Sub BrowseSavesFolderButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseSavesFolderButton.Click
        If FolderBrowserDialog.ShowDialog = Forms.DialogResult.OK Then
            If Not IO.Path.GetFileName(FolderBrowserDialog.SelectedPath) = "saves" Then
                Select Case MetroMessageBox.Show(String.Format(MCBackup.Language.Dictionary("Message.SetSavesFolderWarning"), FolderBrowserDialog.SelectedPath), MCBackup.Language.Dictionary("Message.Caption.AreYouSure"), MessageBoxButton.YesNoCancel, MessageBoxImage.Question)
                    Case MessageBoxResult.Yes
                        SavesFolderTextBox.Text = FolderBrowserDialog.SelectedPath
                    Case MessageBoxResult.No
                        BrowseSavesFolderButton_Click(sender, e)
                    Case Else
                        Exit Sub
                End Select
            Else
                SavesFolderTextBox.Text = FolderBrowserDialog.SelectedPath
            End If
        End If
    End Sub

    Private Sub ListViewOpacitySlider_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        If Me.IsLoaded Then
            Main.ListView.Opacity = ListViewOpacitySlider.Value / 100
            Main.Sidebar.Background = New SolidColorBrush(Color.FromArgb(ListViewOpacitySlider.Value * 2.55, 255, 255, 255))
            OpacityPercentLabel.Content = Math.Round(ListViewOpacitySlider.Value, 0).ToString & "%"
        End If
    End Sub

    Private Sub BackgroundImageBrowseButton_Click(sender As Object, e As RoutedEventArgs) Handles BackgroundImageBrowseButton.Click
        If OpenFileDialog.ShowDialog = Forms.DialogResult.OK Then
            Dim Brush As New ImageBrush(New BitmapImage(New Uri(OpenFileDialog.FileName)))
            Brush.Stretch = My.Settings.BackgroundImageStretch
            Main.Background = Brush
            My.Settings.BackgroundImageLocation = OpenFileDialog.FileName
        End If
    End Sub

    Private Sub BackgroundImageRemoveButton_Click(sender As Object, e As RoutedEventArgs) Handles BackgroundImageRemoveButton.Click
        Main.Background = New SolidColorBrush(Color.FromArgb(255, 240, 240, 240))
        My.Settings.BackgroundImageLocation = ""
    End Sub

    Private Sub BackgroundImageStyle_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles SizeModeComboBox.SelectionChanged
        If Not Me.IsLoaded Or My.Settings.BackgroundImageLocation = "" Then
            Exit Sub
        End If

        Try
            Dim Brush As New ImageBrush(New BitmapImage(New Uri(My.Settings.BackgroundImageLocation)))
            Select Case SizeModeComboBox.SelectedIndex
                Case 0
                    Brush.Stretch = Stretch.None
                Case 1
                    Brush.Stretch = Stretch.Fill
                Case 2
                    Brush.Stretch = Stretch.Uniform
                Case 3
                    Brush.Stretch = Stretch.UniformToFill
            End Select
            My.Settings.BackgroundImageStretch = Int(Brush.Stretch)
            Main.Background = Brush
        Catch ex As Exception
            Log.Print(ex.Message, Log.Type.Severe)
        End Try
    End Sub

    Private Sub SaveButton_Click(sender As Object, e As RoutedEventArgs) Handles CloseButton.Click
        Me.Close()
    End Sub

    Private Sub Window_Unloaded(sender As Object, e As RoutedEventArgs) Handles MyBase.Unloaded
        My.Settings.MinecraftFolderLocation = MinecraftFolderTextBox.Text
        Log.Print("Minecraft folder location set to " & My.Settings.MinecraftFolderLocation)
        My.Settings.SavesFolderLocation = SavesFolderTextBox.Text
        Log.Print("Saves folder location set to " & My.Settings.SavesFolderLocation)
        My.Settings.BackupsFolderLocation = BackupsFolderTextBox.Text
        Log.Print("Backups folder location set to " & My.Settings.BackupsFolderLocation)
        My.Settings.InterfaceOpacity = ListViewOpacitySlider.Value
        My.Settings.CheckForUpdates = CheckForUpdatesCheckBox.IsChecked
        My.Settings.ShowBalloonTips = ShowBalloonTipsCheckBox.IsChecked
        My.Settings.CreateThumbOnWorld = CreateThumbOnWorldCheckBox.IsChecked

        If AlwaysCloseCheckBox.IsChecked Then
            My.Settings.SaveCloseState = True
            My.Settings.CloseToTray = CloseToTrayRadioButton.IsChecked
        Else
            My.Settings.SaveCloseState = False
        End If
        Log.Print("Saving settings...")
        My.Settings.Save()
        Main.RefreshBackupsList()
    End Sub

    Private Sub LanguagesListBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles LanguagesComboBox.SelectionChanged
        If Me.IsLoaded Then
            MCBackup.Language.Load(LanguagesComboBox.SelectedItem.Tag)
            My.Settings.Language = IO.Path.GetFileNameWithoutExtension(LanguagesComboBox.SelectedItem.Tag)
            LoadLanguage()
        End If
    End Sub

    Private Sub LoadLanguage()
        Me.Title = MCBackup.Language.Dictionary("OptionsWindow.Title")

        CloseButton.Content = MCBackup.Language.Dictionary("OptionsWindow.CloseButton.Content")
        ResetButton.Content = MCBackup.Language.Dictionary("OptionsWindow.ResetButton.Content")

        GeneralTabItem.Header = MCBackup.Language.Dictionary("OptionsWindow.Tabs.General")
        AppearanceTabItem.Header = MCBackup.Language.Dictionary("OptionsWindow.Tabs.Appearance")
        FoldersTabItem.Header = MCBackup.Language.Dictionary("OptionsWindow.Tabs.Folders")
        GroupsTabItem.Header = MCBackup.Language.Dictionary("OptionsWindow.Tabs.Groups")

        ' General Tab
        GeneralOptionsGroupBox.Header = MCBackup.Language.Dictionary("OptionsWindow.GeneralPanel.GeneralOptionsGroupBox.Header")
        CloseToTrayOptionsGroupBox.Header = MCBackup.Language.Dictionary("OptionsWindow.GeneralPanel.CloseToTrayOptionsGroupBox.Header")
        LanguageGroupBox.Header = MCBackup.Language.Dictionary("OptionsWindow.GeneralPanel.LanguageGroupBox.Header")

        ShowBalloonTipsCheckBox.Content = MCBackup.Language.Dictionary("OptionsWindow.GeneralPanel.ShowBalloonTipsCheckBox.Content")
        ShowDeleteConfirmationCheckBox.Content = MCBackup.Language.Dictionary("OptionsWindow.GeneralPanel.ShowDeleteConfirmationCheckBox.Content")
        CheckForUpdatesCheckBox.Content = MCBackup.Language.Dictionary("OptionsWindow.GeneralPanel.CheckForUpdatesCheckBox.Content")
        CreateThumbOnWorldCheckBox.Content = MCBackup.Language.Dictionary("OptionsWindow.GeneralPanel.CreateThumbOnWorldCheckBox.Content")
        AlwaysCloseCheckBox.Content = MCBackup.Language.Dictionary("OptionsWindow.GeneralPanel.AlwaysCloseCheckBox.Content")
        CloseToTrayRadioButton.Content = MCBackup.Language.Dictionary("OptionsWindow.GeneralPanel.CloseToTrayRadioButton.Content")
        CloseCompletelyRadioButton.Content = MCBackup.Language.Dictionary("OptionsWindow.GeneralPanel.CloseCompletelyRadioButton.Content")
        AlwaysCloseNoteTextBlock.Text = MCBackup.Language.Dictionary("OptionsWindow.GeneralPanel.AlwaysCloseNoteTextBlock.Text")

        ' Appearance 
        GeneralAppearanceGroupBox.Header = MCBackup.Language.Dictionary("OptionsWindow.AppearancePanel.GeneralAppearanceGroupBox.Header")
        StatusTextColorGroupBox.Header = MCBackup.Language.Dictionary("OptionsWindow.AppearancePanel.StatusTextColorGroupBox.Header")
        ListViewTextColorIntensityGroupBox.Header = MCBackup.Language.Dictionary("OptionsWindow.AppearancePanel.ListViewTextColorIntensityGroupBox.Header")

        ListViewOpacityLabel.Content = MCBackup.Language.Dictionary("OptionsWindow.AppearancePanel.ListViewOpacityLabel.Content")
        BackgroundImageLabel.Content = MCBackup.Language.Dictionary("OptionsWindow.AppearancePanel.BackgroundImageLabel.Content")
        SizeModeLabel.Content = MCBackup.Language.Dictionary("OptionsWindow.AppearancePanel.SizeModeLabel.Content")
        SizeModeComboBox.Items(0).Content = MCBackup.Language.Dictionary("OptionsWindow.AppearancePanel.SizeModeComboBox.Items(0).Content")
        SizeModeComboBox.Items(1).Content = MCBackup.Language.Dictionary("OptionsWindow.AppearancePanel.SizeModeComboBox.Items(1).Content")
        SizeModeComboBox.Items(2).Content = MCBackup.Language.Dictionary("OptionsWindow.AppearancePanel.SizeModeComboBox.Items(2).Content")
        SizeModeComboBox.Items(3).Content = MCBackup.Language.Dictionary("OptionsWindow.AppearancePanel.SizeModeComboBox.Items(3).Content")
        BackgroundImageBrowseButton.Content = MCBackup.Language.Dictionary("OptionsWindow.AppearancePanel.BackgroundImageBrowseButton.Content")
        BackgroundImageRemoveButton.Content = MCBackup.Language.Dictionary("OptionsWindow.AppearancePanel.BackgroundImageRemoveButton.Content")
        ThemeLabel.Content = MCBackup.Language.Dictionary("OptionsWindow.AppearancePanel.ThemeLabel.Content")
        SampleTextG1.Content = MCBackup.Language.Dictionary("OptionsWindow.AppearancePanel.SampleText")
        SampleTextY1.Content = MCBackup.Language.Dictionary("OptionsWindow.AppearancePanel.SampleText")
        SampleTextR1.Content = MCBackup.Language.Dictionary("OptionsWindow.AppearancePanel.SampleText")

        ThemeComboBox.Items.Clear()

        Dim Names = MCBackup.Language.Dictionary("OptionsWindow.AppearancePanel.Themes").Split(";")
        Dim Tags = MCBackup.Language.Dictionary("OptionsWindow.AppearancePanel.ThemeTags").Split(";")

        For i As Integer = 0 To Names.Count - 1
            ThemeComboBox.Items.Add(New TaggedComboBoxItem(Names(i), Tags(i)))
            If Tags(i) = My.Settings.Theme Then
                ThemeComboBox.SelectedIndex = i
            End If
        Next

        ' Folders
        GeneralFoldersGroupBox.Header = MCBackup.Language.Dictionary("OptionsWindow.FoldersPanel.GeneralFoldersGroupBox.Header")

        MinecraftFolderLocationLabel.Content = MCBackup.Language.Dictionary("OptionsWindow.FoldersPanel.MinecraftFolderLocationLabel.Content")
        SavesFolderLocationLabel.Content = MCBackup.Language.Dictionary("OptionsWindow.FoldersPanel.SavesFolderLocationLabel.Content")
        BackupsFolderLocationLabel.Content = MCBackup.Language.Dictionary("OptionsWindow.FoldersPanel.BackupsFolderLocationLabel.Content")
        BrowseMinecraftFolderButton.Content = MCBackup.Language.Dictionary("OptionsWindow.FoldersPanel.BrowseButton.Content")
        BrowseSavesFolderButton.Content = MCBackup.Language.Dictionary("OptionsWindow.FoldersPanel.BrowseButton.Content")
        BrowseBackupsFolderButton.Content = MCBackup.Language.Dictionary("OptionsWindow.FoldersPanel.BrowseButton.Content")

        ' Groups
        AddNewGroupGroupBox.Header = MCBackup.Language.Dictionary("OptionsWindow.GroupsTab.AddNewGroupGroupBox.Header")
        OtherOptionsGroupBox.Header = MCBackup.Language.Dictionary("OptionsWindow.GroupsTab.OtherOptionsGroupBox.Header")
        DeleteGroupButton.Content = MCBackup.Language.Dictionary("OptionsWindow.GroupsTab.DeleteGroupButton.Text")
        RenameGroupButton.Content = MCBackup.Language.Dictionary("OptionsWindow.GroupsTab.RenameGroupButton.Text")
    End Sub

    Private Sub ColorSlider_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles RedColorSlider.ValueChanged, GreenColorSlider.ValueChanged, BlueColorSlider.ValueChanged
        RedColorLabel.Text = CInt(RedColorSlider.Value)
        GreenColorLabel.Text = CInt(GreenColorSlider.Value)
        BlueColorLabel.Text = CInt(BlueColorSlider.Value)
        My.Settings.StatusLabelColor = Color.FromRgb(RedColorSlider.Value, GreenColorSlider.Value, BlueColorSlider.Value)
        ColorRectangle.Fill = New SolidColorBrush(My.Settings.StatusLabelColor)
        Main.StatusLabel.Foreground = New SolidColorBrush(My.Settings.StatusLabelColor)

        Dim RedGradient = New LinearGradientBrush()
        RedGradient.StartPoint = New Point(0, 0)
        RedGradient.EndPoint = New Point(1, 1)
        RedGradient.GradientStops.Add(New GradientStop(Color.FromRgb(0, GreenColorSlider.Value, BlueColorSlider.Value), 0.0))
        RedGradient.GradientStops.Add(New GradientStop(Color.FromRgb(255, GreenColorSlider.Value, BlueColorSlider.Value), 1.0))
        RedRect.Fill = RedGradient

        Dim GreenGradient = New LinearGradientBrush()
        GreenGradient.StartPoint = New Point(0, 0)
        GreenGradient.EndPoint = New Point(1, 1)
        GreenGradient.GradientStops.Add(New GradientStop(Color.FromRgb(RedColorSlider.Value, 0, BlueColorSlider.Value), 0.0))
        GreenGradient.GradientStops.Add(New GradientStop(Color.FromRgb(RedColorSlider.Value, 255, BlueColorSlider.Value), 1.0))
        GreenRect.Fill = GreenGradient

        Dim BlueGradient = New LinearGradientBrush()
        BlueGradient.StartPoint = New Point(0, 0)
        BlueGradient.EndPoint = New Point(1, 1)
        BlueGradient.GradientStops.Add(New GradientStop(Color.FromRgb(RedColorSlider.Value, GreenColorSlider.Value, 0), 0.0))
        BlueGradient.GradientStops.Add(New GradientStop(Color.FromRgb(RedColorSlider.Value, GreenColorSlider.Value, 255), 1.0))
        BlueRect.Fill = BlueGradient
    End Sub

    Private Sub ThemeComboBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles ThemeComboBox.SelectionChanged
        If Not ThemeComboBox.SelectedItem Is Nothing Then
            Dim SelectedTag = DirectCast(ThemeComboBox.SelectedItem, TaggedComboBoxItem).Tag
            ThemeManager.ChangeTheme(My.Application, New Accent(SelectedTag, New Uri("pack://application:,,,/MahApps.Metro;component/Styles/Accents/" & SelectedTag & ".xaml")), Theme.Light)
            My.Settings.Theme = ThemeComboBox.SelectedItem.Tag.ToString
        End If
    End Sub

    Private Sub ListViewTextColorIntensitySlider_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles ListViewTextColorIntensitySlider.ValueChanged
        My.Settings.ListViewTextColorIntensity = ListViewTextColorIntensitySlider.Value
        SampleTextR1.Foreground = New SolidColorBrush(Color.FromRgb(ListViewTextColorIntensitySlider.Value, 0, 0))
        SampleTextY1.Foreground = New SolidColorBrush(Color.FromRgb(ListViewTextColorIntensitySlider.Value, ListViewTextColorIntensitySlider.Value, 0))
        SampleTextG1.Foreground = New SolidColorBrush(Color.FromRgb(0, ListViewTextColorIntensitySlider.Value, 0))
    End Sub

    Private Sub RedColorLabel_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles RedColorLabel.PreviewTextInput, GreenColorLabel.PreviewTextInput, BlueColorLabel.PreviewTextInput
        If Not AreAllValidNumericCharacters(e.Text) Then
            e.Handled = True
            System.Media.SystemSounds.Asterisk.Play()
        End If
    End Sub

    Private Function AreAllValidNumericCharacters(str As String)
        For Each Character As Char In str
            If Not Char.IsNumber(Character) Then Return False
        Next
        Return True
    End Function

    Private Sub ColorLabel_PreviewExecuted(sender As Object, e As ExecutedRoutedEventArgs)
        If e.Command Is ApplicationCommands.Paste Then
            e.Handled = True
            System.Media.SystemSounds.Asterisk.Play()
        End If
    End Sub

    Private Sub RedColorLabel_TextChanged(sender As Object, e As TextChangedEventArgs) Handles RedColorLabel.TextChanged, GreenColorLabel.TextChanged, BlueColorLabel.TextChanged
        If Me.IsLoaded Then
            If RedColorLabel.Text = "" Then
                RedColorLabel.Text = "0"
            End If

            If GreenColorLabel.Text = "" Then
                GreenColorLabel.Text = "0"
            End If

            If BlueColorLabel.Text = "" Then
                BlueColorLabel.Text = "0"
            End If

            RedColorSlider.Value = CInt(RedColorLabel.Text)
            GreenColorSlider.Value = CInt(GreenColorLabel.Text)
            BlueColorSlider.Value = CInt(BlueColorLabel.Text)
        End If
    End Sub

    Private Sub ResetButton_Click(sender As Object, e As RoutedEventArgs) Handles ResetButton.Click
        If MetroMessageBox.Show(MCBackup.Language.Dictionary("Message.ResetSettings"), MCBackup.Language.Dictionary("Message.Caption.AreYouSure"), MessageBoxButton.YesNo, MessageBoxImage.Question) = MessageBoxResult.Yes Then
            My.Settings.Reset()

            Process.Start(Application.ResourceAssembly.Location)
            Main.ClsType = CloseAction.CloseType.ForceClose
            Main.Close()
        End If
    End Sub

#Region "Backup Groups Tab"
    Private Sub ReloadBackupGroups()
        Main.GroupsTabControl.Items.Clear()
        BackupGroupsListBox.Items.Clear()

        Main.GroupsTabControl.Items.Clear()
        Main.GroupsTabControl.Items.Add("All")

        For Each Group As String In My.Settings.BackupGroups
            BackupGroupsListBox.Items.Add(Group)
            Main.GroupsTabControl.Items.Add(Group)
        Next

        BackupGroupsListBox.SelectedIndex = 0
        Main.GroupsTabControl.SelectedIndex = 0
    End Sub

    Private Sub CreateNewGroupTextBox_TextChanged(sender As Object, e As TextChangedEventArgs) Handles CreateNewGroupTextBox.TextChanged
        If CreateNewGroupButton IsNot Nothing Then
            If CreateNewGroupTextBox.Text = "" Then
                CreateNewGroupButton.IsEnabled = False
            Else
                CreateNewGroupButton.IsEnabled = True
            End If
        End If
    End Sub

    Private Sub BackupGroupsListBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles BackupGroupsListBox.SelectionChanged
        If BackupGroupsListBox.SelectedItems.Count = 1 Then
            DeleteGroupButton.IsEnabled = True
        Else
            DeleteGroupButton.IsEnabled = False
        End If
    End Sub

    Private Sub CreateNewGroupButton_Click(sender As Object, e As RoutedEventArgs) Handles CreateNewGroupButton.Click
        My.Settings.BackupGroups.Add(CreateNewGroupTextBox.Text)
        CreateNewGroupTextBox.Text = ""
        ReloadBackupGroups()
    End Sub

    Private Sub DeleteGroupButton_Click(sender As Object, e As RoutedEventArgs) Handles DeleteGroupButton.Click
        If MetroMessageBox.Show("Are you sure you want to delete this group? It will be lost forever (a long time)!", MCBackup.Language.Dictionary("Message.Caption.AreYouSure"), MessageBoxButton.YesNo, MessageBoxImage.Question) = MessageBoxResult.Yes Then
            My.Settings.BackupGroups.RemoveAt(BackupGroupsListBox.SelectedIndex)
            ReloadBackupGroups()
        End If
    End Sub
#End Region
End Class

Public Class TaggedComboBoxItem
    Private m_Name As String
    Public Property Name() As String
        Get
            Return m_Name
        End Get
        Set(value As String)
            m_Name = value
        End Set
    End Property

    Private m_Tag As String
    Public Property Tag() As String
        Get
            Return m_Tag
        End Get
        Set(value As String)
            m_Tag = value
        End Set
    End Property

    Public Sub New(Name As String, Tag As String)
        Me.Name = Name
        Me.Tag = Tag
    End Sub
End Class