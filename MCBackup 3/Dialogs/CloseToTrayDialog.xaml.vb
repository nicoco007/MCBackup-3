﻿'   ╔═══════════════════════════════════════════════════════════════════════════╗
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

Public Class CloseToTrayDialog
    Private Main As MainWindow = DirectCast(Application.Current.MainWindow, MainWindow)
    Private PressedButton As Forms.DialogResult

    Sub New()
        InitializeComponent()
        LoadLanguage()
    End Sub

    Private Sub YesButton_Click(sender As Object, e As RoutedEventArgs) Handles YesButton.Click
        'Main.CloseType = CloseType.CloseToTray
        My.Settings.SaveCloseState = SaveCheckBox.IsChecked
        'My.Settings.CloseToTray = True
        'My.Settings.Save()
        PressedButton = Forms.DialogResult.Yes
        Me.Close()
    End Sub

    Private Sub NoButton_Click(sender As Object, e As RoutedEventArgs) Handles NoButton.Click
        'Main.CloseType = CloseType.CloseCompletely
        My.Settings.SaveCloseState = SaveCheckBox.IsChecked
        'My.Settings.CloseToTray = False
        'My.Settings.Save()
        PressedButton = Forms.DialogResult.No
        Me.Close()
    End Sub

    Private Sub CancelButton_Click(sender As Object, e As RoutedEventArgs) Handles CancelButton.Click
        'Main.CloseType = CloseType.Cancel
        My.Settings.SaveCloseState = False
        'My.Settings.CloseToTray = False
        'My.Settings.Save()
        PressedButton = Forms.DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub LoadLanguage()
        Me.Title = Application.Language.GetString("Close to Tray?")
        MessageLabel.Content = Application.Language.GetString("Would you like to close MCBackup to tray?")
        YesButton.Content = Application.Language.GetString("Yes")
        NoButton.Content = Application.Language.GetString("No")
        CancelButton.Content = Application.Language.GetString("Cancel")
        SaveCheckBox.Content = Application.Language.GetString("Always do this in the future")
        RevertLabel.Content = Application.Language.GetString("You can always revert this in the options menu.")
    End Sub

    Public Overloads Function ShowDialog() As Forms.DialogResult
        MyBase.ShowDialog()

        Return PressedButton
    End Function
End Class
