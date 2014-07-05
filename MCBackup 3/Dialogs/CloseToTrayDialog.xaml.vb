'   ╔═══════════════════════════════════════════════════════════════════════════╗
'   ║                        Copyright © 2014 nicoco007                         ║
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

Imports MCBackup.CloseAction

Public Class CloseToTrayDialog
    Private Main As MainWindow = DirectCast(Application.Current.MainWindow, MainWindow)

    Sub New()
        InitializeComponent()
        LoadLanguage()
        Me.Height = 120
    End Sub

    Private Sub YesButton_Click(sender As Object, e As RoutedEventArgs) Handles YesButton.Click
        Main.CloseType = CloseType.CloseToTray
        My.Settings.SaveCloseState = SaveCheckBox.IsChecked
        My.Settings.CloseToTray = True
        My.Settings.Save()
        Me.Close()
    End Sub

    Private Sub NoButton_Click(sender As Object, e As RoutedEventArgs) Handles NoButton.Click
        Main.CloseType = CloseType.CloseCompletely
        My.Settings.SaveCloseState = SaveCheckBox.IsChecked
        My.Settings.CloseToTray = False
        My.Settings.Save()
        Me.Close()
    End Sub

    Private Sub CancelButton_Click(sender As Object, e As RoutedEventArgs) Handles CancelButton.Click
        Main.CloseType = CloseType.Cancel
        My.Settings.SaveCloseState = False
        My.Settings.CloseToTray = False
        My.Settings.Save()
        Me.Close()
    End Sub

    Private Sub SaveCheckBox_Click(sender As Object, e As RoutedEventArgs) Handles SaveCheckBox.Click
        CancelButton.IsEnabled = Not SaveCheckBox.IsChecked
        If SaveCheckBox.IsChecked Then
            Me.Height = 140
        Else
            Me.Height = 120
        End If
    End Sub

    Private Sub LoadLanguage()
        Me.Title = MCBackup.Language.Dictionary("CloseToTrayWindow.Title")
        MessageLabel.Content = MCBackup.Language.Dictionary("CloseToTrayWindow.MessageLabel.Content")
        YesButton.Content = MCBackup.Language.Dictionary("CloseToTrayWindow.YesButton.Content")
        NoButton.Content = MCBackup.Language.Dictionary("CloseToTrayWindow.NoButton.Content")
        CancelButton.Content = MCBackup.Language.Dictionary("CloseToTrayWindow.CancelButton.Content")
        SaveCheckBox.Content = MCBackup.Language.Dictionary("CloseToTrayWindow.SaveCheckBox.Content")
        RevertLabel.Content = MCBackup.Language.Dictionary("CloseToTrayWindow.RevertLabel.Content")
    End Sub

    Private Sub CloseToTrayWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles MyBase.Loaded
        Dim Margin = SaveCheckBox.Margin
        Margin.Left = (Grid.Width / 2) - (SaveCheckBox.ActualWidth / 2)
        SaveCheckBox.Margin = Margin
    End Sub
End Class
