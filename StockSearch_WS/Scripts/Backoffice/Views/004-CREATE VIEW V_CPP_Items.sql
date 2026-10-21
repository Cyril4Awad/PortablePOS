SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
GO
  
CREATE OR ALTER VIEW V_CPP_Items
AS
    SELECT  STK_RECID itemRecID ,
            STK_REF itemReference ,
            STK_SingleRef2 itemBarcode ,
            STK_DESC itemDescription ,
            STK_VATTYPE itemVatType ,
            STK_VATPER itemVatPerc ,
            STK_ITMTYPE itemProductGroup ,
			CASE WHEN SP_PromotionCriteria = 'C' THEN STK_CATEGORY ELSE CONVERT(VARCHAR, STK_ITMIDENT) END itemPromotionCriteria,
            STK_UNIT itemUnit ,
            CONVERT(NUMERIC(18, 0), ISNULL(SP_LCTTC, 0)) itemPrice ,
            'LBP' itemCurrency ,
            ISNULL(stk04_stkqty.STQ_AVAILABLE, 0) itemAvailableQty ,
            STK_RecalculateLC itemRecalculateLC ,
			CONCAT(STK_REF, '|', STK_SingleRef2, '|', STK_DESC) itemSearchString
    FROM    STK04_STKFIL
            INNER JOIN dbo.Stk04_Category ON Stk04_Category.Cat_Code = STK04_STKFIL.STK_CATEGORY
            LEFT JOIN ( SELECT  SP_StkRecId ,
                                ISNULL(SP_LCTTC, 0) SP_LCTTC
                        FROM    Stk04_stkprices
                        WHERE   ISNULL(SP_StartingQty, 0) = 0 AND EXISTS ( SELECT TOP 1
                                                1
                                         FROM   dbo.Stk04_SalPriceType
                                         WHERE  ISNULL(Price_Default, 'N') = 'Y'
                                                AND SP_PriceType = Price_Code )
                      ) stk04_stkprices ON SP_StkRecId = STK_RECID
            LEFT JOIN ( SELECT  STQ_STKRECID ,
                                ISNULL(SUM(STQ_AVAILABLE), 0) STQ_AVAILABLE
                        FROM    dbo.stk04_stkqty
                        GROUP BY STQ_STKRECID
                      ) stk04_stkqty ON stk04_stkqty.STQ_STKRECID = STK04_STKFIL.STK_RECID
			INNER JOIN (SELECT ISNULL(SP_PromotionCriteria, 'C') SP_PromotionCriteria FROM dbo.stk04_SysParam) sysParam ON 1=1
    WHERE   ISNULL(STK_ACTIVE, 'N') = 'Y'
            AND ISNULL(STK_SendToPOS, 'N') = 'Y'
            --AND ISNULL(STK_cashvan, 'N') = 'Y'
            AND STK_CurrentArchive = 'C';
 


GO
