SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
GO

CREATE OR ALTER PROCEDURE SP_CPP_GetItemPriceAndDiscount
     @LocationCode VARCHAR(2) ,
     @ClientRecID NUMERIC(18,0) ,
     @ItemRecID NUMERIC(18,0) ,
	 @Currency VARCHAR(3) ,
	 @Qty NUMERIC(18, 3) = 0,
     @WorkingDay DATE = NULL

AS

/* to be used for testing only
DECLARE @LocationCode VARCHAR(2) ;
DECLARE @ClientRecID NUMERIC(18,0) ;
DECLARE @ItemRecID NUMERIC(18,0) ;
DECLARE @Currency VARCHAR(3) ;
DECLARE @WorkingDay DATE = NULL;

SET @LocationCode = 'BA'
SET @ClientRecID = 1
SET @ItemRecID = 6297
SET @Currency = 'LBP'
SET @WorkingDay =  '2023-02-28'
*/

IF @WorkingDay IS NULL
	BEGIN
		SET @WorkingDay = CONVERT(DATE, GETDATE())
	END	

DECLARE @Company VARCHAR(2);
DECLARE @Year VARCHAR(4);
DECLARE @LocalCurrency VARCHAR(3);
DECLARE @ControlCurrency VARCHAR(3);

DECLARE @ItemVatType VARCHAR(1);
DECLARE @LcBasedOnCCFromItem BIT;
DECLARE @ItemBrand VARCHAR(3);
DECLARE @ItemCategory VARCHAR(4);
DECLARE @ItemType VARCHAR(3);
DECLARE @ItemFourthParty VARCHAR(1);
DECLARE @BrandLCRound NUMERIC(18,0);
DECLARE @BrandLCRoundMode VARCHAR(1);
DECLARE @BrandConsider1000As990 VARCHAR(1);

DECLARE @LocVatType VARCHAR(1);
DECLARE @LocVatRETtype VARCHAR(1);
DECLARE @LocVatWtype VARCHAR(1);
DECLARE @InvPriceLCBasedOnCC BIT;
DECLARE @RoundVal NUMERIC(18,0);
DECLARE @RoundMode VARCHAR(1);

DECLARE @ClientPriceCategory VARCHAR(1);
DECLARE @DefaultCashClientRecID NUMERIC(18,0);
DECLARE @PrefPriceCode VARCHAR(2);	
DECLARE @ConsiderClientDiscAsAddDisc VARCHAR(1);
DECLARE @ClientPriceCode VARCHAR(2);
DECLARE @ClientType VARCHAR(3);
DECLARE @ClientDiscount NUMERIC(5,2);
DECLARE @ClientDiscountGroup VARCHAR(1);

DECLARE @StockItemPrice AS NUMERIC(18,3);
DECLARE @StockItemDiscount AS NUMERIC(18,3);

SELECT  @Company = Loc_CoCode ,
        @LocVatType = Loc_Vat ,
        @LocVatRETtype = Loc_VATRetType ,
        @LocVatWtype = Loc_VATWType ,
        @InvPriceLCBasedOnCC = CASE WHEN OP_LCBasedOnCC = 'Y' THEN 1
                                    ELSE 0
                               END ,
        @RoundVal = OP_Roundval ,
        @RoundMode = OP_RoundMode ,
        @DefaultCashClientRecID = SL_DefaultCCrecid ,
        @PrefPriceCode = ISNULL(SL_PrefPrc, ''),
		@ConsiderClientDiscAsAddDisc = ISNULL(SL_ClDiscAsAddDisc, 'N')
FROM    dbo.STK04_Location
        INNER JOIN ( SELECT OP_Loc ,
                            ISNULL(OP_LCBasedOnCC, 'N') OP_LCBasedOnCC ,
                            OP_Roundval ,
                            OP_RoundMode
                     FROM   STK04_OPParam
                     WHERE  OP_Loc = @LocationCode
                   ) STK04_OPParam ON STK04_OPParam.OP_Loc = STK04_Location.Loc_Code
        INNER JOIN ( SELECT SL_Loc ,
                            SL_DefaultCCrecid ,
                            SL_PrefPrc ,
							SL_ClDiscAsAddDisc
                     FROM   pos_splocation
                     WHERE  SL_Loc = @LocationCode
                   ) pos_splocation ON SL_Loc = Loc_Code
