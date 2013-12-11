Imports System.IO

Public Class Language
    Public Shared Main As MainWindow = DirectCast(Application.Current.MainWindow, MainWindow)

    Public Shared LanguageDictionnary As New Dictionary(Of String, String)

    Public Shared Sub Load(FileName As String)
        LoadDictionnary(FileName)
        Main.BackupButton.Content = FindString("MainWindow.BackupButton.Content", FileName)
        Main.RestoreButton.Content = FindString("MainWindow.RestoreButton.Content", FileName)
        Main.DeleteButton.Content = FindString("MainWindow.DeleteButton.Content", FileName)
        Main.RenameButton.Content = FindString("MainWindow.RenameButton.Content", FileName)

        If Main.AutoBackupWindow.IsVisible Then
            Main.AutomaticBackupButton.Content = LanguageDictionnary("MainWindow.AutomaticBackupButton.Content") & " <"
        Else
            Main.AutomaticBackupButton.Content = LanguageDictionnary("MainWindow.AutomaticBackupButton.Content") & " >"
        End If

        Main.ListViewGridView.Columns(0).Header = FindString("MainWindow.ListView.Columns(0).Header", FileName)
        Main.ListViewGridView.Columns(1).Header = FindString("MainWindow.ListView.Columns(1).Header", FileName)
        Main.ListViewGridView.Columns(2).Header = FindString("MainWindow.ListView.Columns(2).Header", FileName)
        Main.OriginalNameLabel.Text = FindString("MainWindow.OriginalNameLabel.Text", FileName) & ":"
        Main.TypeLabel.Text = FindString("MainWindow.TypeLabel.Text", FileName) & ":"

        Main.MenuBar.Items(0).Header = FindString("MainWindow.MenuBar.Items(0).Header", FileName)
        Main.MenuBar.Items(0).Items(0).Header = FindString("MainWindow.MenuBar.Items(0).Items(0).Header", FileName)
        Main.MenuBar.Items(1).Header = FindString("MainWindow.MenuBar.Items(1).Header", FileName)
        Main.MenuBar.Items(1).Items(0).Header = FindString("MainWindow.MenuBar.Items(1).Items(0).Header", FileName)
        Main.MenuBar.Items(1).Items(1).Header = FindString("MainWindow.MenuBar.Items(1).Items(1).Header", FileName)
        Main.MenuBar.Items(2).Header = FindString("MainWindow.MenuBar.Items(2).Header", FileName)
        Main.MenuBar.Items(2).Items(0).Header = FindString("MainWindow.MenuBar.Items(2).Items(0).Header", FileName)
        Main.MenuBar.Items(3).Header = FindString("MainWindow.MenuBar.Items(3).Header", FileName)
        Main.MenuBar.Items(3).Items(0).Header = FindString("MainWindow.MenuBar.Items(3).Items(0).Header", FileName)
        Main.MenuBar.Items(3).Items(2).Header = FindString("MainWindow.MenuBar.Items(3).Items(2).Header", FileName)
        Main.MenuBar.Items(3).Items(3).Header = FindString("MainWindow.MenuBar.Items(3).Items(3).Header", FileName)

    End Sub

    Public Shared Sub LoadDictionnary(FileName As String)
        LanguageDictionnary.Clear()
        LanguageDictionnary.Add("MainWindow.AutomaticBackupButton.Content", FindString("MainWindow.AutomaticBackupButton.Content", FileName))
    End Sub

    Public Shared Function GetIDFromName(Name As String)
        Dim LanguageDirectory As New IO.DirectoryInfo(Main.StartupPath & "\language")
        Dim LanguageFiles As IO.FileInfo() = LanguageDirectory.GetFiles()
        Dim LanguageFile As IO.FileInfo

        For Each LanguageFile In LanguageFiles
            Using SR As New StreamReader(Main.StartupPath & "\language\" & LanguageFile.Name)
                If FindString("fullname", LanguageFile.Name) = Name Then
                    Return LanguageFile.Name
                End If
            End Using
        Next

        Return Nothing
    End Function

    Public Shared Function FindString(Name As String, FileName As String)
        Using SR As New StreamReader(Main.StartupPath & "\language\" & FileName)
            Dim LineNumber As Integer = 0
            While SR.Peek <> -1
                LineNumber += 1
                Dim Line As String = SR.ReadLine
                If Line.StartsWith(Name) And Not Line.StartsWith("#") Then
                    Dim ReturnString = Line.Substring(Name.Length + 2)

                    If Not ReturnString.Length - 1 = ReturnString.LastIndexOf("""") Then
                        Log.Print("FORMATTING ERROR @ LINE " & LineNumber & ": Unknown string """ & ReturnString.Substring(ReturnString.LastIndexOf("""") + 1) & """", Log.Type.Severe)
                        Return "[ERROR]"
                        Exit Function
                    End If

                    ReturnString = ReturnString.Remove(ReturnString.LastIndexOf(""""))
                    Return ReturnString
                End If
            End While
        End Using
        Log.Print("FORMATTING ERROR: """ & Name & """ indentifier not found!", Log.Type.Severe)
        Return "[ERROR]"
    End Function
End Class
