Public Class Game
    Public Enum Launcher As Integer
        Minecraft
        Technic
        FeedTheBeast
        ATLauncher
    End Enum

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
