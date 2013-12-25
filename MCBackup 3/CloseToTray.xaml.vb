﻿Imports MCBackup.CloseAction

Public Class CloseToTray
    Private Main As MainWindow = DirectCast(Application.Current.MainWindow, MainWindow)

    Sub New()
        InitializeComponent()

        Me.Height = 125
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
            Me.Height = 145
        Else
            Me.Height = 125
        End If
    End Sub
End Class
