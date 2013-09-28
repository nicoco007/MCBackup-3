Public Class UpdateDialog
    Private Main As MainWindow = DirectCast(Application.Current.MainWindow, MainWindow)

    Private Sub YesButton_Click(sender As Object, e As RoutedEventArgs) Handles YesButton.Click
        Process.Start("http://www.nicoco007.com/minecraft/applications/mcbackup-3/downloads/?fromApp=true")
        Me.Close()
    End Sub

    Private Sub NoButton_Click(sender As Object, e As RoutedEventArgs) Handles NoButton.Click
        Me.Close()
    End Sub

    Private Sub UpdateDialog_Loaded(sender As Object, e As RoutedEventArgs)
        CurrentVersionLabel.Content = "Current version: " & Main.ApplicationVersion
        LatestVersionLabel.Content = "Latest version: " & Main.LatestVersion
    End Sub
End Class
