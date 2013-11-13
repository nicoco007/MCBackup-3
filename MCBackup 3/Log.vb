Imports System.IO

Public Class Log
    Public Enum Type
        Info
        Warning
        Severe
    End Enum

    Public Shared Main As MainWindow = DirectCast(Application.Current.MainWindow, MainWindow)

    Public Shared Sub StartNew()
        Dim SW As New StreamWriter(Main.StartupPath & "\mcbackup.log", True)
        Debug.Print("")
        Debug.Print("---------- Starting MCBackup v" & Main.ApplicationVersion & " ----------")
        Debug.Print("Start Time: " & DebugTimeStamp() & "")
        SW.WriteLine("")
        SW.WriteLine("---------- Starting MCBackup v" & Main.ApplicationVersion & " ----------")
        SW.WriteLine("Session Start Time: " & DebugTimeStamp() & "")
        SW.Dispose()
    End Sub

    Public Shared Sub Print(Message As String)
        Print(Message, Type.Info)
    End Sub

    Public Shared Sub Print(Message As String, LogType As Type)
        Dim SW As New StreamWriter(Main.StartupPath & "\mcbackup.log", True)
        Debug.Print(DebugTimeStamp() & " " & LogTypeToString(LogType) & " " & Message)
        SW.WriteLine(DebugTimeStamp() & " " & LogTypeToString(LogType) & " " & Message)
        SW.Dispose()
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
End Class
