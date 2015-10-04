'   ╔═══════════════════════════════════════════════════════════════════════════╗
'   ║                      Copyright © 2013-2015 nicoco007                      ║
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
Imports System.Text.RegularExpressions

Public Class Language
    Private Shared Main As MainWindow = DirectCast(Application.Current.MainWindow, MainWindow)
    Private Shared ErrorOccured As Boolean

    Private Shared _IsLoaded As Boolean = False
    Public Shared Property IsLoaded() As Boolean
        Get
            Return _IsLoaded
        End Get
        Set(value As Boolean)
            _IsLoaded = value
        End Set
    End Property

    Public Shared Dictionary As New Dictionary(Of String, String)

    Public Shared Sub Load(FileName As String)

        Log.Print("Loading language from file '" & FileName & "'...")
        Dictionary.Clear()

        Using sr As New StreamReader(Directory.GetCurrentDirectory & "\language\" & FileName)

            Dim lineNumber As Integer = 0

            While sr.Peek <> -1

                lineNumber += 1

                Dim line As String = sr.ReadLine()

                If line.Contains("//") Then

                    line = line.Remove(line.IndexOf("//"))

                End If

                Dim parts As String() = line.Split("=")

                If line.Contains("=") Then

                    Dim identifier = line.Remove(line.IndexOf("="))

                    Dim value = line.Substring(line.IndexOf("=") + 1)

                    Dictionary.Add(identifier, value)

                Else

                    Log.Print("Line {0} did not contain a valid language entry!", Log.Level.Warning, lineNumber)

                End If

            End While

        End Using

    End Sub

    Public Shared Function GetString(identifier As String)

        Return GetString(identifier, Nothing)

    End Function

    Public Shared Function GetString(identifier As String, ParamArray args() As Object)

        If Dictionary.Keys.Contains(identifier) Then

            If args IsNot Nothing Then

                Return String.Format(Dictionary(identifier), args)

            Else

                Return Dictionary(identifier)

            End If

        Else

            Log.Print("Identifier {0} does not exist in language file!", Log.Level.Warning, identifier)

            Return "[MISSING]"

        End If

    End Function

    Public Shared Function FindString(Identifier As String, LanguageFile As String)
        Using SR As New StreamReader(Directory.GetCurrentDirectory & "\language\" & LanguageFile)
            Dim LineNumber As Integer = 0
            While SR.Peek <> -1
                LineNumber += 1
                Dim Line As String = SR.ReadLine
                If Line.StartsWith(Identifier & "=") And Not Line.StartsWith("#") Then
                    Dim ReturnString = Line.Substring(Identifier.Length + 1)

                    If String.IsNullOrEmpty(ReturnString) Then
                        Log.Print("[Language] Error at line " & LineNumber & ": Entry is empty!", Log.Level.Warning)
                        ErrorOccured = True
                        Exit While
                    End If

                    Return ReturnString.Replace("\n", vbNewLine)
                End If
            End While
        End Using
        If Regex.Matches(Identifier, "\.").Count > 1 Then
            Return Identifier.Split(".")(Identifier.Split(".").Count - 2) & "." & Identifier.Split(".").Last
        Else
            Return Identifier
        End If
    End Function
End Class
