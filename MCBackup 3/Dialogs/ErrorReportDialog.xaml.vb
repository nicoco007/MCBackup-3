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

Imports System.Text.RegularExpressions

Public Class ErrorReportDialog
    Private Shared ErrorReportDialog As ErrorReportDialog
    Private Shared Main = TryCast(Application.Current.MainWindow, MainWindow)

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs) Handles MyBase.Loaded
        ' SizeToContent Black Border Fix © nicoco007
        Dim s As New Size(Me.Width, Me.Height)
        Me.SizeToContent = SizeToContent.Manual
        Me.Width = s.Width
        Me.Height = s.Height
    End Sub

    Public Overloads Shared Sub Show(Message As String, Exception As Exception)
        Log.Print(Message & " " & Exception.Message)

        ErrorReportDialog = New ErrorReportDialog

        Try
            If MCBackup.Language.IsLoaded Then
                ErrorReportDialog.MessageLabel.Text = String.Format(MCBackup.Language.Dictionary("Message.ExceptionOccured") & Message, Exception.GetType)

                Dim StackTrace As String = New StackTrace(Exception, True).ToString
                Log.Print(StackTrace)
                ErrorReportDialog.ErrorTextBlock.Text = Exception.Message & vbNewLine & StackTrace

                ErrorReportDialog.Title = MCBackup.Language.Dictionary("Message.Caption.Error")
                ErrorReportDialog.ContinueButton.Content = MCBackup.Language.Dictionary("ErrorWindow.ContinueButton.Content")
                ErrorReportDialog.CopyToClipboardButton.Content = MCBackup.Language.Dictionary("ErrorWindow.CopyToClipboardButton.Content")
                ErrorReportDialog.ContactMessage.Text = MCBackup.Language.Dictionary("ErrorWindow.ContactMessage")
                ErrorReportDialog.ReportBugButton.Content = MCBackup.Language.Dictionary("MainWindow.Toolbar.HelpContextMenu.Items(0).Header")
                System.Media.SystemSounds.Hand.Play()
                ErrorReportDialog.ShowDialog()
            Else
                ErrorReportDialog.MessageLabel.Text = String.Format("An exception of type {0} occured: " & Message, Exception.GetType)

                Dim StackTrace As String = New StackTrace(Exception, True).ToString
                Log.Print(StackTrace)
                ErrorReportDialog.ErrorTextBlock.Text = Exception.Message & vbNewLine & StackTrace

                System.Media.SystemSounds.Hand.Play()
                ErrorReportDialog.ShowDialog()
            End If
            Application.CloseAction = Application.AppCloseAction.Force
        Catch ex As Exception
            Log.Print("Could not show ErrorReportDialog: " & ex.Message, Log.Level.Warning)
            Try
                Log.Print(New StackTrace(ex, True).ToString)
            Catch InnerException As Exception
                Log.Print("Could not trace exception: " & InnerException.Message, Log.Level.Warning)
            End Try
        End Try
    End Sub

    Private Sub CopyToClipboardButton_Click(sender As Object, e As RoutedEventArgs) Handles CopyToClipboardButton.Click
        Clipboard.SetData(DataFormats.Text, Me.ErrorTextBlock.Text)
        Try
            MetroMessageBox.Show(MCBackup.Language.Dictionary("Message.CopiedToClipboard"), MCBackup.Language.Dictionary("Message.Caption.Copied"), MessageBoxButton.OK, MessageBoxImage.Information)
        Catch ex As Exception
            MetroMessageBox.Show("Copied to clipboard.", "Copied", MessageBoxButton.OK, MessageBoxImage.Information)
        End Try
    End Sub

    Private Sub ContinueButton_Click(sender As Object, e As RoutedEventArgs) Handles ContinueButton.Click
        Me.Close()
    End Sub

    Private Sub ReportBugButton_Click(sender As Object, e As RoutedEventArgs) Handles ReportBugButton.Click
        Process.Start("http://go.nicoco007.com/fwlink/?LinkID=5000")
    End Sub
End Class

Public Class TextblockEx
    Inherits TextBlock

    Sub New(Text As String)
        Me.Text = Text
    End Sub
End Class