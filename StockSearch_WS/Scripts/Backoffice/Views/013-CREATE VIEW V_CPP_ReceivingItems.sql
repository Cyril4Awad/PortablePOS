SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
GO

-- The stock file downloaded by Free Receiving (active items only)
CREATE OR ALTER VIEW V_CPP_ReceivingItems
AS
    SELECT  STK_RECID [itemRecID] ,
            LTRIM(RTRIM(STK_REF)) [itemReference] ,
            LTRIM(RTRIM(ISNULL(STK_DESC, ''))) [itemDescription] ,
            LTRIM(RTRIM(ISNULL(STK_SUPPLIER, ''))) [itemSupplier] ,
            LTRIM(RTRIM(ISNULL(STK_CATEGORY, ''))) [itemCategory] ,
            ISNULL(CONVERT(VARCHAR(18), STK_SERIE), '') [itemSerie] ,
            LTRIM(RTRIM(ISNULL(STK_SingleRef1, ''))) [itemSingleRef1] ,
            LTRIM(RTRIM(ISNULL(STK_SingleRef2, ''))) [itemSingleRef2] ,
            LTRIM(RTRIM(ISNULL(STK_UNIT, ''))) [itemUnit]
    FROM    dbo.STK04_STKFIL
    WHERE   ISNULL(STK_ACTIVE, 'N') = 'Y'
            AND STK_CurrentArchive = 'C';


GO
