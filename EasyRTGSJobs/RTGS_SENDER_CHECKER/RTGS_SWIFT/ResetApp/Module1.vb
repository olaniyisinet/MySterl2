Imports System.Data.SqlClient

Module Module1

    Sub Main()
        Dim sql As String = "update Transactions set status='Paid' where status in ('SwiftFailed') and convert(date,uploaded_date)=convert(date,getdate())" 'SRS1701040903507
        Dim cn As New SqlConnection("Data Source=10.0.0.158,1490;Initial Catalog=easyRTGSdb_T24;user id=rtgs;password=(srtgs56*);Application Name=easyRTGSdbapp;")
        Using cn
            Try
                cn.Open()

            Catch ex As Exception

            End Try
            Dim cmd As New SqlCommand(sql, cn)
            Dim i As Integer = cmd.ExecuteNonQuery()

        End Using
    End Sub

End Module
