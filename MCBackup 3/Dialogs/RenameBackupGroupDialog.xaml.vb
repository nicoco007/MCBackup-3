Public Class RenameBackupGroupDialog
    Sub New(GroupName As String)
        InitializeComponent()

        TextBox.Text = GroupName
    End Sub

    Public Overloads Function ShowDialog() As String
        MyBase.ShowDialog()
        Return TextBox.Text
    End Function

    Private Sub RenameButton_Click(sender As Object, e As RoutedEventArgs) Handles RenameButton.Click
        Me.Close()
    End Sub

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs) Handles Window.Loaded
        LoadLanguage()
    End Sub

    Private Sub LoadLanguage()
        Me.Title = MCBackup.Language.Dictionary("RenameWindow.Title")
        RenameButton.Content = MCBackup.Language.Dictionary("RenameWindow.RenameButton.Content")
        CancelButton.Content = MCBackup.Language.Dictionary("RenameWindow.CancelButton.Content")
    End Sub
End Class
