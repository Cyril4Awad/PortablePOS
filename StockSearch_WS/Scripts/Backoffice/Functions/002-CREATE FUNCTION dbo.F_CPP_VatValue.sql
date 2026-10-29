SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
GO

CREATE OR ALTER FUNCTION dbo.F_CPP_VatValue
    (
      @Amount NUMERIC(18, 3) ,
      @LocationCode VARCHAR(2) ,
      @ItemVatType VARCHAR(1) ,
      @ItemVatPerc DECIMAL(6, 2) ,
      @ClientPriceCat VARCHAR(1)
    )
RETURNS NUMERIC(18, 3)
    BEGIN

        DECLARE @ValValue NUMERIC(18, 3);
    
        SELECT  @ValValue = dbo.GetVatValue(@Amount, @ItemVatType,
                                            @ItemVatPerc, @ClientPriceCat,
                                            dbo.F_CPP_VatType(Loc_Code,
                                                              @ClientPriceCat),
                                            CASE WHEN Loc_Type1 = 'P' THEN 'R'
                                                 ELSE Loc_Type1
                                            END, Loc_VATRetType, Loc_Vat,
                                            Loc_VATWType)
        FROM    STK04_Location
        WHERE   Loc_Code = @LocationCode;

        RETURN ISNULL(@ValValue,0) ;

    END;




GO