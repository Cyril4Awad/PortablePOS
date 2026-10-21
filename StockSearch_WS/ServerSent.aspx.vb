Imports System.Data.SqlClient
Imports System.Web.Script.Serialization

Public Class ServerSent
    Inherits System.Web.UI.Page

    Public Shared lcConSql As String = ConfigurationManager.ConnectionStrings("AppConnectionString").ConnectionString()

    Private Sub ServerSent_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try

        Catch ex As Exception

        End Try
    End Sub

End Class

