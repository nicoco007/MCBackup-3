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

Public Class Rename
    Private Main As MainWindow = DirectCast(Application.Current.MainWindow, MainWindow)

    Private Sub Rename_Load(sender As Object, e As EventArgs) Handles MyBase.Loaded
        TextBox.Text = Main.ListView.SelectedItem.Name
    End Sub

    Private Sub RenameButton_Click(sender As Object, e As EventArgs) Handles RenameButton.Click
        If TextBox.Text = "" Then
            MessageBox.Show("Please enter a valid name.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK)
            Exit Sub
        End If

        My.Computer.FileSystem.RenameDirectory(My.Settings.BackupsFolderLocation & "\" & Main.ListView.SelectedItem.Name, TextBox.Text)
        Main.RefreshBackupsList()
        Me.Close()
    End Sub

    Private Sub CancelButton_Click(sender As Object, e As EventArgs) Handles CancelButton.Click
        Me.Close()
    End Sub
End Class