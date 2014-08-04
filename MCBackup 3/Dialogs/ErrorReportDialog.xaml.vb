Imports System.Text.RegularExpressions

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

Public Class ErrorReportDialog
    Private Shared ErrorReportDialog As ErrorReportDialog
    Private Shared Main = TryCast(Application.Current.MainWindow, MainWindow)

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs) Handles MyBase.Loaded
        ' MahApps.Metro Black Border Fix © nicoco007
        Dim s As New Size(Me.Width, Me.Height)
        Me.SizeToContent = SizeToContent.Manual
        Me.Width = s.Width
        Me.Height = s.Height
    End Sub

    Public Overloads Shared Sub Show(Message As String, Exception As Exception)
        Log.Print(Message & " " & Exception.Message)

        ErrorReportDialog = New ErrorReportDialog

        Try
            If MCBackup.Language.IsLoaded Then
                ErrorReportDialog.MessageLabel.Content = Message

                Dim StackTrace As String = New StackTrace(Exception, True).ToString

                Debug.Print(StackTrace)
                ErrorReportDialog.ErrorTextBlock.Text = Exception.Message & vbNewLine & StackTrace

                ErrorReportDialog.Title = MCBackup.Language.Dictionary("Message.Caption.Error")
                ErrorReportDialog.ContinueButton.Content = MCBackup.Language.Dictionary("ErrorWindow.ContinueButton.Content")
                ErrorReportDialog.CopyToClipboardButton.Content = MCBackup.Language.Dictionary("ErrorWindow.CopyToClipboardButton.Content")
                ErrorReportDialog.ContactMessage.Content = MCBackup.Language.Dictionary("ErrorWindow.ContactMessage")
                System.Media.SystemSounds.Hand.Play()
                ErrorReportDialog.ShowDialog()
            Else
                ErrorReportDialog.MessageLabel.Content = Message

                Dim StackTrace As New StackTrace(Exception, True)
                For Each st As StackFrame In StackTrace.GetFrames
                    If String.IsNullOrEmpty(StackTrace.GetFrame(0).GetFileName) Then
                        If st.GetFileLineNumber <> 0 Then
                            ErrorReportDialog.ErrorTextBlock.Text = String.Format(MCBackup.Language.Dictionary("ErrorWindow.ErrorAtLine"), st.GetFileLineNumber, IO.Path.GetFileName(st.GetFileName), Exception.Message)
                        End If
                    Else
                        ErrorReportDialog.ErrorTextBlock.Text = String.Format(MCBackup.Language.Dictionary("ErrorWindow.ErrorAtLine"), StackTrace.GetFrame(0).GetFileLineNumber, IO.Path.GetFileName(StackTrace.GetFrame(0).GetFileName), Exception.Message)
                    End If
                Next

                System.Media.SystemSounds.Hand.Play()
                ErrorReportDialog.ShowDialog()
            End If
            Application.CloseAction = Application.AppCloseAction.Force
        Catch ex As Exception
            Debug.Print("Could not show ErrorReportDialog: " & ex.Message)
            Dim StackTrace As New StackTrace(ex, True)
            For Each Frame As StackFrame In StackTrace.GetFrames
                Debug.Print(Frame.ToString)
            Next
        End Try
    End Sub

    Private Sub CopyToClipboardButton_Click(sender As Object, e As RoutedEventArgs) Handles CopyToClipboardButton.Click
        Clipboard.SetData(DataFormats.Text, Me.ErrorTextBlock.Text)
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
