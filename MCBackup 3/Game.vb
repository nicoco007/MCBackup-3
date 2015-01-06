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

Public Class Game
    Public Enum Launcher As Integer
        Minecraft
        Technic
        FeedTheBeast
        ATLauncher
    End Enum

    Public Shared Function LauncherToString(Launcher As Launcher)
        Select Case Launcher
            Case Game.Launcher.Minecraft
                Return "Minecraft"
            Case Game.Launcher.Technic
                Return "Technic"
            Case Game.Launcher.FeedTheBeast
                Return "Feed the Beast"
            Case Game.Launcher.ATLauncher
                Return "ATLauncher"
            Case Else
                Return "Minecraft"
        End Select
    End Function

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
