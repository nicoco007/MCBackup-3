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

Public Class UpdateDialog
    Private MainWindow As MainWindow = DirectCast(Application.Current.MainWindow, MainWindow)

    Private Sub YesButton_Click(sender As Object, e As RoutedEventArgs) Handles YesButton.Click
        Process.Start("http://go.nicoco007.com/fwlink/?LinkID=1006")
        Me.Close()
    End Sub

    Private Sub NoButton_Click(sender As Object, e As RoutedEventArgs) Handles NoButton.Click
        Me.Close()
    End Sub

    Private Sub UpdateDialog_Loaded(sender As Object, e As RoutedEventArgs)
        Me.Title = MCBackup.Language.GetString("UpdateDialog.Title")
        Label1.Content = MCBackup.Language.GetString("UpdateDialog.Label1.Text")
        CurrentVersionLabel.Content = String.Format(MCBackup.Language.GetString("UpdateDialog.CurrentVersionLabel.Text"), MainWindow.ApplicationVersion)
        LatestVersionLabel.Content = String.Format(MCBackup.Language.GetString("UpdateDialog.LatestVersionLabel.Text"), MainWindow.LatestVersion)
        Label2.Content = MCBackup.Language.GetString("UpdateDialog.Label2.Text")
        YesButton.Content = MCBackup.Language.GetString("MetroMsgBox.Button.Yes")
        NoButton.Content = MCBackup.Language.GetString("MetroMsgBox.Button.No")
        ShowChangelogButton.Content = MCBackup.Language.GetString("UpdateDialog.ShowChangelogButton.Text")
    End Sub

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        Process.Start("http://go.nicoco007.com/fwlink/?LinkID=1001&utm_source=mcbackup&utm_medium=mcbackup")
    End Sub
End Class
