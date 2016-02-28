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

Module ExtensionModule
    Private EmptyDelegate As Action = Sub()
                                      End Sub

    <Runtime.CompilerServices.Extension> Public Sub Refresh(uiElement As UIElement)
        uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate)
    End Sub

    <Runtime.CompilerServices.Extension> Public Function GetStringValue(TheEnum As Object) As String
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

    <Runtime.CompilerServices.Extension> Public Function GetTranslation(TheEnum As Object) As String
        If TheEnum >= 0 Then
            Dim MemberInfo As MemberInfo() = TheEnum.GetType().GetMember(TheEnum.ToString)
            If MemberInfo IsNot Nothing And MemberInfo.Length > 0 Then
                Dim Attr As Translation = Attribute.GetCustomAttribute(MemberInfo(0), GetType(Translation))
                If Attr IsNot Nothing Then
                    Return Language.GetString(Attr.Key)
                Else
                    Return TheEnum.ToString()
                End If
            Else
                Return TheEnum.ToString()
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
