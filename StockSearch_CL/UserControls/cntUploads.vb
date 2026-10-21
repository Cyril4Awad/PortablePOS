#Region "Imports"

Imports System.Data.SqlClient

#End Region

Public Class cntUploads

#Region "Declarations"

    Private cntTitle As String = "Live Data Monitor"
    Private LiveData_Thread As Threading.Thread
    Private ReadingData As Boolean = False

#End Region

    Private Sub cnt_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try

            LiveData_Thread = New Threading.Thread(AddressOf GetLiveData)
            LiveData_Thread.Start()

        Catch ex As Exception

        End Try
    End Sub

    Private Sub LoadUploads()
        Dim strsql As String = ""
        Try

            flpMonitor.SuspendLayout()
            flpMonitor.Controls.Clear()

            'strsql = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
            '                      SELECT 
            '                          ORDH_Number ,
            '                          CONCAT (ORDH_LocCode, ' - ', CONVERT(VARCHAR,ORDH_Number) ) UplNo ,
            '                          ORDH_HeaderRemark,
            '                          CL_Name ,
            '                          ORDH_UserStamp ,
            '                          ORDH_DateStamp 
            '                      FROM tbl_CPP_OrderHeader 
            '                      INNER JOIN STK04_Clients on CL_RecID = ORDH_ClientRecID 
            '                      WHERE ORDH_LocCode = '" & clsConfig.LocationCode & "'
            '                           "

            Using dtUploads As New DataTable

                Using dasql As New SqlDataAdapter(strsql, clsConfig.SQLConnectionString)
                    dasql.SelectCommand.CommandTimeout = 300
                    dasql.Fill(dtUploads)
                End Using

                'Remove used
                For Each cntrl In flpMonitor.Controls
                    If cntrl.GetType Is GetType(cntUpload) Then

                        If dtUploads.Select(" ORDH_Number = " & CType(cntrl, cntUpload).HeadRecID.ToString & " ").Count = 0 Then
                            flpMonitor.Controls.Remove(cntrl)
                        End If

                    End If
                Next

                'Add New
                For Each rw In dtUploads.Select()

                    AddUpload(CDbl(rw("ORDH_Number").ToString),
                              "No : " & rw("UplNo").ToString & " // " & rw("ORDH_HeaderRemark").ToString,
                              rw("CL_Name").ToString,
                             CDate(rw("ORDH_DateStamp").ToString).ToString("dd/MM/yyyy HH:mm tt"))

                Next

            End Using

            flpMonitor.ResumeLayout()

        Catch ex As Exception
            Console.WriteLine(ex.ToString)
        End Try
    End Sub

    Private Sub AddUpload(_HeadRecID As Double, _Header As String, _Client As String, _DateStamp As String)
        Dim cntU As cntUpload
        Try

            cntU = New cntUpload

            With cntU
                .HeadRecID = _HeadRecID
                .lblHeader.Text = _Header
                .lblClient.Text = _Client
                .lblDateStamp.Text = _DateStamp
                .FillGrid()
            End With

            flpMonitor.Controls.Add(cntU)

        Catch ex As Exception
            Throw
        End Try
    End Sub

    Private Sub GetLiveData()
        Try

            While True

                If Not ReadingData Then
                    GetDataInBG()
                End If

            End While

        Catch ex As Exception

        End Try
    End Sub

    Private Sub GetDataInBG()
        Try

            ReadingData = True

            Me.Invoke(Sub() LoadUploads())

            Threading.Thread.Sleep(100000)

        Catch ex As Exception
        Finally
            ReadingData = False
        End Try
    End Sub

    Private Sub cnt_Disposed(sender As Object, e As EventArgs) Handles Me.Disposed
        Try

            If LiveData_Thread IsNot Nothing Then
                If LiveData_Thread.IsAlive Then
                    LiveData_Thread.Abort()
                    LiveData_Thread = Nothing
                End If
            End If

        Catch ex As Exception

        End Try
    End Sub

End Class
