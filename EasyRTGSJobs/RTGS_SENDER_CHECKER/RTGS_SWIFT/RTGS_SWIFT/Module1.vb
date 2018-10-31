Imports System.Data.SqlClient
Imports System.IO
Imports SwiftLib
Imports System.Text
Imports RTGS_SWIFT.ftpService
Imports System.Net
Imports Nini

Module Module1
    Private Sub ExitIfThereIsAClone()
        Console.WriteLine("Check for clone.")
        If (System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Count() > 1) Then
            System.Diagnostics.Process.GetCurrentProcess().Kill()
        End If
    End Sub



    Sub Main()
        'WriteToSwiftInbox("testcontent2")

        ExitIfThereIsAClone()
        'While True

        '        Dim sql As String = "SELECT top 1 * FROM transactions where Status='SwiftReady' ORDER BY [date] DESC"
        Dim sql As String = Utils.GetMainQuery() '"SELECT * FROM transactions where Status='SwiftReady' ORDER BY [date] DESC"
        Dim linePrefix As String = Utils.GetField72LinePrefix()
        Dim novelConnString As String = Utils.GetNovelTradeFTConnString()
        'Dim cn As New SqlConnection(ReadFromConfig())
        Dim UseTestAddress As String = Utils.GetUseTestAddress()

        'Dim cn As New SqlConnection(Utils.GetSqlConnString())
        Dim cn As SqlConnection
        cn = New SqlConnection(Utils.GetSqlConnString())
        Using cn
            Try

                cn.Open()
                Dim cmd As New SqlCommand(sql, cn)
                Dim dr As SqlDataReader = cmd.ExecuteReader
                If dr.HasRows Then
                    'variables for use in the mt103 for rtgs
                    Dim sendersRef_f20 As String
                    Dim bankOpCode_f23B As String
                    Dim instructionCode_23E As String
                    Dim transactionType_26T As String
                    Dim valDateCurAmt_32A As String
                    Dim orderingCust_50K As String
                    Dim sendersCorrespondent_53A As String
                    Dim acctWithInstitution_57A As String
                    Dim detailOfCharges_71A As String
                    Dim benCust_59 As String = String.Empty
                    Dim senderToReceiverInfo_72 As String
                    Dim amt As String = ""
                    Dim beneAccount As String = ""
                    Dim beneName As String = ""
                    Dim remarks As String = ""
                    Dim rows As Integer = 5
                    Dim cols As Integer = 35
                    Dim utilObj As New Utils
                    Dim existingMt103 As String = String.Empty
                    Using dr
                        While dr.Read
                            sendersRef_f20 = dr("transactionid").ToString
                            existingMt103 = dr("mt103_text").ToString()

                            If Not String.IsNullOrEmpty(existingMt103) Then

                                Dim status As Boolean = WriteToSwiftInbox(existingMt103)

                                If status = True Then
                                    UpdateStatus(sendersRef_f20, "SwiftInProcess")
                                    'UpdateMt103(sendersRef_f20, mt103)
                                Else
                                    UpdateStatus(sendersRef_f20, "SwiftFailed")
                                End If

                                ReadFromSwiftOutbox()

                                Console.WriteLine(existingMt103)
                            Else
                                bankOpCode_f23B = "CRED"
                                instructionCode_23E = "SDVA"
                                transactionType_26T = "001"
                                amt = CDec(dr("amount")).ToString()
                                If amt.Contains(".") Then
                                    amt = amt.Replace(".", ",")
                                End If
                                'TODO: Uncomment the line below
                                valDateCurAmt_32A = CDate(dr("date")).ToString("yyMMdd") & "NGN" & amt
                                'valDateCurAmt_32A = "140101" & "NGN" & amt
                                beneAccount = dr("Beneficiary_account").ToString()
                                beneName = dr("Beneficiary").ToString
                                orderingCust_50K = "/" & utilObj.GetNuban(utilObj.RemoveSepFromAccountNumber(dr("Customer_account").ToString, "-", "")) & vbCrLf & FormatSwift(SanitizeForSwift(dr("Customer_name").ToString), 4, 35)
                                sendersCorrespondent_53A = Utils.GetSendersCorrespondent()
                                Dim retval() As String = GetCorrespondentBankBIC(dr("transactionid").ToString)
                                Dim nuban As String = retval(1)
                                Dim bic As String = retval(0)
                                acctWithInstitution_57A = "/" & nuban & vbCrLf & bic
                                If Not String.IsNullOrEmpty(beneAccount) Then
                                    benCust_59 = "/" & beneAccount
                                Else
                                    benCust_59 = "/" & "0"
                                End If

                                If Not String.IsNullOrEmpty(beneName) Then
                                    benCust_59 &= vbCrLf
                                    benCust_59 = benCust_59 & FormatSwift(SanitizeForSwift(beneName), 4, 35)
                                Else
                                    'must always have a name.
                                End If

                                detailOfCharges_71A = "OUR"
                                If Not IsDBNull(dr("Remarks")) AndAlso Not String.IsNullOrEmpty(dr("Remarks").ToString.Trim) Then
                                    remarks = dr("Remarks").ToString.Replace(sendersRef_f20, String.Empty)
                                Else
                                    remarks = ""
                                End If

                                If Not String.IsNullOrEmpty(remarks) Then
                                    remarks = "/CODTYPTR/001" & vbCrLf & FormatSwift(linePrefix & SanitizeForSwift(remarks), rows, cols, linePrefix, True)
                                Else
                                    remarks = "/CODTYPTR/001" & vbCrLf & linePrefix & "RTGS"
                                End If

                                senderToReceiverInfo_72 = remarks.ToUpper()
                                Dim newRef As String = String.Empty
                                Dim m As Message = New RtgsMt103Message
                                'If (sendersRef_f20.Length > 16) Then
                                '    newRef = GetNewRef(sendersRef_f20)
                                '    m.AddField(New Field20(newRef.ToUpper.Trim))
                                'Else
                                '    m.AddField(New Field20(sendersRef_f20.ToUpper.Trim))
                                'End If
                                m.AddField(New Field20(sendersRef_f20.ToUpper.Trim))

                                m.AddField(New Field23B(bankOpCode_f23B.ToUpper.Trim))
                                m.AddField(New Field23E(instructionCode_23E.ToUpper.Trim))
                                m.AddField(New Field26T(transactionType_26T.ToUpper.Trim))
                                m.AddField(New Field32A(valDateCurAmt_32A.ToUpper.Trim))
                                m.AddField(New Field50K(orderingCust_50K.ToUpper.Trim))
                                Dim f As Field = New Field53A(sendersCorrespondent_53A.ToUpper.Trim)
                                m.AddField(f)
                                m.AddField(New Field57A(acctWithInstitution_57A.ToUpper.Trim))
                                m.AddField(New Field59(benCust_59.ToUpper.Trim))
                                m.AddField(New Field71A(detailOfCharges_71A.ToUpper.Trim))
                                If Not String.IsNullOrEmpty(remarks.ToUpper.Trim) Then
                                    Dim f72 As Field = New Field72(senderToReceiverInfo_72.ToUpper.Trim)
                                    m.AddField(f72)
                                Else
                                    'if the remark is empty or null, then we do not include field 72.
                                End If

                                If Not String.IsNullOrEmpty(UseTestAddress) AndAlso UseTestAddress.Trim.ToLower().CompareTo("true") = 0 Then
                                    m.DestinationTerminal = "NAMENGLAXXXX"
                                Else
                                    m.DestinationTerminal = bic.PadRight(12, "X"c)
                                End If


                                m.SourceTerminal = "NAMENGLAAXXX" 'sterling bank


                                m.DbConnectionString = Utils.GetSqlConnString()
                                Try
                                    If m.Validate() Then
                                        Dim mt103 As String = m.Render()

                                        If mt103.Contains("{113:0070}") Then
                                            mt103 = mt103.Replace("{113:0070}", "{113:0030}")
                                        End If

                                        Dim status As Boolean = WriteToSwiftInbox(mt103)
                                        If status = True Then
                                            UpdateStatus(sendersRef_f20, "SwiftInProcess")
                                            UpdateMt103(sendersRef_f20, mt103)
                                        Else
                                            UpdateErrorText(sendersRef_f20, "Failed To Send To SWIFT server.")
                                            UpdateStatus(sendersRef_f20, "SwiftFailed")
                                        End If

                                        ''The method below has not been fully implemented it should read from the output folder on SWIFT and then send the telex or ACKed copy to the customer via email.
                                        'ReadFromSwiftOutbox()

                                        'Console.WriteLine(mt103)
                                    Else
                                        UpdateStatus(sendersRef_f20, "SwiftFailed")
                                        Dim erList As List(Of String) = m.ErrorMessageList
                                        Dim errorMsgBuilder = New StringBuilder
                                        errorMsgBuilder.AppendLine("SWIFT validation failed: ")
                                        For Each er As String In erList
                                            errorMsgBuilder.AppendLine(er)
                                        Next

                                        UpdateErrorText(sendersRef_f20, errorMsgBuilder.ToString())

                                    End If
                                Catch ex As Exception
                                    UpdateStatus(sendersRef_f20, "SwiftFailedValidation")
                                End Try
                            End If



                        End While

                    End Using

                End If
            Catch ex As Exception
                Console.WriteLine("Error: " & ex.Message)
                If ex.InnerException IsNot Nothing Then
                    Console.WriteLine(vbTab & " Additional Errors are: " & vbCrLf & vbTab & ex.InnerException.Message)
                End If
            End Try
        End Using
        'End While

    End Sub

    Private Function ReadFromConfig() As String
        Dim config As String = String.Empty
        config = File.ReadAllText("connection.txt")
        Return (config)
    End Function

    ''' <summary>
    ''' Formats the remark to conform to the number of rows and columns and the line prefix supplied.
    ''' It does not add the line prefix to the first line. That means that if you want the line prefix on the first line of the resulting
    ''' formatted string, then you should add it prior to calling this function.
    ''' This function truncates to exactly LESS_THAN_OR_EQUAL_TO rows as the number of rows that you will have in the formatted output.
    ''' </summary>
    ''' <param name="remarks">the string you want to format</param>
    ''' <param name="rows">the number of rows you'd ideally like to have.</param>
    ''' <param name="cols">then umber of columns to appear in the formatted output.</param>
    ''' <param name="linePrefix">the string to prepend to each line except the first line.</param>
    ''' <returns></returns>
    ''' <remarks>throws an invalid argument exception if the string to format is empty or null.</remarks>
    Private Function FormatSwift(ByVal remarks As String, ByVal rows As Integer, ByVal cols As Integer, Optional ByVal linePrefix As String = "", Optional ByVal isRemarks As Boolean = False) As String
        Dim formattedString As String = String.Empty
        Dim content As New StringBuilder
        Dim numLinesFormated As Integer = 0
        Dim lines() As String = Field.GetLinesFromContent(remarks)
        If lines IsNot Nothing Then
            If lines.Length > 1 Then 'combine into a single line

                For Each line As String In lines
                    content.Append(line)
                Next
                remarks = content.ToString()
            End If

            content.Clear()
            Dim len As Integer = remarks.Length
            Dim remnant As Integer = len
            Dim startIndex As Integer = 0
            Dim count As Integer = len
            Dim storedPrefix As String = linePrefix

            For i As Integer = 0 To rows - 1

                If remnant <= 0 Then
                    Exit For
                End If

                If Not String.IsNullOrEmpty(linePrefix) Then
                    If i <> 0 Then
                        content.Append(linePrefix)
                        If cols - linePrefix.Length <= remnant Then
                            count = cols - linePrefix.Length
                        Else
                            count = remnant
                        End If
                    Else
                        count = cols
                    End If
                Else
                    count = cols
                End If
                If startIndex + count <= remarks.Length - 1 Then
                    content.AppendLine(remarks.Substring(startIndex, count))
                Else
                    content.AppendLine(remarks.Substring(startIndex))
                End If
                numLinesFormated += 1
                If isRemarks Then
                    If numLinesFormated >= 1 Then
                        linePrefix = "//"
                    End If
                End If
                
                remnant -= count
                startIndex += count
            Next
        Else
            Throw New ArgumentException("The remark is invalid. Either empty string or null.")
        End If
        formattedString = content.ToString

        Return formattedString

    End Function
    
    Private Function BreakIntoLines(ByVal remarks As String, ByVal rows As Integer, ByVal cols As Integer, Optional ByVal linePrefix As String = "") As String
        Dim content As String = String.Empty
        Dim list As New ArrayList
        Dim strbuf As New StringBuilder
        Dim remnant As Integer = remarks.Length
        Dim rowAllowable As Integer = remnant Mod cols
        Dim rowCount As Integer = 0
        Dim storedPrefix As String = linePrefix
        Dim storedCols As Integer = cols
        While remnant > 0
            rowCount += 1
            If rowCount = 1 Then
                linePrefix = ""

            Else
                linePrefix = storedPrefix
            End If
            cols = cols - linePrefix.Length
            If cols < remnant Then
                list.Add(linePrefix & remarks.Substring(0, cols))
            Else
                list.Add(linePrefix & remarks.Substring(0))
            End If

            list.Add(vbCrLf)
            remnant = remnant - cols

            If remnant > 0 Then
                list.Add(linePrefix & remarks.Substring(cols, Math.Abs(remnant)))
                list.Add(vbCrLf)
            End If
            remnant = remnant - cols
        End While


        For Each l As String In list
            strbuf.Append(l)
        Next
        content = strbuf.ToString
        Return content
    End Function
    Private Function WriteWithWindows(ByVal filename As String, ByVal contents As String, ByVal remotePath As String, ByVal domainname As String, ByVal username As String, ByVal password As String) As String
        Dim status As String = String.Empty
        Dim mimic As Impersonator

        Try
            mimic = New Impersonator(username, domainname, password)
            Using mimic

                Dim fullpathname As String = remotePath & Path.DirectorySeparatorChar & filename
                File.WriteAllText(fullpathname, contents)
                status = "Upload File Complete"

            End Using
        Catch ex As Exception
            status = String.Empty
            Dim s As String = ex.Message
            File.AppendAllText("D:\\AppLogs\\easyrtgs\\log\\" & DateTime.Now.Year & "_" & DateTime.Now.Month & "_" & DateTime.Now.Day & ".txt", s)
        End Try
        Return status
    End Function
    Private Function WriteToSwiftInbox(ByVal mt103 As String, Optional ByVal useFtp As Boolean = False) As Boolean
        Dim status As Boolean = False
        ''Hack to include the priority code

        'If Not mt103.Contains("{113:0010}") Then
        '    mt103 = mt103.Replace("{3:{103:NGR}}", "{3:{103:NGR}{113:0010}}")
        'End If

        ''Lines below written to update the priority from 0010 to 0030 18-MAY-2017
        If Not mt103.Contains("{113:0010}") Then
            mt103 = mt103.Replace("{3:{103:NGR}}", "{3:{103:NGR}{113:0030}}")
        End If


        Try
            System.Threading.Thread.Sleep(500) 'wait for 1/2 second.
            Dim outFolder As String = "OutBox"
            Dim filen As String = DateTime.Now.Hour & DateTime.Now.Minute & DateTime.Now.Second & ".txt"
            Dim filename As String = outFolder & "\" & "rtgs_" & filen

            Dim retval As String = String.Empty
            Try
                Try
                    If useFtp Then
                        File.WriteAllText(filename, mt103)
                        retval = FtpUploadFile(Utils.GetFTPHost(), filename, Utils.GetInputFolder(), Utils.Username(), Utils.Password())
                    Else
                        Dim remotePath As String = Utils.GetSwiftWindowsRemotePath() ' 
                        Dim domain As String = Utils.GetSwiftWindowsDomain() '  
                        Dim username As String = Utils.GetSwiftWindowsUsername() ' 
                        Dim password As String = Utils.GetSwiftWindowsPassword() '  

                        retval = WriteWithWindows(Path.GetFileName(filen), mt103, remotePath, domain, username, password)

                    End If

                    If retval.Contains("Upload File Complete") Then
                        status = True
                    Else
                        status = False
                    End If
                Catch ex As Exception
                    status = False
                Finally
                    If File.Exists(filename) Then
                        File.Delete(filename)
                    End If
                End Try

            Catch ex As Exception
                status = False
            End Try

        Catch ex As Exception
            dolog(ex)
            status = False
        End Try

        Return status
    End Function
    'Private Function WriteToSwiftInbox(ByVal mt103 As String) As Boolean
    '    Dim status As Boolean = False

    '    'Console.WriteLine("Successfully Generated the mt103 for RTGS:")
    '    'If Not Directory.Exists("C:\Users\osagbemio\Desktop\RTGS") Then
    '    '    Directory.CreateDirectory("C:\Users\osagbemio\Desktop\RTGS")
    '    'End If
    '    Try
    '        System.Threading.Thread.Sleep(500) 'wait for 1/2 second.
    '        Dim outFolder As String = "OutBox"
    '        Dim filename As String = outFolder & "\" & "rtgs_" & DateTime.Now.Hour & DateTime.Now.Minute & DateTime.Now.Second & ".txt"
    '        File.WriteAllText(filename, mt103)
    '        Dim retval As String = String.Empty
    '        Try
    '            Try
    '                retval = FtpUploadFile(Utils.GetFTPHost(), filename, Utils.GetInputFolder(), Utils.Username(), Utils.Password())
    '                If retval.Contains("Upload File Complete") Then
    '                    status = True
    '                Else
    '                    status = False
    '                End If
    '            Catch ex As Exception
    '                status = False
    '            Finally
    '                If File.Exists(filename) Then
    '                    File.Delete(filename)
    '                End If
    '            End Try

    '        Catch ex As Exception
    '            status = False
    '        End Try

    '    Catch ex As Exception
    '        dolog(ex)
    '        status = False
    '    End Try

    '    Return status
    'End Function

    Private Sub dolog(ByVal ex As Exception)
        'TODO: implement
    End Sub

    Private Function UpdateStatus(ByVal transid As String, ByVal status As String) As Boolean
        Dim retstatus As Boolean = False
        Dim sql As String = "UPDATE transactions set Status = @status where transactionid = @transid"
        Dim cn As New SqlConnection(Utils.GetSqlConnString)
        Using cn
            Try
                cn.Open()
                Dim cmd As New SqlCommand(sql, cn)
                cmd.Parameters.AddWithValue("@status", status)
                cmd.Parameters.AddWithValue("@transid", transid)

                If cmd.ExecuteNonQuery() >= 0 Then
                    retstatus = True
                Else
                    retstatus = False
                End If

            Catch ex As Exception
                dolog(ex)
                retstatus = False
            End Try
        End Using
        Return retstatus
    End Function
    Private Function GetNewRef(ByVal ref As String) As String
        Dim refn As String = String.Empty
        If ref.Length > 16 Then
            refn = ref.Substring(0, 9)

            Dim cn As New SqlConnection("Data Source=10.0.0.230;Initial Catalog=AcctOpen;user id=sa;password=($terl1ng);Application Name=easyRTGSdbapp;")
            Using cn
                Try
                    cn.Open()
                    Dim cmd As New SqlCommand("spGetNewRef", cn)
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.Parameters.AddWithValue("@oldRef", ref)
                    cmd.Parameters.Add("@id", SqlDbType.BigInt).Direction = ParameterDirection.Output
                    cmd.ExecuteNonQuery()
                    Dim id As Long = Convert.ToInt64(cmd.Parameters("@id").Value)
                    refn = refn & id.ToString().PadLeft(7, "0")
                    cmd.Parameters.Clear()
                    cmd.CommandType = CommandType.Text
                    cmd.CommandText = "update tblRtgsRefSerial set newRef=@refn where id=@id"
                    cmd.Parameters.AddWithValue("@id", id)
                    cmd.Parameters.AddWithValue("@refn", refn)
                    cmd.ExecuteNonQuery()

                Catch ex As Exception

                End Try
            End Using
           




        Else
            Return ref
        End If
        Return refn
    End Function
    Private Function UpdateErrorText(ByVal transid As String, ByVal errormsg As String) As Boolean
        Dim retstatus As Boolean = False
        Dim sql As String = "UPDATE TransactTemp2_log set ErrorText = @msg where transactionid = @transid"
        Dim cn As New SqlConnection(Utils.GetSqlConnString)
        Using cn
            Try
                cn.Open()
                Dim cmd As New SqlCommand(sql, cn)
                cmd.Parameters.AddWithValue("@msg", errormsg)
                cmd.Parameters.AddWithValue("@transid", transid)

                If cmd.ExecuteNonQuery() >= 0 Then
                    retstatus = True
                Else
                    retstatus = False
                End If

            Catch ex As Exception
                dolog(ex)
                retstatus = False
            End Try
        End Using
        Return retstatus
    End Function
    '''The method below has not been fully implemented it should read from the output folder on SWIFT and then send the telex or ACKed copy to the customer via email.
    ''' 
    Private Function ReadFromSwiftOutbox() As String
        Dim status As String = "" 'SwiftComplete
        'SwiftFailed

        Dim host As String = Utils.GetFTPHost
        Dim outfolder As String = Utils.GetTelexFolder()
        Dim failedFolder As String = Utils.GetFailedFolder()
        Dim user As String = Utils.Username()
        Dim pwd As String = Utils.Password
        Dim fodlerList() As String = Nothing


        Using ftpService = New ftpService.Service
            ftpService.Timeout *= 2
            Try
                fodlerList = ftpService.FtpDownloadFileList(host, outfolder, user, pwd)
            Catch ex As Exception
                status = "SwiftFailed"
                dolog(ex)
                Return status
            End Try

        End Using

        If fodlerList IsNot Nothing AndAlso fodlerList.Length > 0 Then
            For Each File In fodlerList
                If Not String.IsNullOrEmpty(File) Then
                    Using ftpService = New ftpService.Service
                        Try
                            Dim resp As String = ftpService.FtpDownloadFile("C:\Users\osagbemi\Desktop\In_Swift_Rtgs\path.txt", File, host, user, pwd, outfolder)

                        Catch ex As Exception
                            dolog(ex)

                        End Try
                    End Using
                End If
            Next
        End If
        Return status
    End Function

    Private Function GetCorrespondentBankBIC(ByVal transactionid As String) As String()
        Dim retvalArr() As String = New String(1) {}
        Dim corrBankBic As String = String.Empty '"FBNINGLA"
        Dim cn As New SqlConnection(Utils.GetSqlConnString())
        Dim benbank As String = String.Empty
        Dim sql1 As String = "select Beneficiary_Bank from transactions where TransactionID='" & transactionid & "'"

        Try
            cn.Open()
            Dim cmd As New SqlCommand(sql1, cn)
            Dim dr As SqlDataReader = cmd.ExecuteReader()

            If dr IsNot Nothing AndAlso dr.HasRows Then
                While dr.Read
                    benbank = dr("Beneficiary_Bank").ToString()

                    If benbank.Contains(":") Then
                        benbank = benbank.Split(":")(1).Trim
                    Else
                        'only old data will not contain : Therefore, let us return a default bank code
                        benbank = "070"
                    End If

                End While
            End If
        Catch ex As Exception
            Throw ex
        Finally
            cn.Close()
        End Try

        Dim Sql As String = "SELECT bic, nuban from tbl_banks where bank_code='" & benbank & "'"
        Using cn
            Try
                cn.Open()
                Dim cmd As New SqlCommand(Sql, cn)
                Dim dr As SqlDataReader = cmd.ExecuteReader()
                If dr IsNot Nothing AndAlso dr.HasRows Then
                    While dr.Read
                        retvalArr(0) = dr("bic").ToString().Trim
                        retvalArr(1) = dr("nuban").ToString().Trim

                    End While
                End If
            Catch ex As Exception
                Throw ex
            Finally

            End Try
        End Using
        Return retvalArr
    End Function

    Private Function UpdateMt103(ByVal sendersRef_f20 As String, ByVal mt103 As String) As Boolean
        Dim retstatus As Boolean = False
        Dim sql As String = "UPDATE transactions set mt103_text = @msg where transactionid = @transid"
        Dim cn As New SqlConnection(Utils.GetSqlConnString())
        Using cn
            Try
                cn.Open()
                Dim cmd As New SqlCommand(sql, cn)
                cmd.Parameters.AddWithValue("@msg", mt103)
                cmd.Parameters.AddWithValue("@transid", sendersRef_f20)

                If cmd.ExecuteNonQuery() >= 0 Then
                    retstatus = True
                Else
                    retstatus = False
                End If

            Catch ex As Exception
                dolog(ex)
                retstatus = False
            End Try
        End Using
        Return retstatus
    End Function

    Private Function SanitizeForSwift(ByVal content As String) As String
        Dim sanitize As String = String.Empty

        If Not Field.ContainsInvalidCharacter(content) Then
            sanitize = content
        Else
            sanitize = CleanInvalidCharacters(content)
        End If
        Return sanitize
    End Function

    Private Function CleanInvalidCharacters(ByVal content As String) As String
        Dim invalidChars() As Char = Field.invalidCharacterArray
        Dim cleaned As String = String.Empty
        Dim strbuf As New StringBuilder
        For Each c As Char In invalidChars
            content = content.Replace("&amp;", "&").Replace("&", "AND").Replace(c, "")
        Next
        cleaned = content
        Return cleaned
    End Function



    ''' <summary>
    ''' Uploads a file for upload to the SWIFT Server via FTP.
    ''' </summary>
    ''' <param name="ftpHost">The IP address of the FTP host.</param>
    ''' <param name="filenameToUpload">the filename of the file to be uploaded either full filepath or just the file name only.</param>
    ''' <param name="uploadFolderName">The folder to which you want to upload.</param>
    ''' <returns>a sttring that identifies whether the upload succeeded.</returns>
    ''' <remarks></remarks>
    Public Function FtpUploadFile(ByVal ftpHost As String, ByVal filenameToUpload As String, ByVal uploadFolderName As String, ByVal usr As String, ByVal pwd As String) As String
        'Dim fi As FileInfo
        Dim status As Boolean = False
        Dim fne As String
        fne = "ftp://" & ftpHost & "/" & uploadFolderName & "/" & Path.GetFileName(filenameToUpload)

        Dim statusString As String = String.Empty
        '' Get the object used to communicate with the server.
        Dim request As FtpWebRequest = DirectCast(WebRequest.Create(fne), FtpWebRequest)
        request.Method = WebRequestMethods.Ftp.UploadFile

        'Dim wp As WebProxy
        'wp = New WebProxy("10.0.0.120", 80)
        request.Proxy = Nothing 'wp
        request.Credentials = New NetworkCredential(usr, pwd) ' New NetworkCredential("domft", "($terl1ng)")
        request.KeepAlive = True


        '' Copy the contents of the file to the request stream.
        Dim sourceStream As New StreamReader(filenameToUpload)
        Dim fileContents As Byte() = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd())
        sourceStream.Close()
        request.ContentLength = fileContents.Length

        Dim requestStream As System.IO.Stream = request.GetRequestStream()
        requestStream.Write(fileContents, 0, fileContents.Length)
        requestStream.Close()

        Dim response As FtpWebResponse = DirectCast(request.GetResponse(), FtpWebResponse)

        statusString = String.Format("Upload File Complete, status {0}", response.StatusDescription)

        response.Close()

        Return statusString
    End Function
End Module

