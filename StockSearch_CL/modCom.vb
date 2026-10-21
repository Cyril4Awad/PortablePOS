Imports System.Data.SqlClient

Module modCom

    Public Function SQL_ExecuteScalar(_StrQuery As String) As String
        Dim obj As Object
        Try

            Using cnsql As New SqlConnection(clsConfig.SQLConnectionString)
                cnsql.Open()

                Using cmsql As New SqlCommand(_StrQuery, cnsql)
                    obj = cmsql.ExecuteScalar
                End Using

                If obj Is Nothing Then
                    Return ""
                Else
                    Return obj.ToString
                End If

                cnsql.Close()
            End Using

        Catch ex As Exception
            Throw
        End Try
    End Function

End Module
