
Imports System.Threading
Imports System.Data.SqlClient
Imports System.Web.Script.Serialization
Imports System.Net
Imports System.Text
Imports System.Security.Cryptography
Imports System.IO
'Imports CT ServStockSearch_WS

Public Class Form1
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Try



            PortablePOS_CL.clsConfig.SQLConnectionString = "server=SERVER;database=DATABASE;user=USER;password=PASSWORD"
            PortablePOS_CL.clsConfig.SystemUser = "Jad"
            PortablePOS_CL.clsConfig.SystemUserName = "Jamaleddine"
            PortablePOS_CL.clsConfig.LocalCurrency = "LBP"
            PortablePOS_CL.clsConfig.ControlCurrency = "USD"



            Using frm As New PortablePOS_CL.frmDashboard

               frm.ShowDialog()
            End Using

        Catch ex As Exception

        End Try
    End Sub

#Region "Encryption"

    Private initVector As String = "REDACTED"
    Private keysize As Integer = 256

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

End Class
