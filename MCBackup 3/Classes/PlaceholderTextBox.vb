Imports System.Globalization

Public Class PlaceholderTextBox
    Inherits TextBox

    ''' <summary>
    ''' Keeps track of whether placeholder text is visible to know when to call InvalidateVisual to show or hide it.
    ''' </summary>

    Private _isPlaceholderVisible As Boolean

    ''' <summary>
    ''' Identifies the PlaceholderText dependency property.
    ''' </summary>
    Public Shared ReadOnly PlaceholderTextProperty As DependencyProperty = DependencyProperty.Register("PlaceholderText", GetType(String), GetType(TextBox), New FrameworkPropertyMetadata(String.Empty, FrameworkPropertyMetadataOptions.AffectsRender))

    ''' <summary>        
    ''' Gets or sets the placeholder text to be shown when text box has no text and is not in focus. This is a dependency property.
    ''' </summary>
    Public Property PlaceholderText As String
        Get
            Return DirectCast(GetValue(PlaceholderTextProperty), String)
        End Get

        Set
            SetValue(PlaceholderTextProperty, Value)
        End Set
    End Property

    ''' <summary>
    '''   Identifies the Background dependency property.
    ''' </summary>
    Public Shared Shadows ReadOnly BackgroundProperty As DependencyProperty = DependencyProperty.Register("Background", GetType(Brush), GetType(TextBox), New FrameworkPropertyMetadata(Nothing, FrameworkPropertyMetadataOptions.AffectsRender))

    ''' <summary>
    '''   Gets or sets a brush that describes the background of a control. This is a  dependency property.
    ''' </summary>
    Public Shadows Property Background As Brush
        Get
            Return TryCast(GetValue(BackgroundProperty), Brush)
        End Get

        Set
            SetValue(BackgroundProperty, Value)
        End Set
    End Property

    ''' <summary>
    ''' Raises the Initialized event. This method is invoked whenever IsInitialized is set to true internally.
    ''' </summary>
    ''' <param name="e">The EventArgs that contains the event data.</param>
    Protected Overrides Sub OnInitialized(e As EventArgs)
        MyBase.OnInitialized(e)

        If Background Is Nothing Then

            Background = MyBase.Background
        End If

        MyBase.Background = Nothing
    End Sub

    ''' <summary>
    ''' Called when one or more of the dependency properties that exist on the element have had their effective values changed.
    ''' (Overrides FrameworkElement.OnPropertyChanged(DependencyPropertyChangedEventArgs).)
    ''' </summary>
    ''' <param name="e">The DependencyPropertyChangedEventArgs that contains the event data.</param>
    Protected Overrides Sub OnPropertyChanged(e As DependencyPropertyChangedEventArgs)
        If (e.[Property] Is IsFocusedProperty OrElse e.[Property] Is TextProperty) AndAlso Not String.IsNullOrEmpty(PlaceholderText) Then

            If Not IsFocused AndAlso String.IsNullOrEmpty(Text) Then
                If Not _isPlaceholderVisible Then
                    InvalidateVisual()
                End If

            ElseIf _isPlaceholderVisible Then
                InvalidateVisual()
            End If
        End If

        MyBase.OnPropertyChanged(e)
    End Sub

    ''' <summary>
    ''' When overridden in a derived class, participates in rendering operations that are directed by the layout system.
    ''' The rendering instructions for this element are not used directly when this method is invoked, and are instead
    ''' preserved for later asynchronous use by layout and drawing.
    ''' </summary>
    ''' <param name="drawingContext">The drawing instructions for a specific element. This context is provided to the layout system.</param>
    Protected Overrides Sub OnRender(drawingContext As DrawingContext)
        MyBase.OnRender(drawingContext)
        _isPlaceholderVisible = False
        drawingContext.DrawRectangle(Background, Nothing, New Rect(RenderSize))

        If Not IsFocused AndAlso String.IsNullOrEmpty(Text) AndAlso Not String.IsNullOrEmpty(PlaceholderText) Then

            _isPlaceholderVisible = True

            Dim computedAlignment As TextAlignment = ComputedTextAlignment()

            Dim foreground As Brush = SystemColors.GrayTextBrush.Clone()

            foreground.Opacity = foreground.Opacity

            Dim typeface As New Typeface(FontFamily, Me.FontStyle, FontWeight, FontStretch)

            Dim formattedText As New FormattedText(PlaceholderText, CultureInfo.CurrentCulture, FlowDirection, typeface, FontSize, foreground)

            formattedText.TextAlignment = computedAlignment

            formattedText.MaxTextHeight = RenderSize.Height - BorderThickness.Top - BorderThickness.Bottom - Padding.Top - Padding.Bottom

            formattedText.MaxTextWidth = RenderSize.Width - BorderThickness.Left - BorderThickness.Right - Padding.Left - Padding.Right - 4.0

            Dim left As Double
            Dim top As Double = 0.0

            If FlowDirection = FlowDirection.RightToLeft Then
                left = BorderThickness.Right + Padding.Right + 2.0
            Else
                left = BorderThickness.Left + Padding.Left + 2.0
            End If

            Select Case VerticalContentAlignment
                Case VerticalAlignment.Top, VerticalAlignment.Stretch
                    top = BorderThickness.Top + Padding.Top
                    Exit Select

                Case VerticalAlignment.Bottom
                    top = RenderSize.Height - BorderThickness.Bottom - Padding.Bottom - formattedText.Height
                    Exit Select

                Case VerticalAlignment.Center
                    top = (RenderSize.Height + BorderThickness.Top - BorderThickness.Bottom + Padding.Top - Padding.Bottom - formattedText.Height) / 2.0
                    Exit Select

            End Select

            If FlowDirection = FlowDirection.RightToLeft Then
                drawingContext.PushTransform(New ScaleTransform(-1.0, 1.0, RenderSize.Width / 2.0, 0.0))
                drawingContext.DrawText(formattedText, New Point(left, top))
                drawingContext.Pop()
            Else
                drawingContext.DrawText(formattedText, New Point(left, top))
            End If
        End If

    End Sub

    ''' <summary>
    ''' Computes changes in text alignment caused by HorizontalContentAlignment. TextAlignment has priority over HorizontalContentAlignment.
    ''' </summary>
    ''' <returns>Returns the effective text alignment.</returns>
    Private Function ComputedTextAlignment() As TextAlignment
        If DependencyPropertyHelper.GetValueSource(Me, TextBox.HorizontalContentAlignmentProperty).BaseValueSource = BaseValueSource.Local AndAlso DependencyPropertyHelper.GetValueSource(Me, TextBox.TextAlignmentProperty).BaseValueSource <> BaseValueSource.Local Then
            Select Case HorizontalContentAlignment
                Case HorizontalAlignment.Left
                    Return TextAlignment.Left

                Case HorizontalAlignment.Right
                    Return TextAlignment.Right

                Case HorizontalAlignment.Center
                    Return TextAlignment.Center

                Case HorizontalAlignment.Stretch
                    Return TextAlignment.Justify

            End Select
        End If

        Return TextAlignment
    End Function
End Class