WHERE   Loc_Code = @LocationCode;

SELECT TOP 1
        @Year = Ac_Year ,
        @LocalCurrency = Ac_Curr1 ,
        @ControlCurrency = Ac_Curr2
FROM    dbo.Com_AppComp WITH ( NOLOCK )
WHERE   Ac_Code = 'POSO'
        AND Ac_Comp = @Company
ORDER BY Ac_StDate DESC;

SELECT  @ItemVatType = STK_VATTYPE ,
        @LcBasedOnCCFromItem = CASE WHEN STK_RecalculateLC = 'Y'
                                         OR ISNULL(STK_RecalculateLC, '') = ''
                                    THEN 1
                                    ELSE 0
                               END ,
        @ItemFourthParty = ISNULL(STK_FourthParty, 'N') ,
		@ItemBrand = STK_Brand ,
		@ItemCategory = STK_CATEGORY ,
		@ItemType = STK_ITMTYPE ,
		@BrandLCRound = ISNULL(br_LCRound, 0) ,
        @BrandLCRoundMode = br_LCRoundMode ,
        @BrandConsider1000As990 = br_Consider1000As990
FROM    dbo.STK04_STKFIL
LEFT JOIN ( SELECT  br_Code ,
                    br_LCRound ,
                    br_LCRoundMode ,
                    br_Consider1000As990
            FROM    Stk04_Brand
            ) Stk04_Brand ON Stk04_Brand.br_Code = STK04_STKFIL.STK_Brand
WHERE   STK_RECID = @ItemRecID;

SELECT  @ClientPriceCategory = CL_PriceCat ,
        @ClientPriceCode = CL_UsedPrice ,
        @ClientType = CL_Type ,
		@ClientDiscount = CASE WHEN @ConsiderClientDiscAsAddDisc = 'N' THEN ISNULL(CL_Disc, 0) ELSE CONVERT(NUMERIC(5,2), 0) END
FROM    dbo.Stk04_Clients
WHERE   CL_RecID = @ClientRecID;

SELECT  @ClientDiscountGroup = ISNULL(SP_ClientDiscountGroup, 'P')
FROM    stk04_SysParam WITH ( NOLOCK );

--Get Item Price

IF (@ItemFourthParty = 'Y') 

	BEGIN

		SELECT @StockItemPrice = 0

	END

