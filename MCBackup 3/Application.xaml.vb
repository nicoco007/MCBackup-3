'   ╔═══════════════════════════════════════════════════════════════════════════╗
'   ║                      Copyright © 2013-2015 nicoco007                      ║
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

Imports System.Threading

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

        InitializeComponent()

        CloseAction = AppCloseAction.Force

    End Sub

    <STAThread>
    Public Shared Sub Main()
        Dim createdNew As Boolean
        Dim mutex As New Mutex(True, "{f5d1ae04-b456-4a7d-a1ea-19f45838908e}", createdNew)

        If Not createdNew Then
            Dim currentProcess As Process = Process.GetCurrentProcess()
            Dim processes As Process() = Process.GetProcessesByName(currentProcess.ProcessName)

            For Each p As Process In processes
                If (p.MainWindowHandle <> IntPtr.Zero) Then
                    NativeMethods.ShowWindow(p.MainWindowHandle, 1)
                    NativeMethods.SetForegroundWindow(p.MainWindowHandle)
                End If
            Next
        Else
            Dim app As New Application()
            Dim mainWindow As New MainWindow()
            app.Run(mainWindow)
        End If
    End Sub
End Class
