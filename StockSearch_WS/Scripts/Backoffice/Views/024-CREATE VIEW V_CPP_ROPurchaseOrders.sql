SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
GO

-- The purchase orders a Receiving Order can import (FrmDO) : under process, not fashion, not invoiced,
-- approved when the supplier needs an approval. The PO must still have a qty to receive (V_CPP_ROPurchaseOrderLines)
CREATE OR ALTER VIEW V_CPP_ROPurchaseOrders
AS
    SELECT DISTINCT
            RMO_RECID [poRecID] ,
            RMO_DATE [poDate] ,
            rmo_comp [companyCode] ,
            Sup_RecId [supplierRecID] ,
            LTRIM(RTRIM(SUP_CODE)) [supplierCode] ,
            LTRIM(RTRIM(ISNULL(RMO_ManualNo, ''))) [poManualNo] ,
            rmo_DELdATE [poDeliveryDate] ,
            ISNULL(RMO_tOToRDvAL, 0) [poTotal] ,
            LTRIM(RTRIM(ISNULL(rmo_cUR, ''))) [poCurrency] ,
            LTRIM(RTRIM(ISNULL(RMO_REMARK, ''))) [poRemark]
    FROM    dbo.V_PORD2_ManOrdList
    WHERE   RMO_POStatus = 'U'
            AND ISNULL(RMO_POType, '') <> 'F'
            AND ConfPI = 0
            AND ( ( Conf <> 0 AND sup_approve = 'Y' ) OR sup_approve = 'N' );


GO
