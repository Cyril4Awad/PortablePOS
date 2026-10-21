SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
GO

-- The stock qty of the items per location and lot (stk04_stkqty), read by the portable (Receiving Order) :
-- stockExpiry : DD.MM.YYYY (00 for the day = the whole month)
CREATE OR ALTER VIEW V_CPP_StockQty
AS
    SELECT  STQ_STKRECID [itemRecID] ,
            STQ_LOCCODE [locationCode] ,
            STQ_BBFQTY [stockBBFQty] ,
            STQ_BBFDATE [stockBBFDate] ,
            STQ_BBFSRC [stockBBFSource] ,
            STQ_RESERVED [stockReserved] ,
            STQ_AVAILABLE [stockAvailable] ,
            STQ_DSTAMP [stockDStamp] ,
            STQ_USTAMP [stockUStamp] ,
            STQ_RecId [stockRecID] ,
            STQ_Color [stockColor] ,
            STQ_Grid [stockGrid] ,
            STQ_DetGrid [stockSize] ,
            STQ_ExpiryDt [stockExpiry] ,
            STQ_BatchInfo [stockBatch] ,
            STQ_TransPendO [stockTransferOut] ,
            STQ_TransPendI [stockTransferIn] ,
            STQ_ReturnSale [stockReturnSale] ,
            STQ_ProcProduction [stockInProduction] ,
            STQ_ProductionDt [stockProductionDate]
    FROM    dbo.stk04_stkqty;


GO
