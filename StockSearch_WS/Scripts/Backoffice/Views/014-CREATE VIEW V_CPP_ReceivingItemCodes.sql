SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
GO

-- The barcodes and the supplier references of the active items, downloaded by Free Receiving
-- codeKind : barcode | supplierRef
-- codeRecID : unique in its kind, the download pages follow it
-- codeSupplierRecID : the supplier of a supplier reference (0 for a barcode)
-- codeExpiry / codeBatch : the expiry (DD.MM.YYYY, 00 for the whole month) and lot of a barcode ('' when it has none), like V_CPP_ROItemCodes
CREATE OR ALTER VIEW V_CPP_ReceivingItemCodes
AS
    SELECT  'barcode' [codeKind] ,
            dbar_recid [codeRecID] ,
            dbar_stkid [itemRecID] ,
            LTRIM(RTRIM(dbar_barcode)) [codeValue] ,
            CONVERT(NUMERIC(18, 0), 0) [codeSupplierRecID] ,
            CONVERT(VARCHAR(10), ISNULL(dbar_expdate, '')) [codeExpiry] ,
            CONVERT(VARCHAR(15), ISNULL(dbar_batch, '')) [codeBatch]
    FROM    dbo.STK04_DetBarcode
            INNER JOIN dbo.STK04_STKFIL ON STK_RECID = dbar_stkid
    WHERE   ISNULL(STK_ACTIVE, 'N') = 'Y'
            AND STK_CurrentArchive = 'C'
            AND ISNULL(dbar_barcode, '') <> ''
    UNION ALL
    SELECT  'supplierRef' ,
            SREF_RecId ,
            SRef_StkRecId ,
            LTRIM(RTRIM(SRef_SupRef)) ,
            ISNULL(SRef_SupRecId, 0) ,
            '' ,
            ''
    FROM    dbo.STK04_StkSupRef
            INNER JOIN dbo.STK04_STKFIL ON STK_RECID = SRef_StkRecId
    WHERE   ISNULL(STK_ACTIVE, 'N') = 'Y'
            AND STK_CurrentArchive = 'C'
            AND ISNULL(SRef_SupRef, '') <> '';


GO
