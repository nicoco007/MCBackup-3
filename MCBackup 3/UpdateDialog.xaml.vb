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

Public Class UpdateDialog
    Private Main As MainWindow = DirectCast(Application.Current.MainWindow, MainWindow)

    Private Sub YesButton_Click(sender As Object, e As RoutedEventArgs) Handles YesButton.Click
        Process.Start("http://www.nicoco007.com/minecraft/applications/mcbackup-3/downloads/?fromApp=true")
        Me.Close()
    End Sub

    Private Sub NoButton_Click(sender As Object, e As RoutedEventArgs) Handles NoButton.Click
        Me.Close()
    End Sub

    Private Sub UpdateDialog_Loaded(sender As Object, e As RoutedEventArgs)
        If Main.IsLoaded Then
            CurrentVersionLabel.Content = "Current version: " & Main.ApplicationVersion
            LatestVersionLabel.Content = "Latest version: " & Main.LatestVersion
        End If
    End Sub
End Class
