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
Imports System.Management
Imports System.Data.SqlClient
Imports System.Threading
Imports Oracle.ManagedDataAccess.Client
Imports System.Configuration
Imports BankCore
Imports BankCore.T24
Imports Newtonsoft.Json

Module Module1
	Private esy As New easyrtgs

	Dim t As New Threading.Thread(AddressOf performCredit)
	Dim reversalThread As New Threading.Thread(AddressOf performReversal)
	Dim cbnCreditThread As New Threading.Thread(AddressOf performCBNCredit)

	Private Function GetConnectionStringFromFile(ByVal Optional key As String = "") As String
		Dim Path As String = "conn.txt"
		Dim conn As String = String.Empty

		If String.IsNullOrEmpty(key) Then
			Path = "conn.txt"
			If File.Exists(Path) Then conn = File.ReadAllText(Path)
			Return conn
		Else

			Try
				conn = OneConfig.Text.[Get]("oracle_appusr")
			Catch ex As Exception
				LogException(ex)
				conn = "user id=appusr;password=testapp;data source=(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST=10.0.0.174)(PORT=1590))(CONNECT_DATA=(SID = HOBANK1)))"
			End Try

			If String.IsNullOrEmpty(conn) Then conn = "user id=appusr;password=testapp;data source=(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST=10.0.0.174)(PORT=1590))(CONNECT_DATA=(SID = HOBANK1)))"
			Return conn
		End If
	End Function

	Function sqlconn() As String
		Return GetConnectionStringFromFile()
	End Function

	Sub Main()
		Do_New_Posting_Steps()
	End Sub

	Sub Do_Former_Posting_Steps()
		Dim cnt As Integer
		Dim query As String = "Select * from Win32_Process Where Name = 'easiRTGSBatch.exe'"
		Dim searcher As ManagementObjectSearcher = New ManagementObjectSearcher(query)
		Dim processList As ManagementObjectCollection = searcher.[Get]()
		cnt = processList.Count

		If cnt > 1 Then
			System.Environment.[Exit](0)
		Else
			performCredit()
		End If
	End Sub

	Sub performCredit()
		Console.WriteLine("EasyRTGS Transaction processing started!")

		Do

			While Not Console.KeyAvailable
				Console.WriteLine("Searching for Transactions....." & DateTime.Now)
				action2()
				action()
				System.Threading.Thread.Sleep(1000)
				Console.Clear()
			End While
		Loop While Console.ReadKey(True).Key <> ConsoleKey.Escape
	End Sub

	Sub performCBNCredit()
		Console.WriteLine("EasyRTGS CBN processing started!")

		Do

			While Not Console.KeyAvailable
				Console.WriteLine("Searching for Transactions....." & DateTime.Now)
				action2(True)
				action()
				System.Threading.Thread.Sleep(1000)
				Console.Clear()
			End While
		Loop While Console.ReadKey(True).Key <> ConsoleKey.Escape
	End Sub

	Sub performReversal()
		Console.WriteLine("EasyRTGS Transaction processing started!")

		Do

			While Not Console.KeyAvailable
				Console.WriteLine("Searching for Transactions to Reverse....." & DateTime.Now)
				action2(True)
				System.Threading.Thread.Sleep(1000)
				Console.Clear()
			End While
		Loop While Console.ReadKey(True).Key <> ConsoleKey.Escape
	End Sub

	Sub action(ByVal Optional isReversal As Boolean = False)
		Using conn As System.Data.SqlClient.SqlConnection = New System.Data.SqlClient.SqlConnection(sqlconn())
			conn.Open()
			Dim cusAcc As String = ""
			Dim remarks As String = ""
			Dim amount As String = ""
			Dim isIMAL As Boolean = False
			Dim beneFiciaryName As String = ""
			Dim explcode As String = esy.getExplCode()
			Dim charges As String = ""
			Dim rtgsAcc As String = ""
			Dim rtgsPL As String = ""
			Dim cusPay As String = ""
			Dim cusCharges As String = ""
			Dim rtgsSuspense As String = ""
			Dim cmd As System.Data.SqlClient.SqlCommand = New System.Data.SqlClient.SqlCommand()
			cmd.Connection = conn
			cmd.CommandText = "select * from Transactions where status=@status order by id asc"
			Dim status As String = "Pay"
			cmd.Parameters.AddWithValue("@status", status)
			Dim rs As System.Data.SqlClient.SqlDataReader
			rs = cmd.ExecuteReader()

			While rs.Read()
				Console.WriteLine(rs("TransactionID").ToString() & " was selected for processing.")
				cusAcc = rs("Customer_account").ToString()
				remarks = rs("remarks").ToString()
				amount = rs("amount").ToString()
				charges = rs("charges").ToString()

				If cusAcc.StartsWith("05") Then
					isIMAL = True
					beneFiciaryName = rs("Beneficiary").ToString()
				Else
					isIMAL = False
					beneFiciaryName = ""
				End If

				rtgsAcc = esy.getRTGSAccount(isIMAL)
				rtgsPL = esy.getPLAccount(isIMAL)
				rtgsSuspense = esy.getRTGSSuspense(isIMAL)

				Try

					If Not isReversal Then
						cusPay = pay(isIMAL, rtgsSuspense, rtgsAcc, beneFiciaryName, remarks, amount, explcode)
					Else
						cusPay = pay(isIMAL, rtgsAcc, rtgsSuspense, beneFiciaryName, "PRINCIPAL REVERSAL - " & remarks, amount, explcode)
					End If

					esy.TransactionAudit(cusPay, DateTime.Now, rs("TransactionID").ToString())
					Console.WriteLine(rs("TransactionID").ToString() & ": Treasury Ops Debitting principal..." & Constants.vbCrLf & cusPay)

                    If cusPay.Contains("Response Code:0") Or cusPay.Contains("responseCode:0") Then

                        If Not isReversal Then
                            cusPay = "Customer`s Account was debited SUCCESSFULLY." & rtgsAcc & " Credited."
                        Else
                            cusPay = "Customer`s Account was credited SUCCESSFULLY." & rtgsAcc & " Debited."
                        End If
                    Else
                        cusPay = "Failed to debit customer account to credit " & rtgsAcc
					End If

				Catch ex As Exception
					LogException(ex)
					esy.createErrorLog(ex.Message, DateTime.Now)
					Console.WriteLine(rs("TransactionID").ToString() & ":Treasury Ops Debitting principal..." & Constants.vbCrLf & ex.Message)

					If ex.Message.Contains("TNS") Then
						cusPay = "Cannot connect to BANKS."
						esy.TransactionAudit(cusPay, DateTime.Now, rs("TransactionID").ToString())
					Else
						cusPay = "Customer account debit Failed to complete."
						esy.TransactionAudit(cusPay, DateTime.Now, rs("TransactionID").ToString())
					End If
				End Try

				Try

					If Not isReversal Then
						cusCharges = pay(isIMAL, rtgsSuspense, rtgsPL, beneFiciaryName, "CHARGES - " & remarks, charges, explcode)
					Else
						cusCharges = pay(isIMAL, rtgsPL, rtgsSuspense, beneFiciaryName, "CHARGES REVERSAL - " & remarks, charges, explcode)
					End If

					esy.TransactionAudit(cusCharges, DateTime.Now, rs("TransactionID").ToString())
					Console.WriteLine(rs("TransactionID").ToString() & ":Treasury Ops Debitting charge(s)..." & Constants.vbCrLf & cusCharges)

                    If cusCharges.Contains("Response Code:0") Or cusCharges.Contains("responseCode:0") Then
                        cusCharges = "Charge(s) was debited SUCCESSFULLY. " & rtgsPL & " Credited."
                    Else
                    End If

				Catch ex2 As Exception
					LogException(ex2)
					Console.WriteLine(rs("TransactionID").ToString() & ":Treasury Ops Debitting charge(s)..." & Constants.vbCrLf & ex2.Message)
					esy.createErrorLog(ex2.Message, DateTime.Now)

					If ex2.Message.Contains("TNS") Then
						cusCharges = "Cannot connect to BANKS."
						esy.TransactionAudit(cusCharges, DateTime.Now, rs("TransactionID").ToString())
					Else
						cusCharges = "Charges Failed to complete."
						esy.TransactionAudit(cusCharges, DateTime.Now, rs("TransactionID").ToString())
					End If
				End Try

				If cusPay.Contains("SUCCESSFULLY") And cusCharges.Contains("SUCCESSFULLY") Then

					If Not isReversal Then
						esy.UpdatetxnAsPaid(rs("TransactionID").ToString(), isIMAL)
					Else
						esy.UpdatetxnAsPaid(rs("TransactionID").ToString(), isIMAL)
					End If
				Else
					esy.UpdatetxnAsFailedTreasury(rs("TransactionID").ToString(), cusPay, isIMAL)
				End If

				Console.WriteLine(rs("TransactionID").ToString() & " Completed." & Constants.vbCrLf & Constants.vbCrLf & "***********************************************")
				System.Threading.Thread.Sleep(2000)
			End While
		End Using
	End Sub

	Sub action2(ByVal Optional isReversal As Boolean = False)
		Using conn As System.Data.SqlClient.SqlConnection = New System.Data.SqlClient.SqlConnection(sqlconn())
			conn.Open()
			Dim cusAcc As String = ""
			Dim remarks As String = ""
			Dim amount As String = ""
			Dim explcode As String = esy.getExplCode()
			Dim isIMAL As Boolean = False
			Dim beneFiciaryName As String = ""
			Dim charges As String = ""
			Dim rtgsAcc As String = ""
			Dim rtgsPL As String = ""
			Dim rtgsSuspense As String = ""
			Dim cusPay As String = ""
			Dim cusCharges As String = ""
			Dim cmd As System.Data.SqlClient.SqlCommand = New System.Data.SqlClient.SqlCommand()
			cmd.Connection = conn
			Dim queryBuilder As StringBuilder = New StringBuilder()
			queryBuilder.Append("select * from Transactions where status in (")

			If Not isReversal Then
				queryBuilder.Append("'Authorize', 'Authorize-Fail', 'Authorize-Sent'")
			Else
				queryBuilder.Append("'Reverse','Reverse-Fail','Reverse-Sent'")
			End If

			queryBuilder.Append(")")
			cmd.CommandText = queryBuilder.ToString()
			Dim rs As System.Data.SqlClient.SqlDataReader
			rs = cmd.ExecuteReader()

			While rs.Read()
				Console.WriteLine(rs("TransactionID").ToString() & " was selected for processing.")
				cusAcc = rs("Customer_account")
				remarks = rs("remarks")
				amount = rs("amount")
				charges = rs("charges")

				If cusAcc.StartsWith("05") Then
					isIMAL = True
					beneFiciaryName = rs("Beneficiary").ToString()
				Else
					isIMAL = False
					beneFiciaryName = ""
				End If

				rtgsAcc = esy.getRTGSAccount(isIMAL)
				rtgsPL = esy.getPLAccount(isIMAL)
				rtgsSuspense = esy.getRTGSSuspense(isIMAL)

				Try

					If Not isReversal Then
						cusPay = pay(isIMAL, cusAcc, rtgsSuspense, beneFiciaryName, remarks, amount, explcode)
					Else
						explcode = GetReversalExplanationCode(explcode)
						cusPay = pay(isIMAL, rtgsSuspense, cusAcc, beneFiciaryName, "Reversal for transaction ID: " & rs("TransactionID").ToString(), amount, explcode)
					End If

					esy.TransactionAudit(cusPay, DateTime.Now, rs("TransactionID").ToString())
					Console.WriteLine(rs("TransactionID").ToString() & ":Branch Reversing principal..." & Constants.vbCrLf & cusPay)

                    If cusPay.Contains("Response Code:0") Or cusPay.Contains("responseCode:0") Then

                        If Not isReversal Then
                            cusPay = "Customer`s Account was debited SUCCESSFULLY. " & rtgsSuspense & " Credited."
                        Else
                            cusPay = "Customer`s Account was credited SUCCESSFULLY. " & rtgsSuspense & " Debited."
                        End If

                        esy.TransactionAudit(cusPay, DateTime.Now, rs("TransactionID").ToString())
                    Else

                        If Not isReversal Then
							cusPay = "Failed to debit customer account to credit " & rtgsSuspense
						Else
							cusPay = "Failed to credit customer account to debit " & rtgsSuspense
						End If

						esy.TransactionAudit(cusPay, DateTime.Now, rs("TransactionID").ToString())
					End If

				Catch ex As Exception
					LogException(ex)
					esy.createErrorLog(ex.Message, DateTime.Now)
					Console.WriteLine(rs("TransactionID").ToString() & ": Branch Debitting principal..." & Constants.vbCrLf & ex.Message)

					If ex.Message.Contains("TNS") Then
						cusPay = "Cannot connect to BANKS."
						esy.TransactionAudit(cusPay, DateTime.Now, rs("TransactionID").ToString())
					Else

						If Not isReversal Then
							cusPay = "Customer account debit Failed to complete."
						Else
							cusPay = "Customer account credit/reversal Failed to complete."
						End If

						esy.TransactionAudit(cusPay, DateTime.Now, rs("TransactionID").ToString())
					End If
				End Try

				Try

					If Not isReversal Then
						cusCharges = pay(isIMAL, cusAcc, rtgsSuspense, beneFiciaryName, remarks, charges, explcode)
					Else
						explcode = GetReversalExplanationCode(explcode)
						cusCharges = pay(isIMAL, rtgsSuspense, cusAcc, beneFiciaryName, remarks, charges, explcode)
					End If

					esy.TransactionAudit(cusCharges, DateTime.Now, rs("TransactionID").ToString())
					Console.WriteLine(rs("TransactionID").ToString() & ": Branch Debitting charge(s)..." & Constants.vbCrLf & cusCharges)

                    If cusCharges.Contains("Response Code:0") Or cusCharges.Contains("responseCode:0") Then

                        If Not isReversal Then
                            cusCharges = "Charge(s) was debited SUCCESSFULLY." & rtgsSuspense & " Credited."
                        Else
                            cusCharges = "Charge(s) was reversed SUCCESSFULLY." & rtgsSuspense & " Debited."
                        End If

                        esy.TransactionAudit(cusCharges, DateTime.Now, rs("TransactionID").ToString())
                    Else
                    End If

				Catch ex2 As Exception
					LogException(ex2)
					Console.WriteLine(rs("TransactionID").ToString() & ":Branch Debitting charge(s)..." & Constants.vbCrLf & ex2.Message)
					esy.createErrorLog(ex2.Message, DateTime.Now)

					If ex2.Message.Contains("TNS") Then
						cusCharges = "Cannot connect to BANKS."
						esy.TransactionAudit(cusCharges, DateTime.Now, rs("TransactionID").ToString())
					Else

						If Not isReversal Then
							cusCharges = "Charges Failed to complete."
						Else
							cusCharges = "Charges Reversal Failed to complete."
						End If

						esy.TransactionAudit(cusCharges, DateTime.Now, rs("TransactionID").ToString())
					End If
				End Try

				If cusPay.Contains("SUCCESSFULLY") And cusCharges.Contains("SUCCESSFULLY") Then

					If Not isReversal Then
						esy.UpdatetxnAsAuthorized(rs("TransactionID").ToString(), isIMAL)
					Else
						esy.UpdatetxnAsAuthorized(rs("TransactionID").ToString(), isIMAL, "Reversed")
					End If
				Else
					esy.UpdatetxnAsFailedAuthorizer(rs("TransactionID").ToString(), cusCharges)
				End If

				Console.WriteLine(rs("TransactionID").ToString() & " Completed." & Constants.vbCrLf & Constants.vbCrLf & "***********************************************")
				System.Threading.Thread.Sleep(2000)
			End While
		End Using
	End Sub

	Function pay(ByVal isIMAL As Boolean, ByVal fromAcc As String, ByVal toAccount As String, ByVal beneficiaryName As String, ByVal narration As String, ByVal amount As String, ByVal explcode As String) As String
		If isIMAL Then
			Dim imal As iMalWebservice = New iMalWebservice()
            '	imal.localTransfer(fromAcc, toAccount, amount, beneficiaryName, narration & " on RTGS at Branch")
            Dim resp = JsonConvert.DeserializeObject(Of LocalFTResp)(imal.localTransfer(fromAcc, toAccount, amount, narration & " on RTGS at Branch"))
            Return fromAcc & "-----> " & toAccount & " (" & beneficiaryName & ") :: Amount(" & amount & ") :-> Response Code:" & resp.responseCode.Trim() & ": " + resp.responseMessage.Trim()
		Else
			Dim fbc As String = ""
			Dim fcn As String = ""
			Dim fcc As String = ""
			Dim flc As String = ""
			Dim fsc As String = ""
			Dim tbc As String = ""
			Dim tcn As String = ""
			Dim tcc As String = ""
			Dim tlc As String = ""
			Dim tsc As String = ""
			Dim fa As Array
			fa = fromAcc.Split("-")
			fbc = fa(0)
			fcn = fa(1)
			fcc = fa(2)
			flc = fa(3)
			fsc = fa(4)
			Dim ta As Array
			ta = toAccount.Split("-")
			tbc = ta(0)
			tcn = ta(1)
			tcc = ta(2)
			tlc = ta(3)
			tsc = ta(4)
			Dim xg As XMLGenerator = New XMLGenerator("0000")
			xg.startDebit()
			xg.addAccount(fbc, fcn, fcc, flc, fsc, amount, explcode, narration)
			xg.addAccount(fbc, fcn, fcc, flc, fsc, "0.00", explcode, narration)
			xg.endDebit()
			xg.startCredit()
			xg.addAccount(tbc, tcn, tcc, tlc, tsc, amount, explcode, narration)
			xg.addAccount(tbc, tcn, tcc, tlc, tsc, "0.00", explcode, narration)
			xg.endCredit()
			xg.closeXML()
			Dim req As String = xg.req
			Dim vt As paytest.supa = New paytest.supa()
			xg.resp = vt.SupaTrx(req, "rtgs")
			xg.parseResponse()
			Dim responseText As String = ""
			Dim responseCode As String = ""
			responseText = xg.debits(0).ChildNodes(10).InnerText
			responseCode = xg.debits(0).ChildNodes(8).InnerText
			Return fromAcc & "-----> " & toAccount & ":: Amount(" & amount & ") :-> Response Code:" & responseCode.Trim() & ": " & responseText
		End If
	End Function

	Private Function GetReversalExplanationCode(ByVal explcode As String) As String
		Return explcode
	End Function

	Private Sub Do_New_Posting_Steps()
		Dim cnt As Integer
		Dim query As String = "Select * from Win32_Process Where Name = 'easiRTGSBatch.exe'"
		Dim searcher As ManagementObjectSearcher = New ManagementObjectSearcher(query)
		Dim processList As ManagementObjectCollection = searcher.[Get]()
		cnt = processList.Count

		If cnt > 1 Then
			System.Environment.[Exit](0)
		Else
			Post()
		End If
	End Sub

	Private Sub Post()
		Dim ds As DataSet = Nothing
		Dim isReversal As Boolean = False
		Dim statusArray As String() = New String() {"", "", "", ""}
		statusArray(StatusTypesIndex.Initial_Status_Index) = "Authorize"
		statusArray(StatusTypesIndex.Sent_Status_Index) = "Authorize-Sent"
		statusArray(StatusTypesIndex.Fail_Status_Index) = "Authorize-Fail"
		statusArray(StatusTypesIndex.Success_Status_Index) = "Authorized"
		ds = GetTransactionEntriesNormal(statusArray)
		DoPostingNormal(ds, statusArray, isReversal)
		AggregateSuccessfullyPostedAndMoveToTreasury(statusArray(StatusTypesIndex.Success_Status_Index))
		statusArray(StatusTypesIndex.Initial_Status_Index) = "Pay"
		statusArray(StatusTypesIndex.Sent_Status_Index) = "Pay-Sent"
		statusArray(StatusTypesIndex.Fail_Status_Index) = "Pay-Fail"
		statusArray(StatusTypesIndex.Success_Status_Index) = "Paid"
		ds.Clear()
		ds = GetTransactionEntriesNormal(statusArray)
		DoPostingNormal(ds, statusArray, isReversal)
		AggregateSuccessfullyPostedAndMoveToTreasury(statusArray(StatusTypesIndex.Success_Status_Index))
		statusArray(StatusTypesIndex.Initial_Status_Index) = "Reverse"
		statusArray(StatusTypesIndex.Sent_Status_Index) = "Reverse-Sent"
		statusArray(StatusTypesIndex.Fail_Status_Index) = "Reverse-Fail"
		statusArray(StatusTypesIndex.Success_Status_Index) = "Reversed"
		ds.Clear()
		isReversal = True
		ds = GetTransactionEntriesNormal(statusArray)
		DoPostingNormal(ds, statusArray, isReversal)
		AggregateSuccessfullyPostedAndMoveToTreasury(statusArray(StatusTypesIndex.Success_Status_Index))
	End Sub

	Private Function GetTransactionEntriesNormal(ByVal statusArray As String()) As DataSet
		Dim ds As DataSet = Nothing
		Dim cn As SqlConnection = New SqlConnection(sqlconn())
		Dim currentYear As String = DateTime.Now.Year
		Dim currentMonthName As String = DateTimeFormatInfo.InvariantInfo.GetAbbreviatedMonthName(DateTime.Now.Month).ToUpper()

		Try
			cn.Open()

			Using cn
				Dim sqlBuilder As StringBuilder = New StringBuilder()
				Dim sql As String = String.Empty
				sqlBuilder.Append("SELECT * FROM TRANSACTTEMP2 WHERE STATUS IN ")
				sqlBuilder.Append("(")
				sqlBuilder.Append("'")
				sqlBuilder.Append(statusArray(StatusTypesIndex.Initial_Status_Index))
				sqlBuilder.Append("'")
				sqlBuilder.Append(")")
				sqlBuilder.Append(" AND CONVERT(DATE,ENTRYDATE)>=CONVERT(DATE,'01-" & currentMonthName & "-" & currentYear & "') AND Amount >= 0 ORDER BY ID ASC, EntryDate DESC")
				sql = sqlBuilder.ToString()
                Dim cmd As SqlCommand = New SqlCommand(sql, cn)
                cmd.CommandTimeout = 0
                Dim cmdadapter As SqlDataAdapter = New SqlDataAdapter(cmd)
                ds = New DataSet()
				cmdadapter.Fill(ds)
			End Using

		Catch ex As Exception
			LogException(ex)
		End Try

		Return ds
	End Function

	Private Function SanitizeRemark(ByVal remark As String) As String
		Dim newRemark As String = remark

		If remark.Length > 200 Then
			Dim partOne As String = remark.Substring(0, 184)
			Dim partTwo As String = remark.Substring(remark.Length - 16, 16)
			newRemark = partOne & partTwo
		End If

		newRemark = newRemark.Replace(Strings.ChrW(38), "-").Replace(Strings.ChrW(62), "").Replace(Strings.ChrW(60), "").Replace(Strings.ChrW(39), "").Replace(Strings.ChrW(34), "").Replace("'", "")
		Return newRemark
	End Function

	Private Sub DoPostingNormal(ByVal ds As DataSet, ByVal statusArray As String(), ByVal isReverse As Boolean)
		If ds IsNot Nothing AndAlso ds.Tables.Count > 0 AndAlso ds.Tables(0).Rows.Count > 0 Then
			Dim rowCollection As DataRowCollection = Nothing
			Dim dracct As String = String.Empty
			Dim cracct As String = String.Empty
			Dim remark As String = String.Empty
			Dim status As String = String.Empty
			Dim amt As Decimal = 0D
			Dim explcode As Integer = -1
			Dim id As Long = -1
			Dim sn As String = ""
			Dim customername As String = String.Empty
			Dim transRef As String = String.Empty
			Dim rowID As Integer = -1
			Dim connString = sqlconn()

			Using ds
				rowCollection = ds.Tables(0).Rows

				For Each r As DataRow In rowCollection
					sn = r("TransactionID").ToString
					amt = Convert.ToDecimal(r("Amount").ToString)
					explcode = Convert.ToInt32(r("expl_code").ToString)
					rowID = Convert.ToInt32(r("ID").ToString)
					remark = SanitizeRemark(Convert.ToString(r("Remarks").ToString))

					If Not isReverse Then
						cracct = Convert.ToString(r("CrCommAcct").ToString.Trim())
						dracct = Convert.ToString(r("DrCustAcct").ToString.Trim())
					Else
						cracct = Convert.ToString(r("DrCustAcct").ToString.Trim())
						dracct = Convert.ToString(r("CrCommAcct").ToString.Trim())
					End If

					customername = r("CustomerName").ToString
					transRef = IIf(IsDBNull(r("TransRef")), String.Empty, r("TransRef").ToString())
					status = IIf(IsDBNull(r("Status")), String.Empty, r("Status").ToString())
					Dim post As ThreadedChequeChargesPoster = New ThreadedChequeChargesPoster()
					post.connectionString = connString
					post.CreditAccountnumber = cracct
					post.DebitAccountnumber = dracct
					post.Explcode = explcode
					post.Amount = amt
					post.Remarks = remark
					post.CustomerName = customername
					post.ID = rowID
					post.SN = sn
					post.StatusArrays = statusArray
					post.TransactionReference = transRef
					post.Status = status

					Try
						post.Post()
					Catch ex As Exception
						LogException(ex)
					End Try
				Next
			End Using
		End If
	End Sub

	Private Function IsPostedBefore(ByVal validator As ThreadedChequeChargesPoster) As Boolean
		Dim postedBefore As Boolean = True

		If Not String.IsNullOrEmpty(validator.TransactionReference) And validator.Status.Trim().ToLower() <> "validated" Then
			postedBefore = True
		Else
			postedBefore = False
		End If

		Return postedBefore
	End Function

	Private Function CheckEntryOnBanks(ByVal validator As ThreadedChequeChargesPoster) As Boolean
		Dim entrydate As String = String.Empty
		Dim isOk As Boolean = False
		Dim cn As SqlConnection = New SqlConnection(sqlconn())
		cn.Open()
		Dim sqlGetEntrydate As String = "SELECT ENTRYDATE FROM TRANSACTTEMP2 WHERE ID=@ID"
		Dim cmdGetDate As SqlCommand = New SqlCommand(sqlGetEntrydate, cn)
		cmdGetDate.Parameters.AddWithValue("@ID", validator.ID)
		Dim drSQL As SqlDataReader = cmdGetDate.ExecuteReader()

		Using cn
			Using drSQL
				If drSQL IsNot Nothing AndAlso drSQL.Read Then
					entrydate = CDate(drSQL(0)).ToString("dd-MMM-yy").ToUpper().Trim
				End If
			End Using
		End Using

		Try
			Dim sqlBuff As StringBuilder = New StringBuilder()
			Dim sql As String = "SELECT * FROM TELL_ACT WHERE BRA_CODE=@BRA_CODE AND CUS_NUM=@CUS_NUM AND CUR_CODE=@CUR_CODE AND LED_CODE=@LED_CODE AND SUB_ACCT_CODE=@SUB_ACCT_CODE AND TRA_AMT=@TRA_AMT AND EXPL_CODE=@EXPL_CODE AND REMARKS=@REMARKS AND TELL_ID=9948 AND DEB_CRE_IND=1"
			Dim Ds As DataSet = New DataSet()
			Dim OracleConn As OracleConnection = New OracleConnection()
			Dim spwd As String = "banksys"
			OracleConn.ConnectionString = GetConnectionString("oracle_appusr")
			Dim Bra_codeDR As Integer
			Dim Cus_numDR As Integer
			Dim Cur_codeDR As Integer
			Dim Led_codeDR As Integer
			Dim Sub_Acct_codeDR As Integer
			Dim Tra_amt As Decimal
			Dim ExplanationCode As Integer
			Dim Remark As String = String.Empty
			Dim AcctParts As String() = validator.DebitAccountnumber.Split("-"c)
			Bra_codeDR = AcctParts(0)
			Cus_numDR = AcctParts(1)
			Cur_codeDR = AcctParts(2)
			Led_codeDR = AcctParts(3)
			Sub_Acct_codeDR = AcctParts(4)
			Tra_amt = validator.Amount
			ExplanationCode = validator.Explcode
			Remark = validator.Remarks
			sqlBuff.Append("SELECT BRA_CODE,CUS_NUM,CUR_CODE, LED_CODE, SUB_ACCT_CODE, TRA_AMT, EXPL_CODE, DEB_CRE_IND, TELL_ID, REMARKS FROM TELL_ACT WHERE ")
			sqlBuff.Append("BRA_CODE=")
			sqlBuff.Append(Convert.ToInt32(Bra_codeDR))
			sqlBuff.Append(" AND CUS_NUM=")
			sqlBuff.Append(Convert.ToInt32(Cus_numDR))
			sqlBuff.Append(" AND CUR_CODE=")
			sqlBuff.Append(Convert.ToInt32(Cur_codeDR))
			sqlBuff.Append(" AND LED_CODE=")
			sqlBuff.Append(Convert.ToInt32(Led_codeDR))
			sqlBuff.Append(" AND SUB_ACCT_CODE=")
			sqlBuff.Append(Convert.ToInt32(Sub_Acct_codeDR))
			sqlBuff.Append(" AND TRA_AMT=")
			sqlBuff.Append(Convert.ToDecimal(Tra_amt))

			If validator.Explcode <> 999 Then
				sqlBuff.Append(" AND EXPL_CODE=")
				sqlBuff.Append(Convert.ToInt32(ExplanationCode))
			End If

			sqlBuff.Append(" AND REMARKS=")
			sqlBuff.Append("'")
			sqlBuff.Append(Remark.ToString().ToUpper().Trim())
			sqlBuff.Append("'")
			sqlBuff.Append(" AND TELL_ID=9948 AND DEB_CRE_IND=1")

			If Not String.IsNullOrEmpty(entrydate) Then
				sqlBuff.Append(" AND  TRA_DATE>='")
				sqlBuff.Append(entrydate)
				sqlBuff.Append("'")
			End If

			sqlBuff.Append(" UNION ")
			sqlBuff.Append("SELECT BRA_CODE,CUS_NUM,CUR_CODE, LED_CODE, SUB_ACCT_CODE, TRA_AMT, EXPL_CODE, DEB_CRE_IND, TELL_ID, REMARKS FROM TRANSACT WHERE ")
			sqlBuff.Append("BRA_CODE=")
			sqlBuff.Append(Convert.ToInt32(Bra_codeDR))
			sqlBuff.Append(" AND CUS_NUM=")
			sqlBuff.Append(Convert.ToInt32(Cus_numDR))
			sqlBuff.Append(" AND CUR_CODE=")
			sqlBuff.Append(Convert.ToInt32(Cur_codeDR))
			sqlBuff.Append(" AND LED_CODE=")
			sqlBuff.Append(Convert.ToInt32(Led_codeDR))
			sqlBuff.Append(" AND SUB_ACCT_CODE=")
			sqlBuff.Append(Convert.ToInt32(Sub_Acct_codeDR))
			sqlBuff.Append(" AND TRA_AMT=")
			sqlBuff.Append(Convert.ToDecimal(Tra_amt))

			If validator.Explcode <> 999 Then
				sqlBuff.Append(" AND EXPL_CODE=")
				sqlBuff.Append(Convert.ToInt32(ExplanationCode))
			End If

			sqlBuff.Append(" AND REMARKS=")
			sqlBuff.Append("'")
			sqlBuff.Append(Remark.ToString().ToUpper())
			sqlBuff.Append("'")
			sqlBuff.Append(" AND TELL_ID=9948 AND DEB_CRE_IND=1")

			If Not String.IsNullOrEmpty(entrydate) Then
				sqlBuff.Append(" AND  TRA_DATE>='")
				sqlBuff.Append(entrydate)
				sqlBuff.Append("'")
			End If

			sql = sqlBuff.ToString()

			Using OracleConn
				OracleConn.Open()
				Dim myCMD As OracleCommand = OracleConn.CreateCommand
				myCMD.CommandText = sql
				myCMD.CommandType = CommandType.Text
				Dim dr As OracleDataReader = myCMD.ExecuteReader()

				Using dr

					If dr.HasRows Then
						isOk = False
					Else
						isOk = True
					End If
				End Using
			End Using

		Catch ex As Exception
			LogException(ex)
			Throw
		End Try

		Return isOk
	End Function

	Private Function GetConnectionString(ByVal key As String) As String
		Dim conn As String = ""

		If key.ToLower().CompareTo("oracle_appusr") = 0 Then

			Try
				conn = OneConfig.Text.[Get](key)
			Catch ex As Exception
				conn = "user id=appusr;password=testapp;data source=(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST=10.0.0.174)(PORT=1590))(CONNECT_DATA=(SID = HOBANK1)))"
			End Try

			If String.IsNullOrEmpty(conn) Then conn = "user id=appusr;password=testapp;data source=(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST=10.0.0.174)(PORT=1590))(CONNECT_DATA=(SID = HOBANK1)))"
		Else
			conn = "user id=appusr;password=testapp;data source=(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST=10.0.0.174)(PORT=1590))(CONNECT_DATA=(SID = HOBANK1)))"
		End If

		Return conn
	End Function

	Public Class ThreadedChequeChargesPoster
		Public Const UPLOADED_STATUS As String = "Uploaded"
		Public Const SERVER_ERROR_STATUS As String = "Server Error"
		Public Const SENT_STATUS As String = "Sent"
		Public Const VALIDATED_STATUS As String = "Validated"

		Public Sub New()
		End Sub

		Public Sub New(ByVal conn As IDbConnection, ByVal id As Integer, ByVal sn As Long, ByVal creditAcctNum As String, ByVal debitAcctNum As String, ByVal amt As Decimal, ByVal explcode As Integer, ByVal remark As String)
			Me.connection = conn
			Me.connectionString = conn.ConnectionString
			Me.ID = id
			Me.SN = sn
			Me.CreditAccountnumber = creditAcctNum
			Me.DebitAccountnumber = debitAcctNum
			Me.Amount = amt
			Me.Explcode = explcode
			Me.Remarks = remark
		End Sub

		Public Function Post() As Boolean
			Dim vtellerAppEndPoint As String = "easyrtgs"
			Dim status As Boolean = False
			Dim xg As XMLGenerator = New XMLGenerator(Teller_Id)

			If Amount = 0 Then
				updateTransactionRef("FT_ZeroAmount")
				markUploaded()
			End If

			If Amount > 0 Then

				If Not IsPostedBefore(Me) Then

                    If markSent() Then
                        Dim t24 As T24Bank = New T24Bank()
                        Dim imal As iMalWebservice = New iMalWebservice()
                        Dim cur As String
                        Dim transferRes As ITransactionResult = Nothing
                        Dim acct As IAccount = Nothing
                        Dim resp As LocalFTResp = Nothing
                        Explcode = 111

                        If (CreditAccountnumber.StartsWith("05")) Then
                            Dim details = JsonConvert.DeserializeObject(Of AccountDetailsResp)(imal.getAccountDetails(DebitAccountnumber))
                            cur = details.currencyCode
                        Else
                            acct = t24.GetAccountInfoByAccountNumber(DebitAccountnumber)
                            cur = acct.CurrencyCode
                        End If

                        If (CreditAccountnumber.StartsWith("05")) Then
                            resp = JsonConvert.DeserializeObject(Of LocalFTResp)(imal.localTransfer(DebitAccountnumber, CreditAccountnumber, Amount, Remarks))
                            'If resp.responseCode.Equals("0") Then
                            '        transferRes.Status = True
                            '        transferRes.Message = resp.responseMessage
                            '        transferRes.TransactionReference = resp.iMALTransactionCode

                            '    Else
                            '        transferRes.Status = False
                            '        transferRes.Message = resp.errorMessage
                            '        transferRes.TransactionReference = resp.iMALTransactionCode
                            '    End If

                        Else
                                If (CreditAccountnumber.ToUpper().StartsWith("PL")) Then
                                    transferRes = t24.TransferInternalAccount(DebitAccountnumber, CreditAccountnumber, acct.CurrencyCode, acct.BranchCode, "FT", Amount, Remarks, 111, vtellerAppEndPoint)
                                Else
                                    transferRes = t24.Transfer(DebitAccountnumber, CreditAccountnumber, "FT", Amount, Remarks, 111, vtellerAppEndPoint)
                                End If
                            End If

                        Try

                            If (CreditAccountnumber.StartsWith("05")) Then
                                If (resp.responseCode.Equals("0")) Then
                                    markUploaded()
                                    updateTransactionRef(resp.iMALTransactionCode)
                                Else
                                    markServerError()
                                    UpdateRequestErrorText(resp.errorMessage.Substring(0, 225))
                                End If

                            Else

                                    If transferRes IsNot Nothing Then

                                    If transferRes.Status = True Then
                                        markUploaded()
                                        updateTransactionRef(transferRes.TransactionReference)
                                    Else
                                        markServerError()
                                        Dim msg As String = String.Empty

                                        Try

                                            If transferRes.Message.Length > 255 Then
                                                msg = transferRes.Message.Substring(0, 255)
                                            Else
                                                msg = transferRes.Message
                                            End If

                                        Catch ex As Exception
                                        End Try

                                        UpdateRequestErrorText(msg)
                                    End If
                                Else
                                    markServerError()
                                    UpdateRequestErrorText("Could not perform FT. Result object was NULL.")
                                End If

                            End If
                        Catch ex As Exception
                            status = False
                                Console.WriteLine("An error occurred while retrieving the status from VTeller XML generator object.")
                                If ex.InnerException IsNot Nothing Then Console.WriteLine(Constants.vbTab & "There are inner exception message: " & ex.InnerException.Message)
                            End Try
                        End If
                    Else
					markUploaded()
				End If
			End If

			Return status
		End Function

		Private Function SanitizeRemark(ByVal remark As String) As String
			remark = remark.Replace(Strings.ChrW(38), "-").Replace(Strings.ChrW(62), "").Replace(Strings.ChrW(60), "").Replace(Strings.ChrW(39), "").Replace(Strings.ChrW(34), "")
			Dim newRemark As String = remark

			If remark.Length > 200 Then
				Dim partOne As String = remark.Substring(0, 184)
				Dim partTwo As String = remark.Substring(remark.Length - 16, 16)
				newRemark = partOne & partTwo
			End If

			Return newRemark
		End Function

		Public Function markSent() As Boolean
			Dim status As Boolean = False
			Dim cn As SqlConnection = New SqlConnection(connectionString)

			If cn Is Nothing Then
				Return False
			ElseIf cn.State <> ConnectionState.Open Then

				Try
					cn.Open()

					Using cn
						Dim cmd As SqlCommand = New SqlCommand("update Transacttemp2 set status=@status where id=@id and TransactionID=@sn", cn)

						Using cmd
							cmd.Parameters.AddWithValue("@status", StatusArrays(StatusTypesIndex.Sent_Status_Index))
							cmd.Parameters.AddWithValue("@id", ID)
							cmd.Parameters.AddWithValue("@sn", SN)
							Dim rt As Integer = cmd.ExecuteNonQuery()
							If rt > 0 Then status = True
						End Using
					End Using

				Catch ex As Exception
					LogException(ex)
					status = True
				End Try
			End If

			Return status
		End Function

		Private Function updateTransactionRef(ByVal tra_seq As String) As Boolean
			Dim status As Boolean = False
			Dim cn As SqlConnection = New SqlConnection(connectionString)

			If cn Is Nothing Then
				Return False
			ElseIf cn.State <> ConnectionState.Open Then

				Try
					cn.Open()

					Using cn
						Dim cmd As SqlCommand = New SqlCommand("update Transacttemp2 set TransRef=@TransRef where id=@id and TransactionID=@sn", cn)

						Using cmd
							cmd.Parameters.AddWithValue("@TransRef", tra_seq)
							cmd.Parameters.AddWithValue("@id", ID)
							cmd.Parameters.AddWithValue("@sn", SN)
							Dim rt As Integer = cmd.ExecuteNonQuery()
							If rt >= 0 Then status = True
						End Using
					End Using

				Catch ex As Exception
				End Try
			End If

			Return status
		End Function

		Public Function markUploaded() As Boolean
			Dim status As Boolean = False
			Dim cn As SqlConnection = New SqlConnection(connectionString)

			If cn Is Nothing Then
				Return False
			ElseIf cn.State <> ConnectionState.Open Then

				Try
					cn.Open()

					Using cn
						Dim cmd As SqlCommand = New SqlCommand("update Transacttemp2 set status=@status where id=@id and TransactionID=@sn", cn)

						Using cmd
							cmd.Parameters.AddWithValue("@status", StatusArrays(StatusTypesIndex.Success_Status_Index))
							cmd.Parameters.AddWithValue("@id", ID)
							cmd.Parameters.AddWithValue("@sn", SN)
							Dim rt As Integer = cmd.ExecuteNonQuery()
							If rt >= 0 Then status = True
						End Using
					End Using

				Catch ex As Exception
					LogException(ex)
				End Try
			End If

			Return status
		End Function

		Public Function markServerError() As Boolean
			Dim status As Boolean = False
			Dim cn As SqlConnection = New SqlConnection(connectionString)

			If cn Is Nothing Then
				Return False
			ElseIf cn.State <> ConnectionState.Open Then

				Try
					cn.Open()

					Using cn
						Dim cmd As SqlCommand = New SqlCommand("update Transacttemp2 set status=@status where id=@id and TransactionID=@sn", cn)

						Using cmd
							cmd.Parameters.AddWithValue("@status", StatusArrays(StatusTypesIndex.Fail_Status_Index))
							cmd.Parameters.AddWithValue("@id", ID)
							cmd.Parameters.AddWithValue("@sn", SN)
							Dim rt As Integer = cmd.ExecuteNonQuery()
							If rt >= 0 Then status = True
						End Using
					End Using

				Catch ex As Exception
					LogException(ex)
				End Try
			End If

			Return status
		End Function

		Public Function UpdateRequestErrorText(ByVal errText As String) As Boolean
			Dim status As Boolean = False
			Dim cn As SqlConnection = New SqlConnection(connectionString)

			If cn Is Nothing Then
				Return False
			ElseIf cn.State <> ConnectionState.Open Then

				Try
					cn.Open()

					Using cn
						Dim cmd As SqlCommand = New SqlCommand("update Transacttemp2 set ErrorText=@errText where id=@id and TransactionID=@sn", cn)

						Using cmd
							cmd.Parameters.AddWithValue("@errText", errText)
							cmd.Parameters.AddWithValue("@id", ID)
							cmd.Parameters.AddWithValue("@sn", SN)
							Dim rt As Integer = cmd.ExecuteNonQuery()
							If rt >= 0 Then status = True
						End Using
					End Using

				Catch ex As Exception
					LogException(ex)
				End Try
			End If

			Return status
		End Function

		Public Property DebitAccountnumber As String
		Public Property CreditAccountnumber As String
		Public Property Explcode As Integer
		Public Property Remarks As String
		Public Property Amount As Decimal
		Public Property Status As String
		Public Property ID As Long
		Public Property SN As String
		Public Property CustomerName As String
		Public Property connection As IDbConnection
		Public Const AcctSepChar As String = "-"c
		Public Const Teller_Id As String = "9948"
		Public Property connectionString As String
		Public Property StatusArrays As String()
		Public Property TransactionReference As String
	End Class

	Public Enum StatusTypesIndex
		Initial_Status_Index = 0
		Sent_Status_Index = 1
		Fail_Status_Index = 2
		Success_Status_Index = 3
	End Enum

	Private Sub AggregateSuccessfullyPostedAndMoveToTreasury(ByVal finalStatus As String)
		Dim reqid As String = String.Empty
		Dim dsPostedRequestIds As DataSet = GetSuccessfullyPostedIds(finalStatus)

		Try

			For Each row As DataRow In dsPostedRequestIds.Tables(0).Rows
				reqid = row("TransactionID").ToString()
				UpdateAndBackupRequestId(reqid, finalStatus)
			Next

		Catch ex As Exception
			LogException(ex)
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

	Private Function GetSuccessfullyPostedIds(ByVal finalStatus As String) As DataSet
		Dim ds As DataSet = New DataSet()
		Dim cn As SqlConnection = New SqlConnection(sqlconn())

		Using cn

			Try
				cn.Open()
				Dim sql As String = "spGetSuccessfullyPostedRequestIds"
				Dim cmd As SqlCommand = New SqlCommand()
				cmd.Connection = cn
				cmd.CommandText = sql
				cmd.CommandType = CommandType.StoredProcedure
				cmd.Parameters.AddWithValue("@finalStatus", finalStatus.Trim())
				cmd.CommandTimeout = 0
				Dim dr As SqlDataAdapter = New SqlDataAdapter(cmd)
				dr.Fill(ds)
			Catch ex As Exception
				LogException(ex)
			End Try
		End Using

		Return ds
	End Function

	Private Sub UpdateAndBackupRequestId(ByVal reqid As String, ByVal finalStatus As String)
		Dim transactionid As String = reqid
		Dim cn As SqlConnection = New SqlConnection(sqlconn())

		Using cn

			Try
				cn.Open()
				Dim trans As SqlTransaction = cn.BeginTransaction()
				Dim isUpdatedTrans As Boolean = False
				Dim isBackedUp As Boolean = False
				Dim cmdUpdate As SqlCommand = New SqlCommand("UPDATE transactions SET Status=@status WHERE TransactionID=@transid", cn, trans)
				Dim cmdBackup As SqlCommand = New SqlCommand("spBackUpTransactions", cn, trans)
				cmdUpdate.CommandType = CommandType.Text
				cmdBackup.CommandType = CommandType.StoredProcedure

				Using trans

					Try
						cmdUpdate.Parameters.AddWithValue("@status", finalStatus)
						cmdUpdate.Parameters.AddWithValue("@transid", transactionid)

						If cmdUpdate.ExecuteNonQuery() >= 0 Then
							isUpdatedTrans = True
						Else
							isUpdatedTrans = False
						End If

					Catch ex As Exception

						Try
							trans.Rollback()
						Catch ex2 As Exception
							LogException(ex2)
						End Try

						LogException(ex)
					End Try

					Try
						cmdBackup.Parameters.AddWithValue("@transactionid", transactionid)
						cmdBackup.Parameters.AddWithValue("@status", finalStatus)

						If cmdBackup.ExecuteNonQuery() >= 0 Then
							isBackedUp = True
						Else
							isBackedUp = False
						End If

					Catch ex As Exception
						LogException(ex)
					End Try

					If isUpdatedTrans = True AndAlso isBackedUp = True Then

						Try
							trans.Commit()
						Catch ex As Exception

							Try
								trans.Rollback()
							Catch ex2 As Exception
								LogException(ex2)
							End Try

							LogException(ex)
						End Try
					Else

						Try
							trans.Rollback()
						Catch ex As Exception
							LogException(ex)
						End Try
					End If
				End Using

			Catch ex As Exception
				LogException(ex)
			End Try
		End Using
	End Sub

	Private Sub Reset_Sql()
		Dim sqlBuilder As StringBuilder = New StringBuilder()
		Dim sql As String = "select * from TransactTemp2 where convert(date,entrydate)=convert(date,getdate()) and Status='Paid-Sent'"
		Dim cn As SqlConnection = New SqlConnection(sqlconn())

		Using cn

			Try
				cn.Open()
				Dim cmd As SqlCommand = New SqlCommand(sql, cn)
				Dim ad As SqlDataAdapter = New SqlDataAdapter(cmd)
				Dim dsd As DataSet = New DataSet()
				ad.Fill(dsd)

				If dsd.Tables.Count > 0 Then

					If dsd.Tables(0).Rows.Count > 0 Then
					End If
				End If

				cmd.CommandTimeout = 0
				cmd.ExecuteNonQuery()
			Catch ex As Exception
			End Try
		End Using
	End Sub
End Module
