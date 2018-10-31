Imports System.IO
Imports SwiftLib
Imports System.Configuration
Imports System.Data.SqlClient
Imports Oracle.ManagedDataAccess.Client

Module Module1
    Sub Main()
        Dim rmSrcPath As String = ConfigurationManager.AppSettings("srcPath").ToString() '"\\192.168.5.8\inward"
        Dim rmDestPath As String = ConfigurationManager.AppSettings("destPath").ToString() '"\\192.168.5.8\amlworkfolder"
        Dim domain As String = ConfigurationManager.AppSettings("domain").ToString() '"APPS"
        Dim username As String = ConfigurationManager.AppSettings("username").ToString() '"AmlDataApp"
        Dim password As String = File.ReadAllText("file_password.txt") ' "SBPSw\\AMLdmin0_&#"
        Dim LOG_FILE_NAME As String = ConfigurationManager.AppSettings("App_Log_Path").ToString() '"E:\AppLogs\SAS_SWIFT_ALLIANCE_LOGS"
        Dim mimic As Impersonator = New Impersonator(username, domain, password)
        Using mimic
            ProcessWithWindowsCredentials(rmSrcPath, rmDestPath, domain, username, password)

            DeleteFilesInFolder(rmDestPath)

        End Using
        '//Update the FTreference from T24
        UpdateFundsTransferReference()

        UpdateLastExtractionDate()
    End Sub
    Private Function GetLogFileNameForToday() As String
        Dim filename As String = String.Empty
        Dim ext As String = ".txt"
        Dim year As String = DateTime.Now.Year
        Dim month As String = DateTime.Now.Month
        Dim day As String = DateTime.Now.Day

        filename = year & "_" & month & "_" & day & ext
        Return filename
    End Function

    Public Sub LogException(ByVal ex As Exception)
        Dim LOG_FILE_NAME As String = ConfigurationManager.AppSettings("App_Log_Path").ToString()
        Try
            If ex IsNot Nothing Then
                Dim targetSite As String = "" 'ex.TargetSite.Name
                Dim message As String = "" 'ex.Message
                Dim stackTrace As String = "" ' ex.StackTrace
                Dim source As String = "" 'ex.Source
                Dim trace As String = "" 'ex.StackTrace

                Try
                    message = ex.Message
                    targetSite = ex.TargetSite.Name
                    stackTrace = ex.StackTrace
                    source = ex.Source
                    trace = ex.StackTrace
                Catch exc As Exception
                    LogException(exc)
                End Try
                Dim innerException As Exception = ex.InnerException

                File.AppendAllText(LOG_FILE_NAME & Path.DirectorySeparatorChar & GetLogFileNameForToday(), _
                                   "[Date]>> " & Date.Now & vbCrLf & _
                                   "[Method Name]>> " & targetSite & vbCrLf & _
                                   "[Message]>> " & message & vbCrLf & _
                                   "[Source]>> " & source & vbCrLf & _
                                   "[Stack Trace]>> " & trace & vbCrLf)

                If innerException IsNot Nothing Then
                    LogException(innerException)
                Else
                    File.AppendAllText(LOG_FILE_NAME & Path.DirectorySeparatorChar & GetLogFileNameForToday(), vbCrLf & vbCrLf)
                End If
            Else
                File.AppendAllText(LOG_FILE_NAME & Path.DirectorySeparatorChar & GetLogFileNameForToday(), _
                                   "[Date]>> " & Date.Today & vbCrLf & _
                                   "[Method Name]>> " & "Exception is NOTHING" & vbCrLf & _
                                   "[Message]>> " & "Exception is NOTHING" & vbCrLf & _
                                   "[Source]>> " & "Exception is NOTHING" & vbCrLf & _
                                   "[Stack Trace]>> " & "Exception is NOTHING" & vbCrLf)
            End If
        Catch exInner As Exception
            LogException(exInner)
        End Try

    End Sub
    Private Sub CopyFileToDestination(ByVal srcFilename As String, ByVal destFilename As String)
        Dim isOverwriteDestination As Boolean = True
        Try
            File.Copy(srcFilename, destFilename, isOverwriteDestination)
        Catch ex As Exception
            LogException(ex)
        End Try

    End Sub

    Private Function GetMultiMessageDelimiterString() As String
        Return "$"
    End Function
    Private Function GetArrayOfSplitMessagesFromContent(ByVal content As String, ByVal delimiter As String) As String()
        Dim messages As String() = Nothing
        If content.Contains(delimiter) Then
            '  'split the content based on the content based on the string
            messages = content.Split(delimiter.ToCharArray())
        Else
            messages = New String() {content}
        End If
        Return messages
    End Function
    Private Function CreateNewFullPathFileName(ByVal parentFilename As String, ByVal counter As Integer) As String
        Dim filename As String = String.Empty
        'get the filename of the parent without extension
        Dim pfn As String = Path.GetFileNameWithoutExtension(parentFilename)

        Dim pext As String = Path.GetExtension(parentFilename)

        'get the parent file's srcPath without the filename
        Dim pfolder As String = Path.GetDirectoryName(parentFilename)

        'get the directory separator character
        Dim dirSep As String = Path.DirectorySeparatorChar.ToString()

        filename = String.Format("{0}{1}{2}_{3}{4}", pfolder, dirSep, pfn, counter, pext)


        Return filename
    End Function
    Private Function CreateNewFilesForTheseMessages(ByVal messages As String(), ByVal parentFilename As String) As Integer
        Dim numOfCreatedFiles As Integer = 0
        'TODO: Implement
        If messages IsNot Nothing Then
            If messages.Length <= 1 Then
                numOfCreatedFiles = 0
            Else
                Dim i As Integer = 1
                For counter As Integer = 0 To messages.Length - 1
                    Dim newfilename As String = String.Empty
                    Dim newContent As String = String.Empty

                    newfilename = CreateNewFullPathFileName(parentFilename, counter)

                    newContent = messages(counter)
                    Try
                        File.WriteAllText(newfilename, newContent)
                        numOfCreatedFiles += 1
                        'TODO: Log this.
                    Catch ex As Exception
                        LogException(ex)
                    End Try

                Next
            End If
        End If
        Return numOfCreatedFiles
    End Function

    Private Function GetFilesNew(ByVal srcPath As String, ByVal destPath As String, ByVal extraction_date As DateTime, Optional ByVal isRemote As Boolean = False) As String()
        Dim files As String() = Nothing
        Dim fileList As List(Of String)
        Dim today As DateTime = DateTime.Now
        Try


            'DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            'DirectoryInfo[] dirs = dir.GetDirectories();
            Dim domain As String = "APPS"

            Dim username As String = "AmlDataApp"

            Dim password As String = "SBPSw\AMLdmin0_&#"

            If Directory.Exists(srcPath) Then

                'fileList = Directory.EnumerateFiles(srcPath, "*.*", System.IO.SearchOption.AllDirectories).Where(Function(path) File.GetCreationTime(path) >= extraction_date).ToList()

                fileList = Directory.EnumerateFiles(srcPath, "*.*", System.IO.SearchOption.AllDirectories).Where(Function(path) File.GetCreationTime(path) >= extraction_date).ToList()

                'fileList = Directory.EnumerateFiles(srcPath, "*.*", System.IO.SearchOption.AllDirectories).Where(Function(path) File.GetCreationTime(path) >= extraction_date).OrderByDescending(Function(p) File.GetCreationTime(p)) '.OrderByDescending(Function(p) p.CreationTime).ToList()


                Dim destPathFile As String = String.Empty
                'Read each file and see the files that contain multiple messages per files.
                For Each filename As String In fileList
                    'Copy the file to the destPath
                    destPathFile = Path.Combine(destPath, Path.GetFileNameWithoutExtension(filename) + "_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".txt")
                    CopyFileToDestination(filename, destPathFile)
                    Dim content As String = File.ReadAllText(destPathFile)
                    Dim delimiter As String = GetMultiMessageDelimiterString()
                    Dim messages As String() = Nothing
                    messages = GetArrayOfSplitMessagesFromContent(content, delimiter)
                    'check if there were messages returned
                    If messages IsNot Nothing Then
                        If messages.Length > 1 Then
                            Dim parentFilename As String = destPathFile
                            Dim numberOfNewFilesCreated As Integer = CreateNewFilesForTheseMessages(messages, parentFilename)
                            'the number of new files created should equal the length of the array of split messages called "messages"
                            If numberOfNewFilesCreated = messages.Length Then
                                'we are good. We can delete the parent file which was the one containing multiple messages per file
                                Try
                                    File.Delete(parentFilename)
                                    'TODO: Log this.
                                Catch ex As Exception
                                    LogException(ex)
                                End Try
                                'TODO: something is wrong? log this.
                            Else
                                'There is just one message in this file. Continue the loop
                            End If
                        ElseIf messages.Length = 1 Then
                            Continue For

                        End If
                    End If
                Next
            End If
            'if (destDir.Exists)
            '{
            If Directory.Exists(destPath) Then
                files = Directory.GetFiles(destPath)
            End If



        Catch ex As Exception
            LogException(ex)
        End Try

        Return files
    End Function
    'Private Function GetFieldContent(ByVal msg As Message, ByVal tagName As String) As String
    '    Dim content As String = String.Empty
    '    Dim fieldList As List(Of Field) = Nothing
    '    fieldList = msg(tagName)
    '    If fieldList.Count > 0 Then
    '        Dim f As Field = fieldList(0)
    '        If f IsNot Nothing Then
    '            If Not String.IsNullOrEmpty(f.Content) Then
    '                Return f.Content

    '            End If
    '        End If
    '    End If
    '    Return content

    'End Function

    Private Function ConvertToDateTime(ByVal valdatestr As String) As DateTime
        Dim dt As DateTime = DateTime.Now
        If Not String.IsNullOrEmpty(valdatestr) Then
            If valdatestr.Length = 6 Then
                Dim year As Integer = Convert.ToInt32(DateTime.Now.Year.ToString().Substring(0, 2) + valdatestr.Substring(0, 2))
                Dim month As Integer = Convert.ToInt32(valdatestr.Substring(2, 2))
                Dim day As Integer = Convert.ToInt32(valdatestr.Substring(4, 2))
                dt = New DateTime(year, month, day)
            End If
        End If
        Return dt
    End Function
    Private Function ProcessWithWindowsCredentials(ByVal remoteSourcePath As String, ByVal remoteDestPath As String, ByVal domainname As String, ByVal username As String, ByVal password As String) As String
        Dim status As String = String.Empty
        Dim mimic As Impersonator
        Dim TradeId As String = ""
        Dim reference As String
        Dim currency As String
        Dim messageType As String = ""
        Dim correspondent As String
        Dim identifier As String = ""
        Dim valuedate As DateTime
        Dim CurAmt As Decimal = 0D
        Dim format As String
        Dim statuss As String
        Dim NetworkStatus As String = ""
        Dim CreationDate As DateTime = DateTime.Now
        Dim fn As String
        Try
            'mimic = New Impersonator(username, domainname, password)
            'Using mimic

            Dim fullpathname As String = remoteSourcePath

            'If Not Directory.Exists(remoteSourcePath) Then

            '    Exit Function
            'End If

            Dim isRemote As Boolean = True
            Dim lastExtraction_date As DateTime = GetLastExtractionDate()
            Dim files() As String = GetFilesNew(remoteSourcePath, remoteDestPath, lastExtraction_date, isRemote)


            'If Directory.Exists(srcPath) Then

            Dim fc As String() = Nothing
            Dim filecontent As String = String.Empty
            Dim msg As Message = Nothing
            If files Is Nothing Then
                Exit Function
            End If

            For Each filepath As String In files

                fn = filepath
                fc = File.ReadAllLines(filepath)
                'File is not empty
                If fc IsNot Nothing Then
                    'File is not empty
                    If fc.Length > 0 Then
                        filecontent = File.ReadAllText(filepath)
                        If fc(0).ToUpper().Trim().Contains("{2:I103") OrElse fc(0).ToUpper().Trim().Contains("{2:O103") Then
                            messageType = "103"
                            msg = New Mt103Message()
                        ElseIf fc(0).ToUpper().Trim().Contains("{2:I202") OrElse fc(0).ToUpper().Trim().Contains("{2:O202") Then
                            messageType = "202"

                            'correspondent=mt202.des
                            msg = New Mt202Message()
                            'ElseIf fc(0).ToUpper().Trim().Contains("{2:I700") OrElse fc(0).ToUpper().Trim().Contains("{2:O700") Then
                            '    messageType = "700"

                            '    msg = New Mt700Message()
                            'ElseIf fc(0).ToUpper().Trim().Contains("{2:I950") OrElse fc(0).ToUpper().Trim().Contains("{2:O950") Then
                            '    messageType = "202"

                            '    msg = New Mt950Message()
                        Else
                            Continue For
                        End If
                        Try
                            msg.ReadFromString(filecontent)
                        Catch ex As Exception
                            If ex.Message.Contains("SwiftLib.Field59F") Then
                                File.AppendAllText("E:\AppLogs\SAS_SWIFT_ALLIANCE_LOGS\Field59F_Sample.txt", File.ReadAllText(filepath))
                            ElseIf ex.Message.Contains("Could not read tags from the file specified. The tag collection is empty") Then
                                File.AppendAllText("E:\AppLogs\SAS_SWIFT_ALLIANCE_LOGS\Invalid_Message_Sample.txt", File.ReadAllText(filepath))
                            Else
                                Throw ex
                            End If

                        End Try

                        correspondent = msg.DestinationTerminal
                        reference = String.Empty
                        'Count how many fields you have in the Message object
                        If msg.Fields.Count > 0 Then

                            Try
                                Dim tagName As String = "20"
                                reference = GetFieldContent(msg, tagName)
                                tagName = "72"
                                TradeId = GetFieldContent(msg, tagName)

                                tagName = "32A"
                                Dim valDateCurAmt As String = GetFieldContent(msg, tagName)
                                Dim valdatestr As String = valDateCurAmt.Substring(0, 6)
                                Dim amtstring As String = valDateCurAmt.Substring(9).Replace(",", ".")
                                CurAmt = Convert.ToDecimal(amtstring)
                                currency = valDateCurAmt.Substring(6, 3)
                                valuedate = ConvertToDateTime(valdatestr)
                                CreationDate = valuedate

                                Dim SenderCustomerName As String = GetSenderCustomerName(msg)
                                Dim SenderAccount As String = GetSenderAccount(msg)
                                Dim SendingBankCode As String = GetSenderBankCode(msg, fc(0))
                                Dim ReceivingBankCode As String = GetReceiverBankCode(msg, fc(0))
                                Dim SenderBank As String = GetSenderBankName(msg, fc(0))
                                Dim ReceiverCustomerName As String = GetReceiverName(msg)
                                Dim ReceiverAccount As String = GetReceiverAccount(msg)
                                Dim ReceiverBank As String = GetReceiverBankName(msg, fc(0))

                                Dim cnstr As String = ConfigurationManager.ConnectionStrings("SasConn").ConnectionString
                                'Dim cnstr As String = ConfigurationManager.ConnectionStrings("BaselDB").ConnectionString
                                Dim cn As New SqlConnection(cnstr)
                                cn.Open()
                                Dim updateCount As Integer = 0
                                Dim cmd As New SqlCommand("spd_insertRecord", cn)
                                Using cn
                                    cmd.CommandType = CommandType.StoredProcedure
                                    cmd.Parameters.AddWithValue("@Correspondent", correspondent)
                                    cmd.Parameters.AddWithValue("@Identifier", identifier)
                                    cmd.Parameters.AddWithValue("@Reference", reference)
                                    cmd.Parameters.AddWithValue("@ValueDate", valuedate)
                                    cmd.Parameters.AddWithValue("@currency", currency)
                                    cmd.Parameters.AddWithValue("@CurAmt", CurAmt)
                                    'set as zero by default. For now :-)
                                    cmd.Parameters.AddWithValue("@Format", messageType)
                                    cmd.Parameters.AddWithValue("@Status", status)
                                    cmd.Parameters.AddWithValue("@NetworkStatus", NetworkStatus)
                                    cmd.Parameters.AddWithValue("@CreationDate", CreationDate)
                                    'This should be the field 32A date portion.
                                    cmd.Parameters.AddWithValue("@TradeId", TradeId)
                                    cmd.Parameters.AddWithValue("@type", messageType)
                                    cmd.Parameters.AddWithValue("@filename", fn)
                                    cmd.Parameters.AddWithValue("@sendingBankName", SenderBank)
                                    cmd.Parameters.AddWithValue("@sendingAccount", SenderAccount)
                                    cmd.Parameters.AddWithValue("@sendingCustomerName", SenderCustomerName)
                                    cmd.Parameters.AddWithValue("@receivingBankName", ReceiverBank)
                                    cmd.Parameters.AddWithValue("@receivingAccount", ReceiverAccount)
                                    cmd.Parameters.AddWithValue("@receivingCustomerName", ReceiverCustomerName)
                                    cmd.Parameters.AddWithValue("@sendingBankCode", SendingBankCode)
                                    cmd.Parameters.AddWithValue("@receivingBankCode", ReceivingBankCode)
                                    cmd.Parameters.AddWithValue("@inwardBranchCode", DBNull.Value)
                                    Dim content As String = String.Empty

                                    Try
                                        content = msg.Render()
                                    Catch ex As Exception

                                    End Try

                                    cmd.Parameters.AddWithValue("@msgContent", content)

                                    '@inwardBranchCode
                                    If String.IsNullOrEmpty(SenderBank) OrElse String.IsNullOrEmpty(ReceiverBank) Then
                                        File.AppendAllText("E:\AppLogs\SAS_SWIFT_ALLIANCE_LOGS\Sample_msg.txt", msg.Render())
                                    End If
                                    ',@IO  as nvarchar(max)
                                    updateCount = cmd.ExecuteNonQuery()

                                    If updateCount > 0 Then
                                        Console.WriteLine("Success Reference {0} and Trade ID {1}", reference, TradeId)

                                    End If


                                End Using
                            Catch ex As Exception
                                LogException(ex)
                            End Try
                        End If
                    End If

                End If
            Next


            'End Using
        Catch ex As Exception
            status = String.Empty
            Dim s As String = ex.Message
            LogException(ex)
        End Try
        Return status
    End Function

    Private Function GetInwardSwiftDeliveryRecord(ByVal swiftReference As String) As DataRow
        Dim ftReference As String = String.Empty
        Dim r As DataRow = Nothing

        Dim oraConString As String = ConfigurationManager.ConnectionStrings("T24Conn").ConnectionString
        Dim oraCon As New OracleConnection(oraConString)
        'STAFFJ.V_F_DE_I_HEADER is a T24 table or view that contains delivery information.
        Dim sqlGetSwiftInfo As String = "select t24_inw_trans_ref, company_code  from STAFJ.V_F_DE_I_HEADER where transaction_ref= :transaction_ref"


        Using oraCon
            Try
                oraCon.Open()
                Dim oraCmd As OracleCommand = oraCon.CreateCommand()
                oraCmd.CommandText = sqlGetSwiftInfo
                oraCmd.Parameters.Add(":transaction_ref", swiftReference)
                Dim adap As New OracleDataAdapter(oraCmd)
                Dim ds As New DataSet()
                adap.Fill(ds)
                If ds.Tables.Count > 0 Then
                    If ds.Tables(0).Rows.Count = 1 Then
                        r = ds.Tables(0).Rows(0)
                    Else
                        Return Nothing
                    End If
                End If
            Catch ex As Exception
                LogException(ex)
            End Try
        End Using
        Return r
    End Function
    Private Sub UpdateFundsTransferReference()
        Dim oraConString As String = ConfigurationManager.ConnectionStrings("T24Conn").ConnectionString
        Dim oraCon As New OracleConnection(oraConString)
        'string sqlGetSwiftInfo = "select customer_number, company_code, to_address, from_address, t24_inw_trans_ref, transaction_ref from STAFJ.V_F_DE_I_HEADER where value_date >= :start and value_date <= :end"; //STAFFJ.V_F_DE_I_HEADER is a T24 table or view that contains delivery information.
        Dim sqlGetSwiftInfo As String = "select Reference FROM tbl_messagedetails where FtReference is null"
        'STAFFJ.V_F_DE_I_HEADER is a T24 table or view that contains delivery information.
        Dim cn As New SqlConnection(ConfigurationManager.ConnectionStrings("SasConn").ConnectionString)
        Dim ftReference As String = String.Empty
        Dim branchcode As String = String.Empty
        Dim swiftReference As String = String.Empty
        Using cn
            Dim cmd As New SqlCommand(sqlGetSwiftInfo, cn)
            Try
                cn.Open()
                Dim ds As New DataSet()
                Dim ada As New SqlDataAdapter(cmd)
                ada.Fill(ds)

                If ds.Tables.Count > 0 Then
                    If ds.Tables(0).Rows.Count > 0 Then
                        For Each r As DataRow In ds.Tables(0).Rows
                            swiftReference = r("Reference").ToString()
                            Dim row As DataRow = GetInwardSwiftDeliveryRecord(swiftReference)
                            If (row IsNot Nothing) Then
                                ftReference = row("t24_inw_trans_ref").ToString()
                                branchcode = row("company_code").ToString()
                                cmd.CommandText = "UPDATE tbl_messagedetails SET FtReference=@FtReference, inwardBranchCode=@inwardBranchCode WHERE Reference=@swiftRef"
                                cmd.Parameters.Clear()
                                cmd.Parameters.AddWithValue("@FtReference", ftReference)
                                cmd.Parameters.AddWithValue("@inwardBranchCode", branchcode)
                                cmd.Parameters.AddWithValue("@swiftRef", swiftReference)

                                cmd.ExecuteNonQuery()
                            End If
                            
                        Next

                    End If
                End If
                'to-do
            Catch sEx As SqlException
                'to-do
                LogException(sEx)
            Catch ex As Exception
                LogException(ex)
            End Try
        End Using


    End Sub
    Private Sub DeleteFilesInFolder(ByVal destPath As String)


        Dim filenames As String() = Directory.GetFiles(destPath)
        For i As Integer = 0 To filenames.Length - 1
            File.Delete(filenames(i))
        Next

    End Sub
    Private Function GetReceiverBankName(ByVal msg As Message, ByVal firstLine As String) As String
        Dim start As Integer = firstLine.IndexOf("}{1:")
        Dim receiverBankBic As String = firstLine.Substring(start + 7, 12)
        Dim bankName As String = GetBankNameFromBic(receiverBankBic)
        Return bankName

    End Function
    Private Function GetReceiverBankCode(ByVal msg As Message, ByVal firstLine As String) As String
        Dim start As Integer = firstLine.IndexOf("}{1:")
        Dim receiverBankBic As String = firstLine.Substring(start + 7, 12)

        Return receiverBankBic

    End Function
    Private Function GetBankNameFromBic(ByVal receiverBankBic As String) As String
        Dim bnkname As String = String.Empty
        Dim cn As New SqlConnection(ConfigurationManager.ConnectionStrings("NovelTrade_Live").ConnectionString)
        Dim sql As String = String.Empty
        If receiverBankBic.Length >= 8 Then
            sql = String.Format("select top 1 FI_NAME from tblSwiftBicCodes where bic_code like '{0}%'", receiverBankBic.Substring(0, 8))
        Else
            sql = String.Format("select top 1 FI_NAME from tblSwiftBicCodes where bic_code like '{0}%'", receiverBankBic)
        End If

        Dim cmd As New SqlCommand(sql, cn)
        Using cn
            Try
                cn.Open()
                Dim r As SqlDataReader = cmd.ExecuteReader()
                If r.HasRows Then
                    Using r
                        r.Read()
                        bnkname = r(0).ToString()
                    End Using
                End If
            Catch ex As Exception
                bnkname = ""
                LogException(ex)
            End Try
        End Using
        Return bnkname
    End Function

    Private Function GetReceiverAccount(ByVal msg As Message) As String
        Dim val As String = String.Empty
        Dim tagName As String = "59"
        If msg.MessageNumber = "103" Then
            tagName = "59"
        ElseIf msg.MessageNumber = "202" Then
            tagName = "58A"
            val = GetFieldContent(msg, tagName)
            Dim content As String() = Field.GetLinesFromContent(val)
            If content.Length > 1 Then
                If content(0).StartsWith("/") Then
                    Try
                        val = content(0).Substring(3)
                        'start from index 3 till the end.
                        Return val
                    Catch ex As Exception
                        LogException(ex)
                    End Try
                End If
                Return val

            End If
        End If
        val = GetFieldContent(msg, tagName)
        If Not String.IsNullOrEmpty(val) Then
            Dim content As String() = Field.GetLinesFromContent(val)
            'should be two lines.
            If content.Length > 1 Then
                val = content(0)
                If val.StartsWith("/") Then
                    'remove the leading slash to get at the NUBAN account.
                    val = val.Substring(1)
                End If
                Return val
            End If
        End If
        Return val
    End Function

   
    Private Function GetReceiverName(ByVal msg As Message) As String

        Dim val As String = String.Empty
        Dim tagNameArray As String() = Nothing
        Dim tagName As String = "59"
        Dim msgContent As String = msg.Render()
        If msg.MessageNumber = "103" Then

            If msgContent.Contains(":59:") Then
                tagName = "59"
            ElseIf msgContent.Contains(":59A:") Then
                tagName = "59A"
            ElseIf msgContent.Contains(":59F:") Then
                tagName = "59F"
            End If

        ElseIf msg.MessageNumber = "202" Then
            tagName = "58A"
            val = GetFieldContent(msg, tagName)
            Dim content As String() = Field.GetLinesFromContent(val)
            If content.Length > 1 Then

                val = GetBankNameFromBic(content(1))

                Return val

            End If
        End If
        val = GetFieldContent(msg, tagName)
        If Not String.IsNullOrEmpty(val) Then
            Dim content As String() = Field.GetLinesFromContent(val)
            'should be two lines.
            If content.Length > 1 Then
                val = content(1)
                'Check if the it starts with "1/"
                If val.StartsWith("1/") Then
                    val = val.Substring(2)
                End If
                Return val
            End If
        End If
        Return val
    End Function
    Private Function GetSenderBankName(ByVal msg As Message, ByVal firstLine As String) As String

        'int start = firstLine.IndexOf("}{1:");
        'string receiverBankBic = firstLine.Substring(start + 7, 12);
        'string bankName = GetBankNameFromBic(receiverBankBic);
        'return bankName;


        Dim start As Integer = firstLine.IndexOf("{2:")
        Dim receiverBankBic As String = firstLine.Substring(start + 17, 12)
        Dim bankName As String = GetBankNameFromBic(receiverBankBic)
        Return bankName
    End Function
    Private Function GetSenderBankCode(ByVal msg As Message, ByVal firstLine As String) As String

        'int start = firstLine.IndexOf("}{1:");
        'string receiverBankBic = firstLine.Substring(start + 7, 12);
        'string bankName = GetBankNameFromBic(receiverBankBic);
        'return bankName;


        Dim start As Integer = firstLine.IndexOf("{2:")
        Dim receiverBankBic As String = firstLine.Substring(start + 17, 12)
        'string bankName = GetBankNameFromBic(receiverBankBic);
        Return receiverBankBic
    End Function
    'Private Function GetSenderAccount(ByVal msg As Message) As String
    '    Dim val As String = String.Empty
    '    Dim tagName As String = "50K"
    '    If msg.MessageNumber = "103" Then
    '        tagName = "50K"
    '    ElseIf msg.MessageNumber = "202" Then
    '        tagName = "53A"
    '        val = GetFieldContent(msg, tagName)
    '        Dim content As String() = Field.GetLinesFromContent(val)
    '        If content.Length > 1 Then
    '            If content(0).StartsWith("/") Then
    '                Try
    '                    val = content(0).Substring(3)
    '                    'start from index 3 till the end.
    '                    Return val
    '                Catch ex As Exception
    '                    LogException(ex)
    '                End Try
    '            End If
    '            Return val

    '        End If
    '    End If
    '    val = GetFieldContent(msg, tagName)
    '    If Not String.IsNullOrEmpty(val) Then
    '        Dim content As String() = Field.GetLinesFromContent(val)
    '        'should be two lines.
    '        If content.Length > 1 Then
    '            val = content(0)
    '            If val.StartsWith("/") Then
    '                'remove the leading slash to get at the NUBAN account.
    '                val = val.Substring(1)
    '            End If
    '            Return val
    '        End If
    '    End If
    '    Return val
    'End Function
    Private Function GetSenderAccount(ByVal msg As Message) As String
        Dim val As String = String.Empty
        Dim tagName As String = "50K"
        'If msg.MessageNumber = "103" Then
        '    tagName = "50K"
        Dim msgContent As String = msg.Render()
        If msg.MessageNumber = "103" Then

            If msgContent.Contains(":50:") Then
                tagName = "50"
            ElseIf msgContent.Contains(":50A:") Then
                tagName = "50A"
            ElseIf msgContent.Contains(":50F:") Then
                tagName = "50F"
            ElseIf msgContent.Contains(":50K:") Then
                tagName = "50K"
            End If
        ElseIf msg.MessageNumber = "202" Then
            tagName = "53A"
            val = GetFieldContent(msg, tagName)
            Dim content As String() = Field.GetLinesFromContent(val)
            If content.Length > 1 Then
                If content(0).StartsWith("/") Then
                    Try
                        val = content(0).Substring(3)
                        'start from index 3 till the end.
                        Return val
                    Catch ex As Exception
                        LogException(ex)
                    End Try
                End If
                Return val

            End If
        End If
        val = GetFieldContent(msg, tagName)
        If Not String.IsNullOrEmpty(val) Then
            Dim content As String() = Field.GetLinesFromContent(val)
            'should be two lines.
            If content.Length > 1 Then
                val = content(0)
                If val.StartsWith("/") Then
                    'remove the leading slash to get at the NUBAN account.
                    val = val.Substring(1)
                End If
                Return val
            End If
        End If
        Return val
    End Function
    'Private Function GetSenderCustomerName(ByVal msg As Message) As String
    '    Dim val As String = String.Empty
    '    Dim tagName As String = "50K"
    '    If msg.MessageNumber = "103" Then
    '        tagName = "50K"
    '    ElseIf msg.MessageNumber = "202" Then
    '        tagName = "53A"
    '        val = GetFieldContent(msg, tagName)
    '        Dim content As String() = Field.GetLinesFromContent(val)
    '        If content.Length > 1 Then
    '            val = GetBankNameFromBic(content(1))
    '            Return val

    '        End If
    '    End If
    '    val = GetFieldContent(msg, tagName)
    '    If Not String.IsNullOrEmpty(val) Then
    '        Dim content As String() = Field.GetLinesFromContent(val)
    '        'should be two lines.
    '        If content.Length > 1 Then
    '            val = content(1)
    '            Return val
    '        End If
    '    End If
    '    Return val
    'End Function
    Private Function GetSenderCustomerName(ByVal msg As Message) As String
        Dim val As String = String.Empty
        Dim tagName As String = "50K"
        'If msg.MessageNumber = "103" Then
        '    tagName = "50K"
        Dim msgContent As String = msg.Render()
        If msg.MessageNumber = "103" Then

            If msgContent.Contains(":50:") Then
                tagName = "50"
            ElseIf msgContent.Contains(":50A:") Then
                tagName = "50A"
            ElseIf msgContent.Contains(":50F:") Then
                tagName = "50F"
            ElseIf msgContent.Contains(":50K:") Then
                tagName = "50K"
            End If
        ElseIf msg.MessageNumber = "202" Then
            tagName = "53A"
            val = GetFieldContent(msg, tagName)
            Dim content As String() = Field.GetLinesFromContent(val)
            If content.Length > 1 Then
                val = GetBankNameFromBic(content(1))
                Return val

            End If
        End If
        val = GetFieldContent(msg, tagName)
        If Not String.IsNullOrEmpty(val) Then
            Dim content As String() = Field.GetLinesFromContent(val)
            'should be two lines.
            If content.Length > 1 Then
                val = content(1)
                Return val
            End If
        End If
        Return val
    End Function
    ''' <summary>
    ''' Returns the field content without bias as to the format of the content.
    ''' </summary>
    ''' <param name="msg"></param>
    ''' <param name="tagName"></param>
    ''' <returns></returns>
    Private Function GetFieldContent(ByVal msg As Message, ByVal tagName As String) As String
        Dim content As String = String.Empty
        Dim fieldList As List(Of Field) = Nothing
        fieldList = msg(tagName)
        If fieldList.Count > 0 Then
            Dim f As Field = fieldList(0)
            If f IsNot Nothing Then
                If Not String.IsNullOrEmpty(f.Content) Then
                    Return f.Content

                End If
            End If
        End If
        Return content

    End Function

    Private Function GetLastExtractionDate() As Date
        Dim extdate As Date
        'TODO: 
        Dim sqlConn As New SqlConnection(ConfigurationManager.ConnectionStrings("SasConn").ConnectionString)
        Using sqlConn
            Try
                sqlConn.Open()
                Dim sql As String = "select top 1 last_extraction_date from tblSwiftAllianceSettings WHERE id=1"
                Dim sqlCmd As New SqlCommand(sql, sqlConn)
                Return CType(sqlCmd.ExecuteScalar(), Date)
            Catch ex As Exception
                LogException(ex)
            End Try
        End Using
        Return extdate
    End Function

    Private Sub UpdateLastExtractionDate()
        Dim cn As New SqlConnection(ConfigurationManager.ConnectionStrings("SasConn").ConnectionString)
        Using cn
            Try
                cn.Open()
                Dim cmd As New SqlCommand("update tblSwiftAllianceSettings set last_extraction_date=DATEADD(DAY, 1, last_extraction_date) WHERE ID=1", cn)
                cmd.ExecuteNonQuery()
            Catch ex As Exception
                LogException(ex)
            End Try
        End Using
    End Sub

End Module
