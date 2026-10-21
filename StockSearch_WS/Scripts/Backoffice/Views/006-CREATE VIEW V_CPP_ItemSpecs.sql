SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
GO

CREATE OR ALTER VIEW V_CPP_ItemSpecs
AS
    SELECT  STK_RECID [itemRecID] ,
            DSTech_Meas [Technical Contents] ,
            DSTech_Desc1 [Description 2] ,
            DSTech_Desc2 [Description 3] 
    FROM    STK04_STKFIL
            LEFT JOIN ( SELECT  DSTech_RecId ,
                                DSTech_Meas ,
                                DSTech_Desc1 ,
                                DSTech_Desc2
                        FROM    dbo.STK04_DSTechnical
                      ) STK04_DSTechnical ON STK04_DSTechnical.DSTech_RecId = STK04_STKFIL.STK_RECID;



GO
