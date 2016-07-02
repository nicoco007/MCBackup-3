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

Public Class ErrorReportDialog

    Public Overloads Shared Sub ShowDialog(Message As String, Exception As Exception)
        ShowDialog(Message, Exception, False)
    End Sub

    Public Overloads Shared Sub ShowDialog(Message As String, Exception As Exception, fatal As Boolean)
        Log.Print(Message & " " & Exception.Message)

        Dim ErrorReportDialog As New ErrorReportDialog()

        Try
            ErrorReportDialog.MessageLabel.Text = String.Format(Application.Language.GetString("An exception of type {0} occured: ") + Message, Exception.GetType)

            Dim StackTrace As String = New StackTrace(Exception, True).ToString
            Log.Print(StackTrace)
            ErrorReportDialog.ErrorTextBlock.Text = Exception.Message & vbNewLine & StackTrace

            ErrorReportDialog.Title = Application.Language.GetString("Error")
            ErrorReportDialog.ContinueButton.Content = IIf(fatal, Application.Language.GetString("Exit"), Application.Language.GetString("Continue"))
            ErrorReportDialog.CopyToClipboardButton.Content = Application.Language.GetString("Copy to Clipboard")
            ErrorReportDialog.ContactMessage.Text = Application.Language.GetString("If this error persists, please consider contacting us at support@nicoco007.com or submit a bug report using the button below.")
            ErrorReportDialog.ReportBugButton.Content = Application.Language.GetString("Report a Bug")
            Media.SystemSounds.Hand.Play()
            ErrorReportDialog.ShowDialog()
        Catch ex As Exception
            Log.Severe("Could not show ErrorReportDialog: " & ex.Message)
            Log.Severe(ex.StackTrace)
        End Try
    End Sub

    Private Sub CopyToClipboardButton_Click(sender As Object, e As RoutedEventArgs) Handles CopyToClipboardButton.Click
        Clipboard.SetData(DataFormats.Text, Me.ErrorTextBlock.Text)
        MetroMessageBox.Show(Application.Language.GetString("Successfully copied to clipboard!"), Application.Language.GetString("Copied to Clipboard!"), MessageBoxButton.OK, MessageBoxImage.Information)
    End Sub

    Private Sub ContinueButton_Click(sender As Object, e As RoutedEventArgs) Handles ContinueButton.Click
        Me.Close()
    End Sub

    Private Sub ReportBugButton_Click(sender As Object, e As RoutedEventArgs) Handles ReportBugButton.Click
        Process.Start("http://go.nicoco007.com/fwlink/?LinkID=5000")
    End Sub
End Class