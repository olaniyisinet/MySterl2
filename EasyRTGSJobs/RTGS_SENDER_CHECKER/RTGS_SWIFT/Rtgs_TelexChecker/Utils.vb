Imports Oracle.ManagedDataAccess.Client
Imports Oracle.ManagedDataAccess.Types
Imports Nini.Config
Imports System.Data.SqlClient

Public Class Utils
    Private oraConnStr As String = "user id=appusr;password=testapp;data source=" + "(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)" + "(HOST=10.0.0.176)(PORT=1590))(CONNECT_DATA=" + "(SID = HOBANK3)))"
    Dim sqlConnStr As String = ""
    Public Function RemoveSepFromAccountNumber(ByVal acctnum As String, ByVal sepCharToRemove As String, ByVal replacementChar As String) As String
        Return acctnum.Replace(sepCharToRemove, replacementChar)
    End Function
    Public Function GetNuban(ByVal sterlingaccount As String) As String
        Dim nuban As String = String.Empty
        Dim oraSql As String = "SPRETURNNUBANNUMBER"
        Return NubanNumberRetrieval(sterlingaccount)

        'Using oraConn = New OracleConnection(oraConnStr)
        '    Try
        '        oraConn.Open()
        '        Dim oracmd As New OracleCommand(oraSql, oraConn)
        '        oracmd.CommandType = CommandType.StoredProcedure
        '        oracmd.Parameters.Add("@BankAccNum", sterlingaccount)
        '        oracmd.Parameters.Add("@nuban", OracleDbType.Varchar2).Direction = ParameterDirection.Output
        '        oracmd.Parameters.Add("@customername", OracleDbType.Varchar2).Direction = ParameterDirection.Output

        '        oracmd.ExecuteNonQuery()

        '        nuban = oracmd.Parameters("nuban").Value.ToString

        '    Catch ex As Exception
        '        Throw ex
        '    End Try
        'End Using
        'Return nuban
    End Function

    Public Function GetRtgsConnectionString() As String
        Dim rtgsConn As String = ""

        'TODO: Read from the Config file to get the connection string


        Return rtgsConn
    End Function

    Public Function GetOracleConnectionString() As String
        Dim oraConStr As String = String.Empty

        'TODO: Read the oracle connections tring rom the Init file

        Return oraConStr
    End Function

    Public Function NubanNumberRetrieval(ByVal BankAccNum As String) As String
        Dim result As String = String.Empty
        Dim sprocAccNameRetrieval As String = "SPRETURNNUBANNUMBER"
        Dim OracleConn As New OracleConnection()

        'Dim spwd As String = "testapp"

        OracleConn.ConnectionString = Utils.GetOraConnString()


        Try
            If Not OracleConn.State = ConnectionState.Open Then
                OracleConn.Open()
            End If

            Dim myCMD As OracleCommand = OracleConn.CreateCommand

            myCMD.CommandText = sprocAccNameRetrieval '"C07EDC06"

            myCMD.CommandType = CommandType.StoredProcedure

            ''Now let's set the command timeout to allow for long-running execution of the stored proc.
            myCMD.CommandTimeout = 0
            myCMD.Parameters.Add(New OracleParameter("@BankAccNum", OracleDbType.Varchar2, ParameterDirection.Input)).Value = BankAccNum.Trim
            myCMD.Parameters.Add(New OracleParameter("@nuban", OracleDbType.Varchar2, ParameterDirection.Output))
            myCMD.Parameters.Add(New OracleParameter("@customername", OracleDbType.Varchar2, ParameterDirection.Output))
            myCMD.Parameters("@nuban").Size = 50
            myCMD.Parameters("@customername").Size = 50
            myCMD.ExecuteNonQuery()
            'result = myCMD.Parameters("@customername").Value.ToString & "-" & myCMD.Parameters("@nuban").Value.ToString
            result = myCMD.Parameters("@nuban").Value.ToString

        Catch ex As OracleException
            Throw ex
        Finally


            If OracleConn IsNot Nothing Then
                If OracleConn.State <> ConnectionState.Closed Then OracleConn.Close()
            End If

        End Try

        Return result
    End Function
    Public Function BanksNumberRetrieval(ByVal nuban As String) As String
        Dim result As String = String.Empty
        Dim sprocAccNameRetrieval As String = "GET_STER_FRM_NUBAN1"
        Dim OracleConn As New OracleConnection()


        'Dim spwd As String = "testapp"
        OracleConn.ConnectionString = Utils.GetOraConnString()

        Try
            If Not OracleConn.State = ConnectionState.Open Then
                OracleConn.Open()
            End If

            Dim myCMD As OracleCommand = OracleConn.CreateCommand

            myCMD.CommandText = sprocAccNameRetrieval '"C07EDC06"

            myCMD.CommandType = CommandType.StoredProcedure

            ''Now let's set the command timeout to allow for long-running execution of the stored proc.
            myCMD.CommandTimeout = 0

            myCMD.Parameters.Add(New OracleParameter("@nuban", OracleDbType.Varchar2, 46, ParameterDirection.Input)).Value = nuban.Trim
            myCMD.Parameters.Add(New OracleParameter("@banksaccountnum", OracleDbType.Varchar2, ParameterDirection.Output))
            myCMD.Parameters.Add(New OracleParameter("@customername", OracleDbType.Varchar2, ParameterDirection.Output))


            myCMD.Parameters("@customername").Size = 50
            myCMD.Parameters("@banksaccountnum").Size = 50
            myCMD.ExecuteNonQuery()
            'result = myCMD.Parameters("@customername").Value.ToString & "-" & myCMD.Parameters("@banksaccountnum").Value.ToString
            result = myCMD.Parameters("@banksaccountnum").Value.ToString


        Catch ex As OracleException
            Throw ex
        Finally

            If OracleConn IsNot Nothing Then
                If OracleConn.State <> ConnectionState.Closed Then OracleConn.Close()
            End If

        End Try
        Return result
    End Function

    Shared Function GetMainQuery() As String
        Dim source As New IniConfigSource("Rtgs_FileChecker.ini")
        Dim query As String = String.Empty


        query = source.Configs("Query").Get("Main Query")

        Return query
    End Function

    Shared Function GetSqlConnString() As String
        Dim source As New IniConfigSource("Rtgs_FileChecker.ini")
        Dim connString As String = String.Empty


        connString = source.Configs("Connections").Get("Rtgs Conn")

        Return connString
    End Function

    Shared Function GetField72LinePrefix() As String
        Dim source As New IniConfigSource("Rtgs_FileChecker.ini")
        Dim linePrefix As String = String.Empty


        linePrefix = source.Configs("SWIFT").Get("Line Prefix For Field72")

        Return linePrefix
    End Function

    Shared Function GetNovelTradeFTConnString() As String
        'Novel Conn String
        Dim source As New IniConfigSource("Rtgs_FileChecker.ini")
        Dim connString As String = String.Empty


        connString = source.Configs("SWIFT").Get("Novel Conn String")

        Return connString
    End Function

    Private Shared Function GetOraConnString() As String
        Dim source As New IniConfigSource("Rtgs_FileChecker.ini")
        Dim connString As String = String.Empty


        connString = source.Configs("Connections").Get("Ora Conn")

        Return connString
    End Function
    Shared Function GetFTPHost() As String
        'Dim source As New IniConfigSource("Rtgs.ini")
        Dim ftpHost As String = String.Empty

        Dim cn As SqlConnection = New SqlConnection(GetNovelTradeFTConnString())
        Using cn
            cn.Open()
            Dim cmd As New SqlCommand("select top 1 swiftServerUrl from tblSwiftSettings")
            cmd.Connection = cn
            Dim dr As SqlDataReader = cmd.ExecuteReader()
            Using dr
                dr.Read()
                ftpHost = dr(0).ToString()
            End Using
        End Using
        'ftpHost = source.Configs("FTP").Get("Host")

        Return ftpHost
    End Function
    'Shared Function GetFTPHost() As String
    '    Dim source As New IniConfigSource("Rtgs_FileChecker.ini")
    '    Dim ftpHost As String = String.Empty


    '    ftpHost = source.Configs("FTP").Get("Host")

    '    Return ftpHost
    'End Function
    Shared Function GetOutputFolder() As String
        Dim source As New IniConfigSource("Rtgs_FileChecker.ini")
        Dim ftpInputFolder As String = String.Empty


        ftpInputFolder = source.Configs("FTP").Get("OutFolder")

        Return ftpInputFolder
    End Function
    'ProcessedFolder
    Shared Function GetProcessedFolder() As String
        Dim source As New IniConfigSource("Rtgs_FileChecker.ini")
        Dim ftpInputFolder As String = String.Empty


        ftpInputFolder = source.Configs("FTP").Get("ProcessedFolder")

        Return ftpInputFolder
    End Function
    Shared Function GetInputFolder() As String
        Dim source As New IniConfigSource("Rtgs_FileChecker.ini")
        Dim ftpInputFolder As String = String.Empty


        ftpInputFolder = source.Configs("FTP").Get("InFolder")

        Return ftpInputFolder
    End Function

    Shared Function Username() As String
        Dim source As New IniConfigSource("Rtgs_FileChecker.ini")
        Dim ftpUsername As String = String.Empty


        ftpUsername = source.Configs("FTP").Get("User")

        Return ftpUsername
    End Function

    Shared Function Password() As String
        Dim source As New IniConfigSource("Rtgs_FileChecker.ini")
        Dim ftpPassword As String = String.Empty


        ftpPassword = source.Configs("FTP").Get("Password")

        Return ftpPassword
    End Function

    Shared Function GetTelexFolder() As String
        'OutFolder
        Dim source As New IniConfigSource("Rtgs_FileChecker.ini")
        Dim ftpOutfolder As String = String.Empty


        ftpOutfolder = source.Configs("FTP").Get("OutFolder")

        Return ftpOutfolder
    End Function

    Shared Function GetFailedFolder() As String
        'OutFolder
        Dim source As New IniConfigSource("Rtgs_FileChecker.ini")
        Dim ftpOutfolder As String = String.Empty


        ftpOutfolder = source.Configs("FTP").Get("FailedFolder")

        Return ftpOutfolder
    End Function

    Shared Function GetInProcessQuery() As String
        'In Process Query
        Dim source As New IniConfigSource("Rtgs_FileChecker.ini")
        Dim query As String = String.Empty


        query = source.Configs("Query").Get("In Process Query")

        Return query
    End Function






End Class
