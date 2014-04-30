Public Class TaggedComboBoxItem
    Private m_Name As String
    Public Property Name() As String
        Get
            Return m_Name
        End Get
        Set(value As String)
            m_Name = value
        End Set
    End Property

    Private m_Tag As String
    Public Property Tag() As String
        Get
            Return m_Tag
        End Get
        Set(value As String)
            m_Tag = value
        End Set
    End Property

    Public Sub New(Name As String, Tag As String)
        Me.Name = Name
        Me.Tag = Tag
    End Sub
End Class