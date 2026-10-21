SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
GO

-- The lines of the purchase orders with the qty already received by the receiving orders (FillGridDetails of FrmDO)
-- itemRecID is NULL for an item of the pending stock file (it cannot be received from the portable)
CREATE OR ALTER VIEW V_CPP_ROPurchaseOrderLines
AS
    SELECT  PO.MOD_HRecid [poRecID] ,
            PO.MOD_LineNO [poLineNo] ,
            REPLACE(REPLACE(LTRIM(RTRIM(PO.MOD_SupItmRef)), CHAR(13), ''), CHAR(10), '') [supplierRef] ,
            ISNULL(PO.MOD_Stat, 'N') [lineStatus] ,
            PO.MOD_RMRecid [itemRecID] ,
            PO.MOD_PendRecid [pendingRecID] ,
            PO.MOQty [orderedQty] ,
            ISNULL(RO.DOD_Qty, 0) [receivedQty] ,
            PO.MOQty - ISNULL(RO.DOD_Qty, 0) [remainingQty] ,
            ISNULL(PO.MOD_UnitCost, 0) [unitCost]
    FROM    dbo.V_PORD2_MOSTKQty1 PO
            LEFT JOIN dbo.V_PORD2_DOSTKQty1 RO ON PO.MOD_HRecid = RO.DOH_PORecid
                                                  AND RO.dod_itmRef = PO.MOD_SupItmRef
                                                  AND RO.DOD_StkRecid = PO.MOD_RMRecid
                                                  AND RO.DOD_Stat = PO.MOD_Stat;


GO
