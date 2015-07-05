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

Imports System.Windows.Threading
Imports System.Threading

Public Class Splash
    Private Main As MainWindow = DirectCast(Application.Current.MainWindow, MainWindow)

    Sub New()
        InitializeComponent()

        VersionLabel.Content = "MCBackup v" & Main.ApplicationVersion
    End Sub

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        LoadLanguage()
    End Sub

    Private Sub LoadLanguage()
        Me.Title = "MCBackup v" & Main.ApplicationVersion
    End Sub

    Public Sub ShowStatus(DictionaryEntry As String, DefaultString As String)
        Try
            Status.Content = MCBackup.Language.FindString(DictionaryEntry, My.Settings.Language & ".lang")
        Catch ex As Exception
            Status.Content = DefaultString
        End Try
        Status.Refresh()
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
