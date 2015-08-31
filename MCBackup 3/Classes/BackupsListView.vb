Public Class BackupsListView
    Enum SortBy As Integer
        Name
        DateCreated
        Type
    End Enum

    Enum GroupBy As Integer
        [Nothing]
        OriginalName
        Type
    End Enum
End Class
