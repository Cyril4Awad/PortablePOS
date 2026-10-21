SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
GO

-- The suppliers a Receiving Order can receive from (active), supplierForeign : 1 = foreign supplier (Sup_LocFor = F)
CREATE OR ALTER VIEW V_CPP_ROSuppliers
AS
    SELECT  Sup_RecId [supplierRecID] ,
            LTRIM(RTRIM(Sup_Code)) [supplierCode] ,
            LTRIM(RTRIM(ISNULL(Sup_Name, ''))) [supplierName] ,
            CASE WHEN UPPER(ISNULL(Sup_LocFor, '')) = 'F' THEN 1 ELSE 0 END [supplierForeign] ,
            LTRIM(RTRIM(ISNULL(Sup_Country, ''))) [supplierCountry] ,
            LTRIM(RTRIM(ISNULL(Sup_Curr, ''))) [supplierCurrency]
    FROM    dbo.stk04_supplier
    WHERE   ISNULL(Sup_Active, 'N') = 'Y';


GO
