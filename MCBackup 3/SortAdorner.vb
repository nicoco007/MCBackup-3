Imports System.ComponentModel
Imports MahApps.Metro

Public Class SortAdorner
    Inherits Adorner
    Private Shared ReadOnly _AscGeometry As Geometry = Geometry.Parse("M 0,0 L 10,0 L 5,5 Z")

    Private Shared ReadOnly _DescGeometry As Geometry = Geometry.Parse("M 0,5 L 10,5 L 5,0 Z")

    Public Property Direction() As ListSortDirection
        Get
            Return m_Direction
        End Get
        Private Set(value As ListSortDirection)
            m_Direction = value
        End Set
    End Property
    Private m_Direction As ListSortDirection

    Public Sub New(element As UIElement, dir As ListSortDirection)
        MyBase.New(element)
        Direction = dir
    End Sub

    Protected Overrides Sub OnRender(drawingContext As DrawingContext)
        MyBase.OnRender(drawingContext)

        If AdornedElement.RenderSize.Width < 20 Then
            Return
        End If

        drawingContext.PushTransform(New TranslateTransform(AdornedElement.RenderSize.Width - 15, (AdornedElement.RenderSize.Height - 5) \ 2))

        drawingContext.DrawGeometry(Brushes.Black, Nothing, If(Direction = ListSortDirection.Ascending, _AscGeometry, _DescGeometry))

        drawingContext.Pop()
    End Sub
End Class