Imports System.Windows
Imports System.Threading

Public Class ErrorWindow
    Shared newMessageBox As ErrorWindow
    Shared Button As Integer

    Public Overloads Shared Function Show(Message As String, Exception As Exception)
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
                Return Button
            Catch ex As Exception
                newMessageBox = New ErrorWindow
                newMessageBox.MessageLabel.Content = Message
                newMessageBox.ErrorTextBlock.Text = Exception.Message
                System.Media.SystemSounds.Hand.Play()
                newMessageBox.ShowDialog()
                Return Button
            End Try
        Else
            Return Application.Current.Dispatcher.Invoke(Function() Show(Message, Exception))
        End If
    End Function

    Private Sub CopyToClipboardButton_Click(sender As Object, e As RoutedEventArgs) Handles CopyToClipboardButton.Click
        Clipboard.SetData(DataFormats.Text, newMessageBox.ErrorTextBlock.Text)
        MessageBox.Show("Copied to clipboard.", "Copied")
    End Sub

    Private Sub ContinueButton_Click(sender As Object, e As RoutedEventArgs) Handles ContinueButton.Click
        Me.Close()
    End Sub
End Class
