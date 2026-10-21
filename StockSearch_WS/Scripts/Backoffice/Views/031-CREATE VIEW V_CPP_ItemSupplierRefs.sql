SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
GO

-- The supplier references of the items (STK04_StkSupRef), read by the portable (Receiving Order) :
-- refSource : DO = added by a receiving order
CREATE OR ALTER VIEW V_CPP_ItemSupplierRefs
AS
    SELECT  SRef_StkRecId [itemRecID] ,
            SRef_SupRecId [supplierRecID] ,
            SRef_SupRef [supplierRef] ,
            SRef_Source [refSource] ,
            SRef_UStamp [refUStamp] ,
            SRef_DStamp [refDStamp] ,
            SRef_SupColor [refSupplierColor] ,
            SRef_ItmColor [refColor] ,
            SREF_RecId [refRecID]
    FROM    dbo.STK04_StkSupRef;


GO
