SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
GO

-- The price of each item in each location and company, in the local and the control currency :
-- the preferred price type of the location (SL_PrefPrc), its discount while its date is not passed,
-- VAT included or not (exempt location or item), the local price calculated from the control one when
-- the location and the item recalculate it (at the rate of the working day of the location)
CREATE OR ALTER VIEW V_CPP_LabelItemPrices
AS
    SELECT  itemRecID ,
            locationCode ,
            companyCode ,
            CASE WHEN RecalculateLCBasedOnCC = 'Y'
                 THEN dbo.convert5('POSO', companyCode, CompanyYear, PriceControlCurrency, ControlCurrency, LocalCurrency, WorkingDate)
                 ELSE PriceLocalCurrency
            END [priceLC] ,
            PriceControlCurrency [priceCC]
    FROM    ( SELECT    STK_RECID itemRecID ,
                        Loc_Code locationCode ,
                        Ac_Comp companyCode ,
                        Ac_Year CompanyYear ,
                        Ac_Curr1 LocalCurrency ,
                        Ac_Curr2 ControlCurrency ,
                        ISNULL(CASE WHEN Loc_Vat = 'N'
                                         OR STK_VATTYPE = 'N'
                                         OR Loc_VATRetType = 'A'
                                    THEN SP_LCTAXEX
                                         - CASE WHEN SP_DISCDATE >= WorkingDate
                                                THEN ( SP_LCTAXEX * SP_DISC ) / 100
                                                ELSE 0
                                           END
                                    ELSE SP_LCTTC
                                         - CASE WHEN SP_DISCDATE >= WorkingDate
                                                THEN ( SP_LCTTC * SP_DISC ) / 100
                                                ELSE 0
                                           END
                               END, 0) PriceLocalCurrency ,
                        ISNULL(CASE WHEN Loc_Vat = 'N'
                                         OR STK_VATTYPE = 'N'
                                         OR Loc_VATRetType = 'A'
                                    THEN SP_FCTAXEX
                                         - CASE WHEN SP_DISCDATE >= WorkingDate
                                                THEN ( SP_FCTAXEX * SP_DISC ) / 100
                                                ELSE 0
                                           END
                                    ELSE SP_FCTTC
                                         - CASE WHEN SP_DISCDATE >= WorkingDate
                                                THEN ( SP_FCTTC * SP_DISC ) / 100
                                                ELSE 0
                                           END
                               END, 0) PriceControlCurrency ,
                        CASE WHEN ISNULL(OP_LCBasedOnCC, 'N') = 'Y'
                                  AND ISNULL(NULLIF(STK_RecalculateLC, ''), 'Y') = 'Y'
                             THEN 'Y'
                             ELSE 'N'
                        END RecalculateLCBasedOnCC ,
                        WorkingDate
              FROM      dbo.Stk04_stkprices
                        INNER JOIN dbo.STK04_STKFIL ON STK_RECID = SP_StkRecId
                        INNER JOIN dbo.pos_splocation ON SL_PrefPrc = SP_PriceType
                        INNER JOIN dbo.STK04_Location ON Loc_Code = SL_Loc
                        INNER JOIN dbo.STK04_OPParam ON OP_Loc = Loc_Code
                        INNER JOIN ( SELECT Ac_Comp ,
                                            Ac_Year ,
                                            Ac_Curr1 ,
                                            Ac_Curr2 ,
                                            ROW_NUMBER() OVER ( PARTITION BY Ac_Comp ORDER BY Ac_EndDate DESC ) RowNumber
                                     FROM   dbo.Com_AppComp
                                     WHERE  Ac_Code = 'POSO'
                                   ) Com_AppComp ON RowNumber = 1
                        LEFT JOIN ( SELECT  CT_LOCATION ,
                                            MAX(CT_OPDATE) WorkingDate
                                    FROM    dbo.POS_CASHTRANSACTION
                                    GROUP BY CT_LOCATION
                                  ) WkDateTbl ON CT_LOCATION = Loc_Code
              WHERE     ISNULL(SP_StartingQty, 0) = 0
            ) tblStock;


GO
