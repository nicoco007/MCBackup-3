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

Public Class ListViewBackupItem
    Private m_Name As String
    Public Property Name() As String
        Get
            Return m_Name
        End Get
        Set(value As String)
            m_Name = value
        End Set
    End Property

    Private m_DateCreated As String
    Public Property DateCreated() As String
        Get
            Return m_DateCreated
        End Get
        Set(value As String)
            m_DateCreated = value
        End Set
    End Property

    Private m_Color As SolidColorBrush
    Public Property Color() As SolidColorBrush
        Get
            Return m_Color
        End Get
        Set(value As SolidColorBrush)
            m_Color = value
        End Set
    End Property

    Private m_OriginalName As String
    Public Property OriginalName() As String
        Get
            Return m_OriginalName
        End Get
        Set(value As String)
            m_OriginalName = value
        End Set
    End Property

    Private m_Type As String
    Public Property Type() As String
        Get
            Return m_Type
        End Get
        Set(value As String)
            m_Type = value
        End Set
    End Property

    Public Sub New(Name As String, DateCreated As String, Color As SolidColorBrush, OriginalName As String, Type As String)
        Me.Name = Name
        Me.DateCreated = DateCreated
        Me.Color = Color
        Me.OriginalName = OriginalName
        Me.Type = Type
    End Sub
End Class