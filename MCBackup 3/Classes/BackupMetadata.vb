Imports System.IO
Imports MCBackup.BackupManager
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Public Class BackupMetadata

    Public ReadOnly Property Loaded As Boolean
    Public ReadOnly Property OriginalName As String
    Public ReadOnly Property Type As BackupTypes
    Public ReadOnly Property Description As String
    Public ReadOnly Property Launcher As Game.Launcher
    Public ReadOnly Property Modpack As String
    Public ReadOnly Property Group As String

    Public Sub New(BackupLocation As String)

        If File.Exists(Path.Combine(BackupLocation, "info.json")) Then ' new format

            Dim json As JObject

            Using SR As New StreamReader(Path.Combine(BackupLocation, "info.json"))
                json = JsonConvert.DeserializeObject(SR.ReadToEnd)
            End Using

            OriginalName = json("OriginalName")
            Type = GetBackupType(json("Type"))
            Description = json("Description")
            Launcher = GetLauncher(json("Launcher"))
            Modpack = json("Modpack")
            Group = json("Group")

            Loaded = True

        ElseIf File.Exists(Path.Combine(BackupLocation, "info.mcb")) Then ' old format

            Using SR As New StreamReader(Path.Combine(BackupLocation, "\info.mcb"))
                Do While SR.Peek <> -1
                    Dim Line As String = SR.ReadLine
                    If Not Line.StartsWith("#") Then
                        If Line.StartsWith("baseFolderName=") Then
                            OriginalName = Line.Substring(15)
                        ElseIf Line.StartsWith("type=") Then
                            Type = GetBackupType(Line.Substring(5))
                        ElseIf Line.StartsWith("desc=") Then
                            Description = Line.Substring(5)
                        ElseIf Line.StartsWith("groupName=") Then
                            Group = Line.Substring(10)
                        ElseIf Line.StartsWith("launcher=") Then
                            Launcher = GetLauncher(Line.Substring(9))
                        ElseIf Line.StartsWith("modpack=") Then
                            Modpack = Line.Substring(8)
                        End If
                    End If
                Loop
            End Using

            Loaded = True

        End If

    End Sub

    Private Function GetBackupType(Input As Object) As BackupTypes

        If IsNumeric(Input) Then

            Return Integer.Parse(Input)

        Else

            Select Case Input

                Case "save"
                    Return BackupTypes.World

                Case "version"
                    Return BackupTypes.Version

                Case "everything"
                    Return BackupTypes.Full

                Case Else
                    Return BackupTypes.World

            End Select

        End If

    End Function

    Private Function GetLauncher(Input As Object) As Game.Launcher

        If IsNumeric(Input) Then

            If Input < 0 Or Input > [Enum].GetValues(GetType(Game.Launcher)).Cast(Of Game.Launcher).Last() Then

                Return Game.Launcher.Minecraft

            Else

                Return Input

            End If

        ElseIf Not String.IsNullOrEmpty(Input) Then

            Select Case Input.ToString().ToLower()

                Case "minecraft"
                    Return Game.Launcher.Minecraft

                Case "technic"
                    Return Game.Launcher.Technic

                Case "ftb"
                Case "feedthebeast"
                    Return Game.Launcher.FeedTheBeast

                Case "atlauncher"
                    Return Game.Launcher.ATLauncher

                Case Else
                    Return Game.Launcher.Minecraft

            End Select

        End If

        Return Game.Launcher.Minecraft

    End Function

End Class
