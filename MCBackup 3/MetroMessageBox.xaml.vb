'   ╔═══════════════════════════════════════════════════════════════════════════╗
'   ║                        Copyright © 2014 nicoco007                         ║
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
        If Image > 0 Then
            MsgBox.Message.Margin = New Thickness(42, 10, 10, 44)
        End If

        Select Case Image
            Case 16
                MsgBox.Image.Source = New BitmapImage(New Uri("pack://application:,,,/Resources/msgbox.error.png"))
                System.Media.SystemSounds.Hand.Play()
            Case 32
                MsgBox.Image.Source = New BitmapImage(New Uri("pack://application:,,,/Resources/msgbox.question.png"))
                System.Media.SystemSounds.Asterisk.Play()
            Case 64
                MsgBox.Image.Source = New BitmapImage(New Uri("pack://application:,,,/Resources/msgbox.info.png"))
                System.Media.SystemSounds.Asterisk.Play()
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
