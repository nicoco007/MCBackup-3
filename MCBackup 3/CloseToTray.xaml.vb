Public Class CloseToTray

    Private Main As MainWindow = DirectCast(Application.Current.MainWindow, MainWindow)
    Public ToTray As Boolean

    Private Sub YesButton_Click(sender As Object, e As RoutedEventArgs) Handles YesButton.Click
        Main.CloseToTray = True
        My.Settings.SaveCloseState = SaveCheckBox.IsChecked
        My.Settings.CloseToTray = True
        My.Settings.Save()
        Me.Close()
    End Sub

    Private Sub NoButton_Click(sender As Object, e As RoutedEventArgs) Handles NoButton.Click
        Main.CloseToTray = False
        My.Settings.SaveCloseState = SaveCheckBox.IsChecked
        My.Settings.CloseToTray = False
        My.Settings.Save()
        Me.Close()
    End Sub
End Class
