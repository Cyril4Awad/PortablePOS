SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
GO

-- The stock items a Receiving Order can receive from its purchase order (same conditions as the PO import of FrmDO "Receiving Order") :
-- not consignment and with the measurement of its sub unit, active or not, current or archived (FrmDO imports the lines of the inactive items too)
-- trading / barcode / expiry items only (the portable does not receive fashion items)
-- Same columns as V_CPP_ROItems, itemTag (STK_NEWUSED) and itemBrand : checked against the user privileges (V_CPP_UserItemTags, V_CPP_UserPurchaseAccess)
CREATE OR ALTER VIEW V_CPP_ROPurchaseOrderItems
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
            UPPER(ISNULL(MEASUB.Meas_Type, 'P')) [itemMeasType] ,
            CONVERT(DECIMAL(18, 6), CASE WHEN ISNULL(STK_UNITPACK, 1) = 0 OR ISNULL(MEASUB.Meas_Value, 1) = 0 THEN 0
                                         WHEN ISNULL(MEASUB.Meas_Sign, '*') = '/' AND LTRIM(RTRIM(ISNULL(STK_OPERATOR, '*'))) <> '/' THEN ISNULL(STK_UNITPACK, 1) / ISNULL(MEASUB.Meas_Value, 1)
                                         WHEN ISNULL(MEASUB.Meas_Sign, '*') = '/' THEN 1.0 / ISNULL(STK_UNITPACK, 1) / ISNULL(MEASUB.Meas_Value, 1)
                                         WHEN LTRIM(RTRIM(ISNULL(STK_OPERATOR, '*'))) <> '/' THEN ISNULL(STK_UNITPACK, 1) * ISNULL(MEASUB.Meas_Value, 1)
                                         ELSE 1.0 / ISNULL(STK_UNITPACK, 1) * ISNULL(MEASUB.Meas_Value, 1) END) [itemConsFactor] ,
            ISNULL(MEASUB.Meas_Value, 1) [itemMeasValue]
    FROM    dbo.STK04_STKFIL
            LEFT JOIN dbo.stk04_measurements MEASU ON MEASU.Meas_unit = STK_UNIT
            INNER JOIN dbo.stk04_measurements MEASUB ON MEASUB.Meas_unit = STK_SUBUNIT
            LEFT JOIN dbo.STK04_ItemType ON IT_Code = STK_ITMTYPE
    WHERE   STK_ItmClass IN ( 'B', 'E', 'T' )
            AND ISNULL(STK_NEWUSED, '') <> 'C';


GO
