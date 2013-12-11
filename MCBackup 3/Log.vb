Imports System.IO

Public Class Log
    Public Enum Type
        Info
        Warning
        Severe
    End Enum

    Public Shared Main As MainWindow = DirectCast(Application.Current.MainWindow, MainWindow)

    Public Shared Sub StartNew()
        DPrint("")
        DPrint("---------- Starting MCBackup v" & Main.ApplicationVersion & " @ " & DebugTimeStamp() & " ----------")
        Print("OS Name: " & GetWindowsVersion())
        Print("OS Version: " & Environment.OSVersion.Version.Major & "." & Environment.OSVersion.Version.Minor)
        Print("Architecture: " & GetWindowsArch())
        Print(".NET Framework Version: " & Environment.Version.Major & "." & Environment.Version.Minor)
    End Sub

    Public Shared Sub DPrint(Message As String)
        Using SW As New StreamWriter(Main.StartupPath & "\mcbackup.log", True)
            Debug.Print(Message)
            SW.WriteLine(Message)
        End Using
    End Sub

    Public Shared Sub Print(Message As String)
        Print(Message, Type.Info)
    End Sub

    Public Shared Sub Print(Message As String, LogType As Type)
        Using SW As New StreamWriter(Main.StartupPath & "\mcbackup.log", True)
            Debug.Print(DebugTimeStamp() & " " & LogTypeToString(LogType) & " " & Message)
            SW.WriteLine(DebugTimeStamp() & " " & LogTypeToString(LogType) & " " & Message)
        End Using
    End Sub

    Public Shared Function DebugTimeStamp()
        Dim Day As String = Format(Now(), "dd")
        Dim Month As String = Format(Now(), "MM")
        Dim Year As String = Format(Now(), "yyyy")
        Dim Hours As String = Format(Now(), "hh")
        Dim Minutes As String = Format(Now(), "mm")
        Dim Seconds As String = Format(Now(), "ss")

        Return Year & "-" & Month & "-" & Day & " " & Hours & ":" & Minutes & ":" & Seconds
    End Function

    Public Shared Function LogTypeToString(LogType As Type)
        Select Case LogType
            Case 0
                Return "[INFO]"
            Case 1
                Return "[WARNING]"
            Case 2
                Return "[SEVERE]"
        End Select
        Return ""
    End Function

    Public Shared Function GetWindowsVersion()
        Select Case Environment.OSVersion.Version.Major
            Case 5
                Select Case Environment.OSVersion.Version.Minor
                    Case 0
                        Return "Windows 2000"
                    Case 1 Or 2
                        Return "Windows XP"
                End Select
            Case 6
                Select Case Environment.OSVersion.Version.Minor
                    Case 0
                        Return "Windows Vista"
                    Case 1
                        Return "Windows 7"
                    Case 2
                        Return "Windows 8"
                    Case 3
                        Return "Windows 8.1"
                End Select
        End Select
        Return "Unknown"
    End Function

    Public Shared Function GetWindowsArch()
        If Environment.Is64BitOperatingSystem Then
            Return "amd64"
        Else
            Return "x86"
        End If
    End Function
End Class
