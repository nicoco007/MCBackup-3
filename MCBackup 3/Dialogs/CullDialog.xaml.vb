'   ╔═══════════════════════════════════════════════════════════════════════════╗
'   ║                      Copyright © 2013-2015 nicoco007                      ║
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

Imports Scripting
Imports System.ComponentModel

Public Class CullDialog
    Private Main As MainWindow = DirectCast(Application.Current.MainWindow, MainWindow)

    Private DeleteBackgroundWorker As New BackgroundWorker()
    Private ItemsToDelete As New ArrayList

    Sub New()
        InitializeComponent()

        AddHandler DeleteBackgroundWorker.DoWork, New DoWorkEventHandler(AddressOf DeleteBackgroundWorker_DoWork)
        AddHandler DeleteBackgroundWorker.RunWorkerCompleted, New RunWorkerCompletedEventHandler(AddressOf DeleteBackgroundWorker_RunWorkerCompleted)

        Me.Title = MCBackup.Language.Dictionary("CullWindow.Title")
        Label1.Content = MCBackup.Language.Dictionary("CullWindow.Label1.Content")
        Label2.Content = MCBackup.Language.Dictionary("CullWindow.Label2.Content")
        CullButton.Content = MCBackup.Language.Dictionary("CullWindow.CullButton.Content")
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
        If MetroMessageBox.Show(String.Format(MCBackup.Language.Dictionary("CullWindow.AreYouSureMsg"), DaysNumUpDown.Value), MCBackup.Language.Dictionary("Message.Caption.AreYouSure"), MessageBoxButton.YesNo, MessageBoxImage.Question) = MessageBoxResult.No Then
            Exit Sub
        End If

        ItemsToDelete.Clear()

        For Each Item In Main.ListView.Items
            If Main.GetFolderDateCreated(My.Settings.BackupsFolderLocation & "\" & Item.Name).AddDays(DaysNumUpDown.Value) < Date.Today Then
                ItemsToDelete.Add(Item.Name)
            End If
        Next

        DeleteBackgroundWorker.RunWorkerAsync()
        Main.StatusLabel.Content = MCBackup.Language.Dictionary("Status.Deleting")
        Main.ProgressBar.IsIndeterminate = True
        Me.Close()
    End Sub

    Private Sub DeleteBackgroundWorker_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs)
        For Each Item As String In ItemsToDelete
            Try
                My.Computer.FileSystem.DeleteDirectory(My.Settings.BackupsFolderLocation & "\" & Item, FileIO.DeleteDirectoryOption.DeleteAllContents)
            Catch ex As Exception
                ErrorReportDialog.Show(MCBackup.Language.Dictionary("Exception.Delete"), ex)
            End Try
        Next
    End Sub

    Private Sub DeleteBackgroundWorker_RunWorkerCompleted(sender As Object, e As System.ComponentModel.RunWorkerCompletedEventArgs)
        Main.StatusLabel.Content = MCBackup.Language.Dictionary("Status.DeleteComplete")
        Main.ProgressBar.IsIndeterminate = False
        Main.RefreshBackupsList()
    End Sub

    Private Sub Window_ContentRendered(sender As Object, e As EventArgs) Handles MyBase.ContentRendered
        ' SizeToContent Black Border Fix © nicoco007
        Dim s As New Size(Me.Width, Me.Height)
        Me.SizeToContent = SizeToContent.Manual
        Me.Width = s.Width
        Me.Height = s.Height
    End Sub
End Class
