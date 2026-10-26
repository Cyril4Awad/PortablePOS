SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
GO

-- The items Transfer can send (TransferAndIBR_StockFile of frmTransferStock) : active, sent to the POS, with a brand,
-- not halted for transfers (or the halt is over). itemFourthParty = 'Y' : a fourth party item, it cannot be transacted
-- itemProductGroup : the item type (the product group of the stock search)
CREATE OR ALTER VIEW V_CPP_TransferItems
AS
    SELECT  STK_RECID [itemRecID] ,
            LTRIM(RTRIM(STK_REF)) [itemReference] ,
            LTRIM(RTRIM(ISNULL(STK_DESC, ''))) [itemDescription] ,
            LTRIM(RTRIM(ISNULL(br_Name, ''))) [itemBrandName] ,
            LTRIM(RTRIM(ISNULL(STK_SingleRef1, ''))) [itemSingleRef1] ,
            LTRIM(RTRIM(ISNULL(STK_SingleRef2, ''))) [itemSingleRef2] ,
            LTRIM(RTRIM(ISNULL(MEAS_DESC, ISNULL(STK_UNIT, '')))) [itemUnit] ,
            ISNULL(STK_ItmClass, 'T') [itemClass] ,
            ISNULL(STK_QTYCTRL, 'Y') [itemQtyControl] ,
            ISNULL(STK_SERVICE, 'R') [itemService] ,
            CASE WHEN ISNULL(STK_FourthParty, 'N') = 'Y' THEN 'Y' ELSE 'N' END [itemFourthParty] ,
            LTRIM(RTRIM(ISNULL(STK_ITMTYPE, ''))) [itemProductGroup] ,
            LTRIM(RTRIM(ISNULL(IT_DESC, ''))) [itemProductGroupDesc]
    FROM    dbo.STK04_STKFIL
            INNER JOIN dbo.Stk04_Brand ON br_Code = STK_Brand
            LEFT JOIN dbo.STK04_Measurements ON MEAS_UNIT = STK_UNIT
            LEFT JOIN dbo.STK04_ItemType ON IT_Code = STK_ITMTYPE
    WHERE   ISNULL(STK_ACTIVE, 'N') = 'Y'
            AND ISNULL(STK_SendToPOS, 'N') = 'Y'
            AND ( ISNULL(STK_HaltTransfers, 'N') = 'N'
                  OR ( STK_HaltTransfers = 'Y' AND STK_HaltActTill < CONVERT(DATE, GETDATE()) ) );


GO
