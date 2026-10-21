SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
GO

-- The locations a Receiving Order can receive in (LocationStrsql of FrmDO : active, not the 00 location)
CREATE OR ALTER VIEW V_CPP_ROLocations
AS
    SELECT  LTRIM(RTRIM(Loc_Code)) [locationCode] ,
            LTRIM(RTRIM(ISNULL(Loc_Desc, ''))) [locationDescription]
    FROM    dbo.STK04_Location
    WHERE   ISNULL(Loc_Active, 'N') = 'Y'
            AND Loc_Code <> '00';


GO
