﻿Imports Microsoft.VisualBasic
Imports System.Data.SqlClient
Imports System.Data
Imports SwiftLib

Public Class EasyRtgTransaction
    Public Property id As Long
    Public Property TransactionID As String
    Public Property Customer_name As String
    Public Property Customer_account As String
    Public Property amount As String
    Public Property charges As String
    Public Property Remarks As String
    Public Property status As String
    Public Property Uploaded_by As String
    Public Property Authorized_by As String
    Public Property Authorized_date As DateTime
    Public Property Treasury_approval As String
    Public Property Approved_date As DateTime
    Public Property Branch As String
    Public Property Instruction As String
    Public Property Beneficiary_Bank As String
    Public Property Beneficiary As String
    Public Property Beneficiary_account As String
    Public Property [date] As Date
    'Public Property mt103_text As String
    Public Property telex_copy As String
    Public Property InErrorFolderBit As Integer
    Public Property CustomerEmail As String
    Public Property Comment As String 'comment
    Public Property RequiresCustCareApproval As Integer 'requiresCustCareApproval
    Public Property CustCareComment As String 'CustCareComment
    Public Property CustCareApproverUserid As String 'CustCareApproverUserid
    Public Property CustCareApprovalDate As Date 'CustCareApprovalDate
    Public Property CustCareApproveReject As String 'CustCareApproveReject
    Public Property MessagetypeID As Integer 'messagetypeID
    Public Property MessageVariantID As Integer 'messageVariantID
    Public Property RequestingBranch As String
    Public Property RequestingBranchAccount As String
    Public Property PostConfirmationStatus As String
    Private _messageTypeName As String
    Private _messageVariantName As String

    Public ReadOnly Property MessageTypeName As String
        Get
            If Not String.IsNullOrEmpty(_messageTypeName) Then
                Return _messageTypeName
            Else
                Dim msgDet As New MtMessageDetails
                _messageTypeName = msgDet.GetMessageTypeNameByID(MessagetypeID)
                If String.IsNullOrEmpty(_messageTypeName) Then
                    _messageTypeName = String.Empty
                End If
                Return _messageTypeName
            End If


        End Get
    End Property


    Public ReadOnly Property MessageVariantName As String
        Get
            If Not String.IsNullOrEmpty(_messageVariantName) Then
                Return _messageVariantName
            Else
                Dim msgDet As New MtMessageDetails
                _messageVariantName = msgDet.GetMessageVariantNameByID(MessageVariantID)
                Return _messageVariantName
            End If


        End Get
    End Property
    Public Property mt103_text As String
        Get
            Dim ms As String = String.Empty
            Try
                Dim cn As New Connect("select mt103_text from transactions where TransactionID=@transid")
                cn.addparam("@transid", TransactionID)
                Dim ds As DataSet = cn.query("dsMt103")
                If ds IsNot Nothing Then
                    If ds.Tables.Count > 0 Then
                        If ds.Tables(0).Rows.Count > 0 Then
                            If Not IsDBNull(ds.Tables(0).Rows(0)("mt103_text")) Then
                                ms = ds.Tables(0).Rows(0)("mt103_text").ToString()
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
                Gadget.LogException(ex)
            End Try

            Return ms
        End Get
        Set(ByVal value As String)
            Try
                Dim cn As New Connect("UPDATE transactions SET mt103_text=@mt103 where transactionid=@transid")

                cn.addparam("@transid", TransactionID)
                cn.addparam("@mt103", value)

                Dim i As Integer = cn.query()

                If i > 0 Then
                    'success
                End If
            Catch ex As Exception
                Gadget.LogException(ex)
            End Try




        End Set
    End Property

    Public Sub New(ByVal transactionId As String)
        populateObject(transactionId)
    End Sub

    Private Shared Function BuildRemark(ByVal oldRemark As String, ByVal FrmBankCode As String, ByVal FrmBankName As String, ByVal FromAcct As String, ByVal frmCurrency As String, ByVal Initiator As String, ByVal fromBranchCode As String, ByVal transid As String, ByVal toBankCode As String, ByVal toBankName As String, ByVal toAccount As String, ByVal tocurrency As String, ByVal beneficiary As String, ByVal toBranchCode As String) As String
        Dim remark As String = String.Empty

        '"FrmBankCode:FrmBankName:FromAcct:frmcurrency:Initiator:BranchCode:sessionID|toBankCode:toBankName:toAccount:tocurrency:beneficiary:toBranchCode"
        'Dim format As String = "{13}-{0}:{1}:{2}:{3}:{4}:{5}:{6}/{7}:{8}:{9}:{10}:{11}:{12}"
        Dim format As String = "{0}:{1}:{2}:{3}:{4}:{5}:{6}/{7}:{8}:{9}:{10}:{11}:{12}"
        Dim nameLenSetting As String = String.Empty
        nameLenSetting = ConfigurationManager.AppSettings("nfiuNameLength").ToString()

        Dim nameLen As Integer = 0

        If Not String.IsNullOrEmpty(nameLenSetting) Then
            nameLen = Convert.ToInt64(nameLenSetting)
        End If

        If nameLen = 0 Then
            'remark = String.Format(format, FrmBankCode, FrmBankName, FromAcct, frmCurrency, Initiator, fromBranchCode, transid, toBankCode, toBankName, toAccount, tocurrency, beneficiary, toBranchCode, oldRemark)
            remark = String.Format(format, FrmBankCode, FrmBankName, FromAcct, frmCurrency, Initiator, fromBranchCode, transid, toBankCode, toBankName, toAccount, tocurrency, beneficiary, toBranchCode)
        Else
            remark = String.Format(format, FrmBankCode, formatNameByLenSetting(FrmBankName, nameLen), FromAcct, frmCurrency, formatNameByLenSetting(Initiator, nameLen), fromBranchCode, transid, toBankCode, formatNameByLenSetting(toBankName, nameLen), toAccount, tocurrency, formatNameByLenSetting(beneficiary, nameLen), toBranchCode)
        End If



        Return remark
    End Function

    'Private Shared Function BuildRemark(ByVal oldRemark As String, ByVal FrmBankCode As String, ByVal FrmBankName As String, ByVal FromAcct As String, ByVal frmCurrency As String, ByVal Initiator As String, ByVal fromBranchCode As String, ByVal transid As String, ByVal toBankCode As String, ByVal toBankName As String, ByVal toAccount As String, ByVal tocurrency As String, ByVal beneficiary As String, ByVal toBranchCode As String) As String
    '    Dim remark As String = String.Empty

    '    '"FrmBankCode:FrmBankName:FromAcct:frmcurrency:Initiator:BranchCode:sessionID|toBankCode:toBankName:toAccount:tocurrency:beneficiary:toBranchCode"
    '    Dim format As String = "{13}-{0}:{1}:{2}:{3}:{4}:{5}:{6}/{7}:{8}:{9}:{10}:{11}:{12}"
    '    Dim nameLenSetting As String = String.Empty
    '    nameLenSetting = ConfigurationManager.AppSettings("nfiuNameLength").ToString()

    '    Dim nameLen As Integer = 0

    '    If Not String.IsNullOrEmpty(nameLenSetting) Then
    '        nameLen = Convert.ToInt64(nameLenSetting)
    '    End If

    '    If nameLen = 0 Then
    '        remark = String.Format(format, FrmBankCode, FrmBankName, FromAcct, frmCurrency, Initiator, fromBranchCode, transid, toBankCode, toBankName, toAccount, tocurrency, beneficiary, toBranchCode, oldRemark)
    '    Else
    '        remark = String.Format(format, FrmBankCode, formatNameByLenSetting(FrmBankName, nameLen), FromAcct, frmCurrency, formatNameByLenSetting(Initiator, nameLen), fromBranchCode, transid, toBankCode, formatNameByLenSetting(toBankName, nameLen), toAccount, tocurrency, formatNameByLenSetting(beneficiary, nameLen), toBranchCode, oldRemark)
    '    End If



    '    Return remark
    'End Function

    ''' <summary>
    ''' Builds SAS-Compliant remarks.
    ''' 24th May 2016. Oluremi Oladipupo in response to customer complaints that the SAS-formatted remarks are not readable (meaning is not clear) asked that
    ''' the SAS formatting be removed.
    ''' </summary>
    ''' <param name="transid"></param>
    ''' <param name="legacyRemark"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function BuildNfiuCompliantRemark_T24(ByVal transid As String, ByVal legacyRemark As String) As String
        Return legacyRemark

        ''Commented out because we are not using the SAS formatting anymore as at 24th May 2016

        'If Not String.IsNullOrEmpty(legacyRemark) Then
        '    legacyRemark = legacyRemark.Replace("-", String.Empty)
        'End If
        'Dim remark As String = String.Empty
        'Dim mt103 As String = String.Empty
        'Dim fromAcctNo As String = String.Empty
        'Dim fromBankCode As String = ConfigurationManager.AppSettings("SenderBIC").ToString() '"NAMENGLA"
        'Dim fromBankName As String = ConfigurationManager.AppSettings("SenderName").ToString()  '"STERLING BANK PLC"
        'Dim fromCurrency As String = String.Empty
        'Dim fromInitiator As String = String.Empty
        'Dim fromBranchCode As String = String.Empty
        'Dim fromSessionID As String = transid

        'Dim toBankCode As String = String.Empty
        'Dim toBankName As String = String.Empty
        'Dim toAccount As String = String.Empty
        'Dim toCurrency As String = String.Empty
        'Dim beneficiaryName As String = String.Empty
        'Dim toBranchCode As String = String.Empty
        'Dim rtgsCurrency As String = String.Empty
        'rtgsCurrency = ConfigurationManager.AppSettings("rtgs_currency").ToString()
        'Dim branchSeparatorChar As String = ConfigurationManager.AppSettings("branchNameSeparatorChar").ToString()


        ''get the mt103 message generated
        'Dim ds As New DataSet()
        'Dim cn As New Connect("select * from transactions where TransactionID=@transid")
        'cn.addparam("@transid", transid)
        'ds = cn.query("transdataset")
        'Dim rawBranch As String = String.Empty
        'Dim splitParts() As String = Nothing
        'If ds IsNot Nothing Then
        '    If ds.Tables.Count > 0 Then
        '        If ds.Tables(0).Rows.Count > 0 Then
        '            Dim dr As DataRow = ds.Tables(0).Rows(0)
        '            If Not IsDBNull(dr("mt103_text")) Then
        '                If Not String.IsNullOrEmpty(dr("mt103_text").ToString()) Then
        '                    mt103 = dr("mt103_text").ToString()
        '                Else
        '                    Dim rtg As New EasyRtgTransaction(transid)
        '                    Dim msgbuild As New MtMessageDetails()
        '                    If rtg.MessageTypeName = easyrtgs.MESSAGETYPE_MT103 Then
        '                        mt103 = msgbuild.generateSwiftMt103Message(transid)
        '                    ElseIf rtg.MessageTypeName = easyrtgs.MESSAGETYPE_MT202 Then
        '                        mt103 = msgbuild.generateSwiftMt202Message(transid)
        '                    Else
        '                        mt103 = msgbuild.generateSwiftMt103Message(transid)
        '                    End If
        '                End If

        '            Else
        '                Dim rtg As New EasyRtgTransaction(transid)
        '                Dim msgbuild As New MtMessageDetails()
        '                If rtg.MessageTypeName = easyrtgs.MESSAGETYPE_MT103 Then
        '                    mt103 = msgbuild.generateSwiftMt103Message(transid)
        '                ElseIf rtg.MessageTypeName = easyrtgs.MESSAGETYPE_MT202 Then
        '                    mt103 = msgbuild.generateSwiftMt202Message(transid)
        '                Else
        '                    mt103 = msgbuild.generateSwiftMt103Message(transid)
        '                End If
        '            End If

        '            If Not IsDBNull(dr("Customer_account")) Then
        '                fromAcctNo = dr("Customer_account").ToString()
        '            End If

        '            fromCurrency = rtgsCurrency
        '            fromInitiator = dr("Customer_name").ToString()
        '            rawBranch = dr("Branch").ToString()
        '            If rawBranch.Contains(branchSeparatorChar) Then
        '                splitParts = rawBranch.Split(branchSeparatorChar)
        '                If splitParts IsNot Nothing Then
        '                    If splitParts.Length > 0 Then
        '                        rawBranch = splitParts(0)
        '                    End If
        '                End If
        '            End If
        '            fromBranchCode = rawBranch
        '            fromSessionID = transid.ToString()
        '            toCurrency = fromCurrency

        '            If String.IsNullOrEmpty(mt103) Then
        '                Return legacyRemark
        '            Else
        '                '{LEGACY_REMARK}:{BANK_CODE}:{DESTINATION_ACCOUNT_NO}:{ DESTINATION _ACCT_NAME}
        '                'SBK/XXXX IFO MESAPOL ALUMINYUM-BILAL BOSTANCIOGLU B/O IGWE UCHE EVERISTUS TELEX CHRGS:223:XXA123456: MESAPOL ALUMINYUM-BILAL BOSTANCIOGLU

        '                'new format: FrmBankCode:FrmBankName:FromAcct:frmcurrency:Initiator:BranchCode:sessionID/toBankCode:toBankName:toAccount:tocurrency:beneficiary:toBranchCode


        '                Dim bank_code As String = String.Empty
        '                Dim dest_acct_no As String = String.Empty
        '                Dim dest_acct_name As String = String.Empty

        '                'Get the information from the mt103 object
        '                Dim m As Message

        '                Dim r As New EasyRtgTransaction(transid)
        '                Dim msgTypeName As String = r.MessageTypeName.Trim().ToLower()
        '                If msgTypeName.Contains("202") Then
        '                    m = New Mt202Message()
        '                ElseIf msgTypeName.Contains("103") Then
        '                    m = New Mt103Message()
        '                Else
        '                    m = New Mt103Message()
        '                End If

        '                If Not String.IsNullOrEmpty(mt103) Then
        '                    m.ReadFromString(mt103)


        '                    ' toBranchCode  = String.Empty
        '                    Dim vToBankCode As String = String.Empty
        '                    toBankCode = m.DestinationTerminal
        '                    If (toBankCode.Trim().ToLower().Contains("x")) Then
        '                        vToBankCode = toBankCode.TrimEnd(New Char() {"X"}).Substring(0, 8)
        '                        toBankName = Gadget.ExpandBIC(vToBankCode, True)
        '                    Else
        '                        toBankName = toBankCode
        '                    End If

        '                    toBranchCode = String.Empty 'How can I get the branch code?

        '                    Dim sl As List(Of Field) = m("59")
        '                    Dim beneficiaryAccountNumber_f59 As Field = Nothing
        '                    If sl.Count > 0 Then
        '                        beneficiaryAccountNumber_f59 = CType(sl.Item(0), Field)
        '                    End If
        '                    If beneficiaryAccountNumber_f59 IsNot Nothing Then
        '                        Dim lines() As String = Field.GetLinesFromContent(beneficiaryAccountNumber_f59.Content)
        '                        'The first line should be the account number of the beneficiary remove the first slash
        '                        toAccount = lines(0).Trim("/")
        '                        Dim lineBuffer As New StringBuilder

        '                        For index As Integer = 1 To lines.Length - 1
        '                            lineBuffer.Append(lines(index).Trim().ToUpper())
        '                        Next

        '                        beneficiaryName = lineBuffer.ToString()

        '                    End If

        '                End If
        '            End If
        '            remark = BuildRemark(legacyRemark, fromBankCode, fromBankName, fromAcctNo, fromCurrency, fromInitiator, fromBranchCode, fromSessionID, toBankCode, toBankName, toAccount, toCurrency, beneficiaryName, toBranchCode)
        '            'Next
        '        End If
        '    End If
        'End If



        'Return remark
    End Function

    Private Sub populateObject(ByVal transactionId As String)
        Dim esy As New easyrtgs
        Dim cn As SqlConnection = New SqlConnection(esy.sqlconn1())
        Using cn
            cn.Open()
            Dim sql As String = "SELECT * FROM transactions WHERE TransactionID=@transid"
            Dim cmd As New SqlCommand(sql, cn)
            cmd.Parameters.AddWithValue("transid", transactionId)
            Dim dr As SqlDataReader = cmd.ExecuteReader()

            Using dr

                dr.Read()


                Me.id = dr("id").ToString
                Me.TransactionID = dr("TransactionID").ToString
                Me.Customer_name = dr("Customer_name").ToString
                Me.Customer_account = dr("Customer_account").ToString
                Me.amount = dr("amount").ToString
                Me.charges = dr("charges").ToString
                Me.Remarks = dr("Remarks").ToString
                Me.status = dr("status").ToString
                Me.Uploaded_by = dr("Uploaded_by").ToString
                Me.Authorized_by = dr("Authorized_by").ToString
                If Not IsDBNull(dr("Authorized_date")) Then
                    Me.Authorized_date = Convert.ToDateTime(dr("Authorized_date").ToString())
                Else
                    Me.Authorized_date = DateTime.Now
                End If


                Me.Treasury_approval = dr("Treasury_approval").ToString
                If Not IsDBNull(dr("Approved_date")) Then
                    Me.Approved_date = Convert.ToDateTime(dr("Approved_date").ToString)
                End If

                Me.Treasury_approval = dr("Treasury_approval").ToString
                If Not IsDBNull(dr("Approved_date")) Then
                    Me.Approved_date = dr("Approved_date").ToString
                End If

                Me.Branch = dr("Branch").ToString
                Me.Instruction = dr("Instruction").ToString
                Me.Beneficiary_Bank = dr("Beneficiary_Bank").ToString()
                Me.Beneficiary = dr("Beneficiary").ToString
                Me.Beneficiary_account = dr("Beneficiary_account").ToString()
                If Not IsDBNull(dr("date")) Then
                    Me.[date] = Convert.ToDateTime(dr("date").ToString())
                End If
                Me.mt103_text = dr("mt103_text").ToString()
                Me.telex_copy = dr("telex_copy").ToString()
                If Not IsDBNull(dr("InErrorFolder")) Then
                    Me.InErrorFolderBit = Convert.ToInt32(dr("InErrorFolder").ToString())
                Else
                    Me.InErrorFolderBit = -1
                End If
                Me.CustomerEmail = dr("Customer_email").ToString()

                'Set the property
                '
                Me.Comment = IIf(Not IsDBNull(dr("Comment")), dr("Comment").ToString(), String.Empty)
                Me.RequiresCustCareApproval = IIf(Not IsDBNull(dr("requiresCustCareApproval")), dr("requiresCustCareApproval").ToString(), String.Empty)
                Me.CustCareComment = IIf(Not IsDBNull(dr("CustCareComment")), dr("CustCareComment").ToString(), String.Empty)
                Me.CustCareApproverUserid = IIf(Not IsDBNull(dr("CustCareApproverUserid")), dr("CustCareApproverUserid").ToString(), String.Empty)

                Try
                    Me.CustCareApprovalDate = IIf(Not IsDBNull(dr("CustCareApprovalDate")), Convert.ToDateTime(dr("CustCareApprovalDate").ToString()), DateTime.Now)

                Catch ex As Exception
                    Me.CustCareApprovalDate = DateTime.Now
                End Try
                If Not IsDBNull(dr("CustCareApproveReject")) Then
                    Dim approveRejectString As String = dr("CustCareApproveReject").ToString()
                    If approveRejectString.Trim().ToLower().Contains("-") Then
                        Dim parts() As String = approveRejectString.Split("-")

                        Me.CustCareApproveReject = Convert.ToInt32(parts(1))

                    Else
                        Me.CustCareApproveReject = Convert.ToInt32(dr("CustCareApproveReject").ToString())
                    End If

                Else
                    Me.CustCareApproveReject = 0
                End If


                If Not IsDBNull(dr("messagetypeID")) Then
                    Try
                        If Not String.IsNullOrEmpty(dr("messagetypeID").ToString()) Then
                            Me.MessagetypeID = Convert.ToInt32(dr("messagetypeID").ToString())
                        Else
                            Me.MessagetypeID = -1
                        End If

                    Catch ex As Exception

                    End Try
                    Me.MessagetypeID = dr("messagetypeID").ToString()
                Else
                    Me.MessagetypeID = -1
                End If


                If Not IsDBNull(dr("messageVariantID")) Then
                    Try
                        If Not String.IsNullOrEmpty(dr("messageVariantID").ToString()) Then
                            Me.MessageVariantID = Convert.ToInt32(dr("messageVariantID").ToString())
                        Else
                            Me.MessageVariantID = -1
                        End If

                    Catch ex As Exception

                    End Try
                    Me.MessageVariantID = dr("messageVariantID").ToString()
                Else
                    Me.MessageVariantID = -1
                End If

                If Not IsDBNull(dr("requestingBranch")) Then
                    Try
                        If Not String.IsNullOrEmpty(dr("requestingBranch").ToString()) Then
                            Me.RequestingBranch = Convert.ToInt32(dr("requestingBranch").ToString())
                        Else
                            Me.RequestingBranch = 0
                        End If

                    Catch ex As Exception

                    End Try
                    Me.RequestingBranch = dr("requestingBranch").ToString()
                Else
                    Me.RequestingBranch = 0
                End If


                'Me.RequestingBranch = IIf(Not IsDBNull(dr("requestingBranch")) OrElse Not String.IsNullOrEmpty(dr("requestingBranch").ToString()), Convert.ToInt32(dr("requestingBranch").ToString()), -1)

                If Not IsDBNull(dr("requestingBranchAccount")) Then
                    Try
                        If Not String.IsNullOrEmpty(dr("requestingBranchAccount").ToString()) Then
                            Me.RequestingBranchAccount = dr("requestingBranchAccount").ToString()
                        Else
                            Me.RequestingBranchAccount = "0"
                        End If
                    Catch ex As Exception

                    End Try
                    Me.RequestingBranchAccount = dr("requestingBranchAccount").ToString()
                Else
                    Me.RequestingBranchAccount = 0
                End If
                If Not IsDBNull(dr("postConfirmationStatus")) Then
                    If Not String.IsNullOrEmpty(dr("postConfirmationStatus").ToString()) Then
                        PostConfirmationStatus = dr("postConfirmationStatus").ToString()
                    Else
                        PostConfirmationStatus = easyrtgs.SERVICE_MANAGER_STATUS
                    End If
                Else
                    PostConfirmationStatus = easyrtgs.SERVICE_MANAGER_STATUS
                End If

                dr.Close()
            End Using



        End Using
    End Sub

    Private Shared Function formatNameByLenSetting(ByVal name As String, ByVal nameLen As Integer) As String
        Dim val As String = String.Empty
        If Not String.IsNullOrEmpty(name) Then
            If name.Length >= nameLen Then
                val = name.Substring(0, nameLen)
            Else
                val = name
            End If
        End If
        Return val
    End Function

End Class
