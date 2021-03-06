' ------------------------------------------------------------------
' The FolderSelectDialog and Reflector classes are not of my doing.
' They are based on the Front-End for DOSBox project:
' http://code.google.com/p/fed/
' All credit goes to the author.
' ------------------------------------------------------------------

Imports System.Windows.Forms
Imports System.Windows.Threading

' ------------------------------------------------------------------
' Wraps System.Windows.Forms.OpenFileDialog to make it present
' a vista-style dialog.
' ------------------------------------------------------------------

''' <summary>
''' Wraps System.Windows.Forms.OpenFileDialog to make it present
''' a vista-style dialog.
''' </summary>
Public Class FolderSelectDialog
    ' Wrapped dialog
    Private ofd As System.Windows.Forms.OpenFileDialog = Nothing

    ''' <summary>
    ''' Default constructor
    ''' </summary>
    Public Sub New()
        ofd = New System.Windows.Forms.OpenFileDialog()

        ofd.Filter = "Folders|" & vbLf
        ofd.AddExtension = False
        ofd.CheckFileExists = False
        ofd.DereferenceLinks = True
        ofd.Multiselect = False
    End Sub

#Region "Properties"

    ''' <summary>
    ''' Gets/Sets the initial folder to be selected. A null value selects the current directory.
    ''' </summary>
    Public Property InitialDirectory() As String
        Get
            Return ofd.InitialDirectory
        End Get
        Set(value As String)
            ofd.InitialDirectory = If(value Is Nothing OrElse value.Length = 0, Environment.CurrentDirectory, value)
        End Set
    End Property

    ''' <summary>
    ''' Gets/Sets the title to show in the dialog
    ''' </summary>
    Public Property Title() As String
        Get
            Return ofd.Title
        End Get
        Set(value As String)
            ofd.Title = If(value Is Nothing, "Select a folder", value)
        End Set
    End Property

    ''' <summary>
    ''' Gets the selected folder
    ''' </summary>
    Public ReadOnly Property FolderName() As String
        Get
            Return ofd.FileName
        End Get
    End Property

#End Region

#Region "Methods"

    ''' <summary>
    ''' Shows the dialog
    ''' </summary>
    ''' <returns>True if the user presses OK else false</returns>
    Public Function ShowDialog() As DialogResult
        Return ShowDialog(IntPtr.Zero)
    End Function

    ''' <summary>
    ''' Shows the dialog
    ''' </summary>
    ''' <param name="hWndOwner">Handle of the control to be parent</param>
    ''' <returns>True if the user presses OK else false</returns>
    Public Function ShowDialog(hWndOwner As IntPtr) As DialogResult

        If Environment.OSVersion.Version.Major >= 6 Then
            Dim r = New Reflector("System.Windows.Forms")

            Dim num As UInteger = 0
            Dim typeIFileDialog As Type = r.[GetType]("FileDialogNative.IFileDialog")
            Dim dialog As Object = r.[Call](ofd, "CreateVistaDialog")
            r.[Call](ofd, "OnBeforeVistaDialog", dialog)

            Dim options As UInteger = CUInt(r.CallAs(GetType(System.Windows.Forms.FileDialog), ofd, "GetOptions"))
            options = options Or CUInt(r.GetEnum("FileDialogNative.FOS", "FOS_PICKFOLDERS"))
            r.CallAs(typeIFileDialog, dialog, "SetOptions", options)

            Dim pfde As Object = r.[New]("FileDialog.VistaDialogEvents", ofd)
            Dim parameters As Object() = New Object() {pfde, num}
            Try
                r.CallAs2(typeIFileDialog, dialog, "Advise", parameters)
            Catch ex As Exception
                ErrorReportDialog.ShowDialog("Could not render FolderBrowserDialog.", ex)
            End Try
            num = CUInt(parameters(1))
            Try
                Dim result As Integer = CInt(r.CallAs(typeIFileDialog, dialog, "Show", hWndOwner))
                If result = 0 Then
                    Return DialogResult.OK
                Else
                    Return DialogResult.Cancel
                End If
            Finally
                r.CallAs(typeIFileDialog, dialog, "Unadvise", num)
                GC.KeepAlive(pfde)
            End Try
        Else
            Dim fbd = New FolderBrowserDialog()
            fbd.Description = Me.Title
            fbd.SelectedPath = Me.InitialDirectory
            fbd.ShowNewFolderButton = False
            If fbd.ShowDialog(New WindowWrapper(hWndOwner)) <> DialogResult.OK Then
                Return DialogResult.Cancel
            End If
            ofd.FileName = fbd.SelectedPath
            Return DialogResult.OK
        End If
    End Function

#End Region
End Class

''' <summary>
''' Creates IWin32Window around an IntPtr
''' </summary>
Public Class WindowWrapper
    Implements System.Windows.Forms.IWin32Window
    ''' <summary>
    ''' Constructor
    ''' </summary>
    ''' <param name="handle">Handle to wrap</param>
    Public Sub New(handle As IntPtr)
        _hwnd = handle
    End Sub

    ''' <summary>
    ''' Original ptr
    ''' </summary>
    Public ReadOnly Property Handle() As IntPtr Implements IWin32Window.Handle
        Get
            Return _hwnd
        End Get
    End Property

    Private _hwnd As IntPtr
End Class