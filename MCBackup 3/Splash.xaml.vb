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

Imports System.Windows.Threading
Imports System.Threading

Public Class Splash
    Private Main As MainWindow = DirectCast(Application.Current.MainWindow, MainWindow)

    Sub New()
        InitializeComponent()

        VersionLabel.Content = "MCBackup v" & Main.ApplicationVersion
    End Sub

    Public Sub ShowStatus(DictionaryEntry As String, DefaultString As String)
        Try
            Status.Content = MCBackup.Language.FindString(DictionaryEntry, My.Settings.Language & ".lang")
        Catch ex As Exception
            Status.Content = DefaultString
        End Try
    End Sub
End Class
