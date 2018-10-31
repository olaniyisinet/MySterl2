Imports Microsoft.VisualBasic

Public Class easyrtgs





    Public Function getSessionStatus(ByVal username As String, ByVal sessionID As String) As Boolean


        Dim conn As New Data.SqlClient.SqlConnection(sqlconn1())
        conn.Open()
        Dim cmd As New Data.SqlClient.SqlCommand

        cmd.Connection = conn
        cmd.CommandText = "select * from Sessions where username=@username and sessionID=@sessionID"
        cmd.Parameters.AddWithValue("@username", username)
        cmd.Parameters.AddWithValue("@sessionID", sessionID)

        Dim rs As Data.SqlClient.SqlDataReader
        rs = cmd.ExecuteReader

        Dim s As String = ""

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

        conn.Close()


    End Function

    Public Function CreateSessionStatus(ByVal username As String, ByVal sessionID As String) As Boolean


        Dim conn As New Data.SqlClient.SqlConnection(sqlconn1())
        conn.Open()
        Dim cmd As New Data.SqlClient.SqlCommand
        Dim s As String = ""

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

        conn.Close()


    End Function


    Public Function DeleteSessionStatus(ByVal username As String) As Boolean


        Dim conn As New Data.SqlClient.SqlConnection(sqlconn1())
        conn.Open()
        Dim cmd As New Data.SqlClient.SqlCommand
        Dim s As String = ""

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

        conn.Close()


    End Function


    Public Function getName(ByVal username As String) As String

        Dim conn As New Data.OleDb.OleDbConnection(connUser())
        conn.Open()
        Dim cmd As New Data.OleDb.OleDbCommand
        cmd.Connection = conn
        cmd.CommandText = "select * from users where username='" & username & "'"
        Dim rs As Data.OleDb.OleDbDataReader
        rs = cmd.ExecuteReader

        Dim s As String = ""

        While rs.Read

            s = rs("name")


        End While
        conn.Close()


        Return s



    End Function

    Public Function getTypex(ByVal username As String) As String

        Dim conn As New Data.OleDb.OleDbConnection(connUser())
        conn.Open()
        Dim cmd As New Data.OleDb.OleDbCommand
        cmd.Connection = conn
        cmd.CommandText = "select * from users where username='" & username & "'"
        Dim rs As Data.OleDb.OleDbDataReader
        rs = cmd.ExecuteReader

        Dim s As String = ""

        While rs.Read

            s = rs("type")


        End While
        conn.Close()


        Return s



    End Function
    Public Function getAuthorizerEmail(ByVal branch As String) As String

        Dim conn As New Data.OleDb.OleDbConnection(connUser())
        conn.Open()
        Dim cmd As New Data.OleDb.OleDbCommand
        cmd.Connection = conn
        cmd.CommandText = "select * from users where branch='" & branch & "' AND status='Active' AND type='Authorizer'"
        Dim rs As Data.OleDb.OleDbDataReader
        rs = cmd.ExecuteReader

        Dim s As String = ""

        While rs.Read

            s = rs("email")


        End While
        conn.Close()


        Return s



    End Function

    Public Function sendmail(ByVal host As String, ByVal toAddress As String, ByVal cc As String, ByVal subject As String, ByVal body As String) As Boolean

        Dim mailHost As New Net.Mail.SmtpClient
        mailHost.Host = host
        Dim mailMessage As New Net.Mail.MailMessage
        Dim nc As New Net.NetworkCredential(System.Configuration.ConfigurationManager.AppSettings("mailUserName"), System.Configuration.ConfigurationManager.AppSettings("mailUserPwd"))
        mailHost.Credentials = nc
        mailMessage.IsBodyHtml = True
        mailMessage.To.Add(toAddress)
        mailMessage.From = New Net.Mail.MailAddress(System.Configuration.ConfigurationManager.AppSettings("AppMail"))
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

        Dim conn As New Data.OleDb.OleDbConnection(connUser())
        conn.Open()
        Dim cmd As New Data.OleDb.OleDbCommand
        cmd.Connection = conn
        cmd.CommandText = "select * from users where username='" & username & "'"
        Dim rs As Data.OleDb.OleDbDataReader
        rs = cmd.ExecuteReader

        Dim s As String = ""

        While rs.Read

            s = rs("email")


        End While
        conn.Close()


        Return s



    End Function

    Public Function getTxnStatus(ByVal tid As String) As String

        Dim conn As New Data.OleDb.OleDbConnection(conn1())
        conn.Open()
        Dim cmd As New Data.OleDb.OleDbCommand
        cmd.Connection = conn
        cmd.CommandText = "select * from transactions where transactionid='" & tid & "'"
        Dim rs As Data.OleDb.OleDbDataReader
        rs = cmd.ExecuteReader

        Dim s As String = ""

        While rs.Read

            s = rs("status")


        End While
        conn.Close()


        Return s



    End Function

    Function getAuthorizerEmailbyName(ByVal name As String) As String
        Dim conn As New Data.OleDb.OleDbConnection(connUser())
        conn.Open()
        Dim cmd As New Data.OleDb.OleDbCommand
        cmd.Connection = conn
        cmd.CommandText = "select * from users where name='" & name & "' and status='Active'"
        Dim rs As Data.OleDb.OleDbDataReader
        rs = cmd.ExecuteReader

        Dim s As String = ""

        While rs.Read

            s = rs("email")


        End While
        conn.Close()

        Return s


    End Function
    Function getEmailbyName(ByVal name As String) As String
        Dim conn As New Data.OleDb.OleDbConnection(connUser())
        conn.Open()
        Dim cmd As New Data.OleDb.OleDbCommand
        cmd.Connection = conn
        cmd.CommandText = "select * from users where name='" & name & "' and status='Active'"
        Dim rs As Data.OleDb.OleDbDataReader
        rs = cmd.ExecuteReader

        Dim s As String = ""

        While rs.Read

            s = rs("email")


        End While
        conn.Close()

        Return s


    End Function





    Public Function getRTGSAccount() As String

        Dim conn As New Data.OleDb.OleDbConnection(conn1())
        conn.Open()
        Dim cmd As New Data.OleDb.OleDbCommand
        cmd.Connection = conn
        cmd.CommandText = "select * from Admin where id=1"
        Dim rs As Data.OleDb.OleDbDataReader
        rs = cmd.ExecuteReader

        Dim s As String = ""

        While rs.Read

            s = rs("RTGSAccount")


        End While
        conn.Close()


        Return s



    End Function
    Public Function getPLAccount() As String

        Dim conn As New Data.OleDb.OleDbConnection(conn1())
        conn.Open()
        Dim cmd As New Data.OleDb.OleDbCommand
        cmd.Connection = conn
        cmd.CommandText = "select * from Admin where id=1"
        Dim rs As Data.OleDb.OleDbDataReader
        rs = cmd.ExecuteReader

        Dim s As String = ""

        While rs.Read

            s = rs("PLAccount")


        End While
        conn.Close()


        Return s



    End Function

    Public Function getCustomerCharges() As String

        Dim conn As New Data.OleDb.OleDbConnection(conn1())
        conn.Open()
        Dim cmd As New Data.OleDb.OleDbCommand
        cmd.Connection = conn
        cmd.CommandText = "select * from Admin where id=1"
        Dim rs As Data.OleDb.OleDbDataReader
        rs = cmd.ExecuteReader

        Dim s As String = ""

        While rs.Read

            s = rs("ChargesCustomer")


        End While
        conn.Close()


        Return s



    End Function

    Public Function getStaffCharges() As String

        Dim conn As New Data.OleDb.OleDbConnection(conn1())
        conn.Open()
        Dim cmd As New Data.OleDb.OleDbCommand
        cmd.Connection = conn
        cmd.CommandText = "select * from Admin where id=1"
        Dim rs As Data.OleDb.OleDbDataReader
        rs = cmd.ExecuteReader

        Dim s As String = ""

        While rs.Read

            s = rs("ChargesStaff")


        End While
        conn.Close()


        Return s



    End Function

    Public Function getRTGSMail() As String

        Dim conn As New Data.OleDb.OleDbConnection(conn1())
        conn.Open()
        Dim cmd As New Data.OleDb.OleDbCommand
        cmd.Connection = conn
        cmd.CommandText = "select * from Admin where id=1"
        Dim rs As Data.OleDb.OleDbDataReader
        rs = cmd.ExecuteReader

        Dim s As String = ""

        While rs.Read

            s = rs("RTGSEmail")


        End While
        conn.Close()


        Return s



    End Function

    Public Function getTreasuryMail() As String

        Dim conn As New Data.OleDb.OleDbConnection(conn1())
        conn.Open()
        Dim cmd As New Data.OleDb.OleDbCommand
        cmd.Connection = conn
        cmd.CommandText = "select * from Admin where id=1"
        Dim rs As Data.OleDb.OleDbDataReader
        rs = cmd.ExecuteReader

        Dim s As String = ""

        While rs.Read

            s = rs("TreasuryEMail")


        End While
        conn.Close()


        Return s



    End Function

    Public Function getBranch(ByVal username As String) As String

        Dim conn As New Data.OleDb.OleDbConnection(connUser())
        conn.Open()
        Dim cmd As New Data.OleDb.OleDbCommand
        cmd.Connection = conn
        cmd.CommandText = "select * from users where username='" & username & "'"
        Dim rs As Data.OleDb.OleDbDataReader
        rs = cmd.ExecuteReader

        Dim s As String = ""

        While rs.Read

            s = rs("branch")


        End While
        conn.Close()


        Return s



    End Function

    Public Function InsertLog(ByVal action As String) As String

        Dim conn As New Data.SqlClient.SqlConnection(sqlconn1())
        conn.Open()
        Dim cmd As New Data.SqlClient.SqlCommand
        cmd.Connection = conn
        cmd.CommandText = "Insert into logs(action,date) values(@action,@date)"
        cmd.Parameters.AddWithValue("@action", action)
        cmd.Parameters.AddWithValue("@date", Date.Now.ToString("yyyy-MM-dd hh:mm tt"))
        cmd.ExecuteNonQuery()

        conn.Close()


        Return "Action was logged"



    End Function
    Public Function TransactionAudit(ByVal action As String, ByVal date_time As DateTime, ByVal tid As String) As Boolean
        Dim s As String = ""

        Dim f As New IO.StreamWriter(AppDomain.CurrentDomain.BaseDirectory & "\logs\" & tid & ".txt", True)

        Try


            f.WriteLine(Now & ": " & action)
            f.Close()

            s = "t"


        Catch ex As Exception

            If ex.Message.Contains("KEY") Then

                s = "f"
            End If

            s = "f"


        End Try

        'conn.Close()

        If s = "t" Then
            Return True

        Else
            Return False

        End If
    End Function
    Public Function createAudit(ByVal action As String, ByVal date_time As DateTime) As Boolean
        Dim s As String = ""

        Dim f As New IO.StreamWriter(AppDomain.CurrentDomain.BaseDirectory & "\logs\" & Year(Now) & "-" & Month(Now) & "-" & Day(Now) & ".txt", True)

        Try
            'Dim conn As New Data.OleDb.OleDbConnection(conn1())
            'conn.Open()
            'Dim cmd As New Data.OleDb.OleDbCommand
            'cmd.Connection = conn
            'cmd.CommandText = "insert into log values('" & action & "','" & date_time & "')"
            'cmd.ExecuteNonQuery()
            'conn.Close()



            f.WriteLine(Now & ": " & action)
            f.Close()

            s = "t"


        Catch ex As Exception

            If ex.Message.Contains("KEY") Then

                s = "f"
            End If

            s = "f"


        End Try

        'conn.Close()

        If s = "t" Then
            Return True

        Else
            Return False

        End If
    End Function


    Public Function UpdatetxnAsPaid(ByVal tid As String) As String

        Dim bnk As New bank.Account

        Dim conn As New Data.SqlClient.SqlConnection(sqlconn1())
        conn.Open()
        Dim cmd As New Data.SqlClient.SqlCommand
        cmd.Connection = conn
        cmd.CommandText = "update transactions set status='Paid' where transactionid=@tid"
        cmd.Parameters.AddWithValue("@tid", tid)
        cmd.ExecuteNonQuery()

        Dim authby As String = ""
        Dim inputby As String = ""
        Dim cmd2 As New Data.SqlClient.SqlCommand
        cmd2.Connection = conn
        cmd2.CommandText = "select * from transactions where transactionid=@tid"
        cmd2.Parameters.AddWithValue("@tid", tid)

        Dim rs As Data.SqlClient.SqlDataReader
        rs = cmd2.ExecuteReader
        While rs.Read
            authby = rs("authorized_by")
            inputby = rs("uploaded_by")
        End While

        Try
            sendmail(System.Configuration.ConfigurationManager.AppSettings("mail"), getAuthorizerEmailbyName(authby), getEmailbyName(inputby), "Third Party Transaction (" & tid & ") Treated.", "Hello All, This Transaction with Transaction ID: " & tid & ", inputted by " & inputby & ", authorized by " & authby & " has been Paid by the system. Log on to the portal to view the full status. <br> Thank You")
        Catch

        End Try



        Return "Paid"
        conn.Close()






    End Function

    Public Function UpdatetxnAsDiscarded(ByVal tid As String) As String


        Dim bnk As New bank.Account

        Dim conn As New Data.SqlClient.SqlConnection(sqlconn1())
        conn.Open()
        Dim cmd As New Data.SqlClient.SqlCommand
        cmd.Connection = conn
        cmd.CommandText = "update transactions set status='Discarded' where transactionid=@tid"
        cmd.Parameters.AddWithValue("@tid", tid)

        cmd.ExecuteNonQuery()

        Dim authby As String = ""
        Dim inputby As String = ""
        Dim cmd2 As New Data.SqlClient.SqlCommand
        cmd2.Connection = conn
        cmd2.CommandText = "select * from transactions where transactionid=@tid"
        cmd2.Parameters.AddWithValue("@tid", tid)
        Dim rs As Data.SqlClient.SqlDataReader
        rs = cmd2.ExecuteReader
        While rs.Read
            authby = rs("authorized_by").ToString
            inputby = rs("uploaded_by").ToString
        End While

        Try
            sendmail(System.Configuration.ConfigurationManager.AppSettings("mail"), getRTGSMail() & "," & getTreasuryMail() & "," & getAuthorizerEmailbyName(authby), getEmailbyName(inputby), "Third Party Transaction (" & tid & ") Failed.", "Hello All, This Transaction with Transaction ID: " & tid & ", inputted by " & inputby & ", authorized by " & authby & " has been DISCARDED in the system. Confirm that both the Transaction and Charges were both sucessful. Log on to the portal to view the full status. <br> Thank You")
        Catch

        End Try



        Return "Discarded"
        conn.Close()






    End Function

    Public Function UpdatetxnAsFailed(ByVal tid As String) As String


        Dim bnk As New bank.Account

        Dim conn As New Data.SqlClient.SqlConnection(sqlconn1())
        conn.Open()
        Dim cmd As New Data.SqlClient.SqlCommand
        cmd.Connection = conn
        cmd.CommandText = "update transactions set status='Failed' where transactionid=@tid"
        cmd.Parameters.AddWithValue("@tid", tid)

        cmd.ExecuteNonQuery()

        Dim authby As String = ""
        Dim inputby As String = ""
        Dim cmd2 As New Data.SqlClient.SqlCommand
        cmd2.Connection = conn
        cmd2.CommandText = "select * from transactions where transactionid=@tid"
        cmd2.Parameters.AddWithValue("@tid", tid)
        Dim rs As Data.SqlClient.SqlDataReader
        rs = cmd2.ExecuteReader
        While rs.Read
            authby = rs("authorized_by")
            inputby = rs("uploaded_by")
        End While

        Try
            sendmail(System.Configuration.ConfigurationManager.AppSettings("mail"), getAuthorizerEmailbyName(authby), getEmailbyName(inputby), "Third Party Transaction (" & tid & ") Failed.", "Hello All, This Transaction with Transaction ID: " & tid & ", inputted by " & inputby & ", authorized by " & authby & " has failed in the system. Confirm that both the Transaction and Charges were both sucessful. Log on to the portal to view the full status. <br> Thank You")
        Catch

        End Try



        Return "Failed"
        conn.Close()






    End Function

    Public Function getUserStatus(ByVal id As Integer) As String
        Dim conn As New Data.OleDb.OleDbConnection(connUser())
        conn.Open()
        Dim cmd As New Data.OleDb.OleDbCommand
        cmd.Connection = conn
        cmd.CommandText = "select * from users where id=" & id
        Dim rs As Data.OleDb.OleDbDataReader
        rs = cmd.ExecuteReader

        Dim s As String = ""
        While rs.Read
            If rs("status") = "Active" Then
                s = "<img src='images\up.png' title='Active user' width='25'>"
            Else
                s = "<img src='images\down.png' title='InActive user' width='25'>"
            End If
        End While


        Return s
        conn.Close()


    End Function

    Public Function sqlconn1() As String

        Return "user id=sa;password=tylent;initial catalog=easyRTGSDB;data source=10.0.0.221"
    End Function

    Public Function conn1() As String

        Return "provider=sqloledb.1;user id=sa;password=tylent;initial catalog=easyRTGSDB;data source=10.0.0.221"
    End Function

    Public Function connUser() As String

        Return "provider=sqloledb.1;user id=sa;password=tylent;initial catalog=bulktraDB;data source=10.0.0.221"
    End Function

    Public Function SqlconnUser() As String

        Return "user id=sa;password=tylent;initial catalog=bulktraDB;data source=10.0.0.221"
    End Function


    Public Function UpdateAdminSettings(ByVal RTGSAccount As String, ByVal PLAccount As String, ByVal CustomerCharges As String, ByVal StaffCharges As String, ByVal RTGSmail As String, ByVal TreasuryMail As String) As String
        Dim conn As New Data.SqlClient.SqlConnection(sqlconn1())
        conn.Open()
        Dim cmd As New Data.SqlClient.SqlCommand
        cmd.Connection = conn
        cmd.CommandText = "update admin set RTGSAccount=@rtgsacc,PLAccount=@placc,ChargesCustomer=@cuscharges,ChargesStaff=@staffcharges,RTGSEmail=@rtgsmail,TreasuryEMail=@trmail"
        Dim s As String = ""
        cmd.Parameters.AddWithValue("@rtgsacc", RTGSAccount)
        cmd.Parameters.AddWithValue("@placc", PLAccount)
        cmd.Parameters.AddWithValue("@cuscharges", CustomerCharges)
        cmd.Parameters.AddWithValue("@staffcharges", StaffCharges)
        cmd.Parameters.AddWithValue("@rtgsmail", RTGSmail)
        cmd.Parameters.AddWithValue("@trmail", TreasuryMail)

        Try
            cmd.ExecuteNonQuery()
            s = "Updated"
        Catch ex As Exception

            s = "Update Failed"
            s = ex.Message

        End Try



        Return s
        conn.Close()


    End Function

    Public Function CheckAdminUser(ByVal username As String) As String
        Dim conn As New Data.OleDb.OleDbConnection(conn1())
        conn.Open()
        Dim cmd As New Data.OleDb.OleDbCommand
        cmd.Connection = conn
        cmd.CommandText = "select * from Administrators where username='" & username & "'"

        Dim rs As Data.OleDb.OleDbDataReader
        rs = cmd.ExecuteReader

        Dim s As String = ""
        While rs.Read
            If rs.HasRows Then
                s = "True"
            Else
                s = "False"
            End If
        End While


        Return s
        conn.Close()


    End Function
End Class
