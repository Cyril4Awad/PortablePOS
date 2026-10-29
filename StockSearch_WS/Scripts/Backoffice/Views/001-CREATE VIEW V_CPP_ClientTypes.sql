SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
GO

CREATE OR ALTER VIEW V_CPP_ClientTypes
AS
    SELECT  ClTp_Code [clientTypeCode] ,
            ClTp_Desc [clientTypeDesc]
    FROM    dbo.Stk04_ClientType
    WHERE   ISNULL(ClTp_Active, 'N') = 'Y';


GO
