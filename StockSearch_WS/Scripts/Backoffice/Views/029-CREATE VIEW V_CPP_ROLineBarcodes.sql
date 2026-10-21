SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
GO

-- Receiving Order barcodes of the lines (stk04_PurDelOrdDBE), with the expiry / lot for the expiry items,
-- read by the portable
CREATE OR ALTER VIEW V_CPP_ROLineBarcodes
AS
    SELECT  DOB_RecID [barcodeRecID] ,
            DOB_Drecid [lineRecID] ,
            DOB_Barcode [barcode] ,
            DOB_ExpiryDate [lotExpiry] ,
            DOB_BatchInfo [lotBatch] ,
            DOB_GridCode [lotGrid] ,
            DOB_GridSize [lotSize] ,
            DOB_CrDstamp [barcodeCreateDate] ,
            DOB_CrUstamp [barcodeCreateUser] ,
            DOB_Dstamp [barcodeDStamp] ,
            DOB_Ustamp [barcodeUStamp]
    FROM    dbo.stk04_PurDelOrdDBE;


GO
