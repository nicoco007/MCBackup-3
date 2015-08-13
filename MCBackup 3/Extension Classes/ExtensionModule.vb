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
Imports System.Drawing
Imports System.Reflection
Imports System.Windows.Threading
Imports MCBackup.Game

Module ExtensionModule
    Private EmptyDelegate As Action = Sub()
                                      End Sub

    <System.Runtime.CompilerServices.Extension> Public Sub Refresh(uiElement As UIElement)
        uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate)
    End Sub

    <System.Runtime.CompilerServices.Extension> Public Function DeleteAsync(Directory As IO.DirectoryInfo)
        Dim Thread As New System.Threading.Thread(Sub()
                                                      IO.Directory.Delete(Directory.FullName, True)
                                                  End Sub)
        Thread.Start()
        Return Thread
    End Function

    <System.Runtime.CompilerServices.Extension> Public Function DeleteContentsAsync(Directory As IO.DirectoryInfo)
        Dim Thread As New System.Threading.Thread(Sub()
                                                      For Each File As IO.FileInfo In Directory.GetFiles
                                                          IO.File.Delete(File.FullName)
                                                      Next

                                                      For Each Folder As IO.DirectoryInfo In Directory.GetDirectories
                                                          IO.Directory.Delete(Folder.FullName, True)
                                                      Next
                                                  End Sub)
        Thread.Start()
        Return Thread
    End Function

    <System.Runtime.CompilerServices.Extension> Public Function GetStringValue(TheEnum As Game.Launcher) As String
        If TheEnum >= 0 Then
            Dim MemberInfo As MemberInfo() = TheEnum.GetType().GetMember(TheEnum.ToString)
            If MemberInfo IsNot Nothing And MemberInfo.Length > 0 Then
                Dim Attr As StringValue = Attribute.GetCustomAttribute(MemberInfo(0), GetType(StringValue))
                If Attr IsNot Nothing Then
                    Return Attr.Name
                Else
                    Return TheEnum.ToString()
                End If
            Else
                Return TheEnum.ToString()
            End If
        End If
        Return Nothing
    End Function

    <System.Runtime.CompilerServices.Extension> Public Function ToImageSource(Icon As Icon) As ImageSource
        Dim Bitmap As Bitmap = Icon.ToBitmap()
        Dim Hbitmap As IntPtr = Bitmap.GetHbitmap()

        Return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(Hbitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions())
    End Function
End Module
