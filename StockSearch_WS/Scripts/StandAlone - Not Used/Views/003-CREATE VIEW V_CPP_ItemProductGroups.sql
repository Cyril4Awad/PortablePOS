SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
GO
CREATE VIEW V_CPP_ItemProductGroups
AS
    SELECT  IT_Code [itemProductGroupCode] ,
            IT_DESC [itemProductGroupDesc]
    FROM    dbo.STK04_ItemType
    WHERE   ISNULL(IT_ACTIVE, 'N') = 'Y'
            AND EXISTS ( SELECT TOP 1
                                1
                         FROM   POS_STKFIL
                         WHERE  STK_ITMTYPE = IT_Code
                                AND ISNULL(STK_SendToPOS, 'N') = 'Y'
                                AND ISNULL(STK_cashvan, 'N') = 'Y'
                                AND STK_CurrentArchive = 'C' );



GO
