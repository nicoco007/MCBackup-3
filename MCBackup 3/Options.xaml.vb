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

Public Class Options
    Private Main As MainWindow = DirectCast(Application.Current.MainWindow, MainWindow)
    Private FolderBrowserDialog As New FolderBrowserDialog
    Private OpenFileDialog As New OpenFileDialog

    Sub New()
        InitializeComponent()
    End Sub

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        OpenFileDialog.Filter = "All Supported Image Files (*bmp, *.jpg, *.jpeg, *.png)|*bmp;*.gif;*.png;*.jpg;*.jpeg;*.png|BMP (*.bmp)|*.bmp|JPEG (*.jpg, *.jpeg)|*.jpg;*.jpeg;|PNG (*.png)|*.png"

        ListBox.SelectedIndex = 0
        MinecraftFolderTextBox.Text = My.Settings.MinecraftFolderLocation
        BackupsFolderTextBox.Text = My.Settings.BackupsFolderLocation
        SavesFolderTextBox.Text = My.Settings.SavesFolderLocation
        ListViewOpacitySlider.Value = My.Settings.OpacityPercent
        OpacityPercentLabel.Content = Int(ListViewOpacitySlider.Value).ToString & "%"
        BackgroundImageStyle.SelectedIndex = My.Settings.BackgroundImageStretch
        CheckForUpdatesCheckBox.IsChecked = My.Settings.CheckForUpdates
        ShowBalloonTipsCheckBox.IsChecked = My.Settings.ShowBalloonTips

        AlwaysCloseCheckBox.IsChecked = My.Settings.SaveCloseState
        CloseToTrayRadioButton.IsChecked = My.Settings.CloseToTray
        CloseCompletelyRadioButton.IsChecked = Not My.Settings.CloseToTray

        AlwaysCloseCheckBox_Checked(New Object, New RoutedEventArgs)
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
                MessageBox.Show("Minecraft folder set to " & FolderBrowserDialog.SelectedPath, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1) ' Tell user that folder has been selected successfully
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
                Log.Print("[SEVERE] " & ex.Message)
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
            Dim Value As Double = ListViewOpacitySlider.Value / 100
            Main.ListView.Opacity = Value
            Main.Sidebar.Opacity = Value
            OpacityPercentLabel.Content = Math.Round(ListViewOpacitySlider.Value, 0).ToString & "%"
        End If
    End Sub

    Private Sub BackgroundImageBrowseButton_Click(sender As Object, e As RoutedEventArgs) Handles BackgroundImageBrowseButton.Click
        If OpenFileDialog.ShowDialog = Forms.DialogResult.OK Then
            Dim Brush As New ImageBrush(New BitmapImage(New Uri(OpenFileDialog.FileName)))
            Brush.Stretch = My.Settings.BackgroundImageStretch
            Main.Background = Brush
            My.Settings.BackgroundImageLocation = OpenFileDialog.FileName
            MsgBox(My.Settings.BackgroundImageLocation)
        End If
    End Sub

    Private Sub BackgroundImageRemoveButton_Click(sender As Object, e As RoutedEventArgs) Handles BackgroundImageRemoveButton.Click
        Main.Background = New SolidColorBrush(Color.FromArgb(255, 240, 240, 240))
        My.Settings.BackgroundImageLocation = ""
    End Sub

    Private Sub BackgroundImageStyle_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles BackgroundImageStyle.SelectionChanged
        If Not Me.IsLoaded Or My.Settings.BackgroundImageLocation = "" Then
            Exit Sub
        End If

        Try
            Dim Brush As New ImageBrush(New BitmapImage(New Uri(My.Settings.BackgroundImageLocation)))
            Select Case BackgroundImageStyle.SelectedIndex
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
        Log.Print("[INFO] Saving settings...")
        My.Settings.MinecraftFolderLocation = MinecraftFolderTextBox.Text
        Log.Print("[INFO] Minecraft folder location set to " & My.Settings.MinecraftFolderLocation)
        My.Settings.SavesFolderLocation = SavesFolderTextBox.Text
        Log.Print("[INFO] Saves folder location set to " & My.Settings.SavesFolderLocation)
        My.Settings.BackupsFolderLocation = BackupsFolderTextBox.Text
        Log.Print("[INFO] Backups folder location set to " & My.Settings.BackupsFolderLocation)
        My.Settings.OpacityPercent = ListViewOpacitySlider.Value
        My.Settings.CheckForUpdates = CheckForUpdatesCheckBox.IsChecked
        My.Settings.ShowBalloonTips = ShowBalloonTipsCheckBox.IsChecked

        If AlwaysCloseCheckBox.IsChecked Then
            My.Settings.SaveCloseState = True
            My.Settings.CloseToTray = CloseToTrayRadioButton.IsChecked
        Else
            My.Settings.SaveCloseState = False
        End If

        My.Settings.Save()
        Main.RefreshBackupsList()
    End Sub
End Class
