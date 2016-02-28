Imports System.IO
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Public Class BackupMetadata

    Private _FullPath As String

    Private _Loaded As Boolean
    Public ReadOnly Property Loaded As Boolean
        Get
            Return _Loaded
        End Get
    End Property

    Private _Name As String
    Public ReadOnly Property Name As String
        Get
            Return _Name
        End Get
    End Property

    Private _OriginalName As String
    Public ReadOnly Property OriginalName As String
        Get
            Return _OriginalName
        End Get
    End Property

    Private _Type As BackupType
    Public ReadOnly Property Type As BackupType
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

    Private _Launcher As Launcher
    Public ReadOnly Property Launcher As Launcher
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

    Const BackupsStayFreshFor As Integer = 7
    Const BackupsBecomeCrapAfter As Integer = 31

    Public Function GetColor() As Color

        Dim percent = Math.Max(Math.Min((Date.Today.Subtract(GetDateCreated()).TotalDays - BackupsStayFreshFor) / (BackupsBecomeCrapAfter - BackupsStayFreshFor), 1), 0)

        ' for red, we want p where
        ' 0 < p < 0.5    y = 2px
        ' 0.5 <= p < 1   y = p
        Dim red As Integer = IIf(percent < 0.5, percent * My.Settings.ListViewTextColorIntensity * 2, My.Settings.ListViewTextColorIntensity)

        ' for green, we want p where
        ' 0 < p < 0.5    y = p
        ' 0.5 <= p < 1   y = -2px + 2p (inverse of 2px)
        '                  = -2p(x - 1)
        Dim green As Integer = IIf(percent > 0.5, -2 * My.Settings.ListViewTextColorIntensity * (percent - 1), My.Settings.ListViewTextColorIntensity)

        Return Color.FromRgb(red, green, 0)

    End Function

    Public Sub New(BackupLocation As String)

        _Name = Path.GetFileName(BackupLocation)
        _FullPath = BackupLocation

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

    Private Function GetBackupType(Input As Object) As BackupType

        If IsNumeric(Input) Then

            Return Integer.Parse(Input)

        Else

            Select Case Input

                Case "save"
                    Return BackupType.World

                Case "version"
                    Return BackupType.Version

                Case "everything"
                    Return BackupType.Full

                Case Else
                    Return BackupType.World

            End Select

        End If

    End Function

    Private Function GetLauncher(Input As Object) As Launcher

        If IsNumeric(Input) Then

            If Input < 0 Or Input > [Enum].GetValues(GetType(Launcher)).Cast(Of Launcher).Last() Then

                Return Launcher.Minecraft

            Else

                Return Input

            End If

        ElseIf Not String.IsNullOrEmpty(Input) Then

            Select Case Input.ToString().ToLower()

                Case "minecraft"
                    Return Launcher.Minecraft

                Case "technic"
                    Return Launcher.Technic

                Case "ftb"
                Case "feedthebeast"
                    Return Launcher.FeedTheBeast

                Case "atlauncher"
                    Return Launcher.ATLauncher

            End Select

        End If

        Return Launcher.Minecraft

    End Function

    Public Function GetDateCreated()

        Return (New Scripting.FileSystemObject).GetFolder(_FullPath).DateCreated ' Get FolderPath's date of creation

    End Function

End Class
