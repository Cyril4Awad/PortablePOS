Imports System.Threading
Imports System.Data.SqlClient
Imports System.Web.Script.Serialization
Imports System.Net
Imports System.Text
Imports System.Security.Cryptography
Imports System.IO
Imports PortablePOS_WS
Imports Newtonsoft.Json.Linq
Imports Newtonsoft.Json.Serialization

Module modCommon

#Region "SQL"

    Public Sub Execute_WOXACT(ByVal ParamArray SqlStatments() As String)
        Try

            Using Conn As New SqlConnection(DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                Conn.Open()

                Using Com As New SqlCommand("", Conn)

                    Com.CommandTimeout = 900
                    For Idx As Integer = 0 To SqlStatments.Length - 1
                        Com.CommandText = SqlStatments(Idx)
                        Com.ExecuteNonQuery()
                    Next

                End Using

                Conn.Close()
            End Using

        Catch e As Exception
            Throw e
        End Try
    End Sub

    Public Function ExecuteScalar_WOXACT(ByVal SqlStatment As String) As String
        Dim obj As Object
        Try

            Using Conn As New SqlConnection(DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                Conn.Open()

                Using Com As New SqlCommand(SqlStatment, Conn)
                    obj = Com.ExecuteScalar()
                    ExecuteScalar_WOXACT = IIf(obj Is Nothing, "", obj).ToString
                End Using

                Conn.Close()
            End Using

        Catch e As Exception
            Throw e
        End Try
    End Function

    Public Function RecordsExists(ByRef _strsql As String) As Boolean
        Try

            Using cnSql As New SqlConnection(DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                cnSql.Open()

                Using cmSql As New SqlCommand(_strsql, cnSql)

                    If cmSql.ExecuteScalar IsNot Nothing Then
                        Return True
                    Else
                        Return False
                    End If

                End Using

            End Using

        Catch ex As Exception
            Throw
        End Try
    End Function

    Public Function GetFieldDesc(ByVal Table As String, ByVal Criteria As String, ByVal ReturnDesc As String, Optional ByVal cmSqlTrans As SqlCommand = Nothing) As String
        'Function to look up Description In A given Table
        Dim strsql As String
        Dim cnSql As SqlConnection
        Dim cmSql As SqlCommand
        Dim drSql As SqlDataReader

        Try
            If Criteria = "" Then GetFieldDesc = "" : Exit Function


            strsql = "select " & ReturnDesc & " from " & Table & " where " & Criteria

            If cmSqlTrans Is Nothing Then
                cnSql = New SqlConnection(DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                cnSql.Open()

                cmSql = New SqlCommand(strsql, cnSql)

                drSql = cmSql.ExecuteReader
            Else
                cmSqlTrans.CommandText = strsql
                drSql = cmSqlTrans.ExecuteReader
            End If

            If drSql.Read Then
                GetFieldDesc = drSql.Item(0).ToString
            Else
                GetFieldDesc = ""
            End If
        Catch ex As Exception
            'MessageBox.Show("Error: " & ex.Message, "POS Module", _
            'MessageBoxButtons.OK, MessageBoxIcon.Error)
            Throw ex
        Finally
            If Not drSql Is Nothing Then
                If drSql.IsClosed = False Then drSql.Close()
                drSql = Nothing
            End If

            cmSql = Nothing

            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql = Nothing
            End If
        End Try
    End Function

    Public Sub DropSqlObject(ByVal ObjName As String, ByVal ObjType As String, Optional ByVal _cmSql As SqlCommand = Nothing)
        Dim strsql As String
        Dim cnsql As SqlConnection
        Dim cmsql As SqlCommand
        Try
            If _cmSql Is Nothing Then
                cnsql = New SqlConnection(DecryptString(ServerSent.lcConSql, DecPS("REDACTED")))
                cnsql.Open()
                cmsql = New SqlCommand(strsql, cnsql)
            End If

            If Trim(ObjName) = "" Then Exit Sub

            strsql = "IF EXISTS (SELECT * FROM SYSOBJECTS WHERE ID = OBJECT_ID(N'" + ObjName + "') "

            If ObjType = "VIEW" Then
                strsql &= " AND OBJECTPROPERTY(ID, N'IsView') = 1)"
                strsql &= " DROP view [dbo].[" + ObjName + "]"

                If _cmSql Is Nothing Then
                    cmsql.CommandText = strsql
                    cmsql.ExecuteNonQuery()
                Else
                    _cmSql.CommandText = strsql
                    _cmSql.ExecuteNonQuery()
                End If

            ElseIf ObjType = "TABLE" Then
                strsql &= " AND OBJECTPROPERTY(ID, N'IsUserTable') = 1)"
                strsql &= " DROP Table [dbo].[" + ObjName + "]"

                If _cmSql Is Nothing Then
                    cmsql.CommandText = strsql
                    cmsql.ExecuteNonQuery()
                Else
                    _cmSql.CommandText = strsql
                    _cmSql.ExecuteNonQuery()
                End If
            End If
        Catch ex As Exception
            Throw ex
        Finally
            cmsql = Nothing

            If Not cnsql Is Nothing Then
                If cnsql.State = ConnectionState.Open Then cnsql.Close()
                cnsql = Nothing
            End If
        End Try
    End Sub

#End Region

#Region "Encryption"

    Private initVector As String = "REDACTED"
    Private keysize As Integer = 256

    Public Function EncPS(ByVal spwd1 As String) As String
        Dim lp As Integer
        Try
            lp = Len(spwd1)
            For i = 1 To lp
                EncPS = EncPS + System.Text.Encoding.GetEncoding("windows-1256").GetString(System.Text.Encoding.GetEncoding("windows-1252").GetBytes(Chr(Asc(Right(spwd1, i)) + lp)))
            Next i
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Public Function DecPS(ByVal spwd1 As String) As String
        Dim lp As Integer
        Try
            lp = Len(spwd1)
            For i = 1 To lp
                DecPS = DecPS + System.Text.Encoding.GetEncoding("windows-1256").GetString(System.Text.Encoding.GetEncoding("windows-1252").GetBytes(Chr(Asc(Microsoft.VisualBasic.Right(spwd1, i)) - lp)))
            Next i
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Public Function DecryptString(ByVal cipherText As String, ByVal passPhrase As String) As String

        Try

            Dim initVectorBytes As Byte() = Encoding.UTF8.GetBytes(initVector)
            Dim cipherTextBytes As Byte() = Convert.FromBase64String(cipherText)
            Dim password As PasswordDeriveBytes = New PasswordDeriveBytes(passPhrase, Nothing)
            Dim keyBytes As Byte() = password.GetBytes(CInt(keysize / 8))
            Dim symmetricKey As RijndaelManaged = New RijndaelManaged()
            symmetricKey.Mode = CipherMode.CBC
            Dim decryptor As ICryptoTransform = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes)
            Dim memoryStream As MemoryStream = New MemoryStream(cipherTextBytes)
            Dim cryptoStream As CryptoStream = New CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read)
            Dim plainTextBytes As Byte() = New Byte(cipherTextBytes.Length - 1) {}
            Dim decryptedByteCount As Integer = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length)
            memoryStream.Close()
            cryptoStream.Close()
            Return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount)

        Catch ex As Exception
            Return ""
        End Try

    End Function

