SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
GO

CREATE PROCEDURE SP_CPP_GetClientDues
       @ClientRecID NUMERIC(18,0) 
  as

SELECT  ROW_NUMBER() OVER ( ORDER BY DDA_INVHeader, DDA_OPNBR ) lineNumber ,
        'False' Checked ,
        Loc_Code locationCode ,
        Loc_Desc locationDescription ,
        DDA_INVHeader invoiceRecID ,
        INVH_Number invoiceNumber ,
        CONVERT(VARCHAR, INVH_DATE, 103) invoiceDate ,
        invh_nettotal invoiceNetTotal ,
        invh_Currency invoiceCurrency ,
		DDA_CRegister dueCashRegister,
		DDA_OPNBR dueOperationNumber,
		DDA_Counter dueCounter,
        DDA_ValueDate dueValueDate ,
        CONVERT(VARCHAR, DDA_ValueDate, 103) dueValueDateDisplay ,
        DDA_Perc duePercentage ,
        InitialDueValue initDueValue ,
        PendingValue dueValue ,
		CT_CURRCODE dueCurrency ,
        DDA_Paid amountToPay ,
        DDA_Remark dueRemark
FROM    ( SELECT    Loc_Code ,
                    Loc_Desc ,
                    DDA_INVHeader ,
                    INVH_DATE ,
                    CONCAT(INVH_DOCTYP1, ' ', INVH_DOCPREFIX, ' ',
                           CONVERT(VARCHAR(1000), INVH_SERNO)) INVH_Number ,
                    CASE WHEN OP_OPCurr = 'FC' THEN INVH_NETFC
                         ELSE INVH_NET
                    END AS [invh_nettotal] ,
                    CASE WHEN OP_OPCurr = 'FC' THEN INVH_CURRFC
                         ELSE INVH_CURR
                    END [invh_Currency] ,
                    DDA_CRegister ,
                    DDA_OPNBR ,
                    DDA_Counter ,
                    DDA_Perc ,
                    ISNULL(DDA_Value, 0) [InitialDueValue] ,
                    ISNULL(DDA_Value, 0) - ISNULL(AmountPaid, 0)
                    + ISNULL(AmountPaidcv, 0) AS PendingValue ,
                    ISNULL(AmountPaid, 0) PAIDRC ,
                    ISNULL(AmountPaidcv, 0) PAIDCV ,
                    CONVERT(NUMERIC(18, 3), 0) DDA_Paid ,
                    DDA_ValueDate ,
					CT_CURRCODE ,
                    DDA_Remark
          FROM      POS_DueDaysOnAC
                    INNER JOIN (SELECT * FROM POS_CASHTRANSACTION WHERE CT_SETTLTYPE= '500') POS_CASHTRANSACTION ON CT_LOCATION = DDA_Location
                                                          AND CT_INVHEADER = DDA_INVHeader
                                                          AND CT_CREGISTER = DDA_CRegister
                                                          AND CT_OPNBR = DDA_OPNBR
                    INNER JOIN ( SELECT *
                                 FROM   POS_SALEINVHEAD
                                 WHERE  INVH_CLIENTSUP = @ClientRecID
								 AND INVH_DOCTYP1='IN'
                               ) POS_SALEINVHEAD ON CT_LOCATION = INVH_LOC
                                                    AND CT_INVHEADER = INVH_RECID
                    INNER JOIN STK04_Location ON STK04_Location.Loc_Code = POS_DueDaysOnAC.DDA_Location
                    INNER JOIN dbo.STK04_OPParam ON STK04_OPParam.OP_Loc = STK04_Location.Loc_Code
                    LEFT JOIN ( SELECT  SUM(lrc_Amount) AmountPaid ,
                                        lrc_InvLoc ,
                                        lrc_InvHRecid ,
                                        lrc_OpNum ,
                                        lrc_DueCounter
                                FROM    POS_SALEINVLinkRCDue
                                        INNER JOIN ( SELECT *
                                                     FROM   POS_SALEINVHEAD
                                                     WHERE  INVH_CLIENTSUP = @ClientRecID
                                                   ) POS_SALEINVHEAD ON INVH_RECID = lrc_CNRCHRecid
                                                              AND INVH_LOC = lrc_CNRCLoc
                                                              AND INVH_DOCTYP1 IN (
                                                              'rc', 'cn' )
                                GROUP BY lrc_InvLoc ,
                                        lrc_InvHRecid ,
                                        lrc_OpNum ,
                                        lrc_DueCounter
                              ) LinkDue ON LinkDue.lrc_InvLoc = DDA_Location
                                           AND LinkDue.lrc_InvHRecid = DDA_INVHeader
                                           AND LinkDue.lrc_OpNum = DDA_OPNBR
                                           AND LinkDue.lrc_DueCounter = DDA_Counter
                    LEFT JOIN ( SELECT  SUM(lrc_Amount) AmountPaidcv ,
                                        lrc_InvLoc ,
                                        lrc_InvHRecid ,
                                        lrc_OpNum ,
                                        lrc_DueCounter
                                FROM    POS_SALEINVLinkRCDue
                                        INNER JOIN ( SELECT *
                                                     FROM   dbo.POS_SALEINVHEAD
                                                     WHERE  INVH_CLIENTSUP = @ClientRecID
                                                   ) POS_SALEINVHEAD ON INVH_RECID = lrc_CNRCHRecid
                                                              AND INVH_LOC = lrc_CNRCLoc
                                                              AND INVH_DOCTYP1 IN (
                                                              'cv', 'dv' )
                                GROUP BY lrc_InvLoc ,
                                        lrc_InvHRecid ,
                                        lrc_OpNum ,
                                        lrc_DueCounter
                              ) LinkDuecv ON LinkDuecv.lrc_InvLoc = DDA_Location
                                             AND LinkDuecv.lrc_InvHRecid = DDA_INVHeader
                                             AND LinkDuecv.lrc_OpNum = DDA_OPNBR
                                             AND LinkDuecv.lrc_DueCounter = DDA_Counter
          WHERE   /*  CT_CURRCODE = @Currency
                    AND */ ISNULL(DDA_Value, 0) - ISNULL(AmountPaid, 0)
                    + ISNULL(AmountPaidcv, 0) > 0
        ) tblDues;



GO