SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
GO

-- The locations Transfer works with (the location we transfer from and the destinations) : not the head office type (Loc_Type1 = H)
CREATE OR ALTER VIEW V_CPP_TransferLocations
AS
    SELECT  LTRIM(RTRIM(Loc_Code)) [locationCode] ,
            LTRIM(RTRIM(ISNULL(Loc_Desc, ''))) [locationDescription]
    FROM    dbo.STK04_Location
    WHERE   ISNULL(Loc_Type1, '') <> 'H'
            AND ISNULL(Loc_Active, 'Y') = 'Y';


GO
