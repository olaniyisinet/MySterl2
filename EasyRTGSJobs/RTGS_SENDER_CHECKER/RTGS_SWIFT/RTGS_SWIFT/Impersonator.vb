Imports System.Security.Principal
Imports System.Runtime.InteropServices
Imports System.ComponentModel

Public Class Impersonator
    Implements IDisposable


    ' ------------------------------------------------------------------

    ''' <summary>
    ''' Constructor. Starts the impersonation with the given credentials.
    ''' Please note that the account that instantiates the Impersonator class
    ''' needs to have the 'Act as part of operating system' privilege set.
    ''' </summary>
    ''' <param name="userName">The name of the user to act as.</param>
    ''' <param name="domainName">The domain name of the user to act as.</param>
    ''' <param name="password">The password of the user to act as.</param>
    Public Sub New(ByVal userName As String, ByVal domainName As String, ByVal password As String)
        ImpersonateValidUser(userName, domainName, password)
    End Sub



    Public Sub Dispose() Implements System.IDisposable.Dispose
        UndoImpersonation()
    End Sub





    <DllImport("advapi32.dll", SetLastError:=True)> _
    Private Shared Function LogonUser(ByVal lpszUserName As String, ByVal lpszDomain As String, ByVal lpszPassword As String, ByVal dwLogonType As Integer, ByVal dwLogonProvider As Integer, ByRef phToken As IntPtr) As Integer
    End Function

    <DllImport("advapi32.dll", CharSet:=CharSet.Auto, SetLastError:=True)> _
    Private Shared Function DuplicateToken(ByVal hToken As IntPtr, ByVal impersonationLevel As Integer, ByRef hNewToken As IntPtr) As Integer
    End Function

    <DllImport("advapi32.dll", CharSet:=CharSet.Auto, SetLastError:=True)> _
    Private Shared Function RevertToSelf() As Boolean
    End Function

    <DllImport("kernel32.dll", CharSet:=CharSet.Auto)> _
    Private Shared Function CloseHandle(ByVal handle As IntPtr) As Boolean
    End Function

    Private Const LOGON32_LOGON_INTERACTIVE As Integer = 2
    Private Const LOGON32_PROVIDER_DEFAULT As Integer = 0
    Private Const LOGON32_LOGON_NEW_CREDENTIALS As Integer = 9
    Private Const LOGON32_PROVIDER_WINNT50 As Integer = 3





    ''' <summary>
    ''' Does the actual impersonation.
    ''' </summary>
    ''' <param name="userName">The name of the user to act as.</param>
    ''' <param name="domainName">The domain name of the user to act as.</param>
    ''' <param name="password">The password of the user to act as.</param>
    Private Sub ImpersonateValidUser(ByVal userName As String, ByVal domain As String, ByVal password As String)
        Dim tempWindowsIdentity As WindowsIdentity = Nothing
        Dim token As IntPtr = IntPtr.Zero
        Dim tokenDuplicate As IntPtr = IntPtr.Zero

        Try
            If RevertToSelf() Then
                If LogonUser(userName, domain, password, LOGON32_LOGON_NEW_CREDENTIALS, LOGON32_PROVIDER_WINNT50, token) <> 0 Then
                    If DuplicateToken(token, 2, tokenDuplicate) <> 0 Then
                        tempWindowsIdentity = New WindowsIdentity(tokenDuplicate)
                        impersonationContext = tempWindowsIdentity.Impersonate()
                    Else
                        Throw New Win32Exception(Marshal.GetLastWin32Error())
                    End If
                Else
                    Throw New Win32Exception(Marshal.GetLastWin32Error())
                End If
            Else
                Throw New Win32Exception(Marshal.GetLastWin32Error())
            End If
        Finally
            If token <> IntPtr.Zero Then
                CloseHandle(token)
            End If
            If tokenDuplicate <> IntPtr.Zero Then
                CloseHandle(tokenDuplicate)
            End If
        End Try
    End Sub

    ''' <summary>
    ''' Reverts the impersonation.
    ''' </summary>
    Private Sub UndoImpersonation()
        If impersonationContext IsNot Nothing Then
            impersonationContext.Undo()
        End If
    End Sub

    Private impersonationContext As WindowsImpersonationContext = Nothing

End Class
