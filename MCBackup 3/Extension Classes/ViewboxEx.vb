Public Class ViewboxEx
    Inherits Viewbox

    Sub New(Content As String, Stretch As Stretch, StretchDirection As StretchDirection)
        Dim ContentControl As New ContentControl
        ContentControl.Content = Content
        Me.Child = ContentControl

        Me.Stretch = Stretch
        Me.StretchDirection = StretchDirection
    End Sub
End Class