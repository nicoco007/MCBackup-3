<AttributeUsage(AttributeTargets.Field)>
Public Class StringValue
    Inherits Attribute
    Private _Name As String
    Public ReadOnly Property Name As String
        Get
            Return _Name
        End Get
    End Property

    Public Sub New(Name As String)
        _Name = Name
    End Sub
End Class