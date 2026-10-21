SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
GO

-- The stock of the active items in each location, one row per lot (expiry / lot, color / grid / size)
-- lotExpiry : DD.MM.YYYY, MM.YYYY when the day is 00
CREATE OR ALTER VIEW V_CPP_TransferStock
AS
    SELECT  STQ_RecId [lotRecID] ,
            STQ_STKRECID [itemRecID] ,
            STQ_LOCCODE [locationCode] ,
            CASE WHEN LEFT(ISNULL(STQ_ExpiryDt, ''), 3) = '00.' THEN SUBSTRING(STQ_ExpiryDt, 4, 7)
                 ELSE LTRIM(RTRIM(ISNULL(STQ_ExpiryDt, '')))
            END [lotExpiry] ,
            LTRIM(RTRIM(ISNULL(STQ_BatchInfo, ''))) [lotBatch] ,
            LTRIM(RTRIM(ISNULL(STQ_Color, ''))) [lotColor] ,
            LTRIM(RTRIM(ISNULL(STQ_Grid, ''))) [lotGrid] ,
            LTRIM(RTRIM(ISNULL(STQ_DetGrid, ''))) [lotSize] ,
            ISNULL(STQ_AVAILABLE, 0) [lotAvailable]
    FROM    dbo.STK04_STKQTY
            INNER JOIN dbo.STK04_STKFIL ON STK_RECID = STQ_STKRECID
    WHERE   ISNULL(STK_ACTIVE, 'N') = 'Y'
            AND STK_CurrentArchive = 'C';


GO
