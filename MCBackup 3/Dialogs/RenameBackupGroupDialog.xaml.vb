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
End Class
