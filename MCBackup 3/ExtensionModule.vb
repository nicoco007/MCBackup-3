Imports System.Windows.Threading

Module ExtensionModule
    Private EmptyDelegate As Action = Sub()
                                      End Sub

    <System.Runtime.CompilerServices.Extension> Public Sub Refresh(uiElement As UIElement)
        uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate)
    End Sub
End Module
