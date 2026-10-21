Imports System.Web.SessionState
Imports System.Data.SqlClient

Public Class Global_asax
    Inherits System.Web.HttpApplication

    Sub Application_Start(ByVal sender As Object, ByVal e As EventArgs)
        ' Fires when the application is started
    End Sub

    Sub Session_Start(ByVal sender As Object, ByVal e As EventArgs)
        ' Fires when the session is started
    End Sub

    Sub Application_BeginRequest(ByVal sender As Object, ByVal e As EventArgs)
        ' Fires at the beginning of each request
        Try

            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*") 'http://180.180.180.237:3000

            'HttpContext.Current.Response.AddHeader("Cache-Control", "no-cache")
            'HttpContext.Current.Response.AddHeader("Access-Control-Allow-Methods", "POST, PUT, DELETE")
            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept, token")
            'HttpContext.Current.Response.AddHeader("Access-Control-Max-Age", "1728000")
            'HttpContext.Current.Response.[End]()

            If Request.Headers("token") = "|CT$erv|Reg" & (Now.Day * 2).ToString Then Exit Try

            If Request.Headers("token") = "" OrElse Not RecordsExists($"Select TOP 1 1
                              From tbl_CPP_Devices WITH (NOLOCK)
                              Where CONCAT(DEV_Name, '|', DEV_ID, '_', DEV_UniqueID) = '{Request.Headers("token").Replace("'", "''")}' ") Then

                'HttpContext.Current.Response.StatusCode = 403
                'HttpContext.Current.Response.StatusDescription = "Forbidden"
                'HttpContext.Current.Response.SuppressContent = True
                'HttpContext.Current.ApplicationInstance.CompleteRequest()

            End If

        Catch ex As Exception
        End Try
    End Sub

    Sub Application_AuthenticateRequest(ByVal sender As Object, ByVal e As EventArgs)
        ' Fires upon attempting to authenticate the use
    End Sub

    Sub Application_Error(ByVal sender As Object, ByVal e As EventArgs)
        ' Fires when an error occurs
        'HttpContext.Current.Response.StatusCode = Server.GetLastError.Message

        'Server.ClearError()
    End Sub

    Sub Session_End(ByVal sender As Object, ByVal e As EventArgs)
        ' Fires when the session ends
    End Sub

    Sub Application_End(ByVal sender As Object, ByVal e As EventArgs)
        ' Fires when the application ends
    End Sub

End Class