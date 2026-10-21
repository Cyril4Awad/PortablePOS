SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
GO

-- Receiving Order qty per expiry / lot of the expiry items (stk04_PurDelOrdDQty), read by the portable :
-- lotExpiry : DD.MM.YYYY (00 for the day = the whole month)
CREATE OR ALTER VIEW V_CPP_ROLineLots
AS
    SELECT  DOQ_RecID [lotRecID] ,
            DOQ_Drecid [lineRecID] ,
            DOQ_ExpiryDate [lotExpiry] ,
            DOQ_BatchInfo [lotBatch] ,
            DOQ_GridCode [lotGrid] ,
            DOQ_GridSize [lotSize] ,
            DOQ_Recqty [lotQty] ,
            DOQ_CrDstamp [lotCreateDate] ,
            DOQ_CrUstamp [lotCreateUser] ,
            DOQ_Dstamp [lotDStamp] ,
            DOQ_Ustamp [lotUStamp]
    FROM    dbo.stk04_PurDelOrdDQty;


GO
