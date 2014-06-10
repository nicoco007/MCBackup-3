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

    Public Enum Prefix
        Info
        Warning
        Severe
    End Enum

    ''' <summary>
    ''' Prints a message in the log file and the debug.
    ''' </summary>
    ''' <param name="Message">Message to display</param>
    Public Shared Sub Print(Message As String, ParamArray args As Object())
        Using SW As New StreamWriter(Main.StartupPath & "\mcbackup.log", True)
            Debug.Print(DebugTimeStamp() & " [INFO] " & Message, args)
            SW.WriteLine(DebugTimeStamp() & " [INFO] " & Message, args)
        End Using
    End Sub

    ''' <summary>
    ''' Prints a message in the log file and the debug with the specified prefix.
    ''' </summary>
    ''' <param name="Message">Message to be printed</param>
    ''' <param name="Prefix">Prefix, either [INFO], [WARNING], or [SEVERE]</param>
    Public Shared Sub Print(Message As String, Prefix As Prefix, ParamArray args As Object())
        Using SW As New StreamWriter(Main.StartupPath & "\mcbackup.log", True)
            Select Case Prefix
                Case Log.Prefix.Info
                    Debug.Print(DebugTimeStamp() & " [INFO] " & Message, args)
                    SW.WriteLine(DebugTimeStamp() & " [INFO] " & Message, args)
                Case Log.Prefix.Warning
                    Debug.Print(DebugTimeStamp() & " [INFO] " & Message, args)
                    SW.WriteLine(DebugTimeStamp() & " [INFO] " & Message, args)
                Case Log.Prefix.Severe
                    Debug.Print(DebugTimeStamp() & " [INFO] " & Message, args)
                    SW.WriteLine(DebugTimeStamp() & " [INFO] " & Message, args)
            End Select
        End Using
    End Sub

    ''' <summary>
    ''' Silently prints a message in the log file and the debug without a prefix/time stamp.
    ''' </summary>
    ''' <param name="Message">Message to be printed</param>
    Public Shared Sub SPrint(Message As String, ParamArray args As Object())
        Using SW As New StreamWriter(Main.StartupPath & "\mcbackup.log", True)
            Debug.Print(Message, args)
            SW.WriteLine(Message, args)
        End Using
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