ELSE

	BEGIN

		DECLARE @ClientSpecialPrice AS VARCHAR(1); -- C : client | T : client type

		IF EXISTS ( SELECT TOP 1
							1
					FROM    dbo.Stk04_CLSTKPrice WITH ( NOLOCK )
					WHERE   CL_ClRecid = @ClientRecID
							AND CL_StkRecid = @ItemRecID )
			BEGIN
				SET @ClientSpecialPrice = 'C';
			END;	

		ELSE
			BEGIN 

				IF EXISTS ( SELECT TOP 1
									1
							FROM    dbo.Stk04_CLSTKPrice WITH ( NOLOCK )
							WHERE   CL_ClientType = @ClientType
									AND CL_StkRecid = @ItemRecID )
					BEGIN
						SET @ClientSpecialPrice = 'T';
					END;	
				ELSE
					BEGIN
						SET @ClientSpecialPrice = '';
					END;	

			END	

		IF (@ClientSpecialPrice = '') 

			BEGIN

				SELECT @StockItemPrice =

				CASE WHEN @LocVatType = 'N' OR @ClientPriceCategory = 'X' THEN

					CASE WHEN @Currency = @LocalCurrency AND @InvPriceLCBasedOnCC = 0 THEN 
						SP_LCTAXEX
					ELSE
						CASE WHEN @Currency = @LocalCurrency  THEN
							CASE WHEN @InvPriceLCBasedOnCC = 1 AND @LcBasedOnCCFromItem = 1 THEN
								SP_FCTAXEX
							ELSE
								SP_LCTAXEX
							END
						ELSE
							SP_FCTAXEX
						END
					END

				WHEN @LocVatType = 'Y' THEN

					CASE WHEN @ItemVatType <> 'T' THEN

						CASE WHEN @Currency = @LocalCurrency AND @InvPriceLCBasedOnCC = 0 THEN 
							SP_LCTAXEX
						ELSE
							CASE WHEN @Currency = @LocalCurrency  THEN
								CASE WHEN @InvPriceLCBasedOnCC = 1 AND @LcBasedOnCCFromItem = 1 THEN
									SP_FCTAXEX
								ELSE
									SP_LCTAXEX
								END
							ELSE
								SP_FCTAXEX
							END			
						END

					ELSE

						CASE WHEN @ClientPriceCategory = 'W' THEN
        
							CASE WHEN @LocVatWtype = 'T' THEN	
				
								CASE WHEN @Currency = @LocalCurrency AND @InvPriceLCBasedOnCC = 0 THEN 
									SP_LCTTC
								ELSE	
									CASE WHEN @Currency = @LocalCurrency  THEN
										CASE WHEN @InvPriceLCBasedOnCC = 1 AND @LcBasedOnCCFromItem = 1 THEN
											SP_FCTTC
										ELSE
											SP_LCTTC
										END
									ELSE
										SP_FCTTC
									END
								END

							ELSE		
				
								CASE WHEN @Currency = @LocalCurrency AND @InvPriceLCBasedOnCC = 0 THEN 
									SP_LCTAXEX
								ELSE
									CASE WHEN @Currency = @LocalCurrency  THEN
										CASE WHEN @InvPriceLCBasedOnCC = 1 AND @LcBasedOnCCFromItem = 1 THEN
											SP_FCTAXEX
										ELSE
											SP_LCTAXEX
										END
									ELSE
										SP_FCTAXEX
									END
								END

							END

						ELSE	

							CASE WHEN @LocVatRETtype = 'T' THEN
				
								CASE WHEN @Currency = @LocalCurrency AND @InvPriceLCBasedOnCC = 0 THEN 
									SP_LCTTC
								ELSE
									CASE WHEN @Currency = @LocalCurrency  THEN
										CASE WHEN @InvPriceLCBasedOnCC = 1 AND @LcBasedOnCCFromItem = 1 THEN
											SP_FCTTC
										ELSE
											SP_LCTTC
										END
									ELSE
										SP_FCTTC
									END
								END

							ELSE

								CASE WHEN @Currency = @LocalCurrency AND @InvPriceLCBasedOnCC = 0 THEN 
									SP_LCTAXEX
								ELSE
									CASE WHEN @Currency = @LocalCurrency  THEN
										CASE WHEN @InvPriceLCBasedOnCC = 1 AND @LcBasedOnCCFromItem = 1 THEN
											SP_FCTAXEX
										ELSE
											SP_LCTAXEX
										END
									ELSE
										SP_FCTAXEX
									END
								END

							END

						END

					END

				END 

				FROM 

				--the bellow sub query is only in case of sales at back office, you can simply remove it in case of stand alone
				(
					SELECT  SP_StkRecId ,
							SP_PriceType ,
							SP_LCTAXEX ,
							SP_LCTTC ,
							SP_FCTAXEX ,
							SP_FCTTC 
					FROM    dbo.Stk04_stkprices
							LEFT JOIN Stk04_Locstkprices ON LSP_StkRecId = SP_StkRecId
															AND LSP_LocCode = @LocationCode
															AND LSP_PriceType = SP_PriceType
					WHERE   ISNULL(SP_StartingQty, 0) = dbo.F_GetStockPriceStaringQty(@ItemRecID, CASE WHEN @ClientRecID = @DefaultCashClientRecID AND @PrefPriceCode <> '' THEN @PrefPriceCode ELSE @ClientPriceCode END, @Qty) AND LSP_StkRecId IS NULL
					UNION
					SELECT  LSP_StkRecId ,
							LSP_PriceType ,
							LSP_LCTAXEX ,
							LSP_LCTTC ,
							LSP_FCTAXEX ,
							LSP_FCTTC 
					FROM    Stk04_Locstkprices
					WHERE   LSP_LocCode = @LocationCode
				) Stk04_stkprices 

				WHERE SP_StkRecId = @ItemRecID
				  AND SP_PriceType = CASE WHEN @ClientRecID = @DefaultCashClientRecID AND @PrefPriceCode <> '' THEN @PrefPriceCode ELSE @ClientPriceCode END

			END	

		ELSE	

			BEGIN

				SELECT @StockItemPrice = 

				CASE WHEN @LocVatType = 'N' OR @ClientPriceCategory = 'X' OR ( @LocVatType = 'Y' AND @ItemVatType <> 'T' ) THEN
	
					CASE WHEN @Currency = @LocalCurrency AND @InvPriceLCBasedOnCC = 0 THEN 
						CL_PriceLCEX
					ELSE

						CASE WHEN @Currency = @LocalCurrency  THEN
							CASE WHEN @InvPriceLCBasedOnCC = 1 AND @LcBasedOnCCFromItem = 1 THEN
								CL_PriceFCEX
							ELSE
								CL_PriceLCEX
							END
						ELSE
							CL_PriceFCEX
						END

					END

				WHEN @LocVatType = 'Y' AND @ItemVatType = 'T' THEN

					CASE WHEN @ClientPriceCategory = 'W' THEN
		
						CASE WHEN @LocVatWtype = 'T' THEN
        
							CASE WHEN @Currency = @LocalCurrency AND @InvPriceLCBasedOnCC = 0 THEN 
								CL_PriceLCTTC
							ELSE	
								CASE WHEN @Currency = @LocalCurrency  THEN
									CASE WHEN @InvPriceLCBasedOnCC = 1 AND @LcBasedOnCCFromItem = 1 THEN
										CL_PriceFCTTC
									ELSE
										CL_PriceLCTTC
									END
								ELSE	
									CL_PriceFCTTC
								END
							END

						ELSE	

							CASE WHEN @Currency = @LocalCurrency AND @InvPriceLCBasedOnCC = 0 THEN 
								CL_PriceLCEX
							ELSE
								CASE WHEN @Currency = @LocalCurrency  THEN
									CASE WHEN @InvPriceLCBasedOnCC = 1 AND @LcBasedOnCCFromItem = 1 THEN				
										CL_PriceFCEX
									ELSE
										CL_PriceLCEX
									END
								ELSE
									CL_PriceFCEX
								END
							END

						END
	
					ELSE	

						CASE WHEN @LocVatRETtype = 'T' THEN
        
							CASE WHEN @Currency = @LocalCurrency AND @InvPriceLCBasedOnCC = 0 THEN 
								CL_PriceLCTTC
							ELSE
								CASE WHEN @Currency = @LocalCurrency  THEN
									CASE WHEN @InvPriceLCBasedOnCC = 1 AND @LcBasedOnCCFromItem = 1 THEN	
										CL_PriceFCTTC
									ELSE
										CL_PriceLCTTC
									END
								ELSE
									CL_PriceFCTTC
								END
							END

						ELSE	

							CASE WHEN @Currency = @LocalCurrency AND @InvPriceLCBasedOnCC = 0 THEN 
								CL_PriceLCEX
							ELSE
								CASE WHEN @Currency = @LocalCurrency  THEN
									CASE WHEN @InvPriceLCBasedOnCC = 1 AND @LcBasedOnCCFromItem = 1 THEN
										CL_PriceFCEX
									ELSE	
										CL_PriceLCEX
									END
								ELSE
									CL_PriceFCEX
								END			
							END

						END

					END

				END 
				
				FROM dbo.Stk04_CLSTKPrice 
				WHERE CL_StkRecid = @ItemRecID
				AND CASE WHEN @ClientSpecialPrice = 'C' THEN CONVERT(VARCHAR,CL_ClRecid  ) ELSE CONVERT(VARCHAR,CL_ClientType) END = 
					CASE WHEN @ClientSpecialPrice = 'C' THEN CONVERT(VARCHAR,@ClientRecID) ELSE CONVERT(VARCHAR,@ClientType  ) END 

			END

	END;

