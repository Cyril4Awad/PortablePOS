using SuperStoreBusiness.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperStoreBusiness.Controller
{
    public class StockItemsController
    {
        public void InsertStockItems(StockItems stockItems, SqlCommand sqlCommand = null)
        {
            using (SqlConnection cnSQL = new SqlConnection(ClsConfig.SQLConnectionString))
            {
                cnSQL.Open();
                using (SqlTransaction transaction = cnSQL.BeginTransaction())
                {
                    using (SqlCommand cmSQL = cnSQL.CreateCommand())
                    {
                        cmSQL.Transaction = transaction;
                        try
                        {
                            string strQuery = $@" INSERT INTO dbo.tbl_StockItm
                                VALUES        ( @Stk_Loc ,
                                                @Stk_StkId ,
                                                @Stk_HoStkId ,
                                                @Stk_Supplier ,
                                                @Stk_Category ,
                                                @Stk_Serial ,
                                                @Stk_CrDate ,
                                                @Stk_Status ,
                                                @Stk_Consignment ,
                                                @Stk_VATStatus ,
                                                @Stk_ModSal ,
                                                @Stk_DescPos ,
                                                @Stk_Desc ,
                                                @Stk_Barcode1 ,
                                                null,
                                                @Stk_Barcode2 ,
                                                null ,
                                                @Stk_Packing ,
                                                @Stk_UnitCode ,
                                                null ,
                                                @Stk_Transp ,
                                                @Stk_ItmService ,
                                                @Stk_Assembly ,
                                                @Stk_3rdParty ,
                                                @Stk_PLUnbr ,
                                                @Stk_GoodsLoc ,
                                                @Stk_Gift ,
                                                @Stk_StopOrder ,
                                                @Stk_MinQty ,
                                                @Stk_MaxQty ,
                                                null ,
                                                null ,
                                                @Stk_BBFDate1,
                                	        	null,
                                                @Stk_BBFQty1 ,
                                                null ,
                                                @Stk_LDateCount,
                                                @Stk_AvlbQty1 ,
                                                null ,
                                                @Stk_Return1 ,
                                                null ,
                                                @Stk_BBFCostLC ,
                                                @Stk_BBFCostFC ,
                                                @Stk_ActCostLL ,
                                                @Stk_ActCostFC ,
                                                @Stk_LstCostLL ,
                                                @Stk_LstCostFC ,
                                                @Stk_FCcurr ,
                                                @stk_PotcostLL ,
                                                @stk_PotCostFC ,
                                                @stk_potchangeDate,
                                                @Stk_1SlPriceLC ,
                                                @Stk_1SlPrice$ ,
                                                @Stk_Qty1stPrice ,
                                                @Stk_2SLPriceLC ,
                                                @Stk_2SLPrice$ ,
                                                @Stk_Qty2stPrice ,
                                                @Stk_3SLPriceLC ,
                                                @Stk_3SLPrice$ ,
                                                @Stk_SLCoef ,
                                                @Stk_LStatCoef ,
                                                @Stk_UserCoef ,
                                                @Stk_DiscWh ,
                                                @Stk_YTDQtyIn ,
                                                @Stk_YTDQtyOut ,
                                                @Stk_YTDAmtInLL ,
                                                @Stk_YTDAmtIn$ ,
                                                @Stk_YTDAmtOutLL ,
                                                @Stk_YTDAmtOut$ ,
                                                @Stk_SubjectVat ,
                                                @Stk_VatType ,
                                                @Stk_VatPer ,
                                                @Stk_AddOnRef ,
                                                GETDATE(),
                                                GETDATE(),
                                                @Stk_StaticUstmp ,
                                                GETDATE(),
                                                @Stk_Ustmp ,
                                                GETDATE(),
                                                @Stk_ValidityDays,
                                                @Stk_Expiry,
                                                @Stk_DiscRetail,
                                                @Stk_Brand,
                                                @Stk_VatCode,
                                                @Stk_BBFCostCC,
                                                @Stk_ActCostCC,
                                                @Stk_LstCostCC,
                                                @stk_PotCostCC 
                                              ) ";
                            ExecuteSqlQuery(stockItems, strQuery, cmSQL);
                            if (stockItems.Stk_StockQtyList != null && stockItems.Stk_StockQtyList.Count > 0)
                            {
                                StockQtyController controller = new StockQtyController();
                                foreach (StockQty stockQty in stockItems.Stk_StockQtyList)
                                {
                                    controller.InsertStockQty(stockQty, cmSQL);
                                }
                            }
                            //if (stockItems.Stk_StockQtyExpiryList != null && stockItems.Stk_StockQtyExpiryList.Count > 0)
                            //{
                            //    StockQtyExpiryController controller = new StockQtyExpiryController();
                            //    foreach (StockQtyExpiry stockQtyExpiry in  stockItems.Stk_StockQtyExpiryList)
                            //    {
                            //        controller.InsertStockQtyExpiry(stockQtyExpiry, cmSQL);
                            //    }
                            //}
                            if (stockItems.Stk_AddenSupplierList != null && stockItems.Stk_AddenSupplierList.Count > 0)
                            {
                                AddenSupplierController controller = new AddenSupplierController();
                                foreach (AddenSupplier addenSupp in stockItems.Stk_AddenSupplierList.Where(Supp => !Supp.Delete).ToList())
                                {
                                    controller.InsertAddenSupplier(addenSupp, cmSQL);
                                }
                            }
                            if (stockItems.stk_StockUnitList != null && stockItems.stk_StockUnitList.Count > 0)
                            {
                                StockUnitController controller = new StockUnitController();
                                ChangeItmPriceController changePriceController = new ChangeItmPriceController();

                                foreach (StockUnit stockUnit in stockItems.stk_StockUnitList.Where(Unit => !Unit.Delete).ToList())
                                {
                                    controller.InsertStockUnit(stockUnit, cmSQL);
                                    changePriceController.InsertChangeItmPrice(stockItems.stk_changeItmPriceList
                                        .FirstOrDefault(p => p.Cip_Unit?.ToUpper() == stockUnit.SUnit_Unit?.ToUpper() && p.Cip_StkId == stockUnit.SUnit_StkId), cmSQL);

                                    if (stockUnit.Stk_BarcodeList != null && stockUnit.Stk_BarcodeList.Count > 0)
                                    {
                                        BarcodeSynController BCcontroller = new BarcodeSynController();
                                        foreach (BarcodeSyn barcode in stockUnit.Stk_BarcodeList.Where(barcode => !barcode.Delete).ToList())
                                        {
                                            BCcontroller.InsertBarcodeSyn(barcode, cmSQL);
                                        }
                                    }
                                }
                            }
                            if (stockItems.Stk_OHStockAdd != null)
                            {
                                OHStockAddController controller = new OHStockAddController();
                                controller.InsertOHStockAdd(stockItems.Stk_OHStockAdd, cmSQL);
                            }
                            if (stockItems.stk_FilePrice != null)
                            {
                                StockFilePriceController controller = new StockFilePriceController();
                                controller.InsertStockFilePrice(stockItems.stk_FilePrice, cmSQL);
                            }
                            if (stockItems.Stk_Online != null)
                            {
                                StockOnlineController controller = new StockOnlineController();
                                controller.InsertStockOnline(stockItems.Stk_Online, cmSQL);
                            }
                            if (stockItems.Stk_DiscExc != null)
                            {
                                StockDiscExclusionController controller = new StockDiscExclusionController();
                                controller.InsertStockDiscExclusion(stockItems.Stk_DiscExc, cmSQL);
                            }
                            if (stockItems.Stk_LinkStockDept != null)
                            {
                                if (stockItems.Stk_LinkStockDept.lsd_DeptRecID != null)
                                {
                                    LinkStockDeptController controller = new LinkStockDeptController();
                                    controller.InsertLinkStockDept(stockItems.Stk_LinkStockDept, cmSQL);
                                }  
                            }

                            //stockItems.ClsDynamicFields.SaveDynamicFields(cmSQL);

                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                transaction.Rollback();
                                Console.WriteLine(ex.Message);

                            }
                            catch (Exception)
                            {
                            }
                            throw;
                        }
                    }
                }
            }
        }

        public void UpdateStockItems(StockItems stockItems = null, SqlCommand sqlCommand = null, string FromTemp = null, string TrType = null)
        {
            if (string.IsNullOrEmpty(FromTemp))
            {
                using (SqlConnection cnSQL = new SqlConnection(ClsConfig.SQLConnectionString))
                {
                    cnSQL.Open();
                    using (SqlTransaction transaction = cnSQL.BeginTransaction())
                    {

                        using (SqlCommand cmSQL = cnSQL.CreateCommand())
                        {
                            cmSQL.Transaction = transaction;
                            try
                            {
                                string strQuery = $@" UPDATE tbl_StockItm
                                     SET
                                         Stk_HoStkId = @Stk_HoStkId,
                                         Stk_Supplier = @Stk_Supplier,
                                         Stk_Category = @Stk_Category,
                                         Stk_Serial = @Stk_Serial,
                                         Stk_CrDate = @Stk_CrDate,
                                         Stk_Status = @Stk_Status,
                                         Stk_Consignment = @Stk_Consignment,
                                         Stk_VATStatus = @Stk_VATStatus,
                                         Stk_ModSal = @Stk_ModSal,
                                         Stk_DescPos = @Stk_DescPos,
                                         Stk_Desc = @Stk_Desc,
                                         Stk_Barcode1 = @Stk_Barcode1,
                                         Stk_Barcode2 = @Stk_Barcode2,
                                     	 Stk_Packing = @Stk_Packing,
                                     	 Stk_UnitCode = @Stk_UnitCode,
                                     	 Stk_UnitsPack= null,
                                         Stk_Transp = @Stk_Transp,
                                         Stk_ItmService = @Stk_ItmService,
                                         Stk_Assembly = @Stk_Assembly,
                                         Stk_3rdParty = @Stk_3rdParty,
                                         Stk_PLUnbr = @Stk_PLUnbr,
                                         Stk_GoodsLoc = @Stk_GoodsLoc,
                                         Stk_Gift = @Stk_Gift,
                                         Stk_StopOrder = @Stk_StopOrder,
                                         Stk_MinQty = @Stk_MinQty,
                                         Stk_MaxQty = @Stk_MaxQty,
                                         Stk_BBFDate1 =@Stk_BBFDate1,
                                         Stk_BBFQty1 = @Stk_BBFQty1,
                                         Stk_LDateCount =@Stk_LDateCount,
                                         Stk_AvlbQty1 = @Stk_AvlbQty1,
                                         Stk_Return1 = @Stk_Return1,
                                         Stk_BBFCostLC = @Stk_BBFCostLC,
                                         Stk_BBFCostFC = @Stk_BBFCostFC,
                                         Stk_ActCostLL = @Stk_ActCostLL,
                                         Stk_ActCostFC = @Stk_ActCostFC,
                                         Stk_LstCostLL = @Stk_LstCostLL,
                                         Stk_LstCostFC = @Stk_LstCostFC,
                                         Stk_FCcurr = @Stk_FCcurr,
                                         stk_PotcostLL = @stk_PotcostLL,
                                         stk_PotCostFC = @stk_PotCostFC,
                                         stk_potchangeDate = @stk_potchangeDate,
                                         Stk_1SlPriceLC = @Stk_1SlPriceLC,
                                         [Stk_1SlPrice$] = @Stk_1SlPrice$,
                                         Stk_Qty1stPrice = @Stk_Qty1stPrice,
                                         Stk_2SLPriceLC = @Stk_2SLPriceLC,
                                         [Stk_2SLPrice$] = @Stk_2SLPrice$,
                                         Stk_Qty2stPrice = @Stk_Qty2stPrice,
                                         Stk_3SLPriceLC = @Stk_3SLPriceLC,
                                         [Stk_3SLPrice$] = @Stk_3SLPrice$,
                                         Stk_SLCoef = @Stk_SLCoef,
                                         Stk_LStatCoef = @Stk_LStatCoef,
                                         Stk_UserCoef = @Stk_UserCoef,
                                         Stk_DiscWh = @Stk_DiscWh,
                                         Stk_YTDQtyIn = @Stk_YTDQtyIn,
                                         Stk_YTDQtyOut = @Stk_YTDQtyOut,
                                         Stk_YTDAmtInLL = @Stk_YTDAmtInLL,
                                         [Stk_YTDAmtIn$] = @Stk_YTDAmtIn$,
                                         Stk_YTDAmtOutLL = @Stk_YTDAmtOutLL,
                                         [Stk_YTDAmtOut$] = @Stk_YTDAmtOut$,
                                         Stk_SubjectVat = @Stk_SubjectVat,
                                         Stk_VatType = @Stk_VatType,
                                         Stk_VatPer = @Stk_VatPer,
                                         Stk_AddOnRef = @Stk_AddOnRef,
                                         Stk_HoExport = GETDATE(),
                                         Stk_HoImport = GETDATE(),
                                         Stk_StaticUstmp = @Stk_StaticUstmp,
                                         Stk_StaticDstmp =GETDATE(),
                                         Stk_Ustmp = @Stk_Ustmp,
                                         Stk_Dstmp = GETDATE(),
                                     	 Stk_ValidityDays = @Stk_ValidityDays,
                                     	 Stk_Expiry= @Stk_Expiry,
                                     	 Stk_DiscRetail=@Stk_DiscRetail ,
                                     	 Stk_Brand = @Stk_Brand,
                                     	 Stk_VatCode = @Stk_VatCode,
                                     	 Stk_BBFCostCC = @Stk_BBFCostCC,
                                     	 Stk_ActCostCC = @Stk_ActCostCC,
                                     	 Stk_LstCostCC = @Stk_LstCostCC,
                                     	 stk_PotCostCC = @stk_PotCostCC
                                    WHERE
                                         Stk_Loc = @Stk_Loc
                                     AND Stk_StkId = @Stk_StkId
                                         ;";
                                ExecuteSqlQuery(stockItems, strQuery, cmSQL);

                                if (stockItems.Stk_StockQtyList != null)
                                {
                                    StockQtyController controller = new StockQtyController();
                                    foreach (StockQty stockQty in stockItems.Stk_StockQtyList)
                                    {
                                        controller.UpdateStockQty(stockQty, cmSQL);
                                    }
                                }

                                if (stockItems.Stk_OHStockAdd != null)
                                {
                                    OHStockAddController controller = new OHStockAddController();
                                    controller.UpdateOHStockAdd(stockItems.Stk_OHStockAdd, cmSQL);
                                }
                                if (stockItems.stk_FilePrice != null)
                                {
                                    StockFilePriceController controller = new StockFilePriceController();
                                    controller.UpdateStockFilePrice(stockItems.stk_FilePrice, cmSQL);
                                }
                                if (stockItems.Stk_Online == null && StockOnlineController.StockOnlineExists(stockItems.Stk_StkId))
                                {
                                    StockOnlineController controller = new StockOnlineController();
                                    StockOnline stockOnline = new StockOnline() { Onl_Stkid = stockItems.Stk_StkId };
                                    controller.DeleteStockOnline(stockOnline, cmSQL);
                                }
                                else if (stockItems.Stk_Online != null)
                                {
                                    StockOnlineController controller = new StockOnlineController();
                                    controller.UpdateStockOnline(stockItems.Stk_Online, cmSQL);
                                }
                                if (stockItems.Stk_DiscExc == null && StockDiscExclusionController.StockDiscExclusionExists(stockItems.Stk_StkId))
                                {
                                    StockDiscExclusionController controller = new StockDiscExclusionController();
                                    StockDiscExclusion stockDiscExclusion = new StockDiscExclusion() { StkExcl_Stkid = stockItems.Stk_StkId };

                                    controller.DeleteStockDiscExclusion(stockDiscExclusion, cmSQL);
                                }
                                else if (stockItems.Stk_DiscExc != null)
                                {
                                    StockDiscExclusionController controller = new StockDiscExclusionController();
                                    controller.UpdateStockDiscExclusion(stockItems.Stk_DiscExc, cmSQL);

                                }
                                if (stockItems.Stk_AddenSupplierList != null && stockItems.Stk_AddenSupplierList.Count > 0)
                                {
                                    AddenSupplierController controller = new AddenSupplierController();
                                    //JAD
                                    foreach (AddenSupplier addenSupplier in stockItems.Stk_AddenSupplierList.Where(Ref => Ref.Delete).ToList())
                                    {
                                        controller.DeleteAddenSupplier(addenSupplier, cmSQL);
                                    }
                                    foreach (AddenSupplier addenSupplier in stockItems.Stk_AddenSupplierList.Where(Ref => !Ref.Delete).ToList())
                                    {
                                        controller.UpdateAddenSupplier(addenSupplier, cmSQL);
                                    }
                                    //JAD
                                }
                                if (stockItems.stk_StockUnitList != null && stockItems.stk_StockUnitList.Count > 0)
                                {
                                    StockUnitController SUcontroller = new StockUnitController();
                                    BarcodeSynController BScontroller = new BarcodeSynController();
                                    ChangeItmPriceController CIPcontroller = new ChangeItmPriceController();

                                    foreach (StockUnit unit in stockItems.stk_StockUnitList.Where(U => U.Delete).ToList())
                                    {
                                        if (unit.Stk_BarcodeList != null && unit.Stk_BarcodeList.Count > 0)
                                        {

                                            foreach (BarcodeSyn barcode in unit.Stk_BarcodeList.Where(barCode => barCode.BSy_UnitCode?.ToUpper() == unit.SUnit_Unit?.ToUpper() && barCode.Bsy_Stkid == unit.SUnit_StkId).ToList())
                                            {
                                                BScontroller.DeleteBarcodeSyn(barcode, cmSQL);
                                            }
                                        }

                                        SUcontroller.DeleteStockUnit(unit, cmSQL);
                                    }
                                    foreach (StockUnit unit in stockItems.stk_StockUnitList.Where(U => !U.Delete).ToList())
                                    {
                                        if (unit.Stk_BarcodeList != null && unit.Stk_BarcodeList.Count > 0)
                                        {

                                            foreach (BarcodeSyn barcode in unit.Stk_BarcodeList.Where(barCode => barCode.BSy_UnitCode?.ToUpper() == unit.SUnit_Unit?.ToUpper() && barCode.Bsy_Stkid == unit.SUnit_StkId && barCode.Delete).ToList())
                                            {
                                                BScontroller.DeleteBarcodeSyn(barcode, cmSQL);
                                            }
                                            foreach (BarcodeSyn barcode in unit.Stk_BarcodeList.Where(barCode => barCode.BSy_UnitCode?.ToUpper() == unit.SUnit_Unit?.ToUpper() && barCode.Bsy_Stkid == unit.SUnit_StkId && !barCode.Delete).ToList())
                                            {
                                                BScontroller.UpdateBarcodeSyn(barcode, cmSQL);
                                            }
                                        }
                                        SUcontroller.UpdateStockUnit(unit, cmSQL);
                                        if (stockItems.stk_changeItmPriceList.FirstOrDefault(p => p.Cip_Unit?.ToUpper() == unit.SUnit_Unit?.ToUpper() && p.Cip_StkId == unit.SUnit_StkId) != null)
                                            CIPcontroller.InsertChangeItmPrice(stockItems.stk_changeItmPriceList
                                                .FirstOrDefault(p => p.Cip_Unit?.ToUpper() == unit.SUnit_Unit?.ToUpper() && p.Cip_StkId == unit.SUnit_StkId), cmSQL);

                                    }

                                    if (stockItems.Stk_LinkStockDept != null)
                                    {
                                        LinkStockDeptController controller = new LinkStockDeptController();
                                        if (stockItems.Stk_LinkStockDept.lsd_DeptRecID != null)
                                        {
                                            controller.UpdateLinkStockDept(stockItems.Stk_LinkStockDept, cmSQL);
                                        }
                                        else
                                        {
                                            controller.DeleteLinkStockDept(stockItems.Stk_LinkStockDept, cmSQL);
                                        }

                                    }

                                }

                                //stockItems.ClsDynamicFields.SaveDynamicFields(cmSQL);

                                transaction.Commit();

                            }
                            catch (Exception ex)
                            {
                                try
                                {
                                    transaction.Rollback();
                                    Console.WriteLine(ex.Message);
                                }
                                catch (Exception)
                                { }
                            }
                        }
                    }

                }
            }
            else if (FromTemp.Contains("PI"))
            {
                string strQuery = $@"UPDATE  SI
                                     SET     SI.Stk_AvlbQty1 =  Q.NetQty ,
                                             SI.Stk_ActCostLL = BU.CostLL ,
                                             SI.Stk_ActCostFC = BU.CostFC ,
                                             SI.Stk_ActCostCC = BU.CostCC ,
                                             SI.Stk_SLCoef = CASE WHEN BU.[Coef] IS NULL THEN Stk_SLCoef
                                                                  ELSE BU.[Coef]
                                                             END ,
                                             SI.Stk_1SlPriceLC = CASE WHEN BU.[PriceLL] IS NULL THEN Stk_1SlPriceLC
                                                                      ELSE BU.[PriceLL]
                                                                 END ,
                                             SI.Stk_1SlPrice$ = CASE WHEN BU.[PriceCC] IS NULL THEN Stk_1SlPrice$
                                                                     ELSE BU.[PriceCC] 
                                                                END ,
                                     {(ClsConfig.locationDirectives.Ld_SalePriceCalculationMode == "X" || ClsConfig.locationDirectives.Ld_SalePriceCalculationMode == "Y" ? 
                                     $@" SI.Stk_Qty1stPrice = CASE WHEN BU.[Coef2] IS NULL THEN Stk_Qty1stPrice
                                                                       ELSE BU.[Coef2]
                                                                  END ,
                                             SI.Stk_2SLPriceLC = CASE WHEN BU.[Price2LL] IS NULL
                                                                      THEN Stk_2SLPriceLC
                                                                      ELSE BU.[Price2LL]
                                                                 END ,
                                             SI.Stk_2SLPrice$ = CASE WHEN BU.[Price2CC] IS NULL THEN Stk_2SLPrice$
                                                                     ELSE BU.[Price2CC]
                                                                END ,
                                             SI.Stk_Qty2stPrice = CASE WHEN BU.[Coef3] IS NULL THEN Stk_Qty2stPrice
                                                                       ELSE BU.[Coef3]
                                                                  END ,
                                             SI.Stk_3SLPriceLC = CASE WHEN BU.[Price3LL] IS NULL
                                                                      THEN Stk_3SLPriceLC
                                                                      ELSE BU.[Price3LL]
                                                                 END ,
                                             SI.Stk_3SLPrice$ = CASE WHEN BU.[Price3CC] IS NULL THEN Stk_3SLPrice$
                                                                     ELSE BU.[Price3CC]
                                                                END ,
                                            " : $@""
                                     )}
                                            SI.Stk_LstCostLL = CASE WHEN BU.[LCostLL] IS NULL THEN Stk_LstCostLL
                                                                     ELSE BU.[LCostLL]
                                                                END ,
                                             SI.Stk_LstCostFC = CASE WHEN BU.[LCostFC] IS NULL THEN Stk_LstCostFC
                                                                     ELSE BU.[LCostFC]
                                                                END ,
                                             SI.Stk_LstCostCC = CASE WHEN BU.[LCostCC] IS NULL THEN Stk_LstCostCC
                                                                     ELSE BU.[LCostCC]
                                                                END ,
                                            {(TrType != "X" ? 
                                                (ClsConfig.oHSystem != null && ClsConfig.oHSystem.Osh_Active.ToUpper() == "Y".ToUpper() && (ClsConfig.locationDirectives.Ld_UpdatePotentialCostPI == "1"  || ClsConfig.locationDirectives.Ld_UpdatePotentialCostPI == "3") ) 
                                                ||
                                                (ClsConfig.oHSystem != null && ClsConfig.oHSystem.Osh_Active.ToUpper() == "N".ToUpper() && (ClsConfig.locationDirectives.Ld_UpdatePotentialCostPI == "1" || ClsConfig.locationDirectives.Ld_UpdatePotentialCostPI == "2"))
                                                ?
                                                $@"SI.stk_PotcostLL = CASE WHEN ( BU.[PCostLL] IS NULL )
                                                                          AND Stk_ModSal = 'Y' THEN stk_PotcostLL
                                                                     WHEN ( (BU.[PCostLL] IS NULL) )
                                                                          AND Stk_ModSal = 'N' THEN stk_PotcostLL
                                                                     ELSE BU.[PCostLL]
                                                                END ,
                                                   SI.stk_PotcostCC = CASE WHEN ( BU.[PCostCC] IS NULL )
                                                                          AND Stk_ModSal = 'Y' THEN stk_PotcostCC
                                                                     WHEN ( (BU.[PCostCC] IS NULL) )
                                                                          AND Stk_ModSal = 'N' THEN stk_PotcostCC
                                                                     ELSE BU.[PCostCC]
                                                                END ,
                                                  SI.stk_PotCostFC = CASE WHEN ( BU.[PCostFC] IS NULL )
                                                                          AND Stk_ModSal = 'Y' THEN stk_PotCostFC
                                                                     WHEN ( (BU.[PCostLL] IS NULL) )
                                                                          AND Stk_ModSal = 'N' THEN stk_PotCostFC
                                                                     ELSE BU.[PCostFC]
                                                                END ," 
                                                : $@" SI.stk_PotcostLL = CASE WHEN ( ( BU.[PCostLL] IS NULL )
                                                                            OR ( BU.PriceLL IS NULL )
                                                                          )
                                                                          AND Stk_ModSal = 'Y' THEN stk_PotcostLL
                                                                     WHEN ( ( BU.[PCostLL] IS NULL )
                                                                            OR ( BU.PriceLL IS NULL )
                                                                          )
                                                                          AND Stk_ModSal = 'N' THEN stk_PotcostLL
                                                                     ELSE BU.[PCostLL]
                                                                END ,
                                                     SI.stk_PotcostCC = CASE WHEN ( ( BU.[PCostCC] IS NULL )
                                                                            OR ( BU.PriceCC IS NULL )
                                                                          )
                                                                          AND Stk_ModSal = 'Y' THEN stk_PotcostCC
                                                                     WHEN ( ( BU.[PCostCC] IS NULL )
                                                                            OR ( BU.PriceCC IS NULL )
                                                                          )
                                                                          AND Stk_ModSal = 'N' THEN stk_PotcostCC
                                                                     ELSE BU.[PCostCC]
                                                                END ,
                                             SI.stk_PotCostFC = CASE WHEN ( ( BU.[PCostFC] IS NULL )
                                                                            OR ( BU.PriceLL IS NULL )
                                                                          )
                                                                          AND Stk_ModSal = 'Y' THEN stk_PotCostFC
                                                                     WHEN ( ( BU.[PCostLL] IS NULL )
                                                                            OR ( BU.PriceLL IS NULL )
                                                                          )
                                                                          AND Stk_ModSal = 'N' THEN stk_PotCostFC
                                                                     ELSE BU.[PCostFC]
                                                                END ," 
                                                : $@"")}
                                             
                                            
                                             SI.stk_potchangeDate = CASE WHEN BU.[PCostLL] IS NULL
                                                                         THEN stk_potchangeDate
                                                                         ELSE GETDATE()
                                                                    END,
                                             SI.stk_Ustmp = '{ClsConfig.LoginUser}',
                                             SI.stk_Dstmp = GETDATE()
                                     FROM    dbo.tbl_StockItm SI
                                             INNER JOIN ( SELECT   PT.Item ,
                                                             SUM( PT.Qty1 * SU.SUnit_PackingBasic) AS NetQty
                                                    FROM     {FromTemp} PT
                                                             JOIN dbo.tbl_StockUnit SU ON SU.SUnit_StkId = PT.Item
                                                                                          AND SU.SUnit_Unit = PT.UnitCode
                                                    GROUP BY PT.Item
                                                  ) Q ON SI.Stk_StkId = Q.Item
                                             LEFT JOIN ( SELECT  PT.*
                                                         FROM    {FromTemp} PT
                                                                 JOIN dbo.tbl_StockUnit SU ON SU.SUnit_StkId = PT.Item
                                                                                              AND SU.SUnit_Unit = PT.UnitCode
                                                        WHERE   SU.SUnit_PackingBasic = 1
                                                       ) BU ON SI.Stk_StkId = BU.Item;";

                sqlCommand.CommandText = strQuery;
                sqlCommand.CommandTimeout = ClsConfig.SQLCommandTimeout;
                sqlCommand.ExecuteNonQuery();
            }
            else if(FromTemp.Contains("DN"))
            {
                string strQuery = $@"UPDATE SI
                                        SET
                                            SI.Stk_ActCostLL = T.CostLL,
                                            SI.Stk_ActCostFC = T.CostFC,
                                        
                                            SI.Stk_SLCoef =
                                                CASE
                                                    WHEN T.[Coef] IS NULL THEN SI.Stk_SLCoef
                                                    ELSE T.[Coef]
                                                END,
                                        
                                            SI.Stk_1SlPriceLC =
                                                CASE
                                                    WHEN T.[PriceLL] IS NULL THEN SI.Stk_1SlPriceLC
                                                    ELSE T.[PriceLL]
                                                END,
                                        
                                            SI.Stk_1SlPrice$ =
                                                CASE
                                                    WHEN T.[PriceCC] IS NULL THEN SI.Stk_1SlPrice$
                                                    ELSE T.[PriceCC]
                                                END,
                                        {(ClsConfig.locationDirectives.Ld_SalePriceCalculationMode == "X" || ClsConfig.locationDirectives.Ld_SalePriceCalculationMode == "Y" ? 
                                        $@"SI.Stk_Qty1stPrice =
                                                CASE
                                                    WHEN T.[Coef2] IS NULL THEN SI.Stk_Qty1stPrice
                                                    ELSE T.[Coef2]
                                                END,
                                        
                                            SI.Stk_2SlPriceLC =
                                                CASE
                                                    WHEN T.[Price2LL] IS NULL THEN SI.Stk_2SlPriceLC
                                                    ELSE T.[Price2LL]
                                                END,
                                        
                                            SI.Stk_2SlPrice$ =
                                                CASE
                                                    WHEN T.[Price2CC] IS NULL THEN SI.Stk_2SlPrice$
                                                    ELSE T.[Price2CC]
                                                END,
                                        
                                            SI.Stk_Qty2stPrice =
                                                CASE
                                                    WHEN T.[Coef3] IS NULL THEN SI.Stk_Qty2stPrice
                                                    ELSE T.[Coef3]
                                                END,
                                        
                                            SI.Stk_3SlPriceLC =
                                                CASE
                                                    WHEN T.[Price3LL] IS NULL THEN SI.Stk_3SlPriceLC
                                                    ELSE T.[Price3LL]
                                                END,
                                        
                                            SI.Stk_3SlPrice$ =
                                                CASE
                                                    WHEN T.[Price3CC] IS NULL THEN SI.Stk_3SlPrice$
                                                    ELSE T.[Price3CC]
                                                END," 
                                                :
                                                ""
                                        )}
                                            
                                        {(ClsConfig.locationDirectives.Ld_DNCostToUse == "2" ?
                                        
                                        $@" SI.Stk_LstCostLL =
                                                CASE
                                                    WHEN T.[PCostLL] IS NULL THEN SI.Stk_LstCostLL
                                                    ELSE T.[PCostLL]
                                                END,
                                        
                                            SI.Stk_LstCostFC =
                                                CASE
                                                    WHEN T.[PCostFC] IS NULL THEN SI.Stk_LstCostFC
                                                    ELSE T.[PCostFC]
                                                END," : "")}
                                 
                                        
                                            SI.Stk_Dstmp = GETDATE(),
                                            SI.Stk_Ustmp = '{ClsConfig.LoginUser}',
                                        {(ClsConfig.locationDirectives.Ld_DNCostToUse == "1" && TrType != "X" ?
                                       
                                                (ClsConfig.oHSystem != null && ClsConfig.oHSystem.Osh_Active.ToUpper() == "Y".ToUpper() && (ClsConfig.locationDirectives.Ld_UpdatePotentialCostPI == "1" || ClsConfig.locationDirectives.Ld_UpdatePotentialCostPI == "3"))
                                                ||
                                                (ClsConfig.oHSystem != null && ClsConfig.oHSystem.Osh_Active.ToUpper() == "N".ToUpper() && (ClsConfig.locationDirectives.Ld_UpdatePotentialCostPI == "1" || ClsConfig.locationDirectives.Ld_UpdatePotentialCostPI == "2")) ?
                                        $@"  SI.Stk_PotcostLL =
                                                CASE
                                                    WHEN T.[PCostLL] IS NULL
                                                         AND SI.Stk_ModSal = 'Y'
                                                        THEN SI.Stk_PotcostLL
                                        
                                                    WHEN T.[PCostLL] IS NULL
                                                         AND SI.Stk_ModSal = 'N'
                                                        THEN SI.Stk_PotcostLL
                                        
                                                    ELSE T.[PCostLL]
                                                END,
                                         SI.Stk_PotCostCC =
                                                CASE
                                                    WHEN T.[PCostCC] IS NULL
                                                         AND SI.Stk_ModSal = 'Y'
                                                        THEN SI.Stk_PotCostCC
                                        
                                                    WHEN T.[PCostLL] IS NULL
                                                         AND SI.Stk_ModSal = 'N'
                                                        THEN SI.Stk_PotCostCC
                                        
                                                    ELSE T.[PCostCC]
                                                END,
                                         SI.Stk_PotCostFC =
                                                CASE
                                                    WHEN T.[PCostFC] IS NULL
                                                         AND SI.Stk_ModSal = 'Y'
                                                        THEN SI.Stk_PotCostFC
                                        
                                                    WHEN T.[PCostLL] IS NULL
                                                         AND SI.Stk_ModSal = 'N'
                                                        THEN SI.Stk_PotCostFC
                                        
                                                    ELSE T.[PCostFC]
                                                END,
                                         SI.Stk_PotChangeDate =
                                                CASE
                                                    WHEN T.[PCostLL] IS NULL
                                                        THEN SI.Stk_PotChangeDate
                                                    ELSE GETDATE()
                                                END" 
                                           : 
                                           
                                      $@"  SI.Stk_PotcostLL =
                                                CASE
                                                    WHEN (T.[PCostLL] IS NULL OR PriceLL is null)
                                                         AND SI.Stk_ModSal = 'Y'
                                                        THEN SI.Stk_PotcostLL
                                        
                                                    WHEN (T.[PCostLL] IS NULL OR PriceLL is null)
                                                         AND SI.Stk_ModSal = 'N'
                                                        THEN SI.Stk_PotcostLL
                                        
                                                    ELSE T.[PCostLL]
                                                END,
                                         SI.Stk_PotCostCC =
                                                CASE
                                                    WHEN (T.[PCostCC] IS NULL OR PriceLL is null)
                                                         AND SI.Stk_ModSal = 'Y'
                                                        THEN SI.Stk_PotCostCC
                                        
                                                    WHEN (T.[PCostLL] IS NULL OR PriceLL is null)
                                                         AND SI.Stk_ModSal = 'N'
                                                        THEN SI.Stk_PotCostCC
                                        
                                                    ELSE T.[PCostCC]
                                                END,
                                         SI.Stk_PotCostFC =
                                                CASE
                                                    WHEN (T.[PCostFC] IS NULL OR PriceLL is null)
                                                         AND SI.Stk_ModSal = 'Y'
                                                        THEN SI.Stk_PotCostFC
                                        
                                                    WHEN (T.[PCostLL] IS NULL OR PriceLL is null)
                                                         AND SI.Stk_ModSal = 'N'
                                                        THEN SI.Stk_PotCostFC
                                        
                                                    ELSE T.[PCostFC]
                                                END,
                                         SI.Stk_PotChangeDate =
                                                CASE
                                                    WHEN T.[PCostLL] IS NULL
                                                        THEN SI.Stk_PotChangeDate
                                                    ELSE GETDATE()
                                                END"                                                                               
                                        : 
                                        $@"    SI.Stk_PotChangeDate =
                                                CASE
                                                    WHEN T.[PCostLL] IS NULL
                                                        THEN SI.Stk_PotChangeDate
                                                    ELSE GETDATE()
                                                END
                                        ")}                                                                                                                      
                                        FROM dbo.tbl_StockItm AS SI
                                        INNER JOIN {FromTemp} AS T
                                            ON T.Item = SI.stk_StkId
                                            AND T.UnitCode = SI.stk_UnitCode;";

                sqlCommand.CommandText = strQuery;
                sqlCommand.CommandTimeout = ClsConfig.SQLCommandTimeout;
                sqlCommand.ExecuteNonQuery();
            }
        }

        public void DeleteStockItems(StockItems stockItems, SqlCommand sqlCommand = null)
        {


            using (SqlConnection cnSQL = new SqlConnection(ClsConfig.SQLConnectionString))
            {
                cnSQL.Open();
                using (SqlTransaction transaction = cnSQL.BeginTransaction())
                {

                    using (SqlCommand cmSQL = cnSQL.CreateCommand())
                    {
                        cmSQL.Transaction = transaction;
                        try
                        {
                            if (stockItems.Stk_Online != null)
                            {
                                StockOnlineController OnlineController = new StockOnlineController();
                                OnlineController.DeleteStockOnline(stockItems.Stk_Online, cmSQL);
                            }
                            if (stockItems.Stk_DiscExc != null)
                            {
                                StockDiscExclusionController DiscExcController = new StockDiscExclusionController();
                                DiscExcController.DeleteStockDiscExclusion(stockItems.Stk_DiscExc, cmSQL);
                            }

                            if (stockItems.stk_StockUnitList != null && stockItems.stk_StockUnitList.Count > 0)
                            {

                                StockUnitController SUcontroller = new StockUnitController();
                                ChangeItmPriceController CIPcontroller = new ChangeItmPriceController();
                                foreach (StockUnit unit in stockItems.stk_StockUnitList)
                                {
                                    if (unit.Stk_BarcodeList != null && unit.Stk_BarcodeList.Count > 0)
                                    {
                                        BarcodeSynController BScontroller = new BarcodeSynController();

                                        foreach (BarcodeSyn barcode in unit.Stk_BarcodeList)
                                        {
                                            BScontroller.DeleteBarcodeSyn(barcode, cmSQL);
                                        }

                                    }
                                    SUcontroller.DeleteStockUnit(unit, cmSQL);
                                }
                                CIPcontroller.DeleteChangeItmPrice(stockItems.Stk_StkId,stockItems.Stk_UnitCode, cmSQL);
                            }
                            if (stockItems.Stk_AddenSupplierList != null && stockItems.Stk_AddenSupplierList.Count > 0)
                            {
                                AddenSupplierController controller = new AddenSupplierController();

                                foreach (AddenSupplier addenSupplier in stockItems.Stk_AddenSupplierList)
                                {
                                    controller.DeleteAddenSupplier(addenSupplier, cmSQL);
                                }
                            }
                            if (stockItems.Stk_OHStockAdd != null)
                            {
                                OHStockAddController OHController = new OHStockAddController();
                                OHController.DeleteOHStockAdd(stockItems.Stk_OHStockAdd, cmSQL);
                            }
                            if (stockItems.stk_FilePrice != null)
                            {
                                StockFilePriceController FilePriceController = new StockFilePriceController();
                                FilePriceController.DeleteStockFilePrice(stockItems.stk_FilePrice, cmSQL);
                            }
                            if (stockItems.Stk_StockQtyExpiryList != null && stockItems.Stk_StockQtyExpiryList.Count > 0)
                            {
                                foreach (StockQtyExpiry stockQtyExp in stockItems.Stk_StockQtyExpiryList)
                                {
                                    StockQtyExpiryController QtyExpController = new StockQtyExpiryController();
                                    QtyExpController.DeleteStockQtyExpiry(stockQtyExp, cmSQL);
                                }

                            }
                            if (stockItems.Stk_StockQtyList != null && stockItems.Stk_StockQtyList.Count > 0)
                            {
                                foreach (StockQty stockQty in stockItems.Stk_StockQtyList)
                                {
                                    StockQtyController QtyController = new StockQtyController();
                                    QtyController.DeleteStockQty(stockQty, cmSQL);
                                }

                            }
                            if (stockItems.Stk_LinkStockDept != null)
                            {
                                LinkStockDeptController StockLinkDepController = new LinkStockDeptController();
                                StockLinkDepController.DeleteLinkStockDept(stockItems.Stk_LinkStockDept, cmSQL);
                            }

                            string strQuery = $@"DELETE FROM tbl_StockItm 
                                  WHERE Stk_Loc = @Stk_Loc
                                    AND Stk_StkId = @Stk_StkId";

                            ExecuteSqlQuery(stockItems, strQuery, cmSQL);

                            //stockItems.ClsDynamicFields.DeleteDynamicFields(cmSQL);

                            transaction.Commit();

                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                transaction.Rollback();
                                Console.WriteLine(ex.Message);
                            }
                            catch (Exception)
                            { }
                        }

                    }
                }
            }

        }

        public StockItems GetStockItems(int Code = 0 , string Stk_Supplier = null , string Stk_Category = null , string Stk_Serial = null , bool ActiveOnly = false, string AndCondition = null)
        {
            using (SqlConnection cnSQL = new SqlConnection(ClsConfig.SQLConnectionString))
            {
                string strQuery = GetStockItemsQuery(ClsCom.QueryType.Normal, Code != 0 ? $@" And Stk_StkID = @Stk_StkID {AndCondition}" : $@" AND Stk_Supplier = @Stk_Supplier AND Stk_Category = @Stk_Category AND Stk_Serial = @Stk_Serial {AndCondition}");

                cnSQL.Open();

                using (SqlCommand cmSQL = new SqlCommand(strQuery, cnSQL))
                {
                    cmSQL.Parameters.AddWithValue("@Stk_Loc", ClsConfig.LocationCode);
                    if (!string.IsNullOrEmpty(Stk_Supplier) && !string.IsNullOrEmpty(Stk_Category) && !string.IsNullOrEmpty(Stk_Serial))
                    {
                        cmSQL.Parameters.AddWithValue("@Stk_Supplier", Stk_Supplier);
                        cmSQL.Parameters.AddWithValue("@Stk_Category", Stk_Category);
                        cmSQL.Parameters.AddWithValue("@Stk_Serial", Stk_Serial);

                    }
                    else if (Code != 0)
                    {
                        cmSQL.Parameters.AddWithValue("@Stk_StkID", Code);
                    }

                    using (SqlDataReader drSQL = cmSQL.ExecuteReader())
                    {
                        if (drSQL.Read())
                        {
                            return GetAndFillStockItemsObject(drSQL);
                        }
                    }
                }
            }
            return null;
        }

        public List<StockItems> GetStockItemsList()
        {
            List<StockItems> StockItemsList = new List<StockItems>();
            using (SqlConnection cnSQL = new SqlConnection(ClsConfig.SQLConnectionString))
            {
                string strQuery = GetStockItemsQuery();

                cnSQL.Open();
                using (SqlCommand cmSQL = new SqlCommand(strQuery, cnSQL))
                {
                    cmSQL.Parameters.AddWithValue("@Stk_Loc", ClsConfig.LocationCode);

                    using (SqlDataReader drSQL = cmSQL.ExecuteReader())
                    {
                        while (drSQL.Read())
                        {
                            StockItems StockItems = GetAndFillStockItemsObject(drSQL);
                            StockItemsList.Add(StockItems);
                        }
                    }
                }
            }
            return StockItemsList;
        }

        public DataTable GetStockItemsDataTable(ClsCom.QueryType QueryType = ClsCom.QueryType.Normal, string AndCondition = "", bool ActiveOnly = false)
        {
            string strQuery = GetStockItemsQuery(QueryType, AndCondition, ActiveOnly);

            using (DataTable dtData = new DataTable())
            {
                using (SqlDataAdapter daSQL = new SqlDataAdapter(strQuery, ClsConfig.SQLConnectionString))
                {
                    daSQL.SelectCommand.Parameters.AddWithValue("@Stk_Loc", ClsConfig.LocationCode);

                    daSQL.Fill(dtData);
                    return dtData;
                }
            }
        }

        public DataTable GetAddOnRefDataTable(string AndCondition = "")
        {
            string strQuery = $@" SELECT   DISTINCT
                                        'false' AS [chk] ,
                                        Stk_StkId AS id ,
                                        Stk_AddOnRef AS [Add On Reference]
                                FROM    tbl_StockItm
                                WHERE   COALESCE(Stk_AddOnRef, '') <> ''
                                        AND Stk_Status = 'A'
                                        AND Stk_Loc = @Stk_Loc 

                               {AndCondition} ";

            using (DataTable dtData = new DataTable())
            {
                using (SqlDataAdapter daSQL = new SqlDataAdapter(strQuery, ClsConfig.SQLConnectionString))
                {
                    daSQL.SelectCommand.Parameters.AddWithValue("@Stk_Loc", ClsConfig.LocationCode);

                    daSQL.Fill(dtData);
                    return dtData;
                }
            }
        }

        public DataTable GetPurchaseBySupCatItemDataTable()
        {
            string strQuery = $@" SELECT	stk_stkid,
		                                    stk_supplier [Supplier],
		                                    sup_name [Supp. Name],
		                                    stk_Category [Category],
		                                    Cat_Desc [Cat. Name] ,
		                                    stk_Serial [Serial],
		                                    stk_AddOnRef [Add On Ref],
		                                    stk_Desc [Description],
		                                    stk_Barcode1 [Barcode1],
		                                    stk_Barcode2 [Barcode2],
		                                    ISNULL(stk_actcostLL,0) [Avg Unit Cost {ClsConfig.gbCurr1}],
                                            CASE WHEN stk_FCCurr = '{ClsConfig.gbCurr2}' THEN ISNULL(Stk_ActCostFC,0) ELSE CAST(dbo.convert5('{ClsConfig.gbApp}','{ClsConfig.gbComp}','{ClsConfig.gbYear}',isnull(stk_ActCostLL, 0),'{ClsConfig.gbCurr1}','{ClsConfig.gbCurr2}',convert(varchar(10), getdate(), 101)) as decimal(18, 3)) end[Avg Unit Cost '{ClsConfig.gbCurr2}']
                                    FROM tbl_stockitm
                                    LEFT JOIN tbl_suppliers ON stk_supplier = sup_code
                                    INNER JOIN tbl_categories ON stk_Category = cat_code";

            using (DataTable dtData = new DataTable())
            {
                using (SqlDataAdapter daSQL = new SqlDataAdapter(strQuery, ClsConfig.SQLConnectionString))
                {
                    daSQL.SelectCommand.Parameters.AddWithValue("@Stk_Loc", ClsConfig.LocationCode);

                    daSQL.Fill(dtData);
                    return dtData;
                }
            }
        }

        public DataTable GetStockListDataTable(string _OnlineShop, bool ZeroQty, string Curr, string DateFrom = "", string DateTo = "", string AndCondition = "")
        {
            WarehousesController WarehousesController = new WarehousesController();
            List<string> whCodes = WarehousesController.GetWarehousesList().Select(w => w.Wh_Code).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();

            string strQuery = $@" SELECT 'false' AS [chk],
		                                 stk_loc,
		                                 Ld_LocationName,
		                                 stk_stkid,
		                                 stk_HoStkID,
		                                 stk_supplier,
		                                 sup_name,
		                                 stk_category,
		                                 cat_desc,
		                                 stk_serial,
		                                 CONVERT(VARCHAR(10),stk_crdate,103) AS [Creation Date],
		                                 CASE stk_status WHEN 'A' THEN 'Y' WHEN 'I' THEN 'N' END AS [Active],
		                                 --stk_qtycontrol,
		                                 --CASE stk_returndel WHEN 'R' THEN 'Return' WHEN 'D' THEN 'Delete' WHEN 'I' THEN 'Inventory' END AS [Item Marked],
		                                 stk_modsal,
		                                 stk_descpos,
		                                 stk_desc,
		                                 stk_barcode1,
		                                 CASE stk_orbarcode1 WHEN 'O' THEN 'Original' WHEN 'G' THEN 'Generated' WHEN 'X' THEN 'Other' END AS [Barcode1 Origin],
		                                 stk_barcode2,
		                                 CASE stk_orbarcode2 WHEN 'O' THEN 'Original' WHEN 'G' THEN 'Generated' WHEN 'X' THEN 'Other' END AS [Barcode2 Origin],
		                                 stk_packing,
		                                 stk_unitcode,
		                                 stk_unitspack,
		                                 stk_transp,
		                                 CASE stk_itmservice WHEN 'I' THEN 'Item' WHEN 'S' THEN 'Service' END AS [Type],
		                                 stk_assembly,
		                                 stk_3rdparty,
		                                 stk_PLUNbr,
		                                 stk_Goodsloc,
		                                 CASE WHEN stk_gift = 'C' THEN 'C' ELSE 'L' END AS PriceCurr,
		                                --stk_itmprom,
                                        ";
            for (int i = 0; i < whCodes.Count; i++)
            {
                strQuery += $@" t{whCodes[i]}.Qty_Min as [t{whCodes[i]}.Qty_Min],
                                t{whCodes[i]}.Qty_Max as [t{whCodes[i]}.Qty_Max],
                                t{whCodes[i]}.Qty_bbf as [t{whCodes[i]}.Qty_bbf],
                                t{whCodes[i]}.Qty_avlbqty as [t{whCodes[i]}.Qty_avlbqty],
                                t{whCodes[i]}.Qty_return as [t{whCodes[i]}.Qty_return],";
            }
            strQuery += $@"     CONVERT(VARCHAR(10),stk_bbfdate1,103) AS [BB/F Date],
                                stk_bbfcostlc,
		                        stk_bbfcostfc,
		                        CONVERT(VARCHAR(10),stk_ldatecount,103) AS [Last Date Physical Count],
                                stk_actcostll,
		                        stk_actcostfc,
		                        stk_lstcostll,
		                        stk_lstcostfc,
		                        stk_fccurr,
		                        stk_potcostll,
		                        stk_potcostfc,
		                        CONVERT(VARCHAR(20),stk_potchangedate,109) AS [Change Date/Time Stamp],
		                        stk_1slpricelc,
		                        stk_1slprice$,
		                        stk_Qty1stprice,
		                        stk_2slpricelc,
		                        stk_2slprice$,
		                        stk_Qty2stprice,
		                        stk_3slpricelc,
		                        stk_3slprice$,
		                        stk_slcoef,
		                        CASE stk_lstatcoef WHEN 'C' THEN 'Computed' WHEN 'U' THEN 'User Defined' END AS [Last Status for Sales Coef],
		                        stk_usercoef,
		                        u1.user_name AS [User Name that Changed Coef],
		                        stk_discwh,
		                        stk_ytdqtyin,
		                        stk_ytdqtyout,
		                        stk_ytdamtinll,
		                        stk_ytdamtin$,
		                        stk_ytdamtoutll,
		                        stk_ytdamtout$,
		                        stk_subjectvat,
		                        CASE stk_vattype WHEN 'N' THEN 'Normal' WHEN 'E' THEN 'Embedded' END AS [VAT Type],
		                        -- CASE stk_returndel WHEN '0' THEN 'N/A' WHEN '1' THEN 'VDR' WHEN '2' THEN 'VRR' WHEN '3' THEN 'VNA' ELSE NULL END [Vat Status],
		                        stk_vatper,
		                        stk_addonref,
		                        CONVERT(VARCHAR(20),stk_hoexport,109) AS [Date/Time of Export to HO],
		                        CONVERT(VARCHAR(20),stk_hoimport,109) AS [Date/Time of Import to HO],
		                        CONVERT(VARCHAR(10),stk_staticdstmp,103) AS [Date Static Data Modifications],
		                        stk_staticUstmp,
		                        u2.user_name AS [User Name for Static Data Modifications],
		                        CONVERT(VARCHAR(10),stk_dstmp,103) AS [Date Program Data Modifications],
		                        stk_Ustmp,
		                        u3.user_name AS [User Name for Program Data modifications],
		                        sfp_potunitprice,
		                        sfp_potItemDiscount,
		                        sfp_potCurr,
		                        sfp_potfollowinvdisc,
		                        sfp_potinvdiscount,
		                        sfp_potpurqty,
		                        sfp_potpromqty,
		                        sfp_potfreeqty,
		                        sfp_potextraqty,
		                        sfp_potcontractqty,
		                        sfp_potremarks,
		                        sfp_potuseitmdisc,
		                        sfp_potuseinvdisc,
		                        sfp_potusepromqty,
		                        sfp_lastcostparam,
		                        NULL AS [Remarks] 
                                {(ClsConfig.oHSystem.Osh_Active == "Y" ? ",Soh_CustPer ,Soh_FixedOHPer ,Soh_AppCostCoef " : "")}
                                ,Ads_SuppRef,  
		                        CAST((CASE WHEN tbl_StockItm.stk_crdate < CAST('01/01/' + CAST(YEAR(GETDATE()) AS VARCHAR(4)) AS DATETIME) THEN tbl_StockItm.Stk_YTDQtyOut/(DATEDIFF(DAY, CAST('01/01/' + CAST(YEAR(GETDATE()) AS VARCHAR(4)) AS DATETIME), GETDATE())+1) * 30.00 WHEN DATEDIFF(DAY,tbl_StockItm.stk_crdate, GETDATE()) = 0 THEN tbl_StockItm.Stk_YTDQtyOut ELSE tbl_StockItm.Stk_YTDQtyOut/ (DATEDIFF(DAY,tbl_StockItm.stk_crdate,GETDATE())+1) * 30.00 END ) AS DECIMAL(18,2)) AS [AvgCons],
		                        dep_Code ,
		                        dep_Desc ,
		                        grp_Code ,
		                        grp_Desc ,
		                        SG1_Code ,
		                        SG1_Desc ,
		                        SG2_Code ,
		                        SG2_Desc ,
		                        SG3_Code ,
		                        SG3_Desc ,  
		                        SG4_Code ,
		                        SG4_Desc ,
		                        SUBSTRING(Stk_GoodsLoc,18,4) Validity, 
		                        SalesQty,
                    		    SalesvbCLC,
                                WSQty,
                                WSvbCLC,
                                sfp_lcitmdiscount,
		                        sfp_lcinvdiscount
                                {(_OnlineShop == "Y" ? ",case when not Onl_Stkid is null then 'Y' else 'N' end [Online]" : "")}
                                --,sfp_Expirydate [Expiry] 
                                ,CASE WHEN sup_vatclass in ('1','2') then 'Local' else 'Foreign' end [Supplier Type] 
                        FROM tbl_stockitm 
		                LEFT JOIN tbl_LocationDirectives ON ld_loc = stk_loc 
		                LEFT JOIN tbl_suppliers ON stk_supplier = sup_code AND stk_loc = sup_loc 
		                LEFT JOIN tbl_categories ON cat_code = stk_category AND cat_loc = stk_loc 
		                LEFT JOIN Com_UserInfo U1 ON u1.user_code = stk_usercoef 
		                LEFT JOIN com_userinfo U2 ON U2.user_code = stk_staticUstmp 
		                LEFT JOIN com_userinfo U3 ON U3.user_code = stk_Ustmp 
		                LEFT JOIN tbl_stockfileprice ON stk_loc = sfp_loc AND stk_Stkid = sfp_stkid 
		                LEFT JOIN tbl_AddenSupplier ON Stk_Stkid = Ads_StkId AND Ads_Supplier = Stk_supplier 
                        {(ClsConfig.oHSystem.Osh_Active == "Y" ? " LEFT JOIN tbl_OHStockAdd ON stk_Stkid = Soh_Stkid " : "")}   
                        LEFT JOIN tbl_LinkStockDept ON lsd_StkRecID = stk_Stkid 
		                LEFT JOIN tbl_Department ON lsd_DeptRecID = dep_RecID 
		                LEFT JOIN tbl_Group ON lsd_GrpRecID = grp_RecID 
		                LEFT JOIN tbl_SubGroup1 ON lsd_SG1RecID = SG1_RecID 
		                LEFT JOIN tbl_SubGroup2 ON lsd_SG2RecID = SG2_RecID 
		                LEFT JOIN tbl_SubGroup3 ON lsd_SG3RecID = SG3_RecID 
		                LEFT JOIN tbl_SubGroup4 ON lsd_SG4RecID = SG4_RecID 
                        {(_OnlineShop == "Y" ? "LEFT JOIN tbl_StockOnline on stk_stkid = Onl_Stkid" : "")}";
            
            for (int i = 0; i < whCodes.Count; i++)
                strQuery += $@" LEFT JOIN tbl_StockQty t{whCodes[i]} ON stk_stkid = t{whCodes[i]}.Qty_stkid AND t{whCodes[i]}.Qty_wh = {whCodes[i]}";

            strQuery += $@" LEFT JOIN (SELECT sms_Stkid, SUM(CASE sms_status WHEN 'S' THEN sms_totqty WHEN 'R' THEN -1 * sms_totqty ELSE 0 END) AS SalesQty, ROUND(SUM (CASE WHEN sms_type IN ('F','P','G','C') THEN 0 WHEN sms_type = 'N' THEN CASE sms_status WHEN 'S' THEN sms_totnet WHEN 'R' THEN -1*sms_totnet END END),0) SalesvbCLC FROM tbl_salesmvtsummary WHERE 1 = 1 {(!string.IsNullOrEmpty(DateFrom) ? $"AND Sms_Date >= '{Convert.ToDateTime(DateFrom):MM/dd/yyyy}'" : "")} {(!string.IsNullOrEmpty(DateTo) ? $"AND Sms_Date <= '{Convert.ToDateTime(DateTo):MM/dd/yyyy}'" : "")} GROUP BY sms_Stkid) RetailSales ON stk_stkid = sms_stkid 
                            LEFT JOIN (SELECT ind_stockrecid, SUM(CASE ind_status WHEN 'S' THEN ind_qty WHEN 'R' THEN -1 * ind_qty ELSE 0 END) AS WSQty, ROUND(SUM(CASE WHEN ind_type IN ('F','P','G') THEN 0 WHEN ind_type = 'N' THEN (CASE ind_status WHEN 'S' THEN (CASE 'Curr' WHEN '{ClsConfig.gbCurr1}' THEN (CASE ind_subjtovat WHEN 'Y' THEN (CASE WHEN ISNULL(inh_totalvataddon,0) <> 0 THEN ind_totalpricelc *(1 + ind_vatper/100) ELSE ind_totalpricelc END) ELSE ind_totalpricelc END) WHEN '{ClsConfig.gbCurr2}' THEN (CASE inh_opercurr WHEN '{ClsConfig.gbCurr2}' THEN (CASE ind_subjtovat WHEN 'Y' THEN (CASE WHEN ISNULL (inh_totalvataddon,0) <> 0 THEN ind_totalprice *(1+ind_vatper/100) ELSE ind_totalprice END) ELSE ind_totalprice END) WHEN '{ClsConfig.gbCurr1}' THEN dbo.convert6('{ClsConfig.gbApp}', '{ClsConfig.gbComp}', '{ClsConfig.gbYear}', ISNULL((CASE ind_subjtovat WHEN 'Y' THEN (CASE WHEN ISNULL(inh_totalvataddon,0) <> 0 THEN ind_totalprice * ( 1 + ind_vatper/100 ) ELSE ind_totalprice END) ELSE ind_totalprice END),0),'{ClsConfig.gbCurr1}','{ClsConfig.gbCurr2}',CONVERT(VARCHAR(10),inh_date,101),2) ELSE 0 END) ELSE 0 END) WHEN 'R' THEN (CASE '{Curr}' WHEN '{ClsConfig.gbCurr1}' THEN -1 * (CASE ind_subjtovat WHEN 'Y' THEN (CASE WHEN ISNULL(inh_totalvataddon,0) <> 0 THEN ind_totalpricelc * (1 + ind_vatper/100) ELSE ind_totalpricelc END) ELSE ind_totalpricelc END) WHEN '{ClsConfig.gbCurr2}' THEN -1 * (CASE inh_opercurr WHEN '{ClsConfig.gbCurr2}' THEN (CASE ind_subjtovat WHEN 'Y' THEN (CASE WHEN ISNULL(inh_totalvataddon,0) <> 0 THEN ind_totalprice *(1+ind_vatper/100) ELSE ind_totalprice END) ELSE ind_totalprice END) WHEN '{ClsConfig.gbCurr1}' THEN dbo.convert6('{ClsConfig.gbApp}', '{ClsConfig.gbComp}', '{ClsConfig.gbYear}',ISNULL((CASE ind_subjtovat WHEN 'Y' THEN (CASE WHEN ISNULL(inh_totalvataddon,0) <> 0 THEN ind_totalprice * ( 1 + ind_vatper/100) ELSE ind_totalprice END) ELSE ind_totalprice END),0),'{ClsConfig.gbCurr1}','{ClsConfig.gbCurr2}', CONVERT(VARCHAR(10),inh_date,101),2)ELSE 0 END) ELSE 0 END) ELSE 0 END) ELSE 0 END),0) AS WSvbCLC FROM tbl_invoicedetailwhole INNER JOIN tbl_invoiceheaderwhole ON ind_hdrrecid = inh_recid WHERE SUBSTRING(InH_InvoiceNo,3,10) < 10000000 {(!string.IsNullOrEmpty(DateFrom) ? $" AND inh_date >= '{Convert.ToDateTime(DateFrom):MM/dd/yyyy}'" : "")} {(!string.IsNullOrEmpty(DateTo) ? $" AND inh_date <=  '{Convert.ToDateTime(DateTo):MM/dd/yyyy}'" : "")} GROUP BY ind_stockrecid) WSales ON stk_stkid = ind_stockrecid 
                            WHERE stk_loc = '{ClsConfig.LocationCode}' {(ZeroQty == false ? "AND ISNULL(stk_avlbqty1,0) <> 0 " : "")}
                            {AndCondition}
                            ORDER BY stk_category 
                            ";

            using (DataTable dtData = new DataTable())
            {
                using (SqlDataAdapter daSQL = new SqlDataAdapter(strQuery, ClsConfig.SQLConnectionString))
                {
                    daSQL.Fill(dtData);
                    return dtData;
                }
            }
        }

        public string GetStockItemsQuery(ClsCom.QueryType QueryType = ClsCom.QueryType.Normal, string AndCondition = "", bool ActiveOnly = false)
        {
            string strQuery = "";

            if (QueryType == ClsCom.QueryType.Normal)
            {
                strQuery = $@"select Stk_StkId ,
                                     Stk_HoStkId ,
                                     Stk_Supplier ,
                                     Stk_Category ,
                                     Stk_Serial ,
                                     Stk_CrDate ,
                                     Stk_Status ,
                                     Stk_Consignment ,
                                     Stk_VATStatus ,
                                     Stk_ModSal ,
                                     Stk_DescPos ,
                                     Stk_Desc ,
                                     Stk_Barcode1 ,
                                     Stk_Barcode2 ,
                                     Stk_Packing ,
                                     Stk_UnitCode ,
                                     Stk_Transp ,
                                     Stk_ItmService ,
                                     Stk_Assembly ,
                                     Stk_3rdParty ,
                                     ISNULL(Stk_PLUnbr,0) Stk_PLUnbr ,
                                     Stk_GoodsLoc ,
                                     Stk_Gift ,
                                     Stk_StopOrder ,
                                     Stk_MinQty ,
                                     Stk_MaxQty ,
                                     Stk_BBFDate1,
                                     Stk_BBFQty1 ,
                                     Stk_LDateCount,
                                     ISNULL(Stk_AvlbQty1,0) Stk_AvlbQty1,
                                     Stk_Return1 ,
                                     Stk_BBFCostLC ,
                                     Stk_BBFCostFC ,
                                     Stk_ActCostLL ,
                                     Stk_ActCostFC ,
                                     Stk_LstCostLL ,
                                     Stk_LstCostFC ,
                                     Stk_FCcurr ,
                                     stk_PotcostLL ,
                                     stk_PotCostFC ,
                                     stk_potchangeDate,
                                     Stk_1SlPriceLC ,
                                     [Stk_1SlPrice$] ,
                                     Stk_Qty1stPrice ,
                                     Stk_2SLPriceLC ,
                                     [Stk_2SLPrice$] ,
                                     Stk_Qty2stPrice ,
                                     Stk_3SLPriceLC ,
                                     [Stk_3SLPrice$] ,
                                     Stk_SLCoef ,
                                     Stk_LStatCoef ,
                                     Stk_UserCoef ,
                                     Stk_DiscWh ,
                                     Stk_YTDQtyIn ,
                                     Stk_YTDQtyOut ,
                                     Stk_YTDAmtInLL ,
                                     [Stk_YTDAmtIn$] ,
                                     Stk_YTDAmtOutLL ,
                                     [Stk_YTDAmtOut$] ,
                                     Stk_SubjectVat ,
                                     Stk_VatType ,
                                     ISNULL(Stk_VatPer,0) Stk_VatPer,
                                     Stk_AddOnRef ,
                                     Stk_HoExport,
                                     Stk_HoImport,
                                     Stk_StaticUstmp ,
                                     Stk_StaticDstmp,
                                     Stk_Ustmp ,
                                     Stk_Dstmp,
                                     Stk_ValidityDays,
                                     Stk_Expiry,
                                     Stk_DiscRetail,
                                     Stk_Brand,
                                     Stk_VatCode,
                                     Stk_BBFCostCC,
                                     Stk_ActCostCC,
                                     Stk_LstCostCC,
                                     stk_PotCostCC
                              FROM tbl_StockItm
                            WHERE  Stk_Loc = @Stk_Loc  
                               {(ActiveOnly ? "AND isNull(Stk_Status, 'I') = 'A'" : "") }
                               {AndCondition} ";
            }
            else if (QueryType == ClsCom.QueryType.Readable)
            {
                strQuery = $@" SELECT  Stk_StkId [Code],
                                       Stk_HoStkId [HO Code],
                                       Stk_Supplier [Supplier],
                                       Stk_Category [Category],
                                       Stk_Serial [Serial],
                                       Stk_CrDate [Creation Date],
                                       Stk_Consignment [Consignment],
                                       Stk_ModSal [Mod Sal],
                                       Stk_DescPos [Pos Description],
                                       Stk_Desc [Stock Description],
                                       Stk_Barcode1 [Barcode 1],
                                       Stk_OrBarcode1 [Or Barcode 1],
                                       Stk_Barcode2 [Barcode 2],
                                       Stk_OrBarcode2 [Or Barcode 2],
                                       Stk_Packing [Packing],
                                       Stk_UnitCode [Unit Code],
                                       Stk_Transp [Transp],
                                       Stk_ItmService [Item Service],
                                       Stk_Assembly [Assembly],
                                       Stk_3rdParty [Third Party],
                                       Stk_PLUnbr [PLU Number],
                                       Stk_GoodsLoc [Goods Location],
                                       Stk_Gift [Gift],
                                       Stk_StopOrder [Stop Order],
                                       Stk_MinQty [Minimum Quantity],
                                       Stk_MaxQty [Maximum Quantity],
                                       stk_MinQtyWH2 [Minimum Quantity Warehouse 2],
                                       stk_MaxQtyWH2 [Maximum Quantity Warehouse 2],
                                       Stk_BBFDate1 [BBF Date 1],
                                       Stk_BBFDate2 [BBF Date 2],
                                       Stk_BBFQty1 [BBF Quantity 1],
                                       Stk_BBFQty2 [BBF Quantity 2],
                                       Stk_LDateCount [LDate Count],
                                       Stk_AvlbQty1 [Available Quantity 1],
                                       Stk_AvlbQty2 [Available Quantity 2],
                                       Stk_Return1 [Return 1],
                                       Stk_Return2 [Return 2],
                                       Stk_BBFCostLC [BBF Cost LC],
                                       Stk_BBFCostFC [BBF Cost FC],
                                       Stk_ActCostLL [Act Cost LL],
                                       Stk_ActCostFC [Act Cost FC],
                                       Stk_LstCostLL [Last Cost LL],
                                       Stk_LstCostFC [Last Cost FC],
                                       Stk_FCcurr [FC Currency],
                                       stk_PotcostLL [Pot Cost LL],
                                       stk_PotCostFC [Pot Cost FC],
                                       stk_potchangeDate [Pot Change Date],
                                       Stk_1SlPriceLC [1Sl Price LC],
                                       [Stk_1SlPrice$] [1Sl Price $],
                                       Stk_Qty1stPrice [Quantity 1st Price],
                                       Stk_2SLPriceLC [2 SL Price LC],
                                       [Stk_2SLPrice$] [2 SL Price $],
                                       Stk_Qty2stPrice [Quantity 2st Price],
                                       Stk_3SLPriceLC [3 SL Price LC],
                                       [Stk_3SLPrice$] [3 SL Price $],
                                       Stk_SLCoef [SL Coefficient],
                                       Stk_LStatCoef [L Stat Coefficient],
                                       Stk_UserCoef [User Coefficient],
                                       Stk_DiscWh [Discount Warehouse],
                                       Stk_YTDQtyIn [YTD Quantity In],
                                       Stk_YTDQtyOut [YTD Quantity Out],
                                       Stk_YTDAmtInLL [YTD Amount In LL],
                                       [Stk_YTDAmtIn$] [YTD Amount In $],
                                       Stk_YTDAmtOutLL [YTD Amount Out LL],
                                       [Stk_YTDAmtOut$] [YTD Amount Out $],
                                       Stk_SubjectVat [Subject VAT],
                                       Stk_VatType [VAT Type],
                                       Stk_VatPer [VAT Percentage],
                                       Stk_AddOnRef [Add On Ref],
                                       Stk_HoExport [HO Export],
                                       Stk_HoImport [HO Import],
                                       Stk_StaticUstmp [Static User Stamp],
                                       Stk_StaticDstmp [Static Date Stamp],
                                       Stk_Ustmp [User Stamp],
                                       Stk_Dstmp [Date Stamp],
                                       Stk_ValidityDays [Validity Date],
                                       Stk_Expiry [Expiry],
                                       Stk_DiscRetail [Disc Retail],
                                       Stk_Brand [Brand],
                                       Stk_VatCode [VAT Code],
                                       Stk_BBFCostCC [BBF Cost CC],
                                       Stk_ActCostCC [Act Cost CC],
                                       Stk_LstCostCC [Last Cost CC],
                                       stk_PotCostCC [Pot Cost CC],
                                       CONVERT(BIT, CASE WHEN ISNULL(Stk_VATStatus, 0) = '1' THEN 1 ELSE 0 END) [VAT Active],
                                       CONVERT(BIT, CASE WHEN ISNULL(Stk_Status, '') = 'A' THEN 1 ELSE 0 END) [Active]
	                                   FROM tbl_StockItm 
                                       WHERE  Stk_Loc = '{ClsConfig.LocationCode}'
                                       {(ActiveOnly ? "AND isNull(Stk_Status, '') = 'A'" : "") }
                                       {AndCondition}";
            }
            else if (QueryType == ClsCom.QueryType.SimpleReadable)
            {
                //strQuery = $@"SELECT  Stk_StkId [Code],
                //                     Stk_HoStkId [HO Code],
                //                     Stk_Supplier [Supplier],
                //                     Stk_Category [Category],
                //                     Stk_Serial [Serial],
                //                     Stk_CrDate  [Creation Date] ,
                //                     Case when ISNULL(Stk_Status, 'I') = 'A' then 'True' else 'False' end [Active] 
                //               FROM   tbl_StockItm                               
                //               WHERE  Stk_Loc = @Stk_Loc
                //               {(ActiveOnly ? "AND isNull(Stk_Status, 'I') = 'A'" : "") }
                //                      {AndCondition}  ";

                strQuery = $@"SELECT	'False' as [chk], 1 ,
                                        stk_serial as [Item Serial] , 
				                        [Stk_Desc] as [Description] , 
				                        [Stk_Category] as [Category], 
				                        stk_supplier as [Supplier] 
		                        FROM    [tbl_StockItm] 
		                        WHERE   [Stk_Status] = 'A' 
		                        AND     [Stk_Loc] = @Stk_Loc
                               {(ActiveOnly ? "AND isNull(Stk_Status, 'I') = 'A'" : "") }
                                      {AndCondition}  ";
            }
            else if(QueryType == ClsCom.QueryType.Report)
            {
                strQuery = $@" SELECT   Stk_Supplier ,
                                        Stk_Category ,
                                        Stk_Serial ,
                                        Stk_Barcode1 ,
                                        Stk_Desc ,
                                        Stk_AvlbQty1 ,
                                        Stk_AvlbQty2
                                FROM    tbl_StockItm
                                WHERE   1 = 1
                                       {AndCondition}";

            }

            return strQuery;
        }

        private void ExecuteSqlQuery(StockItems StockItems, String strQuery, SqlCommand sqlCommand)
        {
            if (sqlCommand == null)
            {
                using (SqlConnection cnSQL = new SqlConnection(ClsConfig.SQLConnectionString))
                {
                    cnSQL.Open();

                    using (SqlCommand cmSQL = new SqlCommand(strQuery, cnSQL))
                    {
                        FillCommandParameters(cmSQL, StockItems);
                        cmSQL.CommandTimeout = ClsConfig.SQLCommandTimeout;
                        cmSQL.ExecuteNonQuery();
                    }
                }
            }
            else
            {
                sqlCommand.CommandText = strQuery;
                FillCommandParameters(sqlCommand, StockItems);
                sqlCommand.CommandTimeout = ClsConfig.SQLCommandTimeout;
                sqlCommand.ExecuteNonQuery();
            }
        }

        private void FillCommandParameters(SqlCommand cmSQL, StockItems StockItems)
        {

            cmSQL.Parameters.AddWithValue("@Stk_Loc", ClsConfig.LocationCode);
            cmSQL.Parameters.AddWithValue("@Stk_StkId", StockItems.Stk_StkId);
            cmSQL.Parameters.AddWithValue("@Stk_HoStkId", StockItems.Stk_HoStkId ?? (object)DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_Supplier", StockItems.Stk_Supplier);
            cmSQL.Parameters.AddWithValue("@Stk_Category", StockItems.Stk_Category);
            cmSQL.Parameters.AddWithValue("@Stk_Serial", StockItems.Stk_Serial);
            cmSQL.Parameters.AddWithValue("@Stk_CrDate", StockItems.Stk_CrDate ?? (object)DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_Status", StockItems.Stk_Status);
            cmSQL.Parameters.AddWithValue("@Stk_Consignment", StockItems.Stk_Consignment);
            cmSQL.Parameters.AddWithValue("@Stk_VATStatus", StockItems.Stk_VATStatus);
            cmSQL.Parameters.AddWithValue("@Stk_ModSal", StockItems.Stk_ModSal);
            cmSQL.Parameters.AddWithValue("@Stk_DescPos", StockItems.Stk_DescPos ?? (object)DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_Desc", StockItems.Stk_Desc ?? (object)DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_Barcode1", StockItems.Stk_Barcode1 ?? (object)DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_Barcode2", StockItems.Stk_Barcode2 ?? (object)DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_Packing", StockItems.Stk_Packing ?? (object)DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_UnitCode", StockItems.Stk_UnitCode ?? (object)DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_Transp", StockItems.Stk_Transp);
            cmSQL.Parameters.AddWithValue("@Stk_ItmService", StockItems.Stk_ItmService);
            cmSQL.Parameters.AddWithValue("@Stk_Assembly", StockItems.Stk_Assembly);
            cmSQL.Parameters.AddWithValue("@Stk_3rdParty", StockItems.Stk_3rdParty);
            cmSQL.Parameters.AddWithValue("@Stk_PLUnbr", StockItems.Stk_PLUnbr.HasValue ? (object)StockItems.Stk_PLUnbr.Value : DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_GoodsLoc", StockItems.Stk_GoodsLoc ?? (object)DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_Gift", StockItems.Stk_Gift);
            cmSQL.Parameters.AddWithValue("@Stk_StopOrder", StockItems.Stk_StopOrder);
            cmSQL.Parameters.AddWithValue("@Stk_MinQty", StockItems.Stk_MinQty.HasValue ? (object)StockItems.Stk_MinQty.Value : DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_MaxQty", StockItems.Stk_MaxQty.HasValue ? (object)StockItems.Stk_MaxQty.Value : DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_BBFDate1", StockItems.Stk_CrDate ?? (object)DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_BBFQty1", StockItems.Stk_BBFQty1.HasValue ? (object)StockItems.Stk_BBFQty1.Value : DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_LDateCount", StockItems.Stk_LDateCount ?? (object)DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_AvlbQty1", StockItems.Stk_AvlbQty1.HasValue ? (object)StockItems.Stk_AvlbQty1.Value : DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_Return1", StockItems.Stk_Return1.HasValue ? (object)StockItems.Stk_Return1.Value : DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_BBFCostLC", StockItems.Stk_BBFCostLC.HasValue ? (object)StockItems.Stk_BBFCostLC.Value : DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_BBFCostFC", StockItems.Stk_BBFCostFC.HasValue ? (object)StockItems.Stk_BBFCostFC.Value : DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_ActCostLL", StockItems.Stk_ActCostLL.HasValue ? (object)StockItems.Stk_ActCostLL.Value : DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_ActCostFC", StockItems.Stk_ActCostFC.HasValue ? (object)StockItems.Stk_ActCostFC.Value : DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_LstCostLL", StockItems.Stk_LstCostLL.HasValue ? (object)StockItems.Stk_LstCostLL.Value : DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_LstCostFC", StockItems.Stk_LstCostFC.HasValue ? (object)StockItems.Stk_LstCostFC.Value : DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_FCcurr", StockItems.Stk_FCcurr ?? (object)DBNull.Value);
            cmSQL.Parameters.AddWithValue("@stk_PotcostLL", StockItems.stk_PotcostLL.HasValue ? (object)StockItems.stk_PotcostLL.Value : DBNull.Value);
            cmSQL.Parameters.AddWithValue("@stk_PotCostFC", StockItems.stk_PotCostFC.HasValue ? (object)StockItems.stk_PotCostFC.Value : DBNull.Value);
            cmSQL.Parameters.AddWithValue("@stk_potchangeDate", StockItems.stk_potchangeDate ?? (object)DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_1SlPriceLC", StockItems.Stk_1SlPriceLC.HasValue ? (object)StockItems.Stk_1SlPriceLC.Value : DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_1SlPrice$", StockItems.Stk_1SlPriceCC.HasValue ? (object)StockItems.Stk_1SlPriceCC.Value : DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_Qty1stPrice", StockItems.Stk_Qty1stPrice.HasValue ? (object)StockItems.Stk_Qty1stPrice.Value : DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_2SLPriceLC", StockItems.Stk_2SLPriceLC.HasValue ? (object)StockItems.Stk_2SLPriceLC.Value : DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_2SLPrice$", StockItems.Stk_2SLPriceCC.HasValue ? (object)StockItems.Stk_2SLPriceCC.Value : DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_Qty2stPrice", StockItems.Stk_Qty2stPrice.HasValue ? (object)StockItems.Stk_Qty2stPrice.Value : DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_3SLPriceLC", StockItems.Stk_3SLPriceLC.HasValue ? (object)StockItems.Stk_3SLPriceLC.Value : DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_3SLPrice$", StockItems.Stk_3SLPriceCC.HasValue ? (object)StockItems.Stk_3SLPriceCC.Value : DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_SLCoef", StockItems.Stk_SLCoef.HasValue ? (object)StockItems.Stk_SLCoef.Value : DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_LStatCoef", StockItems.Stk_LStatCoef ?? (object)DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_UserCoef", StockItems.Stk_UserCoef ?? (object)DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_DiscWh", StockItems.Stk_DiscWh.HasValue ? (object)StockItems.Stk_DiscWh.Value : DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_YTDQtyIn", StockItems.Stk_YTDQtyIn.HasValue ? (object)StockItems.Stk_YTDQtyIn.Value : DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_YTDQtyOut", StockItems.Stk_YTDQtyOut.HasValue ? (object)StockItems.Stk_YTDQtyOut.Value : DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_YTDAmtInLL", StockItems.Stk_YTDAmtInLL.HasValue ? (object)StockItems.Stk_YTDAmtInLL.Value : DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_YTDAmtIn$", StockItems.Stk_YTDAmtInUSD.HasValue ? (object)StockItems.Stk_YTDAmtInUSD.Value : DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_YTDAmtOutLL", StockItems.Stk_YTDAmtOutLL.HasValue ? (object)StockItems.Stk_YTDAmtOutLL.Value : DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_YTDAmtOut$", StockItems.Stk_YTDAmtOutUSD.HasValue ? (object)StockItems.Stk_YTDAmtOutUSD.Value : DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_SubjectVat", StockItems.Stk_SubjectVat ?? (object)DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_VatType", StockItems.Stk_VatType);
            cmSQL.Parameters.AddWithValue("@Stk_VatPer", StockItems.Stk_VatPer.HasValue ? (object)StockItems.Stk_VatPer.Value : DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_AddOnRef", StockItems.Stk_AddOnRef ?? (object)DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_StaticUstmp", ClsConfig.LoginUser);
            cmSQL.Parameters.AddWithValue("@Stk_Ustmp", ClsConfig.LoginUser);
            cmSQL.Parameters.AddWithValue("@Stk_ValidityDays", StockItems.Stk_ValidityDays != null ? StockItems.Stk_ValidityDays : 0); //Handle
            cmSQL.Parameters.AddWithValue("@Stk_Expiry", StockItems.Stk_Expiry);
            cmSQL.Parameters.AddWithValue("@Stk_DiscRetail", StockItems.Stk_DiscRetail.HasValue ? (object)StockItems.Stk_DiscRetail.Value : DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_Brand", StockItems.Stk_Brand ?? (object)DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_VatCode", StockItems.Stk_VatCode ?? (object)DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_BBFCostCC", StockItems.Stk_BBFCostCC.HasValue ? (object)StockItems.Stk_BBFCostCC.Value : DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_ActCostCC", StockItems.Stk_ActCostCC.HasValue ? (object)StockItems.Stk_ActCostCC.Value : DBNull.Value);
            cmSQL.Parameters.AddWithValue("@Stk_LstCostCC", StockItems.Stk_LstCostCC.HasValue ? (object)StockItems.Stk_LstCostCC.Value : DBNull.Value);
            cmSQL.Parameters.AddWithValue("@stk_PotCostCC", StockItems.stk_PotCostCC.HasValue ? (object)StockItems.stk_PotCostCC.Value : DBNull.Value);

        }

        private StockItems GetAndFillStockItemsObject(SqlDataReader drSQL)
        {
            StockItems StockItem = new StockItems();
            //return new StockItems
            {
                StockItem.Stk_StkId = Convert.ToInt32(drSQL["Stk_StkId"]);
                StockItem.Stk_HoStkId = drSQL["Stk_HoStkId"] != DBNull.Value ? Convert.ToInt32(drSQL["Stk_HoStkId"]) : (int?)null; 
                StockItem.Stk_Supplier = drSQL["Stk_Supplier"].ToString();
                StockItem.Stk_Category = drSQL["Stk_Category"].ToString();
                StockItem.Stk_Serial = drSQL["Stk_Serial"].ToString();
                StockItem.Stk_CrDate = drSQL["Stk_CrDate"] != DBNull.Value ? Convert.ToDateTime(drSQL["Stk_CrDate"]) : (DateTime?)null;
                StockItem.Stk_Status = drSQL["Stk_Status"].ToString();
                StockItem.Stk_Consignment = drSQL["Stk_Consignment"].ToString();
                StockItem.Stk_VATStatus = drSQL["Stk_VATStatus"].ToString();
                StockItem.Stk_ModSal = drSQL["Stk_ModSal"].ToString();
                StockItem.Stk_DescPos = drSQL["Stk_DescPos"] != DBNull.Value ? drSQL["Stk_DescPos"].ToString() : null;
                StockItem.Stk_Desc = drSQL["Stk_Desc"] != DBNull.Value ? drSQL["Stk_Desc"].ToString() : null;
                StockItem.Stk_Barcode1 = drSQL["Stk_Barcode1"] != DBNull.Value ? drSQL["Stk_Barcode1"].ToString() : null;
                StockItem.Stk_Barcode2 = drSQL["Stk_Barcode2"] != DBNull.Value ? drSQL["Stk_Barcode2"].ToString() : null;
                StockItem.Stk_Packing = drSQL["Stk_Packing"] != DBNull.Value ? drSQL["Stk_Packing"].ToString() : null;
                StockItem.Stk_UnitCode = drSQL["Stk_UnitCode"] != DBNull.Value ? drSQL["Stk_UnitCode"].ToString() : null;
                StockItem.Stk_Transp = drSQL["Stk_Transp"].ToString();
                StockItem.Stk_ItmService = drSQL["Stk_ItmService"].ToString();
                StockItem.Stk_Assembly = drSQL["Stk_Assembly"].ToString();
                StockItem.Stk_3rdParty = drSQL["Stk_3rdParty"].ToString();
                StockItem.Stk_PLUnbr = drSQL["Stk_PLUnbr"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_PLUnbr"]) : (double?)null;
                StockItem.Stk_GoodsLoc = drSQL["Stk_GoodsLoc"] != DBNull.Value ? drSQL["Stk_GoodsLoc"].ToString() : null;
                StockItem.Stk_Gift = drSQL["Stk_Gift"].ToString();
                StockItem.Stk_StopOrder = drSQL["Stk_StopOrder"].ToString();
                StockItem.Stk_MinQty = drSQL["Stk_MinQty"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_MinQty"]) : (double?)null;
                StockItem.Stk_MaxQty = drSQL["Stk_MaxQty"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_MaxQty"]) : (double?)null;
                StockItem.Stk_BBFDate1 = drSQL["Stk_BBFDate1"] != DBNull.Value ? Convert.ToDateTime(drSQL["Stk_BBFDate1"]) : (DateTime?)null;
                StockItem.Stk_BBFQty1 = drSQL["Stk_BBFQty1"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_BBFQty1"]) : (double?)null;
                StockItem.Stk_LDateCount = drSQL["Stk_LDateCount"] != DBNull.Value ? Convert.ToDateTime(drSQL["Stk_LDateCount"]) : (DateTime?)null;
                StockItem.Stk_AvlbQty1 = drSQL["Stk_AvlbQty1"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_AvlbQty1"]) : (double?)null;
                StockItem.Stk_Return1 = drSQL["Stk_Return1"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_Return1"]) : (double?)null;
                StockItem.Stk_BBFCostLC = drSQL["Stk_BBFCostLC"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_BBFCostLC"]) : (double?)null;
                StockItem.Stk_BBFCostFC = drSQL["Stk_BBFCostFC"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_BBFCostFC"]) : (double?)null;
                StockItem.Stk_ActCostLL = drSQL["Stk_ActCostLL"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_ActCostLL"]) : (double?)null;
                StockItem.Stk_ActCostFC = drSQL["Stk_ActCostFC"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_ActCostFC"]) : (double?)null;
                StockItem.Stk_LstCostLL = drSQL["Stk_LstCostLL"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_LstCostLL"]) : (double?)null;
                StockItem.Stk_LstCostFC = drSQL["Stk_LstCostFC"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_LstCostFC"]) : (double?)null;
                StockItem.Stk_FCcurr = drSQL["Stk_FCcurr"] != DBNull.Value ? drSQL["Stk_FCcurr"].ToString() : null;
                StockItem.stk_PotcostLL = drSQL["stk_PotcostLL"] != DBNull.Value ? Convert.ToDouble(drSQL["stk_PotcostLL"]) : (double?)null;
                StockItem.stk_PotCostFC = drSQL["stk_PotCostFC"] != DBNull.Value ? Convert.ToDouble(drSQL["stk_PotCostFC"]) : (double?)null;
                StockItem.stk_potchangeDate = drSQL["stk_potchangeDate"] != DBNull.Value ? Convert.ToDateTime(drSQL["stk_potchangeDate"]) : (DateTime?)null;
                StockItem.Stk_1SlPriceLC = drSQL["Stk_1SlPriceLC"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_1SlPriceLC"]) : (double?)null;
                StockItem.Stk_1SlPriceCC = drSQL["Stk_1SlPrice$"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_1SlPrice$"]) : (double?)null;
                StockItem.Stk_Qty1stPrice = drSQL["Stk_Qty1stPrice"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_Qty1stPrice"]) : (double?)null;
                StockItem.Stk_2SLPriceLC = drSQL["Stk_2SLPriceLC"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_2SLPriceLC"]) : (double?)null;
                StockItem.Stk_2SLPriceCC = drSQL["Stk_2SLPrice$"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_2SLPrice$"]) : (double?)null;
                StockItem.Stk_Qty2stPrice = drSQL["Stk_Qty2stPrice"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_Qty2stPrice"]) : (double?)null;
                StockItem.Stk_3SLPriceLC = drSQL["Stk_3SLPriceLC"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_3SLPriceLC"]) : (double?)null;
                StockItem.Stk_3SLPriceCC = drSQL["Stk_3SLPrice$"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_3SLPrice$"]) : (double?)null;
                StockItem.Stk_SLCoef = drSQL["Stk_SLCoef"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_SLCoef"]) : (double?)null;
                StockItem.Stk_LStatCoef = drSQL["Stk_LStatCoef"] != DBNull.Value ? drSQL["Stk_LStatCoef"].ToString() : null;
                StockItem.Stk_UserCoef = drSQL["Stk_UserCoef"] != DBNull.Value ? drSQL["Stk_UserCoef"].ToString() : null;
                StockItem.Stk_DiscWh = drSQL["Stk_DiscWh"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_DiscWh"]) : (double?)null;
                StockItem.Stk_YTDQtyIn = drSQL["Stk_YTDQtyIn"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_YTDQtyIn"]) : (double?)null;
                StockItem.Stk_YTDQtyOut = drSQL["Stk_YTDQtyOut"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_YTDQtyOut"]) : (double?)null;
                StockItem.Stk_YTDAmtInLL = drSQL["Stk_YTDAmtInLL"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_YTDAmtInLL"]) : (double?)null;
                StockItem.Stk_YTDAmtInUSD = drSQL["Stk_YTDAmtIn$"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_YTDAmtIn$"]) : (double?)null;
                StockItem.Stk_YTDAmtOutLL = drSQL["Stk_YTDAmtOutLL"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_YTDAmtOutLL"]) : (double?)null;
                StockItem.Stk_YTDAmtOutUSD = drSQL["Stk_YTDAmtOut$"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_YTDAmtOut$"]) : (double?)null;
                StockItem.Stk_SubjectVat = drSQL["Stk_SubjectVat"] != DBNull.Value ? drSQL["Stk_SubjectVat"].ToString() : null;
                StockItem.Stk_VatType = drSQL["Stk_VatType"] != DBNull.Value ? drSQL["Stk_VatType"].ToString() : null;
                StockItem.Stk_VatPer = drSQL["Stk_VatPer"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_VatPer"]) : (double?)null;
                StockItem.Stk_AddOnRef = drSQL["Stk_AddOnRef"] != DBNull.Value ? drSQL["Stk_AddOnRef"].ToString() : null;
                StockItem.Stk_HoExport = drSQL["Stk_HoExport"] != DBNull.Value ? Convert.ToDateTime(drSQL["Stk_HoExport"]) : (DateTime?)null;
                StockItem.Stk_HoImport = drSQL["Stk_HoImport"] != DBNull.Value ? Convert.ToDateTime(drSQL["Stk_HoImport"]) : (DateTime?)null;
                StockItem.Stk_StaticUstmp = drSQL["Stk_StaticUstmp"] != DBNull.Value ? drSQL["Stk_StaticUstmp"].ToString() : null;
                StockItem.Stk_StaticDstmp = drSQL["Stk_StaticDstmp"] != DBNull.Value ? Convert.ToDateTime(drSQL["Stk_StaticDstmp"]) : (DateTime?)null;
                StockItem.Stk_Ustmp = drSQL["Stk_Ustmp"] != DBNull.Value ? drSQL["Stk_Ustmp"].ToString() : null;
                StockItem.Stk_Dstmp = drSQL["Stk_Dstmp"] != DBNull.Value ? Convert.ToDateTime(drSQL["Stk_Dstmp"]) : (DateTime?)null;
                StockItem.Stk_ValidityDays = drSQL["Stk_ValidityDays"] != DBNull.Value ? Convert.ToInt32(drSQL["Stk_ValidityDays"]) : 0;
                StockItem.Stk_Expiry = drSQL["Stk_Expiry"] != DBNull.Value ? drSQL["Stk_Expiry"].ToString() : null;
                StockItem.Stk_DiscRetail = drSQL["Stk_DiscRetail"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_DiscRetail"]) : (double?)null;
                StockItem.Stk_Brand = drSQL["Stk_Brand"] != DBNull.Value ? drSQL["Stk_Brand"].ToString() : null;
                StockItem.Stk_VatCode = drSQL["Stk_VatCode"] != DBNull.Value ? drSQL["Stk_VatCode"].ToString() : null;
                StockItem.Stk_BBFCostCC = drSQL["Stk_BBFCostCC"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_BBFCostCC"]) : (double?)null;
                StockItem.Stk_ActCostCC = drSQL["Stk_ActCostCC"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_ActCostCC"]) : (double?)null;
                StockItem.Stk_LstCostCC = drSQL["Stk_LstCostCC"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_LstCostCC"]) : (double?)null;
                StockItem.stk_PotCostCC = drSQL["stk_PotCostCC"] != DBNull.Value ? Convert.ToDouble(drSQL["stk_PotCostCC"]) : (double?)null;

                StockUnitController stockUnitController = new StockUnitController();
                StockItem.stk_StockUnitList = stockUnitController.GetStockUnitList(StockItem.Stk_StkId);

                StockQtyController QtyController = new StockQtyController();
                StockItem.Stk_StockQtyList = QtyController.GetStockQtyList(StockItem.Stk_StkId);

                StockQtyExpiryController QtyExpController = new StockQtyExpiryController();
                StockItem.Stk_StockQtyExpiryList = QtyExpController.GetStockQtyExpiry(StockItem.Stk_StkId);

                AddenSupplierController addenSupplierController = new AddenSupplierController();
                StockItem.Stk_AddenSupplierList = addenSupplierController.GetAddenSupplierList(StockItem.Stk_StkId);

                StockFilePriceController filePriceController = new StockFilePriceController();
                StockItem.stk_FilePrice = filePriceController.GetStockFilePrice(StockItem.Stk_StkId);

                OHStockAddController OHController = new OHStockAddController();
                StockItem.Stk_OHStockAdd = OHController.GetOHStockAdd(StockItem.Stk_StkId);

                LinkStockDeptController LinkDeptController = new LinkStockDeptController();
                StockItem.Stk_LinkStockDept = LinkDeptController.GetLinkStockDept(StockItem.Stk_StkId);
            };
            return StockItem;
        }

        //private StockItems FillStockItemsObject(SqlDataReader drSQL)
        //{
        //    StockItems StockItem = new StockItems();
        //    //return new StockItems
        //    {
        //        StockItem.Stk_StkId = Convert.ToInt32(drSQL["Stk_StkId"]);
        //        StockItem.Stk_HoStkId = Convert.ToInt32(drSQL["Stk_HoStkId"]);
        //        StockItem.Stk_Supplier = drSQL["Stk_Supplier"].ToString();
        //        StockItem.Stk_Category = drSQL["Stk_Category"].ToString();
        //        StockItem.Stk_Serial = drSQL["Stk_Serial"].ToString();
        //        StockItem.Stk_CrDate = drSQL["Stk_CrDate"] != DBNull.Value ? Convert.ToDateTime(drSQL["Stk_CrDate"]) : (DateTime?)null;
        //        StockItem.Stk_Status = drSQL["Stk_Status"].ToString();
        //        StockItem.Stk_Consignment = drSQL["Stk_Consignment"].ToString();
        //        StockItem.Stk_VATStatus = drSQL["Stk_VATStatus"].ToString();
        //        StockItem.Stk_ModSal = drSQL["Stk_VATStatus"].ToString();
        //        StockItem.Stk_DescPos = drSQL["Stk_DescPos"] != DBNull.Value ? drSQL["Stk_DescPos"].ToString() : null;
        //        StockItem.Stk_Desc = drSQL["Stk_Desc"] != DBNull.Value ? drSQL["Stk_Desc"].ToString() : null;
        //        StockItem.Stk_Barcode1 = drSQL["Stk_Barcode1"] != DBNull.Value ? drSQL["Stk_Barcode1"].ToString() : null;
        //        StockItem.Stk_Barcode2 = drSQL["Stk_Barcode2"] != DBNull.Value ? drSQL["Stk_Barcode2"].ToString() : null;
        //        StockItem.Stk_Packing = drSQL["Stk_Packing"] != DBNull.Value ? drSQL["Stk_Packing"].ToString() : null;
        //        StockItem.Stk_UnitCode = drSQL["Stk_UnitCode"] != DBNull.Value ? drSQL["Stk_UnitCode"].ToString() : null;
        //        StockItem.Stk_Transp = drSQL["Stk_Transp"].ToString();
        //        StockItem.Stk_ItmService = drSQL["Stk_ItmService"].ToString();
        //        StockItem.Stk_Assembly = drSQL["Stk_Assembly"].ToString();
        //        StockItem.Stk_3rdParty = drSQL["Stk_3rdParty"].ToString();
        //        StockItem.Stk_PLUnbr = drSQL["Stk_PLUnbr"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_PLUnbr"]) : (double?)null;
        //        StockItem.Stk_GoodsLoc = drSQL["Stk_GoodsLoc"] != DBNull.Value ? drSQL["Stk_GoodsLoc"].ToString() : null;
        //        StockItem.Stk_Gift = drSQL["Stk_Gift"].ToString();
        //        StockItem.Stk_StopOrder = drSQL["Stk_StopOrder"].ToString();
        //        StockItem.Stk_MinQty = drSQL["Stk_MinQty"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_MinQty"]) : (double?)null;
        //        StockItem.Stk_MaxQty = drSQL["Stk_MaxQty"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_MaxQty"]) : (double?)null;
        //        StockItem.Stk_BBFDate1 = drSQL["Stk_BBFDate1"] != DBNull.Value ? Convert.ToDateTime(drSQL["Stk_BBFDate1"]) : (DateTime?)null;
        //        StockItem.Stk_BBFQty1 = drSQL["Stk_BBFQty1"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_BBFQty1"]) : (double?)null;
        //        StockItem.Stk_LDateCount = drSQL["Stk_LDateCount"] != DBNull.Value ? Convert.ToDateTime(drSQL["Stk_LDateCount"]) : (DateTime?)null;
        //        StockItem.Stk_AvlbQty1 = drSQL["Stk_AvlbQty1"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_AvlbQty1"]) : (double?)null;
        //        StockItem.Stk_Return1 = drSQL["Stk_Return1"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_Return1"]) : (double?)null;
        //        StockItem.Stk_BBFCostLC = drSQL["Stk_BBFCostLC"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_BBFCostLC"]) : (double?)null;
        //        StockItem.Stk_BBFCostFC = drSQL["Stk_BBFCostFC"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_BBFCostFC"]) : (double?)null;
        //        StockItem.Stk_ActCostLL = drSQL["Stk_ActCostLL"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_ActCostLL"]) : (double?)null;
        //        StockItem.Stk_ActCostFC = drSQL["Stk_ActCostFC"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_ActCostFC"]) : (double?)null;
        //        StockItem.Stk_LstCostLL = drSQL["Stk_LstCostLL"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_LstCostLL"]) : (double?)null;
        //        StockItem.Stk_LstCostFC = drSQL["Stk_LstCostFC"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_LstCostFC"]) : (double?)null;
        //        StockItem.Stk_FCcurr = drSQL["Stk_FCcurr"] != DBNull.Value ? drSQL["Stk_FCcurr"].ToString() : null;
        //        StockItem.stk_PotcostLL = drSQL["stk_PotcostLL"] != DBNull.Value ? Convert.ToDouble(drSQL["stk_PotcostLL"]) : (double?)null;
        //        StockItem.stk_PotCostFC = drSQL["stk_PotCostFC"] != DBNull.Value ? Convert.ToDouble(drSQL["stk_PotCostFC"]) : (double?)null;
        //        StockItem.stk_potchangeDate = drSQL["stk_potchangeDate"] != DBNull.Value ? Convert.ToDateTime(drSQL["stk_potchangeDate"]) : (DateTime?)null;
        //        StockItem.Stk_1SlPriceLC = drSQL["Stk_1SlPriceLC"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_1SlPriceLC"]) : (double?)null;
        //        StockItem.Stk_1SlPriceCC = drSQL["Stk_1SlPrice$"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_1SlPrice$"]) : (double?)null;
        //        StockItem.Stk_Qty1stPrice = drSQL["Stk_Qty1stPrice"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_Qty1stPrice"]) : (double?)null;
        //        StockItem.Stk_2SLPriceLC = drSQL["Stk_2SLPriceLC"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_2SLPriceLC"]) : (double?)null;
        //        StockItem.Stk_2SLPriceCC = drSQL["Stk_2SLPrice$"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_2SLPrice$"]) : (double?)null;
        //        StockItem.Stk_Qty2stPrice = drSQL["Stk_Qty2stPrice"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_Qty2stPrice"]) : (double?)null;
        //        StockItem.Stk_3SLPriceLC = drSQL["Stk_3SLPriceLC"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_3SLPriceLC"]) : (double?)null;
        //        StockItem.Stk_3SLPriceCC = drSQL["Stk_3SLPrice$"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_3SLPrice$"]) : (double?)null;
        //        StockItem.Stk_SLCoef = drSQL["Stk_SLCoef"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_SLCoef"]) : (double?)null;
        //        StockItem.Stk_LStatCoef = drSQL["Stk_LStatCoef"] != DBNull.Value ? drSQL["Stk_LStatCoef"].ToString() : null;
        //        StockItem.Stk_UserCoef = drSQL["Stk_UserCoef"] != DBNull.Value ? drSQL["Stk_UserCoef"].ToString() : null;
        //        StockItem.Stk_DiscWh = drSQL["Stk_DiscWh"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_DiscWh"]) : (double?)null;
        //        StockItem.Stk_YTDQtyIn = drSQL["Stk_YTDQtyIn"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_YTDQtyIn"]) : (double?)null;
        //        StockItem.Stk_YTDQtyOut = drSQL["Stk_YTDQtyOut"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_YTDQtyOut"]) : (double?)null;
        //        StockItem.Stk_YTDAmtInLL = drSQL["Stk_YTDAmtInLL"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_YTDAmtInLL"]) : (double?)null;
        //        StockItem.Stk_YTDAmtInUSD = drSQL["Stk_YTDAmtIn$"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_YTDAmtIn$"]) : (double?)null;
        //        StockItem.Stk_YTDAmtOutLL = drSQL["Stk_YTDAmtOutLL"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_YTDAmtOutLL"]) : (double?)null;
        //        StockItem.Stk_YTDAmtOutUSD = drSQL["Stk_YTDAmtOut$"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_YTDAmtOut$"]) : (double?)null;
        //        StockItem.Stk_SubjectVat = drSQL["Stk_SubjectVat"] != DBNull.Value ? drSQL["Stk_SubjectVat"].ToString() : null;
        //        StockItem.Stk_VatType = drSQL["Stk_VatType"] != DBNull.Value ? drSQL["Stk_VatType"].ToString() : null;
        //        StockItem.Stk_VatPer = drSQL["Stk_VatPer"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_VatPer"]) : (double?)null;
        //        StockItem.Stk_AddOnRef = drSQL["Stk_AddOnRef"] != DBNull.Value ? drSQL["Stk_AddOnRef"].ToString() : null;
        //        StockItem.Stk_HoExport = drSQL["Stk_HoExport"] != DBNull.Value ? Convert.ToDateTime(drSQL["Stk_HoExport"]) : (DateTime?)null;
        //        StockItem.Stk_HoImport = drSQL["Stk_HoImport"] != DBNull.Value ? Convert.ToDateTime(drSQL["Stk_HoImport"]) : (DateTime?)null;
        //        StockItem.Stk_StaticUstmp = drSQL["Stk_StaticUstmp"] != DBNull.Value ? drSQL["Stk_StaticUstmp"].ToString() : null;
        //        StockItem.Stk_StaticDstmp = drSQL["Stk_StaticDstmp"] != DBNull.Value ? Convert.ToDateTime(drSQL["Stk_StaticDstmp"]) : (DateTime?)null;
        //        StockItem.Stk_Ustmp = drSQL["Stk_Ustmp"] != DBNull.Value ? drSQL["Stk_Ustmp"].ToString() : null;
        //        StockItem.Stk_Dstmp = drSQL["Stk_Dstmp"] != DBNull.Value ? Convert.ToDateTime(drSQL["Stk_Dstmp"]) : (DateTime?)null;
        //        StockItem.Stk_ValidityDays = drSQL["Stk_ValidityDays"] != DBNull.Value ? Convert.ToInt32(drSQL["Stk_ValidityDays"]) : 0;
        //        StockItem.Stk_Expiry = drSQL["Stk_Expiry"] != DBNull.Value ? drSQL["Stk_Expiry"].ToString() : null;
        //        StockItem.Stk_DiscRetail = drSQL["Stk_DiscRetail"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_DiscRetail"]) : (double?)null;
        //        StockItem.Stk_Brand = drSQL["Stk_Brand"] != DBNull.Value ? drSQL["Stk_Brand"].ToString() : null;
        //        StockItem.Stk_VatCode = drSQL["Stk_VatCode"] != DBNull.Value ? drSQL["Stk_VatCode"].ToString() : null;
        //        StockItem.Stk_BBFCostCC = drSQL["Stk_BBFCostCC"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_BBFCostCC"]) : (double?)null;
        //        StockItem.Stk_ActCostCC = drSQL["Stk_ActCostCC"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_ActCostCC"]) : (double?)null;
        //        StockItem.Stk_LstCostCC = drSQL["Stk_LstCostCC"] != DBNull.Value ? Convert.ToDouble(drSQL["Stk_LstCostCC"]) : (double?)null;
        //        StockItem.stk_PotCostCC = drSQL["stk_PotCostCC"] != DBNull.Value ? Convert.ToDouble(drSQL["stk_PotCostCC"]) : (double?)null;

        //    };
        //    return StockItem;
        //}

        public static int GetLast()
        {
            string strsql = "SELECT ISNULL(MAX(Stk_StkId), 0) + 1 FROM tbl_StockItm WHERE Stk_Loc = @Stk_Loc";
            int lastId = 0;

            using (SqlConnection cnSQL = new SqlConnection(ClsConfig.SQLConnectionString))
            {

                cnSQL.Open();

                using (SqlCommand cmSQL = new SqlCommand(strsql, cnSQL))
                {
                    cmSQL.Parameters.AddWithValue("@Stk_Loc", ClsConfig.LocationCode);

                    using (SqlDataReader drSQL = cmSQL.ExecuteReader())
                    {
                        if (drSQL.Read())
                        {
                            lastId = Convert.ToInt32(drSQL[0]);
                        }
                    }

                }
            }

            return lastId;
        }

        public static long GetMaxSerial(string Supplier = null, string Category = null)
        {
            string strsql = $@" SELECT  ISNULL(MAX(Cast(stk_serial As Numeric)),0)+1 
                                  FROM  tbl_stockitm
                                 WHERE  stk_loc= @stk_loc 
                                    AND ISNUMERIC(stk_serial)=1
                                   {(!string.IsNullOrEmpty(Supplier) ? "AND stk_supplier = @stk_supplier" : "")}
                                   {(!string.IsNullOrEmpty(Category) ? "AND stk_category = @stk_category" : "")}";
            long lastId = 0;

            using (SqlConnection cnSQL = new SqlConnection(ClsConfig.SQLConnectionString))
            {

                cnSQL.Open();

                using (SqlCommand cmSQL = new SqlCommand(strsql, cnSQL))
                {
                    cmSQL.Parameters.AddWithValue("@Stk_Loc", ClsConfig.LocationCode);
                    if (!string.IsNullOrEmpty(Supplier)) cmSQL.Parameters.AddWithValue("@stk_supplier", Supplier);
                    if (!string.IsNullOrEmpty(Category)) cmSQL.Parameters.AddWithValue("@stk_category", Category);

                    using (SqlDataReader drSQL = cmSQL.ExecuteReader())
                    {
                        if (drSQL.Read())
                        {
                            lastId = Convert.ToInt64(drSQL[0]);
                        }
                    }

                }
            }

            return lastId;
        }

        public static bool GetAddOnRef(string AddOnRef , string AndCondition = "")
        {
            string strsql = $@" SELECT  top 1 1 
                                FROM    dbo.tbl_stockitm
                                WHERE   Stk_AddOnRef = '{AddOnRef}'
                                        AND stk_loc = @stk_loc 
                                        {(AndCondition)}";

            using (SqlConnection cnSQL = new SqlConnection(ClsConfig.SQLConnectionString))
            {
                cnSQL.Open();

                using (SqlCommand cmSQL = new SqlCommand(strsql, cnSQL))
                {
                    cmSQL.Parameters.AddWithValue("@stk_loc", ClsConfig.LocationCode);
                    return (cmSQL.ExecuteScalar() != null);
                }

            }
        }

        public static bool GetPLU(string PluNo, string AndCondition = "")
        {
            string strsql = $@" SELECT  top 1 1 
                                FROM    dbo.tbl_stockitm
                                WHERE   stk_plunbr = {decimal.Parse(PluNo.Replace(",", ""))}
                                        AND stk_loc = @stk_loc 
                                        {(AndCondition)}";

            using (SqlConnection cnSQL = new SqlConnection(ClsConfig.SQLConnectionString))
            {
                cnSQL.Open();

                using (SqlCommand cmSQL = new SqlCommand(strsql, cnSQL))
                {
                    cmSQL.Parameters.AddWithValue("@stk_loc", ClsConfig.LocationCode);
                    return (cmSQL.ExecuteScalar() != null);
                }

            }
        }

        public static long GeneratePLU()
        {
            string strsql = $@" SELECT  ISNULL(MAX(stk_plunbr),0) + 1 
                                  FROM  tbl_stockitm
                                 WHERE  stk_loc= @stk_loc ";
            long PluNum = 0;

            using (SqlConnection cnSQL = new SqlConnection(ClsConfig.SQLConnectionString))
            {

                cnSQL.Open();

                using (SqlCommand cmSQL = new SqlCommand(strsql, cnSQL))
                {
                    cmSQL.Parameters.AddWithValue("@Stk_Loc", ClsConfig.LocationCode);

                    using (SqlDataReader drSQL = cmSQL.ExecuteReader())
                    {
                        if (drSQL.Read())
                        {
                            PluNum = Convert.ToInt64(drSQL[0]);
                        }
                    }

                }
            }

            return PluNum;
        }

        public static bool IsMovemented(int StockID)
        {

            string strQuery = $@"SELECT TOP 1 ind_stockrecid
                                    FROM tbl_invoicedetailwhole
                                    INNER JOIN tbl_invoiceheaderwhole ON inh_recid = ind_hdrrecid
                                    WHERE ind_stockrecid = @StockID
                                     AND inh_loccode = @LocationCode
                                    UNION
                                    SELECT TOP 1 sms_stkid
                                    FROM tbl_salesmvtsummary
                                    WHERE sms_stkid = @StockID
                                            AND sms_loc = @LocationCode
                                    UNION
                                    SELECT TOP 1 trd_stkid
                                    FROM tbl_transferdetail
                                    INNER JOIN tbl_transferheader ON trh_recid = trd_transrecid
                                    WHERE trd_stkid =@StockID 
                                          AND trh_loccode = @LocationCode
                                    UNION
                                    SELECT TOP 1 pid_stkid
                                    FROM tbl_purinvdetails
                                    WHERE pid_stkid = @StockID
                                            AND pid_loc = @LocationCode
                                    UNION
                                    SELECT TOP 1 tad_StkId
                                    FROM tbl_TranspositionAssemblyDetails
                                    WHERE tad_StkId = @StockID
                                            AND tad_Loc = @LocationCode
                                    UNION
                                    SELECT TOP 1 sdd_stkid
                                    FROM tbl_stkdispdetails
                                    WHERE sdd_stkid = @StockID
                                            AND sdd_loc = @LocationCode
                                    UNION
                                    SELECT TOP 1 add_stkid
                                    FROM tbl_adjustdetail
                                    WHERE add_stkid = @StockID
                                            AND add_loc =@LocationCode
                                    UNION
                                    SELECT TOP 1 pod_stkid
                                    FROM tbl_purordersdetails
                                    WHERE pod_stkid = @StockID
                                            AND pod_loc = @LocationCode
                                    UNION
                                    SELECT TOP 1 ind_stockrecid
                                    FROM tbl_profinvdetwhole
                                    INNER JOIN tbl_profinvheaderwhole ON inh_recid = ind_hdrrecid
                                    WHERE inh_loccode = @LocationCode AND ind_stockrecid = @StockID";

            using (SqlConnection cnSQL = new SqlConnection(ClsConfig.SQLConnectionString))
            {
                cnSQL.Open();

                using (SqlCommand cmSQL = new SqlCommand(strQuery, cnSQL))
                {

                    cmSQL.Parameters.AddWithValue("@LocationCode", ClsConfig.LocationCode);
                    cmSQL.Parameters.AddWithValue("@StockID", StockID);

                    return (cmSQL.ExecuteScalar() != null);
                }

            }
        }

        public string GetBaseUnit()
        {
            string strsql = $@" SELECT  Unit_Code
                                FROM    dbo.tbl_Units
                                WHERE   Unit_Default = 1
                                        AND Unit_Active = 'Y' ";

            using (SqlConnection cnSQL = new SqlConnection(ClsConfig.SQLConnectionString))
            {
                cnSQL.Open();

                using (SqlCommand cmSQL = new SqlCommand(strsql, cnSQL))
                {
                    return cmSQL.ExecuteScalar().ToString();
                }

            }

        }

        public DataTable GetBarcodes(int StockItemID, string Unit = null)
        {
            string strQuery = $@"DECLARE @StkID NUMERIC(18, 0) = {StockItemID};
                         DECLARE @Unit VARCHAR(3) = {(string.IsNullOrWhiteSpace(Unit) ? "NULL" : $"'{Unit.Replace("'", "''")}'")};

                         SELECT
                             BarcodeType,
                             Barcode,
                             UnitCode
                         FROM
                         (
                             SELECT
                                 'Barcode 1' AS BarcodeType,
                                 SI.Stk_Barcode1 AS Barcode,
                                 SI.Stk_UnitCode AS UnitCode,
                                 1 AS [ORDER]
                             FROM tbl_StockItm SI
                             WHERE SI.stk_Loc = '{ClsConfig.LocationCode}'
                               AND SI.Stk_StkId = @StkID
                               AND ISNULL(SI.Stk_Barcode1, '') <> ''

                             UNION ALL

                             SELECT
                                 'Barcode 2' AS BarcodeType,
                                 SI.Stk_Barcode2 AS Barcode,
                                 SI.Stk_UnitCode AS UnitCode,
                                 2 AS [ORDER]
                             FROM tbl_StockItm SI
                             WHERE SI.stk_Loc = '{ClsConfig.LocationCode}'
                               AND SI.Stk_StkId = @StkID
                               AND ISNULL(SI.Stk_Barcode2, '') <> ''

                             UNION ALL

                             SELECT
                                 'Barcode Synonym ' +
                                     CAST(
                                         ROW_NUMBER() OVER (
                                             ORDER BY BS.Bsy_Barcode
                                         ) AS VARCHAR(10)
                                     ) AS BarcodeType,
                                 BS.Bsy_Barcode AS Barcode,
                                 BS.BSy_UnitCode AS UnitCode,
                                 2 + ROW_NUMBER() OVER (
                                         ORDER BY BS.Bsy_Barcode
                                     ) AS [ORDER]
                             FROM tbl_BarcodeSyn BS
                             INNER JOIN tbl_StockItm SI ON SI.Stk_StkId = BS.Bsy_Stkid AND SI.stk_Loc = '{ClsConfig.LocationCode}'
                             WHERE BS.Bsy_Stkid = @StkID
                               AND BS.BSy_UnitCode = ISNULL(@Unit, SI.Stk_UnitCode)
                         ) AS B
                         ORDER BY [ORDER]; ";

            using (DataTable dtData = new DataTable())
            {
                using (SqlDataAdapter daSQL = new SqlDataAdapter(strQuery, ClsConfig.SQLConnectionString))
                {
                    daSQL.Fill(dtData);
                    return dtData;
                }
            }
        }

        public StockItems GetItemInfo(string ItemReference, bool ActiveOnly = false, string Unit = null)
        {
            using (SqlConnection cnSQL = new SqlConnection(ClsConfig.SQLConnectionString))
            {
                string strQuery = $@" SELECT    Stk_StkId ItemID,
                                    CAST(Stk_Supplier AS VARCHAR(4)) + '/'
                                    + CAST(Stk_Category AS VARCHAR(3)) + '/'
                                    + CAST(Stk_Serial AS NVARCHAR(6)) ItemReference,
                                    {(string.IsNullOrEmpty(Unit) ? $@"ISNULL(BSy_UnitCode, Stk_UnitCode)" : "@Unit")} UnitCode,
                                    ISNULL(sunit_SpecDesc, Stk_Desc) Description

                                    FROM    tbl_StockItm

                                    LEFT JOIN ( SELECT  BSy_Loc, Bsy_Stkid, BSy_UnitCode, Bsy_Barcode
                                                FROM    tbl_BarcodeSyn
                                                WHERE   Bsy_Barcode = @ItemReference

                                              ) tbl_BarcodeSyn ON tbl_BarcodeSyn.BSy_Loc = tbl_StockItm.Stk_Loc
                                                                  AND tbl_BarcodeSyn.Bsy_Stkid = tbl_StockItm.Stk_StkId
                                    LEFT JOIN tbl_StockUnit ON SUnit_Loc = Stk_Loc
                                                  AND SUnit_StkId = Stk_StkId
                                                  AND SUnit_Unit = {(string.IsNullOrEmpty(Unit) ? $@" ISNULL(BSy_UnitCode,stk_unitcode)" : "@Unit")}
                            WHERE   Stk_Loc = @Stk_Loc
                                    {(ActiveOnly ? "AND ISNULL(Stk_Status, 'I') = 'A'" : "")}
                                    AND (
                                         CAST(Stk_Supplier AS VARCHAR(4)) + '/' + CAST(Stk_Category AS VARCHAR(3)) + '/' + CAST(Stk_Serial AS NVARCHAR(6)) = @ItemReference
                                         OR Stk_Barcode1 = @ItemReference
                                         OR Stk_Barcode2 = @ItemReference
                                         OR CAST(Stk_PLUnbr AS VARCHAR) = @ItemReference
                                         OR Bsy_Barcode = @ItemReference
                                         OR Stk_StkId = TRY_CAST(@ItemReference AS NUMERIC(18,0))
                                        )
                                    ORDER BY
                                        CASE
                                            WHEN CAST(Stk_Supplier AS VARCHAR(4)) + '/' + CAST(Stk_Category AS VARCHAR(3)) + '/' + CAST(Stk_Serial AS NVARCHAR(6)) = @ItemReference THEN 1
                                            WHEN CAST(Stk_PLUnbr AS VARCHAR) = @ItemReference THEN 2
                                            WHEN Stk_Barcode1 = @ItemReference THEN 3
                                            WHEN Stk_Barcode2 = @ItemReference THEN 4
                                            WHEN Bsy_Barcode = @ItemReference THEN 5
                                            ELSE 6
                                        END ";
                cnSQL.Open();
                using (SqlCommand cmSQL = new SqlCommand(strQuery, cnSQL))
                {
                    cmSQL.Parameters.AddWithValue("@Stk_Loc", ClsConfig.LocationCode);
                    cmSQL.Parameters.AddWithValue("@ItemReference", ItemReference);
                    if (!string.IsNullOrEmpty(Unit))
                    {
                        cmSQL.Parameters.AddWithValue("@Unit", Unit);
                    }
                    using (SqlDataReader drSQL = cmSQL.ExecuteReader())
                    {
                        if (drSQL.Read())
                        {
                            StockItems StockItem = new StockItems();
                            StockItem.Stk_StkId = Convert.ToInt32(drSQL["ItemID"]);
                            StockItem.Stk_UnitCode = drSQL["UnitCode"].ToString();
                            StockItem.ItemReference = drSQL["ItemReference"].ToString();
                            StockItem.ItemDescription = drSQL["Description"].ToString();
                            return StockItem;
                        }
                    }
                }
            }
            return null;
        }
    }
}
