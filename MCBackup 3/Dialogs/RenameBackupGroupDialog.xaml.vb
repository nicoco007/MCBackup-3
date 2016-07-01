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

Public Class RenameBackupGroupDialog
    Sub New(GroupName As String)
        InitializeComponent()

        TextBox.Text = GroupName
    End Sub

    Public Overloads Function ShowDialog() As String
        MyBase.ShowDialog()
        Return TextBox.Text
    End Function

    Private Sub RenameButton_Click(sender As Object, e As RoutedEventArgs) Handles RenameButton.Click
        Me.Close()
    End Sub

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs) Handles Window.Loaded
        LoadLanguage()
    End Sub

    Private Sub LoadLanguage()
        Me.Title = Application.Language.GetString("Rename Backup Group")
        RenameButton.Content = Application.Language.GetString("Rename")
        CancelButton.Content = Application.Language.GetString("Cancel")
    End Sub
End Class
