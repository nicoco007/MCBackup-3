'   ╔═══════════════════════════════════════════════════════════════════════════╗
'   ║                      Copyright © 2013-2016 nicoco007                      ║
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

Imports System.Windows.Interop
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Windows.Media.Animation
Imports System.ComponentModel
Imports GettextSharp

Public Class OptionsDialog
    Private MainWindow As MainWindow = DirectCast(Application.Current.MainWindow, MainWindow)
    Private AutoBackupWindow As AutoBackupWindow = Application.Current.Windows.OfType(Of AutoBackupWindow).FirstOrDefault()
    Private OpenFileDialog As New Forms.OpenFileDialog

    Public Sub Window_Loaded(sender As Object, e As RoutedEventArgs) Handles MyBase.Loaded
        InitializeComponent()
        OpenFileDialog.Filter = Application.Language.GetString("All supported image types") + " (*bmp, *.jpg, *.jpeg, *.png)|*bmp;*.gif;*.png;*.jpg;*.jpeg|BMP (*.bmp)|*.bmp|JPEG (*.jpg, *.jpeg)|*.jpg;*.jpeg|PNG (*.png)|*.png"

        ListViewOpacitySlider.Value = My.Settings.InterfaceOpacity
        OpacityPercentLabel.Content = Int(ListViewOpacitySlider.Value).ToString & "%"
        SizeModeComboBox.SelectedIndex = My.Settings.BackgroundImageStretch
        CheckForUpdatesCheckBox.IsChecked = My.Settings.CheckForUpdates
        ShowBalloonTipsCheckBox.IsChecked = My.Settings.ShowBalloonTips
        CreateThumbOnWorldCheckBox.IsChecked = My.Settings.CreateThumbOnWorld
        ShowNewsCheckBox.IsChecked = My.Settings.ShowNewsOnStartup

        AlwaysCloseCheckBox.IsChecked = My.Settings.SaveCloseState
        CloseToTrayRadioButton.IsChecked = My.Settings.CloseToTray
        CloseCompletelyRadioButton.IsChecked = Not My.Settings.CloseToTray

        Dim StatusLabelColor = My.Settings.StatusLabelColor
        RedColorSlider.Value = StatusLabelColor.R
        GreenColorSlider.Value = StatusLabelColor.G
        BlueColorSlider.Value = StatusLabelColor.B

        UpdateColorSliderValues()

        RedColorLabel.ContextMenu = Nothing
        GreenColorLabel.ContextMenu = Nothing
        BlueColorLabel.ContextMenu = Nothing

        SendAnonymousDataCheckBox.IsChecked = My.Settings.SendAnonymousData
        ShowDeleteConfirmationCheckBox.IsChecked = My.Settings.ShowDeleteDialog

        Select Case My.Settings.Launcher
            Case Launcher.Minecraft
                MinecraftInstallationRadioButton.IsChecked = True
            Case Launcher.Technic
                TechnicInstallationRadioButton.IsChecked = True
            Case Launcher.FeedTheBeast
                FTBInstallationRadioButton.IsChecked = True
            Case Launcher.ATLauncher
                ATLauncherInstallationRadioButton.IsChecked = True
        End Select

        BaseFolderTextBox.Text = My.Settings.MinecraftFolderLocation
        SavesFolderTextBox.Text = My.Settings.SavesFolderLocation
        BackupsFolderTextBox.Text = My.Settings.BackupsFolderLocation

        If My.Settings.Launcher = Launcher.Minecraft Then
            SavesFolderBrowseButton.IsEnabled = True
            SavesFolderTextBox.IsEnabled = True
            SavesFolderTextBox.Text = My.Settings.SavesFolderLocation
        Else
            SavesFolderBrowseButton.IsEnabled = False
            SavesFolderTextBox.IsEnabled = False
            SavesFolderTextBox.Text = ""
        End If

        LanguagesComboBox.Items.Add(New TaggedComboBoxItem("English (United States)", "en_US"))

        For Each LanguageFile As String In Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "language"), "*.mo")
            LanguagesComboBox.Items.Add(New TaggedComboBoxItem(New LanguageInfo(LanguageFile).LanguageTitle, Path.GetFileNameWithoutExtension(LanguageFile)))
        Next

        LanguagesComboBox.SelectedItem = LanguagesComboBox.Items.OfType(Of TaggedComboBoxItem)().FirstOrDefault(Function(Item) Item.Tag = My.Settings.Language)

        AlwaysCloseCheckBox_Checked(Nothing, Nothing)

        LoadLanguage()

        ListViewTextColorIntensitySlider.Value = My.Settings.ListViewTextColorIntensity

        DefaultBackupNameTextBox.Text = My.Settings.DefaultBackupName
        DefaultAutoBackupNameTextBox.Text = My.Settings.DefaultAutoBackupName
        DefaultBackupNameTextBox_TextChanged(Nothing, Nothing)
        DefaultAutoBackupNameTextBox_TextChanged(Nothing, Nothing)

        ReloadBackupGroups()
    End Sub

    Public Overloads Sub ShowDialog(Tab As Integer)
        If Not Tab > TabControl.Items.Count - 1 Then
            TabControl.SelectedIndex = Tab
        End If
        MyBase.ShowDialog()
    End Sub

    Private Sub AlwaysCloseCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles AlwaysCloseCheckBox.Click
        CloseToTrayRadioButton.IsEnabled = AlwaysCloseCheckBox.IsChecked
        CloseCompletelyRadioButton.IsEnabled = AlwaysCloseCheckBox.IsChecked
    End Sub

    Private Sub ReloadBackupGroups()
        BackupGroupsListBox.Items.Clear()

        For Each Group As String In My.Settings.BackupGroups
            BackupGroupsListBox.Items.Add(Group)
        Next

        MainWindow.ReloadBackupGroups()
    End Sub

    Private Sub ListViewOpacitySlider_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        If Me.IsLoaded Then
            Dim DefaultBackground As SolidColorBrush = DirectCast(FindResource("ControlBackgroundBrush"), SolidColorBrush)
            Dim InterfaceOpacityBackground As New SolidColorBrush(Color.FromArgb(ListViewOpacitySlider.Value * 2.55, DefaultBackground.Color.R, DefaultBackground.Color.G, DefaultBackground.Color.B))

            MainWindow.ListView.Background = InterfaceOpacityBackground
            MainWindow.Sidebar.Background = InterfaceOpacityBackground
            AutoBackupWindow.MinutesNumUpDown.Background = InterfaceOpacityBackground
            AutoBackupWindow.SavesListView.Background = InterfaceOpacityBackground

            OpacityPercentLabel.Content = Math.Round(ListViewOpacitySlider.Value, 0).ToString & "%"
        End If
    End Sub

    Private Sub BackgroundImageBrowseButton_Click(sender As Object, e As RoutedEventArgs) Handles BackgroundImageBrowseButton.Click
        If OpenFileDialog.ShowDialog() = Forms.DialogResult.OK Then
            My.Settings.BackgroundImageLocation = OpenFileDialog.FileName
            MainWindow.BackgroundImageBitmap = New BitmapImage(New Uri(OpenFileDialog.FileName))
            MainWindow.AdjustBackground()
        End If
    End Sub

    Private Sub BackgroundImageRemoveButton_Click(sender As Object, e As RoutedEventArgs) Handles BackgroundImageRemoveButton.Click
        MainWindow.ClearValue(BackgroundProperty)
        AutoBackupWindow.ClearValue(BackgroundProperty)
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
            My.Settings.BackgroundImageStretch = Brush.Stretch
            MainWindow.AdjustBackground()
        Catch ex As Exception
            Log.Severe(ex.Message)
        End Try
    End Sub

    Private Sub CloseButton_Click(sender As Object, e As RoutedEventArgs) Handles CloseButton.Click
        Me.Close()
    End Sub

    Private Async Sub Window_Closing(sender As Object, e As CancelEventArgs) Handles MyBase.Closing
        Dim DefaultBackupName As String = BackupName.Process(DefaultBackupNameTextBox.Text, Application.Language.GetString("[Folder Name]"))
        Dim DefaultAutoBackupName As String = BackupName.Process(DefaultAutoBackupNameTextBox.Text, Application.Language.GetString("[Folder Name]"))

        If Regex.IsMatch(DefaultBackupName, "[\/:*?""<>|]") Or Regex.IsMatch(DefaultAutoBackupName, "[\/:*?""<>|]") Then
            MetroMessageBox.Show(Application.Language.GetString("A backup name cannot contain the following characters:\n\ / : * ? "" < > |\nPlease check your settings and try again."), Application.Language.GetString("Error"), MessageBoxButton.OK, MessageBoxImage.Error, TextAlignment.Center)
            e.Cancel = True
            Exit Sub
        End If

        Log.Info("Minecraft folder location set to " & My.Settings.MinecraftFolderLocation)
        Log.Info("Saves folder location set to " & My.Settings.SavesFolderLocation)
        Log.Info("Backups folder location set to " & My.Settings.BackupsFolderLocation)

        My.Settings.InterfaceOpacity = ListViewOpacitySlider.Value
        My.Settings.CheckForUpdates = CheckForUpdatesCheckBox.IsChecked
        My.Settings.ShowBalloonTips = ShowBalloonTipsCheckBox.IsChecked
        My.Settings.CreateThumbOnWorld = CreateThumbOnWorldCheckBox.IsChecked
        My.Settings.ShowNewsOnStartup = ShowNewsCheckBox.IsChecked

        My.Settings.DefaultBackupName = DefaultBackupNameTextBox.Text
        My.Settings.DefaultAutoBackupName = DefaultAutoBackupNameTextBox.Text

        If AlwaysCloseCheckBox.IsChecked Then
            My.Settings.SaveCloseState = True
            My.Settings.CloseToTray = CloseToTrayRadioButton.IsChecked
        Else
            My.Settings.SaveCloseState = False
        End If

        My.Settings.Save()
        Await MainWindow.RefreshBackupsList()
        ReloadBackupGroups()
    End Sub

    Private Sub LanguagesComboBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles LanguagesComboBox.SelectionChanged
        If Me.IsLoaded Then
            Dim moFile As String = Path.Combine(Directory.GetCurrentDirectory(), "language", LanguagesComboBox.SelectedItem.Tag + ".mo")
            Try
                If File.Exists(moFile) Then
                    Application.Language.Load(moFile)
                Else
                    Application.Language.Clear()
                End If
            Catch ex As Exception
                ErrorReportDialog.ShowDialog(Application.Language.GetString("Failed to load language file"), ex)
            End Try
            My.Settings.Language = LanguagesComboBox.SelectedItem.Tag
            LoadLanguage()
            DefaultBackupNameTextBox.Text = Application.Language.GetString("%worldname% - %timestamp:yyyy-MM-dd (hh\hmm\mss\s tt)%")
            DefaultAutoBackupNameTextBox.Text = Application.Language.GetString("[AUTO] %worldname% - %timestamp:yyyy-MM-dd (hh\hmm\mss\s tt)%")
            MainWindow.LoadLanguage()
            ReloadBackupGroups()
            AutoBackupWindow.LoadLanguage()
        End If
    End Sub

    Private Sub LoadLanguage()
        Me.Title = Application.Language.GetString("Preferences")

        CloseButton.Content = Application.Language.GetString("Close")
        ResetButton.Content = Application.Language.GetString("Reset")

        GeneralTabItem.Header = Application.Language.GetString("General")
        AppearanceTabItem.Header = Application.Language.GetString("Appearance")
        FoldersTabItem.Header = Application.Language.GetString("Folders")
        GroupsTabItem.Header = Application.Language.GetString("Groups")
        AdvancedTabItem.Header = Application.Language.GetString("Advanced")

        ' General Tab
        GeneralOptionsGroupBox.Header = Application.Language.GetString("General Options")
        CloseToTrayOptionsGroupBox.Header = Application.Language.GetString("Close to Tray Options")
        LanguageGroupBox.Header = Application.Language.GetString("Language")

        ShowBalloonTipsCheckBox.Content = Application.Language.GetString("Show balloon tips")
        ShowDeleteConfirmationCheckBox.Content = Application.Language.GetString("Show delete confirmation dialog")
        CheckForUpdatesCheckBox.Content = Application.Language.GetString("Check for updates on startup")
        CreateThumbOnWorldCheckBox.Content = Application.Language.GetString("Create thumbnail when backing up a world")
        AlwaysCloseCheckBox.Content = Application.Language.GetString("Always...")
        CloseToTrayRadioButton.Content = Application.Language.GetString("Close to tray")
        CloseCompletelyRadioButton.Content = Application.Language.GetString("Close completely")
        SendAnonymousDataCheckBox.Content = Application.Language.GetString("Send anonymouse statistics")
        ShowNewsCheckBox.Content = Application.Language.GetString("Show news window on startup")

        ' Appearance 
        GeneralAppearanceGroupBox.Header = Application.Language.GetString("General Options")
        StatusTextColorGroupBox.Header = Application.Language.GetString("Text Color")
        ListViewTextColorIntensityGroupBox.Header = Application.Language.GetString("Backups List Text Color Intensity")

        ListViewOpacityLabel.Content = Application.Language.GetString("User Interface Opacity")
        BackgroundImageLabel.Content = Application.Language.GetString("Background Image")
        SizeModeLabel.Content = Application.Language.GetString("Size Mode")
        SizeModeComboBox.Items(0).Content = Application.Language.GetString("Center")
        SizeModeComboBox.Items(1).Content = Application.Language.GetString("Stretch")
        SizeModeComboBox.Items(2).Content = Application.Language.GetString("Auto Size")
        SizeModeComboBox.Items(3).Content = Application.Language.GetString("Zoom")
        BackgroundImageBrowseButton.Content = Application.Language.GetString("Browse...")
        BackgroundImageRemoveButton.Content = Application.Language.GetString("Remove")
        ThemeLabel.Content = Application.Language.GetString("Theme Color")
        SampleTextG1.Content = Application.Language.GetString("Sample Text")
        SampleTextY1.Content = Application.Language.GetString("Sample Text")
        SampleTextR1.Content = Application.Language.GetString("Sample Text")

        YAlignComboBox.Items.Clear()
        YAlignComboBox.Items.Add(Application.Language.GetString("Top"))
        YAlignComboBox.Items.Add(Application.Language.GetString("Center"))
        YAlignComboBox.Items.Add(Application.Language.GetString("Bottom"))
        YAlignComboBox.SelectedIndex = My.Settings.BackgroundImageYAlign

        ' Theme colors
        Dim names As TaggedComboBoxItem() =
        {
            New TaggedComboBoxItem(Application.Language.GetString("Amber"), "Amber"),
            New TaggedComboBoxItem(Application.Language.GetString("Blue"), "Blue"),
            New TaggedComboBoxItem(Application.Language.GetString("Brown"), "Brown"),
            New TaggedComboBoxItem(Application.Language.GetString("Cobalt"), "Cobalt"),
            New TaggedComboBoxItem(Application.Language.GetString("Crimson"), "Crimson"),
            New TaggedComboBoxItem(Application.Language.GetString("Cyan"), "Cyan"),
            New TaggedComboBoxItem(Application.Language.GetString("Emerald"), "Emerald"),
            New TaggedComboBoxItem(Application.Language.GetString("Green"), "Green"),
            New TaggedComboBoxItem(Application.Language.GetString("Indigo"), "Indigo"),
            New TaggedComboBoxItem(Application.Language.GetString("Lime"), "Lime"),
            New TaggedComboBoxItem(Application.Language.GetString("Magenta"), "Magenta"),
            New TaggedComboBoxItem(Application.Language.GetString("Mauve"), "Mauve"),
            New TaggedComboBoxItem(Application.Language.GetString("Olive"), "Olive"),
            New TaggedComboBoxItem(Application.Language.GetString("Orange"), "Orange"),
            New TaggedComboBoxItem(Application.Language.GetString("Pink"), "Pink"),
            New TaggedComboBoxItem(Application.Language.GetString("Purple"), "Purple"),
            New TaggedComboBoxItem(Application.Language.GetString("Red"), "Red"),
            New TaggedComboBoxItem(Application.Language.GetString("Sienna"), "Sienna"),
            New TaggedComboBoxItem(Application.Language.GetString("Steel"), "Steel"),
            New TaggedComboBoxItem(Application.Language.GetString("Teal"), "Teal"),
            New TaggedComboBoxItem(Application.Language.GetString("Violet"), "Violet"),
            New TaggedComboBoxItem(Application.Language.GetString("Yellow"), "Yellow")
        }

        ' Sort names by translation
        Array.Sort(names, Function(a As TaggedComboBoxItem, b As TaggedComboBoxItem) a.Name.CompareTo(b.Name))

        ' Set source & selected item
        ThemeComboBox.ItemsSource = names
        ThemeComboBox.SelectedItem = ThemeComboBox.Items.OfType(Of TaggedComboBoxItem).FirstOrDefault(Function(item) item.Tag = My.Settings.Theme)

        ' Theme shades
        ThemeShadeComboBox.Items.Clear()
        ThemeShadeComboBox.Items.Add(New TaggedComboBoxItem(Application.Language.GetString("Light"), "BaseLight"))
        ThemeShadeComboBox.Items.Add(New TaggedComboBoxItem(Application.Language.GetString("Dark"), "BaseDark"))
        ThemeShadeComboBox.SelectedItem = ThemeShadeComboBox.Items.OfType(Of TaggedComboBoxItem).FirstOrDefault(Function(Item) Item.Tag = My.Settings.ThemeShade)

        ' Folders
        InstallTypeGroupBox.Header = Application.Language.GetString("Installation Type")
        MinecraftInstallationRadioButton.Content = Application.Language.GetString("Minecraft")
        TechnicInstallationRadioButton.Content = Application.Language.GetString("Technic")
        FTBInstallationRadioButton.Content = Application.Language.GetString("Feed the Beast")
        ATLauncherInstallationRadioButton.Content = Application.Language.GetString("ATLauncher")
        BaseFolderLabel.Content = Application.Language.GetString("Base Folder")
        BaseFolderBrowseButton.Content = Application.Language.GetString("Browse...")
        GeneralFoldersGroupBox.Header = Application.Language.GetString("General Folders")
        SavesFolderLabel.Content = Application.Language.GetString("Saves Folder")
        SavesFolderBrowseButton.Content = Application.Language.GetString("Browse...")
        BackupsFolderLabel.Content = Application.Language.GetString("Backups Folder")
        BackupsFolderBrowseButton.Content = Application.Language.GetString("Browse...")

        ' Groups
        AddNewGroupGroupBox.Header = Application.Language.GetString("Add New Group")
        OtherOptionsGroupBox.Header = Application.Language.GetString("Other Options")
        DeleteGroupButton.Content = Application.Language.GetString("Delete")
        RenameGroupButton.Content = Application.Language.GetString("Rename")
        MoveGroupUpButton.Content = New Viewbox() With {.Child = New ContentControl() With {.Content = Application.Language.GetString("Move up")}, .Stretch = Stretch.Uniform, .StretchDirection = StretchDirection.DownOnly}
        MoveGroupDownButton.Content = New Viewbox() With {.Child = New ContentControl() With {.Content = Application.Language.GetString("Move down")}, .Stretch = Stretch.Uniform, .StretchDirection = StretchDirection.DownOnly}

        ' Advanced
        DefaultBackupNameLabel.Content = Application.Language.GetString("Default Backup Name")
        DefaultAutoBackupNameLabel.Content = Application.Language.GetString("Default Automatic Backup Name")
        IgnoreSystemLocalizationCheckBox.Content = Application.Language.GetString("Ignore system language settings when formatting backup names")
        PlaceholdersLink.Text = Application.Language.GetString("Placeholders")
    End Sub

    Private Sub ColorSlider_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles RedColorSlider.ValueChanged, GreenColorSlider.ValueChanged, BlueColorSlider.ValueChanged
        UpdateColorSliderValues()
    End Sub

    Private Sub UpdateColorSliderValues()
        RedColorLabel.Text = CInt(RedColorSlider.Value)
        GreenColorLabel.Text = CInt(GreenColorSlider.Value)
        BlueColorLabel.Text = CInt(BlueColorSlider.Value)
        My.Settings.StatusLabelColor = Color.FromRgb(RedColorSlider.Value, GreenColorSlider.Value, BlueColorSlider.Value)
        ColorRectangle.Fill = New SolidColorBrush(My.Settings.StatusLabelColor)
        MainWindow.StatusLabel.Foreground = New SolidColorBrush(My.Settings.StatusLabelColor)

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
        If Not ThemeComboBox.SelectedItem Is Nothing And IsLoaded Then
            My.Settings.Theme = DirectCast(ThemeComboBox.SelectedItem, TaggedComboBoxItem).Tag
            MainWindow.UpdateTheme()
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
            Media.SystemSounds.Asterisk.Play()
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
            Media.SystemSounds.Asterisk.Play()
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
        If MetroMessageBox.Show(Application.Language.GetString("Are you sure you want to reset all of your settings?\n\nClick 'Yes' to reset all your settings and restart MCBackup."), Application.Language.GetString("Are you sure?"), MessageBoxButton.YesNo, MessageBoxImage.Question) = MessageBoxResult.Yes Then
            My.Settings.Reset()

            Process.Start(Application.ResourceAssembly.Location)
            Application.CloseAction = Application.AppCloseAction.Force
            MainWindow.Close()
        End If
    End Sub

