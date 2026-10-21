SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
GO

-- The barcodes of the items (STK04_DetBarcode), with the expiry / lot of the expiry items,
-- read by the portable (Receiving Order)
CREATE OR ALTER VIEW V_CPP_ItemBarcodes
AS
    SELECT  dbar_recid [barcodeRecID] ,
            dbar_stkid [itemRecID] ,
            dbar_grid [barcodeGrid] ,
            dbar_Detgrid [barcodeSize] ,
            dbar_color [barcodeColor] ,
            dbar_expdate [barcodeExpiry] ,
            dbar_batch [barcodeBatch] ,
            dbar_barcode [barcode] ,
            dbar_DSTMP [barcodeDStamp] ,
            dbar_USTMP [barcodeUStamp] ,
            dbar_CrDSTMP [barcodeCreateDate] ,
            dbar_CrUSTMP [barcodeCreateUser]
    FROM    dbo.STK04_DetBarcode;


GO
