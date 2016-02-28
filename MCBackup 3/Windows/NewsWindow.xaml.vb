Imports System.Xml
Imports System.ServiceModel.Syndication

Public Class NewsWindow

    Private Sub Window_Loaded(sender As Object, e As EventArgs) Handles MyBase.Loaded

        LoadLanguage()

        'Using Reader As XmlReader = XmlReader.Create("https://www.nicoco007.com/category/mcbackup-3/feed")

        '    Dim Feed As SyndicationFeed = SyndicationFeed.Load(Reader)
        '    Log.Info("[RSS] " + Feed.Title.Text)
        '    WebBrowser.Navigate(Feed.Items(0).Links(0).Uri)

        '    For Each Item As SyndicationItem In Feed.Items
        '        Log.Info("[RSS] " + Item.Title.Text)
        '    Next

        'End Using

    End Sub

    Private Sub WebBrowser_Navigating(sender As Object, e As NavigatingCancelEventArgs) Handles WebBrowser.Navigating

        If e.Uri IsNot Nothing AndAlso Not (e.Uri.ToString.StartsWith("https://www.nicoco007.com/category/mcbackup-3") Or e.Uri.ToString.StartsWith("https://www.nicoco007.com/mcbackup-3")) Then

            Process.Start(e.Uri.ToString())
            e.Cancel = True

        Else

            If Me.IsLoaded Then

                ProgressRing.IsActive = True

            End If

        End If

    End Sub

    Private Sub WebBrowser_LoadCompleted(sender As Object, e As EventArgs) Handles WebBrowser.LoadCompleted

        ProgressRing.IsActive = False

    End Sub

    Private Sub CloseButton_Click(sender As Object, e As RoutedEventArgs) Handles CloseButton.Click

        Me.Close()

    End Sub

    Private Sub BackButton_Click(sender As Object, e As RoutedEventArgs) Handles BackButton.Click

        If WebBrowser.CanGoBack Then

            WebBrowser.GoBack()

        End If

    End Sub

    Private Sub ForwardButton_Click(sender As Object, e As RoutedEventArgs) Handles ForwardButton.Click

        If WebBrowser.CanGoForward Then

            WebBrowser.GoForward()

        End If

    End Sub

    Private Sub ProgressRing_Loaded(sender As Object, e As RoutedEventArgs) Handles ProgressRing.Loaded

        ' WebBrowser.Navigate("https://www.nicoco007.com/category/mcbackup-3?utm_source=mcbackup-news&utm_medium=mcbackup")

    End Sub

    Private Sub checkBox_Click(sender As Object, e As RoutedEventArgs) Handles ShowOnStartupCheckBox.Click

        My.Settings.ShowNewsOnStartup = ShowOnStartupCheckBox.IsChecked

    End Sub

    Private Sub LoadLanguage()

        Me.Title = MCBackup.Language.GetString("NewsWindow.Title")

        CloseButton.Content = MCBackup.Language.GetString("NewsWindow.CloseButton.Text")
        ShowOnStartupCheckBox.Content = MCBackup.Language.GetString("NewsWindow.ShowOnStartup.Text")

    End Sub

End Class
