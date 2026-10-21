SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
GO

-- The item tags (STK_NEWUSED) each user can work with in each application (AppStkCond)
CREATE OR ALTER VIEW V_CPP_UserItemTags
AS
    SELECT  UPPER(LTRIM(RTRIM(ASP_USER))) [userCode] ,
            LTRIM(RTRIM(ASP_APPcODE)) [appCode] ,
            ISNULL(ASP_STKTAG, '') [itemTag]
    FROM    dbo.STK04_APPSTKPRIV;


GO
