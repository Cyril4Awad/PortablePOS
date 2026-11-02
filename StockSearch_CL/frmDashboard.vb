Public Class frmDashboard

    Private cntPC As cntPotentialClients
    Private cntMI As cntMissingItems
    Private cntUP As cntUserPrivilege
    Private cntNG As cntNegations

    Private Sub btnSettings_Click(sender As Object, e As EventArgs)
        Try


        Catch ex As Exception

        End Try
    End Sub

    Private Sub btn_Click(sender As Object, e As EventArgs) Handles btnPotentialClients.Click, btnMissingItems.Click, btnUserPrivilege.Click, btnNegations.Click
        Try

            If sender Is btnPotentialClients Then

                If cntPC Is Nothing Then

                    cntPC = New cntPotentialClients

                    cntPC.Anchor = Windows.Forms.AnchorStyles.Bottom Or
                                   Windows.Forms.AnchorStyles.Left Or
                                   Windows.Forms.AnchorStyles.Right Or
                                   Windows.Forms.AnchorStyles.Top

                    cntPC.Size = pnlContainer.Size
                    cntPC.Location = pnlContainer.Location

                    Me.Controls.Add(cntPC)

                End If

                cntPC.BringToFront()

            ElseIf sender Is btnMissingItems Then

                If cntMI Is Nothing Then

                    cntMI = New cntMissingItems

                    cntMI.Anchor = Windows.Forms.AnchorStyles.Bottom Or
                                     Windows.Forms.AnchorStyles.Left Or
                                     Windows.Forms.AnchorStyles.Right Or
                                     Windows.Forms.AnchorStyles.Top

                    cntMI.Size = pnlContainer.Size
                    cntMI.Location = pnlContainer.Location

                    Me.Controls.Add(cntMI)

                End If

                cntMI.BringToFront()

            ElseIf sender Is btnUserPrivilege Then

                If cntUP Is Nothing Then

                    cntUP = New cntUserPrivilege

                    cntUP.Anchor = Windows.Forms.AnchorStyles.Bottom Or
                                   Windows.Forms.AnchorStyles.Left Or
                                   Windows.Forms.AnchorStyles.Right Or
                                   Windows.Forms.AnchorStyles.Top

                    cntUP.Size = pnlContainer.Size
                    cntUP.Location = pnlContainer.Location

                    Me.Controls.Add(cntUP)

                End If

                cntUP.BringToFront()

            ElseIf sender Is btnNegations Then

                If cntNG Is Nothing Then

                    cntNG = New cntNegations

                    cntNG.Anchor = Windows.Forms.AnchorStyles.Bottom Or
                                   Windows.Forms.AnchorStyles.Left Or
                                   Windows.Forms.AnchorStyles.Right Or
                                   Windows.Forms.AnchorStyles.Top

                    cntNG.Size = pnlContainer.Size
                    cntNG.Location = pnlContainer.Location

                    Me.Controls.Add(cntNG)

                End If

                cntNG.BringToFront()

            End If

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub Panel1_Paint(sender As Object, e As Windows.Forms.PaintEventArgs) Handles Panel1.Paint

    End Sub
End Class