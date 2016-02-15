Imports System.IO
Imports MCBackup.BackupManager
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Public Class BackupMetadata

    Private _Loaded As Boolean
    Public ReadOnly Property Loaded As Boolean
        Get
            Return _Loaded
        End Get
    End Property

    Private _OriginalName As String
    Public ReadOnly Property OriginalName As String
        Get
            Return _OriginalName
        End Get
    End Property

    Private _Type As BackupTypes
    Public ReadOnly Property Type As BackupTypes
        Get
            Return _Type
        End Get
    End Property

    Private _Description As String
    Public ReadOnly Property Description As String
        Get
            Return _Description
        End Get
    End Property

    Private _Launcher As Game.Launcher
    Public ReadOnly Property Launcher As Game.Launcher
        Get
            Return _Launcher
        End Get
    End Property

    Private _Modpack As String
    Public ReadOnly Property Modpack As String
        Get
            Return _Modpack
        End Get
    End Property

    Private _Group As String
    Public ReadOnly Property Group As String
        Get
            Return _Group
        End Get
    End Property

    Public Sub New(BackupLocation As String)

        If File.Exists(Path.Combine(BackupLocation, "info.json")) Then ' new format

            Dim json As JObject

            Using SR As New StreamReader(Path.Combine(BackupLocation, "info.json"))
                json = JsonConvert.DeserializeObject(SR.ReadToEnd)
            End Using

            _OriginalName = json("OriginalName")
            _Type = GetBackupType(json("Type"))
            _Description = json("Description")
            _Launcher = GetLauncher(json("Launcher"))
            _Modpack = json("Modpack")
            _Group = json("Group")

            _Loaded = True

        ElseIf File.Exists(Path.Combine(BackupLocation, "info.mcb")) Then ' old format

            Using SR As New StreamReader(Path.Combine(BackupLocation, "\info.mcb"))
                Do While SR.Peek <> -1
                    Dim Line As String = SR.ReadLine
                    If Not Line.StartsWith("#") Then
                        If Line.StartsWith("baseFolderName=") Then
                            _OriginalName = Line.Substring(15)
                        ElseIf Line.StartsWith("type=") Then
                            _Type = GetBackupType(Line.Substring(5))
                        ElseIf Line.StartsWith("desc=") Then
                            _Description = Line.Substring(5)
                        ElseIf Line.StartsWith("groupName=") Then
                            _Group = Line.Substring(10)
                        ElseIf Line.StartsWith("launcher=") Then
                            _Launcher = GetLauncher(Line.Substring(9))
                        ElseIf Line.StartsWith("modpack=") Then
                            _Modpack = Line.Substring(8)
                        End If
                    End If
                Loop
            End Using

            _Loaded = True

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
