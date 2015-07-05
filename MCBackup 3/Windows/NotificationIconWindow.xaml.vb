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

Public Class NotificationIconWindow
    Private MainWindow As MainWindow = DirectCast(Application.Current.MainWindow, MainWindow)

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs) Handles Window.Loaded
        TitleLabel.Content = "MCBackup v" & MainWindow.ApplicationVersion
    End Sub

    Private Sub Window_ContentRendered(sender As Object, e As EventArgs) Handles Window.ContentRendered
        Me.Top = System.Windows.Forms.Cursor.Position.Y - Me.Height - 10
        Me.Left = System.Windows.Forms.Cursor.Position.X - (Me.Width / 2)
    End Sub
End Class
