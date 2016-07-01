'   ╔═══════════════════════════════════════════════════════════════════════════╗
'   ║                      Copyright © 2013-2016 nicoco007                      ║
'   ║                                                                           ║
'   ║      Licensed under the Apache License, Version 2.0 (the "License");      ║
'   ║      you may not use this file except in compliance with the License.     ║
'   ║                  You may obtain a copy of the License at                  ║
'   ║                                                                           ║
'   ║                 http://www.apache.org/licenses/LICENSE-2.0                ║
'   ║                                                                           ║
'   ║    Unless required by applicable law or agreed to in writing, software    ║
'   ║     distributed under the License is distributed on an "AS IS" BASIS,     ║
'   ║  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. ║
'   ║     See the License for the specific language governing permissions and   ║
'   ║                      limitations under the License.                       ║
'   ╚═══════════════════════════════════════════════════════════════════════════╝

Public Class AboutDialog
    Private Main As MainWindow = DirectCast(Application.Current.MainWindow, MainWindow)

    Private Sub AboutWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles MyBase.Loaded
        Me.Title = Application.Language.GetString("About")
        TextBlock1.Text = String.Format(Application.Language.GetString("MCBackup Single Player\nVersion {0}\n\nCopyright © 2013-2016 nicoco007\n\nLicensed under the Apache License, Version 2.0 (the License);\nyou may not use this file except in compliance with the License.\nYou may obtain a copy of the License at\n\nhttp://www.apache.org/licenses/LICENSE-2.0\n\nUnless required by applicable law or agreed to in writing, software\ndistributed under the License is distributed on an AS IS BASIS,\nWITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.\nSee the License for the specific language governing permissions and\nlimitations under the License.\n\nMCBackup also uses the following 3rd party software:\nJSON.NET - Licensed under the Microsoft Public License\nMahApps.Metro - Licensed under the Microsoft Public License\nMCMap - License-free\nModernUIIcons - Licensed under the Creative Commons BY-ND 3.0 License\nSubstrate - Licensed under the MIT License"), MainWindow.ApplicationVersion)
    End Sub

    Private Sub Window_ContentRendered(sender As Object, e As EventArgs) Handles MyBase.ContentRendered
        ' SizeToContent Black Border Fix © nicoco007
        Dim s As New Size(Me.Width, Me.Height)
        Me.SizeToContent = SizeToContent.Manual
        Me.Width = s.Width
        Me.Height = s.Height
    End Sub
End Class
