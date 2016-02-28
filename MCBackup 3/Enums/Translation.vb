<AttributeUsage(AttributeTargets.Field)>
Public Class Translation
    Inherits Attribute
    Private _Key As String
    Public ReadOnly Property Key As String
        Get
            Return _Key
        End Get
    End Property

    Public Sub New(Key As String)
        _Key = Key
    End Sub
End Class