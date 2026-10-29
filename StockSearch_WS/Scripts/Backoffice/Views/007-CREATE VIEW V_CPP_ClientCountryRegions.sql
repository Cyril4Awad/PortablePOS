SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE OR ALTER VIEW V_CPP_ClientCountryRegions
AS
    SELECT  CONCAT(Cou_CouCode, ' / ', Cou_RegCode) [clientCountryRegionsCode] ,
            CONCAT(Cou_CouName, ' / ', Cou_RegName) [clientCountryRegionsDesc]
    FROM    dbo.Com_Country
    WHERE   EXISTS ( SELECT TOP 1
                            1
                     FROM   dbo.V_CPP_Clients
                     WHERE  clientCountryRegion = CONCAT(Cou_CouCode, ' / ',
                                                         Cou_RegCode) );

GO
