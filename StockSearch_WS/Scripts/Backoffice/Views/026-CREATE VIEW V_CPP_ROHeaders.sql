SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
GO

-- Receiving Order header (stk04_PurDelOrdH), read by the portable
-- roStatus : N = normal, R = reverse (the cancel of an RO), roType : T = trading, F = fashion
-- piRecID : the purchase invoice the RO is linked to (an RO linked to a PI cannot be modified or cancelled)
CREATE OR ALTER VIEW V_CPP_ROHeaders
AS
    SELECT  DOH_Recid [roRecID] ,
            DOH_PORecid [poRecID] ,
            DOH_SupRecid [supplierRecID] ,
            DOH_Ref [roRef] ,
            DOH_Date [roDate] ,
            DOH_Status [roStatus] ,
            DOH_loc [locationCode] ,
            DOH_ActDelDate [roActualDate] ,
            DOH_PurRecid [piRecID] ,
            DOH_Dstamp [roDStamp] ,
            DOH_Ustamp [roUStamp] ,
            DOH_Type [roType] ,
            DOH_Brand [roBrand] ,
            DOH_Season [roSeason] ,
            DOH_CreateDate [roCreateDate] ,
            DOH_CreateUser [roCreateUser] ,
            DOH_Curr [roCurrency]
    FROM    dbo.stk04_PurDelOrdH;


GO
