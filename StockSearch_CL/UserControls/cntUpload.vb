Imports System.Data.SqlClient
Imports C1.Win.C1TrueDBGrid
Imports ApplicationsDesign.Grid

Public Class cntUpload

    Public HeadRecID As Double = -1
    Private dtItems As DataTable

    Public Sub FillGrid()
        Try

            If dtItems Is Nothing Then
                dtItems = New DataTable
            Else
                dtItems.Rows.Clear()
            End If

            'Using daItems As New SqlDataAdapter("SELECT 
            '                                Stk_Ref  [Reference] ,
            '                                STK_DESC [Description] ,
            '                                         ORDD_ItemQty [Qty] ,
            '                                         ORDD_ItemPrice [Price LBP] ,
            '                                         ORDD_ItemDiscount [Disc] ,
            '                                         ORDD_ItemTotal [Total LBP]
            '                                     FROM dbo.tbl_CPP_OrderDetails
            '                            INNER JOIN dbo.STK04_STKFIL ON ORDD_ItemRecID = STK_RECID
            '                                     WHERE ORDD_LocCode = '" & clsConfig.LocationCode & "' 
            '                                       AND ORDD_Number = " & HeadRecID & " 
            '                                     ORDER BY ORDD_RecID",
            '                                    clsConfig.SQLConnectionString)

            '    daItems.SelectCommand.CommandTimeout = 300
            '    daItems.Fill(dtItems)

            'End Using

            If dtItems.Rows.Count = 0 Then
                lblTotal.Text = ""
            Else
                lblTotal.Text = "Net LBP : " & CDbl(dtItems.Compute(" SUM([Total LBP]) ", " 1=1 ")).ToString("N")
            End If

            DesignGrid()

        Catch ex As Exception
            Throw
        End Try
    End Sub

    Private Sub DesignGrid()
        Try

            If lstItems.DataSource IsNot Nothing Then Exit Try

            With lstItems
                .DataSource = dtItems
                .ExtendRightColumn = True
                .FilterBar = True
                .ColumnFooters = True
                .FetchRowStyles = True
                .AllowSort = False
                .AllowUpdate = False
                .AllowDelete = False
                .AllowAddNew = False
                .ExtendRightColumn = True

                .Splits(0).ColumnCaptionHeight = 35

                With .Splits(0).DisplayColumns("Reference")
                    .Style.HorizontalAlignment = AlignHorzEnum.Near
                    .AllowSizing = True
                    .Locked = True
                    .Width = 120
                End With

                With .Splits(0).DisplayColumns("Description")
                    .Style.HorizontalAlignment = AlignHorzEnum.Near
                    .AllowSizing = True
                    .Locked = True
                    .Width = 230
                End With

                With .Splits(0).DisplayColumns("Qty")
                    .Style.HorizontalAlignment = AlignHorzEnum.Center
                    .Locked = True
                    .AllowSizing = False
                    .DataColumn.NumberFormat = "N"
                    .Style.Font = New Drawing.Font(.Style.Font.FontFamily, .Style.Font.Size, Drawing.FontStyle.Bold)
                End With

                With .Splits(0).DisplayColumns("Price LBP")
                    .Style.HorizontalAlignment = AlignHorzEnum.Center
                    .Locked = True
                    .AllowSizing = False
                    .DataColumn.NumberFormat = "N"
                    .Style.Font = New Drawing.Font(.Style.Font.FontFamily, .Style.Font.Size, Drawing.FontStyle.Bold)
                End With

                With .Splits(0).DisplayColumns("Disc")
                    .Style.HorizontalAlignment = AlignHorzEnum.Center
                    .Locked = True
                    .AllowSizing = False
                    .DataColumn.NumberFormat = "N2"
                    .Style.Font = New Drawing.Font(.Style.Font.FontFamily, .Style.Font.Size, Drawing.FontStyle.Bold)
                End With

                With .Splits(0).DisplayColumns("Total LBP")
                    .Style.HorizontalAlignment = AlignHorzEnum.Center
                    .Locked = True
                    .AllowSizing = False
                    .DataColumn.NumberFormat = "N"
                    .Style.Font = New Drawing.Font(.Style.Font.FontFamily, .Style.Font.Size, Drawing.FontStyle.Bold)
                End With

                SetGirdHeaders(lstItems, dtItems)

            End With

        Catch ex As Exception
            Throw
        End Try
    End Sub
End Class
