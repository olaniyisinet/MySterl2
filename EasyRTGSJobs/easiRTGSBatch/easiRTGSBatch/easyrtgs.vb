Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Security
Imports System.Text
Imports System.Threading.Tasks
Imports Microsoft.VisualBasic

Public Class easyrtgs
	Public Function getSessionStatus(ByVal username As String, ByVal sessionID As String) As Boolean
		Dim s As String = ""

		Using conn As System.Data.SqlClient.SqlConnection = New System.Data.SqlClient.SqlConnection(sqlconn1())
			conn.Open()
			Dim cmd As System.Data.SqlClient.SqlCommand = New System.Data.SqlClient.SqlCommand()
			cmd.Connection = conn
			cmd.CommandText = "select * from Sessions where username=@username and sessionID=@sessionID"
			cmd.Parameters.AddWithValue("@username", username)
			cmd.Parameters.AddWithValue("@sessionID", sessionID)
			Dim rs As System.Data.SqlClient.SqlDataReader
			rs = cmd.ExecuteReader()

			If rs.HasRows Then
				s = "t"
			Else
				s = "f"
			End If

			If s = "t" Then
				Return True
			Else
				Return False
			End If
		End Using
	End Function

	Public Function CreateSessionStatus(ByVal username As String, ByVal sessionID As String) As Boolean
		Dim s As String = ""

		Using conn As System.Data.SqlClient.SqlConnection = New System.Data.SqlClient.SqlConnection(sqlconn1())
			conn.Open()
			Dim cmd As System.Data.SqlClient.SqlCommand = New System.Data.SqlClient.SqlCommand()
			cmd.Connection = conn
			cmd.CommandText = "Insert into Sessions(username,sessionID) values(@username,@sessionID)"
			cmd.Parameters.AddWithValue("@username", username)
			cmd.Parameters.AddWithValue("@sessionID", sessionID)

			Try
				cmd.ExecuteNonQuery()
				s = "t"
			Catch
				s = "f"
			End Try

			If s = "t" Then
				Return True
			Else
				Return False
			End If
		End Using
	End Function

	Public Function DeleteSessionStatus(ByVal username As String) As Boolean
		Dim s As String = ""

		Using conn As System.Data.SqlClient.SqlConnection = New System.Data.SqlClient.SqlConnection(sqlconn1())
			conn.Open()
			Dim cmd As System.Data.SqlClient.SqlCommand = New System.Data.SqlClient.SqlCommand()
			cmd.Connection = conn
			cmd.CommandText = "delete from  Sessions where username=@username"
			cmd.Parameters.AddWithValue("@username", username)

			Try
				cmd.ExecuteNonQuery()
				s = "t"
			Catch
				s = "f"
			End Try

			If s = "t" Then
				Return True
			Else
				Return False
			End If
		End Using
	End Function

	Public Function getName(ByVal username As String) As String
		Dim s As String = ""

		Using conn As System.Data.SqlClient.SqlConnection = New System.Data.SqlClient.SqlConnection(connUser())
			conn.Open()
			Dim cmd As System.Data.SqlClient.SqlCommand = New System.Data.SqlClient.SqlCommand()
			cmd.Connection = conn
			cmd.CommandText = "select * from users where username='" & username & "'"
			Dim rs As System.Data.SqlClient.SqlDataReader
			rs = cmd.ExecuteReader()

			While rs.Read()
				s = rs("name")
			End While
		End Using

		Return s
	End Function

	Public Function getTypex(ByVal username As String) As String
		Dim s As String = ""

		Using conn As System.Data.SqlClient.SqlConnection = New System.Data.SqlClient.SqlConnection(connUser())
			conn.Open()
			Dim cmd As System.Data.SqlClient.SqlCommand = New System.Data.SqlClient.SqlCommand()
			cmd.Connection = conn
			cmd.CommandText = "select * from users where username='" & username & "'"
			Dim rs As System.Data.SqlClient.SqlDataReader
			rs = cmd.ExecuteReader()

			While rs.Read()
				s = rs("type")
			End While
		End Using

		Return s
	End Function

	Public Function getAuthorizerEmail(ByVal branch As String) As String
		Dim s As String = ""

		Using conn As System.Data.SqlClient.SqlConnection = New System.Data.SqlClient.SqlConnection(connUser())
			conn.Open()
			Dim cmd As System.Data.SqlClient.SqlCommand = New System.Data.SqlClient.SqlCommand()
			cmd.Connection = conn
			cmd.CommandText = "select * from users where branch='" & branch & "' AND status='Active' AND type='Authorizer'"
			Dim rs As System.Data.SqlClient.SqlDataReader
			rs = cmd.ExecuteReader()

			While rs.Read()
				s = rs("email")
			End While
		End Using

		Return s
	End Function

	Public Function sendmail(ByVal host As String, ByVal toAddress As String, ByVal cc As String, ByVal subject As String, ByVal body As String) As Boolean
		Dim mailHost As System.Net.Mail.SmtpClient = New System.Net.Mail.SmtpClient()
		mailHost.Host = host
		Dim mailMessage As System.Net.Mail.MailMessage = New System.Net.Mail.MailMessage()
		Dim nc As System.Net.NetworkCredential = New System.Net.NetworkCredential("easyrtgs@sterlingbankng.com", "easyrtgs123")
		mailHost.Credentials = nc
		mailMessage.IsBodyHtml = True
		mailMessage.[To].Add(toAddress)
		mailMessage.From = New System.Net.Mail.MailAddress("easyrtgs@sterlingbank.com", "EasyRTGS Portal")
		mailMessage.CC.Add(cc)
		mailMessage.Subject = subject
		mailMessage.Body = body

		Try
			mailHost.Send(mailMessage)
			Return True
		Catch
			Return False
		End Try
	End Function

	Public Function getEmail(ByVal username As String) As String
		Dim s As String = ""

		Using conn As System.Data.SqlClient.SqlConnection = New System.Data.SqlClient.SqlConnection(connUser())
			conn.Open()
			Dim cmd As System.Data.SqlClient.SqlCommand = New System.Data.SqlClient.SqlCommand()
			cmd.Connection = conn
			cmd.CommandText = "select * from users where username='" & username & "'"
			Dim rs As System.Data.SqlClient.SqlDataReader
			rs = cmd.ExecuteReader()

			While rs.Read()
				s = rs("email")
			End While
		End Using

		Return s
	End Function

	Public Function getTxnStatus(ByVal tid As String) As String
		Dim s As String = ""

		Using conn As System.Data.SqlClient.SqlConnection = New System.Data.SqlClient.SqlConnection(sqlconn1())
			conn.Open()
			Dim cmd As System.Data.SqlClient.SqlCommand = New System.Data.SqlClient.SqlCommand()
			cmd.Connection = conn
			cmd.CommandText = "select * from transactions where transactionid='" & tid & "'"
			Dim rs As System.Data.SqlClient.SqlDataReader
			rs = cmd.ExecuteReader()

			While rs.Read()
				s = rs("status")
			End While
		End Using

		Return s
	End Function

	Public Function getAuthorizerEmailbyName(ByVal name As String) As String
		Dim s As String = ""

		Using conn As System.Data.SqlClient.SqlConnection = New System.Data.SqlClient.SqlConnection(connUser())
			conn.Open()
			Dim cmd As System.Data.SqlClient.SqlCommand = New System.Data.SqlClient.SqlCommand()
			cmd.Connection = conn
			cmd.CommandText = "select * from users where name='" & name & "' and status='Active'"
			Dim rs As System.Data.SqlClient.SqlDataReader
			rs = cmd.ExecuteReader()

			While rs.Read()
				s = rs("email")
			End While
		End Using

		Return s
	End Function

	Public Function getEmailbyName(ByVal name As String) As String
		Dim s As String = ""

		Using conn As System.Data.SqlClient.SqlConnection = New System.Data.SqlClient.SqlConnection(connUser())
			conn.Open()
			Dim cmd As System.Data.SqlClient.SqlCommand = New System.Data.SqlClient.SqlCommand()
			cmd.Connection = conn
			cmd.CommandText = "select * from users where name='" & name & "' and status='Active'"
			Dim rs As System.Data.SqlClient.SqlDataReader
			rs = cmd.ExecuteReader()

			While rs.Read()
				s = rs("email")
			End While
		End Using

		Return s
	End Function

	Public Function getRTGSAccount(ByVal isIMAL As Boolean) As String
		Dim s As String = ""

		Using conn As System.Data.SqlClient.SqlConnection = New System.Data.SqlClient.SqlConnection(sqlconn1())
			conn.Open()
			Dim cmd As System.Data.SqlClient.SqlCommand = New System.Data.SqlClient.SqlCommand()
			cmd.Connection = conn
			cmd.CommandText = "select * from Admin where id=2"
			Dim rs As System.Data.SqlClient.SqlDataReader
			rs = cmd.ExecuteReader()

			While rs.Read()

				If isIMAL Then
					s = rs("iMALRTGSAccount")
				Else
					s = rs("RTGSAccount")
				End If
			End While
		End Using

		Return s
	End Function

	Public Function getExplCode() As String
		Return "949"
	End Function

	Public Function getPLAccount(ByVal isIMAL As Boolean) As String
		Dim s As String = ""

		Using conn As System.Data.SqlClient.SqlConnection = New System.Data.SqlClient.SqlConnection(sqlconn1())
			conn.Open()
			Dim cmd As System.Data.SqlClient.SqlCommand = New System.Data.SqlClient.SqlCommand()
			cmd.Connection = conn
			cmd.CommandText = "select * from Admin where id=2"
			Dim rs As System.Data.SqlClient.SqlDataReader
			rs = cmd.ExecuteReader()

			While rs.Read()

				If isIMAL Then
					s = rs("iMALPLAccount")
				Else
					s = rs("PLAccount")
				End If
			End While
		End Using

		Return s
	End Function

	Public Function getCustomerCharges() As String
		Dim s As String = ""

		Using conn As System.Data.SqlClient.SqlConnection = New System.Data.SqlClient.SqlConnection(sqlconn1())
			conn.Open()
			Dim cmd As System.Data.SqlClient.SqlCommand = New System.Data.SqlClient.SqlCommand()
			cmd.Connection = conn
			cmd.CommandText = "select * from Admin where id=2"
			Dim rs As System.Data.SqlClient.SqlDataReader
			rs = cmd.ExecuteReader()

			While rs.Read()
				s = rs("ChargesCustomer")
			End While
		End Using

		Return s
	End Function

	Public Function getStaffCharges() As String
		Dim s As String = ""

		Using conn As System.Data.SqlClient.SqlConnection = New System.Data.SqlClient.SqlConnection(sqlconn1())
			conn.Open()
			Dim cmd As System.Data.SqlClient.SqlCommand = New System.Data.SqlClient.SqlCommand()
			cmd.Connection = conn
			cmd.CommandText = "select * from Admin where id=2"
			Dim rs As System.Data.SqlClient.SqlDataReader
			rs = cmd.ExecuteReader()

			While rs.Read()
				s = rs("ChargesStaff")
			End While
		End Using

		Return s
	End Function

	Public Function getRTGSMail(ByVal isIMAL As Boolean) As String
		Dim s As String = ""

		Using conn As System.Data.SqlClient.SqlConnection = New System.Data.SqlClient.SqlConnection(sqlconn1())
			conn.Open()
			Dim cmd As System.Data.SqlClient.SqlCommand = New System.Data.SqlClient.SqlCommand()
			cmd.Connection = conn
			cmd.CommandText = "select * from Admin where id=2"
			Dim rs As System.Data.SqlClient.SqlDataReader
			rs = cmd.ExecuteReader()

			While rs.Read()

				If isIMAL Then
					s = rs("iMALRTGSEmail")
				Else
					s = rs("RTGSEmail")
				End If
			End While
		End Using

		Return s
	End Function

	Public Function getTreasuryMail(ByVal isIMAL As Boolean) As String
		Dim s As String = ""

		Using conn As System.Data.SqlClient.SqlConnection = New System.Data.SqlClient.SqlConnection(sqlconn1())
			conn.Open()
			Dim cmd As System.Data.SqlClient.SqlCommand = New System.Data.SqlClient.SqlCommand()
			cmd.Connection = conn
			cmd.CommandText = "select * from Admin where id=2"
			Dim rs As System.Data.SqlClient.SqlDataReader
			rs = cmd.ExecuteReader()

			While rs.Read()

				If isIMAL Then
					s = rs("iMALTreasuryEMail")
				Else
					s = rs("TreasuryEMail")
				End If
			End While
		End Using

		Return s
	End Function

	Public Function getBranch(ByVal username As String) As String
		Dim s As String = ""

		Using conn As System.Data.SqlClient.SqlConnection = New System.Data.SqlClient.SqlConnection(connUser())
			conn.Open()
			Dim cmd As System.Data.SqlClient.SqlCommand = New System.Data.SqlClient.SqlCommand()
			cmd.Connection = conn
			cmd.CommandText = "select * from users where username='" & username & "'"
			Dim rs As System.Data.SqlClient.SqlDataReader
			rs = cmd.ExecuteReader()

			While rs.Read()
				s = rs("branch")
			End While
		End Using

		Return s
	End Function

	Public Function getTransactionDetails(ByVal tid As String) As String
		Dim s As String = ""

		Using conn As System.Data.SqlClient.SqlConnection = New System.Data.SqlClient.SqlConnection(sqlconn1())
			conn.Open()
			Dim cmd As System.Data.SqlClient.SqlCommand = New System.Data.SqlClient.SqlCommand()
			cmd.Connection = conn
			cmd.CommandText = "select * from transactions where TransactionID=@tid"
			cmd.Parameters.AddWithValue("@tid", tid)
			Dim rs As System.Data.SqlClient.SqlDataReader
			rs = cmd.ExecuteReader()

			While rs.Read()
				s = rs("customer_name") & "***" + rs("customer_account") & "***" + rs("amount") & "***" + rs("remarks")
			End While
		End Using

		Return s
	End Function

	Public Function InsertLog(ByVal action As String) As String
		Dim s As String = ""

		Using conn As System.Data.SqlClient.SqlConnection = New System.Data.SqlClient.SqlConnection(sqlconn1())
			conn.Open()
			Dim cmd As System.Data.SqlClient.SqlCommand = New System.Data.SqlClient.SqlCommand()
			cmd.Connection = conn
			cmd.CommandText = "Insert into logs(action,date) values(@action,@date)"
			cmd.Parameters.AddWithValue("@action", action)
			cmd.Parameters.AddWithValue("@date", DateTime.Now.ToString("yyyy-MM-dd hh:mm tt"))
			cmd.ExecuteNonQuery()
		End Using

		Return "Action was logged"
	End Function

	Public Function TransactionAudit(ByVal action As String, ByVal date_time As DateTime, ByVal tid As String) As Boolean
		Dim s As String = ""
		Dim f As System.IO.StreamWriter = New System.IO.StreamWriter(AppDomain.CurrentDomain.BaseDirectory & "\logs\" & tid & ".txt", True)

		Try
			f.WriteLine(Constants.vbCrLf & DateTime.Now & ": " & action & Constants.vbCrLf)
			f.Close()
			s = "t"
		Catch ex As Exception
			If ex.Message.Contains("KEY") Then s = "f"
			s = "f"
		End Try

		If s = "t" Then
			Return True
		Else
			Return False
		End If
	End Function

	Public Function createAudit(ByVal action As String, ByVal date_time As DateTime) As Boolean
		Dim s As String = ""
		Dim f As System.IO.StreamWriter = New System.IO.StreamWriter(AppDomain.CurrentDomain.BaseDirectory & "\logs\" & Year(Now) & "-" + Month(Now) & "-" + Day(Now) & ".txt", True)

		Try
			f.WriteLine(DateTime.Now & ": " & action)
			f.Close()
			s = "t"
		Catch ex As Exception
			If ex.Message.Contains("KEY") Then s = "f"
			s = "f"
		End Try

		If s = "t" Then
			Return True
		Else
			Return False
		End If
	End Function

	Public Function createErrorLog(ByVal action As String, ByVal date_time As DateTime) As Boolean
		Dim s As String = ""
		Dim f As System.IO.StreamWriter = New System.IO.StreamWriter(AppDomain.CurrentDomain.BaseDirectory & "\ErrorLogs\" & Year(Now) & "-" + Month(Now) & "-" + Day(Now) & ".txt", True)

		Try
			f.WriteLine(DateTime.Now & ": " & action)
			f.Close()
			s = "t"
		Catch ex As Exception
			If ex.Message.Contains("KEY") Then s = "f"
			s = "f"
		End Try

		If s = "t" Then
			Return True
		Else
			Return False
		End If
	End Function

	Public Function UpdatetxnAsPaid(ByVal tid As String, ByVal isIMAL As Boolean) As String
		Dim bnk As bank.Account = New bank.Account()
		Dim s As String = ""

		Using conn As System.Data.SqlClient.SqlConnection = New System.Data.SqlClient.SqlConnection(sqlconn1())
			conn.Open()
			Dim cmd As System.Data.SqlClient.SqlCommand = New System.Data.SqlClient.SqlCommand()
			cmd.Connection = conn
			cmd.CommandText = "update transactions set status='Paid' where transactionid=@tid"
			cmd.Parameters.AddWithValue("@tid", tid)
			cmd.ExecuteNonQuery()
			Dim authby As String = ""
			Dim inputby As String = ""
			Dim cmd2 As System.Data.SqlClient.SqlCommand = New System.Data.SqlClient.SqlCommand()
			cmd2.Connection = conn
			cmd2.CommandText = "select * from transactions where transactionid=@tid"
			cmd2.Parameters.AddWithValue("@tid", tid)
			Dim rs As System.Data.SqlClient.SqlDataReader
			rs = cmd2.ExecuteReader()

			While rs.Read()
				authby = rs("authorized_by")
				inputby = rs("uploaded_by")
			End While

			Try
				Dim txn As Array = getTransactionDetails(tid).Split("***")
				sendmail("10.0.0.88", getAuthorizerEmailbyName(authby) & "," & getRTGSMail(isIMAL) & "," & getTreasuryMail(isIMAL), getEmailbyName(inputby), "Third Party Transaction (" & tid & ") TREATED/PAID.", "Hello All,<br> This Transaction with Transaction ID: " & tid & "<br>Customer Name:" & txn(0) & "<br>Customer Account:" + txn(1) & "<br>Remarks:" + txn(2) & "<br>Initiated by " & inputby & "<br>Authorized by " & authby & "<br>Status: TREATED/PAID.<br> Log on to the portal to view the full status. <br> Thank You")
			Catch
			End Try

			Return "Paid"
		End Using
	End Function

	Public Function UpdatetxnAsPaymentReversed(ByVal tid As String, ByVal isIMAL As Boolean) As String
		Dim bnk As bank.Account = New bank.Account()
		Dim s As String = ""

		Using conn As System.Data.SqlClient.SqlConnection = New System.Data.SqlClient.SqlConnection(sqlconn1())
			conn.Open()
			Dim cmd As System.Data.SqlClient.SqlCommand = New System.Data.SqlClient.SqlCommand()
			cmd.Connection = conn
			cmd.CommandText = "update transactions set status='PaymentReversed' where transactionid=@tid"
			cmd.Parameters.AddWithValue("@tid", tid)
			cmd.ExecuteNonQuery()
			Dim authby As String = ""
			Dim inputby As String = ""
			Dim cmd2 As System.Data.SqlClient.SqlCommand = New System.Data.SqlClient.SqlCommand()
			cmd2.Connection = conn
			cmd2.CommandText = "select * from transactions where transactionid=@tid"
			cmd2.Parameters.AddWithValue("@tid", tid)
			Dim rs As System.Data.SqlClient.SqlDataReader
			rs = cmd2.ExecuteReader()

			While rs.Read()
				authby = rs("authorized_by")
				inputby = rs("uploaded_by")
			End While

			Try
				Dim txn As Array = getTransactionDetails(tid).Split("***")
				sendmail("10.0.0.88", getAuthorizerEmailbyName(authby) & "," & getRTGSMail(isIMAL) & "," & getTreasuryMail(isIMAL), getEmailbyName(inputby), "Third Party Transaction (" & tid & ") Principal and Charges Reversed.", "Hello All,<br> This Transaction with Transaction ID: " & tid & "<br>Customer Name:" & txn(0) & "<br>Customer Account:" + txn(1) & "<br>Remarks:" + txn(2) & "<br>Initiated by " & inputby & "<br>Authorized by " & authby & "<br>Status: REVERSED.<br> Log on to the portal to view the full status. <br> Thank You")
			Catch
			End Try
		End Using

		Return "PaymentReversed"
	End Function

	Public Function getRTGSSuspense(ByVal isIMAL As Boolean) As String
		Dim s As String = ""

		Using conn As System.Data.SqlClient.SqlConnection = New System.Data.SqlClient.SqlConnection(sqlconn1())
			conn.Open()
			Dim cmd As System.Data.SqlClient.SqlCommand = New System.Data.SqlClient.SqlCommand()
			cmd.Connection = conn
			cmd.CommandText = "select * from Admin where id=2"
			Dim rs As System.Data.SqlClient.SqlDataReader
			rs = cmd.ExecuteReader()

			While rs.Read()

				If isIMAL Then
					s = rs("iMALRTGSSuspense")
				Else
					s = rs("RTGSSuspense")
				End If
			End While
		End Using

		Return s
	End Function

	Public Function UpdatetxnAsDiscarded(ByVal tid As String) As String
		Dim bnk As bank.Account = New bank.Account()
		Dim s As String = ""

		Using conn As System.Data.SqlClient.SqlConnection = New System.Data.SqlClient.SqlConnection(sqlconn1())
			conn.Open()
			Dim cmd As System.Data.SqlClient.SqlCommand = New System.Data.SqlClient.SqlCommand()
			cmd.Connection = conn
			cmd.CommandText = "update transactions set status='Discarded' where transactionid=@tid"
			cmd.Parameters.AddWithValue("@tid", tid)
			cmd.ExecuteNonQuery()
			Dim authby As String = ""
			Dim inputby As String = ""
			Dim cmd2 As System.Data.SqlClient.SqlCommand = New System.Data.SqlClient.SqlCommand()
			cmd2.Connection = conn
			cmd2.CommandText = "select * from transactions where transactionid=@tid"
			cmd2.Parameters.AddWithValue("@tid", tid)
			Dim rs As System.Data.SqlClient.SqlDataReader
			rs = cmd2.ExecuteReader()

			While rs.Read()
				authby = rs("authorized_by").ToString()
				inputby = rs("uploaded_by").ToString()
			End While

			Try
				Dim txn As Array = getTransactionDetails(tid).Split("***")
				sendmail("10.0.0.88", getAuthorizerEmailbyName(authby), getEmailbyName(inputby), "Third Party Transaction (" & tid & ") Discarded.", "Hello All,<br> This Transaction with Transaction ID: " & tid & "<br>Customer Name:" & txn(0) & "<br>Customer Account:" + txn(1) & "<br>Remarks:" + txn(2) & "<br>Initiated by " & inputby & "<br>Authorized by " & authby & "<br>Status: DISCARDED.<br> Log on to the portal to view the full status. <br> Thank You")
			Catch
			End Try

			Return "Discarded"
		End Using
	End Function

	Public Function UpdatetxnAsFailedTreasury(ByVal tid As String, ByVal reason As String, ByVal isIMAL As Boolean) As String
		Dim bnk As bank.Account = New bank.Account()
		Dim s As String = ""

		Using conn As System.Data.SqlClient.SqlConnection = New System.Data.SqlClient.SqlConnection(sqlconn1())
			conn.Open()
			Dim cmd As System.Data.SqlClient.SqlCommand = New System.Data.SqlClient.SqlCommand()
			cmd.Connection = conn
			cmd.CommandText = "update transactions set status='Failed' where transactionid=@tid"
			cmd.Parameters.AddWithValue("@tid", tid)
			cmd.ExecuteNonQuery()
			Dim authby As String = ""
			Dim inputby As String = ""
			Dim cmd2 As System.Data.SqlClient.SqlCommand = New System.Data.SqlClient.SqlCommand()
			cmd2.Connection = conn
			cmd2.CommandText = "select * from transactions where transactionid=@tid"
			cmd2.Parameters.AddWithValue("@tid", tid)
			Dim rs As System.Data.SqlClient.SqlDataReader
			rs = cmd2.ExecuteReader()

			While rs.Read()
				authby = rs("authorized_by").ToString()
				inputby = rs("uploaded_by").ToString()
			End While

			Try
				Dim txn As Array = getTransactionDetails(tid).Split("***")
				sendmail("10.0.0.88", getAuthorizerEmailbyName(authby) & "," & getRTGSMail(isIMAL) & "," & getTreasuryMail(isIMAL), getEmailbyName(inputby), "Third Party Transaction (" & tid & ") Failed.", "Hello All,<br> This Transaction with Transaction ID: " & tid & "<br>Customer Name:" & txn(0) & "<br>Customer Account:" + txn(1) & "<br>Remarks:" + txn(2) & "<br>Initiated by " & inputby & "<br>Authorized by " & authby & "<br>Status: FAILED: " & reason & ".<br> Log on to the portal to view the full status. <br> Thank You")
			Catch
			End Try

			Return "Failed"
		End Using
	End Function

	Public Function UpdatetxnAsFailedAuthorizer(ByVal tid As String, ByVal reason As String) As String
		Dim bnk As bank.Account = New bank.Account()
		Dim s As String = ""

		Using conn As System.Data.SqlClient.SqlConnection = New System.Data.SqlClient.SqlConnection(sqlconn1())
			conn.Open()
			Dim cmd As System.Data.SqlClient.SqlCommand = New System.Data.SqlClient.SqlCommand()
			cmd.Connection = conn
			cmd.CommandText = "update transactions set status='Failed' where transactionid=@tid"
			cmd.Parameters.AddWithValue("@tid", tid)
			cmd.ExecuteNonQuery()
			Dim authby As String = ""
			Dim inputby As String = ""
			Dim cmd2 As System.Data.SqlClient.SqlCommand = New System.Data.SqlClient.SqlCommand()
			cmd2.Connection = conn
			cmd2.CommandText = "select * from transactions where transactionid=@tid"
			cmd2.Parameters.AddWithValue("@tid", tid)
			Dim rs As System.Data.SqlClient.SqlDataReader
			rs = cmd2.ExecuteReader()

			While rs.Read()
				authby = rs("authorized_by").ToString()
				inputby = rs("uploaded_by").ToString()
			End While

			Try
				Dim txn As Array = getTransactionDetails(tid).Split("***")
				sendmail("10.0.0.88", getAuthorizerEmailbyName(authby), getEmailbyName(inputby), "Third Party Transaction (" & tid & ") Failed.", "Hello All,<br> This Transaction with Transaction ID: " & tid & "<br>Customer Name:" & txn(0) & "<br>Customer Account:" + txn(1) & "<br>Remarks:" + txn(2) & "<br>Initiated by " & inputby & "<br>Authorized by " & authby & "<br>Status: FAILED: " & reason & ".<br> Log on to the portal to view the full status. <br> Thank You")
			Catch
			End Try

			Return "Failed"
		End Using
	End Function

	Public Function UpdatetxnAsAuthorized(ByVal tid As String, ByVal isIMAL As Boolean, ByVal Optional status As String = "Authorized") As String
		Dim bnk As bank.Account = New bank.Account()
		Dim s As String = ""

		Using conn As System.Data.SqlClient.SqlConnection = New System.Data.SqlClient.SqlConnection(sqlconn1())
			conn.Open()
			Dim cmd As System.Data.SqlClient.SqlCommand = New System.Data.SqlClient.SqlCommand()
			cmd.Connection = conn
			cmd.CommandText = "update transactions set status=@status where transactionid=@tid"
			cmd.Parameters.AddWithValue("@tid", tid)
			cmd.Parameters.AddWithValue("@status", status)
			cmd.ExecuteNonQuery()
			Dim authby As String = ""
			Dim inputby As String = ""
			Dim cmd2 As System.Data.SqlClient.SqlCommand = New System.Data.SqlClient.SqlCommand()
			cmd2.Connection = conn
			cmd2.CommandText = "select * from transactions where transactionid=@tid"
			cmd2.Parameters.AddWithValue("@tid", tid)
			Dim rs As System.Data.SqlClient.SqlDataReader
			rs = cmd2.ExecuteReader()

			While rs.Read()
				authby = rs("authorized_by").ToString()
				inputby = rs("uploaded_by").ToString()
			End While

			Try
				Dim txn As Array = getTransactionDetails(tid).Split("***")
				sendmail("10.0.0.88", getAuthorizerEmailbyName(authby) & "," & getRTGSMail(isIMAL) & "," & getTreasuryMail(isIMAL), getEmailbyName(inputby), "Third Party Transaction (" & tid & ") Authorized.", "Hello All,<br> This Transaction with Transaction ID: " & tid & "<br>Customer Name:" & txn(0) & "<br>Customer Account:" + txn(1) & "<br>Remarks:" + txn(2) & "<br>Initiated by " & inputby & "<br>Authorized by " & authby & "<br>Status: PAID by the system.<br> Log on to the portal to view the full status. <br> Thank You")
			Catch
			End Try

			Return "Paid"
		End Using
	End Function

	Public Function getUserStatus(ByVal id As Integer) As String
		Dim s As String = ""

		Using conn As System.Data.SqlClient.SqlConnection = New System.Data.SqlClient.SqlConnection(connUser())
			conn.Open()
			Dim cmd As System.Data.SqlClient.SqlCommand = New System.Data.SqlClient.SqlCommand()
			cmd.Connection = conn
			cmd.CommandText = "select * from users where id=" & id
			Dim rs As System.Data.SqlClient.SqlDataReader
			rs = cmd.ExecuteReader()

			While rs.Read()

				If rs("status") = "Active" Then
					s = "<img src='images\up.png' title='Active user' width='25'>"
				Else
					s = "<img src='images\down.png' title='InActive user' width='25'>"
				End If
			End While
		End Using

		Return s
	End Function

	Public Function sqlconn1() As String
		Return "user id=sa;password=tylent;initial catalog=easyRTGSDB;data source=10.0.41.101"
	End Function

	Public Function conn1() As String
		Return "provider=sqloledb.1;user id=sa;password=tylent;initial catalog=easyRTGSDB;data source=10.0.41.101"
	End Function

	Public Function connUser() As String
		Return "provider=sqloledb.1;user id=sa;password=tylent;initial catalog=bulktraDB;data source=10.0.41.101"
	End Function

	Public Function SqlconnUser() As String
		Return "provider=sqloledb.1;user id=sa;password=tylent;initial catalog=bulktraDB;data source=10.0.41.101"
	End Function

	Public Function UpdateAdminSettings(ByVal isIMAL As Boolean, ByVal RTGSAccount As String, ByVal PLAccount As String, ByVal CustomerCharges As String, ByVal StaffCharges As String, ByVal RTGSmail As String, ByVal TreasuryMail As String) As String
		Dim s As String = ""

		Using conn As System.Data.SqlClient.SqlConnection = New System.Data.SqlClient.SqlConnection(sqlconn1())
			conn.Open()
			Dim cmd As System.Data.SqlClient.SqlCommand = New System.Data.SqlClient.SqlCommand()
			cmd.Connection = conn

			If isIMAL Then
				cmd.CommandText = "update admin set iMALRTGSAccount=@rtgsacc,iMALPLAccount=@placc,ChargesCustomer=@cuscharges,ChargesStaff=@staffcharges,iMALRTGSEmail=@rtgsmail,iMALTreasuryEMail=@trmail"
				cmd.Parameters.AddWithValue("@rtgsacc", RTGSAccount)
				cmd.Parameters.AddWithValue("@placc", PLAccount)
				cmd.Parameters.AddWithValue("@cuscharges", CustomerCharges)
				cmd.Parameters.AddWithValue("@staffcharges", StaffCharges)
				cmd.Parameters.AddWithValue("@rtgsmail", RTGSmail)
				cmd.Parameters.AddWithValue("@trmail", TreasuryMail)
			Else
				cmd.CommandText = "update admin set RTGSAccount=@rtgsacc,PLAccount=@placc,ChargesCustomer=@cuscharges,ChargesStaff=@staffcharges,RTGSEmail=@rtgsmail,TreasuryEMail=@trmail"
				cmd.Parameters.AddWithValue("@rtgsacc", RTGSAccount)
				cmd.Parameters.AddWithValue("@placc", PLAccount)
				cmd.Parameters.AddWithValue("@cuscharges", CustomerCharges)
				cmd.Parameters.AddWithValue("@staffcharges", StaffCharges)
				cmd.Parameters.AddWithValue("@rtgsmail", RTGSmail)
				cmd.Parameters.AddWithValue("@trmail", TreasuryMail)
			End If

			Try
				cmd.ExecuteNonQuery()
				s = "Updated"
			Catch ex As Exception
				s = "Update Failed"
				s = ex.Message
			End Try

			Return s
		End Using
	End Function

	Public Function CheckAdminUser(ByVal username As String) As String
		Dim s As String = ""

		Using conn As System.Data.SqlClient.SqlConnection = New System.Data.SqlClient.SqlConnection(sqlconn1())
			conn.Open()
			Dim cmd As System.Data.SqlClient.SqlCommand = New System.Data.SqlClient.SqlCommand()
			cmd.Connection = conn
			cmd.CommandText = "select * from Administrators where username='" & username & "'"
			Dim rs As System.Data.SqlClient.SqlDataReader
			rs = cmd.ExecuteReader()

			While rs.Read()

				If rs.HasRows Then
					s = "True"
				Else
					s = "False"
				End If
			End While

			Return s
		End Using
	End Function
End Class
