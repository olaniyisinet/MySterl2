﻿Imports System.Web
Imports System.Web.Services
Imports System.Web.Services.Protocols

' To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line.
' <System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class easyrtgsapi
     Inherits System.Web.Services.WebService

    <WebMethod()> _
    Public Function doNameEnquiry(ByVal bankCode As String, ByVal accountno As String) As String
        Dim response As String = String.Empty
        Dim gad As New Gadget
        Dim name As String = gad.DoNameEnquiryOutput(bankCode, accountno)
        Dim dict As Dictionary(Of String, String) = gad.parseXmlResponse(name)


        If dict.ContainsKey("RESPONSECODE") AndAlso dict.ContainsKey("RESPONSETEXT") Then
            Dim code As String = dict("RESPONSECODE")
            Dim nameval As String = dict("RESPONSETEXT")

            If Not String.IsNullOrEmpty(code) AndAlso code.Trim().CompareTo("00") = 0 Then

                Return nameval

            Else
                Return "-1"
            End If
        Else
            Return "-1"
        End If
        Return response
    End Function

End Class