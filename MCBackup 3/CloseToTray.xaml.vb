Imports MCBackup.CloseAction

Public Class CloseToTray
    Private Main As MainWindow = DirectCast(Application.Current.MainWindow, MainWindow)

    Sub New()
        InitializeComponent()
        LoadLanguage()
        Me.Height = 120
    End Sub

    Private Sub YesButton_Click(sender As Object, e As RoutedEventArgs) Handles YesButton.Click
        Main.ClsType = CloseType.CloseToTray
        My.Settings.SaveCloseState = SaveCheckBox.IsChecked
        My.Settings.CloseToTray = True
        My.Settings.Save()
        Me.Close()
    End Sub

    Private Sub NoButton_Click(sender As Object, e As RoutedEventArgs) Handles NoButton.Click
        Main.ClsType = CloseType.CloseCompletely
        My.Settings.SaveCloseState = SaveCheckBox.IsChecked
        My.Settings.CloseToTray = False
        My.Settings.Save()
        Me.Close()
    End Sub

    Private Sub CancelButton_Click(sender As Object, e As RoutedEventArgs) Handles CancelButton.Click
        Main.ClsType = CloseType.Cancel
        My.Settings.SaveCloseState = False
        My.Settings.CloseToTray = False
        My.Settings.Save()
        Me.Close()
    End Sub

    Private Sub SaveCheckBox_Click(sender As Object, e As RoutedEventArgs) Handles SaveCheckBox.Click
        CancelButton.IsEnabled = Not SaveCheckBox.IsChecked
        If SaveCheckBox.IsChecked Then
            Me.Height = 140
        Else
            Me.Height = 120
        End If
    End Sub

    Private Sub LoadLanguage()
        Me.Title = MCBackup.Language.Dictionnary("CloseToTrayWindow.Title")
        MessageLabel.Content = MCBackup.Language.Dictionnary("CloseToTrayWindow.MessageLabel.Content")
        YesButton.Content = MCBackup.Language.Dictionnary("CloseToTrayWindow.YesButton.Content")
        NoButton.Content = MCBackup.Language.Dictionnary("CloseToTrayWindow.NoButton.Content")
        CancelButton.Content = MCBackup.Language.Dictionnary("CloseToTrayWindow.CancelButton.Content")
        SaveCheckBox.Content = MCBackup.Language.Dictionnary("CloseToTrayWindow.SaveCheckBox.Content")
        RevertLabel.Content = MCBackup.Language.Dictionnary("CloseToTrayWindow.RevertLabel.Content")
    End Sub

    Private Sub CloseToTrayWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles CloseToTrayWindow.Loaded
        Dim Margin = SaveCheckBox.Margin
        Margin.Left = (Grid.Width / 2) - (SaveCheckBox.ActualWidth / 2)
        SaveCheckBox.Margin = Margin
    End Sub
End Class
