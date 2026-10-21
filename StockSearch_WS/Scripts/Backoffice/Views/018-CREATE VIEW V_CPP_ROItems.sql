SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
GO

-- The items a Receiving Order can receive (same conditions as FrmDO "Receiving Order") :
-- active, current, trading / barcode / expiry items, not consignment, not in production use, not a returned book,
-- not the normal item of a consignment pair (V_StockNormalCons)
-- itemTag (STK_NEWUSED) and itemBrand : checked against the user privileges (V_CPP_UserItemTags, V_CPP_UserPurchaseAccess)
CREATE OR ALTER VIEW V_CPP_ROItems
AS
    SELECT  STK_RECID [itemRecID] ,
            LTRIM(RTRIM(ISNULL(STK_REF, ''))) [itemReference] ,
            LTRIM(RTRIM(ISNULL(STK_SingleRef1, ''))) [itemSingleRef1] ,
            LTRIM(RTRIM(ISNULL(STK_SingleRef2, ''))) [itemBarcode] ,
            LTRIM(RTRIM(ISNULL(STK_DESC, ''))) [itemDescription] ,
            UPPER(STK_ItmClass) [itemClass] ,
            STK_UNIT [itemUnitCode] ,
            LTRIM(RTRIM(ISNULL(MEASU.Meas_Desc, ''))) [itemUnit] ,
            LTRIM(RTRIM(ISNULL(MEASUB.Meas_Desc, ''))) [itemSubUnit] ,
            ISNULL(STK_VATTYPE, '') [itemVatType] ,
            ISNULL(STK_AVGCOSTCC, 0) [itemCostCC] ,
            ISNULL(STK_FCCODE, '') [itemCurrency] ,
            STK_BBFDATE [itemBBFDate] ,
            STK_HaltActTill [itemHaltTill] ,
            UPPER(ISNULL(STK_HaltPoPi, 'N')) [itemHaltPurchase] ,
            UPPER(ISNULL(STK_QTYCTRL, 'N')) [itemQtyControl] ,
            ISNULL(STK_NEWUSED, '') [itemTag] ,
            ISNULL(STK_Brand, '') [itemBrand] ,
            ISNULL(STK_ITMTYPE, '') [itemProductGroup] ,
            LTRIM(RTRIM(ISNULL(IT_DESC, ISNULL(STK_ITMTYPE, '')))) [itemProductGroupDesc] ,
            STK_ReturnSale [itemReturnSale] ,
            ISNULL(STK_UNITPACK, 1) [itemUnitPack] ,
            ISNULL(STK_OPERATOR, '*') [itemOperator] ,
            ISNULL(MEASUB.Meas_Sign, '*') [itemMeasSign] ,
            ISNULL(MEASUB.Meas_Value, 1) [itemMeasValue]
    FROM    dbo.STK04_STKFIL
            INNER JOIN dbo.stk04_measurements MEASU ON MEASU.Meas_unit = STK_UNIT
            INNER JOIN dbo.stk04_measurements MEASUB ON MEASUB.Meas_unit = STK_SUBUNIT
            LEFT JOIN dbo.bibl1_Book ON STK_BOOKID = BO_Recid
            LEFT JOIN dbo.STK04_ItemType ON IT_Code = STK_ITMTYPE
    WHERE   ISNULL(STK_ACTIVE, 'N') = 'Y'
            AND STK_CurrentArchive = 'C'
            AND STK_ItmClass IN ( 'B', 'E', 'T' )
            AND ISNULL(STK_NEWUSED, '') <> 'C'
            AND ISNULL(STK_USAGE, '') <> 'P'
            AND ISNULL(BO_Type, '') <> 'R'
            AND NOT EXISTS ( SELECT TOP 1 1 FROM dbo.V_StockNormalCons WHERE NSTK_RECID = STK_RECID );


GO
