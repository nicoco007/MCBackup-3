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
    Private Main = DirectCast(Application.Current.MainWindow, MainWindow)

    Public Overloads Shared Sub Show(Message As String, Exception As Exception)
        If Application.Current.Dispatcher.CheckAccess() Then
            Try
                newMessageBox = New ErrorWindow
                newMessageBox.MessageLabel.Content = Message
                Dim StackTrace As New StackTrace(Exception, True)
                For Each st As StackFrame In StackTrace.GetFrames
                    If String.IsNullOrEmpty(StackTrace.GetFrame(0).GetFileName) Then
                        If st.GetFileLineNumber <> 0 Then
                            newMessageBox.ErrorTextBlock.Text = String.Format(MCBackup.Language.Dictionary("ErrorWindow.ErrorAtLine"), st.GetFileLineNumber, IO.Path.GetFileName(st.GetFileName), Exception.Message)
                        End If
                    Else
                        newMessageBox.ErrorTextBlock.Text = String.Format(MCBackup.Language.Dictionary("ErrorWindow.ErrorAtLine"), StackTrace.GetFrame(0).GetFileLineNumber, IO.Path.GetFileName(StackTrace.GetFrame(0).GetFileName), Exception.Message)
                    End If
                    
                    Log.Print(st.ToString, Log.Prefix.Severe)
                Next
                newMessageBox.Title = MCBackup.Language.Dictionary("Message.Caption.Error")
                newMessageBox.ContinueButton.Content = MCBackup.Language.Dictionary("ErrorWindow.ContinueButton.Content")
                newMessageBox.CopyToClipboardButton.Content = MCBackup.Language.Dictionary("ErrorWindow.CopyToClipboardButton.Content")
                newMessageBox.ContactMessage.Content = MCBackup.Language.Dictionary("ErrorWindow.ContactMessage")
                System.Media.SystemSounds.Hand.Play()
                newMessageBox.ShowDialog()
            Catch ex As Exception
                newMessageBox = New ErrorWindow
                newMessageBox.MessageLabel.Content = Message
                Dim StackTrace As New StackTrace(Exception, True)
                For Each st As StackFrame In StackTrace.GetFrames
                    If String.IsNullOrEmpty(StackTrace.GetFrame(0).GetFileName) Then
                        If st.GetFileLineNumber <> 0 Then
                            newMessageBox.ErrorTextBlock.Text = String.Format(MCBackup.Language.Dictionary("ErrorWindow.ErrorAtLine"), st.GetFileLineNumber, IO.Path.GetFileName(st.GetFileName), Exception.Message)
                        End If
                    Else
                        newMessageBox.ErrorTextBlock.Text = String.Format(MCBackup.Language.Dictionary("ErrorWindow.ErrorAtLine"), StackTrace.GetFrame(0).GetFileLineNumber, IO.Path.GetFileName(StackTrace.GetFrame(0).GetFileName), Exception.Message)
                    End If
                Next
                System.Media.SystemSounds.Hand.Play()
                newMessageBox.ShowDialog()
            End Try
        Else
            Application.Current.Dispatcher.Invoke(Sub() Show(Message, Exception))
        End If
    End Sub

    Private Sub CopyToClipboardButton_Click(sender As Object, e As RoutedEventArgs) Handles CopyToClipboardButton.Click
        Clipboard.SetData(DataFormats.Text, newMessageBox.ErrorTextBlock.Text)
        Try
            MetroMessageBox.Show(MCBackup.Language.Dictionary("Message.CopiedToClipboard"), MCBackup.Language.Dictionary("Message.Caption.Copied"), MessageBoxButton.OK, MessageBoxImage.Information)
        Catch ex As Exception
            MetroMessageBox.Show("Copied to clipboard.", "Copied", MessageBoxButton.OK, MessageBoxImage.Information)
        End Try
    End Sub

    Private Sub ContinueButton_Click(sender As Object, e As RoutedEventArgs) Handles ContinueButton.Click
        Me.Close()
    End Sub
End Class
