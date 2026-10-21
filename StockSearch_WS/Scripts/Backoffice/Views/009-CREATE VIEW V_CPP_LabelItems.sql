SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
GO

CREATE OR ALTER VIEW V_CPP_LabelItems
AS
    SELECT  STK_RECID [itemRecID] ,
            LTRIM(RTRIM(STK_REF)) [itemReference] ,
            LTRIM(RTRIM(ISNULL(STK_DESC, ''))) [itemDescription] ,
            LTRIM(RTRIM(ISNULL(STK_UNIT, ''))) [itemUnit]
    FROM    dbo.STK04_STKFIL
    WHERE   ISNULL(STK_ACTIVE, 'N') = 'Y'
            AND STK_CurrentArchive = 'C';


GO
