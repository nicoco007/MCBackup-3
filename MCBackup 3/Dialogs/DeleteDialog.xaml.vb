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

Public Class DeleteDialog
    Private Result As Forms.DialogResult = Forms.DialogResult.None

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs) Handles Window.Loaded
        LoadLanguage()
    End Sub

    Public Overloads Shared Function Show(owner As Window) As MessageBoxResult
        Dim deleteDialog As New DeleteDialog
        deleteDialog.Owner = owner
        deleteDialog.Title = Application.Current.MainWindow.Title
        deleteDialog.AreYouSureTextBlock.Text = Application.Language.GetString("Are you sure you want to delete the selected backup(s)?")
        deleteDialog.YesButton.Content = Application.Language.GetString("Yes")
        deleteDialog.NoButton.Content = Application.Language.GetString("No")
        deleteDialog.Image.Source = System.Drawing.SystemIcons.Question.ToImageSource()
        deleteDialog.DoNotAskAgainCheckBox.Content = Application.Language.GetString("Don't ask me again")
        deleteDialog.ShowDialog()
        My.Settings.ShowDeleteDialog = Not deleteDialog.DoNotAskAgainCheckBox.IsChecked
        Return deleteDialog.Result
    End Function

    Private Sub LoadLanguage()
        Me.Title = Application.Language.GetString("Are you sure?")
    End Sub

    Private Sub YesButton_Click(sender As Object, e As RoutedEventArgs) Handles YesButton.Click
        Result = Forms.DialogResult.Yes
        Me.Close()
    End Sub

    Private Sub NoButton_Click(sender As Object, e As RoutedEventArgs) Handles NoButton.Click
        Result = Forms.DialogResult.No
        Me.Close()
    End Sub
End Class
