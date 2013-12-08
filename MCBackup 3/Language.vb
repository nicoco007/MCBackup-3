Imports System.IO

Public Class Language
    Public Shared Main As MainWindow = DirectCast(Application.Current.MainWindow, MainWindow)

    Public Shared LanguageDictionnary As New Dictionary(Of String, String)

    Public Shared Sub Load(FileName As String)
        LoadDictionnary(FileName)
        Main.BackupButton.Content = LanguageDictionnary("MainWindow.BackupButton.Content")
        Main.RestoreButton.Content = LanguageDictionnary("MainWindow.RestoreButton.Content")
        Main.DeleteButton.Content = LanguageDictionnary("MainWindow.DeleteButton.Content")
        Main.RenameButton.Content = LanguageDictionnary("MainWindow.RenameButton.Content")
        Main.AutomaticBackupButton.Content = LanguageDictionnary("MainWindow.AutomaticBackupButton.Content") & " >"
    End Sub

    Public Shared Sub LoadDictionnary(FileName As String)
        LanguageDictionnary.Add("MainWindow.BackupButton.Content", FindString("MainWindow.BackupButton.Content", FileName))
        LanguageDictionnary.Add("MainWindow.RestoreButton.Content", FindString("MainWindow.RestoreButton.Content", FileName))
        LanguageDictionnary.Add("MainWindow.DeleteButton.Content", FindString("MainWindow.DeleteButton.Content", FileName))
        LanguageDictionnary.Add("MainWindow.RenameButton.Content", FindString("MainWindow.RenameButton.Content", FileName))
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
                        Return "[FORMATTING ERROR]"
                        Exit Function
                    End If

                    ReturnString = ReturnString.Remove(ReturnString.LastIndexOf(""""))
                    Return ReturnString
                End If
            End While
        End Using
        Log.Print("FORMATTING ERROR: Unknown identifier """ & Name & """!", Log.Type.Severe)
        Return "[FORMATTING ERROR]"
    End Function
End Class
