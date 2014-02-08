Imports System.Windows.Threading
Imports System.Threading

Public Class Splash
    Private Main As MainWindow = DirectCast(Application.Current.MainWindow, MainWindow)

    Sub New()
        InitializeComponent()

        VersionLabel.Content = "MCBackup v" & Main.ApplicationVersion
    End Sub
End Class