IF ( @Currency = @LocalCurrency
     AND @InvPriceLCBasedOnCC = 1
     AND @LcBasedOnCCFromItem = 1
   )
    BEGIN
				
        SET @StockItemPrice = dbo.Rounding(dbo.convert5('POSO', @Company,
                                                        @Year, @StockItemPrice,
                                                        @ControlCurrency,
                                                        @LocalCurrency,
                                                        @WorkingDay),
                                           @BrandLCRound, @BrandLCRoundMode,
                                           @BrandConsider1000As990);

    END	

SET @StockItemPrice = dbo.Rounding(@StockItemPrice, @RoundVal, @RoundMode, 'N');

IF ( NOT (@Currency = @LocalCurrency OR @Currency = @ControlCurrency) )

	BEGIN

        SET @StockItemPrice = dbo.convert5('POSO', @Company, @Year,
                                           @StockItemPrice, @ControlCurrency,
                                           @Currency, @WorkingDay);

	END	

--Get Item Discount

DECLARE @ItemDisc NUMERIC(5, 2);
DECLARE @ItemDiscPerPriceType NUMERIC(5, 2);
DECLARE @ItemDiscClientSpecial NUMERIC(5, 2);
DECLARE @ItemDiscClientTypeSpecial NUMERIC(5, 2);
DECLARE @MaximumDiscount NUMERIC(5, 2);

