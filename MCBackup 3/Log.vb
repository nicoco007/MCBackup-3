'   ╔═══════════════════════════════════════════════════════════════════════════╗
'   ║                        Copyright © 2014 nicoco007                         ║
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

Imports System.IO

Public Class Log
    Public Shared Main As MainWindow = DirectCast(Application.Current.MainWindow, MainWindow)

    Public Enum Level
        Info
        Warning
        Severe
        Debug
    End Enum

    ''' <summary>
    ''' Prints a message in the log file and the debug.
    ''' </summary>
    ''' <param name="Message">Message to display</param>
    Public Shared Sub Print(Message As String, ParamArray args As Object())
        Dim FullMessage As String = String.Format(Message, args)

        FullMessage = FullMessage.Replace(Environ("USERPROFILE"), "<USERDIRECTORY>")

        Dim FullMessageParts = FullMessage.Split(vbNewLine, 0, StringSplitOptions.RemoveEmptyEntries)

        Try
            Using SW As New StreamWriter(Main.StartupPath & "\mcbackup.log", True)
                For Each Part As String In FullMessageParts
                    Part = Part.Replace(vbNewLine, "")
                    Debug.Print(DebugTimeStamp() & " [INFO] " & Part)
                    SW.WriteLine(DebugTimeStamp() & " [INFO] " & Part)
                Next
            End Using
        Catch
        End Try
    End Sub

    ''' <summary>
    ''' Prints a message in the log file and the debug.
    ''' </summary>
    ''' <param name="Message">Message to display</param>
    Public Shared Sub Print(Message As String, Level As Level)
        Dim FullMessage = Message.Replace(Environ("USERPROFILE"), "<USERDIRECTORY>")

        Dim FullMessageParts = FullMessage.Split(vbNewLine, 0, StringSplitOptions.RemoveEmptyEntries)

        Try
            Using SW As New StreamWriter(Main.StartupPath & "\mcbackup.log", True)
                Select Case Level
                    Case Log.Level.Info
                        For Each Part As String In FullMessageParts
                            Part = Part.Replace(vbNewLine, "")
                            Debug.Print(DebugTimeStamp() & " [INFO] " & Part)
                            SW.WriteLine(DebugTimeStamp() & " [INFO] " & Part)
                        Next
                    Case Log.Level.Warning
                        For Each Part As String In FullMessageParts
                            Part = Part.Replace(vbNewLine, "")
                            Debug.Print(DebugTimeStamp() & " [WARNING] " & Part)
                            SW.WriteLine(DebugTimeStamp() & " [WARNING] " & Part)
                        Next
                    Case Log.Level.Severe
                        For Each Part As String In FullMessageParts
                            Part = Part.Replace(vbNewLine, "")
                            Debug.Print(DebugTimeStamp() & " [SEVERE] " & Part)
                            SW.WriteLine(DebugTimeStamp() & " [SEVERE] " & Part)
                        Next
                    Case Log.Level.Debug
                        If Environment.GetCommandLineArgs().Contains("-debug") Then
                            For Each Part As String In FullMessageParts
                                Part = Part.Replace(vbNewLine, "")
                                Debug.Print(DebugTimeStamp() & " [DEBUG] " & Part)
                                SW.WriteLine(DebugTimeStamp() & " [DEBUG] " & Part)
                            Next
                        End If
                End Select
            End Using
        Catch
        End Try
    End Sub

    ''' <summary>
    ''' Prints a message in the log file and the debug with the specified Level.
    ''' </summary>
    ''' <param name="Message">Message to be printed</param>
    ''' <param name="Level">Level, either [INFO], [WARNING], or [SEVERE]</param>
    Public Shared Sub Print(Message As String, Level As Level, ParamArray args As Object())
        Dim FullMessage As String = String.Format(Message, args)

        FullMessage = FullMessage.Replace(Environ("USERPROFILE"), "<USERDIRECTORY>")

        Dim FullMessageParts = FullMessage.Split(vbNewLine, 0, StringSplitOptions.RemoveEmptyEntries)

        Try
            Using SW As New StreamWriter(Main.StartupPath & "\mcbackup.log", True)
                Select Case Level
                    Case Log.Level.Info
                        For Each Part As String In FullMessageParts
                            Part = Part.Replace(vbNewLine, "")
                            Debug.Print(DebugTimeStamp() & " [INFO] " & Part)
                            SW.WriteLine(DebugTimeStamp() & " [INFO] " & Part)
                        Next
                    Case Log.Level.Warning
                        For Each Part As String In FullMessageParts
                            Part = Part.Replace(vbNewLine, "")
                            Debug.Print(DebugTimeStamp() & " [WARNING] " & Part)
                            SW.WriteLine(DebugTimeStamp() & " [WARNING] " & Part)
                        Next
                    Case Log.Level.Severe
                        For Each Part As String In FullMessageParts
                            Part = Part.Replace(vbNewLine, "")
                            Debug.Print(DebugTimeStamp() & " [SEVERE] " & Part)
                            SW.WriteLine(DebugTimeStamp() & " [SEVERE] " & Part)
                        Next
                    Case Log.Level.Debug
                        If Environment.GetCommandLineArgs().Contains("-debug") Then
                            For Each Part As String In FullMessageParts
                                Part = Part.Replace(vbNewLine, "")
                                Debug.Print(DebugTimeStamp() & " [DEBUG] " & Part)
                                SW.WriteLine(DebugTimeStamp() & " [DEBUG] " & Part)
                            Next
                        End If
                End Select
            End Using
        Catch
        End Try
    End Sub

    ''' <summary>
    ''' Silently prints a message in the log file and the debug without a Level/time stamp.
    ''' </summary>
    ''' <param name="Message">Message to be printed</param>
    Public Shared Sub SPrint(Message As String, ParamArray args As Object())
        Message = Message.Replace(Environ("USERPROFILE"), "<USERDIRECTORY>")
        Try
            Using SW As New StreamWriter(Main.StartupPath & "\mcbackup.log", True)
                Debug.Print(Message, args)
                SW.WriteLine(Message, args)
            End Using
        Catch
        End Try
    End Sub

    ''' <summary>
    ''' Returns a log timestamp
    ''' </summary>
    ''' <returns>A timestamp in the form YYYY-MM-DD hh:mm:ss</returns>
    Public Shared Function DebugTimeStamp()
        Return DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")
    End Function

    ''' <summary>
    ''' Converts Windows NT version to 'human-readable' name
    ''' </summary>
    ''' <returns>Windows OS Version name</returns>
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
        Return "Unknown / Incompatible"
    End Function

    ''' <summary>
    ''' Gets Windows architecture
    ''' </summary>
    ''' <returns>Windows architecture (32/64bit)</returns>

    Public Shared Function GetWindowsArch()
        If Environment.Is64BitOperatingSystem Then
            Return "amd64"
        Else
            Return "x86"
        End If
    End Function
End Class
