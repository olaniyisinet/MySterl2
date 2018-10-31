Imports System
Imports System.Configuration
Imports System.IO
Imports System.Net
Imports System.Text
Imports Newtonsoft.Json


Class iMalWebservice
    'Public iMAL As String = "http://10.0.41.102:812/api/Imal/"


    Private Function GetImalURL() As String
        Dim Path As String = "imal.txt"
        Dim iMAL As String = String.Empty

        Try
            iMAL = File.ReadAllText(Path)

        Catch ex As Exception
            LogException(ex)
            iMAL = "http://10.0.41.102:812/api/Imal/"
        End Try

        Return iMAL
    End Function

    Public Function localTransfer(ByVal from As String, ByVal toAcc As String, ByVal amount As Double, ByVal paymentRef As String) As String

        Dim resp = JsonConvert.DeserializeObject(Of AccountDetailsResp)(getAccountDetails(toAcc))

        Dim jsonRequest = New LocalFTPRequest With {
            .fromAccount = from.Trim,
            .toAccount = toAcc.Trim,
            .amount = amount,
            .requestCode = "1",'not really needed
            .principalIdentifier = GenerateRndNumber(6) + DateTime.Now.ToString("HH"),
            .referenceCode = GenerateRndNumber(6) + DateTime.Now.ToString("HHmmss"),
            .beneficiaryName = resp.name,
            .paymentReference = paymentRef
        }

        Dim json = JsonConvert.SerializeObject(jsonRequest)
        Dim responseMessage = String.Empty
        Dim url = GetImalURL() & "LocalFT"
        Dim apiRequest = CType(WebRequest.Create(url), HttpWebRequest)
        apiRequest.Method = "POST"
        apiRequest.ContentType = "application/json"
        apiRequest.Accept = "application/json"
        Dim data = Encoding.UTF8.GetBytes(json)
        apiRequest.ContentLength = data.Length
        Dim message As String = apiRequest.Method & url & apiRequest.ContentType

        Using stream = apiRequest.GetRequestStream()
            stream.Write(data, 0, data.Length)
        End Using

        Dim response As HttpWebResponse = Nothing

        Try
            response = CType(apiRequest.GetResponse(), HttpWebResponse)

            Using responseStream As Stream = response.GetResponseStream()

                If responseStream IsNot Nothing Then

                    Using reader As StreamReader = New StreamReader(responseStream)
                        responseMessage = RemoveSpecialCharacters(reader.ReadToEnd()).Trim(""""c)
                    End Using
                End If
            End Using

        Catch e As Exception
            responseMessage = "{""errorMessages"":[""" & e.Message.ToString() & """],""errors"":{}}"
        End Try

        Console.WriteLine(responseMessage)
        Return responseMessage
    End Function


    Public Function getAccountDetails(ByVal nuban As String) As String
        Dim jsonRequest = New AccountDetailsRequest With {
        .account = nuban,
        .requestCode = "102",
        .principalIdentifier = "66777",
        .referenceCode = GenerateRndNumber(6) + DateTime.Now.ToString("HHmmss")
    }
        Console.WriteLine("............Request...........")
        Console.WriteLine(jsonRequest.ToString)

        Dim json = JsonConvert.SerializeObject(jsonRequest)
		Dim responseMessage = String.Empty
        Dim url = GetImalURL() & "AccountDetails"
        Dim apiRequest = CType(WebRequest.Create(url), HttpWebRequest)
		apiRequest.Method = "POST"
		apiRequest.ContentType = "application/json"
		apiRequest.Accept = "application/json"
		Dim data = Encoding.UTF8.GetBytes(json)
		apiRequest.ContentLength = data.Length
		Dim message As String = apiRequest.Method + url + apiRequest.ContentType

		Using stream = apiRequest.GetRequestStream()
			stream.Write(data, 0, data.Length)
		End Using

		Dim response As HttpWebResponse = Nothing

		Try
			response = CType(apiRequest.GetResponse(), HttpWebResponse)

			Using responseStream As Stream = response.GetResponseStream()

				If responseStream IsNot Nothing Then

					Using reader As StreamReader = New StreamReader(responseStream)
						responseMessage = RemoveSpecialCharacters(reader.ReadToEnd()).Trim(""""c)
					End Using
				End If
			End Using

		Catch e As Exception
			responseMessage = "{""errorMessages"":[""" & e.Message.ToString() & """],""errors"":{}}"
		End Try

        Console.WriteLine("............Response...........")
        Console.WriteLine(responseMessage)
		Return responseMessage
	End Function

	Public Function GenerateRndNumber(ByVal cnt As Integer) As String
		Dim key2 As String() = {"0", "1", "2", "3", "4", "5", "6", "7", "8", "9"}
		Dim rand1 As Random = New Random()
		Dim txt As String = ""

		For j As Integer = 0 To cnt - 1
			txt += key2(rand1.[Next](0, 9))
		Next

		Return txt
	End Function

    Public Function RemoveSpecialCharacters(ByVal str As String) As String
        Dim sb As StringBuilder = New StringBuilder()

        For Each c As Char In str

            If (c >= "0"c AndAlso c <= "9"c) OrElse (c >= "A"c AndAlso c <= "Z"c) OrElse (c >= "a"c AndAlso c <= "z"c) OrElse c = "/"c OrElse c = """"c OrElse c = ":"c OrElse c = "{"c OrElse c = "}"c OrElse c = ","c OrElse c = " "c OrElse c = "-"c OrElse c = "."c Then
                sb.Append(c)
            End If
        Next

        Return sb.ToString()
    End Function

    Private Sub LogException(ByVal ex As Exception)
        Dim filename As String = getFileNameForToday()

        Try

            If ex IsNot Nothing Then
                Dim targetSite As String = ""
                Dim message As String = ""
                Dim stackTrace As String = ""
                Dim source As String = ""
                Dim trace As String = ""

                Try
                    message = ex.Message
                    targetSite = ex.TargetSite.Name
                    stackTrace = ex.StackTrace
                    source = ex.Source
                    trace = ex.StackTrace
                Catch exc As Exception
                End Try

                Dim innerException As Exception = ex.InnerException
                File.AppendAllText(filename, "[Date]>> " & DateTime.Now & Constants.vbCrLf & "[Method Name]>> " & targetSite & Constants.vbCrLf & "[Message]>> " & message & Constants.vbCrLf & "[Source]>> " & source & Constants.vbCrLf & "[Stack Trace]>> " & trace & Constants.vbCrLf)

                If innerException IsNot Nothing Then
                    LogException(innerException)
                Else
                    File.AppendAllText(filename, Constants.vbCrLf & Constants.vbCrLf)
                End If
            Else
                File.AppendAllText(filename, "[Date]>> " & DateTime.Today & Constants.vbCrLf & "[Method Name]>> " & "Exception is NOTHING" & Constants.vbCrLf & "[Message]>> " & "Exception is NOTHING" & Constants.vbCrLf & "[Source]>> " & "Exception is NOTHING" & Constants.vbCrLf & "[Stack Trace]>> " & "Exception is NOTHING" & Constants.vbCrLf)
            End If

        Catch exInner As Exception
        End Try
    End Sub

    Private Function getFileNameForToday() As String
        Dim day As Integer = DateTime.Now.Day
        Dim month As Integer = DateTime.Now.Month
        Dim year As Integer = DateTime.Now.Year
        Dim filePath As String = ConfigurationManager.AppSettings("errorlog").ToString()
        Dim filename As String = String.Empty
        Dim fn As String = String.Format("{0}_{1}_{2}.txt", year, month, day)
        filename = filePath & fn
        Return filename
    End Function

End Class
