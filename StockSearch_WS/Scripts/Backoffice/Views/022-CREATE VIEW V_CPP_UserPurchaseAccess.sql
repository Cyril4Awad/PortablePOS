SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
GO

-- The purchase privileges of the users (Business Unit Manager, access 1 = purchase) :
-- supplierCode = the user can purchase from this supplier (txtSupp_Validating of FrmDO),
-- brandCode = the user can purchase the items of this brand (AppStkCond with Brand, the row has no supplier)
-- SA and SU are not limited (checked in the code)
CREATE OR ALTER VIEW V_CPP_UserPurchaseAccess
AS
    SELECT  UPPER(LTRIM(RTRIM(BUMP_UserCode))) [userCode] ,
            CASE WHEN ISNULL(BUMP_SuppCode, '') = '' THEN LTRIM(RTRIM(ISNULL(BUMP_BrandCode, ''))) ELSE '' END [brandCode] ,
            LTRIM(RTRIM(ISNULL(BUMP_SuppCode, ''))) [supplierCode]
    FROM    dbo.Stk04_BusUnitManagPriv
    WHERE   BUMP_Access = '1';


GO