#Region "Backup Groups Tab"
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
            If BackupGroupsListBox.SelectedIndex = 0 Then
                MoveGroupUpButton.IsEnabled = False
            Else
                MoveGroupUpButton.IsEnabled = True
            End If
            If BackupGroupsListBox.SelectedIndex = BackupGroupsListBox.Items.Count - 1 Then
                MoveGroupDownButton.IsEnabled = False
            Else
                MoveGroupDownButton.IsEnabled = True
            End If
            RenameGroupButton.IsEnabled = True
        Else
            DeleteGroupButton.IsEnabled = False
            MoveGroupUpButton.IsEnabled = False
            MoveGroupDownButton.IsEnabled = False
            RenameGroupButton.IsEnabled = False
        End If
    End Sub

    Private Sub CreateNewGroupButton_Click(sender As Object, e As RoutedEventArgs) Handles CreateNewGroupButton.Click
        My.Settings.BackupGroups.Add(CreateNewGroupTextBox.Text)
        CreateNewGroupTextBox.Text = ""
        ReloadBackupGroups()
    End Sub

    Private Sub DeleteGroupButton_Click(sender As Object, e As RoutedEventArgs) Handles DeleteGroupButton.Click
        If MetroMessageBox.Show(Application.Language.GetString("Are you sure you want to delete this group? It will be lost forever (a long time)!"), Application.Language.GetString("Are you sure?"), MessageBoxButton.YesNo, MessageBoxImage.Question) = MessageBoxResult.Yes Then
            My.Settings.BackupGroups.RemoveAt(BackupGroupsListBox.SelectedIndex)
            ReloadBackupGroups()
        End If
    End Sub
