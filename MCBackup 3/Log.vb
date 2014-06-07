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
    Public Enum Prefix
        None
        Info
        Warning
        Severe
    End Enum

    Public Shared Main As MainWindow = DirectCast(Application.Current.MainWindow, MainWindow)

    ''' <summary>
    ''' Starts new logging session
    ''' </summary>

    Public Shared Sub StartNew()

    End Sub

    ''' <summary>
    ''' Prints a message in the log file and the debug.
    ''' </summary>
    ''' <param name="Message">Message to display</param>
    ''' <param name="Prefix">Message Prefix ([INFO], [WARNING], [SEVERE])</param>
    ''' <param name="HasTimeStamp"></param>
    ''' <remarks></remarks>
    Public Shared Sub Print(Message As String)
        Using SW As New StreamWriter(Main.StartupPath & "\mcbackup.log", True)
            Debug.Print("{0} [INFO] {1}", DebugTimeStamp(), Message)
            SW.WriteLine("{0} [INFO] {1}", DebugTimeStamp(), Message)
        End Using
    End Sub

    Public Shared Sub Print(Message As String, Prefix As Prefix)
        Using SW As New StreamWriter(Main.StartupPath & "\mcbackup.log", True)
            Select Case Prefix
                Case 0
                    Debug.Print(Message)
                    SW.WriteLine(Message)
                Case 1
                    Debug.Print("{0} [INFO] {1}", DebugTimeStamp(), Message)
                    SW.WriteLine("{0} [INFO] {1}", DebugTimeStamp(), Message)
                Case 2
                    Debug.Print("{0} [WARNING] {1}", DebugTimeStamp(), Message)
                    SW.WriteLine("{0} [WARNING] {1}", DebugTimeStamp(), Message)
                Case 3
                    Debug.Print("{0} [SEVERE] {1}", DebugTimeStamp(), Message)
                    SW.WriteLine("{0} [SEVERE] {1}", DebugTimeStamp(), Message)
            End Select
        End Using
    End Sub

    Public Shared Sub Print(Message As String, Prefix As Prefix, HasTimeStamp As Boolean)
        Using SW As New StreamWriter(Main.StartupPath & "\mcbackup.log", True)
            Select Case Prefix
                Case 0
                    Debug.Print(Message)
                    SW.WriteLine(Message)
                Case 1
                    Debug.Print("{0} [INFO] {1}", IIf(HasTimeStamp, DebugTimeStamp(), ""), Message)
                    SW.WriteLine("{0} [INFO] {1}", IIf(HasTimeStamp, DebugTimeStamp(), ""), Message)
                Case 2
                    Debug.Print("{0} [WARNING] {1}", IIf(HasTimeStamp, DebugTimeStamp(), ""), Message)
                    SW.WriteLine("{0} [WARNING] {1}", IIf(HasTimeStamp, DebugTimeStamp(), ""), Message)
                Case 3
                    Debug.Print("{0} [SEVERE] {1}", IIf(HasTimeStamp, DebugTimeStamp(), ""), Message)
                    SW.WriteLine("{0} [SEVERE] {1}", IIf(HasTimeStamp, DebugTimeStamp(), ""), Message)
            End Select
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
    ''' Gets windows architecture
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
