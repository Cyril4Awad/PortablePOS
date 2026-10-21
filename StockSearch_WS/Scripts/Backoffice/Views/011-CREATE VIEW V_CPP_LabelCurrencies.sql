SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
GO

-- The local and the control currency of each company (its last POS year)
CREATE OR ALTER VIEW V_CPP_LabelCurrencies
AS
    SELECT  Ac_Comp [companyCode] ,
            ISNULL(Ac_Curr1, '') [currencyLC] ,
            ISNULL(Ac_Curr2, '') [currencyCC]
    FROM    ( SELECT    Ac_Comp ,
                        Ac_Curr1 ,
                        Ac_Curr2 ,
                        ROW_NUMBER() OVER ( PARTITION BY Ac_Comp ORDER BY Ac_EndDate DESC ) RowNumber
              FROM      dbo.Com_AppComp
              WHERE     Ac_Code = 'POSO'
            ) Com_AppComp
    WHERE   RowNumber = 1;


GO
