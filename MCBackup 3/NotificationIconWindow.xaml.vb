Public Class NotificationIconWindow
    Private MainWindow As MainWindow = DirectCast(Application.Current.MainWindow, MainWindow)

    Private Sub Window_Deactivated(sender As Object, e As EventArgs) Handles Window.Deactivated
        Me.Hide()
    End Sub

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs) Handles Window.Loaded
        TitleLabel.Content = "MCBackup v" & MainWindow.ApplicationVersion
    End Sub

    Private Sub Window_ContentRendered(sender As Object, e As EventArgs) Handles Window.ContentRendered
        Me.Focus()
    End Sub
End Class
