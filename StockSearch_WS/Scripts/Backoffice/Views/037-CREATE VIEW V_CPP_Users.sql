SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
GO

-- The users of the ERP (User Privilege tab of the backend), userActive = 'Y' : the user can log in
CREATE OR ALTER VIEW V_CPP_Users
AS
    SELECT  LTRIM(RTRIM(User_Code)) [userCode] ,
            LTRIM(RTRIM(ISNULL(User_Name, ''))) [userName] ,
            CASE WHEN ISNULL(User_Status, 'I') = 'A' THEN 'Y' ELSE 'N' END [userActive]
    FROM    dbo.Com_UserInfo
GO
