'   ╔═══════════════════════════════════════════════════════════════════════════╗
'   ║                         Copyright 2013 nicoco007                          ║
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
Imports System.Net
Imports Scripting
Imports System.Windows.Threading
Imports System.Windows.Forms
Imports System.Drawing
Imports System.ComponentModel
Imports System.Windows.Interop.Imaging

Class MainWindow
    Private AppData = Environ("APPDATA")
    Public BackupInfo(3) As String
    Public RestoreInfo(2) As String
    Private FolderBrowserDialog As New FolderBrowserDialog

    Private BackupBackgroundWorker As BackgroundWorker
    Private DeleteForRestoreBackgroundWorker As BackgroundWorker
    Private RestoreBackgroundWorker As BackgroundWorker
    Private DeleteBackgroundWorker As BackgroundWorker
    Private ThumbnailBackgroundWorker As BackgroundWorker

    Public Sub New()
        InitializeComponent()
        BackupBackgroundWorker = New BackgroundWorker()
        DeleteForRestoreBackgroundWorker = New BackgroundWorker()
        RestoreBackgroundWorker = New BackgroundWorker()
        DeleteBackgroundWorker = New BackgroundWorker()
        ThumbnailBackgroundWorker = New BackgroundWorker()
        AddHandler BackupBackgroundWorker.DoWork, New DoWorkEventHandler(AddressOf BackupBackgroundWorker_DoWork)
        AddHandler BackupBackgroundWorker.RunWorkerCompleted, New RunWorkerCompletedEventHandler(AddressOf BackupBackgroundWorker_RunWorkerCompleted)
        AddHandler DeleteForRestoreBackgroundWorker.DoWork, New DoWorkEventHandler(AddressOf DeleteForRestoreBackgroundWorker_DoWork)
        AddHandler DeleteForRestoreBackgroundWorker.RunWorkerCompleted, New RunWorkerCompletedEventHandler(AddressOf DeleteForRestoreBackgroundWorker_RunWorkerCompleted)
        AddHandler RestoreBackgroundWorker.DoWork, New DoWorkEventHandler(AddressOf RestoreBackgroundWorker_DoWork)
        AddHandler RestoreBackgroundWorker.RunWorkerCompleted, New RunWorkerCompletedEventHandler(AddressOf RestoreBackgroundWorker_RunWorkerCompleted)
        AddHandler DeleteBackgroundWorker.DoWork, New DoWorkEventHandler(AddressOf DeleteBackgroundWorker_DoWork)
        AddHandler DeleteBackgroundWorker.RunWorkerCompleted, New RunWorkerCompletedEventHandler(AddressOf DeleteBackgroundWorker_RunWorkerCompleted)
        AddHandler ThumbnailBackgroundWorker.DoWork, New DoWorkEventHandler(AddressOf ThumbnailBackgroundWorker_DoWork)
        AddHandler ThumbnailBackgroundWorker.RunWorkerCompleted, New RunWorkerCompletedEventHandler(AddressOf ThumbnailBackgroundWorker_RunWorkerCompleted)
    End Sub

