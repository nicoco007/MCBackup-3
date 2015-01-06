Imports Microsoft.WindowsAPICodePack.Taskbar

Public Class Progress
    Private Shared MainWindow As MainWindow = DirectCast(Application.Current.MainWindow, MainWindow)

    Public Shared Property Value As Double
        Get
            Return MainWindow.ProgressBar.Value
        End Get
        Set(value As Double)
            MainWindow.ProgressBar.Value = value
            If Environment.OSVersion.Version.Major > 5 Then
                If value >= MainWindow.ProgressBar.Maximum Then
                    TaskbarManager.Instance.SetProgressValue(0, MainWindow.ProgressBar.Maximum)
                Else
                    TaskbarManager.Instance.SetProgressValue(value, MainWindow.ProgressBar.Maximum)
                End If
            End If
        End Set
    End Property

    Public Shared Property IsIndeterminate As Boolean
        Get
            Return MainWindow.ProgressBar.IsIndeterminate
        End Get
        Set(value As Boolean)
            MainWindow.ProgressBar.IsIndeterminate = value
            If Environment.OSVersion.Version.Major > 5 Then
                If value Then TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Indeterminate) Else TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress)
            End If
        End Set
    End Property

    Public Shared Property Maximum As Double
        Get
            Return MainWindow.ProgressBar.Maximum
        End Get
        Set(value As Double)
            MainWindow.ProgressBar.Maximum = value
        End Set
    End Property
End Class
