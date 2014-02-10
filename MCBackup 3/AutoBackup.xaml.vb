Imports System.Windows.Threading
Imports System.Text.RegularExpressions

Public Class AutoBackup
    Private Main As MainWindow = DirectCast(Application.Current.MainWindow, MainWindow)
    Public IsMoving As Boolean

    Private Timer As New DispatcherTimer

    Private WorldName As String = ""

    Sub New()
        InitializeComponent()

        Timer.Interval = TimeSpan.FromSeconds(1)

        AddHandler Timer.Tick, New EventHandler(AddressOf Timer_Tick)
    End Sub

    Public Sub LoadLanguage()
        Try
            Me.Title = MCBackup.Language.Dictionary("AutoBackupWindow.Title")
            BackupEveryLabel.Content = MCBackup.Language.Dictionary("AutoBackupWindow.BackupEveryLabel.Content")
            MinutesLabel.Content = MCBackup.Language.Dictionary("AutoBackupWindow.MinutesLabel.Content")
            WorldToBackUpLabel.Text = MCBackup.Language.Dictionary("AutoBackupWindow.WorldToBackUpLabel.Text")
            RefreshButton.Content = MCBackup.Language.Dictionary("AutoBackupWindow.RefreshButton.Content")
            SaveAsLabel.Content = MCBackup.Language.Dictionary("AutoBackupWindow.SaveAsLabel.Content")
            PrefixLabel.Content = MCBackup.Language.Dictionary("AutoBackupWindow.PrefixLabel.Content")
            SuffixLabel.Content = MCBackup.Language.Dictionary("AutoBackupWindow.SuffixLabel.Content")
            StartButton.Content = MCBackup.Language.Dictionary("AutoBackupWindow.StartButton.Content.Start")
        Catch
        End Try
    End Sub

#Region "Window Handles"
    Private Sub AutoBackupWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles AutoBackupWindow.Loaded
        ReloadSaves()
        LoadLanguage()
        PrefixTextBox.Text = My.Settings.AutoBkpPrefix
        SuffixTextBox.Text = My.Settings.AutoBkpSuffix
    End Sub

    Private Sub AutoBackupWindow_Closing(sender As Object, e As ComponentModel.CancelEventArgs) Handles AutoBackupWindow.Closing
        e.Cancel = True
        Me.Hide()
        Main.Left = Main.Left + (Me.Width / 2)
        Try
            Main.AutomaticBackupButton.Content = MCBackup.Language.Dictionary("MainWindow.AutomaticBackupButton.Content") & " >>"
        Catch
        End Try
    End Sub

    Private Sub AutoBackupWindow_LocationChanged(sender As Object, e As EventArgs) Handles AutoBackupWindow.LocationChanged
        If Not Main.IsMoving Then
            IsMoving = True
            Main.Left = Me.Left - (Main.Width + 5)
            Main.Top = Me.Top
            IsMoving = False
        End If
    End Sub
#End Region

#Region "Timer"
    Private TimerStarted As Boolean

    Private Sub StartButton_Click(sender As Object, e As RoutedEventArgs) Handles StartButton.Click
        If TimerStarted Then
            Timer.Stop()
            TimeLabel.Content = "00:00"
            StartButton.Content = MCBackup.Language.Dictionary("AutoBackupWindow.StartButton.Content.Start")
            TimerStarted = False

            MinutesNumUpDown.IsEnabled = True
            SaveListBox.IsEnabled = True
            RefreshButton.IsEnabled = True
            PrefixTextBox.IsEnabled = True
            SuffixTextBox.IsEnabled = True
        Else
            If WorldName = "" Then
                MetroMessageBox.Show("Please select a world to automatically back up.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error)
                Exit Sub
            End If

            If Regex.IsMatch(PrefixTextBox.Text, "[\/?""|:<>*]") Or Regex.IsMatch(SuffixTextBox.Text, "[\/?""|:<>*]") Then
                MetroMessageBox.Show("Backup name cannot contain characters" & vbNewLine & "\ / : * ? "" < > |" & vbNewLine & "Check the prefix/suffix boxes.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error)
                Exit Sub
            End If

            Minutes = MinutesNumUpDown.Value
            Seconds = 0
            TimeLabel.Content = IntToText(MinutesNumUpDown.Value) & ":00"
            Timer.Start()
            StartButton.Content = MCBackup.Language.Dictionary("AutoBackupWindow.StartButton.Content.Stop")
            TimerStarted = True

            MinutesNumUpDown.IsEnabled = False
            SaveListBox.IsEnabled = False
            RefreshButton.IsEnabled = False
            PrefixTextBox.IsEnabled = False
            SuffixTextBox.IsEnabled = False
        End If
    End Sub

    Private Minutes, Seconds As Integer

    Private Sub Timer_Tick(sender As Object, e As EventArgs)
        If Seconds > 0 Then
            Seconds -= 1
        Else
            Seconds = 59
            Minutes -= 1
        End If

        TimeLabel.Content = IntToText(Minutes) & ":" & IntToText(Seconds)

        If Minutes = 0 And Seconds = 0 Then
            Log.Print("Starting automated backup...", Log.Type.Info)
            Main.BackupInfo(0) = PrefixTextBox.Text & GetTimeAndDate() & SuffixTextBox.Text
            Main.BackupInfo(1) = "Automated backup of " & WorldName
            Main.BackupInfo(2) = My.Settings.SavesFolderLocation & "\" & WorldName
            Main.BackupInfo(3) = "save"
            Main.StartBackup()

            If My.Settings.ShowBalloonTips Then Main.NotifyIcon.ShowBalloonTip(2000, "Automated Backup Started", "An automated backup started for """ & WorldName & """", Forms.ToolTipIcon.Info)

            Minutes = MinutesNumUpDown.Value
            Seconds = 0
            TimeLabel.Content = IntToText(MinutesNumUpDown.Value) & ":00"
        End If
    End Sub
#End Region

#Region "Functions"
    Private Sub ReloadSaves()
        SaveListBox.Items.Clear()
        Dim SavesDirectory As New IO.DirectoryInfo(My.Settings.SavesFolderLocation)
        Dim SavesFolders As IO.DirectoryInfo() = SavesDirectory.GetDirectories()
        Dim SavesFolder As IO.DirectoryInfo

        For Each SavesFolder In SavesFolders
            SaveListBox.Items.Add(SavesFolder.ToString)
        Next
    End Sub

    Private Function IntToText(Int As Integer)
        If Int >= 10 Then
            Return Int.ToString
        Else
            Return "0" & Int.ToString
        End If
    End Function

    Private Function GetTimeAndDate()
        Dim Day As String = Format(Now(), "dd")
        Dim Month As String = Format(Now(), "MM")
        Dim Year As String = Format(Now(), "yyyy")
        Dim Hours As String = Format(Now(), "hh")
        Dim Minutes As String = Format(Now(), "mm")
        Dim Seconds As String = Format(Now(), "ss")

        Return Year & "-" & Month & "-" & Day & " (" & Hours & "h" & Minutes & "m" & Seconds & "s)"
    End Function
#End Region


    Private Sub RefreshButton_Click(sender As Object, e As RoutedEventArgs) Handles RefreshButton.Click
        ReloadSaves()
    End Sub

    Private Sub SaveListBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles SaveListBox.SelectionChanged
        WorldName = SaveListBox.SelectedItem
    End Sub
End Class
