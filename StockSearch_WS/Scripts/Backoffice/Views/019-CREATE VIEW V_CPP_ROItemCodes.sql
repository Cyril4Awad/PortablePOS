SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
GO

-- Every code a scan can match for the Receiving Order (same as itemValidation) :
-- R = item reference, 1 = single reference 1, 2 = single reference 2, B = barcode (STK04_DetBarcode, with its expiry / lot),
-- S = supplier reference (STK04_StkSupRef, supplierRecID is its supplier)
-- codeExpiry : DD.MM.YYYY (00 for the day = the whole month)
CREATE OR ALTER VIEW V_CPP_ROItemCodes
AS
    SELECT  STK_RECID [itemRecID] , LTRIM(RTRIM(STK_REF)) [itemCode] , 'R' [codeType] , CONVERT(NUMERIC(18, 0), NULL) [supplierRecID] ,
            CONVERT(VARCHAR(10), '') [codeExpiry] , CONVERT(VARCHAR(15), '') [codeBatch]
    FROM    dbo.STK04_STKFIL
    WHERE   ISNULL(STK_REF, '') <> ''
    UNION ALL
    SELECT  STK_RECID , LTRIM(RTRIM(STK_SingleRef1)) , '1' , NULL , '' , ''
    FROM    dbo.STK04_STKFIL
    WHERE   ISNULL(STK_SingleRef1, '') <> ''
    UNION ALL
    SELECT  STK_RECID , LTRIM(RTRIM(STK_SingleRef2)) , '2' , NULL , '' , ''
    FROM    dbo.STK04_STKFIL
    WHERE   ISNULL(STK_SingleRef2, '') <> ''
    UNION ALL
    SELECT  dbar_stkid , LTRIM(RTRIM(dbar_barcode)) , 'B' , NULL , ISNULL(dbar_expdate, '') , ISNULL(dbar_batch, '')
    FROM    dbo.STK04_DetBarcode
    WHERE   ISNULL(dbar_barcode, '') <> ''
    UNION ALL
    SELECT  SRef_StkRecId , LTRIM(RTRIM(SRef_SupRef)) , 'S' , SRef_SupRecId , '' , ''
    FROM    dbo.STK04_StkSupRef
    WHERE   ISNULL(SRef_SupRef, '') <> '';


GO