#End Region

#Region "Others"

    Public Sub Disposecontrols(Optional ByRef _cnsql As SqlConnection = Nothing, Optional ByRef _cmsql As SqlCommand = Nothing, Optional ByRef _drsql As SqlDataReader = Nothing, Optional ByRef _trnsql As SqlTransaction = Nothing, Optional ByRef _cbsql As SqlCommandBuilder = Nothing, Optional ByRef _daSQL As SqlDataAdapter = Nothing, Optional ByRef _linkserver As String = Nothing)

        Try


            If Not _drsql Is Nothing Then
                If Not _drsql.IsClosed Then _drsql.Close()
                _drsql = Nothing
            End If

            If Not _trnsql Is Nothing Then
                _trnsql.Dispose()
                _trnsql = Nothing
            End If

            If Not _cmsql Is Nothing Then
                _cmsql.Dispose()
                _cmsql = Nothing
            End If

            If Not _cnsql Is Nothing Then
                If _cnsql.State = ConnectionState.Open Then _cnsql.Close()
                _cnsql.Dispose()
                _cnsql = Nothing
            End If
            If Not _cbsql Is Nothing Then
                _cbsql = Nothing
            End If
            'If Not _linkserver Is Nothing AndAlso Not _linkserver = "" Then DropLinkServer(_linkserver)
        Catch ex As Exception
            Throw ex
        End Try
    End Sub

#End Region

End Module
