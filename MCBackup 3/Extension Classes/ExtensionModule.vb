'   ╔═══════════════════════════════════════════════════════════════════════════╗
'   ║                        Copyright © 2014 nicoco007                         ║
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

Imports System.Windows.Threading

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
End Module
