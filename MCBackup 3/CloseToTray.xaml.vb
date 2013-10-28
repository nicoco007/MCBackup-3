Public Class CloseToTray

    Private Main As MainWindow = DirectCast(Application.Current.MainWindow, MainWindow)
    Public ToTray As Boolean

    Private Sub YesButton_Click(sender As Object, e As RoutedEventArgs) Handles YesButton.Click
        Main.ClsType = CloseAction.CloseType.CloseToTray
        My.Settings.SaveCloseState = SaveCheckBox.IsChecked
        My.Settings.CloseToTray = True
        My.Settings.Save()
        Me.Close()
    End Sub

    Private Sub NoButton_Click(sender As Object, e As RoutedEventArgs) Handles NoButton.Click
        Main.ClsType = CloseAction.CloseType.CloseCompletely
        My.Settings.SaveCloseState = SaveCheckBox.IsChecked
        My.Settings.CloseToTray = False
        My.Settings.Save()
        Me.Close()
    End Sub

    Private Sub CancelButton_Click(sender As Object, e As RoutedEventArgs) Handles CancelButton.Click
        Main.ClsType = CloseAction.CloseType.Cancel
        My.Settings.SaveCloseState = False
        My.Settings.CloseToTray = False
        My.Settings.Save()
        Me.Close()
    End Sub

    Private Sub SaveCheckBox_Click(sender As Object, e As RoutedEventArgs) Handles SaveCheckBox.Click
        CancelButton.IsEnabled = SaveCheckBox.IsChecked
    End Sub
End Class
