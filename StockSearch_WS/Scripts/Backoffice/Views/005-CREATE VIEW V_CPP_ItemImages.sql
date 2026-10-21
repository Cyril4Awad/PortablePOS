SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
GO

CREATE VIEW V_CPP_ItemImages
AS
    SELECT  DSTech_RecId [itemRecID] ,
            DSTech_BMP [itemImage]
    FROM    dbo.STK04_DSTechnical; 


GO
