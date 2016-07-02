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

Public Class RenameDialog
    Private Main As MainWindow = DirectCast(Application.Current.MainWindow, MainWindow)

    Public Sub New()
        InitializeComponent()

        LoadLanguage()
    End Sub

    Private Sub Rename_Load(sender As Object, e As EventArgs) Handles MyBase.Loaded
        TextBox.Text = Main.ListView.SelectedItem.Name
    End Sub

    Private Async Sub RenameButton_Click(sender As Object, e As EventArgs) Handles RenameButton.Click
        If TextBox.Text = "" Then
            MetroMessageBox.Show(Application.Language.GetString("Please enter a valid backup name."), Application.Language.GetString("Error"), MessageBoxButton.OK, MessageBoxImage.Error)
            Exit Sub
        End If

        Try
            If Not My.Computer.FileSystem.DirectoryExists(My.Settings.BackupsFolderLocation & "\" & TextBox.Text) Then
                My.Computer.FileSystem.RenameDirectory(My.Settings.BackupsFolderLocation & "\" & Main.ListView.SelectedItem.Name, TextBox.Text)
            Else
                MetroMessageBox.Show(Application.Language.GetString("A backup with that name already exists! Please choose another name."), Application.Language.GetString("Error"), MessageBoxButton.OK, MessageBoxImage.Error)
                Exit Sub
            End If
        Catch ex As Exception
            ErrorReportDialog.ShowDialog(Application.Language.GetString("An error occured while trying to rename the backup."), ex)
        End Try
        Await Main.RefreshBackupsList()
        Me.Close()
    End Sub

    Private Sub CancelButton_Click(sender As Object, e As EventArgs) Handles CancelButton.Click
        Me.Close()
    End Sub

    Private Sub LoadLanguage()
        Me.Title = Application.Language.GetString("Rename")
        RenameButton.Content = Application.Language.GetString("Rename")
        CancelButton.Content = Application.Language.GetString("Cancel")
    End Sub
End Class