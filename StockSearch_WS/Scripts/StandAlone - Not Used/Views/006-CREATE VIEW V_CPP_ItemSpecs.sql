SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
GO

CREATE VIEW V_CPP_ItemSpecs
AS
    SELECT  STK_RECID [itemRecID] ,
            DSTech_Meas [Technical Contents] ,
            STK_DESCABV [Short Description] ,
            DSTech_Desc1 [Description 2] ,
            DSTech_Desc2 [Description 3] ,
            STK_DESCABV [Desc Abv] ,
            STK_VATTYPE [VAT type] ,
            STK_VATPER [VAT Perc] ,
            STK_VATDATE [VAT Date] ,
            STK_MINQTY [Min QTY] ,
            STK_DSTAMP [Date stamp]
    FROM    dbo.POS_STKFIL STK04_STKFIL
            LEFT JOIN ( SELECT  DSTech_RecId ,
                                DSTech_Meas ,
                                DSTech_Desc1 ,
                                DSTech_Desc2
                        FROM    dbo.STK04_DSTechnical
                      ) STK04_DSTechnical ON STK04_DSTechnical.DSTech_RecId = STK04_STKFIL.STK_RECID;



GO
