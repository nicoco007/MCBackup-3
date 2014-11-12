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
