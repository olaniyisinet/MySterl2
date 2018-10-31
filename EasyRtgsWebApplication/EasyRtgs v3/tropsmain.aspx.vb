﻿Imports System.Net
Imports System.Data
Imports BankCore.T24
Imports BankCore
Imports Newtonsoft.Json

Partial Class main
    Inherits System.Web.UI.Page
    Private tid As String = ""
    Private ess As New easyrtgs
    Private ledcode As String = ""
    Private instruction As String = ""

    Public Function generateRef() As String
        Dim ref As String = ConfigurationManager.AppSettings("REF_PREFIX") & DateTime.Now.ToString("yyMMddHHmmss") & GenerateRndNumber(1)
        Return ref
    End Function

    Protected Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init
        'Dim rnd As New System.Random(Second(Now))

        tid = generateRef()
        'TextBox6.Visible = False

        'For Each dr As Data.DataRow In ess.getBanks().Tables(0).Rows
        '    DropDownList1.Items.Add(New ListItem(dr("bank_name").ToString, dr("bank_code").ToString))
        'Next


    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        If Not IsPostBack Then
            If Session("uname") = "" Then

                Server.Transfer("default.aspx")

            Else
                Label1.Text = Session("name")

                Label4.Text = "N " & ess.getCustomerCharges()
                Label5.Text = "N " & ess.getStaffCharges()


            End If
            Dim role As String = Session("role").ToString()
            Dim transferType As String = Session("transtype").ToString()
            Dim messageType As String = Session("messagetype").ToString()
            Dim messageVariant As String = String.Empty
            If Session("variant") IsNot Nothing Then
                messageVariant = Session("variant").ToString()
            Else
                messageVariant = "N/A"
            End If

            lblMessageVariant.Text = "<b>" & messageVariant & "</b>"
            lblMessageType0.Text = "<b>" & messageVariant & "</b>"
            lblMessageType.Text = "<b>" & transferType & "</b>"
            ModifyUserInterfaceByRoleTransferTypeAndVariant(role, transferType, messageType, messageVariant)

        End If

    End Sub
    Public Function GenerateRndNumber(ByVal cnt As Integer) As String
        Dim key2 As String() = {"0", "1", "2", "3", "4", "5",
         "6", "7", "8", "9"}
        Dim rand1 As New Random()
        Dim txt As String = ""
        For j As Integer = 0 To cnt - 1
            txt += key2(rand1.[Next](0, 9))
        Next
        Return txt
    End Function

    Public Function newSessionId(ByVal bankcode As String) As String
        System.Threading.Thread.Sleep(50)
        Return (Convert.ToString("232") & bankcode) + DateTime.Now.ToString("yyMMddHHmmss") + GenerateRndNumber(12)
    End Function


    Function ret() As Boolean
        Return True

    End Function

    Sub benefName(ByVal sender As Object, ByVal e As System.EventArgs)

        Dim bank_code As String = DropDownList1.SelectedValue
        Dim accountNum As String = txtBenAccount103.Text

        Dim gad As New Gadget()

        Dim name As String = gad.DoNameQueryNew(bank_code, accountNum)



        'The dictionary will not contain the keys "RESPONSECODE" or "RESPONSETEXT" if the 
        'name enquiry failed. So check before retrieving to avoid embarrassing error messages

        Dim appMode As String = ConfigurationManager.AppSettings("APP_MODE").ToString()
        If appMode.Trim().ToLower() = "offline" Then
            txtBenName103.Visible = True
            If Session("name") IsNot Nothing Then
                txtBenName103.Text = Session("name").ToString() 'This is just to get a name out for test purposes. if the name enquiry fails is not likely to succeed. Say because u are running in demo mode.
            Else
                txtBenName103.Text = "John Smith"
            End If
        Else

            If Not String.IsNullOrEmpty(name) Then

                txtBenName103.Visible = True
                txtBenName103.Text = name

            Else
                LinkButton1.Visible = True
                Label2.ForeColor = Drawing.Color.Red
                Label2.Text = "<table id='output' bgcolor='#ccffcc' width='400'><tr><td valign='middle' width='370'>Error: " & "Could not retrieve the name of this account: Message: " & name & "</td><td valign='middle' width='30'><img src='images\bad.png' width='20' title='close' onclick='hide();'></td></tr></table>"
            End If
        End If



    End Sub

    'Sub benefName(ByVal sender As Object, ByVal e As System.EventArgs)

    '    Dim bank_code As String = DropDownList1.SelectedValue
    '    Dim accountNum As String = txtBenAccount103.Text

    '    Dim gad As New Gadget
    '    Dim name As String = gad.DoNameEnquiryOutput(bank_code, accountNum)
    '    Dim dict As Dictionary(Of String, String) = gad.parseXmlResponse(name)

    '    'The dictionary will not contain the keys "RESPONSECODE" or "RESPONSETEXT" if the 
    '    'name enquiry failed. So check before retrieving to avoid embarrassing error messages
    '    If dict.ContainsKey("RESPONSECODE") AndAlso dict.ContainsKey("RESPONSETEXT") Then
    '        Dim code As String = dict("RESPONSECODE")
    '        Dim nameval As String = dict("RESPONSETEXT")
    '        Dim appMode As String = ConfigurationManager.AppSettings("APP_MODE").ToString()
    '        If appMode.Trim().ToLower() = "offline" Then
    '            txtBenName103.Visible = True
    '            If Session("name") IsNot Nothing Then
    '                txtBenName103.Text = Session("name").ToString() 'This is just to get a name out for test purposes. if the name enquiry fails is not likely to succeed. Say because u are running in demo mode.
    '            Else
    '                txtBenName103.Text = "John Smith"
    '            End If
    '        Else
    '            If Not String.IsNullOrEmpty(code) AndAlso code.Trim().CompareTo("00") = 0 Then

    '                txtBenName103.Visible = True
    '                txtBenName103.Text = nameval

    '            Else
    '                LinkButton1.Visible = True
    '                Label2.ForeColor = Drawing.Color.Red
    '                Label2.Text = "<table id='output' bgcolor='#ccffcc' width='400'><tr><td valign='middle' width='370'>Error: " & "Could not retrieve the name of this account: Message: " & nameval & "</td><td valign='middle' width='30'><img src='images\bad.png' width='20' title='close' onclick='hide();'></td></tr></table>"
    '            End If
    '        End If

    '        'If Not String.IsNullOrEmpty(code) AndAlso code.Trim().CompareTo("00") = 0 Then

    '        '    txtBenName103.Visible = True
    '        '    txtBenName103.Text = nameval

    '        'Else
    '        '    LinkButton1.Visible = True
    '        '    Label2.ForeColor = Drawing.Color.Red
    '        '    Label2.Text = "<table id='output' bgcolor='#ccffcc' width='400'><tr><td valign='middle' width='370'>Error: " & "Could not retrieve the name of this account: Message: " & nameval & "</td><td valign='middle' width='30'><img src='images\bad.png' width='20' title='close' onclick='hide();'></td></tr></table>"
    '        'End If
    '    Else
    '        Label2.Text = "<table id='output' bgcolor='#ccffcc' width='400'><tr><td valign='middle' width='370'>Name Enquiry Failed: " & "Could not retrieve the name of this account: Message: Please confirm the bank and account number then try again or the bank could be offline.</td><td valign='middle' width='30'><img src='images\bad.png' width='20' title='close' onclick='hide();'></td></tr></table>"
    '    End If
    'End Sub




    Sub show2(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim inputAcct As String = TextBox1.Text
        Dim ds As DataSet = New DataSet()
        Dim outputNuban As String = String.Empty
        Dim acctName As String = String.Empty
        Dim availBal As String = String.Empty
        Dim oldAcctNumberFull As String = String.Empty
        Dim sep As String = "-"
        Dim g As New Gadget()
        Label2.Text = ""
        Dim bnk As New T24Bank()
        Dim acct As IAccount = Nothing


        Try
            'If Not IsPostBack Then
            Dim ac As String = ""
            '   Dim account As String=  TextBox5.Text & textbox6.Text & TextBox7.Text & textbox8.Text & TextBox9.Text

            'If inputAcct.Trim.Length = 10 Then
            '    'ac = getOldAcc(inputAcct.Trim)
            '    ds = bnk.getAccountByNUBAN(inputAcct)

            'Else
            '    ac = inputAcct.Trim
            '    ds = bnk.getAccountFullinfo(ac)
            'End If

            If inputAcct.StartsWith("05") Then

                If inputAcct.Trim().Length = 10 Then
                    Dim imal As iMalWebservice = New iMalWebservice()
                    '	imal.getAccountDetails(inputAcct)
                    Dim resp = JsonConvert.DeserializeObject(Of AccountDetailsResp)(imal.getAccountDetails(inputAcct))
                    acctName = resp.name
                    availBal = resp.availableBalance
                    availBal = availBal * -1
                    oldAcctNumberFull = inputAcct 'should be modified later
                    Session("acc") = inputAcct
                    Session("ledcode") = resp.glCode
                    Session.Timeout = 30
                    lblBalance103.Text = Strings.FormatNumber(availBal, 2)
                    txtCustName103.Text = acctName
                    Session("bal") = Strings.FormatNumber(availBal, 2)
                    Session("cname") = txtCustName103.Text
                    Session.Timeout = 10
                    Button3.Value = "Get Customer Data"
                    Button3.Disabled = False
                Else
                    Label2.Text = "<font color=red>Please enter 10 digits iMAL account number.</font>"
                End If
            Else
                acct = bnk.GetAccountInfoByAccountNumber(inputAcct)
                If acct IsNot Nothing Then
                    Dim drResult As DataRow = g.GetDataRow(ds)
                    outputNuban = acct.AccountNumberRepresentations(Account.NUBAN).Representation
                    acctName = acct.CustomerName 'drResult("CUS_SHO_NAME").ToString()
                    availBal = acct.UsableBal '(Convert.ToDecimal(drResult("cle_bal")) - Convert.ToDecimal(drResult("for_amt")) - Convert.ToDecimal(drResult("bal_lim")) - Convert.ToDecimal(drResult("tot_blo_fund")) + Convert.ToDecimal(drResult("risk_limit"))).ToString()
                    oldAcctNumberFull = String.Empty
                    Dim acctparts() As String = oldAcctNumberFull.Split(sep)
                    Session("acc") = oldAcctNumberFull
                    Session("ledcode") = acct.ProductCode
                    Session.Timeout = 30
                    lblBalance103.Text = FormatNumber(availBal, 2)
                    txtCustName103.Text = acctName
                    Session("bal") = FormatNumber(availBal, 2)
                    Session("cname") = txtCustName103.Text
                    Session.Timeout = 10

                    Button3.Value = "Get Customer Data"
                    Button3.Disabled = False
                Else
                    'if the acct object is NULL.
                End If

            End If

        Catch ex As Exception
            Gadget.LogException(ex)
            If ex.Message.Contains("TNS") Or ex.Message.Contains("OracleConnection") Then
                Label2.ForeColor = Drawing.Color.Red
                Label2.Text = "<table id='output' bgcolor='#ccffcc' width='400'><tr><td valign='middle' width='370'>" & "Cannot connect to BANKS" & "</td><td valign='middle' width='30'><img src='images\bad.png' width='20' title='close' onclick='hide();'></td></tr></table>"

            Else

                Label2.ForeColor = Drawing.Color.Red
                Label2.Text = "<table id='output' bgcolor='#ccffcc' width='400'><tr><td valign='middle' width='370'>" & ex.Message() & "</td><td valign='middle' width='30'><img src='images\bad.png' width='20' title='close' onclick='hide();'></td></tr></table>"

            End If
        End Try



    End Sub

    'Sub show2(ByVal sender As Object, ByVal e As System.EventArgs)
    '    Label2.Text = ""
    '    Dim bnk As New bank.bank


    '    Try
    '        If IsPostBack Then

    '            Dim ac As String = ""
    '            '   Dim account As String=  TextBox5.Text & textbox6.Text & TextBox7.Text & textbox8.Text & TextBox9.Text

    '            If TextBox1.Text.Trim.Length = 10 Then
    '                ac = getOldAcc(TextBox1.Text.Trim)

    '            Else
    '                ac = TextBox1.Text.Trim

    '            End If


    '            Dim nuban As String = ""



    '            For Each dr As Data.DataRow In bnk.getAccount(ac).Tables(0).Rows


    '                Session("acc") = dr("BC") & "-" & dr("CN") & "-" & dr("CC") & "-" & dr("LC") & "-" & dr("SC")
    '                Session("ledcode") = dr("LC")
    '                Session.Timeout = 30


    '                For Each drN As Data.DataRow In bnk.getNuban(dr("BC"), dr("CN"), dr("CC"), dr("LC"), dr("SC")).Tables(0).Rows
    '                    nuban = drN("MAP_ACC_NO").ToString

    '                Next


    '            Next

    '            txtCustName103.Text = getAccountName(nuban)


    '            Dim accbal As New Data.DataSet
    '            accbal = bnk.getBalance(ac)

    '            For Each dr As Data.DataRow In accbal.Tables(0).Rows
    '                lblBalance103.Text = FormatNumber(dr("cle_bal"), 2)
    '                Session("bal") = FormatNumber(dr("cle_bal"), 2)
    '                Session("cname") = txtCustName103.Text
    '                Session.Timeout = 10
    '            Next

    '            Button3.Value = "Get Customer Data"
    '            Button3.Disabled = False

    '        End If

    '    Catch ex As Exception
    '        Gadget.LogException(ex)
    '        If ex.Message.Contains("TNS") Or ex.Message.Contains("OracleConnection") Then
    '            Label2.ForeColor = Drawing.Color.Red
    '            Label2.Text = "<table id='output' bgcolor='#ccffcc' width='400'><tr><td valign='middle' width='370'>" & "Cannot connect to BANKS" & "</td><td valign='middle' width='30'><img src='images\bad.png' width='20' title='close' onclick='hide();'></td></tr></table>"

    '        Else

    '            Label2.ForeColor = Drawing.Color.Red
    '            Label2.Text = "<table id='output' bgcolor='#ccffcc' width='400'><tr><td valign='middle' width='370'>" & ex.Message() & "</td><td valign='middle' width='30'><img src='images\bad.png' width='20' title='close' onclick='hide();'></td></tr></table>"

    '        End If
    '    End Try



    'End Sub

    Function getAccountName(ByVal accno As String) As String
        Dim sqlstr As String = IO.File.ReadAllText(Server.MapPath("./accnamesql.txt")).Replace("%nuban%", accno)
        'Dim bnk As New bank.banks
        Dim acct As IAccount = Nothing
        Dim t24 As New T24Bank()
        Dim ds As New Data.DataSet
        Dim accname As String = ""
        acct = t24.GetAccountInfoByAccountNumber(accno)
        'ds = bnk.getAccountByNUBAN(accno)
        'ds = bnk.ReportsPortalDDL(sqlstr)

        Try
            accname = acct.CustomerName
            'If ds.Tables(0).Rows.Count <> 0 Then
            '    For Each dr As Data.DataRow In ds.Tables(0).Rows
            '        accname = dr("cus_sho_name").ToString

            '    Next
            'Else
            '    accname = String.Empty

            'End If

        Catch ex As Exception
            Gadget.LogException(ex)
            accname = ex.Message

            ess.Errorlog(ex.ToString, Now)
        End Try

        Return accname







    End Function
    'Function getOldAcc(ByVal nuban As String) As String
    '    Dim ds As New Data.DataSet
    '    Dim bnk As New bank.bank
    '    Dim oldacc As String = ""



    '    ds = bnk.getNubanAccount(nuban)

    '    For Each dr As Data.DataRow In ds.Tables(0).Rows

    '        oldacc = dr("ACCOUNTNUMBER")
    '    Next

    '    Return oldacc

    'End Function



    Function getOldAcc(ByVal nuban As String) As String
        Dim ds As New Data.DataSet

        Dim oldacc As String = ""

        Dim acct As IAccount = Nothing
        Dim t24 As New T24Bank()
        acct = t24.GetAccountInfoByAccountNumber(nuban)
        oldacc = acct.AccountNumberRepresentations(Account.BANKS).Representation
        If Not String.IsNullOrEmpty(oldacc) Then
            Return oldacc
        Else
            oldacc = acct.AccountNumberRepresentations(Account.NUBAN).Representation
        End If
        'TODO: Check for the existing
        'oldacc = bnk.getAccountFromNUBAN(nuban)



        Return oldacc

    End Function



    Sub showprg()


    End Sub


    Protected Sub Button5_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles Button5.Click
        lblSubmissionText.Text = String.Empty

        'Perform token validation here
        Dim username As String = txtUsername103.Text
        Dim token As String = txtToken103.Text

        If ConfigurationManager.AppSettings("APP_MODE").ToString().ToLower() = "online" Then
            Dim tokenValUtil As New TokenValidatorUtil()
            Dim response As TokenResponseHolder = tokenValUtil.ValidateToken(username, token.Trim())
            If Not response.IsValid Then
                Dim responseMsg As String = response.ResponseMessage
                lblSubmissionText.Visible = True
                lblSubmissionText.Text = "<font color=red>" & responseMsg & "</font>"
                Exit Sub
            End If
        End If

        If String.IsNullOrEmpty(TextBox1.Text) Then
            Label2.Text = String.Empty
            Label2.Text = "Please enter the account number."
            Exit Sub
        End If
        rblApproveReject.ClearSelection()
        If hiddenShowPopup.Value.Trim().ToLower() = "true" Then
            DisplayPreview(Session("messagetype").ToString())
            mpe.Show()
            Exit Sub
        End If


        If String.IsNullOrEmpty(txtBenName103.Text) Then
            Label2.ForeColor = Drawing.Color.Red
            Label2.Text = "<table id='output' bgcolor='#ccffcc' width='500'><tr><td valign='middle' width='270'>" & "Missing Beneficiary Name." & "</td><td valign='middle' width='30'><img src='images\bad.png' width='20' title='close' onclick='hide();'></td></tr></table>"
            Exit Sub
        End If


        If Session("bal") Is Nothing Then
            Label2.ForeColor = Drawing.Color.Red
            Label2.Text = "<table id='output' bgcolor='#ccffcc' width='500'><tr><td valign='middle' width='270'>" & "Incomplete fields." & "</td><td valign='middle' width='30'><img src='images\bad.png' width='20' title='close' onclick='hide();'></td></tr></table>"
            Exit Sub


        End If

        If Session("cname") Is Nothing Then
            Label2.ForeColor = Drawing.Color.Red
            Label2.Text = "<table id='output' bgcolor='#ccffcc' width='500'><tr><td valign='middle' width='270'>" & "Incomplete Fields." & "</td><td valign='middle' width='30'><img src='images\bad.png' width='20' title='close' onclick='hide();'></td></tr></table>"
            Exit Sub

            Exit Sub
        End If

        If tid Is Nothing Then
            Server.Transfer("tropsmain.aspx")
        End If

        Dim es As New easyrtgs

        Dim charges As Decimal = 0D
        Dim vatRate As Decimal = 0.05D

        If CheckBox1.Checked Then
            If TextBox6.Text = String.Empty Then

                charges = 0
            Else
                charges = Val(TextBox6.Text)

            End If

        Else
            Dim t24 As New T24Bank()
            Dim acct As IAccount = t24.GetAccountInfoByAccountNumber(TextBox1.Text)
            Dim isstaff As Boolean = False
            If acct IsNot Nothing Then
                isstaff = acct.AccountType.Trim().ToLower().Contains("staff")
            End If

            If isstaff Then

                charges = Convert.ToDecimal(es.getStaffCharges())
            Else

                charges = Convert.ToDecimal(es.getCustomerCharges())

            End If
        End If


        'Check the balance with the transaction
        Dim transAmt As Decimal = Convert.ToDecimal(TextBox3.Text)
        Dim bal As Decimal = Convert.ToDecimal(lblBalance103.Text)
        Dim vat As Decimal = 0D
        If Not String.IsNullOrEmpty(TextBox6.Text) Then
            vat = vatRate * charges
        Else
            vat = vatRate * charges
        End If

        If bal < (vat + charges + transAmt) Then
            Label2.Text = "<table id='output' bgcolor='#ccffcc' width='500'><tr><td valign='middle' width='270'>" & String.Format("Account balance: {0} is not sufficient for transaction charges {1}.", bal.ToString("0,000.00"), (vat + charges).ToString("0,000.00")) & "</td><td valign='middle' width='30'><img src='images\bad.png' width='20' title='close' onclick='hide();'></td></tr></table>"
            Exit Sub
        End If



        If Val(TextBox3.Text) < 0 Then
            Label2.ForeColor = Drawing.Color.Red
            Label2.Text = "<table id='output' bgcolor='#ccffcc' width='500'><tr><td valign='middle' width='270'>" & "Amount cannot be a Negative value." & "</td><td valign='middle' width='30'><img src='images\bad.png' width='20' title='close' onclick='hide();'></td></tr></table>"
            Exit Sub

        End If


        If Val(Session("bal").ToString().Replace(",", "")) < Val(Val(TextBox3.Text) + charges + (charges * vatRate)) Then


            Label2.ForeColor = Drawing.Color.Red
            Label2.Text = "<table id='output' bgcolor='#ccffcc' width='500'><tr><td valign='middle' width='470'>" & "Customer's Balance cannot handle this Transaction. N " & FormatNumber(Val(TextBox3.Text) + charges, 2) & " is Required.</td><td valign='middle' width='30'><img src='images\bad.png' width='20' title='close' onclick='hide();'></td></tr></table>"
            Exit Sub


        End If


        Try

            Button5.Enabled = False


        Catch ex1 As Exception
            Gadget.LogException(ex1)
            Label2.ForeColor = Drawing.Color.Red
            Label2.Text = "<table id='output' bgcolor='#ccffcc' width='300'><tr><td valign='middle' width='270'>" & "Error: Cannot post this transaction." & "</td><td valign='middle' width='30'><img src='images\bad.png' width='20' title='close' onclick='hide();'></td></tr></table>"
            Button5.Text = "Post Transaction"
            Button5.Enabled = True

            Exit Sub

        End Try



        Dim conn As New Data.SqlClient.SqlConnection(es.sqlconn1())
        Dim statusName As String = "Authorized" 'Because we are going to Treasury straight away (see mytransactions.aspx for the Status code that shows pending transactions for Treasury)
        Dim newstatusName As String = GetStatusNameForTransAmount(Convert.ToDecimal(TextBox3.Text))
        'In case it returns CustomerCare, then let us still set the status to Authorized since this is being initiated by the TROPS
        newstatusName = statusName
        Dim msgtype As String = String.Empty
        Dim msgvariant As String = String.Empty

        msgtype = Session("messagetype").ToString()
        If Session("variant") IsNot Nothing Then
            msgvariant = Session("variant").ToString()
        Else
            msgvariant = String.Empty ' Session("variant").ToString()
        End If


        Dim esy As New easyrtgs
        Dim msgTypeID As Integer = 0
        Dim msgVarID As Integer = 0

        Dim mgdetOb As New MtMessageDetails
        msgTypeID = mgdetOb.GetMessageTypeIDByName(msgtype)
        msgVarID = mgdetOb.GetMessageVariantIDByName(msgvariant)





        Using conn
            conn.Open()
            Dim trans As Data.SqlClient.SqlTransaction = conn.BeginTransaction()
            Using trans

                Dim cmd As New Data.SqlClient.SqlCommand
                cmd.Connection = conn

                'cmd.CommandText = "insert into transactions(transactionid,customer_name,customer_account,amount,charges,remarks,status,uploaded_by,uploaded_date,branch,Instruction,Beneficiary,Beneficiary_bank,Beneficiary_account,date) values" & _
                '    "(@tid,@cusname,@cusacc,@amount,@charges,@remarks,'Uploaded',@uploadby,'" & Now.ToString("yyyy-MM-dd hh:mm tt") & "',@branch,@Instruction,@benef,@benefBank,@benefAcc,@date)"

                cmd.CommandText = "insert into transactions(messagetypeID, messageVariantID, transactionid,customer_name,customer_account,amount,charges,remarks,status,uploaded_by,uploaded_date,branch,Instruction,Beneficiary,Beneficiary_bank,Beneficiary_account,date, requiresCustCareApproval) values" &
                   "(@msgtype, @msgvariant, @tid,@cusname,@cusacc,@amount,@charges,@remarks,@uploaded,@uploadby,'" & Now.ToString("yyyy-MM-dd hh:mm tt") & "',@branch,@Instruction,@benef,@benefBank,@benefAcc,@date, @requiresCustCareApproval)"



                Try
                    cmd.Transaction = trans
                    cmd.Parameters.AddWithValue("@tid", tid)
                    cmd.Parameters.AddWithValue("@cusname", Server.HtmlEncode(Session("cname").Trim.Replace("'", "`")))
                    If String.IsNullOrEmpty(Session("acc")) Then
                        Session("acc") = TextBox1.Text
                    End If
                    cmd.Parameters.AddWithValue("@cusacc", Session("acc"))
                    cmd.Parameters.AddWithValue("@amount", Server.HtmlEncode(FormatNumber(TextBox3.Text.Trim, 2)))
                    cmd.Parameters.AddWithValue("@charges", Server.HtmlEncode(FormatNumber(charges, 2)))
                    cmd.Parameters.AddWithValue("@remarks", Server.HtmlEncode(tid & TextBox4.Text.Trim.Replace("'", "`").Replace("&", "-")))
                    cmd.Parameters.AddWithValue("@uploadby", Session("name"))
                    cmd.Parameters.AddWithValue("@branch", Session("branch"))
                    cmd.Parameters.AddWithValue("@Instruction", instruction)
                    cmd.Parameters.AddWithValue("@benef", Server.HtmlEncode(txtBenName103.Text))
                    cmd.Parameters.AddWithValue("@benefBank", Server.HtmlEncode(DropDownList1.SelectedItem.Text & ":" & DropDownList1.SelectedItem.Value))
                    cmd.Parameters.AddWithValue("@benefAcc", Server.HtmlEncode(txtBenAccount103.Text))
                    cmd.Parameters.AddWithValue("@date", Now.ToString("yyyy-MM-dd"))
                    cmd.Parameters.AddWithValue("@uploaded", newstatusName)
                    cmd.Parameters.AddWithValue("@msgtype", msgTypeID)
                    cmd.Parameters.AddWithValue("@msgvariant", msgVarID)

                    If newstatusName.Trim().ToLower().CompareTo("customercare") = 0 Then
                        cmd.Parameters.AddWithValue("@requiresCustCareApproval", 1)
                    Else
                        cmd.Parameters.AddWithValue("@requiresCustCareApproval", 0)
                    End If
                    Dim crcustacct As String = String.Empty '
                    Dim postingStatus As String = "Authorize"
                    'Dim postingStatus As String = "Pending"
                    If cmd.ExecuteNonQuery() > 0 Then
                        Dim isSavedPrincipal As Boolean = False
                        Dim isSavedCharges As Boolean = False
                        Dim isSavedVat As Boolean = False
                        Dim isNextStateUpdated As Boolean = False

                        Try
                            If newstatusName.Trim().ToLower().CompareTo("customercare") = 0 Then
                                Dim sqlUpdate As String = "update transactions set postConfirmationStatus='Authorized' where TransactionID=@tid"
                                Dim cmdUpdate As New Data.SqlClient.SqlCommand(sqlUpdate, conn)
                                cmdUpdate.Transaction = trans
                                cmdUpdate.Parameters.AddWithValue("@tid", tid)
                                If cmdUpdate.ExecuteNonQuery() > 0 Then
                                    isNextStateUpdated = True
                                Else
                                    isNextStateUpdated = False
                                End If
                            Else
                                isNextStateUpdated = True 'cause i need it in the Commit() check
                            End If
                        Catch ex As Exception
                            Gadget.LogException(ex)
                        End Try

                        crcustacct = esy.getRTGSSuspense(esy.checkisIMAL(tid))
                        'crcustacct = esy.getRTGSAccount()
                        Dim sql As String = "spSavePostingEntries"
                        Dim cmdInsertTransactTemp As New Data.SqlClient.SqlCommand(sql, conn)
                        cmdInsertTransactTemp.Transaction = trans
                        cmdInsertTransactTemp.CommandType = Data.CommandType.StoredProcedure
                        cmdInsertTransactTemp.Parameters.AddWithValue("@transid", tid)
                        cmdInsertTransactTemp.Parameters.AddWithValue("@CustomerName", Server.HtmlEncode(Session("cname").Trim.Replace("'", "`")))
                        cmdInsertTransactTemp.Parameters.AddWithValue("@Amount", Convert.ToDecimal(Server.HtmlEncode(FormatNumber(TextBox3.Text.Trim, 2))))
                        cmdInsertTransactTemp.Parameters.AddWithValue("@Officer", Session("name"))
                        cmdInsertTransactTemp.Parameters.AddWithValue("@Supervisor", String.Empty)
                        cmdInsertTransactTemp.Parameters.AddWithValue("@CrCommAcct", crcustacct)
                        cmdInsertTransactTemp.Parameters.AddWithValue("@DrCustAcct", Session("acc").ToString())
                        cmdInsertTransactTemp.Parameters.AddWithValue("@expl_code", esy.getExplCode())
                        cmdInsertTransactTemp.Parameters.AddWithValue("@Remarks", esy.SanitizeRemarks(Convert.ToString(Server.HtmlEncode(tid & "-" & TextBox4.Text.Trim.Replace("'", "`").Replace("&", "-")))))
                        cmdInsertTransactTemp.Parameters.AddWithValue("@Source", "EasyRtgs")
                        cmdInsertTransactTemp.Parameters.AddWithValue("@Branch", Session("branch"))
                        cmdInsertTransactTemp.Parameters.AddWithValue("@Status", postingStatus)

                        If cmdInsertTransactTemp.ExecuteNonQuery() > 0 Then
                            isSavedPrincipal = True


                        End If

                        cmdInsertTransactTemp.Parameters.Clear()

                        'Save the charges
                        crcustacct = esy.getRTGSSuspense(esy.checkisIMAL(tid))
                        cmdInsertTransactTemp.CommandType = Data.CommandType.StoredProcedure
                        cmdInsertTransactTemp.Parameters.AddWithValue("@transid", tid)
                        cmdInsertTransactTemp.Parameters.AddWithValue("@CustomerName", Server.HtmlEncode(Session("cname").Trim.Replace("'", "`")))
                        cmdInsertTransactTemp.Parameters.AddWithValue("@Amount", Convert.ToDecimal(Server.HtmlEncode(FormatNumber(charges, 2))))
                        cmdInsertTransactTemp.Parameters.AddWithValue("@Officer", Session("name"))
                        cmdInsertTransactTemp.Parameters.AddWithValue("@Supervisor", String.Empty)
                        cmdInsertTransactTemp.Parameters.AddWithValue("@CrCommAcct", crcustacct)
                        cmdInsertTransactTemp.Parameters.AddWithValue("@DrCustAcct", Session("acc").ToString())
                        cmdInsertTransactTemp.Parameters.AddWithValue("@expl_code", esy.getExplCode())
                        cmdInsertTransactTemp.Parameters.AddWithValue("@Remarks", esy.SanitizeRemarks(Convert.ToString(Server.HtmlEncode(tid & "-" & TextBox4.Text.Trim.Replace("'", "`").Replace("&", "-")))))
                        cmdInsertTransactTemp.Parameters.AddWithValue("@Source", "EasyRtgs")
                        cmdInsertTransactTemp.Parameters.AddWithValue("@Branch", Session("branch"))
                        cmdInsertTransactTemp.Parameters.AddWithValue("@Status", postingStatus)

                        If cmdInsertTransactTemp.ExecuteNonQuery() > 0 Then
                            isSavedCharges = True
                        End If


                        cmdInsertTransactTemp.Parameters.Clear()

                        'Save the charges
                        crcustacct = esy.getRTGSSuspense(esy.checkisIMAL(tid))
                        cmdInsertTransactTemp.CommandType = Data.CommandType.StoredProcedure
                        cmdInsertTransactTemp.Parameters.AddWithValue("@transid", tid)
                        cmdInsertTransactTemp.Parameters.AddWithValue("@CustomerName", Server.HtmlEncode(Session("cname").Trim.Replace("'", "`")))
                        cmdInsertTransactTemp.Parameters.AddWithValue("@Amount", Convert.ToDecimal(Server.HtmlEncode(FormatNumber(charges, 2))) * vatRate) 'calculate the vat on the charge
                        cmdInsertTransactTemp.Parameters.AddWithValue("@Officer", Session("name"))
                        cmdInsertTransactTemp.Parameters.AddWithValue("@Supervisor", String.Empty)
                        cmdInsertTransactTemp.Parameters.AddWithValue("@CrCommAcct", crcustacct)
                        cmdInsertTransactTemp.Parameters.AddWithValue("@DrCustAcct", Session("acc").ToString())
                        cmdInsertTransactTemp.Parameters.AddWithValue("@expl_code", esy.getExplCode())
                        cmdInsertTransactTemp.Parameters.AddWithValue("@Remarks", esy.SanitizeRemarks(Convert.ToString(Server.HtmlEncode(tid & "-" & TextBox4.Text.Trim.Replace("'", "`").Replace("&", "-")))))
                        cmdInsertTransactTemp.Parameters.AddWithValue("@Source", "EasyRtgs")
                        cmdInsertTransactTemp.Parameters.AddWithValue("@Branch", Session("branch"))
                        cmdInsertTransactTemp.Parameters.AddWithValue("@Status", postingStatus)

                        If cmdInsertTransactTemp.ExecuteNonQuery() > 0 Then
                            isSavedVat = True
                        End If


                        If isSavedPrincipal And isSavedVat And isSavedCharges And isNextStateUpdated Then
                            Try
                                trans.Commit()
                                conn.Close()
                                conn.Dispose()


                                esy.createAudit(tid & " was posted by " & Session("name"), Now)
                                esy.TransactionAudit(tid & " was posted by " & Session("name"), Now, tid)


                                'notify authorizer
                                Dim body As String = ""
                                body = "Sir/Madam" & "<br>" & "<br>" & "This mail is a request for you to authorize a Third Party Transfer Transaction . Details of this transaction are as follows;" & "<br>" &
                                "Customer: " & txtCustName103.Text & "<br>" & "Amount: " & FormatNumber(TextBox3.Text, 2) & "<br>" & " Account Number: " & TextBox1.Text.Trim() & "<br><br>" & "Click on the link below to authorize this transaction. <br><br><a href='http://" & System.Configuration.ConfigurationManager.AppSettings("serverip") & "/" & System.Configuration.ConfigurationManager.AppSettings("AppVirDir") & "/default.aspx?action=authorize&tid=" & tid & "'>Authorize this Transaction</a> <br><br>Thank You"

                                Try
                                    Dim mail As New easyrtgs
                                    If statusName.Trim().ToLower().CompareTo("customercare") = 0 Then 'if it is the customercare
                                        mail.sendmail(System.Configuration.ConfigurationManager.AppSettings("mailHost"), mail.getAuthorizerEmailbyName("Customer Care"), Session("email"), "3RD PARTY TRANSACTION CUSTOMER CARE CONFIRMATION REQUEST (TRANSACTION ID: " & tid & ")", body)
                                    Else
                                        Dim toaddress As String = mail.getAuthorizerEmail("")
                                        mail.sendmail(System.Configuration.ConfigurationManager.AppSettings("mailHost"), mail.getAuthorizerEmail(Session("branch")), Session("email"), "3RD PARTY TRANSACTION AUTHORIZATION REQUEST (TRANSACTION ID: " & tid & ")", body)
                                    End If


                                Catch ex As Exception
                                    Gadget.LogException(ex)
                                End Try


                                Label2.ForeColor = Drawing.Color.Green
                                Label2.Text = "<table id='output' bgcolor='#ccffcc' width='300'><tr><td valign='middle' width='270'>" & "Transaction was posted.(" & tid & ")" & "</td><td valign='middle' width='30'><img src='images\bad.png' width='20' title='close' onclick='hide();'></td></tr></table>"
                                Button5.Text = "Post Transaction"
                                Button5.Enabled = True
                                TextBox1.Text = ""
                                txtCustName103.Text = ""
                                TextBox3.Text = ""
                                TextBox4.Text = ""
                                lblBalance103.Text = ""

                                Server.Transfer("postsuccess.aspx", False)
                            Catch ex As Exception
                                Gadget.LogException(ex)
                                trans.Rollback()
                                Label2.Text = "<table id='output' bgcolor='#ccffcc' width='300'><tr><td valign='middle' width='270'>" & "Transaction was posted.(" & tid & ")" & "</td><td valign='middle' width='30'><img src='images\bad.png' width='20' title='close' onclick='hide();'></td></tr></table>"
                            End Try


                        Else
                            trans.Rollback()
                        End If
                    Else
                        trans.Rollback()
                    End If

                Catch ex As Exception
                    Gadget.LogException(ex)
                    trans.Rollback()

                    If ex.Message.Contains("duplicate") Then
                        Label2.ForeColor = Drawing.Color.Red
                        Label2.Text = "<table id='output' bgcolor='#ccffcc' width='300'><tr><td valign='middle' width='270'>" & "Duplicate Transaction ID. Already exist." & "</td><td valign='middle' width='30'><img src='images\bad.png' width='20' title='close' onclick='hide();'></td></tr></table>"
                        Button5.Text = "Post Transaction"
                        Button5.Enabled = True

                        Exit Sub
                    End If

                    If ex.Message.Contains("Input String") Then

                        Label2.ForeColor = Drawing.Color.Red
                        Label2.Text = "<table id='output' bgcolor='#ccffcc' width='300'><tr><td valign='middle' width='270'>" & "Error: Cannot Post. Check all fields" & "</td><td valign='middle' width='30'><img src='images\bad.png' width='20' title='close' onclick='hide();'></td></tr></table>"
                        Button5.Text = "Post Transaction"
                        Button5.Enabled = True

                        Exit Sub

                    End If

                    Label2.ForeColor = Drawing.Color.Red
                    Label2.Text = "<table id='output' bgcolor='#ccffcc' width='300'><tr><td valign='middle' width='270'>" & ex.Message & "</td><td valign='middle' width='30'><img src='images\bad.png' width='20' title='close' onclick='hide();'></td></tr></table>"
                    Button5.Text = "Post Transaction"
                    Button5.Enabled = True


                End Try

            End Using

        End Using


    End Sub

    Protected Sub CheckBox1_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles CheckBox1.CheckedChanged
        'If CheckBox1.Checked Then
        '    TextBox6.Visible = True
        'Else
        '    TextBox6.Visible = False
        'End If
        TextBox6.Enabled = CheckBox1.Checked
        TextBox6.ReadOnly = Not CheckBox1.Checked
    End Sub

    Protected Sub LinkButton1_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles LinkButton1.Click
        Button6.Visible = True
        txtBenName103.Visible = False
        LinkButton1.Visible = False

    End Sub

    Protected Sub TextBox7_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtBenAccount103.TextChanged
        'TODO: Name Enquiry disabled
        Label6.Text = ""
        Dim acctLen As Integer = txtBenAccount103.Text.Length
        Dim bankCode As String = DropDownList1.SelectedValue.ToString()
        Dim bdet As BankDetail = BankDetail.GetBankDetailByCode(bankCode)
        'bdet.IsNameEnquirySupported 
        'If acctLen = 10 Then
        '    benefName(sender, e)
        'Else
        '    Label6.Text = "Please check the beneficiary account number because it is not 10 digits long."
        'End If
        If bdet.IsNameEnquirySupported Then
            If acctLen = 10 Then
                benefName(sender, e)
            Else
                Label6.Text = "Please check the beneficiary account number because it is not 10 digits long."
            End If

        Else

            txtBenName103.Visible = True  'Make this visible so that it can be inputted directly
            txtBenName103.ReadOnly = False

        End If
    End Sub

    ''' <summary>
    ''' Check for Empty string.
    ''' </summary>
    ''' <param name="amount">The amount that was in the request.</param>
    ''' <returns>The appropriate status name for the transaction.</returns>
    ''' <remarks>if there are any empty string, that means that the request does not have an opinion as regards the status name.</remarks>
    Private Function GetStatusNameForTransAmount(ByVal amount As Decimal) As String
        Dim op As String = ConfigurationManager.AppSettings("CUST_CARE_AMT_OP").ToString()
        Dim thresholdAmt As Decimal = Convert.ToDecimal(ConfigurationManager.AppSettings("CUST_CARE_AMT_LIM").ToString())

        Dim statusName As String = String.Empty
        Select Case op.Trim().ToLower()
            Case ">"
                If amount > thresholdAmt Then
                    statusName = "CustomerCare"
                End If
            Case ">="
                If amount >= thresholdAmt Then
                    statusName = "CustomerCare"
                End If
            Case "<"
                If amount < thresholdAmt Then
                    statusName = "CustomerCare"
                End If
            Case "<="
                If amount <= thresholdAmt Then
                    statusName = "CustomerCare"
                End If
            Case Else
                If amount > thresholdAmt Then
                    statusName = "CustomerCare"
                End If

        End Select

        Return statusName
    End Function

    Private Sub ModifyUserInterfaceByRoleTransferTypeAndVariant(ByVal role As String, ByVal transferType As String, ByVal messageType As String, ByVal messageVariant As String)
        'TODO: Implement.
        Dim mrole As String = role.Trim().ToLower()
        Dim mtranstype As String = transferType.Trim().ToLower()
        Dim mvariant As String = messageVariant.Trim().ToLower()

        If Not messageType.ToLower().StartsWith("mt") Then
            messageType = "mt" & messageType
        End If

        Select Case messageType.Trim().ToLower()
            Case easyrtgs.MESSAGETYPE_MT103.Trim().ToLower()
                pnl103.Visible = True
                pnl202.Visible = False
            Case easyrtgs.MESSAGETYPE_MT202.Trim().ToLower()
                pnl103.Visible = False
                pnl202.Visible = True
            Case Else
                Return
        End Select

        Select Case mtranstype
            Case easyrtgs.TRANSTYPE_CBN.Trim().ToLower()
                DisableNubanCheck(True) 'valRegExpNuban.Enabled = False '
                DisableNameEntry(True) ' TextBox2.ReadOnly = False ' 
                DisableLimitCheck(True) 'hiddenUseAmtLimit.Value = "false" '
                DisableNameQuery(True)
            Case easyrtgs.MESSAGETYPE_MT103.Trim().ToLower()
                valRegExpNuban.Enabled = True
                txtCustName103.ReadOnly = True
                hiddenUseAmtLimit.Value = "true"
            Case easyrtgs.MESSAGETYPE_MT202.Trim().ToLower()
                valRegExpNuban.Enabled = True
                txtCustName103.ReadOnly = True
                hiddenUseAmtLimit.Value = "true"
            Case easyrtgs.TRANSTYPE_INTERBANK
                valRegExpNuban.Enabled = True
                txtCustName103.ReadOnly = True
                hiddenUseAmtLimit.Value = "true"
        End Select

        'Get the message type variant for use in a way that works well.
        SetupForMessageVariant(mvariant, mtranstype)

    End Sub

    Private Sub DisableNubanCheck(ByVal isnubanCheckDisabled As Boolean)
        valRegExpNuban.Enabled = isnubanCheckDisabled
    End Sub

    Private Sub DisableNameEntry(ByVal isNameEntryDisabled As Boolean)
        'btnGetCustData.Visible = Not isNameEntryDisabled
        txtCustName103.ReadOnly = isNameEntryDisabled
    End Sub

    Private Sub DisableLimitCheck(ByVal isLimitCheckDisabled As Boolean)
        If isLimitCheckDisabled Then

        End If
        hiddenUseAmtLimit.Value = "false"
    End Sub

    Protected Sub ddlOwnerBranch_DataBound(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlOwnerBranch.DataBound
        Dim d As New ListItem("--Please Select--", "0")
        ddlOwnerBranch.Items.Insert(0, d)
        Dim transtype As String = Session("transtype").ToString()
        Dim ddl As DropDownList = CType(sender, DropDownList)
        CustomizeDropDownForBanksByTransferType(ddl, transtype)
    End Sub

    Protected Sub ddlOwnerBranch0_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlOwnerBranch202.SelectedIndexChanged
        Dim branchAccountNumberNuban As String = String.Empty
        Dim g As New Gadget
        Dim branchcode As String = ddlOwnerBranch202.SelectedValue.ToString()
        If (branchcode.Trim() <> "0") Then
            Dim eob As New easyrtgs
            Dim bdets As BranchDetail = g.getMt202BranchDetails(branchcode)
            'branchAccountNumberNuban = eob.getBranchMT202AccountNumberByCode(branchcode)
            branchAccountNumberNuban = eob.getBranchMT202AccountNumberByCode()


            Session("acc") = branchAccountNumberNuban 'bdets.BranchCode & "-" & bdets.Cusnum & "-" & bdets.CurrencyCode & "-" & bdets.LedgerCode & "-" & bdets.SubAccountCode
            Session("ledcode") = bdets.LedgerCode

            'Get the customer name
            Dim name As String = String.Empty
            Dim t24 As New T24Bank()
            Dim acct As IAccount = Nothing
            'Dim bs As New bank.banks

            Dim balance As Decimal = 0D
            acct = t24.GetAccountInfoByAccountNumber(branchAccountNumberNuban)
            'Dim ds As DataSet = bs.getAccountByNUBANAll(branchAccountNumberNuban)
            If acct IsNot Nothing Then
                name = acct.CustomerName
                Dim balObj As IBalance = acct.Balances(Account.WORKING_BALANCE)

                balance = balObj.Amount
            End If


            lblBalance202.Text = balance.ToString("0,000.00")
            Session("bal") = balance
            'TODO: Remove this
            If String.IsNullOrEmpty(name) Then
                name = String.Format("Branch {0} Account", branchcode)
            End If
            If String.IsNullOrEmpty(txtCustName202.Text) Then
                txtCustName202.Text = name
            End If
        Else
            Return
        End If
    End Sub

    Protected Sub Button9_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        If String.IsNullOrEmpty(txtBeneName202.Text) Then
            Label2.ForeColor = Drawing.Color.Red
            Label2.Text = "<table id='output' bgcolor='#ccffcc' width='500'><tr><td valign='middle' width='270'>" & "Missing Beneficiary Name." & "</td><td valign='middle' width='30'><img src='images\bad.png' width='20' title='close' onclick='hide();'></td></tr></table>"
            Exit Sub
        End If

        If Convert.ToDecimal(txtAmount202.Text) < 10000000 Then
            CompareValidator1.IsValid = False
            Exit Sub
        End If
        If Session("bal") Is Nothing Then
            Label2.ForeColor = Drawing.Color.Red
            Label2.Text = "<table id='output' bgcolor='#ccffcc' width='500'><tr><td valign='middle' width='270'>" & "Incomplete fields." & "</td><td valign='middle' width='30'><img src='images\bad.png' width='20' title='close' onclick='hide();'></td></tr></table>"
            Exit Sub


        End If

        If Session("cname") Is Nothing Then
            Label2.ForeColor = Drawing.Color.Red
            Label2.Text = "<table id='output' bgcolor='#ccffcc' width='500'><tr><td valign='middle' width='270'>" & "Incomplete Fields." & "</td><td valign='middle' width='30'><img src='images\bad.png' width='20' title='close' onclick='hide();'></td></tr></table>"
            Exit Sub

            Exit Sub
        End If

        If tid Is Nothing Then
            Server.Transfer("tropsmain.aspx")


        End If
        Dim es As New easyrtgs

        Dim charges As Decimal = 0D
        Dim vatRate As Decimal = 0.05D

        If CheckBox1.Checked Then
            If TextBox6.Text = String.Empty Then

                charges = 0
            Else
                charges = Val(TextBox6.Text)

            End If

        Else

            Dim t24 As New T24Bank()
            Dim acct As IAccount = t24.GetAccountInfoByAccountNumber(txtCustAcct.Text)
            Dim isstaff As Boolean = False
            If acct IsNot Nothing Then
                isstaff = acct.AccountType.Trim().ToLower().Contains("staff")
            End If
            'If Session("ledcode") = "9" Then

            '    charges = Convert.ToDecimal(es.getStaffCharges())
            'Else

            '    charges = Convert.ToDecimal(es.getCustomerCharges())

            'End If
            If isstaff Then

                charges = Convert.ToDecimal(es.getStaffCharges())
            Else

                charges = Convert.ToDecimal(es.getCustomerCharges())

            End If
        End If




        If Val(TextBox3.Text) < 0 Then
            Label2.ForeColor = Drawing.Color.Red
            Label2.Text = "<table id='output' bgcolor='#ccffcc' width='500'><tr><td valign='middle' width='270'>" & "Amount cannot be a Negative value." & "</td><td valign='middle' width='30'><img src='images\bad.png' width='20' title='close' onclick='hide();'></td></tr></table>"
            Exit Sub

        End If


        If Val(Session("bal").Replace(",", "")) < Val(Val(TextBox3.Text) + charges + (charges * vatRate)) Then


            Label2.ForeColor = Drawing.Color.Red
            Label2.Text = "<table id='output' bgcolor='#ccffcc' width='500'><tr><td valign='middle' width='470'>" & "Customer's Balance cannot handle this Transaction. N " & FormatNumber(Val(TextBox3.Text) + charges, 2) & " is Required.</td><td valign='middle' width='30'><img src='images\bad.png' width='20' title='close' onclick='hide();'></td></tr></table>"
            Exit Sub


        End If


        Try

            Button5.Enabled = False

            If Not FileUpload1.FileContent.Length > 1048576 Then
                If FileUpload1.FileName.Contains(".jpg") Or FileUpload1.FileName.Contains(".JPG") Then
                    FileUpload1.SaveAs(Server.MapPath("./Instructions/" & tid & ".jpg"))
                    instruction = "JPG"
                ElseIf FileUpload1.FileName.Contains(".png") Or FileUpload1.FileName.Contains(".PNG") Then
                    FileUpload1.SaveAs(Server.MapPath("./Instructions/" & tid & ".png"))
                    instruction = "PNG"

                ElseIf FileUpload1.FileName.Contains(".pdf") Or FileUpload1.FileName.Contains(".PDF") Then
                    FileUpload1.SaveAs(Server.MapPath("./Instructions/" & tid & ".pdf"))
                    instruction = "PDF"

                Else
                    Label2.ForeColor = Drawing.Color.Red
                    Label2.Text = "<table id='output' bgcolor='#ccffcc' width='400'><tr><td valign='middle' width='370'>" & "Scanned Instruction must be .JPG/.PNG/.PDF" & "</td><td valign='middle' width='30'><img src='images\bad.png' width='20' title='close' onclick='hide();'></td></tr></table>"
                    Button5.Text = "Post Transaction"
                    Button5.Enabled = True

                    Exit Sub
                End If
            Else
                Label2.ForeColor = Drawing.Color.Red
                Label2.Text = "<table id='output' bgcolor='#ccffcc' width='300'><tr><td valign='middle' width='270'>" & "Scanned Instruction must be less than 1MB" & "</td><td valign='middle' width='30'><img src='images\bad.png' width='20' title='close' onclick='hide();'></td></tr></table>"
                Button5.Text = "Post Transaction"
                Button5.Enabled = True

                Exit Sub
            End If
        Catch ex1 As Exception
            Gadget.LogException(ex1)
            Label2.ForeColor = Drawing.Color.Red
            Label2.Text = "<table id='output' bgcolor='#ccffcc' width='300'><tr><td valign='middle' width='270'>" & "Error: Cannot post this transaction." & "</td><td valign='middle' width='30'><img src='images\bad.png' width='20' title='close' onclick='hide();'></td></tr></table>"
            Button5.Text = "Post Transaction"
            Button5.Enabled = True

            Exit Sub

        End Try



        Dim conn As New Data.SqlClient.SqlConnection(es.sqlconn1())
        Dim statusName As String = "Authorized" 'Because we are going to Treasury straight away (see mytransactions.aspx for the Status code that shows pending transactions for Treasury)
        Dim newstatusName As String = GetStatusNameForTransAmount(Convert.ToDecimal(TextBox3.Text))
        Dim msgtype As String = String.Empty
        Dim msgvariant As String = String.Empty

        msgtype = Session("messagetype").ToString()
        msgvariant = Session("variant").ToString()


        ''No need to check for Customer Care here
        'If Not String.IsNullOrEmpty(newstatusName) And Session("role") <> "TROPS" Then
        '    statusName = newstatusName
        'End If

        'audit trail
        Dim esy As New easyrtgs
        Using conn
            conn.Open()
            Dim trans As Data.SqlClient.SqlTransaction = conn.BeginTransaction()
            Using trans

                Dim cmd As New Data.SqlClient.SqlCommand
                cmd.Connection = conn

                'cmd.CommandText = "insert into transactions(transactionid,customer_name,customer_account,amount,charges,remarks,status,uploaded_by,uploaded_date,branch,Instruction,Beneficiary,Beneficiary_bank,Beneficiary_account,date) values" & _
                '    "(@tid,@cusname,@cusacc,@amount,@charges,@remarks,'Uploaded',@uploadby,'" & Now.ToString("yyyy-MM-dd hh:mm tt") & "',@branch,@Instruction,@benef,@benefBank,@benefAcc,@date)"

                cmd.CommandText = "insert into transactions(messagetype, messageVariant, transactionid,customer_name,customer_account,amount,charges,remarks,status,uploaded_by,uploaded_date,branch,Instruction,Beneficiary,Beneficiary_bank,Beneficiary_account,date, requiresCustCareApproval) values" &
                   "(@msgtype, @msgvariant, @tid,@cusname,@cusacc,@amount,@charges,@remarks,@uploaded,@uploadby,'" & Now.ToString("yyyy-MM-dd hh:mm tt") & "',@branch,@Instruction,@benef,@benefBank,@benefAcc,@date, @requiresCustCareApproval)"



                Try
                    cmd.Transaction = trans
                    cmd.Parameters.AddWithValue("@tid", tid)
                    cmd.Parameters.AddWithValue("@cusname", Server.HtmlEncode(Session("cname").Trim.Replace("'", "`")))
                    cmd.Parameters.AddWithValue("@cusacc", Session("acc"))
                    cmd.Parameters.AddWithValue("@amount", Server.HtmlEncode(FormatNumber(TextBox3.Text.Trim, 2)))
                    cmd.Parameters.AddWithValue("@charges", Server.HtmlEncode(FormatNumber(charges, 2)))
                    cmd.Parameters.AddWithValue("@remarks", Server.HtmlEncode(tid & TextBox4.Text.Trim.Replace("'", "`").Replace("&", "-")))
                    cmd.Parameters.AddWithValue("@uploadby", Session("name"))
                    cmd.Parameters.AddWithValue("@branch", Session("branch"))
                    cmd.Parameters.AddWithValue("@Instruction", instruction)
                    cmd.Parameters.AddWithValue("@benef", Server.HtmlEncode(txtBenName103.Text))
                    cmd.Parameters.AddWithValue("@benefBank", Server.HtmlEncode(DropDownList1.SelectedItem.Text & ":" & DropDownList1.SelectedItem.Value))
                    cmd.Parameters.AddWithValue("@benefAcc", Server.HtmlEncode(txtBenAccount103.Text))
                    cmd.Parameters.AddWithValue("@date", Now.ToString("yyyy-MM-dd"))
                    cmd.Parameters.AddWithValue("@uploaded", statusName)
                    cmd.Parameters.AddWithValue("@msgtype", msgtype)
                    cmd.Parameters.AddWithValue("@msgvariant", msgvariant)

                    If statusName.Trim().ToLower().CompareTo("customercare") = 0 Then
                        cmd.Parameters.AddWithValue("@requiresCustCareApproval", 1)
                    Else
                        cmd.Parameters.AddWithValue("@requiresCustCareApproval", 0)
                    End If

                    If cmd.ExecuteNonQuery() > 0 Then

                        Dim isSavedPrincipal As Boolean = False
                        Dim isSavedCharges As Boolean = False
                        Dim isSavedVat As Boolean = False

                        Dim sql As String = "spSavePostingEntries"
                        Dim cmdInsertTransactTemp As New Data.SqlClient.SqlCommand(sql, conn)
                        cmdInsertTransactTemp.Transaction = trans
                        cmdInsertTransactTemp.CommandType = Data.CommandType.StoredProcedure
                        cmdInsertTransactTemp.Parameters.AddWithValue("@transid", tid)
                        cmdInsertTransactTemp.Parameters.AddWithValue("@CustomerName", Server.HtmlEncode(Session("cname").Trim.Replace("'", "`")))
                        cmdInsertTransactTemp.Parameters.AddWithValue("@Amount", Convert.ToDecimal(Server.HtmlEncode(FormatNumber(TextBox3.Text.Trim, 2))))
                        cmdInsertTransactTemp.Parameters.AddWithValue("@Officer", Session("name"))
                        cmdInsertTransactTemp.Parameters.AddWithValue("@Supervisor", Session("name"))
                        cmdInsertTransactTemp.Parameters.AddWithValue("@CrCommAcct", esy.getRTGSSuspense(esy.checkisIMAL(tid)))
                        cmdInsertTransactTemp.Parameters.AddWithValue("@DrCustAcct", Session("acc"))
                        cmdInsertTransactTemp.Parameters.AddWithValue("@expl_code", esy.getExplCode())
                        cmdInsertTransactTemp.Parameters.AddWithValue("@Remarks", esy.SanitizeRemarks(Convert.ToString(Server.HtmlEncode(tid & "-" & TextBox4.Text.Trim.Replace("'", "`").Replace("&", "-")))))
                        cmdInsertTransactTemp.Parameters.AddWithValue("@Source", "EasyRtgs")
                        cmdInsertTransactTemp.Parameters.AddWithValue("@Branch", Session("branch"))
                        cmdInsertTransactTemp.Parameters.AddWithValue("@Status", "Pending")

                        If cmdInsertTransactTemp.ExecuteNonQuery() > 0 Then
                            isSavedPrincipal = True
                        End If

                        'cmdInsertTransactTemp.Parameters.Clear()

                        ''Save the charges
                        'cmdInsertTransactTemp.CommandType = Data.CommandType.StoredProcedure
                        'cmdInsertTransactTemp.Parameters.AddWithValue("@transid", tid)
                        'cmdInsertTransactTemp.Parameters.AddWithValue("@CustomerName", Server.HtmlEncode(Session("cname").Trim.Replace("'", "`")))
                        'cmdInsertTransactTemp.Parameters.AddWithValue("@Amount", Convert.ToDecimal(Server.HtmlEncode(FormatNumber(charges, 2))))
                        'cmdInsertTransactTemp.Parameters.AddWithValue("@Officer", Session("name"))
                        'cmdInsertTransactTemp.Parameters.AddWithValue("@Supervisor", Session("name"))
                        'cmdInsertTransactTemp.Parameters.AddWithValue("@CrCommAcct", esy.getRTGSSuspense())
                        'cmdInsertTransactTemp.Parameters.AddWithValue("@DrCustAcct", Session("acc"))
                        'cmdInsertTransactTemp.Parameters.AddWithValue("@expl_code", esy.getExplCode())
                        'cmdInsertTransactTemp.Parameters.AddWithValue("@Remarks", esy.SanitizeRemarks(Convert.ToString(Server.HtmlEncode(tid & "-" & TextBox4.Text.Trim.Replace("'", "`").Replace("&", "-")))))
                        'cmdInsertTransactTemp.Parameters.AddWithValue("@Source", "EasyRtgs")
                        'cmdInsertTransactTemp.Parameters.AddWithValue("@Branch", Session("branch"))
                        'cmdInsertTransactTemp.Parameters.AddWithValue("@Status", "Pending")

                        'If cmdInsertTransactTemp.ExecuteNonQuery() > 0 Then
                        '    isSavedCharges = True
                        'End If


                        cmdInsertTransactTemp.Parameters.Clear()

                        ''Save the charges
                        'cmdInsertTransactTemp.CommandType = Data.CommandType.StoredProcedure
                        'cmdInsertTransactTemp.Parameters.AddWithValue("@transid", tid)
                        'cmdInsertTransactTemp.Parameters.AddWithValue("@CustomerName", Server.HtmlEncode(Session("cname").Trim.Replace("'", "`")))
                        'cmdInsertTransactTemp.Parameters.AddWithValue("@Amount", Convert.ToDecimal(Server.HtmlEncode(FormatNumber(charges, 2))) * vatRate) 'calculate the vat on the charge
                        'cmdInsertTransactTemp.Parameters.AddWithValue("@Officer", Session("name"))
                        'cmdInsertTransactTemp.Parameters.AddWithValue("@Supervisor", Session("name"))
                        'cmdInsertTransactTemp.Parameters.AddWithValue("@CrCommAcct", esy.getRTGSSuspense())
                        'cmdInsertTransactTemp.Parameters.AddWithValue("@DrCustAcct", Session("acc"))
                        'cmdInsertTransactTemp.Parameters.AddWithValue("@expl_code", esy.getExplCode())
                        'cmdInsertTransactTemp.Parameters.AddWithValue("@Remarks", esy.SanitizeRemarks(Convert.ToString(Server.HtmlEncode(tid & "-" & TextBox4.Text.Trim.Replace("'", "`").Replace("&", "-")))))
                        'cmdInsertTransactTemp.Parameters.AddWithValue("@Source", "EasyRtgs")
                        'cmdInsertTransactTemp.Parameters.AddWithValue("@Branch", Session("branch"))
                        'cmdInsertTransactTemp.Parameters.AddWithValue("@Status", "Pending")

                        'If cmdInsertTransactTemp.ExecuteNonQuery() > 0 Then
                        '    isSavedVat = True
                        'End If


                        If isSavedPrincipal Then
                            Try
                                trans.Commit()
                                conn.Close()
                                conn.Dispose()


                                esy.createAudit(tid & " was posted by " & Session("name"), Now)
                                esy.TransactionAudit(tid & " was posted by " & Session("name"), Now, tid)


                                'notify authorizer
                                Dim body As String = ""
                                body = "Sir/Madam" & "<br>" & "<br>" & "This mail is a request for you to authorize a Third Party Transfer Transaction . Details of this transaction are as follows;" & "<br>" &
                                "Customer: " & txtCustName103.Text & "<br>" & "Amount: " & FormatNumber(TextBox3.Text, 2) & "<br>" & " Account Number: " & TextBox1.Text.Trim() & "<br><br>" & "Click on the link below to authorize this transaction. <br><br><a href='http://" & System.Configuration.ConfigurationManager.AppSettings("serverip") & "/" & System.Configuration.ConfigurationManager.AppSettings("AppVirDir") & "/default.aspx?action=authorize&tid=" & tid & "'>Authorize this Transaction</a> <br><br>Thank You"

                                Try
                                    Dim mail As New easyrtgs
                                    If statusName.Trim().ToLower().CompareTo("customercare") = 0 Then 'if it is the customercare
                                        mail.sendmail(System.Configuration.ConfigurationManager.AppSettings("mailHost"), mail.getAuthorizerEmailbyName("Customer Care"), Session("email"), "3RD PARTY TRANSACTION CUSTOMER CARE CONFIRMATION REQUEST (TRANSACTION ID: " & tid & ")", body)
                                    Else
                                        Dim toaddress As String = mail.getAuthorizerEmail("")
                                        mail.sendmail(System.Configuration.ConfigurationManager.AppSettings("mailHost"), mail.getAuthorizerEmail(Session("branch")), Session("email"), "3RD PARTY TRANSACTION AUTHORIZATION REQUEST (TRANSACTION ID: " & tid & ")", body)
                                    End If


                                Catch ex As Exception
                                    Gadget.LogException(ex)
                                End Try


                                Label2.ForeColor = Drawing.Color.Green
                                Label2.Text = "<table id='output' bgcolor='#ccffcc' width='300'><tr><td valign='middle' width='270'>" & "Transaction was posted.(" & tid & ")" & "</td><td valign='middle' width='30'><img src='images\bad.png' width='20' title='close' onclick='hide();'></td></tr></table>"
                                Button5.Text = "Post Transaction"
                                Button5.Enabled = True
                                TextBox1.Text = ""
                                txtCustName103.Text = ""
                                TextBox3.Text = ""
                                TextBox4.Text = ""
                                lblBalance103.Text = ""

                                Server.Transfer("postsuccess.aspx", False)
                            Catch ex As Exception
                                Gadget.LogException(ex)
                                trans.Rollback()
                                Label2.Text = "<table id='output' bgcolor='#ccffcc' width='300'><tr><td valign='middle' width='270'>" & "Transaction was posted.(" & tid & ")" & "</td><td valign='middle' width='30'><img src='images\bad.png' width='20' title='close' onclick='hide();'></td></tr></table>"
                            End Try


                        Else
                            trans.Rollback()
                        End If





                    Else
                        trans.Rollback()
                    End If

                Catch ex As Exception
                    Gadget.LogException(ex)
                    trans.Rollback()

                    If ex.Message.Contains("duplicate") Then
                        Label2.ForeColor = Drawing.Color.Red
                        Label2.Text = "<table id='output' bgcolor='#ccffcc' width='300'><tr><td valign='middle' width='270'>" & "Duplicate Transaction ID. Already exist." & "</td><td valign='middle' width='30'><img src='images\bad.png' width='20' title='close' onclick='hide();'></td></tr></table>"
                        Button5.Text = "Post Transaction"
                        Button5.Enabled = True

                        Exit Sub
                    End If

                    If ex.Message.Contains("Input String") Then

                        Label2.ForeColor = Drawing.Color.Red
                        Label2.Text = "<table id='output' bgcolor='#ccffcc' width='300'><tr><td valign='middle' width='270'>" & "Error: Cannot Post. Check all fields" & "</td><td valign='middle' width='30'><img src='images\bad.png' width='20' title='close' onclick='hide();'></td></tr></table>"
                        Button5.Text = "Post Transaction"
                        Button5.Enabled = True

                        Exit Sub

                    End If

                    Label2.ForeColor = Drawing.Color.Red
                    Label2.Text = "<table id='output' bgcolor='#ccffcc' width='300'><tr><td valign='middle' width='270'>" & ex.Message & "</td><td valign='middle' width='30'><img src='images\bad.png' width='20' title='close' onclick='hide();'></td></tr></table>"
                    Button5.Text = "Post Transaction"
                    Button5.Enabled = True


                End Try

            End Using

        End Using


    End Sub

    Protected Sub ddlOwnerBranch202_DataBound(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlOwnerBranch202.DataBound
        'ddlOwnerBranch202.Items.Insert(0, New ListItem("--Choose Branch--", "0"))
        Dim d As New ListItem("--Please Select--", "0")
        ddlOwnerBranch202.Items.Insert(0, d)

    End Sub

    Protected Sub ddlBen202_DataBound(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlBen202.DataBound
        ddlBen202.Items.Insert(0, New ListItem("-Choose Bank-", "0"))
        Dim transtype As String = Session("transtype").ToString()
        Dim ddl As DropDownList = CType(sender, DropDownList)
        CustomizeDropDownForBanksByTransferType(ddl, transtype)
    End Sub
    Protected Sub DropDownList1_DataBound(ByVal sender As Object, ByVal e As System.EventArgs) Handles DropDownList1.DataBound
        'if the transfer type is cbn and the message type is MT103, then only have the cbn bank in the drop downl ist
        If Session("messagetype").ToString().Trim().ToLower() = easyrtgs.MESSAGETYPE_MT103 AndAlso Session("transtype").ToString().Trim().ToLower() = easyrtgs.TRANSTYPE_CBN Then
            DropDownList1.Items.Clear()
            Dim cbnBank As BankDetail = BankDetail.GetBankDetailByCode(BankDetail.getCbnBankCode())
            Dim text As String = cbnBank.Bank_Name
            Dim value As String = cbnBank.Bank_code
            DropDownList1.Items.Insert(0, New ListItem(text, value))
        Else
            DropDownList1.Items.Insert(0, New ListItem("-Choose Bank-", "0"))
        End If

        Dim transtype As String = Session("transtype").ToString()
        Dim ddl As DropDownList = CType(sender, DropDownList)
        CustomizeDropDownForBanksByTransferType(ddl, transtype)

    End Sub
    Protected Sub btnSubmit202_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnSubmit202.Click
        rblApproveReject.ClearSelection()
        'Perform token validation here
        Dim username As String = txtUsername202.Text
        Dim token As String = txtToken202.Text
        lblSubmissionText202.Text = String.Empty

        If ConfigurationManager.AppSettings("APP_MODE").ToString().ToLower() = "online" Then
            Dim tokenValUtil As New TokenValidatorUtil()
            Dim response As TokenResponseHolder = tokenValUtil.ValidateToken(username, token.Trim())
            If Not response.IsValid Then
                Dim responseMsg As String = response.ResponseMessage
                lblSubmissionText202.Visible = True
                lblSubmissionText202.Text = "<font color=red>" & responseMsg & "</font>"
                Exit Sub
            End If
        End If

        If String.IsNullOrEmpty(txtCustAcct.Text) Then
            Label7.Text = "Please input the account number."
            Exit Sub
        End If
        If hiddenShowPopup.Value.Trim().ToLower() = "true" Then
            DisplayPreview(Session("messagetype").ToString())
            mpe.Show()
            Exit Sub
        End If



        If String.IsNullOrEmpty(txtAmount202.Text) Then
            txtAmount202.Focus()

            Return
        End If
        Try
            Dim amount As Decimal = Convert.ToDecimal(txtAmount202.Text)
            If amount <= 0 Then
                Return
            End If
        Catch ex As Exception
            Return
        End Try

        'Check if the account numbers are empty string as well
        If String.IsNullOrEmpty(txtCustAcct.Text) Then
            Return
        End If

        If ConfigurationManager.AppSettings("APP_MODE").Trim().ToLower() <> "offline" Then
            If Convert.ToDecimal(txtAmount202.Text) < 10000000 Then
                CompareValidator2.IsValid = False
                Exit Sub
            End If
        End If



        If tid Is Nothing Then
            Server.Transfer("tropsmain.aspx")


        End If
        Dim es As New easyrtgs

        Dim charges As Decimal = 0D
        Dim vatRate As Decimal = 0.05D

        If CheckBox2.Checked Then
            If txtConcession202.Text = String.Empty Then

                charges = 0
            Else
                charges = Val(txtConcession202.Text)

            End If

        Else
            Dim t24 As New T24Bank()
            Dim acct As IAccount = t24.GetAccountInfoByAccountNumber(txtCustAcct.Text)
            Dim isstaff As Boolean = False
            If acct IsNot Nothing Then
                isstaff = acct.AccountType.Trim().ToLower().Contains("staff")
            End If

            If isstaff Then

                charges = Convert.ToDecimal(es.getStaffCharges())
            Else

                charges = Convert.ToDecimal(es.getCustomerCharges())

            End If

        End If

        'But since there are no charges for MT202, then set the charges to 0
        charges = 0




        If Val(txtAmount202.Text) < 0 Then
            Label7.ForeColor = Drawing.Color.Red
            Label7.Text = "<table id='output' bgcolor='#ccffcc' width='500'><tr><td valign='middle' width='270'>" & "Amount cannot be a Negative value." & "</td><td valign='middle' width='30'><img src='images\bad.png' width='20' title='close' onclick='hide();'></td></tr></table>"
            Exit Sub

        End If

        If ConfigurationManager.AppSettings("APP_MODE").ToString().ToLower().CompareTo("offline") <> 0 Then
            If Val(Session("bal").ToString().Replace(",", "")) < Val(Val(txtAmount202.Text) + charges + (charges * vatRate)) Then


                Label7.ForeColor = Drawing.Color.Red
                Label7.Text = "<table id='output' bgcolor='#ccffcc' width='500'><tr><td valign='middle' width='470'>" & "Customer's Balance cannot handle this Transaction. N " & FormatNumber(Val(txtAmount202.Text) + charges, 2) & " is Required.</td><td valign='middle' width='30'><img src='images\bad.png' width='20' title='close' onclick='hide();'></td></tr></table>"
                Exit Sub


            End If
        End If



        Try

            Button5.Enabled = False

        Catch ex1 As Exception
            Gadget.LogException(ex1)
            Label7.ForeColor = Drawing.Color.Red
            Label7.Text = "<table id='output' bgcolor='#ccffcc' width='300'><tr><td valign='middle' width='270'>" & "Error: Cannot post this transaction." & "</td><td valign='middle' width='30'><img src='images\bad.png' width='20' title='close' onclick='hide();'></td></tr></table>"
            Button5.Text = "Post Transaction"
            Button5.Enabled = True

            Exit Sub

        End Try

        Dim conn As New Data.SqlClient.SqlConnection(es.sqlconn1())
        'Dim statusName As String = easyrtgs.TREASURY_STATUS 'Because we are going to Treasury straight away (see mytransactions.aspx for the Status code that shows pending transactions for Treasury)
        Dim statusName As String = "Uploaded-TROPS" 'Because we are going to Treasury straight away (see mytransactions.aspx for the Status code that shows pending transactions for Treasury)
        Dim newstatusName As String = GetStatusNameForTransAmount(Convert.ToDecimal(txtAmount202.Text))
        Dim msgtype As String = String.Empty
        Dim msgvariant As String = String.Empty

        msgtype = Session("messagetype").ToString()
        msgvariant = Session("variant").ToString()


        Dim esy As New easyrtgs
        Dim msgTypeID As Integer = 0
        Dim msgVarID As Integer = 0

        Dim mgdetOb As New MtMessageDetails
        msgTypeID = mgdetOb.GetMessageTypeIDByName(msgtype)
        msgVarID = mgdetOb.GetMessageVariantIDByName(msgvariant)
        Using conn
            conn.Open()
            Dim trans As Data.SqlClient.SqlTransaction = conn.BeginTransaction()
            Using trans

                Dim cmd As New Data.SqlClient.SqlCommand
                cmd.Connection = conn



                cmd.CommandText = "insert into transactions(messagetypeID, messageVariantID, transactionid,customer_name,customer_account,amount,charges,remarks,status,uploaded_by,uploaded_date,branch,Instruction,Beneficiary,Beneficiary_bank,Beneficiary_account,date, requiresCustCareApproval, requestingBranch,requestingBranchAccount) values" & _
                   "(@msgtypeID, @msgvariantID, @tid,@cusname,@cusacc,@amount,@charges,@remarks,@uploaded,@uploadby,'" & Now.ToString("yyyy-MM-dd hh:mm tt") & "',@branch,@Instruction,@benef,@benefBank,@benefAcc,@date, @requiresCustCareApproval,@requestingBranch,@requestingBranchAccount)"




                Try
                    cmd.Transaction = trans
                    cmd.Parameters.AddWithValue("@tid", tid)

                    cmd.Parameters.AddWithValue("@cusname", Server.HtmlEncode(txtCustName202.Text.Trim.Replace("'", "`"))) 'txtCustAcct

                    cmd.Parameters.AddWithValue("@cusacc", txtCustAcct.Text)

                    cmd.Parameters.AddWithValue("@amount", Server.HtmlEncode(FormatNumber(txtAmount202.Text.Trim, 2)))
                    cmd.Parameters.AddWithValue("@charges", Server.HtmlEncode(FormatNumber(charges, 2)))
                    cmd.Parameters.AddWithValue("@remarks", Server.HtmlEncode(txtRemarks202.Text.Trim.Replace("'", "`").Replace("&", "-")))
                    cmd.Parameters.AddWithValue("@uploadby", Session("name"))
                    cmd.Parameters.AddWithValue("@branch", Session("branch"))
                    cmd.Parameters.AddWithValue("@Instruction", instruction)
                    cmd.Parameters.AddWithValue("@benef", Server.HtmlEncode(txtBeneName202.Text))
                    cmd.Parameters.AddWithValue("@benefBank", Server.HtmlEncode(ddlBen202.SelectedItem.Text & ":" & ddlBen202.SelectedItem.Value))
                    cmd.Parameters.AddWithValue("@benefAcc", Server.HtmlEncode(txtBeneficiaryAcct202.Text))
                    cmd.Parameters.AddWithValue("@date", Now.ToString("yyyy-MM-dd"))
                    cmd.Parameters.AddWithValue("@uploaded", statusName)
                    cmd.Parameters.AddWithValue("@msgtypeID", msgTypeID)
                    cmd.Parameters.AddWithValue("@msgvariantID", msgVarID)
                    cmd.Parameters.AddWithValue("@requestingBranch", ddlOwnerBranch202.SelectedValue.ToString())
                    cmd.Parameters.AddWithValue("@requestingBranchAccount", Session("acc").ToString())



                    If statusName.Trim().ToLower().CompareTo("customercare") = 0 Then
                        cmd.Parameters.AddWithValue("@requiresCustCareApproval", 1)
                    Else
                        cmd.Parameters.AddWithValue("@requiresCustCareApproval", 0)
                    End If
                    'TODO: We changed from Pending to Authorize in order to achieve the debit here
                    'Dim postingStatus As String = "Pending"
                    Dim postingStatus As String = "Authorize"
                    If cmd.ExecuteNonQuery() > 0 Then

                        Dim isSavedPrincipal As Boolean = False
                        Dim isSavedCharges As Boolean = False
                        Dim isSavedVat As Boolean = False
                        Dim isNextStateUpdated As Boolean = False

                        Try
                            If newstatusName.Trim().ToLower().CompareTo("customercare") = 0 Then
                                Dim sqlUpdate As String = "update transactions set postConfirmationStatus=@newStatus where TransactionID=@tid"
                                Dim cmdUpdate As New Data.SqlClient.SqlCommand(sqlUpdate, conn)
                                cmdUpdate.Transaction = trans
                                cmdUpdate.Parameters.AddWithValue("@tid", tid)
                                cmdUpdate.Parameters.AddWithValue("@newStatus", easyrtgs.SERVICE_MANAGER_STATUS)
                                If cmdUpdate.ExecuteNonQuery() > 0 Then
                                    isNextStateUpdated = True
                                Else
                                    isNextStateUpdated = False
                                End If
                            Else
                                isNextStateUpdated = True 'cause i need it in the Commit() check
                            End If
                        Catch ex As Exception
                            Gadget.LogException(ex)
                        End Try


                        Dim sql As String = "spSavePostingEntries"
                        Dim cmdInsertTransactTemp As New Data.SqlClient.SqlCommand(sql, conn)
                        cmdInsertTransactTemp.Transaction = trans
                        cmdInsertTransactTemp.CommandType = Data.CommandType.StoredProcedure
                        cmdInsertTransactTemp.Parameters.AddWithValue("@transid", tid) 'txtCustName202
                        cmdInsertTransactTemp.Parameters.AddWithValue("@CustomerName", Server.HtmlEncode(txtCustName202.Text.Trim.Replace("'", "`")))
                        cmdInsertTransactTemp.Parameters.AddWithValue("@Amount", Convert.ToDecimal(Server.HtmlEncode(FormatNumber(txtAmount202.Text.Trim(), 2))))
                        cmdInsertTransactTemp.Parameters.AddWithValue("@Officer", Session("name"))
                        cmdInsertTransactTemp.Parameters.AddWithValue("@Supervisor", String.Empty) 'This one will require the name of the approver later on.
                        cmdInsertTransactTemp.Parameters.AddWithValue("@CrCommAcct", esy.getRTGSSuspense(esy.checkisIMAL(tid)))
                        If Session("messagetype") IsNot Nothing AndAlso Session("variant") IsNot Nothing Then
                            If Session("messagetype").ToString().Trim().ToLower() = easyrtgs.MESSAGETYPE_MT202.ToString().Trim().ToLower() AndAlso (Session("variant").ToString().Trim().ToLower() = easyrtgs.MESSAGE_TYPE_VARIANT_REGULAR.ToString().Trim().ToLower() OrElse Session("variant").ToString().Trim().ToLower() = easyrtgs.MESSAGE_TYPE_VARIANT_RESERVATION.ToString().Trim().ToLower()) Then
                                'cmdInsertTransactTemp.Parameters.Remove(cmdInsertTransactTemp.Parameters.Item("@DrCustAcct")) 'first remove DrCustAcct
                                cmdInsertTransactTemp.Parameters.AddWithValue("@DrCustAcct", Session("acc").ToString())
                            ElseIf Session("messagetype").ToString().Trim().ToLower() = easyrtgs.MESSAGETYPE_MT202.ToString().Trim().ToLower() AndAlso Session("variant").ToString().Trim().ToLower() = easyrtgs.MESSAGE_TYPE_VARIANT_SDF.ToString().Trim().ToLower() Then
                                cmdInsertTransactTemp.Parameters.RemoveAt("@CrCommAcct")
                                cmdInsertTransactTemp.Parameters.AddWithValue("@CrCommAcct", esy.getRTGSSuspense202(esy.checkisIMAL(tid)))
                                cmdInsertTransactTemp.Parameters.AddWithValue("@DrCustAcct", esy.getRTGSSuspense202(esy.checkisIMAL(tid)))
                            End If
                        Else
                            cmdInsertTransactTemp.Parameters.AddWithValue("@DrCustAcct", Session("acc").ToString())

                        End If
                        'cmdInsertTransactTemp.Parameters.AddWithValue("@DrCustAcct", Session("acc").ToString())

                        cmdInsertTransactTemp.Parameters.AddWithValue("@expl_code", esy.getExplCode())
                        cmdInsertTransactTemp.Parameters.AddWithValue("@Remarks", esy.SanitizeRemarks(Convert.ToString(Server.HtmlEncode(tid & "-" & txtRemarks202.Text.Trim.Replace("'", "`").Replace("&", "-")))))
                        cmdInsertTransactTemp.Parameters.AddWithValue("@Source", "EasyRtgs")
                        cmdInsertTransactTemp.Parameters.AddWithValue("@Branch", Session("branch").ToString())
                        cmdInsertTransactTemp.Parameters.AddWithValue("@Status", postingStatus)

                        If cmdInsertTransactTemp.ExecuteNonQuery() > 0 Then
                            isSavedPrincipal = True
                        End If


                        cmdInsertTransactTemp.Parameters.Clear()



                        If isSavedPrincipal And isNextStateUpdated Then
                            Try
                                trans.Commit()
                                conn.Close()
                                conn.Dispose()


                                esy.createAudit(tid & " was posted by " & Session("name"), Now)
                                esy.TransactionAudit(tid & " was posted by " & Session("name"), Now, tid)


                                'notify authorizer
                                Dim body As String = ""
                                body = "Sir/Madam" & "<br>" & "<br>" & "This mail is a request for you to authorize a Third Party Transfer Transaction . Details of this transaction are as follows;" & "<br>" & _
                                "Customer: " & txtCustName103.Text & "<br>" & "Amount: " & FormatNumber(txtAmount202.Text, 2) & "<br>" & " Account Number: " & txtCustAcct.Text.Trim() & "<br><br>" & "Click on the link below to authorize this transaction. <br><br><a href='http://" & System.Configuration.ConfigurationManager.AppSettings("serverip") & "/" & System.Configuration.ConfigurationManager.AppSettings("AppVirDir") & "/default.aspx?action=authorize&tid=" & tid & "'>Authorize this Transaction</a> <br><br>Thank You"

                                Try
                                    Dim mail As New easyrtgs
                                    If statusName.Trim().ToLower().CompareTo("customercare") = 0 Then 'if it is the customercare
                                        mail.sendmail(System.Configuration.ConfigurationManager.AppSettings("mailHost"), mail.getAuthorizerEmailbyName("Customer Care"), Session("email"), "3RD PARTY TRANSACTION CUSTOMER CARE CONFIRMATION REQUEST (TRANSACTION ID: " & tid & ")", body)
                                    Else
                                        'send mail to the trops approver whose role is "Approver"
                                        Dim toaddress As String = mail.getAuthorizerEmail("")
                                        Dim host As String = System.Configuration.ConfigurationManager.AppSettings("mailHost")
                                        Dim taddr As String = mail.getAuthorizerEmail(String.Empty, "Approver")
                                        Dim ccaddr As String = Session("email")
                                        Dim subject As String = "3RD PARTY TRANSACTION AUTHORIZATION REQUEST (TRANSACTION ID: " & tid & ")"

                                        mail.sendmail(host, taddr, ccaddr, subject, body)
                                    End If


                                Catch ex As Exception
                                    Gadget.LogException(ex)
                                End Try


                                Label7.ForeColor = Drawing.Color.Green
                                Label7.Text = "<table id='output' bgcolor='#ccffcc' width='300'><tr><td valign='middle' width='270'>" & "Transaction was posted.(" & tid & ")" & "</td><td valign='middle' width='30'><img src='images\bad.png' width='20' title='close' onclick='hide();'></td></tr></table>"
                                ClearMt202Form()

                                Server.Transfer("postsuccess.aspx", False)
                            Catch ex As Exception
                                Gadget.LogException(ex)
                                trans.Rollback()
                                Label7.Text = "<table id='output' bgcolor='#ccffcc' width='300'><tr><td valign='middle' width='270'>" & "Transaction was posted.(" & tid & ")" & "</td><td valign='middle' width='30'><img src='images\bad.png' width='20' title='close' onclick='hide();'></td></tr></table>"
                            End Try


                        Else
                            trans.Rollback()
                        End If





                    Else
                        trans.Rollback()
                    End If

                Catch ex As Exception
                    Gadget.LogException(ex)
                    'trans.Rollback()

                    If ex.Message.Contains("duplicate") Then
                        Label7.ForeColor = Drawing.Color.Red
                        Label7.Text = "<table id='output' bgcolor='#ccffcc' width='300'><tr><td valign='middle' width='270'>" & "Duplicate Transaction ID. Already exist." & "</td><td valign='middle' width='30'><img src='images\bad.png' width='20' title='close' onclick='hide();'></td></tr></table>"
                        Button5.Text = "Post Transaction"
                        Button5.Enabled = True

                        Exit Sub
                    End If

                    If ex.Message.Contains("Input String") Then

                        Label7.ForeColor = Drawing.Color.Red
                        Label7.Text = "<table id='output' bgcolor='#ccffcc' width='300'><tr><td valign='middle' width='270'>" & "Error: Cannot Post. Check all fields" & "</td><td valign='middle' width='30'><img src='images\bad.png' width='20' title='close' onclick='hide();'></td></tr></table>"
                        Button5.Text = "Post Transaction"
                        Button5.Enabled = True

                        Exit Sub

                    End If

                    Label7.ForeColor = Drawing.Color.Red
                    Label7.Text = "<table id='output' bgcolor='#ccffcc' width='300'><tr><td valign='middle' width='270'>" & ex.Message & "</td><td valign='middle' width='30'><img src='images\bad.png' width='20' title='close' onclick='hide();'></td></tr></table>"
                    Button5.Text = "Post Transaction"
                    Button5.Enabled = True


                End Try

            End Using

        End Using
    End Sub

    Private Sub DisableNameQuery(ByVal disableEnquiry As Boolean)
        txtBeneName202.ReadOnly = Not disableEnquiry
    End Sub

    Public Sub DisplayPreview(ByVal messageType As String)
        If messageType.Trim().ToLower() = easyrtgs.MESSAGETYPE_MT103.Trim().ToLower() Then
            DisplayMT103Preview()
        ElseIf messageType.Trim().ToLower() = easyrtgs.MESSAGETYPE_MT202.Trim().ToLower() Then
            DisplayMT202Preview()
        End If
    End Sub


    Protected Sub ddlBen202_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlBen202.SelectedIndexChanged
        'get the nuban and the bank name for the beneficiary name if the message type is 202 and the 
        'TODO:
        Dim bank_code As String = ddlBen202.SelectedValue.ToString()
        txtBeneName202.Text = ddlBen202.SelectedItem.Text
        'get the bank nuban by the bank code
        Dim bankdet As BankDetail = BankDetail.GetBankDetailByCode(bank_code)
        If bankdet IsNot Nothing Then
            txtBeneficiaryAcct202.Text = bankdet.Nuban
        End If

    End Sub

    Private Sub ClearMt202Form()
        btnSubmit202.Text = "Post Transaction"
        btnSubmit202.Enabled = True
        txtCustAcct.Text = ""
        txtCustName103.Text = ""
        txtAmount202.Text = ""
        TextBox4.Text = ""
        lblBalance103.Text = ""
    End Sub


    'Protected Sub rblRem_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles rblRem.SelectedIndexChanged
    '    Dim remarkVal As String = rblRem.SelectedValue.ToString()
    '    Select Case remarkVal
    '        Case "NONE"

    '            txtRemarks202.Text = String.Empty
    '        Case Else
    '            txtRemarks202.Text = remarkVal
    '    End Select  'Session("variant") = easyrtgs.MESSAGE_TYPE_VARIANT_SDF
    'End Sub

    Private Sub SetupForMessageVariant(ByVal mvariant As String, ByVal transtype As String)

        ''TODO: Include a way of not overwriting the place for getting the requesting branch's account number
        ''txtCustAcct.Text = bdet.Nuban
        Select Case mvariant.Trim().ToLower()
            Case easyrtgs.MESSAGE_TYPE_VARIANT_REGULAR.Trim().ToLower()
                'TODO
                Dim bankCode As String = String.Empty
                bankCode = BankDetail.getSterlingBankCode()
                Dim bdet As BankDetail = BankDetail.GetBankDetailByCode(bankCode)
                'Default the sending customer name to Sterling
                txtCustAcct.Text = bdet.Nuban
                txtCustName202.Text = bdet.Bic
                'Default beneficiary bank to Central Bank

                bankCode = BankDetail.getCbnBankCode()
                bdet = BankDetail.GetBankDetailByCode(bankCode)

                ddlBen202.SelectedValue = bankCode
                'Disable changing the value of the drop down list
                ddlBen202.Enabled = False

                txtBeneName202.Text = "C/NGN149250001" 'bdet.Bic
                txtBeneficiaryAcct202.Text = bdet.Nuban
                txtRemarks202.Text = String.Empty ' "AS APPROPRIATE
            Case easyrtgs.MESSAGE_TYPE_VARIANT_SDF.Trim().ToLower()
                'TODO


                Dim bankCode As String = String.Empty
                bankCode = BankDetail.getSterlingBankCode()
                Dim bdet As BankDetail = BankDetail.GetBankDetailByCode(bankCode)
                'Default the sending customer name to Sterling
                txtCustAcct.Text = bdet.Nuban
                'Disable this from being changed
                txtCustAcct.ReadOnly = True

                txtCustName202.Text = bdet.Bic
                'Default beneficiary bank to Central Bank

                bankCode = BankDetail.getCbnBankCode()
                bdet = BankDetail.GetBankDetailByCode(bankCode)

                ddlBen202.SelectedValue = bankCode
                'Disable the drop down list
                ddlBen202.Enabled = False

                txtBeneName202.Text = bdet.Bic
                txtBeneficiaryAcct202.Text = bdet.AlternateAccount
                txtRemarks202.Text = easyrtgs.MESSAGE_TYPE_VARIANT_SDF

            Case easyrtgs.MESSAGE_TYPE_VARIANT_RESERVATION.Trim().ToLower()
                'TODO
                Dim bankCode As String = String.Empty
                bankCode = BankDetail.getSterlingBankCode()
                Dim bdet As BankDetail = BankDetail.GetBankDetailByCode(bankCode)
                'Default the sending customer name to Sterling
                txtCustAcct.Text = bdet.Nuban
                txtCustName202.Text = bdet.Bic

                'Default beneficiary bank to Sterling
                ddlBen202.SelectedValue = bankCode
                'Disable the drop down list
                ddlBen202.Enabled = False


                'Call the event handler for the drop down list to trigger the necessary logic

                ddlBen202.SelectedValue = bankCode
                txtBeneName202.Text = bdet.Bic
                txtBeneficiaryAcct202.Text = bdet.Nuban

                'get the CBN BIC code and remove any X at the end of it 
                bdet = BankDetail.GetBankDetailByCode(BankDetail.getCbnBankCode())
                If bdet.Bic.Trim().ToLower().Contains("X") Then
                    txtRemarks202.Text = bdet.Bic.Replace("X", String.Empty)
                Else
                    txtRemarks202.Text = bdet.Bic
                End If

                ''Default the remark to "RESERVE CREDIT"
                'txtRemarks202.Text = "RESERVE CREDIT"
            Case Else
                'TODO

        End Select

        Select Case transtype.Trim().ToLower()
            Case easyrtgs.TRANSTYPE_INTERBANK.ToLower()
                'if the it is an interbank transaction, then just enable the drop down list of beneficiary bank
                ddlBen202.Enabled = True
            Case Else
                'TODO: 
        End Select
    End Sub

    Protected Sub ddlOwnerBranch_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlOwnerBranch.SelectedIndexChanged
        'Dim branchAccountNumberNuban As String = String.Empty
        'Dim g As New Gadget
        'Dim branchcode As String = ddlOwnerBranch.SelectedValue.ToString()
        'If (branchcode.Trim() <> "0") Then
        '    Dim eob As New easyrtgs
        '    Dim bdets As BranchDetail = g.getMt202BranchDetails(branchcode)
        '    branchAccountNumberNuban = eob.getBranchMT202AccountNumberByCode(branchcode)

        '    Session("acc") = bdets.BranchCode & "-" & bdets.Cusnum & "-" & bdets.CurrencyCode & "-" & bdets.LedgerCode & "-" & bdets.SubAccountCode
        '    Session("ledcode") = bdets.LedgerCode

        '    'Get the customer name
        '    Dim name As String = String.Empty
        '    Dim bs As New bank.bank
        '    name = bs.getAccountName(branchAccountNumberNuban)
        '    Dim balance As Decimal = 0D

        '    TextBox1.Text = branchAccountNumberNuban

        '    Dim banksAcct As String = bdets.BranchCode & bdets.Cusnum & bdets.CurrencyCode & bdets.LedgerCode & bdets.SubAccountCode
        '    '            Dim dsBalance As DataSet = bs.getBalanceDetails(branchAccountNumberNuban)
        '    Dim dsBalance As DataSet = bs.getBalanceDetails(banksAcct)
        '    If dsBalance IsNot Nothing Then
        '        If dsBalance.Tables.Count > 0 Then
        '            If dsBalance.Tables(0).Rows.Count > 0 Then
        '                Dim dr As DataRow = dsBalance.Tables(0).Rows(0)
        '                If Not IsDBNull(dr("AVAIL_BAL")) Then
        '                    balance = Convert.ToDecimal(dr("AVAIL_BAL").ToString())
        '                End If

        '            End If
        '        End If
        '    Else

        '    End If
        '    If Convert.ToDecimal(balance) > 0 Then
        '        lblBalance103.Text = balance.ToString("0,000.00")
        '    Else
        '        lblBalance103.Text = balance.ToString()
        '    End If

        '    Session("bal") = balance
        '    'TODO: Remove this
        '    If String.IsNullOrEmpty(name) Then
        '        name = String.Format("Branch {0} Account", branchcode)
        '    End If

        '    If String.IsNullOrEmpty(txtCustName103.Text) Then
        '        txtCustName103.Text = name
        '    End If
        'Else
        '    Return
        'End If
    End Sub

    Protected Sub DropDownList1_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles DropDownList1.SelectedIndexChanged
        ''get the nuban and the bank name for the beneficiary name if the message type is 202 and the 
        ''TODO:
        Dim bank_code As String = DropDownList1.SelectedValue.ToString()
        'txtBenName103.Text = DropDownList1.SelectedItem.Text
        'get the bank nuban by the bank code
        Dim bankdet As BankDetail = BankDetail.GetBankDetailByCode(bank_code)
        'If bankdet IsNot Nothing Then
        '    txtBenAccount103.Text = bankdet.Nuban
        'End If
        If Not bankdet.IsNameEnquirySupported Then
            RegularExpressionValidator4.Enabled = False 'Disable if the bank does not support name enquiry.
        Else
            RegularExpressionValidator4.Enabled = True
        End If
        'RegularExpressionValidator4
    End Sub

    Protected Sub CheckBox2_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles CheckBox2.CheckedChanged
        txtConcession202.Enabled = CheckBox2.Checked
        txtConcession202.ReadOnly = Not CheckBox2.Checked
    End Sub

    Private Sub DisplayMT103Preview()
        lblBranch.Text = "n/a" 'ddlOwnerBranch.SelectedItem.ToString()
        lblCustAcctNum.Text = TextBox1.Text
        lblCustName.Text = txtCustName103.Text
        lblCustAcctBalance.Text = lblBalance103.Text
        lblAmount.Text = TextBox3.Text
        txtRem.Text = TextBox4.Text
        lblBenAccountNumber.Text = txtBenAccount103.Text
        lblBenBank.Text = DropDownList1.SelectedItem.Text
        lblMessagePreview.Text = easyrtgs.MESSAGETYPE_MT103.ToUpper()
    End Sub

    Private Sub DisplayMT202Preview()
        lblBranch.Text = ddlOwnerBranch202.SelectedItem.ToString()
        lblCustAcctNum.Text = txtCustAcct.Text
        lblCustName.Text = txtCustName202.Text
        lblCustAcctBalance.Text = lblBalance202.Text
        lblAmount.Text = txtAmount202.Text
        txtRem.Text = txtRemarks202.Text
        lblBenAccountNumber.Text = txtBeneficiaryAcct202.Text
        lblBenBank.Text = ddlBen202.SelectedItem.Text

        lblMessagePreview.Text = easyrtgs.MESSAGETYPE_MT202.ToUpper()
    End Sub

    Protected Sub btnContinue_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnContinue.Click
        hiddenShowPopup.Value = "false"
        Dim mtype As String = lblMessagePreview.Text

        Select Case mtype.Trim().ToLower()
            Case easyrtgs.MESSAGETYPE_MT103
                Button5_Click(sender, e)
            Case easyrtgs.MESSAGETYPE_MT202
                btnSubmit202_Click(sender, e)
        End Select

    End Sub

    Protected Sub btnCancel_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnCancel.Click
        hiddenShowPopup.Value = "true"
    End Sub

    Protected Sub rblApproveReject_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles rblApproveReject.SelectedIndexChanged
        If rblApproveReject.SelectedValue.ToString().ToLower() = "approve" Then
            hiddenShowPopup.Value = "false"
        Else
            hiddenShowPopup.Value = "true"
        End If

        If hiddenShowPopup.Value = "false" Then
            Select Case Session("messagetype").ToString().Trim().ToLower()
                Case easyrtgs.MESSAGETYPE_MT202.Trim().ToLower()
                    btnSubmit202_Click(sender, e)
                Case easyrtgs.MESSAGETYPE_MT103.Trim().ToLower()
                    Button5_Click(sender, e)
                Case Else

            End Select
        End If

    End Sub

    Private Sub CustomizeDropDownForBanksByTransferType(ByVal ddl As DropDownList, ByVal transtype As String)
        Select Case transtype.Trim().ToLower()
            Case easyrtgs.TRANSTYPE_INTERBANK.Trim().ToLower()
                ddl.Items.Remove(BankDetail.GetBankDetailByCode(BankDetail.getCbnBankCode()).Bank_Name)
            Case Else
        End Select
    End Sub

End Class

