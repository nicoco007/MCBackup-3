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

Imports System.Drawing
Imports System.Reflection
Imports System.Windows.Threading

Module ExtensionModule
    Private EmptyDelegate As Action = Sub()
                                      End Sub

    <Runtime.CompilerServices.Extension> Public Sub Refresh(uiElement As UIElement)
        uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate)
    End Sub

    <Runtime.CompilerServices.Extension> Public Function GetString(theEnum As Object) As String
        If theEnum >= 0 Then
            Dim memberInfo As MemberInfo() = theEnum.GetType().GetMember(theEnum.ToString)
            If memberInfo IsNot Nothing And memberInfo.Length > 0 Then
                Dim attr As StringValue = Attribute.GetCustomAttribute(memberInfo(0), GetType(StringValue))
                If attr IsNot Nothing Then
                    Return attr.Name
                Else
                    Return theEnum.ToString()
                End If
            Else
                Return theEnum.ToString()
            End If
        End If
        Return Nothing
    End Function

    <Runtime.CompilerServices.Extension> Public Function ToImageSource(Icon As Icon) As ImageSource
        Dim Bitmap As Bitmap = Icon.ToBitmap()
        Dim Hbitmap As IntPtr = Bitmap.GetHbitmap()

        Return Interop.Imaging.CreateBitmapSourceFromHBitmap(Hbitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions())
    End Function
End Module
