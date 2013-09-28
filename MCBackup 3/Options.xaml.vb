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

Public Class Options
    Private Main As MainWindow = DirectCast(Application.Current.MainWindow, MainWindow)
    Private FolderBrowserDialog As New FolderBrowserDialog
    Private OpenFileDialog As New OpenFileDialog


    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        OpenFileDialog.Filter = "All Supported Image Files (*bmp, *.gif, *.jpg, *.jpeg, *.jpe, *.jfif, *.png)|*bmp;*.gif;*.png;*.jpg;*.jpeg;*.jpe;*.jfif;*.png|BMP (*.bmp)|*.bmp|GIF (*.gif)|*.gif|JPEG (*.jpg, *.jpeg, *.jpe, *.jfif)|*.jpg;*.jpeg;*.jpe;*.jfif|PNG (*.png)|*.png"

        ListBox.SelectedIndex = 0
        MinecraftFolderTextBox.Text = My.Settings.MinecraftFolderLocation
        BackupsFolderTextBox.Text = My.Settings.BackupsFolderLocation
        SavesFolderTextBox.Text = My.Settings.SavesFolderLocation
        ListViewOpacitySlider.Value = My.Settings.OpacityPercent
        OpacityPercentLabel.Content = Int(ListViewOpacitySlider.Value).ToString & "%"
        BackgroundImageStyle.SelectedIndex = My.Settings.BackgroundImageStretch
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
        Else
            Me.Close() ' Close program if "Cancel" or "X" buttons are pressed
        End If
    End Sub

    Private Sub BrowseBackupsFolderButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseBackupsFolderButton.Click
        If FolderBrowserDialog.ShowDialog = Forms.DialogResult.OK Then
            BackupsFolderTextBox.Text = FolderBrowserDialog.SelectedPath
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

    Private Sub ListViewOpacitySlider_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        If Me.IsLoaded Then
            Dim Value As Double = ListViewOpacitySlider.Value / 100
            Main.ListView.Opacity = Value
            Main.Sidebar.Opacity = Value
            OpacityPercentLabel.Content = Math.Round(ListViewOpacitySlider.Value, 0).ToString & "%"
        End If
    End Sub

    Private Sub BackgroundImageRemoveButton_Click(sender As Object, e As RoutedEventArgs) Handles BackgroundImageRemoveButton.Click
        Main.Background = New SolidColorBrush(Color.FromArgb(255, 240, 240, 240))
        My.Settings.BackgroundImageLocation = ""
    End Sub

    Private Sub BackgroundImageBrowseButton_Click(sender As Object, e As RoutedEventArgs) Handles BackgroundImageBrowseButton.Click
        If OpenFileDialog.ShowDialog = Forms.DialogResult.OK Then
            Dim Brush As New ImageBrush(New BitmapImage(New Uri(OpenFileDialog.FileName)))
            Brush.Stretch = My.Settings.BackgroundImageStretch
            Main.Background = Brush
            My.Settings.BackgroundImageLocation = OpenFileDialog.FileName
        End If
    End Sub

    Private Sub Window_Unloaded(sender As Object, e As RoutedEventArgs) Handles MyBase.Unloaded
        My.Settings.MinecraftFolderLocation = MinecraftFolderTextBox.Text
        My.Settings.SavesFolderLocation = SavesFolderTextBox.Text
        My.Settings.BackupsFolderLocation = BackupsFolderTextBox.Text
        My.Settings.OpacityPercent = ListViewOpacitySlider.Value
        My.Settings.Save()
        Main.RefreshBackupsList()
    End Sub

    Private Sub SaveButton_Click(sender As Object, e As RoutedEventArgs) Handles CloseButton.Click
        Me.Close()
    End Sub

    Private Sub BackgroundImageStyle_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles BackgroundImageStyle.SelectionChanged
        If Not Me.IsLoaded Then
            Exit Sub
        End If

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
    End Sub
End Class
