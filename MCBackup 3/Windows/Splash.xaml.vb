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

Public Class Splash

    Sub New()
        InitializeComponent()

        VersionLabel.Content = "MCBackup v" + MainWindow.ApplicationVersion.ToString()
    End Sub

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        LoadLanguage()
    End Sub

    Private Sub LoadLanguage()
        Me.Title = "MCBackup v" + MainWindow.ApplicationVersion.ToString()
    End Sub

    Public Sub SetProgress(Progress As Integer)
        Me.Progress.Value = Progress
        Me.Progress.Refresh()
    End Sub

    Public Sub StepProgress()
        Me.Progress.Value += 1
        Me.Progress.Refresh()
    End Sub
End Class
