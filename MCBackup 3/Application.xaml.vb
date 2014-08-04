Class Application
    Public Enum AppCloseAction
        Ask
        Close
        Force
    End Enum

    Private Shared _CloseAction As AppCloseAction
    Public Shared Property CloseAction As AppCloseAction
        Get
            Return _CloseAction
        End Get
        Set(value As AppCloseAction)
            _CloseAction = value
        End Set
    End Property

    Sub New()
        CloseAction = AppCloseAction.Ask
    End Sub
End Class
