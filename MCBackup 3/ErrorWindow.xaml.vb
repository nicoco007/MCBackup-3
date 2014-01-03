Imports System.Windows
Imports System.Threading

Public Class ErrorWindow
    Shared newMessageBox As ErrorWindow
    Shared Button As Integer

    Public Shared Function ShowBox(Title As String, Message As String, Exception As Exception, IsLanguageError As Boolean) As Integer
        newMessageBox = New ErrorWindow
        newMessageBox.Title = Title
        newMessageBox.MessageLabel.Content = Message
        newMessageBox.ErrorTextBlock.Text = Exception.Message
        System.Media.SystemSounds.Hand.Play()
        newMessageBox.ShowDialog()
        Return Button
    End Function

    Public Shared Sub ShowBox(Title As String, Message As String, Exception As Exception)
        If Application.Current.Dispatcher.CheckAccess() Then
            newMessageBox = New ErrorWindow
            newMessageBox.Title = Title
            newMessageBox.MessageLabel.Content = Message
            newMessageBox.ErrorTextBlock.Text = Exception.Message
            newMessageBox.ContinueButton.Content = MCBackup.Language.Dictionnary("ErrorForm.ContinueButton.Content")
            newMessageBox.CopyToClipboardButton.Content = MCBackup.Language.Dictionnary("ErrorForm.CopyToClipboardButton.Content")
            System.Media.SystemSounds.Hand.Play()
            newMessageBox.ShowDialog()
        Else
            Application.Current.Dispatcher.Invoke(Sub() ShowBox(Title, Message, Exception))
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
