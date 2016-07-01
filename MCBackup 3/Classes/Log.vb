'   ╔═══════════════════════════════════════════════════════════════════════════╗
'   ║                      Copyright © 2013-2016 nicoco007                      ║
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

    Public Shared Sub Verbose(message As String, ParamArray args() As Object)

        If CheckCommandLineArguments("-v", "--verbose") Then Print(DebugTimeStamp() + " [VERBOSE] " + message, args)

    End Sub


    Public Shared Sub Debug(message As String, ParamArray args() As Object)

        If CheckCommandLineArguments("-v", "--verbose", "-d", "--debug") Then Print(DebugTimeStamp() + " [DEBUG] " + message, args)

    End Sub


    Public Shared Sub Info(message As String, ParamArray args() As Object)

        Print(DebugTimeStamp() + " [INFO] " + message, args)

    End Sub

    Public Shared Sub Warn(message As String, ParamArray args() As Object)

        Print(DebugTimeStamp() + " [WARNING] " + message, args)

    End Sub

    Public Shared Sub Severe(message As String, ParamArray args() As Object)

        Print(DebugTimeStamp() + " [SEVERE] " + message, args)

    End Sub

    Public Shared Sub Print(text As String, ParamArray args() As Object)

        Dim message() As String

        message = IIf(args IsNot Nothing, FilterText(String.Format(text, args)), FilterText(text))

        Try
            Using streamWriter As New StreamWriter(Directory.GetCurrentDirectory() & "\mcbackup.log", True)

                For Each part As String In message

                    Diagnostics.Debug.Print(part)
                    streamWriter.WriteLine(part)

                Next

            End Using
        Catch ex As Exception

        End Try

    End Sub

    Private Shared Function CheckCommandLineArguments(ParamArray args() As String)

        For Each arg As String In args

            If Environment.GetCommandLineArgs().Contains(arg) Then Return True

        Next

        Return False

    End Function

    Private Shared Function FilterText(text As String) As String()

        text = text.Replace(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "<USERDIRECTORY>")

        Return text.Split(vbNewLine)

    End Function

    ''' <summary>
    ''' Returns a log timestamp
    ''' </summary>
    ''' <returns>A timestamp in the form YYYY-MM-DD hh:mm:ss</returns>
    Public Shared Function DebugTimeStamp() As String

        Return DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")

    End Function

    ''' <summary>
    ''' Gets Windows NT version
    ''' </summary>
    ''' <returns>Windows OS Version name</returns>
    Public Shared Function GetWindowsVersion() As String

        Return Environment.OSVersion.Version.Major & "." & Environment.OSVersion.Version.Minor


    End Function

    ''' <summary>
    ''' Gets Windows architecture
    ''' </summary>
    ''' <returns>Windows architecture (32/64bit)</returns>

    Public Shared Function GetWindowsArch() As String

        If Environment.Is64BitOperatingSystem Then

            Return "amd64"

        Else

            Return "x86"

        End If

    End Function

    Public Shared Function GetWindowsName() As String

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
            Case 10
                Select Case Environment.OSVersion.Version.Minor
                    Case 0
                        Return "Windows 10"
                End Select
        End Select

        Return "Unknown / Incompatible"

    End Function
End Class