#End Region

    Private Sub SendAnonymousDataCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles SendAnonymousDataCheckBox.Click
        If Not SendAnonymousDataCheckBox.IsChecked Then
            If MetroMessageBox.Show(Application.Language.GetString("Are you sure you want to disable anonymous statistics collection? They are only used for global statistics!"), Application.Language.GetString("Are you sure?"), MessageBoxButton.YesNo, MessageBoxImage.Question) = MessageBoxResult.No Then
                SendAnonymousDataCheckBox.IsChecked = True
            End If
        End If
        My.Settings.SendAnonymousData = SendAnonymousDataCheckBox.IsChecked
    End Sub

    Private Sub InstallationType_SelectionChanged(sender As Object, e As RoutedEventArgs)
        If Not Me.IsLoaded Then Exit Sub

        If Not GetInstallationTypeButtons() = My.Settings.Launcher Then
            Dim FSD As New FolderSelectDialog
            If FSD.ShowDialog(New WindowInteropHelper(Me).Handle) = Forms.DialogResult.OK Then
                My.Settings.Launcher = GetInstallationTypeButtons()
                My.Settings.MinecraftFolderLocation = FSD.FolderName
                If My.Settings.Launcher = Launcher.Minecraft Then
                    Directory.CreateDirectory(My.Settings.MinecraftFolderLocation & "\saves")
                    My.Settings.SavesFolderLocation = My.Settings.MinecraftFolderLocation & "\saves"
                End If
                BaseFolderTextBox.Text = My.Settings.MinecraftFolderLocation
                SavesFolderTextBox.Text = My.Settings.SavesFolderLocation
            Else
                SetInstallationTypeButtons(My.Settings.Launcher)
            End If

            If My.Settings.Launcher = Launcher.Minecraft Then
                SavesFolderBrowseButton.IsEnabled = True
                SavesFolderTextBox.IsEnabled = True
                SavesFolderTextBox.Text = My.Settings.SavesFolderLocation
            Else
                SavesFolderBrowseButton.IsEnabled = False
                SavesFolderTextBox.IsEnabled = False
                SavesFolderTextBox.Text = ""
            End If

            AutoBackupWindow.ReloadSaves()
        End If
    End Sub

    Private Sub SetInstallationTypeButtons(Type As Launcher)
        Select Case Type
            Case Launcher.Minecraft
                MinecraftInstallationRadioButton.IsChecked = True
            Case Launcher.Technic
                TechnicInstallationRadioButton.IsChecked = True
            Case Launcher.FeedTheBeast
                FTBInstallationRadioButton.IsChecked = True
            Case Launcher.ATLauncher
                ATLauncherInstallationRadioButton.IsChecked = True
        End Select
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
        Return Launcher.Minecraft
    End Function

    Private Sub BaseFolderBrowseButton_Click(sender As Object, e As RoutedEventArgs) Handles BaseFolderBrowseButton.Click
        Dim FSD As New FolderSelectDialog
        If FSD.ShowDialog(New WindowInteropHelper(Me).Handle) = Forms.DialogResult.OK Then
            My.Settings.Launcher = GetInstallationTypeButtons()
            My.Settings.MinecraftFolderLocation = FSD.FolderName
            If My.Settings.Launcher = Launcher.Minecraft Then
                Directory.CreateDirectory(My.Settings.MinecraftFolderLocation & "\saves")
                My.Settings.SavesFolderLocation = My.Settings.MinecraftFolderLocation & "\saves"
            End If
            BaseFolderTextBox.Text = My.Settings.MinecraftFolderLocation

            If My.Settings.Launcher = Launcher.Minecraft Then
                SavesFolderBrowseButton.IsEnabled = True
                SavesFolderTextBox.IsEnabled = True
                SavesFolderTextBox.Text = My.Settings.SavesFolderLocation
            Else
                SavesFolderBrowseButton.IsEnabled = False
                SavesFolderTextBox.IsEnabled = False
                SavesFolderTextBox.Text = ""
            End If

            AutoBackupWindow.ReloadSaves()
        End If
    End Sub

    Private Sub SavesFolderBrowseButton_Click(sender As Object, e As RoutedEventArgs) Handles SavesFolderBrowseButton.Click
        Dim FSD As New FolderSelectDialog
        FSD.InitialDirectory = My.Settings.BackupsFolderLocation
        If FSD.ShowDialog(New WindowInteropHelper(Me).Handle) = Forms.DialogResult.OK Then
            If New DirectoryInfo(FSD.FolderName).Name = "saves" Then
                My.Settings.SavesFolderLocation = FSD.FolderName
                SavesFolderTextBox.Text = My.Settings.SavesFolderLocation
            Else
                If MetroMessageBox.Show(Application.Language.GetString("That folder is not named 'saves'! Please select another folder."), Application.Language.GetString("Error"), MessageBoxButton.OKCancel, MessageBoxImage.Error) = MessageBoxResult.OK Then
                    SavesFolderBrowseButton_Click(sender, e)
                End If
            End If
        End If
    End Sub

    Private Sub BackupsFolderBrowseButton_Click(sender As Object, e As RoutedEventArgs) Handles BackupsFolderBrowseButton.Click
        Dim FSD As New FolderSelectDialog
        FSD.InitialDirectory = My.Settings.BackupsFolderLocation
        If FSD.ShowDialog(New WindowInteropHelper(Me).Handle) = Forms.DialogResult.OK Then
            Try
                IO.File.Create(FSD.FolderName & "\tmp").Dispose()
                IO.File.Delete(FSD.FolderName & "\tmp")
                My.Settings.BackupsFolderLocation = FSD.FolderName
                BackupsFolderTextBox.Text = My.Settings.BackupsFolderLocation
            Catch ex As Exception
                Log.Severe("Could not set backups folder: " & ex.Message)
                MetroMessageBox.Show(Application.Language.GetString("This folder cannot be set as your backups folder. Please check if you have read and write permissions in this folder."), Application.Language.GetString("Error"), MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End If
    End Sub

    Private Sub MoveGroupUpButton_Click(sender As Object, e As RoutedEventArgs) Handles MoveGroupUpButton.Click
        Dim SelectedIndex = BackupGroupsListBox.SelectedIndex

        Dim temp = My.Settings.BackupGroups(SelectedIndex)
        My.Settings.BackupGroups(SelectedIndex) = My.Settings.BackupGroups(SelectedIndex - 1)
        My.Settings.BackupGroups(SelectedIndex - 1) = temp

        ReloadBackupGroups()

        BackupGroupsListBox.SelectedIndex = SelectedIndex - 1
    End Sub

    Private Sub MoveGroupDownButton_Click(sender As Object, e As RoutedEventArgs) Handles MoveGroupDownButton.Click
        Dim SelectedIndex = BackupGroupsListBox.SelectedIndex

        Dim temp = My.Settings.BackupGroups(SelectedIndex)
        My.Settings.BackupGroups(SelectedIndex) = My.Settings.BackupGroups(SelectedIndex + 1)
        My.Settings.BackupGroups(SelectedIndex + 1) = temp

        ReloadBackupGroups()

        BackupGroupsListBox.SelectedIndex = SelectedIndex + 1
    End Sub

    Private Sub RenameGroupButton_Click(sender As Object, e As RoutedEventArgs) Handles RenameGroupButton.Click
        Dim RenameBackupGroupDialog As New RenameBackupGroupDialog(BackupGroupsListBox.SelectedItem)
        RenameBackupGroupDialog.Owner = Me
        My.Settings.BackupGroups(BackupGroupsListBox.SelectedIndex) = RenameBackupGroupDialog.ShowDialog()
        ReloadBackupGroups()
    End Sub

    Private Sub ShowDeleteConfirmationCheckBox_Click(sender As Object, e As RoutedEventArgs) Handles ShowDeleteConfirmationCheckBox.Click
        My.Settings.ShowDeleteDialog = ShowDeleteConfirmationCheckBox.IsChecked
    End Sub

    Private Sub Grid_Loaded(sender As Object, e As RoutedEventArgs)
        Dim Width As Integer = 414
        If 514 - BaseFolderLabel.ActualWidth < Width Then
            Width = 514 - BaseFolderLabel.ActualWidth
        End If
        If 514 - SavesFolderLabel.ActualWidth < Width Then
            Width = 514 - SavesFolderLabel.ActualWidth
        End If
        If 514 - BackupsFolderLabel.ActualWidth < Width Then
            Width = 514 - BackupsFolderLabel.ActualWidth
        End If

        BaseFolderTextBox.Width = Width
        SavesFolderTextBox.Width = Width
        BackupsFolderTextBox.Width = Width
    End Sub

    Private Sub TextBlock_MouseDown(sender As Object, e As MouseButtonEventArgs)
        TryCast(sender, TextBlock).Foreground = New SolidColorBrush(Color.FromArgb(255, 255, 0, 0))
    End Sub

    Private Sub TextBlock_MouseUp(sender As Object, e As MouseButtonEventArgs)
        TryCast(sender, TextBlock).Foreground = New SolidColorBrush(Color.FromArgb(255, 0, 0, 255))
        If MetroMessageBox.Show(Application.Language.GetString("This action will open a webpage in your default web browser."), Application.Language.GetString("Are you sure?"), MessageBoxButton.OKCancel, MessageBoxImage.Information) = MessageBoxResult.OK Then
            Process.Start("http://go.nicoco007.com/fwlink/?LinkID=1002&utm_source=mcbackup&utm_medium=mcbackup")
        End If
    End Sub

    Private Sub TextBlock_MouseLeave(sender As Object, e As MouseEventArgs)
        TryCast(sender, TextBlock).Foreground = New SolidColorBrush(Color.FromArgb(255, 0, 0, 255))
    End Sub

    Private Sub DefaultBackupNameTextBox_TextChanged(sender As Object, e As TextChangedEventArgs) Handles DefaultBackupNameTextBox.TextChanged
        If Me.IsLoaded Then
            Dim Name As String = BackupName.Process(DefaultBackupNameTextBox.Text, Application.Language.GetString("[Folder Name]"))

            If Regex.IsMatch(Name, "[\/:*?""<>|]") Then
                DefaultBackupNameTextBox.Background = New SolidColorBrush(Color.FromArgb(100, 255, 0, 0))
                Exit Sub
            Else
                DefaultBackupNameTextBox.Background = New SolidColorBrush(Colors.White)
            End If

            BackupNameOutputLabel.Text = Application.Language.GetString("Will back up as: ") & Name
        End If
    End Sub

    Private Sub DefaultAutoBackupNameTextBox_TextChanged(sender As Object, e As TextChangedEventArgs) Handles DefaultAutoBackupNameTextBox.TextChanged
        If Me.IsLoaded Then
            Dim Name As String = BackupName.Process(DefaultAutoBackupNameTextBox.Text, Application.Language.GetString("[Folder Name]"))

            If Regex.IsMatch(Name, "[\/:*?""<>|]") Then
                DefaultAutoBackupNameTextBox.Background = New SolidColorBrush(Color.FromArgb(100, 255, 0, 0))
                Exit Sub
            Else
                DefaultAutoBackupNameTextBox.Background = New SolidColorBrush(Colors.White)
            End If

            AutoBackupNameOutputLabel.Text = Application.Language.GetString("Will back up as: ") & Name
        End If
    End Sub

    Private Sub IgnoreSystemLocalizationCheckBox_Click(sender As Object, e As RoutedEventArgs) Handles IgnoreSystemLocalizationCheckBox.Click
        My.Settings.IgnoreSystemLocalizationWhenFormatting = IgnoreSystemLocalizationCheckBox.IsChecked
        DefaultBackupNameTextBox_TextChanged(sender, Nothing)
        DefaultAutoBackupNameTextBox_TextChanged(sender, Nothing)
    End Sub

    Private Sub TabControl_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles TabControl.SelectionChanged
        If Not Me.IsLoaded() Then
            Me.Height = 120 + GeneralGrid.Height
        End If

        Dim Animation As New DoubleAnimation
        Animation.From = Me.Height
        Select Case TabControl.SelectedIndex
            Case 0
                Animation.To = 120 + GeneralGrid.Height
            Case 1
                Animation.To = 120 + AppearanceGrid.Height
            Case 2
                Animation.To = 120 + FoldersGrid.Height
            Case 3
                Animation.To = 120 + GroupsGrid.Height
            Case 4
                Animation.To = 120 + AdvancedGrid.Height
            Case Else
                Animation.To = 120 + GeneralGrid.Height
        End Select

        Animation.Duration = New Duration(TimeSpan.FromMilliseconds(250))

        Me.BeginAnimation(FrameworkElement.HeightProperty, Animation)
    End Sub

    Private Sub ThemeShadeComboBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles ThemeShadeComboBox.SelectionChanged
        If Not ThemeShadeComboBox.SelectedItem Is Nothing And IsLoaded Then
            My.Settings.ThemeShade = DirectCast(ThemeShadeComboBox.SelectedItem, TaggedComboBoxItem).Tag
            MainWindow.UpdateTheme()
        End If
    End Sub

    Private Sub YAlignComboBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles YAlignComboBox.SelectionChanged
        If Me.IsLoaded And YAlignComboBox.SelectedIndex > -1 Then
            My.Settings.BackgroundImageYAlign = YAlignComboBox.SelectedIndex
            MainWindow.AdjustBackground()
        End If
    End Sub
End Class