#Region "Load"
    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs)
        Debug.Print(DebugTimeStamp() & "[DEBUG] Starting MCBackup")

        If My.Settings.OpacityPercent = 0 Then
            My.Settings.OpacityPercent = 100
        End If

        Dim OpacityDecimal As Double = My.Settings.OpacityPercent / 100

        ListView.Opacity = OpacityDecimal
        Sidebar.Opacity = OpacityDecimal

        If My.Settings.BackgroundImageLocation = "" Then
            Me.Background = New SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 240, 240, 240))
        Else
            Dim Brush As New ImageBrush(New BitmapImage(New Uri(My.Settings.BackgroundImageLocation)))
            Brush.Stretch = My.Settings.BackgroundImageStretch
            Me.Background = Brush
        End If

        If My.Settings.BackupsFolderLocation = "" Then
            My.Settings.BackupsFolderLocation = AppData & "\.mcbackup\backups"
        End If

        Debug.Print(DebugTimeStamp() & "[DEBUG] Set Backups folder location to """ & My.Settings.BackupsFolderLocation & """")
        
        My.Computer.FileSystem.CreateDirectory(My.Settings.BackupsFolderLocation)
        If Not My.Computer.FileSystem.DirectoryExists(My.Settings.MinecraftFolderLocation & "\launcher.jar") Then ' Check if saved directory exists AND still has Minecraft installed in it
            If My.Computer.FileSystem.FileExists(AppData & "\.minecraft\launcher.jar") Then ' If not, check for the usual Minecraft folder location
                My.Settings.MinecraftFolderLocation = AppData & "\.minecraft" ' Set folder location to default Minecraft folder location
                My.Settings.SavesFolderLocation = My.Settings.MinecraftFolderLocation & "\saves"
            Else
                MessageBox.Show("MCBackup was unable to find an installation of Minecraft on your computer. Please select your Minecraft folder in the following dialog", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1)
                Debug.Print(DebugTimeStamp() & "[WARNING] Minecraft folder not found")
                MinecraftFolderSearch()
                Exit Sub
            End If
        End If
        Debug.Print(DebugTimeStamp() & "[DEBUG] Minecraft folder set to """ & My.Settings.MinecraftFolderLocation & """")
        Debug.Print(DebugTimeStamp() & "[DEBUG] Saves folder set to """ & My.Settings.SavesFolderLocation & """")
        RefreshBackupsList()
    End Sub

    Private Sub MinecraftFolderSearch()
        If FolderBrowserDialog.ShowDialog = Windows.Forms.DialogResult.OK Then
            If My.Computer.FileSystem.FileExists(FolderBrowserDialog.SelectedPath & "\launcher.jar") Then ' Check if Minecraft exists in that folder
                MessageBox.Show("Minecraft folder set to " & FolderBrowserDialog.SelectedPath, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1) ' Tell user that folder has been selected successfully
                My.Settings.MinecraftFolderLocation = FolderBrowserDialog.SelectedPath
                My.Settings.SavesFolderLocation = My.Settings.MinecraftFolderLocation & "\saves"
                Debug.Print(DebugTimeStamp() & "[DEBUG] Minecraft folder set to """ & My.Settings.MinecraftFolderLocation & """")
                Debug.Print(DebugTimeStamp() & "[DEBUG] Saves folder set to """ & My.Settings.SavesFolderLocation & """")
                Exit Sub
            Else
                If MessageBox.Show("Minecraft is not installed in that folder! Try again?", "Error!", MessageBoxButtons.YesNo, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1) = Windows.Forms.DialogResult.Yes Then ' Ask if user wants to try finding folder again
                    MinecraftFolderSearch() ' Restart from beginning if "Yes"
                Else
                    Me.Close() ' Close program if "No"
                End If
            End If
        Else
            Me.Close() ' Close program if "Cancel" or "X" buttons are pressed
        End If
    End Sub

    Public Sub RefreshBackupsList()
        ListView.Items.Clear() ' Clear ListView items
        Dim Directory As New IO.DirectoryInfo(My.Settings.BackupsFolderLocation) ' Create a DirectoryInfo variable for the backups folder
        Dim Folders As IO.DirectoryInfo() = Directory.GetDirectories() ' Get all the directories in the backups folder
        Dim Folder As IO.DirectoryInfo ' Used to designate a single folder in the backups folder

        For Each Folder In Folders ' For each folder in the backups folder
            Dim ListViewItem As New ListViewItem()
            Dim ListViewSubItemDate As New ListViewItem.ListViewSubItem()
            Dim ListViewSubItemDescription As New ListViewItem.ListViewSubItem()

            ListViewItem.Text = Folder.ToString ' Set ListViewItem text and name to the name of the folder
            ListViewItem.Name = Folder.ToString

            ListViewSubItemDate.Text = GetFolderDateCreated(Directory.ToString & "\" & Folder.ToString) ' Set date subitem to the folder creation date

            Dim Type As String = "[ERROR]"                  ' <╗
            Dim Description As String = "[ERROR]"           ' <╬ Create variables with default value [ERROR], in case one of the values doesn't exist
            Dim OriginalFolderName As String = "[ERROR]"    ' <╝

            Try
                Using SR As New StreamReader(Directory.ToString & "\" & Folder.ToString & "\info.mcb")
                    Do While SR.Peek <> -1
                        Dim Line As String = SR.ReadLine
                        If Not Line.StartsWith("#") Then
                            If Line.StartsWith("desc=") Then ' If the line starts with description... 
                                Description = Line.Substring(5) ' ...set description subitem to that
                            ElseIf Line.StartsWith("type=") Then
                                Type = Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Line.Substring(5)) ' Set type to capitalized "type=" line
                            ElseIf Line.StartsWith("baseFolderName=") Then
                                OriginalFolderName = Line.Substring(15) ' Set original folder name to "baseFolderName=" line
                            End If
                        End If
                    Loop
                End Using
            Catch ex As Exception
                Debug.Print(DebugTimeStamp() & "[SEVERE] " & ex.Message)
            End Try

            ListView.Items.Add(New With {Key .Name = ListViewItem.Name, Key .DateCreated = GetFolderDateCreated(Directory.ToString & "\" & Folder.ToString), Key .Description = Description})
        Next

        Dim sender As Object = Nothing
        Dim e As EventArgs = Nothing
        ListView_SelectionChanged(sender, e)
    End Sub

    Private Sub ListView_SelectionChanged(sender As Object, e As EventArgs) Handles ListView.SelectionChanged
        If ListView.SelectedItems.Count = 0 Then
            RestoreButton.IsEnabled = False
            RenameButton.IsEnabled = False ' Don't allow anything when no items are selected
            DeleteButton.IsEnabled = False
        ElseIf ListView.SelectedItems.Count = 1 Then
            RestoreButton.IsEnabled = True
            RenameButton.IsEnabled = True ' Allow anything if only 1 item is selected
            DeleteButton.IsEnabled = True
        Else
            RestoreButton.IsEnabled = False
            RenameButton.IsEnabled = False ' Only allow deletion if more than 1 item is selected
            DeleteButton.IsEnabled = True
        End If

        If ListView.SelectedItems.Count = 1 Then
            If My.Computer.FileSystem.FileExists(My.Settings.BackupsFolderLocation & "\" & ListView.SelectedItem.Name & "\thumb.png") Then
                ThumbnailImage.Source = BitmapFromUri(New Uri(My.Settings.BackupsFolderLocation & "\" & ListView.SelectedItem.Name & "\thumb.png"))
            Else
                ThumbnailImage.Source = New BitmapImage(New Uri("pack://application:,,,/Resources/nothumb.png"))
            End If

            Dim Type As String = "N/A"
            Dim OriginalFolderName As String = "N/A"

            Try
                Using SR As New StreamReader(My.Settings.BackupsFolderLocation & "\" & ListView.SelectedItem.Name.ToString & "\info.mcb")
                    Do While SR.Peek <> -1
                        Dim Line As String = SR.ReadLine
                        If Not Line.StartsWith("#") Then
                            If Line.StartsWith("type=") Then
                                Type = Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Line.Substring(5)) ' Set type to capitalized "type=" line
                            ElseIf Line.StartsWith("baseFolderName=") Then
                                OriginalFolderName = Line.Substring(15) ' Set original folder name to "baseFolderName=" line
                            End If
                        End If
                    Loop
                End Using
            Catch ex As Exception
                Debug.Print(DebugTimeStamp() & "[SEVERE] " & ex.Message)
            End Try

            OriginalNameLabel.Text = OriginalFolderName
            OriginalNameLabel.ToolTip = OriginalFolderName
            TypeLabel.Text = Type
            TypeLabel.ToolTip = Type
        ElseIf ListView.SelectedItems.Count = 0 Then
            ThumbnailImage.Source = New BitmapImage(New Uri("pack://application:,,,/Resources/nothumb.png"))
            OriginalNameLabel.Text = "N/A"
            TypeLabel.Text = "N/A"
        End If
    End Sub
#End Region

#Region "Backup"
    Private Delegate Sub UpdateProgressBarDelegate(ByVal dp As System.Windows.DependencyProperty, ByVal value As Object)

    Private Sub BackupButton_Click(sender As Object, e As EventArgs) Handles BackupButton.Click
        Dim BackupWindow As New Backup
        BackupWindow.ShowDialog()
    End Sub

    Public Sub StartBackup()
        Debug.Print(DebugTimeStamp() & "[DEBUG] Starting new backup (name=""" & BackupInfo(0) & """; type=""" & BackupInfo(3) & """)")
        ListView.IsEnabled = False
        BackupButton.IsEnabled = False
        RestoreButton.IsEnabled = False
        DeleteButton.IsEnabled = False
        RenameButton.IsEnabled = False
        BackupBackgroundWorker.RunWorkerAsync()
        UpdateProgress()
    End Sub

    Private Sub BackupBackgroundWorker_DoWork(sender As Object, e As DoWorkEventArgs)
        Try
            My.Computer.FileSystem.CopyDirectory(BackupInfo(2), My.Settings.BackupsFolderLocation & "\" & BackupInfo(0), True) ' Copy selected save/version/everything to backups folder
            Using SW As New StreamWriter(My.Settings.BackupsFolderLocation & "\" & BackupInfo(0) & "\info.mcb") ' Create information fie (stores description and type)
                Dim BaseFolderName = BackupInfo(2).Split("\")
                SW.WriteLine("baseFolderName=" & BaseFolderName(BaseFolderName.GetUpperBound(BaseFolderName.Rank - 1))) ' Write save/version folder name
                SW.WriteLine("type=" & BackupInfo(3)) ' Write type in file
                SW.Write("desc=" & BackupInfo(1)) ' Write description if file
            End Using
        Catch ex As Exception
            MessageBox.Show("An error occured during the backup." & vbNewLine & vbNewLine & ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1)
            Debug.Print(DebugTimeStamp() & "[SEVERE] " & ex.Message)
        End Try
    End Sub

    Private Sub UpdateProgress()
        My.Computer.FileSystem.CreateDirectory(My.Settings.BackupsFolderLocation & "\" & BackupInfo(0))
        Dim PercentComplete As Double = 0

        Dim UpdateProgressBarDelegate As New UpdateProgressBarDelegate(AddressOf ProgressBar.SetValue)

        Do Until Int(PercentComplete) = 100
            PercentComplete = Int(GetFolderSize(My.Settings.BackupsFolderLocation & "\" & BackupInfo(0)) / GetFolderSize(BackupInfo(2)) * 100)
            StatusLabel.Content = "Backing up... (" & Math.Round(PercentComplete, 2) & "% Complete)"
            Dispatcher.Invoke(UpdateProgressBarDelegate, System.Windows.Threading.DispatcherPriority.Background, New Object() {ProgressBar.ValueProperty, PercentComplete})
        Loop
    End Sub

    Private Sub BackupBackgroundWorker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs)
        ProgressBar.Value = 100
        If BackupInfo(3) = "save" Then
            StatusLabel.Content = "Creating thumbnail..."
            Debug.Print(DebugTimeStamp() & "[DEBUG] Creating thumbnail")
            CreateThumb(BackupInfo(2))
        Else
            RefreshBackupsList()
            StatusLabel.Content = "Backup Complete; Ready"
            Debug.Print(DebugTimeStamp() & "[DEBUG] Backup Complete")
            ListView.IsEnabled = True
            BackupButton.IsEnabled = True
        End If
    End Sub
#End Region

#Region "Restore"
    Private Sub RestoreButton_Click(sender As Object, e As EventArgs) Handles RestoreButton.Click
        If MessageBox.Show("Are you sure you want to restore this backup? This will overwrite any existing content!", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) = Forms.DialogResult.Yes Then
            Debug.Print(DebugTimeStamp() & "[DEBUG] Starting Restore")
            RestoreInfo(0) = ListView.SelectedItems(0).Name ' Set place 0 of RestoreInfo array to the backup name

            Dim BaseFolderName As String = ""

            Using SR As New StreamReader(My.Settings.BackupsFolderLocation & "\" & RestoreInfo(0) & "\info.mcb")
                Do While SR.Peek <> -1
                    Dim Line As String = SR.ReadLine
                    If Not Line.StartsWith("#") Then
                        If Line.StartsWith("baseFolderName=") Then
                            BaseFolderName = Line.Substring(15)
                        ElseIf Line.StartsWith("type=") Then
                            RestoreInfo(2) = Line.Substring(5)
                        End If
                    End If
                Loop
            End Using

            Select Case RestoreInfo(2)
                Case "save"
                    RestoreInfo(1) = My.Settings.MinecraftFolderLocation & "\saves\" & BaseFolderName
                Case "version"
                    RestoreInfo(1) = My.Settings.MinecraftFolderLocation & "\versions\" & BaseFolderName
                Case "everything"
                    RestoreInfo(1) = My.Settings.MinecraftFolderLocation
            End Select

            DeleteForRestoreBackgroundWorker.RunWorkerAsync()
            ProgressBar.IsIndeterminate = True
            StatusLabel.Content = "Removing old content, please wait..."
            Debug.Print(DebugTimeStamp() & "[DEBUG] Removing old content")
        Else
            Exit Sub
        End If
    End Sub

    Private Sub DeleteForRestoreBackgroundWorker_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs)
        If My.Computer.FileSystem.DirectoryExists(RestoreInfo(1)) Then
            Try
                My.Computer.FileSystem.DeleteDirectory(RestoreInfo(1), FileIO.DeleteDirectoryOption.DeleteAllContents)
            Catch ex As Exception
                Debug.Print(DebugTimeStamp() & "[SEVERE] " & ex.Message)
            End Try
        End If
    End Sub

    Private Sub DeleteForRestoreBackgroundWorker_RunWorkerCompleted(sender As Object, e As System.ComponentModel.RunWorkerCompletedEventArgs)
        ProgressBar.IsIndeterminate = False
        RestoreBackgroundWorker.RunWorkerAsync()
        UpdateRestoreProgress()
        Debug.Print(DebugTimeStamp() & "[DEBUG] Removed old content, restoring")
    End Sub

    Private Sub RestoreBackgroundWorker_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs)
        Try
            My.Computer.FileSystem.CopyDirectory(My.Settings.BackupsFolderLocation & "\" & RestoreInfo(0), RestoreInfo(1))
            My.Computer.FileSystem.DeleteFile(RestoreInfo(1) & "\info.mcb")
        Catch ex As Exception
            MessageBox.Show("An error occured during the restore." & vbNewLine & vbNewLine & ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1)
            Debug.Print(DebugTimeStamp() & "[SEVERE] " & ex.Message)
        End Try
    End Sub

    Private Sub UpdateRestoreProgress()
        Dim PercentComplete As Integer = 0

        Dim UpdateRestoreProgressBarDelegate As New UpdateProgressBarDelegate(AddressOf ProgressBar.SetValue)

        Do Until PercentComplete = 100
            If My.Computer.FileSystem.DirectoryExists(RestoreInfo(1)) Then
                PercentComplete = GetFolderSize(RestoreInfo(1)) / GetFolderSize(My.Settings.BackupsFolderLocation & "\" & RestoreInfo(0)) * 100
                StatusLabel.Content = "Restoring... (" & Math.Round(PercentComplete, 2) & "% Complete)"
                Dispatcher.Invoke(UpdateRestoreProgressBarDelegate, System.Windows.Threading.DispatcherPriority.Background, New Object() {ProgressBar.ValueProperty, Convert.ToDouble(PercentComplete)})
            End If
        Loop
    End Sub

    Private Sub RestoreBackgroundWorker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs)
        StatusLabel.Content = "Restore Complete; Ready"
        RefreshBackupsList()
        Debug.Print(DebugTimeStamp() & "[DEBUG] Restore Complete")
    End Sub
#End Region

#Region "Functions"
    Private Function GetFolderSize(FolderPath As String)
        Try
            Dim FSO As FileSystemObject = New FileSystemObject
            Return FSO.GetFolder(FolderPath).Size ' Get FolderPath's size
        Catch ex As Exception
            Debug.Print(DebugTimeStamp() & "[SEVERE] Could not find " & FolderPath & "'s size: " & ex.Message)
        End Try
        Return 0
    End Function

    Private Function GetFolderDateCreated(FolderPath As String)
        Try
            Dim FSO As FileSystemObject = New FileSystemObject
            Return FSO.GetFolder(FolderPath).DateCreated ' Get FolderPath's date of creation
        Catch ex As Exception
            Debug.Print(DebugTimeStamp() & "[SEVERE] Could not find " & FolderPath & "'s creation date: " & ex.Message)
        End Try
        Return 0
    End Function

    Public Shared Function BitmapToBitmapSource(bitmap As Bitmap) As BitmapSource
        Return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions())
    End Function

    Public Function BitmapFromUri(Source As Uri) As ImageSource
        Dim Bitmap = New BitmapImage()
        Bitmap.BeginInit()
        Bitmap.UriSource = Source
        Bitmap.CacheOption = BitmapCacheOption.OnLoad
        Bitmap.EndInit()
        Return Bitmap
    End Function
#End Region

#Region "Menu Bar"
    Private Sub ExitMenuItem_Click(sender As Object, e As RoutedEventArgs)
        Me.Close()
    End Sub

    Private Sub BackupsFolderMenuItem(sender As Object, e As RoutedEventArgs)
        Process.Start(My.Settings.BackupsFolderLocation)
    End Sub

    Private Sub WebsiteMenuItem(sender As Object, e As RoutedEventArgs)
        Process.Start("http://www.nicoco007.com/minecraft/applications/mcbackup-3")
    End Sub

    Private Sub AboutMenuItem(sender As Object, e As RoutedEventArgs)
        Dim AboutWindow As New About
        AboutWindow.ShowDialog()
    End Sub
#End Region

#Region "Delete"
    Private ListViewItems As New ArrayList

    Private Sub DeleteButton_Click(sender As Object, e As EventArgs) Handles DeleteButton.Click
        If MessageBox.Show("Are you sure you want to delete the selected backup(s)?", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) = Windows.Forms.DialogResult.Yes Then
            ListViewItems.Clear()
            For Each Item In ListView.SelectedItems
                ListViewItems.Add(Item.Name)
            Next
            ListView.SelectedIndex = -1
            DeleteBackgroundWorker.RunWorkerAsync()
            StatusLabel.Content = "Deleting..."
            ProgressBar.IsIndeterminate = True
        End If
    End Sub

    Private Sub DeleteBackgroundWorker_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs)
        For Each Item As String In ListViewItems
            Try
                My.Computer.FileSystem.DeleteDirectory(My.Settings.BackupsFolderLocation & "\" & Item, FileIO.DeleteDirectoryOption.DeleteAllContents)
            Catch ex As Exception

            End Try
        Next
    End Sub

    Private Sub DeleteBackgroundWorker_RunWorkerCompleted(sender As Object, e As System.ComponentModel.RunWorkerCompletedEventArgs)
        StatusLabel.Content = "Delete Complete; Ready"
        ProgressBar.IsIndeterminate = False
        RefreshBackupsList()
    End Sub
#End Region

    Private Sub RenameButton_Click(sender As Object, e As EventArgs) Handles RenameButton.Click
        Dim RenameWindow As New Rename
        RenameWindow.ShowDialog()
    End Sub

    Private Sub OptionsMenuItem(sender As Object, e As RoutedEventArgs)
        Dim OptionsWindow As New Options
        OptionsWindow.ShowDialog()
    End Sub

    Private WorldPath As String = ""

    Private Sub CreateThumb(Path As String)
        ThumbnailBackgroundWorker.RunWorkerAsync()
        WorldPath = Path
    End Sub

    Private Sub ThumbnailBackgroundWorker_DoWork()
        Try
            Dim MCMapProcess As New Process
            MCMapProcess.StartInfo.FileName = Chr(34) & System.IO.Directory.GetCurrentDirectory() & "\bin\mcmap.exe" & Chr(34)
            MCMapProcess.StartInfo.Arguments = " " & Chr(34) & WorldPath & Chr(34)
            MCMapProcess.StartInfo.WorkingDirectory = System.IO.Directory.GetCurrentDirectory()
            MCMapProcess.StartInfo.CreateNoWindow = True
            MCMapProcess.StartInfo.UseShellExecute = False
            MCMapProcess.StartInfo.WorkingDirectory = System.IO.Directory.GetCurrentDirectory() & "\output\"
            MCMapProcess.Start()
            MCMapProcess.WaitForExit()
            My.Computer.FileSystem.MoveFile(System.IO.Directory.GetCurrentDirectory() & "\output\output.png", WorldPath & "\thumb.png", True)
        Catch ex As Exception
            Debug.Print(DebugTimeStamp() & "[SEVERE] " & ex.Message)
        End Try
    End Sub

    Private Sub ThumbnailBackgroundWorker_RunWorkerCompleted()
        RefreshBackupsList()
        StatusLabel.Content = "Backup Complete; Ready"
        Debug.Print(DebugTimeStamp() & "[DEBUG] Backup Complete")
        ListView.IsEnabled = True
        BackupButton.IsEnabled = True
    End Sub

    Private Function DebugTimeStamp()
        Dim Day As String = Format(Now(), "dd")
        Dim Month As String = Format(Now(), "MM")
        Dim Year As String = Format(Now(), "yyyy")
        Dim Hours As String = Format(Now(), "hh")
        Dim Minutes As String = Format(Now(), "mm")
        Dim Seconds As String = Format(Now(), "ss")

        Return Year & "-" & Month & "-" & Day & " " & Hours & ":" & Minutes & ":" & Seconds & " "
    End Function
End Class