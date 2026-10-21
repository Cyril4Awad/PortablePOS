SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
GO

-- The items of the pending stock file a Receiving Order can receive (btnPending of FrmDO "Receiving Order") :
-- trading / barcode / expiry items, not consignment. Same columns as V_CPP_ROItems : the item is created in the stock file
-- when the receiving order is saved (InsertStkFil), its reference will be single reference 1 / new-used (STK_REF)
-- itemRecID = the record id of the pending item (SFP_Recid)
CREATE OR ALTER VIEW V_CPP_ROPendingItems
AS
    SELECT  SFP_Recid [itemRecID] ,
            LTRIM(RTRIM(CONCAT(SFP_SingleRef1, '/', SFP_NEWUSED))) [itemReference] ,
            LTRIM(RTRIM(ISNULL(SFP_SingleRef1, ''))) [itemSingleRef1] ,
            LTRIM(RTRIM(ISNULL(SFP_SingleRef2, ''))) [itemBarcode] ,
            LTRIM(RTRIM(ISNULL(SFP_Desc, ''))) [itemDescription] ,
            UPPER(SFP_ItmClass) [itemClass] ,
            SFP_UNIT [itemUnitCode] ,
            LTRIM(RTRIM(ISNULL(MEASU.Meas_Desc, ''))) [itemUnit] ,
            LTRIM(RTRIM(ISNULL(MEASUB.Meas_Desc, ''))) [itemSubUnit] ,
            ISNULL(SFP_VATType, '') [itemVatType] ,
            CONVERT(DECIMAL(18, 3), 0) [itemCostCC] ,
            ISNULL(SFP_FCCode, '') [itemCurrency] ,
            CONVERT(DATETIME, NULL) [itemBBFDate] ,
            CONVERT(DATETIME, NULL) [itemHaltTill] ,
            'N' [itemHaltPurchase] ,
            'N' [itemQtyControl] ,
            ISNULL(SFP_NEWUSED, '') [itemTag] ,
            ISNULL(SFP_Brand, '') [itemBrand] ,
            ISNULL(SFP_ItmType, '') [itemProductGroup] ,
            LTRIM(RTRIM(ISNULL(IT_DESC, ISNULL(SFP_ItmType, '')))) [itemProductGroupDesc] ,
            'Y' [itemReturnSale] ,
            ISNULL(SFP_UNITPACK, 1) [itemUnitPack] ,
            ISNULL(SFP_operator, '*') [itemOperator] ,
            ISNULL(MEASUB.Meas_Sign, '*') [itemMeasSign] ,
            ISNULL(MEASUB.Meas_Value, 1) [itemMeasValue] ,
            LTRIM(RTRIM(ISNULL(SFP_OpSource, ''))) [pendingSource]
    FROM    dbo.STK04_StkFilPending
            INNER JOIN dbo.stk04_measurements MEASU ON MEASU.Meas_unit = SFP_UNIT
            INNER JOIN dbo.stk04_measurements MEASUB ON MEASUB.Meas_unit = SFP_SUBUNIT
            LEFT JOIN dbo.STK04_ItemType ON IT_Code = SFP_ItmType
    WHERE   SFP_ItmClass IN ( 'B', 'E', 'T' )
            AND ISNULL(SFP_NEWUSED, '') <> 'C';


GO
