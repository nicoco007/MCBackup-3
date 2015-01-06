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
