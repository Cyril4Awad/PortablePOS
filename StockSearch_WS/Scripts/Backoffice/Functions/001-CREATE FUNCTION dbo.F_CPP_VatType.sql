SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
GO

CREATE FUNCTION dbo.F_CPP_VatType
    (
      @LocationCode VARCHAR(2) ,
      @ClientPriceCat VARCHAR(1)
    )
RETURNS VARCHAR(1)
    BEGIN

		DECLARE @VatType VARCHAR(1);

        SELECT @VatType = CASE WHEN @ClientPriceCat = 'X' THEN 'N'
                     WHEN Loc_Vat = 'Y'
                     THEN CASE CASE WHEN Loc_Type1 = 'P' THEN 'R'
                                    ELSE Loc_Type1
                               END
                            WHEN 'R'
                            THEN CASE WHEN @ClientPriceCat <> 'W'
                                      THEN Loc_VATRetType
                                      ELSE Loc_VATWType
                                 END
                            WHEN 'W' THEN Loc_VATWType
                          END
                     ELSE 'N'
                END
        FROM    dbo.STK04_Location
        WHERE   Loc_Code = @LocationCode;

		RETURN @VatType

    END;


GO