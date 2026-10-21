SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
GO

-- Receiving Order lines (stk04_PurDelOrdD), read by the portable
-- lineStatus : N = normal, F = free, R = return, lineCounter : number of the line among the lines of the same supplier ref and status
CREATE OR ALTER VIEW V_CPP_ROLines
AS
    SELECT  DOD_Recid [lineRecID] ,
            DOD_Hrecid [roRecID] ,
            DOD_ItmRef [supplierRef] ,
            DOD_Stat [lineStatus] ,
            DOD_Counter [lineCounter] ,
            DOD_StkRecid [itemRecID] ,
            DOD_Recqty [lineQty] ,
            DOD_Unit [lineUnitCode] ,
            DOD_ConsQty [lineConsQty] ,
            DOD_AvgCostLC [lineCostLC] ,
            DOD_AvgCostCC [lineCostCC] ,
            DOD_AvgCostFC [lineCostFC] ,
            DOD_vat [lineVatType] ,
            DOD_Dstamp [lineDStamp] ,
            DOD_Ustamp [lineUStamp] ,
            DOD_Color [lineSupplierColor] ,
            DOD_CreateDate [lineCreateDate] ,
            DOD_CreateUser [lineCreateUser] ,
            DOD_ColorCode [lineColor] ,
            DOD_Disc [lineDiscount] ,
            DOD_NetCostLC [lineNetLC] ,
            DOD_NetCostCC [lineNetCC] ,
            DOD_NetCostFC [lineNetFC]
    FROM    dbo.stk04_PurDelOrdD;


GO
