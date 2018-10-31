
Public Class FtpFilenameComparer
    Implements IComparer(Of String)

    Public Function Compare(ByVal x As String, ByVal y As String) As Integer Implements System.Collections.Generic.IComparer(Of String).Compare

        Dim partX() As String = x.Trim().Split("."c)
        Dim partY() As String = y.Trim().Split("."c)

        Dim compareVal As Integer = 0

        If partX.Length = 2 AndAlso partY.Length = 2 Then
            compareVal = partY(0).CompareTo(partX(0)) 'partX(0).CompareTo(partY(0))

        End If

        Return compareVal
    End Function
End Class