DECLARE @MaxDiscFromStock NUMERIC(5, 2);
DECLARE @AllowModifyPrice VARCHAR(1);

SELECT  @ItemDisc = ISNULL(CASE WHEN SP_DISCDATE IS NOT NULL
                                THEN CASE WHEN CONVERT(DATE, SP_DISCDATE) >= @WorkingDay
                                          THEN ISNULL(SP_DISC, 0)
                                          ELSE CONVERT(NUMERIC(5, 2), 0)
                                     END
                                ELSE ISNULL(SP_DISC, 0)
                           END, 0) ,

		@AllowModifyPrice = ISNULL(SP_ACCEPTPR, 'N') ,

		@MaxDiscFromStock = SP_MAxDISC

FROM    
		--the bellow sub query is only in case of sales at back office, you can simply remove it in case of stand alone
	      ( SELECT    SP_StkRecId ,
                    SP_PriceType ,
                    SP_DISCDATE ,
                    SP_DISC ,
					SP_ACCEPTPR ,
					SP_MAxDISC
          FROM      dbo.Stk04_stkprices
                    LEFT JOIN Stk04_Locstkprices ON LSP_StkRecId = SP_StkRecId
                                                    AND LSP_LocCode = @LocationCode
                                                    AND LSP_PriceType = SP_PriceType
          WHERE     ISNULL(SP_StartingQty, 0) = dbo.F_GetStockPriceStaringQty(@ItemRecID, CASE WHEN @ClientRecID = @DefaultCashClientRecID AND @PrefPriceCode <> '' THEN @PrefPriceCode ELSE @ClientPriceCode END, @Qty) AND LSP_StkRecId IS NULL
          UNION
          SELECT    LSP_StkRecId ,
                    LSP_PriceType ,
                    LSP_DISCDATE ,
                    LSP_DISC ,
					LSP_ACCEPTPR ,
					(SELECT SP_MAxDISC FROM stk04_stkprices WHERE ISNULL(SP_StartingQty, 0) = dbo.F_GetStockPriceStaringQty(@ItemRecID, CASE WHEN @ClientRecID = @DefaultCashClientRecID AND @PrefPriceCode <> '' THEN @PrefPriceCode ELSE @ClientPriceCode END, @Qty) AND lsp_stkrecid = sp_stkrecid AND LSP_PriceType = SP_PriceType AND LSP_LocCode =  @LocationCode) LSP_MAxDISC
          FROM      Stk04_Locstkprices
          WHERE     LSP_LocCode = @LocationCode
        ) Stk04_stkprices
		WHERE SP_StkRecId = @ItemRecID
				  AND SP_PriceType = CASE WHEN @ClientRecID = @DefaultCashClientRecID AND @PrefPriceCode <> '' THEN @PrefPriceCode ELSE @ClientPriceCode END ;

IF ( @ClientDiscountGroup = 'B' )
    BEGIN	

        SELECT  @ItemDiscPerPriceType = ISNULL(CD_PerOther, 0) 
        FROM    Stk04_ClientDisc
                INNER JOIN ( SELECT br_RecId ,
                                    br_Code
                             FROM   Stk04_Brand
                           ) Stk04_Brand ON CD_Category = br_RecId
        WHERE   br_Code = @ItemBrand
                AND CD_CLRecid = @ClientRecID
                AND CD_SAPdate <= @WorkingDay
                AND CD_EndApDate >= @WorkingDay
        ORDER BY CD_SAPdate DESC;

    END;

