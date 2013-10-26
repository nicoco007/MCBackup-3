Imports System.Windows.Threading

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

    Private Sub AutoBackupWindow_LocationChanged(sender As Object, e As EventArgs) Handles AutoBackupWindow.LocationChanged
        If Not Main.IsMoving Then
            IsMoving = True
            Main.Left = Me.Left - 855
            Main.Top = Me.Top
            IsMoving = False
        End If
    End Sub

    Private Sub PlusRepeatButton_Click(sender As Object, e As RoutedEventArgs) Handles PlusRepeatButton.Click
        If Not MinutesTextBox.Text = "60" Then
            MinutesTextBox.Text = MinutesTextBox.Text + 1
        End If
    End Sub

    Private Sub MinusRepeatButton_Click(sender As Object, e As RoutedEventArgs) Handles MinusRepeatButton.Click
        If Not MinutesTextBox.Text = "5" Then
            MinutesTextBox.Text = MinutesTextBox.Text - 1
        End If
    End Sub

    Private TimerStarted As Boolean

    Private Sub StartButton_Click(sender As Object, e As RoutedEventArgs) Handles StartButton.Click
        If TimerStarted Then
            Timer.Stop()
            TimeLabel.Content = "00:00"
            TimerStarted = False
        Else
            Minutes = MinutesTextBox.Text
            Seconds = 0
            TimeLabel.Content = IntToText(MinutesTextBox.Text) & ":00"
            Timer.Start()
            TimerStarted = True
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
            Main.BackupInfo(0) = PrefixTextBox.Text & GetTimeAndDate() & SuffixTextBox.Text
            Main.BackupInfo(1) = "Automated backup"
            Main.BackupInfo(2) = My.Settings.SavesFolderLocation & "\" & WorldName
            Main.BackupInfo(3) = "save"
            Main.StartBackup()

            Minutes = MinutesTextBox.Text
            Seconds = 0
            TimeLabel.Content = IntToText(MinutesTextBox.Text) & ":00"
        End If
    End Sub

    Private Function IntToText(Int As Integer)
        If Int >= 10 Then
            Return Int.ToString
        Else
            Return "0" & Int.ToString
        End If
    End Function

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        ReloadSaves()
    End Sub

    Private Sub ReloadSaves()
        SaveListBox.Items.Clear()
        Dim SavesDirectory As New IO.DirectoryInfo(My.Settings.SavesFolderLocation)
        Dim SavesFolders As IO.DirectoryInfo() = SavesDirectory.GetDirectories()
        Dim SavesFolder As IO.DirectoryInfo

        For Each SavesFolder In SavesFolders
            SaveListBox.Items.Add(SavesFolder.ToString)
        Next
    End Sub

    Private Sub AutoBackupWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles AutoBackupWindow.Loaded
        ReloadSaves()
    End Sub

    Private Sub AutoBackupWindow_Closing(sender As Object, e As ComponentModel.CancelEventArgs) Handles AutoBackupWindow.Closing
        e.Cancel = True
        Me.Hide()
        Main.Left = Main.Left + 155
        Main.AutomaticBackupButton.Content = "Automatic Backup >>"
    End Sub

    Private Sub MinutesTextBox_KeyDown(sender As Object, e As KeyEventArgs) Handles MinutesTextBox.KeyDown
        If e.Key < 34 Or e.Key > 43 Then
            If e.Key < 74 Or e.Key > 83 Then
                e.Handled = True
            End If
        End If

        Dim text As String = MinutesTextBox.Text

        If text.Length >= 2 Then
            e.Handled = True
        End If
    End Sub

    Private Sub MinutesTextBox_TextChanged(sender As Object, e As TextChangedEventArgs) Handles MinutesTextBox.TextChanged
        Dim text As String = MinutesTextBox.Text

        If text.IndexOf(" ") > -1 Then
            MinutesTextBox.Text = text.Remove(text.IndexOf(" "), 1)
        End If
    End Sub

    Private Sub MinutesTextBox_LostFocus(sender As Object, e As RoutedEventArgs) Handles MinutesTextBox.LostFocus
        If MinutesTextBox.Text < 5 Then
            MsgBox("You cannot enter a number under 5!")
            MinutesTextBox.Text = "5"
        End If

        If MinutesTextBox.Text > 60 Then
            MsgBox("You cannot enter a number over 60!")
            MinutesTextBox.Text = "60"
        End If
    End Sub

    Private Sub SaveListBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles SaveListBox.SelectionChanged
        WorldNameLabel.Content = SaveListBox.SelectedItem
        WorldName = SaveListBox.SelectedItem
    End Sub

    Private Function GetTimeAndDate()
        Dim Day As String = Format(Now(), "dd")
        Dim Month As String = Format(Now(), "MM")
        Dim Year As String = Format(Now(), "yyyy")
        Dim Hours As String = Format(Now(), "hh")
        Dim Minutes As String = Format(Now(), "mm")
        Dim Seconds As String = Format(Now(), "ss")

        Return Year & "-" & Month & "-" & Day & " (" & Hours & "h" & Minutes & "m" & Seconds & "s)"
    End Function
End Class
