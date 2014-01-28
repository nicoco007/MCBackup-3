Imports Scripting
Imports System.ComponentModel

Public Class CullWindow
    Private Main As MainWindow = DirectCast(Application.Current.MainWindow, MainWindow)

    Private DeleteBackgroundWorker As New BackgroundWorker()
    Private ListViewItems As New ArrayList

    Sub New()
        InitializeComponent()

        AddHandler DeleteBackgroundWorker.DoWork, New DoWorkEventHandler(AddressOf DeleteBackgroundWorker_DoWork)
        AddHandler DeleteBackgroundWorker.RunWorkerCompleted, New RunWorkerCompletedEventHandler(AddressOf DeleteBackgroundWorker_RunWorkerCompleted)
    End Sub

    Private Sub Cull_Loaded(sender As Object, e As RoutedEventArgs) Handles Cull.Loaded
        Dim DaysNumUpDownMargin As Thickness = DaysNumUpDown.Margin
        Dim Label2Margin As Thickness = Label2.Margin
        DaysNumUpDownMargin.Left = Label1.ActualWidth + 10
        Label2Margin.Left = Label1.ActualWidth + 95
        DaysNumUpDown.Margin = DaysNumUpDownMargin
        Label2.Margin = Label2Margin

        Dim CullButtonMargin As Thickness = CullButton.Margin
        CullButtonMargin.Left = (Me.Width / 2) - (CullButton.Width / 2)
        CullButton.Margin = CullButtonMargin
    End Sub

    Private Sub CullButton_Click(sender As Object, e As RoutedEventArgs) Handles CullButton.Click
        MetroMessageBox.Show("Are you sure you want remove all backups older than " & DaysNumUpDown.Value & " days? They will be deleted forever! (A long time)", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Question)
        ListViewItems.Clear()

        For Each Item In Main.ListView.Items
            If Main.GetFolderDateCreated(My.Settings.BackupsFolderLocation & "\" & Item.Name).AddDays(DaysNumUpDown.Value) < Date.Today Then
                ListViewItems.Add(Item.Name)
            End If
        Next

        DeleteBackgroundWorker.RunWorkerAsync()
        Main.StatusLabel.Content = MCBackup.Language.Dictionnary("Status.Deleting")
        Main.ProgressBar.IsIndeterminate = True
        Me.Close()
    End Sub

    Private Sub DeleteBackgroundWorker_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs)
        For Each Item As String In ListViewItems
            Try
                My.Computer.FileSystem.DeleteDirectory(My.Settings.BackupsFolderLocation & "\" & Item, FileIO.DeleteDirectoryOption.DeleteAllContents)
            Catch ex As Exception
                ErrorWindow.Show(MCBackup.Language.Dictionnary("Message.DeleteError"), ex)
            End Try
        Next
    End Sub

    Private Sub DeleteBackgroundWorker_RunWorkerCompleted(sender As Object, e As System.ComponentModel.RunWorkerCompletedEventArgs)
        Main.StatusLabel.Content = MCBackup.Language.Dictionnary("Status.DeleteComplete")
        Main.ProgressBar.IsIndeterminate = False
        Main.RefreshBackupsList()
    End Sub
End Class