ELSE
    IF ( @ClientDiscountGroup = 'C' )
        BEGIN	

            SELECT  @ItemDiscPerPriceType = ISNULL(CD_PerOther, 0)
            FROM    Stk04_ClientDisc
                    INNER JOIN ( SELECT Cat_Recid ,
                                        Cat_Code
                                 FROM   Stk04_Category
                               ) Stk04_Category ON CD_Category = Cat_Recid
            WHERE   Cat_Code = @ItemType
                    AND CD_CLRecid = @ClientRecID
                    AND CD_SAPdate <= @WorkingDay
                    AND CD_EndApDate >= @WorkingDay
            ORDER BY CD_SAPdate DESC;

        END;	

    ELSE
        BEGIN
        
            SELECT  @ItemDiscPerPriceType = ISNULL(CD_PerOther, 0)
            FROM    Stk04_ClientDisc
                    INNER JOIN ( SELECT IT_RecID ,
                                        IT_Code
                                 FROM   STK04_ItemType
                               ) STK04_ItemType ON CD_Category = IT_RecID
            WHERE   IT_Code = @ItemType
                    AND CD_CLRecid = @ClientRecID
                    AND CD_SAPdate <= @WorkingDay
                    AND CD_EndApDate >= @WorkingDay
            ORDER BY CD_SAPdate DESC;

        END;	

SELECT  @ItemDiscClientSpecial = ISNULL(CL_Disc, 0)
FROM    dbo.Stk04_CLSTKPrice
WHERE   CL_StkRecid = @ItemRecID
        AND CL_ClRecid = @ClientRecID;

SELECT  @ItemDiscClientTypeSpecial = ISNULL(CL_Disc, 0)
FROM    dbo.Stk04_CLSTKPrice
WHERE   CL_StkRecid = @ItemRecID
        AND CL_ClientType = @ClientType;

SELECT  @MaximumDiscount = MAX([ItemDiscountValues])
FROM    ( SELECT    ISNULL(ABS(@ItemDisc), 0) [ItemDiscountValues]
          UNION ALL
          SELECT    ISNULL(@ItemDiscPerPriceType, 0)
          UNION ALL
          SELECT    ISNULL(@ItemDiscClientSpecial, 0)
          UNION ALL
          SELECT    ISNULL(@ItemDiscClientTypeSpecial, 0)
          UNION ALL
          SELECT    ISNULL(@ClientDiscount, 0)
        ) tblDiscountValues;

IF ( @MaxDiscFromStock IS NULL )
    BEGIN

        IF ABS(@ItemDisc) = @MaximumDiscount
            AND @ItemDisc < 0
            BEGIN
				
                DECLARE @TmpMaximumDiscount NUMERIC(5, 2);

                SELECT  @TmpMaximumDiscount = MAX([ItemTmpDiscountValues])
                FROM    ( SELECT    ISNULL(@ItemDiscPerPriceType, 0) [ItemTmpDiscountValues]
                          UNION ALL
                          SELECT    ISNULL(@ItemDiscClientSpecial, 0)
                          UNION ALL
                          SELECT    ISNULL(@ItemDiscClientTypeSpecial, 0)
                          UNION ALL
                          SELECT    ISNULL(@ClientDiscount, 0)
                        ) tblTmpDiscountValues;

                IF ( @TmpMaximumDiscount > 0 )
                    BEGIN
                        SET @StockItemDiscount = @TmpMaximumDiscount;
                    END;
                ELSE
                    BEGIN	
                        SET @StockItemDiscount = CONVERT(NUMERIC(5, 2), 0);
                    END;	

            END;

        ELSE
            BEGIN

                SET @StockItemDiscount = @MaximumDiscount; 
	
            END;	

    END;	

ELSE
    BEGIN

        IF ( @MaxDiscFromStock <= 0 )
            BEGIN
                SET @StockItemDiscount = 0;
            END;	
        ELSE
            BEGIN
				
                IF ( @MaximumDiscount > @MaxDiscFromStock )
                    BEGIN
                        SET @StockItemDiscount = @MaxDiscFromStock;
                    END;	
                ELSE
                    BEGIN
                        SET @StockItemDiscount = @MaximumDiscount;
                    END;	

            END;	

    END;	

SELECT  @ItemRecID stockItemRecID,
        ISNULL(@StockItemPrice, 0) stockItemPrice ,
        ISNULL(@StockItemDiscount, 0) stockItemDiscount




GO