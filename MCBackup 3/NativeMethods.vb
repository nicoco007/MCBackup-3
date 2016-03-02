Imports System.Runtime.InteropServices

Public Class NativeMethods
    <DllImport("user32.dll")>
    Public Shared Function SetForegroundWindow(ByVal hWnd As IntPtr) As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function

    <DllImport("user32.dll")>
    Public Shared Function ShowWindow(ByVal hWnd As IntPtr, ByVal nCmdShow As Integer) As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function
End Class
