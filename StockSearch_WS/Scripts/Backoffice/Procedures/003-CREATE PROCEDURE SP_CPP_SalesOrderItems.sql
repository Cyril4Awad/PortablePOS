SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
GO
ALTER PROCEDURE SP_CPP_SalesOrderItems
    @LocationCode VARCHAR(2) ,
    @HeaderRecID NUMERIC(18, 0)
AS
    WITH    tbl
              AS ( SELECT   CASE WHEN DOFR_RECORDTYPE <> 'B'
                                 THEN CASE WHEN ISNULL(DOFR_BUNDLE, 0) = 0
                                           THEN DOFR_Counter
                                           ELSE DOFR_BUNDLE
                                      END
                                 ELSE CASE WHEN DOFR_RECORDTYPE = 'B'
                                           THEN DOFR_Counter
                                      END
                            END OrderCounter1 ,
                            CASE WHEN ( DOFR_RECORDTYPE = 'B'
                                        OR ISNULL(DOFR_BUNDLE, 0) = 0
                                      ) THEN 0
                                 ELSE 1
                            END OrderCounter2 ,
                            DOFR_LOC [OfferLocation] ,
                            DOFR_HeadRecId [OfferHeadRecID] ,
                            DOFR_Counter [OfferCounter] ,
                            DOFR_ITEM ItemRec ,
                            ISNULL(STK_SingleRef1, STK_REF) ItemRef ,
                            STK_ItmClass ,
                            DOFR_ExpDate ExpDate ,
                            DOFR_BatchNo BatchNo ,
                            ( CASE WHEN SUBSTRING(DOFR_ExpDate, 1, 3) = '00.'
                                   THEN SUBSTRING(DOFR_ExpDate, 4,
                                                  LEN(DOFR_ExpDate))
                                   ELSE DOFR_ExpDate
                              END ) ExpiryShown ,
                            DOFR_ITMDESC ItmDesc ,
                            CASE WHEN DOFR_GIFT = 'Y' THEN 'G'
                                 ELSE CASE WHEN DOFR_QTY < 0 THEN 'X'
                                           ELSE 'S'
                                      END
                            END TransStat ,
                            DOFR_QTY Offeredqty ,
                            CASE WHEN DOFR_RECORDTYPE = 'N'
                                      OR DOFR_RECORDTYPE = 'B'
                                 THEN ISNULL(DOD_DelQty, 0)
                                 ELSE NULL
                            END dofr_qty ,
                            CASE DOFR_RECORDTYPE
                              WHEN 'N' THEN ISNULL(invQty, 0)
                              WHEN 'B' THEN ISNULL(bundleInvQty, 0)
                              ELSE NULL
                            END invQty ,
                            CASE WHEN DOFR_RECORDTYPE = 'N'
                                 THEN ISNULL(STQ_AVAILABLE, 0)
                                      - ISNULL(STQ_RESERVED, 0)
                                      + ISNULL(ROD_ResQty, 0)
                                 ELSE NULL
                            END [AvailableQTY] ,
                            CASE WHEN DOFR_RECORDTYPE <> 'B'
                                 THEN CONVERT(DECIMAL(18, 2), CASE
                                                              WHEN STK_QTYCTRL = 'Y'
                                                              AND ISNULL(STQ_AVAILABLE
                                                              - STQ_RESERVED,
                                                              0) <= 0 THEN 0
                                                              ELSE CASE
                                                              WHEN ISNULL(invQty,
                                                              0) > ISNULL(DOD_DelQty,
                                                              0)
                                                              AND ISNULL(invQty,
                                                              0) > DOFR_QTY
                                                              THEN 0
                                                              ELSE CASE
                                                              WHEN ISNULL(invQty,
                                                              0) < ISNULL(DOD_DelQty,
                                                              0)
                                                              AND ISNULL(DOD_DelQty,
                                                              0) > 0
                                                              THEN ISNULL(DOD_DelQty,
                                                              0)
                                                              - ISNULL(invQty,
                                                              0)
                                                              ELSE CASE
                                                              WHEN ISNULL(invQty,
                                                              0) = 0
                                                              AND ISNULL(DOD_DelQty,
                                                              0) = 0
                                                              THEN DOFR_QTY
                                                              ELSE CASE
                                                              WHEN ISNULL(DOD_DelQty,
                                                              0) = 0
                                                              AND ISNULL(invQty,
                                                              0) > 0
                                                              AND ISNULL(invQty,
                                                              0) < DOFR_QTY
                                                              THEN DOFR_QTY
                                                              - ISNULL(invQty,
                                                              0)
                                                              ELSE 0
                                                              END
                                                              END
                                                              END
                                                              END
                                                              END)
                                 WHEN DOFR_RECORDTYPE = 'B'
                                 THEN CONVERT(DECIMAL(18, 2), CASE
                                                              WHEN ISNULL(bundleInvQty,
                                                              0) > ISNULL(DOD_DelQty,
                                                              0)
                                                              AND ISNULL(bundleInvQty,
                                                              0) > DOFR_QTY
                                                              THEN 0
                                                              ELSE CASE
                                                              WHEN ISNULL(bundleInvQty,
                                                              0) < ISNULL(DOD_DelQty,
                                                              0)
                                                              AND ISNULL(DOD_DelQty,
                                                              0) > 0
                                                              THEN ISNULL(DOD_DelQty,
                                                              0)
                                                              - ISNULL(bundleInvQty,
                                                              0)
                                                              ELSE CASE
                                                              WHEN ISNULL(bundleInvQty,
                                                              0) = 0
                                                              AND ISNULL(DOD_DelQty,
                                                              0) = 0
                                                              THEN DOFR_QTY
                                                              ELSE CASE
                                                              WHEN ISNULL(DOD_DelQty,
                                                              0) = 0
                                                              AND ISNULL(bundleInvQty,
                                                              0) > 0
                                                              AND ISNULL(bundleInvQty,
                                                              0) < DOFR_QTY
                                                              THEN DOFR_QTY
                                                              - ISNULL(bundleInvQty,
                                                              0)
                                                              ELSE 0
                                                              END
                                                              END
                                                              END
                                                              END)
                            END QtyImported ,
                            STK_UNIT ,
                            dbo.FromRTFtoText(DOFR_Remarks) Remarks
                   FROM     STK04_OfferHeader
                            INNER JOIN dbo.STK04_OfferDetails ON dbo.STK04_OfferHeader.HOFR_HeadRecId = dbo.STK04_OfferDetails.DOFR_HeadRecId
                                                              AND HOFR_LOC = DOFR_LOC
                            LEFT JOIN ( SELECT  DOD_OffLoc ,
                                                DOD_OffHRecID ,
                                                DOD_OffCounter ,
                                                ISNULL(SUM(CASE
                                                              WHEN DOH_Status = 'D'
                                                              THEN 1
                                                              ELSE -1
                                                           END * DOD_DelQty),
                                                       0) DOD_DelQty
                                        FROM    STK04_DelOrdH
                                                INNER JOIN STK04_DelOrdD ON DOD_Loc = DOH_Loc
                                                              AND DOD_HrecId = DOH_RecId
                                        GROUP BY DOD_OffLoc ,
                                                DOD_OffHRecID ,
                                                DOD_OffCounter
                                      ) STK04_DelOrdD ON DOD_OffLoc = DOFR_LOC
                                                         AND DOD_OffHRecID = DOFR_HeadRecId
                                                         AND DOD_OffCounter = DOFR_Counter
                            LEFT JOIN ( SELECT  INVD_LOC ,
                                                INVCO_OfferRecId ,
                                                INVCO_OfferCounter ,
                                                ISNULL(SUM(CASE
                                                              WHEN INVD_STATUS = 'X'
                                                              OR ( INVH_DOCTYP1 = 'CN'
                                                              AND INVD_STATUS = 'G'
                                                              ) THEN -1
                                                              ELSE 1
                                                           END * INVD_QTY), 0) invQty
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
                            LEFT JOIN ( SELECT  INVH_LOC ,
                                                INVCO_OfferRecId bndlOfferRecId ,
                                                INVCO_OfferCounter bndlOfferCounter ,
                                                ISNULL(MAX(INVB_Qty), 0) bundleInvQty
                                        FROM    POS_SALEINVHEAD
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
                            LEFT JOIN STK04_STKFIL POS_STKFIL ON dbo.STK04_OfferDetails.DOFR_ITEM = POS_STKFIL.STK_RECID
                            LEFT JOIN Stk04_Brand ON br_Code = STK_Brand
                            LEFT JOIN stk04_stkqty POS_STKQTY ON STK_RECID = STQ_STKRECID
                                                              AND COALESCE(STQ_Color,
                                                              '') = COALESCE(DOFR_COLOR,
                                                              '')
                                                              AND COALESCE(STQ_Grid,
                                                              '') = COALESCE(DOFR_GRID,
                                                              '')
                                                              AND COALESCE(STQ_DetGrid,
                                                              '') = COALESCE(DOFR_SIZE,
                                                              '')
                                                              AND COALESCE(STQ_ExpiryDt,
                                                              '') = COALESCE(DOFR_ExpDate,
                                                              '')
                                                              AND COALESCE(STQ_BatchInfo,
                                                              '') = COALESCE(DOFR_BatchNo,
                                                              '')
                                                              AND STQ_LOCCODE = @LocationCode
                            LEFT JOIN dbo.Stk04_COLOR ON DOFR_COLOR = Color_Code
                            INNER JOIN STK04_Employee POS_Employee ON HOFR_ORDERBY = E_Code
                            LEFT JOIN ( SELECT  ROH_OffHeadID ,
                                                ROD_OffCounter ,
                                                ROH_Loc ,
                                                ROD_StkRecid ,
                                                COALESCE(ROD_Grid, '') ROD_Grid ,
                                                COALESCE(ROD_DetGrid, '') ROD_DetGrid ,
                                                COALESCE(ROD_Color, '') ROD_Color ,
                                                COALESCE(ROD_ExpDate, '') ROD_ExpDate ,
                                                COALESCE(ROD_Batch, '') ROD_Batch ,
                                                SUM(ROD_ResQty
                                                    * CASE WHEN ROH_Status = 'N'
                                                           THEN 1
                                                           ELSE -1
                                                      END) ROD_ResQty
                                        FROM    dbo.stk04_ResOrdH
                                                INNER JOIN dbo.STK04_ResOrdD ON dbo.STK04_ResOrdD.ROD_Loc = dbo.stk04_ResOrdH.ROH_Loc
                                                              AND dbo.STK04_ResOrdD.ROD_Hrecid = dbo.stk04_ResOrdH.ROH_Recid
                                        GROUP BY ROH_OffHeadID ,
                                                ROD_OffCounter ,
                                                ROH_Loc ,
                                                ROD_StkRecid ,
                                                COALESCE(ROD_Grid, '') ,
                                                COALESCE(ROD_DetGrid, '') ,
                                                COALESCE(ROD_Color, '') ,
                                                COALESCE(ROD_ExpDate, '') ,
                                                COALESCE(ROD_Batch, '')
                                      ) resOrd ON ROD_StkRecid = DOFR_ITEM
                                                  AND COALESCE(ROD_Grid, '') = COALESCE(DOFR_GRID,
                                                              '')
                                                  AND COALESCE(ROD_DetGrid, '') = COALESCE(DOFR_SIZE,
                                                              '')
                                                  AND COALESCE(ROD_ExpDate, '') = COALESCE(DOFR_ExpDate,
                                                              '')
                                                  AND COALESCE(DOFR_BatchNo,
                                                              '') = COALESCE(ROD_Batch,
                                                              '')
                                                  AND COALESCE(DOFR_COLOR, '') = COALESCE(ROD_Color,
                                                              '')
                                                  AND ROH_OffHeadID = HOFR_HeadRecId
                                                  AND ROD_OffCounter = DOFR_Counter
                                                  AND ROH_Loc = HOFR_LOC
                            LEFT JOIN Stk04_stkprices ON SP_StkRecId = STK_RECID
                                                         AND SP_PriceType = HOFR_PRICEUSED
                                                         AND SP_PriceType = 'BM'
														 AND ISNULL(SP_StartingQty, 0) = 0
                   WHERE    HOFR_LOC = @LocationCode
                            AND HOFR_HeadRecId = @HeaderRecID
                            AND DOFR_RECORDTYPE = 'N'
                 )
        SELECT  ROW_NUMBER() OVER ( ORDER BY OrderCounter1 , OrderCounter2 , OfferCounter ) sodRowNumber ,
                CONCAT(OfferLocation, '|', CONVERT(VARCHAR, OfferHeadRecID),
                       '|', CONVERT(VARCHAR, OfferCounter)) sodKey ,
                OfferLocation sodLocation ,
                OfferHeadRecID sodHRecid ,
                OfferCounter sodCounter ,
                ItemRec sodItemRecID ,
                ItemRef sodItemReference ,
                ItmDesc sodItemDescription ,
                ISNULL(Offeredqty, 0) sodQtyOffered ,
                ISNULL(QtyImported, 0) sodQtyNeeded ,
                ISNULL(dofr_qty, 0) sodQtyDelivered ,
                ISNULL(invQty, 0) sodQtyInvoiced ,
                ISNULL(PreparedQty, 0) sodQtyPrepared ,
                ISNULL(QtyImported, 0) - ISNULL(PreparedQty, 0) sodQtyPending ,
                STK_UNIT sodUnit ,
                Remarks sodRemarks ,
                CONCAT(ItemRef, '|', ItmDesc) sodSearchString
        FROM    tbl
                LEFT JOIN ( SELECT  ODPQ_Location ,
                                    ODPQ_HeaderRecID ,
                                    ODPQ_Counter ,
                                    ISNULL(SUM(ISNULL(ODPQ_Qty, 0)), 0) PreparedQty
                            FROM    STK04_OfferDetailsPreparedQty
                            WHERE   ODPQ_Location = @LocationCode
                                    AND ODPQ_HeaderRecID = @HeaderRecID
                            GROUP BY ODPQ_Location ,
                                    ODPQ_HeaderRecID ,
                                    ODPQ_Counter
                          ) STK04_OfferDetailsPreparedQty ON ODPQ_Location = OfferLocation
                                                             AND ODPQ_HeaderRecID = tbl.OfferHeadRecID
                                                             AND ODPQ_Counter = OfferCounter
        WHERE   ISNULL(QtyImported, 0) <> 0
        ORDER BY OrderCounter1 ,
                OrderCounter2 ,
                OfferCounter;



GO