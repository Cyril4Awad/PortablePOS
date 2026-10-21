SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
GO

-- The suppliers Free Receiving can receive from (active only)
CREATE OR ALTER VIEW V_CPP_ReceivingSuppliers
AS
    SELECT  Sup_RecId [supplierRecID] ,
            LTRIM(RTRIM(Sup_Code)) [supplierCode] ,
            LTRIM(RTRIM(ISNULL(Sup_Name, ''))) [supplierName]
    FROM    dbo.stk04_supplier
    WHERE   ISNULL(Sup_Active, 'N') = 'Y';


GO
