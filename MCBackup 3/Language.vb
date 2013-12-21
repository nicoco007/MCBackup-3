Imports System.IO

Public Class Language
    Public Shared Main As MainWindow = DirectCast(Application.Current.MainWindow, MainWindow)

    Public Shared Dictionnary As New Dictionary(Of String, String)

    Public Shared Sub Load(FileName As String)
        LoadDictionnary(FileName)
        Main.BackupButton.Content = FindString("MainWindow.BackupButton.Content", FileName)
        Main.RestoreButton.Content = FindString("MainWindow.RestoreButton.Content", FileName)
        Main.DeleteButton.Content = FindString("MainWindow.DeleteButton.Content", FileName)
        Main.RenameButton.Content = FindString("MainWindow.RenameButton.Content", FileName)

        If Main.AutoBackupWindow.IsVisible Then
            Main.AutomaticBackupButton.Content = Dictionnary("MainWindow.AutomaticBackupButton.Content") & " <"
        Else
            Main.AutomaticBackupButton.Content = Dictionnary("MainWindow.AutomaticBackupButton.Content") & " >"
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

        Main.StatusLabel.Content = FindString("Status.Ready", FileName)
    End Sub

    Public Shared Sub LoadDictionnary(FileName As String)
        Dictionnary.Clear()
        Dictionnary.Add("MainWindow.AutomaticBackupButton.Content", FindString("MainWindow.AutomaticBackupButton.Content", FileName))

        Dictionnary.Add("Status.BackingUp", FindString("Status.BackingUp", FileName))
        Dictionnary.Add("Status.BackupComplete", FindString("Status.BackupComplete", FileName))
        Dictionnary.Add("Status.CreatingThumb", FindString("Status.CreatingThumb", FileName))
        Dictionnary.Add("Status.RemovingOldContent", FindString("Status.RemovingOldContent", FileName))
        Dictionnary.Add("Status.Restoring", FindString("Status.Restoring", FileName))
        Dictionnary.Add("Status.RestoreComplete", FindString("Status.RestoreComplete", FileName))
        Dictionnary.Add("Status.Deleting", FindString("Status.Deleting", FileName))
        Dictionnary.Add("Status.DeleteComplete", FindString("Status.DeleteComplete", FileName))

        Dictionnary.Add("Message.Error.Title", FindString("Message.Error.Title", FileName))
        Dictionnary.Add("Message.Error.NoMinecraftInstall", FindString("Message.Error.NoMinecraftInstall", FileName))
        Dictionnary.Add("Message.Info.MinecraftFolderSetTo", FindString("Message.Info.MinecraftFolderSetTo", FileName))
    End Sub

    Public Shared Function GetIDFromName(Name As String)
        Dim LanguageDirectory As New IO.DirectoryInfo(Main.StartupPath & "\language")
        Dim LanguageFiles As IO.FileInfo() = LanguageDirectory.GetFiles()
        Dim LanguageFile As IO.FileInfo

        For Each LanguageFile In LanguageFiles
            Using SR As New StreamReader(Main.StartupPath & "\language\" & LanguageFile.Name)
                If FindString("fullname", LanguageFile.Name) = Name Then
                    Return LanguageFile.Name.Replace(".lang", "")
                End If
            End Using
        Next
        Return Nothing
    End Function

    Public Shared Function FindString(Identifier As String, FileName As String)
        Using SR As New StreamReader(Main.StartupPath & "\language\" & FileName)
            Dim LineNumber As Integer = 0
            While SR.Peek <> -1
                LineNumber += 1
                Dim Line As String = SR.ReadLine

                If Line.StartsWith(Identifier) And Not Line.StartsWith("#") Then
                    Dim ReturnString = Line.Substring(Identifier.Length + 2)

                    If Not ReturnString.Length - 1 = ReturnString.LastIndexOf("""") Then
                        Log.Print("FORMATTING ERROR @ LINE " & LineNumber & ": Unknown string """ & ReturnString.Substring(ReturnString.LastIndexOf("""") + 1) & """", Log.Type.Severe)
                        Return "[ERROR]"
                        Exit Function
                    End If

                    If ReturnString.Length - 1 = 0 Then
                        Log.Print("FORMATTING ERROR @ LINE " & LineNumber & ": Entry is empty!")
                        Return "[ERROR]"
                    End If

                    ReturnString = ReturnString.Remove(ReturnString.LastIndexOf(""""))
                    Return ReturnString
                End If
            End While
        End Using
        Log.Print("FORMATTING ERROR: """ & Identifier & """ indentifier not found!", Log.Type.Severe)
        Using SW As New StreamWriter(Main.StartupPath & "\language\" & FileName, True)
            SW.Write(vbNewLine & Identifier & "=""""")
        End Using
        Return "[ERROR]"
    End Function
End Class
