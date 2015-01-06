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

Public Class DeleteDialog
    Private Shared Result As Forms.DialogResult = Forms.DialogResult.None

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs) Handles Window.Loaded
        LoadLanguage()
    End Sub

    Public Overloads Shared Function Show(Owner As Window) As MessageBoxResult
        Dim MsgBox As New DeleteDialog
        MsgBox.Owner = Owner
        MsgBox.Title = Application.Current.MainWindow.Title
        MsgBox.AreYouSureTextBlock.Text = MCBackup.Language.Dictionary("Message.DeleteAreYouSure")
        MsgBox.YesButton.Content = MCBackup.Language.Dictionary("MetroMsgBox.Button.Yes")
        MsgBox.NoButton.Content = MCBackup.Language.Dictionary("MetroMsgBox.Button.No")
        MsgBox.DoNotAskAgainCheckBox.Content = MCBackup.Language.Dictionary("DeleteDialog.DoNotAskAgain.Text")
        MsgBox.ShowDialog()
        My.Settings.ShowDeleteDialog = Not MsgBox.DoNotAskAgainCheckBox.IsChecked
        Return Result
    End Function

    Private Sub LoadLanguage()
        Me.Title = MCBackup.Language.Dictionary("Message.Caption.AreYouSure")
    End Sub

    Private Sub YesButton_Click(sender As Object, e As RoutedEventArgs) Handles YesButton.Click
        Result = Forms.DialogResult.Yes
        Me.Close()
    End Sub

    Private Sub NoButton_Click(sender As Object, e As RoutedEventArgs) Handles NoButton.Click
        Result = Forms.DialogResult.No
        Me.Close()
    End Sub

    Private Sub Window_ContentRendered(sender As Object, e As EventArgs) Handles MyBase.ContentRendered
        ' SizeToContent Black Border Fix © nicoco007
        Dim s As New Size(Me.Width, Me.Height)
        Me.SizeToContent = SizeToContent.Manual
        Me.Width = s.Width
        Me.Height = s.Height
    End Sub
End Class
