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

Imports System.Windows
Imports System.Threading

Public Class ErrorWindow
    Shared newMessageBox As ErrorWindow

    Public Overloads Shared Sub Show(Message As String, Exception As Exception)
        If Application.Current.Dispatcher.CheckAccess() Then
            Try
                newMessageBox = New ErrorWindow
                newMessageBox.MessageLabel.Content = Message
                newMessageBox.ErrorTextBlock.Text = Exception.Message
                newMessageBox.Title = MCBackup.Language.Dictionary("Message.Caption.Error")
                newMessageBox.ContinueButton.Content = MCBackup.Language.Dictionary("ErrorForm.ContinueButton.Content")
                newMessageBox.CopyToClipboardButton.Content = MCBackup.Language.Dictionary("ErrorForm.CopyToClipboardButton.Content")
                System.Media.SystemSounds.Hand.Play()
                newMessageBox.ShowDialog()
            Catch ex As Exception
                newMessageBox = New ErrorWindow
                newMessageBox.MessageLabel.Content = Message
                newMessageBox.ErrorTextBlock.Text = Exception.Message
                System.Media.SystemSounds.Hand.Play()
                newMessageBox.ShowDialog()
            End Try
        Else
            Application.Current.Dispatcher.Invoke(Sub() Show(Message, Exception))
        End If
    End Sub

    Private Sub CopyToClipboardButton_Click(sender As Object, e As RoutedEventArgs) Handles CopyToClipboardButton.Click
        Clipboard.SetData(DataFormats.Text, newMessageBox.ErrorTextBlock.Text)
        MessageBox.Show("Copied to clipboard.", "Copied")
    End Sub

    Private Sub ContinueButton_Click(sender As Object, e As RoutedEventArgs) Handles ContinueButton.Click
        Me.Close()
    End Sub
End Class
