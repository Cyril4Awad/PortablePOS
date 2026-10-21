using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperStoreBusiness
{
    public class ClsPricing
    {

        public static void CalculatePrice(string _BaseCurrency,// Based on the old gift flag
                                          string _PotentialCurrency,// Sfp_PotCurr
                                          double _CoefValue,
                                          double _PotentialCostLC,
                                          double _PotentialCostCC,
                                          double _ItemVatPercentage,
                                          string _ModPrice,
                                          double _PercentageCoef2,
                                          double _PercentageCoef3,
                                          ref double PriceLC,
                                          ref double PriceCC,
                                          ref double Price2LC,
                                          ref double Price2CC,
                                          ref double Price3LC,
                                          ref double Price3CC
                                          )
        {

            string BaseCurrency;
            double CoefValue; //vbCoefVal
            double PotentialCost; //vbPCost
            double ItemVatPercentage; //vbItmVatPer
            string ModPrice = _ModPrice;
            double PercentageCoef2 = _PercentageCoef2; //Per2
            double PercentageCoef3 = _PercentageCoef3; //Per3

            double vbVal = 0, vbVal2 = 0, vbVal3 = 0;

            string vbOper = ""; double vbRate = 0;
            ClsCom.GetRate(ClsConfig.gbCurr2, DateTime.Now.ToString("dd/MM/yyyy"), "LC", ref vbOper, ref vbRate, ClsConfig.locationDirectives.Ld_POSRate);

            //Condition to be applied for the lebanese market in case of inflated $
            if (ClsConfig.gbCurr1?.ToUpper() == "LL".ToUpper() || ClsConfig.gbCurr1?.ToUpper() == "LBP".ToUpper())
            {
                BaseCurrency = _BaseCurrency;//Based on the old gift flag
            }
            else
            {
                if (_PotentialCurrency?.ToUpper() == ClsConfig.gbCurr1?.ToUpper())
                {
                    BaseCurrency = ClsConfig.gbCurr1;
                }
                else
                {
                    BaseCurrency = ClsConfig.gbCurr2;
                }
            }

            if (_CoefValue < 0)
            {
                throw new Exception("Invalid Coef Value. Must be greater than Zero");
            }
            else
            {
                CoefValue = _CoefValue;
            }

            if (BaseCurrency?.ToUpper() == ClsConfig.gbCurr1.ToUpper())
            {
                PotentialCost = _PotentialCostLC;
            }
            else
            {
                PotentialCost = _PotentialCostCC;
            }

            if (_ItemVatPercentage < 0)
            {
                throw new Exception("Invalid Vat Percentage. Must be greater than Zero");
            }
            else
            {
                ItemVatPercentage = _ItemVatPercentage;
            }

            if (ClsConfig.locationDirectives.Ld_SalePriceCalculationMode?.ToUpper() == "M".ToUpper() ||
                ClsConfig.locationDirectives.Ld_SalePriceCalculationMode?.ToUpper() == "Y".ToUpper())
            {
                vbVal = PotentialCost * (1 + (CoefValue / 100));
            }
            else
            {
                if (CoefValue >= 0 && CoefValue < 100)
                {
                    vbVal = PotentialCost / (1 - (CoefValue / 100));
                }
                else
                {
                    vbVal = PotentialCost + ((PotentialCost * CoefValue) / 100);
                }
            }

            if (ItemVatPercentage != 0)
            {
                vbVal += ((vbVal * ItemVatPercentage) / 100);
            }

            if (ClsConfig.locationDirectives.Ld_SalePriceCalculationMode?.ToUpper() == "M".ToUpper() ||
                ClsConfig.locationDirectives.Ld_SalePriceCalculationMode?.ToUpper() == "Y".ToUpper())
            {

                if (string.IsNullOrEmpty(ModPrice) | ModPrice?.ToUpper() == "Price2".ToUpper())
                {
                    CoefValue = PercentageCoef2;

                    if (ClsConfig.locationDirectives.Ld_SalePriceCalculationMode?.ToUpper() == "Y".ToUpper())
                    {
                        vbVal2 = PotentialCost * (1 + (CoefValue / 100));
                    }
                    else
                    {
                        if (CoefValue >= 0 && CoefValue < 100)
                        {
                            vbVal2 = PotentialCost / (1 - (CoefValue / 100));
                        }
                        else
                        {
                            vbVal2 = PotentialCost + ((PotentialCost * CoefValue) / 100);
                        }
                    }

                    if (ItemVatPercentage != 0) { vbVal2 += ((vbVal2 * ItemVatPercentage) / 100); }
                }

                if (string.IsNullOrEmpty(ModPrice) | ModPrice?.ToUpper() == "Price3".ToUpper())
                {
                    CoefValue = PercentageCoef3;

                    if (ClsConfig.locationDirectives.Ld_SalePriceCalculationMode?.ToUpper() == "Y".ToUpper())
                    {
                        vbVal3 = PotentialCost * (1 + (CoefValue / 100));
                    }
                    else
                    {
                        if (CoefValue >= 0 && CoefValue < 100)
                        {
                            vbVal3 = PotentialCost / (1 - (CoefValue / 100));
                        }
                        else
                        {
                            vbVal3 = PotentialCost + ((PotentialCost * CoefValue) / 100);
                        }
                    }

                    if (ItemVatPercentage != 0) { vbVal3 += ((vbVal3 * ItemVatPercentage) / 100); }
                }

            }

            if (BaseCurrency?.ToUpper() == ClsConfig.gbCurr1?.ToUpper())
            {

                vbVal = RoundPrice(Math.Round(vbVal, ClsConfig.locationDirectives.Ld_PriceLC));

                if (string.IsNullOrEmpty(ModPrice) || ModPrice?.ToUpper() == "Price1".ToUpper())
                {

                    PriceLC = vbVal;

                    if (vbOper != "" && vbRate != -1)
                    {
                        if (vbOper == "*")
                        {
                            PriceCC = Convert.ToDouble(vbRate == 0 ? 0 : Math.Round(vbVal / vbRate, ClsConfig.locationDirectives.Ld_PriceCC));
                        }
                        else
                        {
                            PriceCC = Convert.ToDouble(vbRate == 0 ? 0 : Math.Round(vbVal * vbRate, ClsConfig.locationDirectives.Ld_PriceCC));
                        }
                    }
                    else
                    {
                        PriceCC = 0;
                    }

                }

                if (ClsConfig.locationDirectives.Ld_SalePriceCalculationMode?.ToUpper() == "X".ToUpper() ||
                ClsConfig.locationDirectives.Ld_SalePriceCalculationMode?.ToUpper() == "Y".ToUpper())
                {

                    if (string.IsNullOrEmpty(ModPrice) | ModPrice?.ToUpper() == "Price2".ToUpper())
                    {

                        vbVal2 = RoundPrice(Math.Round(vbVal2, ClsConfig.locationDirectives.Ld_PriceLC));

                        Price2LC = vbVal2;

                        if (vbOper != "" && vbRate != -1)
                        {
                            if (vbOper == "*")
                            {
                                Price2CC = Convert.ToDouble(vbRate == 0 ? 0 : Math.Round(vbVal2 / vbRate, ClsConfig.locationDirectives.Ld_PriceCC));
                            }
                            else
                            {
                                Price2CC = Convert.ToDouble(vbRate == 0 ? 0 : Math.Round(vbVal2 * vbRate, ClsConfig.locationDirectives.Ld_PriceCC));
                            }
                        }
                        else
                        {
                            Price2CC = 0;
                        }

                    }

                    if (string.IsNullOrEmpty(ModPrice) | ModPrice?.ToUpper() == "Price3".ToUpper())
                    {

                        vbVal3 = RoundPrice(Math.Round(vbVal3, ClsConfig.locationDirectives.Ld_PriceLC));

                        Price3LC = vbVal3;

                        if (vbOper != "" && vbRate != -1)
                        {

                            if (vbOper == "*")
                            {
                                Price3CC = Convert.ToDouble(vbRate == 0 ? 0 : Math.Round(vbVal3 / vbRate, ClsConfig.locationDirectives.Ld_PriceCC));
                            }
                            else
                            {
                                Price3CC = Convert.ToDouble(vbRate == 0 ? 0 : Math.Round(vbVal3 * vbRate, ClsConfig.locationDirectives.Ld_PriceCC));
                            }

                        }
                        else
                        {
                            Price3CC = 0;
                        }

                    }

                }

            }
            else // (BaseCurrency.ToUpper() != ClsConfig.gbCurr1.ToUpper())
            {

                if (string.IsNullOrEmpty(ModPrice) | ModPrice?.ToUpper() == "Price1".ToUpper())
                {

                    PriceCC = Math.Round(vbVal, ClsConfig.locationDirectives.Ld_PriceCC);

                    if (vbOper != "" && vbRate != -1)
                    {
                        if (vbOper == "*")
                        {
                            PriceLC = vbRate == 0 ? 0 : RoundPrice(Math.Round(vbVal * vbRate, ClsConfig.locationDirectives.Ld_PriceLC));
                        }
                        else
                        {
                            PriceLC = vbRate == 0 ? 0 : RoundPrice(Math.Round(vbVal / vbRate, ClsConfig.locationDirectives.Ld_PriceLC));
                        }
                    }
                    else
                    {
                        PriceLC = 0;
                    }

                }

                if (ClsConfig.locationDirectives.Ld_SalePriceCalculationMode?.ToUpper() == "X".ToUpper() ||
                ClsConfig.locationDirectives.Ld_SalePriceCalculationMode?.ToUpper() == "Y".ToUpper())
                {

                    if (string.IsNullOrEmpty(ModPrice) | ModPrice?.ToUpper() == "Price2".ToUpper())
                    {

                        Price2CC = Math.Round(vbVal2, ClsConfig.locationDirectives.Ld_PriceCC);

                        if (vbOper != "" && vbRate != -1)
                        {
                            if (vbOper == "*")
                            {
                                Price2LC = Convert.ToDouble(vbRate == 0 ? 0 : RoundPrice(Math.Round(vbVal2 * vbRate, ClsConfig.locationDirectives.Ld_PriceLC)));
                            }
                            else
                            {
                                Price2LC = Convert.ToDouble(vbRate == 0 ? 0 : RoundPrice(Math.Round(vbVal2 / vbRate, ClsConfig.locationDirectives.Ld_PriceLC)));
                            }
                        }
                        else
                        {
                            Price2LC = 0;
                        }

                    }

                    if (string.IsNullOrEmpty(ModPrice) | ModPrice?.ToUpper() == "Price3".ToUpper())
                    {

                        Price3CC = Math.Round(vbVal3, ClsConfig.locationDirectives.Ld_PriceCC);

                        if (vbOper != "" && vbRate != -1)
                        {
                            if (vbOper == "*")
                            {
                                Price3LC = Convert.ToDouble(vbRate == 0 ? 0 : RoundPrice(Math.Round(vbVal3 * vbRate, ClsConfig.locationDirectives.Ld_PriceLC)));
                            }
                            else
                            {
                                Price3LC = Convert.ToDouble(vbRate == 0 ? 0 : RoundPrice(Math.Round(vbVal3 / vbRate, ClsConfig.locationDirectives.Ld_PriceLC)));
                            }
                        }
                        else
                        {
                            Price3LC = 0;
                        }

                    }

                }
                else
                {

                    if (string.IsNullOrEmpty(ModPrice) | ModPrice?.ToUpper() == "Price1".ToUpper())
                    {

                        if (vbOper != "" && vbRate != -1)
                        {
                            if (vbOper == "*")
                            {
                                PriceLC = Convert.ToDouble(vbRate == 0 ? 0 : RoundPrice(Math.Round(vbVal * vbRate, ClsConfig.locationDirectives.Ld_PriceLC)));
                            }
                            else
                            {
                                PriceLC = Convert.ToDouble(vbRate == 0 ? 0 : RoundPrice(Math.Round(vbVal / vbRate, ClsConfig.locationDirectives.Ld_PriceLC)));
                            }
                        }
                        else
                        {
                            PriceLC = 0;
                        }

                        vbVal = RoundPrice(PriceLC);

                        if (vbOper != "" && vbRate != -1)
                        {
                            if (vbOper == "*")
                            {
                                PriceCC = Convert.ToDouble(vbRate == 0 ? 0 : Math.Round(vbVal / vbRate, ClsConfig.locationDirectives.Ld_PriceCC));
                            }
                            else
                            {
                                PriceCC = Convert.ToDouble(vbRate == 0 ? 0 : Math.Round(vbVal * vbRate, ClsConfig.locationDirectives.Ld_PriceCC));
                            }
                        }
                        else
                        {
                            PriceCC = 0;
                        }

                    }

                }

                if (ClsConfig.locationDirectives.Ld_SalePriceCalculationMode?.ToUpper() == "X".ToUpper() ||
                ClsConfig.locationDirectives.Ld_SalePriceCalculationMode?.ToUpper() == "Y".ToUpper())
                {

                    if (string.IsNullOrEmpty(ModPrice) | ModPrice?.ToUpper() == "Price2".ToUpper())
                    {

                        if (vbOper != "" && vbRate != -1)
                        {
                            if (vbOper == "*")
                            {
                                Price2LC = Convert.ToDouble(vbRate == 0 ? 0 : RoundPrice(Math.Round(vbVal2 * vbRate, ClsConfig.locationDirectives.Ld_PriceLC)));
                            }
                            else
                            {
                                Price2LC = Convert.ToDouble(vbRate == 0 ? 0 : RoundPrice(Math.Round(vbVal2 / vbRate, ClsConfig.locationDirectives.Ld_PriceLC)));
                            }
                        }
                        else
                        {
                            Price2LC = 0;
                        }

                        vbVal2 = Price2LC;

                        if (vbOper != "" && vbRate != -1)
                        {
                            if (vbOper == "*")
                            {
                                Price2CC = Convert.ToDouble(vbRate == 0 ? 0 : (Math.Round(vbVal2 / vbRate, ClsConfig.locationDirectives.Ld_PriceCC)));
                            }
                            else
                            {
                                Price2CC = Convert.ToDouble(vbRate == 0 ? 0 : (Math.Round(vbVal2 * vbRate, ClsConfig.locationDirectives.Ld_PriceCC)));
                            }
                        }
                        else
                        {
                            Price2CC = 0;
                        }

                    }

                    if (string.IsNullOrEmpty(ModPrice) | ModPrice?.ToUpper() == "Price3".ToUpper())
                    {

                        if (vbOper != "" && vbRate != -1)
                        {
                            if (vbOper == "*")
                            {
                                Price3LC = Convert.ToDouble(vbRate == 0 ? 0 : RoundPrice(Math.Round(vbVal3 * vbRate, ClsConfig.locationDirectives.Ld_PriceLC)));
                            }
                            else
                            {
                                Price3LC = Convert.ToDouble(vbRate == 0 ? 0 : RoundPrice(Math.Round(vbVal3 / vbRate, ClsConfig.locationDirectives.Ld_PriceLC)));
                            }
                        }
                        else
                        {
                            Price3LC = 0;
                        }

                        vbVal3 = Price3LC;

                        if (vbOper != "" && vbRate != -1)
                        {
                            if (vbOper == "*")
                            {
                                Price3CC = Convert.ToDouble(vbRate == 0 ? 0 : Math.Round(vbVal3 / vbRate, ClsConfig.locationDirectives.Ld_PriceCC));
                            }
                            else
                            {
                                Price3CC = Convert.ToDouble(vbRate == 0 ? 0 : Math.Round(vbVal3 * vbRate, ClsConfig.locationDirectives.Ld_PriceCC));
                            }
                        }
                        else
                        {
                            Price3CC = 0;
                        }

                    }

                }
            }

        }

        public static double RoundPrice(double price)
        {
            try
            {
                if (ClsConfig.locationDirectives.Ld_PriceRoundValueLC == 0)
                {
                    return price;
                }
                else
                {
                    if (price % ClsConfig.locationDirectives.Ld_PriceRoundValueLC == 0)
                    {
                        return price;
                    }
                    else
                    {
                        return (double)(price - (price % ClsConfig.locationDirectives.Ld_PriceRoundValueLC) + ClsConfig.locationDirectives.Ld_PriceRoundValueLC);
                    }
                }
            }
            catch (Exception)
            {
                return price;
            }
        }

        public static void CalculatePriceFromPotCost(double PotCost,
                                                     string SalesCoefVal,
                                                     bool SubjectVAT,
                                                     string VatPerc,
                                                     string DiscPrice2,
                                                     string DiscPrice3,
                                                     string FCCode,
                                                     bool BaseCurrencyLC,
                                                     bool BaseCurrencyCC,
                                                     ref string txtPrice1LL,
                                                     ref string txtPrice2LL,
                                                     ref string txtPrice3LL,
                                                     ref string txtPrice1CC,
                                                     ref string txtPrice2CC,
                                                     ref string txtPrice3CC,
                                                     string ModPrice = "",
                                                     double Discount = 0)
        {
            try
            {
                double Prc = 0;
                double Prc2 = 0;
                double Prc3 = 0;
                string Oper2, Oper3;
                double Rate2, Rate3 = 0;
                string Operator = "";
                double Rate = 0;

                if (!string.IsNullOrWhiteSpace(SalesCoefVal.Trim()) && double.TryParse(SalesCoefVal.Trim(), out double coefVal))
                {
                    if (ClsConfig.locationDirectives.Ld_SalePriceCalculationMode?.ToUpper() == "M".ToUpper() || ClsConfig.locationDirectives.Ld_SalePriceCalculationMode?.ToUpper() == "Y".ToUpper())
                    {
                        Prc = PotCost * (1 + coefVal / 100);
                    }
                    else if (ClsConfig.locationDirectives.Ld_SalePriceCalculationMode?.ToUpper() == "N".ToUpper() || ClsConfig.locationDirectives.Ld_SalePriceCalculationMode?.ToUpper() == "X".ToUpper())
                    {
                        if (coefVal >= 0 && coefVal < 100)
                        {
                            Prc = PotCost / (1 - coefVal / 100);
                        }
                        else
                        {
                            Prc = PotCost + (PotCost * coefVal / 100);
                        }
                    }

                    if (SubjectVAT && VatPerc != "")
                    {
                        Prc += Prc * Convert.ToDouble(VatPerc) / 100;
                    }
                }

                if (ClsConfig.locationDirectives.Ld_SalePriceCalculationMode?.ToUpper() == "X".ToUpper() || ClsConfig.locationDirectives.Ld_SalePriceCalculationMode?.ToUpper() == "Y".ToUpper())
                {
                    if (!string.IsNullOrWhiteSpace(DiscPrice2.Trim()) && double.TryParse(DiscPrice2.Trim(), out double qtyPrc1))
                    {
                        if (ClsConfig.locationDirectives.Ld_SalePriceCalculationMode?.ToUpper() == "M".ToUpper() || ClsConfig.locationDirectives.Ld_SalePriceCalculationMode?.ToUpper() == "Y".ToUpper())
                        {
                            Prc2 = PotCost * (1 + Convert.ToDouble(DiscPrice2) / 100);
                        }
                        else if (ClsConfig.locationDirectives.Ld_SalePriceCalculationMode?.ToUpper() == "N".ToUpper() || ClsConfig.locationDirectives.Ld_SalePriceCalculationMode?.ToUpper() == "X".ToUpper())
                        {
                            if (Convert.ToDouble(DiscPrice2) >= 0 && Convert.ToDouble(DiscPrice2) < 100)
                            {
                                Prc2 = PotCost / (1 - Convert.ToDouble(DiscPrice2) / 100);
                            }
                            else
                            {
                                Prc2 = PotCost + (PotCost * Convert.ToDouble(DiscPrice2) / 100);
                            }
                        }

                        if (SubjectVAT == true && VatPerc != "")
                        {
                            Prc2 += Prc2 * Convert.ToDouble(VatPerc) / 100;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(DiscPrice3.Trim()) && double.TryParse(DiscPrice3.Trim(), out double qtyPrc2))
                    {
                        if (ClsConfig.locationDirectives.Ld_SalePriceCalculationMode?.ToUpper() == "M".ToUpper() || ClsConfig.locationDirectives.Ld_SalePriceCalculationMode?.ToUpper() == "Y".ToUpper())
                        {
                            Prc3 = PotCost * (1 + Convert.ToDouble(DiscPrice3) / 100);
                        }
                        else if (ClsConfig.locationDirectives.Ld_SalePriceCalculationMode?.ToUpper() == "N".ToUpper() || ClsConfig.locationDirectives.Ld_SalePriceCalculationMode?.ToUpper() == "X".ToUpper())
                        {
                            if (Convert.ToDouble(DiscPrice3) >= 0 && Convert.ToDouble(DiscPrice3) < 100)
                            {
                                Prc3 = PotCost / (1 - Convert.ToDouble(DiscPrice3) / 100);
                            }
                            else
                            {
                                Prc3 = PotCost + (PotCost * Convert.ToDouble(DiscPrice3) / 100);
                            }
                        }

                        if (SubjectVAT == true && VatPerc != "")
                        {
                            Prc3 += Prc3 * Convert.ToDouble(VatPerc) / 100;
                        }
                    }
                }

                Prc = Math.Round(Prc, 5);
                Prc2 = Math.Round(Prc2, 5);
                Prc3 = Math.Round(Prc3, 5);

                ClsCom.GetRate(ClsConfig.gbCurr2, DateTime.Today.ToString("dd/MM/yyyy"), "LC", ref Operator, ref Rate, ClsConfig.locationDirectives.Ld_POSRate);
                Oper2 = Operator;
                Rate2 = Rate;

                ClsCom.GetRate(FCCode, DateTime.Today.ToString("dd/MM/yyyy"), "CC", ref Operator, ref Rate, ClsConfig.locationDirectives.Ld_POSRate);
                Oper3 = Operator;
                Rate3 = Rate;

                if (Discount != 0)
                {
                    Prc = Prc * (1 - (Discount / 100));
                
                }

                if (BaseCurrencyLC == true)
                {
                    Prc = ClsPricing.RoundPrice(Math.Round(Prc, ClsConfig.locationDirectives.Ld_PriceLC));
                    Prc2 = ClsPricing.RoundPrice(Math.Round(Prc, ClsConfig.locationDirectives.Ld_PriceLC));
                    Prc3 = ClsPricing.RoundPrice(Math.Round(Prc, ClsConfig.locationDirectives.Ld_PriceLC));

                    if (string.IsNullOrEmpty(ModPrice) || ModPrice == "Price1")
                    {
                        txtPrice1LL = Math.Round(Convert.ToDouble(Prc), ClsConfig.locationDirectives.Ld_PriceLC).ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat);
                    }

                    if (ClsConfig.locationDirectives.Ld_SalePriceCalculationMode?.ToUpper() == "M".ToUpper() || ClsConfig.locationDirectives.Ld_SalePriceCalculationMode?.ToUpper() == "N".ToUpper())
                    {
                        txtPrice2LL = (DiscPrice2 == "") | (DiscPrice2 != "" && (Convert.ToDouble(DiscPrice2) == 0)) ?
                            Math.Round(Convert.ToDouble(Prc), ClsConfig.locationDirectives.Ld_PriceLC).ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat) :
                            ClsPricing.RoundPrice(Math.Round(Convert.ToDouble(Prc) + (Convert.ToDouble(Prc) * Convert.ToDouble(string.IsNullOrEmpty(DiscPrice2) ? "0" : DiscPrice2) / 100.0), ClsConfig.locationDirectives.Ld_PriceLC)).ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat);

                        txtPrice3LL = (DiscPrice3 == "") | (DiscPrice3 != "" && (Convert.ToDouble(DiscPrice3) == 0)) ?
                            Math.Round(Convert.ToDouble(Prc), ClsConfig.locationDirectives.Ld_PriceLC).ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat) :
                            ClsPricing.RoundPrice(Math.Round(Convert.ToDouble(Prc) + (Convert.ToDouble(Prc) * Convert.ToDouble(string.IsNullOrEmpty(DiscPrice3) ? "0" : DiscPrice3) / 100.0), ClsConfig.locationDirectives.Ld_PriceLC)).ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(ModPrice) || ModPrice == "Price2")
                            txtPrice2LL = Math.Round(Convert.ToDouble(Prc2), ClsConfig.locationDirectives.Ld_PriceLC).ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat);
                        if (string.IsNullOrEmpty(ModPrice) || ModPrice == "Price3")
                            txtPrice3LL = Math.Round(Convert.ToDouble(Prc3), ClsConfig.locationDirectives.Ld_PriceLC).ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat);
                    }

                    if (ClsConfig.locationDirectives.Ld_SalePriceCalculationMode?.ToUpper() == "M".ToUpper() || ClsConfig.locationDirectives.Ld_SalePriceCalculationMode?.ToUpper() == "N".ToUpper())
                    {
                        if (Oper2 == "*")
                        {
                            txtPrice1CC = Math.Round(Prc / Rate2, 2).ToString(ClsConfig.locationDirectives.Ld_CostCCFormat);
                            txtPrice2CC = Math.Round((Convert.ToDouble(string.IsNullOrEmpty(txtPrice2LL) ? "0" : txtPrice2LL) / Rate2), ClsConfig.locationDirectives.Ld_PriceCC).ToString(ClsConfig.locationDirectives.Ld_PriceCCFormat);
                            txtPrice3CC = Math.Round((Convert.ToDouble(string.IsNullOrEmpty(txtPrice3LL) ? "0" : txtPrice3LL) / Rate2), ClsConfig.locationDirectives.Ld_PriceCC).ToString(ClsConfig.locationDirectives.Ld_PriceCCFormat);
                        }
                        else if (Oper2 == "/")
                        {
                            txtPrice1CC = Math.Round(Prc * Rate2, 2).ToString("N");
                            txtPrice2CC = Math.Round((Convert.ToDouble(string.IsNullOrEmpty(txtPrice2LL) ? "0" : txtPrice2LL) * Rate2), ClsConfig.locationDirectives.Ld_PriceCC).ToString(ClsConfig.locationDirectives.Ld_PriceCCFormat);
                            txtPrice3CC = Math.Round((Convert.ToDouble(string.IsNullOrEmpty(txtPrice3LL) ? "0" : txtPrice3LL) * Rate2), ClsConfig.locationDirectives.Ld_PriceCC).ToString(ClsConfig.locationDirectives.Ld_PriceCCFormat);
                        }
                    }
                    else
                    {
                        if (Oper2 == "*")
                        {
                            if (string.IsNullOrEmpty(ModPrice) | ModPrice == "Price1") txtPrice1CC = Math.Round(Prc / Rate2, ClsConfig.locationDirectives.Ld_PriceCC).ToString(ClsConfig.locationDirectives.Ld_PriceCCFormat);
                            if (string.IsNullOrEmpty(ModPrice) | ModPrice == "Price2") txtPrice2CC = Math.Round(Prc2 / Rate2, ClsConfig.locationDirectives.Ld_PriceCC).ToString(ClsConfig.locationDirectives.Ld_PriceCCFormat);
                            if (string.IsNullOrEmpty(ModPrice) | ModPrice == "Price3") txtPrice3CC = Math.Round(Prc3 / Rate2, ClsConfig.locationDirectives.Ld_PriceCC).ToString(ClsConfig.locationDirectives.Ld_PriceCCFormat);
                        }
                        else if (Oper2 == "/")
                        {

                            if (string.IsNullOrEmpty(ModPrice) | ModPrice == "Price1") txtPrice1CC = Math.Round(Prc * Rate2, ClsConfig.locationDirectives.Ld_PriceCC).ToString(ClsConfig.locationDirectives.Ld_PriceCCFormat);
                            if (string.IsNullOrEmpty(ModPrice) | ModPrice == "Price2") txtPrice2CC = Math.Round(Prc2 * Rate2, ClsConfig.locationDirectives.Ld_PriceCC).ToString(ClsConfig.locationDirectives.Ld_PriceCCFormat);
                            if (string.IsNullOrEmpty(ModPrice) | ModPrice == "Price3") txtPrice3CC = Math.Round(Prc3 * Rate2, ClsConfig.locationDirectives.Ld_PriceCC).ToString(ClsConfig.locationDirectives.Ld_PriceCCFormat);

                        }
                    }
                }
                else if (BaseCurrencyCC == true)
                {
                    if (FCCode != ClsConfig.gbCurr2)
                    {
                        if (Oper3 == "*")
                        {
                            Prc *= Rate3;
                            Prc2 *= Rate3;
                            Prc3 *= Rate3;
                        }
                        else if (Oper3 == "/")
                        {
                            Prc /= Rate3;
                            Prc2 /= Rate3;
                            Prc3 /= Rate3;
                        }
                    }

                    if (string.IsNullOrEmpty(ModPrice) || ModPrice == "Price1")
                    {
                        txtPrice1CC = Math.Round(Prc, ClsConfig.locationDirectives.Ld_PriceCC).ToString(ClsConfig.locationDirectives.Ld_PriceCCFormat);
                    }

                    if (ClsConfig.locationDirectives.Ld_SalePriceCalculationMode?.ToUpper() == "M".ToUpper() || ClsConfig.locationDirectives.Ld_SalePriceCalculationMode?.ToUpper() == "N".ToUpper())
                    {
                        txtPrice2CC = (DiscPrice2 == "") | (DiscPrice2 != "" && (Convert.ToDouble(DiscPrice2) == 0)) ?
                            Math.Round(Convert.ToDouble(Prc), ClsConfig.locationDirectives.Ld_PriceCC).ToString(ClsConfig.locationDirectives.Ld_PriceCCFormat) :
                            (Math.Round(Convert.ToDouble(Prc) + (Convert.ToDouble(Prc) * Convert.ToDouble(string.IsNullOrEmpty(DiscPrice2) ? "0" : DiscPrice2) / 100), ClsConfig.locationDirectives.Ld_PriceCC)).ToString(ClsConfig.locationDirectives.Ld_PriceCCFormat);

                        txtPrice3CC = (DiscPrice3 == "") | (DiscPrice3 != "" && (Convert.ToDouble(DiscPrice3) == 0)) ?
                            Math.Round(Convert.ToDouble(Prc), ClsConfig.locationDirectives.Ld_PriceCC).ToString(ClsConfig.locationDirectives.Ld_PriceCCFormat) :
                            (Math.Round(Convert.ToDouble(Prc) + (Convert.ToDouble(Prc) * Convert.ToDouble(string.IsNullOrEmpty(DiscPrice3) ? "0" : DiscPrice3) / 100), ClsConfig.locationDirectives.Ld_PriceCC)).ToString(ClsConfig.locationDirectives.Ld_PriceCCFormat);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(ModPrice) || ModPrice == "Price2")
                            txtPrice2CC = Math.Round(Convert.ToDouble(Prc2), ClsConfig.locationDirectives.Ld_PriceCC).ToString(ClsConfig.locationDirectives.Ld_PriceCCFormat);
                        if (string.IsNullOrEmpty(ModPrice) || ModPrice == "Price3")
                            txtPrice3CC = Math.Round(Convert.ToDouble(Prc3), ClsConfig.locationDirectives.Ld_PriceCC).ToString(ClsConfig.locationDirectives.Ld_PriceCCFormat);
                    }

                    if (ClsConfig.locationDirectives.Ld_SalePriceCalculationMode?.ToUpper() == "M".ToUpper() || ClsConfig.locationDirectives.Ld_SalePriceCalculationMode?.ToUpper() == "N".ToUpper())
                    {
                        if (Oper2 == "*")
                        {
                            txtPrice1LL = ClsPricing.RoundPrice(Math.Round(Convert.ToDouble(Prc * Rate2), ClsConfig.locationDirectives.Ld_PriceLC)).ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat);
                            txtPrice2LL = ClsPricing.RoundPrice(Math.Round(Convert.ToDouble(string.IsNullOrEmpty(txtPrice2CC) ? "0" : txtPrice2CC) * Rate2, ClsConfig.locationDirectives.Ld_PriceLC)).ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat);
                            txtPrice3LL = ClsPricing.RoundPrice(Math.Round(Convert.ToDouble(string.IsNullOrEmpty(txtPrice3CC) ? "0" : txtPrice3CC) * Rate2, ClsConfig.locationDirectives.Ld_PriceLC)).ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat);
                        }
                        else if (Oper2 == "/")
                        {
                            txtPrice1LL = ClsPricing.RoundPrice(Math.Round(Convert.ToDouble(Prc / Rate2), ClsConfig.locationDirectives.Ld_PriceLC)).ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat);
                            txtPrice2LL = ClsPricing.RoundPrice(Math.Round(Convert.ToDouble(string.IsNullOrEmpty(txtPrice2CC) ? "0" : txtPrice2CC) / Rate2, ClsConfig.locationDirectives.Ld_PriceLC)).ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat);
                            txtPrice3LL = ClsPricing.RoundPrice(Math.Round(Convert.ToDouble(string.IsNullOrEmpty(txtPrice3CC) ? "0" : txtPrice3CC) / Rate2, ClsConfig.locationDirectives.Ld_PriceLC)).ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat);
                        }

                    }
                    else
                    {
                        if (Oper2 == "*")
                        {
                            if (string.IsNullOrEmpty(ModPrice) | ModPrice == "Price1") txtPrice1LL = ClsPricing.RoundPrice(Math.Round(Prc * Rate2, ClsConfig.locationDirectives.Ld_PriceLC)).ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat);
                            if (string.IsNullOrEmpty(ModPrice) | ModPrice == "Price2") txtPrice2LL = ClsPricing.RoundPrice(Math.Round(Prc2 * Rate2, ClsConfig.locationDirectives.Ld_PriceLC)).ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat);
                            if (string.IsNullOrEmpty(ModPrice) | ModPrice == "Price3") txtPrice3LL = ClsPricing.RoundPrice(Math.Round(Prc3 * Rate2, ClsConfig.locationDirectives.Ld_PriceLC)).ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat);
                        }
                        else if (Oper2 == "/")
                        {

                            if (string.IsNullOrEmpty(ModPrice) | ModPrice == "Price1") txtPrice1LL = ClsPricing.RoundPrice(Math.Round(Prc / Rate2, ClsConfig.locationDirectives.Ld_PriceLC)).ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat);
                            if (string.IsNullOrEmpty(ModPrice) | ModPrice == "Price2") txtPrice2LL = ClsPricing.RoundPrice(Math.Round(Prc2 / Rate2, ClsConfig.locationDirectives.Ld_PriceLC)).ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat);
                            if (string.IsNullOrEmpty(ModPrice) | ModPrice == "Price3") txtPrice3LL = ClsPricing.RoundPrice(Math.Round(Prc3 / Rate2, ClsConfig.locationDirectives.Ld_PriceLC)).ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat);

                        }
                    }
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        public static void CalculatePotCost(string UnitCost,
                                            string SupVatType,
                                            bool SubjectVATChecked,
                                            string VatPerc,
                                            string ItemDisc,
                                            bool UseItmDisc,
                                            string InvDisc,
                                            bool UseInvDisc,
                                            string PurchaseQty,
                                            string PromotionQty,
                                            bool UsePromQty,
                                            string CustomsPerc,
                                            string FixedPer,
                                            string PricingCurrLC,
                                            string PricingCurrCC,
                                            ref string PotCostLC,
                                            ref string PotCostCC)
        {
            try
            {
                string Operator = "";
                double Rate = 0;
                double Res = 0, QtyPur = 0, QtyProm = 0;

                if (!string.IsNullOrEmpty(UnitCost))
                {
                    Res = Convert.ToDouble(UnitCost);

                    if (SupVatType == "T" && SubjectVATChecked)
                    {
                        Res = (100 * Res) / (100 + Convert.ToDouble(VatPerc));
                    }

                    if (!string.IsNullOrEmpty(ItemDisc) && double.TryParse(ItemDisc, out double unitDisc) && UseItmDisc)
                    {
                        Res -= Res * unitDisc / 100;
                    }

                    if (!string.IsNullOrEmpty(InvDisc) && double.TryParse(InvDisc, out double addDisc) && UseInvDisc)
                    {
                        Res -= Res * addDisc / 100;
                    }

                    if (!string.IsNullOrEmpty(PurchaseQty) && double.TryParse(PurchaseQty, out double purQty))
                    {
                        QtyPur += purQty;
                    }

                    if (!string.IsNullOrEmpty(PromotionQty) && double.TryParse(PromotionQty, out double promQty))
                    {
                        QtyProm += promQty;
                    }

                    if (QtyPur != 0 && QtyProm != 0 && UsePromQty)
                    {
                        Res *= QtyPur / (QtyPur + QtyProm);
                    }

                    if (ClsConfig.oHSystem.Osh_Active == "Y" || ClsConfig.oHSystem.Osh_Active == "A")
                    {
                        if (string.IsNullOrEmpty(CustomsPerc)) CustomsPerc = "0.00";
                        if (string.IsNullOrEmpty(FixedPer)) FixedPer = "0.00";
                        Res += Res * (double.Parse(CustomsPerc) + double.Parse(FixedPer)) / 100;
                    }

                    Res = Math.Round(Res, 3);

                    if (PricingCurrLC == ClsConfig.gbCurr1)
                    {

                        ClsCom.GetRate(ClsConfig.gbCurr2, DateTime.Today.ToString("dd/MM/yyyy"), "LC", ref Operator, ref Rate, ClsConfig.locationDirectives.Ld_POSRate);

                        PotCostLC = Math.Round(Convert.ToDouble(Res), ClsConfig.locationDirectives.Ld_CostLC).ToString(ClsConfig.locationDirectives.Ld_CostLCFormat);

                        if (Operator != "" && Rate != -1)
                        {
                            if (Operator == "*")
                            {
                                PotCostCC = Math.Round((Res / Rate), ClsConfig.locationDirectives.Ld_CostCC).ToString(ClsConfig.locationDirectives.Ld_CostCCFormat);
                            }
                            else if (Operator == "/")
                            {
                                PotCostCC = Math.Round((Res * Rate), ClsConfig.locationDirectives.Ld_CostCC).ToString(ClsConfig.locationDirectives.Ld_CostCCFormat);
                            }
                        }
                        else
                        {
                            PotCostCC = "0.00";
                        }

                    }
                    else if (PricingCurrCC == ClsConfig.gbCurr2)
                    {
                        ClsCom.GetRate(ClsConfig.gbCurr2, DateTime.Today.ToString("dd/MM/yyyy"), "LC", ref Operator, ref Rate, ClsConfig.locationDirectives.Ld_POSRate);

                        PotCostCC = (Math.Round(Convert.ToDouble(Res), ClsConfig.locationDirectives.Ld_CostCC)).ToString(ClsConfig.locationDirectives.Ld_CostCCFormat);

                        if (Operator != "" && Rate != -1)
                        {
                            if (Operator == "*")
                            {
                                PotCostLC = Math.Round((Res * Rate), ClsConfig.locationDirectives.Ld_CostLC).ToString(ClsConfig.locationDirectives.Ld_CostLCFormat);
                            }
                            else if (Operator == "/")
                            {
                                PotCostLC = Math.Round((Res / Rate), ClsConfig.locationDirectives.Ld_CostLC).ToString(ClsConfig.locationDirectives.Ld_CostLCFormat);
                            }
                        }
                        else
                        {
                            PotCostLC = "0.00";
                        }

                    }
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        public static void CalculatePriceAfterDiscount(string Discount,
                                                       string InitialPriceLL, string InitialPriceCC,
                                                       ref string PriceLL, ref string PriceCC)
        {
            try
            {
                if (Discount != "")
                {

                    if (Convert.ToDouble(InitialPriceLL) != 0)
                    {
                        PriceLL = (Convert.ToDouble(InitialPriceLL) + (Convert.ToDouble(InitialPriceLL) * (Convert.ToDouble(Discount) / 100))).ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat);
                    }
                    else
                    {
                        PriceLL = (Convert.ToDouble(PriceLL) + (Convert.ToDouble(PriceLL) * (Convert.ToDouble(Discount) / 100))).ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat);
                    }
                    if (Convert.ToDouble(InitialPriceCC) != 0)
                    {
                        PriceCC = (Convert.ToDouble(InitialPriceCC) + (Convert.ToDouble(InitialPriceCC) * (Convert.ToDouble(Discount) / 100))).ToString(ClsConfig.locationDirectives.Ld_PriceCCFormat);
                    }
                    else
                    {
                        PriceCC = (Convert.ToDouble(PriceCC) + (Convert.ToDouble(PriceCC) * (Convert.ToDouble(Discount) / 100))).ToString(ClsConfig.locationDirectives.Ld_PriceCCFormat);
                    }
                }
                else
                {
                    PriceLL = (Convert.ToDouble(InitialPriceLL)).ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat);
                    PriceCC = (Convert.ToDouble(InitialPriceCC)).ToString(ClsConfig.locationDirectives.Ld_PriceCCFormat);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static void CalculatePriceForUnits(double toBaseUnit,
                                                  string InitialPrice1LL, string InitialPrice1CC,
                                                  string InitialPrice2LL, string InitialPrice2CC,
                                                  string InitialPrice3LL, string InitialPrice3CC,
                                                  ref string Price1LC, ref string Price1CC,
                                                  ref string Price2LC, ref string Price2CC,
                                                  ref string Price3LC, ref string Price3CC)
        {
            try
            {

                if (!string.IsNullOrWhiteSpace(InitialPrice1LL) && double.TryParse(InitialPrice1LL.Trim(), out double priceValue))
                {
                    priceValue = double.Parse(InitialPrice1LL) * toBaseUnit;
                    Price1LC = priceValue.ToString(ClsConfig.locationDirectives.Ld_CostLCFormat);
                }

                if (!string.IsNullOrWhiteSpace(InitialPrice1CC) && double.TryParse(InitialPrice1CC.Trim(), out double priceValueCC))
                {
                    priceValueCC = double.Parse(InitialPrice1CC) * toBaseUnit;
                    Price1CC = priceValueCC.ToString(ClsConfig.locationDirectives.Ld_CostCCFormat);
                }

                if (!string.IsNullOrWhiteSpace(InitialPrice2LL) && double.TryParse(InitialPrice2LL.Trim(), out double priceValue2))
                {
                    priceValue2 = double.Parse(InitialPrice2LL) * toBaseUnit;
                    Price2LC = priceValue2.ToString(ClsConfig.locationDirectives.Ld_CostLCFormat);
                }

                if (!string.IsNullOrWhiteSpace(InitialPrice2CC) && double.TryParse(InitialPrice2CC.Trim(), out double priceValue2CC))
                {
                    priceValue2CC = double.Parse(InitialPrice2CC) * toBaseUnit;
                    Price2CC = priceValue2CC.ToString(ClsConfig.locationDirectives.Ld_CostCCFormat);
                }

                if (!string.IsNullOrWhiteSpace(InitialPrice3LL) && double.TryParse(InitialPrice3LL.Trim(), out double priceValue3))
                {
                    priceValue3 = Convert.ToDouble(InitialPrice3LL) * toBaseUnit;
                    Price3LC = priceValue3.ToString(ClsConfig.locationDirectives.Ld_CostLCFormat);
                }

                if (!string.IsNullOrWhiteSpace(InitialPrice3CC) && double.TryParse(InitialPrice3CC.Trim(), out double priceValue3CC))
                {
                    priceValue3CC = Convert.ToDouble(InitialPrice3CC) * toBaseUnit;
                    Price3CC = priceValue3CC.ToString(ClsConfig.locationDirectives.Ld_CostCCFormat);
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        public static void CalculateDiscountAfterChangingPrice(string OriginalPrice,
                                                               string NewPrice,
                                                               string OriginalPriceCV,
                                                               ref string Discount,
                                                               ref double NewPriceCV,
                                                               string OldDiscount = "")
        {
            try
            {

                Discount = ((Convert.ToDouble(NewPrice) * Convert.ToDouble(OldDiscount)) / Convert.ToDouble(OriginalPrice)).ToString("N2");

                NewPriceCV = (Convert.ToDouble(OriginalPriceCV) * (Convert.ToDouble(NewPrice) * Convert.ToDouble(OldDiscount)) / Convert.ToDouble(OriginalPrice)) / Convert.ToDouble(OldDiscount);

            }
            catch (Exception)
            {

                throw;
            }
        }

        public static void UpdateStockPrices(SqlCommand sqlCommand, string TempItems)
        {
            try
            {
                string strsql = $@" UPDATE   tbl_StockItm
                                    SET      [Stk_1SlPriceLC] = PriceLC ,
                                             [Stk_1SlPrice$] = PriceCC ,
                                             [Stk_SLCoef] = CASE WHEN Stk_ModSal = 'C'
                                                                              THEN Coef
                                                                              ELSE Stk_SLCoef
                                                                         END
                                    FROM     {TempItems} inter
                                    WHERE tbl_StockItm.[Stk_StkId] = inter.ICD_StockRecId
                                             AND Stk_Loc = '{ClsConfig.LocationCode}'
                                             AND tbl_StockItm.[Stk_ModSal] = 'Y'
                                             AND Stk_UnitCode = inter.ICD_Unit
                                             AND inter.ICD_Type = 'N'
                                             AND inter.ICD_Status = 'R'
                                             AND inter.ChangeSale <> 'N'; ";
                sqlCommand.CommandText = strsql;
                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception)
            {

                throw;
            }

        }


    }
}
