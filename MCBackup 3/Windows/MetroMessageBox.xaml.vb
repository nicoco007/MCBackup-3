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

Public Class MetroMessageBox
    Private Shared MsgBox As MetroMessageBox
    Private Shared Result As MessageBoxResult = MessageBoxResult.None

    Sub New()
        InitializeComponent()
    End Sub

    Public Overloads Shared Function Show(Message As String) As MessageBoxResult
        MsgBox = New MetroMessageBox
        MsgBox.Title = Application.Current.MainWindow.Title
        MsgBox.Message.Text = Message
        ShowButtons(MessageBoxButton.OK)
        MsgBox.ShowDialog()
        Return Result
    End Function

    Public Overloads Shared Function Show(Message As String, Caption As String) As MessageBoxResult
        MsgBox = New MetroMessageBox
        MsgBox.Title = Caption
        MsgBox.Message.Text = Message
        ShowButtons(MessageBoxButton.OK)
        MsgBox.ShowDialog()
        Return Result
    End Function

    Public Overloads Shared Function Show(Message As String, Caption As String, Buttons As MessageBoxButton) As MessageBoxResult
        MsgBox = New MetroMessageBox
        MsgBox.Title = Caption
        MsgBox.Message.Text = Message
        ShowButtons(Buttons)
        MsgBox.ShowDialog()
        Return Result
    End Function

    Public Overloads Shared Function Show(Message As String, Caption As String, Buttons As MessageBoxButton, Image As MessageBoxImage) As MessageBoxResult
        MsgBox = New MetroMessageBox
        MsgBox.Title = Caption
        MsgBox.Message.Text = Message
        ShowButtons(Buttons)
        ShowImage(Image)
        MsgBox.ShowDialog()
        Return Result
    End Function

    Public Overloads Shared Function Show(Message As String, Caption As String, Buttons As MessageBoxButton, Image As MessageBoxImage, TextAlign As TextAlignment) As MessageBoxResult
        MsgBox = New MetroMessageBox
        MsgBox.Title = Caption
        MsgBox.Message.Text = Message
        MsgBox.Message.TextAlignment = TextAlign
        ShowButtons(Buttons)
        ShowImage(Image)
        MsgBox.ShowDialog()
        Return Result
    End Function

    Public Shared Sub ShowButtons(Button As MessageBoxButton)
        Select Case Button
            Case MessageBoxButton.OK
                MsgBox.Button1.Visibility = Visibility.Visible
                MsgBox.Button1.Content = Application.Language.GetString("OK")
                MsgBox.Button1.Tag = MessageBoxResult.OK
                MsgBox.Button2.Visibility = Visibility.Collapsed
                MsgBox.Button3.Visibility = Visibility.Collapsed
            Case MessageBoxButton.OKCancel
                MsgBox.Button1.Visibility = Visibility.Visible
                MsgBox.Button1.Content = Application.Language.GetString("Cancel")
                MsgBox.Button1.Tag = MessageBoxResult.Cancel
                MsgBox.Button2.Visibility = Visibility.Visible
                MsgBox.Button2.Content = Application.Language.GetString("OK")
                MsgBox.Button2.Tag = MessageBoxResult.OK
                MsgBox.Button3.Visibility = Visibility.Collapsed
            Case MessageBoxButton.YesNo
                MsgBox.Button1.Visibility = Visibility.Visible
                MsgBox.Button1.Content = Application.Language.GetString("No")
                MsgBox.Button1.Tag = MessageBoxResult.No
                MsgBox.Button2.Visibility = Visibility.Visible
                MsgBox.Button2.Content = Application.Language.GetString("Yes")
                MsgBox.Button2.Tag = MessageBoxResult.Yes
                MsgBox.Button3.Visibility = Visibility.Collapsed
            Case MessageBoxButton.YesNoCancel
                MsgBox.Button1.Visibility = Visibility.Visible
                MsgBox.Button1.Content = Application.Language.GetString("Cancel")
                MsgBox.Button1.Tag = MessageBoxResult.Cancel
                MsgBox.Button2.Visibility = Visibility.Visible
                MsgBox.Button2.Content = Application.Language.GetString("No")
                MsgBox.Button2.Tag = MessageBoxResult.No
                MsgBox.Button3.Visibility = Visibility.Visible
                MsgBox.Button3.Content = Application.Language.GetString("Yes")
                MsgBox.Button3.Tag = MessageBoxResult.Yes
                MsgBox.MinWidth = 264
        End Select
    End Sub

    Public Shared Sub ShowImage(Image As MessageBoxImage)
        If Image > 0 Then
            MsgBox.Message.Margin = New Thickness(52, 20, 20, 54)
        End If

        Select Case Image
            Case MessageBoxImage.Asterisk
                MsgBox.Image.Source = System.Drawing.SystemIcons.Asterisk.ToImageSource()
                Media.SystemSounds.Asterisk.Play()
            Case MessageBoxImage.Error
                MsgBox.Image.Source = System.Drawing.SystemIcons.Error.ToImageSource()
                Media.SystemSounds.Hand.Play()
            Case MessageBoxImage.Exclamation
                MsgBox.Image.Source = System.Drawing.SystemIcons.Exclamation.ToImageSource()
                Media.SystemSounds.Exclamation.Play()
            Case MessageBoxImage.Hand
                MsgBox.Image.Source = System.Drawing.SystemIcons.Hand.ToImageSource()
                Media.SystemSounds.Hand.Play()
            Case MessageBoxImage.Information
                MsgBox.Image.Source = System.Drawing.SystemIcons.Information.ToImageSource()
                Media.SystemSounds.Asterisk.Play()
            Case MessageBoxImage.Question
                MsgBox.Image.Source = System.Drawing.SystemIcons.Question.ToImageSource()
                Media.SystemSounds.Question.Play()
            Case MessageBoxImage.Stop
                MsgBox.Image.Source = System.Drawing.SystemIcons.Hand.ToImageSource()
                Media.SystemSounds.Hand.Play()
            Case MessageBoxImage.Warning
                MsgBox.Image.Source = System.Drawing.SystemIcons.Warning.ToImageSource()
                Media.SystemSounds.Exclamation.Play()
        End Select
    End Sub

    Private Sub Button1_Click(sender As Object, e As RoutedEventArgs) Handles Button1.Click
        Result = Button1.Tag
        Close()
    End Sub

    Private Sub Button2_Click(sender As Object, e As RoutedEventArgs) Handles Button2.Click
        Result = Button2.Tag
        Close()
    End Sub

    Private Sub Button3_Click(sender As Object, e As RoutedEventArgs) Handles Button3.Click
        Result = Button3.Tag
        Close()
    End Sub
End Class
