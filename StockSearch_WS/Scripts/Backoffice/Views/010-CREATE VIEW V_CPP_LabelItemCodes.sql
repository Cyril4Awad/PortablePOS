SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
GO

-- The codes a label can print as its barcode / QR, they are also the codes a scan or a search matches
-- codeGroup : coding (the references of the item), barcodes, supplier (the supplier references)
-- codeIsMainBarcode : the code printed by default when the scanned one is not a code of the item
-- codeSort : the order the codes are shown in
CREATE OR ALTER VIEW V_CPP_LabelItemCodes
AS
    SELECT  STK_RECID [itemRecID] ,
            'coding' [codeGroup] ,
            'Reference' [codeTitle] ,
            STK_REF [codeValue] ,
            0 [codeIsMainBarcode] ,
            1 [codeSort]
    FROM    dbo.STK04_STKFIL
    UNION ALL
    SELECT  STK_RECID ,
            'coding' ,
            'Single Ref 1' ,
            STK_SingleRef1 ,
            0 ,
            2
    FROM    dbo.STK04_STKFIL
    UNION ALL
    SELECT  STK_RECID ,
            'coding' ,
            'Single Ref 2' ,
            STK_SingleRef2 ,
            1 ,
            3
    FROM    dbo.STK04_STKFIL
    UNION ALL
    SELECT  dbar_stkid ,
            'barcodes' ,
            'Barcode' ,
            dbar_barcode ,
            0 ,
            4
    FROM    dbo.STK04_DetBarcode
    UNION ALL
    SELECT  SRef_StkRecId ,
            'supplier' ,
            Sup_Name ,
            SRef_SupRef ,
            0 ,
            5
    FROM    dbo.STK04_StkSupRef
            INNER JOIN dbo.stk04_supplier ON SRef_SupRecId = Sup_RecId;


GO
