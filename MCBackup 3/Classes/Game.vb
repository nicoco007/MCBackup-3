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

Imports MCBackup

Public Class Game
    <Serializable()>
    Public Class Launcher
        Implements IEquatable(Of Launcher)
        Public Shared Minecraft As New Launcher(0, "Minecraft")
        Public Shared Technic As New Launcher(1, "Technic")
        Public Shared FeedTheBeast As New Launcher(2, "Feed the Beast")
        Public Shared ATLauncher As New Launcher(3, "ATLauncher")

        Private counter As Integer = 0

        Private _id As Integer
        Private Property id As Integer
            Get
                Return _id
            End Get
            Set(value As Integer)
                _id = value
            End Set
        End Property

        Private _name As String
        Private Property name As String
            Get
                Return _name
            End Get
            Set(value As String)
                _name = value
            End Set
        End Property

        Public Sub New(id As Integer, name As String)
            Me.id = id
            Me.name = name
        End Sub

        Public Overrides Function ToString() As String
            Return Me.name
        End Function

        Private Overloads Function Equals(other As Launcher) As Boolean Implements IEquatable(Of Launcher).Equals
            If other Is Nothing Then Return False

            Return Me.id = other.id
        End Function

        Public Shared Operator =(launcher1 As Launcher, launcher2 As Launcher)
            If launcher1 Is Nothing OrElse launcher2 Is Nothing Then Return False

            Return launcher1.Equals(launcher2)
        End Operator

        Public Shared Operator <>(launcher1 As Launcher, launcher2 As Launcher)
            If launcher1 Is Nothing OrElse launcher2 Is Nothing Then Return False

            Return launcher1.Equals(launcher2)
        End Operator
    End Class

    Public Class Images
        Public Class Health
            Inherits Image
            Public Sub New(Margin As Thickness, State As State)
                Select Case State
                    Case Images.State.Full
                        Me.Source = New BitmapImage(New Uri("pack://application:,,,/Resources/NBTInfo/heart_full.png"))
                    Case Images.State.Half
                        Me.Source = New BitmapImage(New Uri("pack://application:,,,/Resources/NBTInfo/heart_half.png"))
                    Case Images.State.Empty
                        Me.Source = New BitmapImage(New Uri("pack://application:,,,/Resources/NBTInfo/heart_empty.png"))
                End Select

                Me.Width = 9
                Me.Height = 9
                Me.HorizontalAlignment = Windows.HorizontalAlignment.Left
                Me.VerticalAlignment = Windows.VerticalAlignment.Stretch
                Me.Stretch = Windows.Media.Stretch.None
                Me.Margin = Margin
            End Sub
        End Class

        Public Class Hunger
            Inherits Image
            Public Sub New(Margin As Thickness, State As State)
                Select Case State
                    Case Images.State.Full
                        Me.Source = New BitmapImage(New Uri("pack://application:,,,/Resources/NBTInfo/hunger_full.png"))
                    Case Images.State.Half
                        Me.Source = New BitmapImage(New Uri("pack://application:,,,/Resources/NBTInfo/hunger_half.png"))
                    Case Images.State.Empty
                        Me.Source = New BitmapImage(New Uri("pack://application:,,,/Resources/NBTInfo/hunger_empty.png"))
                End Select

                Me.Width = 9
                Me.Height = 9
                Me.HorizontalAlignment = Windows.HorizontalAlignment.Left
                Me.VerticalAlignment = Windows.VerticalAlignment.Stretch
                Me.Stretch = Windows.Media.Stretch.None
                Me.Margin = Margin
            End Sub
        End Class

        Public Enum State
            Full
            Half
            Empty
        End Enum
    End Class
End Class
