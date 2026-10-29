SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
GO

CREATE OR ALTER PROCEDURE SP_CPP_SalesOrders
    @LocationCode VARCHAR(2)
AS
    SELECT  ROW_NUMBER() OVER ( ORDER BY HOFR_LOC, RecId ) [soRowNumber] ,
            CONCAT(HOFR_LOC, '|', CONVERT(VARCHAR, RecId)) [soKey] ,
            HOFR_LOC [soLocation] ,
            RecId [soRecID] ,
            CONCAT(Prefix, ' - ', [Header.Id], ' / ', [Seq No]) [soOfferReference] ,
            CONVERT(VARCHAR, [Date], 103) [soDate] ,
            [Type] [soType] ,
            [Cl.Code] [soClientCode] ,
            [Cl.#] [soClientNumber] ,
            [Client] [soClientName] ,
            [General Total] [soGeneralTotal] ,
            [Curr] [soCurrency] ,
            [VAT Type] [soVATType] ,
            ISNULL([Subject/Rem.], '') [soSubjectRemarks] ,
            CASE WHEN MAX(Inv) = 'N'
                      AND MAX(ParInv) = 'N'
                 THEN CASE WHEN MAX(Del) = 'N'
                                AND MAX(ParDel) = 'N'
                           THEN 'No Delivery / No Invoice'
                           WHEN COUNT(RecId) = SUM(CASE WHEN Del = 'Y' THEN 1
                                                        ELSE 0
                                                   END)
                           THEN 'Delivered Not Invoiced'
                           ELSE 'Partially Delivered Not Invoiced'
                      END
                 WHEN COUNT(RecId) = SUM(CASE WHEN Inv = 'Y' THEN 1
                                              ELSE 0
                                         END) THEN 'Invoiced'
                 WHEN MAX(ParInv) = 'Y'
                      OR COUNT(RecId) > SUM(CASE WHEN Inv = 'Y' THEN 1
                                                 ELSE 0
                                            END) THEN 'Partially Invoiced'
            END [soStatus] ,
            CONCAT(CONCAT(Prefix, ' - ', [Header.Id], ' / ', [Seq No]), '|',
                   [Client], '|' , CONVERT(VARCHAR, [Date], 103)) [soSearchString]
    FROM    ( SELECT    STK04_OfferHeader.HOFR_LOC ,
                        HOFR_HeadRecId RecId ,
                        STK04_OfferHeader.HOFR_Prefix Prefix ,
                        CAST(dbo.STK04_OfferHeader.HOFR_RECID AS VARCHAR(18)) AS [Header.Id] ,
                        STK04_OfferHeader.[HOFR_SeqNo] AS [Seq No] ,
                        dbo.STK04_OfferHeader.HOFR_DATE AS [Date] ,
                        CASE WHEN dbo.STK04_OfferHeader.HOFR_TYPE = 'C'
                             THEN 'Specific Client'
                             ELSE 'General-All Clients'
                        END AS [Type] ,
                        CAST(Stk04_Clients.CL_Code AS VARCHAR(18)) AS [Cl.Code] ,
                        CAST(Stk04_Clients.CL_No AS VARCHAR(18)) AS [Cl.#] ,
                        STK04_OfferHeader.HOFR_CLTNAME AS [Client] ,
                        dbo.STK04_OfferHeader.HOFR_ORDERBY AS [A/E Code] ,
                        CASE WHEN HOFR_INVTYPE = 'T' THEN HOFR_NET
                             ELSE ( HOFR_NET + HOFR_VATCURR )
                        END AS [General Total] ,
                        HOFR_CURR [Curr] ,
                        CASE WHEN HOFR_INVTYPE = 'T' THEN 'TTC'
                             WHEN HOFR_INVTYPE = 'A' THEN 'Add On'
                             ELSE 'Not Taxable'
                        END [VAT Type] ,
                        HOFR_INVTYPE [vat] ,
                        HOFR_Subject [Subject/Rem.] ,
                        CASE WHEN SUM(CASE DOFR_RECORDTYPE
                                        WHEN 'N' THEN ISNULL(invQty, 0)
                                        WHEN 'B' THEN ISNULL(bundleInvQty, 0)
                                        ELSE NULL
                                      END) OVER ( PARTITION BY STK04_OfferHeader.HOFR_LOC,
                                                  HOFR_HeadRecId, DOFR_Counter ) > 0
                                  AND SUM(ISNULL(DOFR_QTY, 0)) OVER ( PARTITION BY STK04_OfferHeader.HOFR_LOC,
                                                              HOFR_HeadRecId,
                                                              DOFR_Counter ) <= SUM(CASE DOFR_RECORDTYPE
                                                              WHEN 'N'
                                                              THEN ISNULL(invQty,
                                                              0)
                                                              WHEN 'B'
                                                              THEN ISNULL(bundleInvQty,
                                                              0)
                                                              ELSE NULL
                                                              END) OVER ( PARTITION BY STK04_OfferHeader.HOFR_LOC,
                                                              HOFR_HeadRecId,
                                                              DOFR_Counter )
                             THEN 'Y'
                             ELSE 'N'
                        END [Inv] ,
                        CASE WHEN SUM(CASE DOFR_RECORDTYPE
                                        WHEN 'N' THEN ISNULL(invQty, 0)
                                        WHEN 'B' THEN ISNULL(bundleInvQty, 0)
                                        ELSE NULL
                                      END) OVER ( PARTITION BY STK04_OfferHeader.HOFR_LOC,
                                                  HOFR_HeadRecId, DOFR_Counter ) > 0
                                  AND SUM(ISNULL(DOFR_QTY, 0)) OVER ( PARTITION BY STK04_OfferHeader.HOFR_LOC,
                                                              HOFR_HeadRecId,
                                                              DOFR_Counter ) > SUM(CASE DOFR_RECORDTYPE
                                                              WHEN 'N'
                                                              THEN ISNULL(invQty,
                                                              0)
                                                              WHEN 'B'
                                                              THEN ISNULL(bundleInvQty,
                                                              0)
                                                              ELSE NULL
                                                              END) OVER ( PARTITION BY STK04_OfferHeader.HOFR_LOC,
                                                              HOFR_HeadRecId,
                                                              DOFR_Counter )
                             THEN 'Y'
                             ELSE 'N'
                        END [ParInv] ,
                        CASE WHEN SUM(DelQty) OVER ( PARTITION BY STK04_OfferHeader.HOFR_LOC,
                                                     HOFR_HeadRecId,
                                                     DOFR_Counter ) > 0
                                  AND SUM(ISNULL(DOFR_QTY, 0)) OVER ( PARTITION BY STK04_OfferHeader.HOFR_LOC,
                                                              HOFR_HeadRecId,
                                                              DOFR_Counter ) <= SUM(DelQty) OVER ( PARTITION BY STK04_OfferHeader.HOFR_LOC,
                                                              HOFR_HeadRecId,
                                                              DOFR_Counter )
                             THEN 'Y'
                             ELSE 'N'
                        END [Del] ,
                        CASE WHEN SUM(DelQty) OVER ( PARTITION BY STK04_OfferHeader.HOFR_LOC,
                                                     HOFR_HeadRecId,
                                                     DOFR_Counter ) > 0
                                  AND SUM(ISNULL(DOFR_QTY, 0)) OVER ( PARTITION BY STK04_OfferHeader.HOFR_LOC,
                                                              HOFR_HeadRecId,
                                                              DOFR_Counter ) > SUM(DelQty) OVER ( PARTITION BY STK04_OfferHeader.HOFR_LOC,
                                                              HOFR_HeadRecId,
                                                              DOFR_Counter )
                             THEN 'Y'
                             ELSE 'N'
                        END [ParDel] ,
                        HOFR_ClIndexLOC ,
                        HOFR_ClIndexID ,
                        HOFR_AddDiscPerc ,
                        HOFR_AddDiscType
              FROM      dbo.STK04_OfferHeader
                        LEFT JOIN ( SELECT  CL_RecID ,
                                            CL_Code ,
                                            CL_No
                                    FROM    Stk04_Clients
                                  ) Stk04_Clients ON STK04_OfferHeader.HOFR_CLTCODE = Stk04_Clients.CL_RecID
                        LEFT JOIN ( SELECT DISTINCT
                                            DOFR_HeadRecId ,
                                            DOFR_LOC ,
                                            DOFR_Counter ,
                                            DOFR_RECORDTYPE ,
                                            DOFR_QTY ,
                                            DOFR_ITEM
                                    FROM    dbo.STK04_OfferDetails
                                    WHERE   DOFR_RECORDTYPE <> 'C'
                                  ) FiltredItems ON FiltredItems.DOFR_HeadRecId = STK04_OfferHeader.HOFR_HeadRecId
                                                    AND dbo.STK04_OfferHeader.HOFR_LOC = FiltredItems.DOFR_LOC
                        LEFT JOIN ( SELECT  INVD_LOC ,
                                            INVCO_OfferRecId ,
                                            INVCO_OfferCounter ,
                                            ISNULL(SUM(CASE WHEN INVD_STATUS = 'X'
                                                              OR ( INVH_DOCTYP1 = 'CN'
                                                              AND INVD_STATUS = 'G'
                                                              ) THEN -1
                                                            ELSE INVD_QTY
                                                       END), 0) invQty
                                    FROM    POS_SALEINVHEAD
                                            INNER JOIN dbo.POS_SALEINVDET ON dbo.POS_SALEINVDET.INVD_LOC = dbo.POS_SALEINVHEAD.INVH_LOC
                                                              AND dbo.POS_SALEINVDET.INVD_HEADID = dbo.POS_SALEINVHEAD.INVH_RECID
                                            INNER JOIN POS_SALEINVCO ON dbo.POS_SALEINVCO.INVCO_InvLoc = dbo.POS_SALEINVDET.INVD_LOC
                                                              AND dbo.POS_SALEINVCO.INVCO_InvHeadID = dbo.POS_SALEINVDET.INVD_HEADID
                                                              AND dbo.POS_SALEINVCO.INVCO_InvCounter = dbo.POS_SALEINVDET.INVD_Counter
                                    GROUP BY INVD_LOC ,
                                            INVCO_OfferRecId ,
                                            INVCO_OfferCounter
                                  ) invoiced ON INVD_LOC = DOFR_LOC
                                                AND INVCO_OfferRecId = DOFR_HeadRecId
                                                AND INVCO_OfferCounter = DOFR_Counter
                        LEFT  JOIN ( SELECT INVH_LOC ,
                                            INVCO_OfferRecId bndlOfferRecId ,
                                            INVCO_OfferCounter bndlOfferCounter ,
                                            ISNULL(MAX(INVB_Qty), 0) bundleInvQty
                                     FROM   POS_SALEINVHEAD
                                            INNER JOIN dbo.POS_SaleInvBundle ON INVH_LOC = INVB_Loc
                                                              AND INVH_RECID = INVB_HeadId
                                            INNER JOIN POS_SALEINVCO ON dbo.POS_SALEINVCO.INVCO_InvLoc = INVB_Loc
                                                              AND dbo.POS_SALEINVCO.INVCO_InvHeadID = INVB_HeadId
                                                              AND dbo.POS_SALEINVCO.INVCO_BundleCounter = INVB_BHCounter
                                     GROUP BY INVH_LOC ,
                                            INVCO_OfferRecId ,
                                            INVCO_OfferCounter
                                   ) bndlinvoiced ON INVH_LOC = DOFR_LOC
                                                     AND bndlOfferRecId = DOFR_HeadRecId
                                                     AND bndlOfferCounter = DOFR_Counter
                        LEFT JOIN ( SELECT  DOD_Color [COLOR] ,
                                            DOD_DetGrid [SIZE] ,
                                            DOD_ExpDate [ExpDate] ,
                                            COALESCE(CONVERT(VARCHAR, DOD_StkRecid),
                                                     '') [Item] ,
                                            DOD_Batch [Lot #] ,
                                            DOD_OffCounter ,
                                            DOD_OffHRecID ,
                                            DOD_OffLoc ,
                                            DOD_Grid [Grid] ,
                                            ISNULL(SUM(DOD_DelQty
                                                       * CASE WHEN DOH_Status = 'D'
                                                              THEN 1
                                                              ELSE -1
                                                         END), 0) DelQty
                                    FROM    dbo.STK04_DelOrdD
                                            INNER JOIN dbo.STK04_DelOrdH ON dbo.STK04_DelOrdH.DOH_Loc = dbo.STK04_DelOrdD.DOD_Loc
                                                              AND dbo.STK04_DelOrdH.DOH_RecId = dbo.STK04_DelOrdD.DOD_HrecId
                                    WHERE   DOH_Loc = 'WH'
                                    GROUP BY DOD_Color ,
                                            DOD_OffCounter ,
                                            DOD_OffHRecID ,
                                            DOD_OffLoc ,
                                            DOD_StkRecid ,
                                            DOD_DetGrid ,
                                            DOD_ExpDate ,
                                            DOD_Batch ,
                                            DOD_Grid
                                  ) Delivery ON DOD_OffCounter = DOFR_Counter
                                                AND DOD_OffHRecID = DOFR_HeadRecId
                                                AND DOD_OffLoc = DOFR_LOC
              WHERE     HOFR_CONFDATE IS NOT NULL
                        AND HOFR_ConvSO = 'Y'
                        AND HOFR_Active = 'Y'
                        AND ISNULL(HOFR_INVCLT, 'N') = 'N'
                        AND STK04_OfferHeader.HOFR_LOC = @LocationCode
                        AND EXISTS ( SELECT TOP 1
                                            1
                                     FROM   STK04_STKFIL
                                     WHERE  STK_RECID = DOFR_ITEM
                                            AND STK_FourthParty = 'N' )
            ) TB
    GROUP BY RecId ,
            Prefix ,
            [Header.Id] ,
            [Seq No] ,
            [Date] ,
            [Type] ,
            [Cl.Code] ,
            [Cl.#] ,
            [Client] ,
            [A/E Code] ,
            [General Total] ,
            [Curr] ,
            [VAT Type] ,
            [vat] ,
            [Subject/Rem.] ,
            HOFR_ClIndexLOC ,
            HOFR_ClIndexID ,
            HOFR_AddDiscPerc ,
            HOFR_AddDiscType ,
            HOFR_LOC
    HAVING  COUNT(RecId) <> SUM(CASE WHEN Inv = 'Y' THEN 1
                                     ELSE 0
                                END);





GO