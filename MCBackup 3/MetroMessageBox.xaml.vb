Public Class MetroMessageBox
    Private Shared MsgBox As MetroMessageBox
    Private Shared Result As MessageBoxResult = MessageBoxResult.None

    Sub New()
        InitializeComponent()
    End Sub

    Public Overloads Shared Function Show(Message As String)
        MsgBox = New MetroMessageBox
        MsgBox.Title = Application.Current.MainWindow.Title
        MsgBox.Message.Text = Message
        ShowButtons(MessageBoxButton.OK)
        MsgBox.ShowDialog()
        Return Result
    End Function

    Public Overloads Shared Function Show(Message As String, Caption As String)
        MsgBox = New MetroMessageBox
        MsgBox.Title = Caption
        MsgBox.Message.Text = Message
        ShowButtons(MessageBoxButton.OK)
        MsgBox.ShowDialog()
        Return Result
    End Function

    Public Overloads Shared Function Show(Message As String, Caption As String, Buttons As MessageBoxButton)
        MsgBox = New MetroMessageBox
        MsgBox.Title = Caption
        MsgBox.Message.Text = Message
        ShowButtons(Buttons)
        MsgBox.ShowDialog()
        Return Result
    End Function

    Public Overloads Shared Function Show(Message As String, Caption As String, Buttons As MessageBoxButton, Image As MessageBoxImage)
        MsgBox = New MetroMessageBox
        MsgBox.Title = Caption
        MsgBox.Message.Text = Message
        ShowButtons(Buttons)
        ShowImage(Image)
        MsgBox.ShowDialog()
        Return Result
    End Function

    Public Shared Sub ShowButtons(Button As MessageBoxButton)
        Select Case Button
            Case 0
                MsgBox.Button1.Visibility = Windows.Visibility.Visible
                MsgBox.Button1.Content = "OK"
                MsgBox.Button1.Tag = MessageBoxResult.OK
                MsgBox.Button2.Visibility = Windows.Visibility.Collapsed
                MsgBox.Button3.Visibility = Windows.Visibility.Collapsed
            Case 1
                MsgBox.Button1.Visibility = Windows.Visibility.Visible
                MsgBox.Button1.Content = "Cancel"
                MsgBox.Button1.Tag = MessageBoxResult.Cancel
                MsgBox.Button2.Visibility = Windows.Visibility.Visible
                MsgBox.Button2.Content = "OK"
                MsgBox.Button2.Tag = MessageBoxResult.OK
                MsgBox.Button3.Visibility = Windows.Visibility.Collapsed
            Case 4
                MsgBox.Button1.Visibility = Windows.Visibility.Visible
                MsgBox.Button1.Content = "No"
                MsgBox.Button1.Tag = MessageBoxResult.No
                MsgBox.Button2.Visibility = Windows.Visibility.Visible
                MsgBox.Button2.Content = "Yes"
                MsgBox.Button2.Tag = MessageBoxResult.Yes
                MsgBox.Button3.Visibility = Windows.Visibility.Collapsed
            Case 3
                MsgBox.Button1.Visibility = Windows.Visibility.Visible
                MsgBox.Button1.Content = "Cancel"
                MsgBox.Button1.Tag = MessageBoxResult.Cancel
                MsgBox.Button2.Visibility = Windows.Visibility.Visible
                MsgBox.Button2.Content = "No"
                MsgBox.Button2.Tag = MessageBoxResult.No
                MsgBox.Button3.Visibility = Windows.Visibility.Visible
                MsgBox.Button3.Content = "Yes"
                MsgBox.Button3.Tag = MessageBoxResult.Yes
                MsgBox.MinWidth = 264
        End Select
    End Sub

    Public Shared Sub ShowImage(Image As MessageBoxImage)
        Select Case Image
            Case 16
                MsgBox.Image.Source = New BitmapImage(New Uri("pack://application:,,,/Resources/msgbox.error.png"))
                MsgBox.Message.Margin = New Thickness(40, 0, 0, 44)
            Case 32
                MsgBox.Image.Source = New BitmapImage(New Uri("pack://application:,,,/Resources/msgbox.question.png"))
                MsgBox.Message.Margin = New Thickness(40, 0, 0, 44)
            Case 64
                MsgBox.Image.Source = New BitmapImage(New Uri("pack://application:,,,/Resources/msgbox.info.png"))
                MsgBox.Message.Margin = New Thickness(40, 0, 0, 44)
        End Select
    End Sub

    Private Sub Button1_Click(sender As Object, e As RoutedEventArgs) Handles Button1.Click
        Result = Button1.Tag
        Me.Close()
    End Sub

    Private Sub Button2_Click(sender As Object, e As RoutedEventArgs) Handles Button2.Click
        Result = Button2.Tag
        Me.Close()
    End Sub

    Private Sub Button3_Click(sender As Object, e As RoutedEventArgs) Handles Button3.Click
        Result = Button2.Tag
        Me.Close()
    End Sub
End Class
