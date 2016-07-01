'   ╔═══════════════════════════════════════════════════════════════════════════╗
'   ║                      Copyright © 2013-2016 nicoco007                      ║
'   ║                                                                           ║
'   ║      Licensed under the Apache License, Version 2.0 (the "License");      ║
'   ║      you may not use this file except in compliance with the License.     ║
'   ║                  You may obtain a copy of the License at                  ║
'   ║                                                                           ║
'   ║                 http://www.apache.org/licenses/LICENSE-2.0                ║
'   ║                                                                           ║
'   ║    Unless required by applicable law or agreed to in writing, software    ║
'   ║     distributed under the License is distributed on an "AS IS" BASIS,     ║
'   ║  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. ║
'   ║     See the License for the specific language governing permissions and   ║
'   ║                      limitations under the License.                       ║
'   ╚═══════════════════════════════════════════════════════════════════════════╝

Imports System.ComponentModel

Public Class CullDialog
    Private Main As MainWindow = DirectCast(Application.Current.MainWindow, MainWindow)

    Private DeleteBackgroundWorker As New BackgroundWorker()
    Private ItemsToDelete As New ArrayList

    Sub New()
        InitializeComponent()

        AddHandler DeleteBackgroundWorker.DoWork, New DoWorkEventHandler(AddressOf DeleteBackgroundWorker_DoWork)
        AddHandler DeleteBackgroundWorker.RunWorkerCompleted, New RunWorkerCompletedEventHandler(AddressOf DeleteBackgroundWorker_RunWorkerCompleted)

        Me.Title = Application.Language.GetString("Selective deletion")
        Label1.Content = Application.Language.GetString("Delete all backups older than")
        Label2.Content = Application.Language.GetString("days.")
        CullButton.Content = Application.Language.GetString("Delete")
    End Sub

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Dim m1 As Thickness = DaysNumUpDown.Margin
        Dim m2 As Thickness = Label2.Margin
        m1.Left = Label1.ActualWidth + 15
        m2.Left = Label1.ActualWidth + DaysNumUpDown.ActualWidth + 20
        DaysNumUpDown.Margin = m1
        Label2.Margin = m2
    End Sub

    Private Sub CullButton_Click(sender As Object, e As RoutedEventArgs) Handles CullButton.Click
        ItemsToDelete.Clear()

        For Each Item In Main.ListView.Items
            If Main.GetFolderDateCreated(My.Settings.BackupsFolderLocation & "\" & Item.Name).AddDays(DaysNumUpDown.Value) < Date.Today Then
                ItemsToDelete.Add(Item.Name)
            End If
        Next

        If ItemsToDelete.Count > 0 Then
            If MetroMessageBox.Show(String.Format(Application.Language.GetPlural("You are about to delete one backup. Are you sure you want to do this? They will be deleted forever! (A long time)", "You are about to delete {0} backups. Are you sure you want to do this? They will be deleted forever! (A long time)", ItemsToDelete.Count)), Application.Language.GetString("Are you sure?"), MessageBoxButton.YesNo, MessageBoxImage.Question) = MessageBoxResult.Yes Then
                DeleteBackgroundWorker.RunWorkerAsync()
                Main.StatusLabel.Content = Application.Language.GetString("Deleting... ({0:0.00}% Complete)")
                Main.ProgressBar.IsIndeterminate = True
            End If
        Else
            MetroMessageBox.Show(Application.Language.GetString("No backups were created in the selected range of dates."))
        End If

        Me.Close()
    End Sub

    Private Sub DeleteBackgroundWorker_DoWork(sender As Object, e As DoWorkEventArgs)
        For Each Item As String In ItemsToDelete
            Try
                My.Computer.FileSystem.DeleteDirectory(My.Settings.BackupsFolderLocation & "\" & Item, FileIO.DeleteDirectoryOption.DeleteAllContents)
            Catch ex As Exception
                ErrorReportDialog.Show(Application.Language.GetString("An error occurred during the removal."), ex)
            End Try
        Next
    End Sub

    Private Async Sub DeleteBackgroundWorker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs)
        Main.StatusLabel.Content = Application.Language.GetString("Delete Complete; Ready")
        Main.ProgressBar.IsIndeterminate = False
        Await Main.RefreshBackupsList()
    End Sub
End Class
