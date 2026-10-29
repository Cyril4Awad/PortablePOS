SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
GO

-- The companies of the ERP (Negations tab of the backend)
CREATE OR ALTER VIEW V_CPP_Companies
AS
    SELECT  LTRIM(RTRIM(Comp_Code)) [companyCode] ,
            LTRIM(RTRIM(ISNULL(Comp_Name, ''))) [companyName]
    FROM    dbo.Com_Company
GO
