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