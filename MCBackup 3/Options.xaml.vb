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

Imports System.Windows.Forms
Imports System.Linq
Imports System.Security.Permissions
Imports System.Security

Partial Public Class Options
    Private Main As MainWindow = DirectCast(Application.Current.MainWindow, MainWindow)
    Private FolderBrowserDialog As New FolderBrowserDialog
    Private OpenFileDialog As New OpenFileDialog

    Sub New()
        InitializeComponent()
        OpenFileDialog.Filter = "All Supported Image Files (*bmp, *.jpg, *.jpeg, *.png)|*bmp;*.gif;*.png;*.jpg;*.jpeg|BMP (*.bmp)|*.bmp|JPEG (*.jpg, *.jpeg)|*.jpg;*.jpeg|PNG (*.png)|*.png"

        ListBox.SelectedIndex = 0
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

        LoadLanguage()
    End Sub

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        Dim LanguageDirectory As New IO.DirectoryInfo(Main.StartupPath & "\language")
        Dim LanguageFiles As IO.FileInfo() = LanguageDirectory.GetFiles()
        Dim LanguageFile As IO.FileInfo

        For Each LanguageFile In LanguageFiles
            LanguagesComboBox.Items.Add(MCBackup.Language.FindString("fullname", LanguageFile.ToString))
        Next

        AlwaysCloseCheckBox_Checked(New Object, New RoutedEventArgs)

        LanguagesComboBox.SelectedItem = MCBackup.Language.FindString("fullname", My.Settings.Language & ".lang")
    End Sub

    Private Sub ListBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles ListBox.SelectionChanged
        Select Case ListBox.SelectedIndex
            Case 0
                GeneralPanel.Visibility = Windows.Visibility.Visible
                AppearancePanel.Visibility = Windows.Visibility.Hidden
                FoldersPanel.Visibility = Windows.Visibility.Hidden
            Case 1
                GeneralPanel.Visibility = Windows.Visibility.Hidden
                AppearancePanel.Visibility = Windows.Visibility.Visible
                FoldersPanel.Visibility = Windows.Visibility.Hidden
            Case 2
                GeneralPanel.Visibility = Windows.Visibility.Hidden
                AppearancePanel.Visibility = Windows.Visibility.Hidden
                FoldersPanel.Visibility = Windows.Visibility.Visible
        End Select
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
                If MessageBox.Show("Minecraft is not installed in that folder! Try again?", "Error!", MessageBoxButtons.YesNo, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1) = Windows.Forms.DialogResult.Yes Then ' Ask if user wants to try finding folder again
                    BrowseMinecraftFolderButton_Click(sender, e) ' Restart from beginning if "Yes"
                Else
                    Me.Close() ' Close program if "No"
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
                MessageBox.Show("Error: Unable to set backups folder to """ & FolderBrowserDialog.SelectedPath & """", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1)
            End Try
        End If
    End Sub

    Private Sub BrowseSavesFolderButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseSavesFolderButton.Click
        If FolderBrowserDialog.ShowDialog = Forms.DialogResult.OK Then
            Dim folderPath() As String = FolderBrowserDialog.SelectedPath.Split("\")
            Dim folder As String = folderPath.Last
            If Not folder = "saves" Then
                If MessageBox.Show("Are you sure this is a saves folder? It's name isn't even ""saves""!", "Are you sure?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) = Forms.DialogResult.Yes Then
                    SavesFolderTextBox.Text = FolderBrowserDialog.SelectedPath
                Else
                    Exit Sub
                End If
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
            Log.Print("[SEVERE] " & ex.Message)
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
        MCBackup.Language.Load(MCBackup.Language.GetIDFromName(LanguagesComboBox.SelectedItem) & ".lang")
        My.Settings.Language = MCBackup.Language.GetIDFromName(LanguagesComboBox.SelectedItem)
        LoadLanguage()
    End Sub

    Private Sub LoadLanguage()
        Me.Title = MCBackup.Language.Dictionnary("OptionsWindow.Title")
        ListBox.Items(0).Content = MCBackup.Language.Dictionnary("OptionsWindow.ListBox.Items(0).Content")
        ListBox.Items(1).Content = MCBackup.Language.Dictionnary("OptionsWindow.ListBox.Items(1).Content")
        ListBox.Items(2).Content = MCBackup.Language.Dictionnary("OptionsWindow.ListBox.Items(2).Content")

        ' General Tab
        ShowBalloonTipsCheckBox.Content = MCBackup.Language.Dictionnary("OptionsWindow.GeneralPanel.ShowBalloonTipsCheckBox.Content")
        ShowDeleteConfirmationCheckBox.Content = MCBackup.Language.Dictionnary("OptionsWindow.GeneralPanel.ShowDeleteConfirmationCheckBox.Content")
        CheckForUpdatesCheckBox.Content = MCBackup.Language.Dictionnary("OptionsWindow.GeneralPanel.CheckForUpdatesCheckBox.Content")
        CreateThumbOnWorldCheckBox.Content = MCBackup.Language.Dictionnary("OptionsWindow.GeneralPanel.CreateThumbOnWorldCheckBox.Content")
        AlwaysCloseCheckBox.Content = MCBackup.Language.Dictionnary("OptionsWindow.GeneralPanel.AlwaysCloseCheckBox.Content")
        CloseToTrayRadioButton.Content = MCBackup.Language.Dictionnary("OptionsWindow.GeneralPanel.CloseToTrayRadioButton.Content")
        CloseCompletelyRadioButton.Content = MCBackup.Language.Dictionnary("OptionsWindow.GeneralPanel.CloseCompletelyRadioButton.Content")
        AlwaysCloseNoteTextBlock.Text = MCBackup.Language.Dictionnary("OptionsWindow.GeneralPanel.AlwaysCloseNoteTextBlock.Text")
        LanguageLabel.Content = MCBackup.Language.Dictionnary("OptionsWindow.GeneralPanel.LanguageLabel.Content")

        ' Appearance 
        ListViewOpacityLabel.Content = MCBackup.Language.Dictionnary("OptionsWindow.AppearancePanel.ListViewOpacityLabel.Content")
        BackgroundImageLabel.Content = MCBackup.Language.Dictionnary("OptionsWindow.AppearancePanel.BackgroundImageLabel.Content")
        SizeModeLabel.Content = MCBackup.Language.Dictionnary("OptionsWindow.AppearancePanel.SizeModeLabel.Content")
        SizeModeComboBox.Items(0).Content = MCBackup.Language.Dictionnary("OptionsWindow.AppearancePanel.SizeModeComboBox.Items(0).Content")
        SizeModeComboBox.Items(1).Content = MCBackup.Language.Dictionnary("OptionsWindow.AppearancePanel.SizeModeComboBox.Items(1).Content")
        SizeModeComboBox.Items(2).Content = MCBackup.Language.Dictionnary("OptionsWindow.AppearancePanel.SizeModeComboBox.Items(2).Content")
        SizeModeComboBox.Items(3).Content = MCBackup.Language.Dictionnary("OptionsWindow.AppearancePanel.SizeModeComboBox.Items(3).Content")
        BackgroundImageBrowseButton.Content = MCBackup.Language.Dictionnary("OptionsWindow.AppearancePanel.BackgroundImageBrowseButton.Content")
        BackgroundImageRemoveButton.Content = MCBackup.Language.Dictionnary("OptionsWindow.AppearancePanel.BackgroundImageRemoveButton.Content")
        StatusLabelColor.Content = MCBackup.Language.Dictionnary("OptionsWindow.AppearancePanel.StatusLabelColor.Content")

        ' Folders
        MinecraftFolderLocationLabel.Content = MCBackup.Language.Dictionnary("OptionsWindow.FoldersPanel.MinecraftFolderLocationLabel.Content")
        SavesFolderLocationLabel.Content = MCBackup.Language.Dictionnary("OptionsWindow.FoldersPanel.SavesFolderLocationLabel.Content")
        BackupsFolderLocationLabel.Content = MCBackup.Language.Dictionnary("OptionsWindow.FoldersPanel.BackupsFolderLocationLabel.Content")
        BrowseMinecraftFolderButton.Content = MCBackup.Language.Dictionnary("OptionsWindow.FoldersPanel.BrowseButton.Content")
        BrowseSavesFolderButton.Content = MCBackup.Language.Dictionnary("OptionsWindow.FoldersPanel.BrowseButton.Content")
        BrowseBackupsFolderButton.Content = MCBackup.Language.Dictionnary("OptionsWindow.FoldersPanel.BrowseButton.Content")
    End Sub

    Private Sub ColorSlider_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles RedColorSlider.ValueChanged, GreenColorSlider.ValueChanged, BlueColorSlider.ValueChanged
        RedColorLabel.Content = CInt(RedColorSlider.Value)
        GreenColorLabel.Content = CInt(GreenColorSlider.Value)
        BlueColorLabel.Content = CInt(BlueColorSlider.Value)
        My.Settings.StatusLabelColor = Color.FromRgb(RedColorSlider.Value, GreenColorSlider.Value, BlueColorSlider.Value)
        ColorRectangle.Fill = New SolidColorBrush(My.Settings.StatusLabelColor)
        Main.StatusLabel.Foreground = New SolidColorBrush(My.Settings.StatusLabelColor)
    End Sub
End Class
