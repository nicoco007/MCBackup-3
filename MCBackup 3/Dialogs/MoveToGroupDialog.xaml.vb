Imports System.IO
Imports System.Threading

Public Class MoveToGroupDialog
    Private SelectedItems As List(Of ListViewBackupItem)
    Private Main As MainWindow = DirectCast(Application.Current.MainWindow, MainWindow)

    Sub New(SelectedItems As List(Of ListViewBackupItem))
        InitializeComponent()

        Me.SelectedItems = SelectedItems
    End Sub

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs) Handles Window.Loaded
        GroupsListBox.Items.Add(MCBackup.Language.Dictionary("Groups.None"))

        For Each Group As String In My.Settings.BackupGroups
            GroupsListBox.Items.Add(Group)
        Next

        LoadLanguage()
    End Sub

    Private Sub LoadLanguage()
        Me.Title = MCBackup.Language.Dictionary("MoveToGroupDialog.Title")
        MoveButton.Content = MCBackup.Language.Dictionary("MoveToGroupDialog.MoveButton.Text")
    End Sub

    Private Sub MoveButton_Click(sender As Object, e As RoutedEventArgs) Handles MoveButton.Click
        Dim Group As String = ""

        If Not GroupsListBox.SelectedIndex = 0 Then
            Group = GroupsListBox.SelectedItem
        End If

        Dim Thread As New Thread(Sub() Main.MoveToGroup(SelectedItems, Group))
        Thread.Start()
        Me.Close()
    End Sub
End Class
