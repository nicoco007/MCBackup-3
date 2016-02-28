Public Class BackupInfo
    Private _Name As String
    Public Property Name As String
        Get
            Return _Name
        End Get
        Set(value As String)
            _Name = value
        End Set
    End Property

    Private _Location As String
    Public Property Location As String
        Get
            Return _Location
        End Get
        Set(value As String)
            _Location = value
        End Set
    End Property

    Private _Description As String
    Public Property Description As String
        Get
            Return _Description
        End Get
        Set(value As String)
            _Description = value
        End Set
    End Property

    Private _Type As String
    Public Property Type As String
        Get
            Return _Type
        End Get
        Set(value As String)
            _Type = value
        End Set
    End Property

    Private _Group As String
    Public Property Group As String
        Get
            Return _Group
        End Get
        Set(value As String)
            _Group = value
        End Set
    End Property

    Private _Launcher As Launcher
    Public Property Launcher As Launcher
        Get
            Return _Launcher
        End Get
        Set(value As Launcher)
            _Launcher = value
        End Set
    End Property

    Private _Modpack As String
    Public Property Modpack As String
        Get
            Return _Modpack
        End Get
        Set(value As String)
            _Modpack = value
        End Set
    End Property
End Class