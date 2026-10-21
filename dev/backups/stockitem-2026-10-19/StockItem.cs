using WinApplications;
using WinApplications.Controls;
using WinApplications.Forms;
using SuperStoreBusiness;
using SuperStoreBusiness.Controller;
using SuperStoreBusiness.Model;
using SuperStoreMain.Forms.Reports;
using SuperStoreMain.UserControls;
using SuperStoreStockSearch;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SuperStoreMain.Forms.Steps
{
    public partial class FrmStockItems : Form
    {
        #region Declarations

        private string StepCode;

        private string StepName;

        private bool lcPrivAdd, lcPrivModify, lcPrivDelete, lcPrivView, lcPrivPrint;

        private ClsCommon.FormMode lcFormMode;

        string lastKeysCombination = "";

        public int StockID, StockIDfromOtherForm;

        string ThirdParty;

        int LCcostIscale, CCcostIscale, FCcostIscale, LCpriceIscale, CCpriceIscale, QtyIscale;

        ClsDynamicFields clsDynamicFields;

        private bool AllowAddModifyTechnicalSheet = false;

        private int FillInfo = 0;

        public bool OpenedFromAnotherForm = false, DuplicateItem = false;

        private double AvlbQty;

        #endregion


        #region Step Load / Close

        public FrmStockItems()
        {
            InitializeComponent();

            if (ClsConfig.locationDirectives != null)
            {
                LCpriceIscale = ClsConfig.locationDirectives.Ld_PriceLC;
                CCpriceIscale = ClsConfig.locationDirectives.Ld_PriceCC;
                LCcostIscale = ClsConfig.locationDirectives.Ld_CostLC;
                CCcostIscale = ClsConfig.locationDirectives.Ld_CostCC;
                FCcostIscale = ClsConfig.locationDirectives.Ld_CostFC;
                QtyIscale = ClsConfig.locationDirectives.Ld_NoOfDecQty;

                txtMinQty.Iscale = QtyIscale;
                txtMaxQty.Iscale = QtyIscale;
                txtBBFQty.Iscale = QtyIscale;
                txtAvailableQty.Iscale = QtyIscale;
                txtQtyToReturn.Iscale = QtyIscale;
                txtBBFCostLC.Iscale = LCcostIscale;
                txtActCostLC.Iscale = LCcostIscale;
                txtLastCostLC.Iscale = LCcostIscale;
                txtPotCostLC.Iscale = LCcostIscale;
                txtBBFCostCC.Iscale = CCcostIscale;
                txtActCostCC.Iscale = CCcostIscale;
                txtLastCostCC.Iscale = CCcostIscale;
                txtPotCostCC.Iscale = CCcostIscale;
                txtActCostFC.Iscale = FCcostIscale;
                txtBBFCostFC.Iscale = FCcostIscale;
                txtLastCostFC.Iscale = FCcostIscale;
                txtPotCostFC.Iscale = FCcostIscale;
                txtPrice1LL.Iscale = LCpriceIscale;
                txtPrice2LL.Iscale = LCpriceIscale;
                txtPrice3LL.Iscale = LCpriceIscale;
                txtPrice1CC.Iscale = CCpriceIscale;
                txtPrice2CC.Iscale = CCpriceIscale;
                txtPrice3CC.Iscale = CCpriceIscale;
                txtPurchaseQty.Iscale = QtyIscale;
                txtPromotionQty.Iscale = QtyIscale;
                txtFreeQty.Iscale = QtyIscale;
                txtExtraQty.Iscale = QtyIscale;
                txtContractQty.Iscale = QtyIscale;
                txtPurchaseQtyLastCost.Iscale = QtyIscale;
                txtPromotionQtyLastCost.Iscale = QtyIscale;
                txtFreeQtyLastCost.Iscale = QtyIscale;
                txtExtraQtyLastCost.Iscale = QtyIscale;
                txtContractQtyLastCost.Iscale = QtyIscale;
                //txtYTDQtyIN.Iscale = QtyIscale;
                //txtYTDQtyOUT.Iscale = QtyIscale;

            }
        }

        public void Show(string _StepCode, string _StepName)
        {
            try
            {
                StepCode = _StepCode;
                StepName = _StepName.Substring(_StepName.IndexOf("-") + 1).Trim();
                this.Show();
                Application.DoEvents();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void FrmStock_Load(object sender, EventArgs e)
        {

            try
            {

                //PrepareHelp;

                Text = StepName;

                if (ClsConfig.LoginUser?.ToUpper() == "SU"?.ToUpper())
                {
                    lcPrivAdd = true;
                    lcPrivModify = true;
                    lcPrivDelete = true;
                    lcPrivView = true;
                    lcPrivPrint = true;
                    AllowAddModifyTechnicalSheet = true;
                }
                else
                {
                    //Get Priv
                };

                clsDynamicFields = new ClsDynamicFields()
                {
                    ConnectionString = ClsConfig.SQLConnectionString,
                    intGroup = 0, // Stock File
                    strSubGroup = "", // Category
                    intEntity = -1, // Stock Record ID
                    DynamicFields = StockDynamicFields // User Control
                };
                StockDynamicFields.clsDynamicFields = clsDynamicFields;

                InitForm();

                cntCategory.txtCode.TextChanged += new EventHandler(this.txtKeys_TextChanged);
                cntSupplier.txtCode.TextChanged += new EventHandler(this.txtKeys_TextChanged);
                cntFCCode.txtCode.Validating += new CancelEventHandler(this.CostCalculation_Validating);

                imgItmPhoto.pctPicture.ImageChanged += new EventHandler(this.ItemImageChanged);

                rbBaseCurrencyLC.Text = ClsConfig.gbCurr1;
                rbBaseCurrencyCC.Text = ClsConfig.gbCurr2;
                rbLC.Text = ClsConfig.gbCurr1;

                cntBrand.WithButton = true;
                cntGoodsLoc.WithButton = true;

                //cntDepGrpSub.cntDepartment.frmStockItems = this;
                //cntDepGrpSub.cntGroup.frmStockItems = this;
                //cntDepGrpSub.cntSubGroup1.frmStockItems = this;
                //cntDepGrpSub.cntSubGroup2.frmStockItems = this;
                //cntDepGrpSub.cntSubGroup3.frmStockItems = this;
                //cntDepGrpSub.cntSubGroup4.frmStockItems = this;

                if (OpenedFromAnotherForm)
                {
                    btnAddModifyDelete_Click(btnModify, new EventArgs());
                    pnlKey.Enabled = false;
                    StockItemsController controller = new StockItemsController();
                    StockItems item = new StockItems();
                    item = controller.GetStockItems(StockIDfromOtherForm);
                    cntCategory.txtCode.Text = item.Stk_Category;
                    cntSupplier.txtCode.Text = item.Stk_Supplier;
                    txtSerialNo.Text = item.Stk_Serial;
                    ActiveControl = txtDesc;
                }

                //Add logging
            }
            catch (Exception ex)
            {
                ClsCommon.ShowException(StepName, ex);
            }
        }

        private void InitForm()
        {
            try
            {

                ClsCommon.AutoClearControls(this);
                ManageUserControls();
                LoadQuantityDetailsGrid(-1);

                btnAdd.Enabled = lcPrivAdd;
                btnModify.Enabled = lcPrivModify;
                btnDelete.Enabled = lcPrivDelete;
                btnPreview.Enabled = (lcPrivPrint || lcPrivView);
                btnSearch.Enabled = true;
                btnSave.Enabled = false;

                lcFormMode = ClsCommon.FormMode.None;

                stBar.lblMode.Text = "View";
                stBar.lblUserCode.Text = "";
                stBar.lblDateTime.Text = "";

                tabForm.LockPages();

                ActiveControl = cntSupplier;

                btnGenSerial.Visible = false;
                btnDuplicate.Visible = false;
                dteCrDate.Value = DateTime.Today.Date;
                StockID = 0;
                DuplicateItem = false;
                clsDynamicFields.intEntity = StockID;

                chkConsignmentItem.Checked = false;
                txtDiscPrice2.Text = txtDiscPrice3.Text = lblFullDescription.Text = "";

                cntFCCode.Code = ClsConfig.gbCurr2;
                rbCC.Text = ClsConfig.gbCurr2;
                rbVATStatus0.Enabled = false;

                if (ClsConfig.oHSystem != null && ((ClsConfig.oHSystem.Osh_Active ?? "N").ToUpper() == "N".ToUpper() || (ClsConfig.oHSystem.Osh_Active ?? "A").ToUpper() == "I"?.ToUpper())) this.FlowPanel6.Enabled = false;

            }
            catch (Exception)
            {
                throw;
            }
        }

        private void ManageUserControls()
        {
            try
            {
                FpnlSuppRef.Controls.Clear();

                flpStockUnits.Controls.Clear();
                if (lcFormMode != ClsCommon.FormMode.Modify)
                {
                    AddEmptyUnitRecord();
                }
                txtUnitsLogging.Text = "";
                treeViewUnits.Nodes.Clear();
            }
            catch (Exception ex)
            {

                ClsCommon.ShowException(StepName, ex);
            }
        }

        private void FrmStockItems_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (lcFormMode == ClsCommon.FormMode.Add)
                {
                    if (txtSerialNo.Text == "" && e.Control && e.KeyCode == Keys.S)
                    {
                        btnGenSerial_Click(btnGenSerial, new EventArgs());
                    }

                }
                if (tabForm.SelectedTab == tabStckUnits)
                {
                    if (e.Control && (e.KeyCode == Keys.Oemplus || e.KeyCode == Keys.Add))
                    {
                        if (btnAddUnit.Enabled && btnAddUnit.Focus()) { btnAddUnit_Click(btnAddUnit, new System.EventArgs()); }
                    }
                }

            }
            catch (Exception ex)
            {
                ClsCommon.ShowException(StepName, ex);
            }
        }

        private void FrmStockItems_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                this.AutoValidate = AutoValidate.Disable;

                if (!OpenedFromAnotherForm)
                {
                    if (ClsCommon.ExistNotEmptyControls(this) &&
                  ClsCommon.ShowQuestion(StepName, "Do You Want To Discard Changes ?") != DialogResult.Yes
                   )
                    {
                        e.Cancel = true;
                    }
                }

            }
            catch (Exception ex)
            {
                ClsCommon.ShowException(StepName, ex);
            }
        }

        #endregion


        #region Step Keys

        private void txtKeys_TextChanged(object sender, EventArgs e)
        {
            try
            {
                lastKeysCombination = "";

                SuspendLayout();

                if (lcFormMode != ClsCommon.FormMode.Add)
                {
                    ClsCommon.AutoClearControls(tabForm);
                    ManageUserControls();
                    LoadQuantityDetailsGrid(-1);
                }
            }
            catch (Exception ex)
            {
                ClsCommon.ShowException(StepName, ex);
            }
            finally
            {
                ResumeLayout();
            }
        }

        private void Supp_Cat_Ser_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                txtSerialNo.Text = txtSerialNo.Text.Trim().Replace("'", "");

                if (lcFormMode == ClsCommon.FormMode.Add && sender == txtSerialNo && txtSerialNo.Text == "")
                {
                    btnGenSerial_Click(btnGenSerial, new EventArgs());
                }

                if (cntSupplier.Code == "" || cntCategory.Code == "" || txtSerialNo.Text == "") { return; }
                if (ActiveControl == cntSupplier || ActiveControl == cntCategory || ActiveControl == txtSerialNo) { return; }
                if ((cntSupplier.Code + "|" + cntCategory.Code + "|" + txtSerialNo.Text)?.ToUpper() == lastKeysCombination?.ToUpper()) { return; }

                StockItemsController controller = new StockItemsController();
                StockItems clsStockItems = new StockItems();
                clsStockItems = controller.GetStockItems(0, cntSupplier.Code, cntCategory.Code, txtSerialNo.Text);

                

                if (clsStockItems == null)
                {
                    if (lcFormMode != ClsCommon.FormMode.Add)
                    {
                        ClsCommon.ShowNotification(this.ParentForm, "Invalid Code(s)");
                        e.Cancel = true;
                    }
                    else
                    {
                        lastKeysCombination = (cntSupplier.Code + "|" + cntCategory.Code + "|" + txtSerialNo.Text)?.ToUpper();

                    }
                }
                else
                {
                    if (lcFormMode == ClsCommon.FormMode.Add && !DuplicateItem)
                    {
                        ClsCommon.ShowNotification(this, "Code(s) Already Exist");
                        e.Cancel = true;
                    }
                    else
                    {
                        //Used in saved is allowed to see if it was Y or N
                        ThirdParty = clsStockItems.Stk_3rdParty;

                        lastKeysCombination = (cntSupplier.Code + "|" + cntCategory.Code + "|" + txtSerialNo.Text)?.ToUpper();
                        stBar.lblUserCode.Text = clsStockItems.Stk_Ustmp;
                        stBar.lblDateTime.Text = clsStockItems.Stk_Dstmp?.ToString("dd/MM/yyyy hh:mm:ss tt") ?? "Not Defined";
                        chkActive.Checked = clsStockItems.Stk_Status == "A";
                        if (DuplicateItem) txtSerialNo.Text = "";
                        ////Definition

                        //General
                        if (!DuplicateItem) StockID = clsStockItems.Stk_StkId;
                        if (clsStockItems.Stk_HoStkId != (int?)null && !DuplicateItem) txtHOCode.Text = clsStockItems.Stk_HoStkId.ToString();
                        if (!string.IsNullOrEmpty(clsStockItems.Stk_Desc)) txtDesc.Text = clsStockItems.Stk_Desc;
                        if (!string.IsNullOrEmpty(clsStockItems.Stk_DescPos)) txtPOSDesc.Text = clsStockItems.Stk_DescPos;
                        if (!string.IsNullOrEmpty(clsStockItems.Stk_Brand)) cntBrand.Code = clsStockItems.Stk_Brand;
                        if (!string.IsNullOrEmpty(clsStockItems.Stk_CrDate.ToString()) && !DuplicateItem) dteCrDate.Value = ((DateTime)clsStockItems.Stk_CrDate).ToString("dd/MM/yyyy");
                        switch (clsStockItems.Stk_ItmService)
                        {
                            case "I":
                                rbItmServiceI.Checked = true;
                                break;
                            case "S":
                                rbItmServiceS.Checked = true;
                                break;
                            default:
                                break;
                        }
                        if (!string.IsNullOrEmpty(clsStockItems.Stk_Brand)) cntBrand.Code = clsStockItems.Stk_Brand;
                        if (!string.IsNullOrEmpty(clsStockItems.Stk_GoodsLoc)) cntGoodsLoc.Code = clsStockItems.Stk_GoodsLoc;
                        if (clsStockItems.Stk_ValidityDays != (int?)null) txtValidityDays.NumericText = clsStockItems.Stk_ValidityDays;
                        //Flags
                        if (clsStockItems.Stk_Gift == "C") { rbBaseCurrencyCC.Checked = true; rbBaseCurrencyLC.Checked = false; } else { rbBaseCurrencyLC.Checked = true; rbBaseCurrencyCC.Checked = false; };
                        chkAssembly.Checked = clsStockItems.Stk_Assembly == "Y";
                        chkStopOrder.Checked = clsStockItems.Stk_StopOrder == "Y";
                        chk3rdParty.Checked = clsStockItems.Stk_3rdParty == "Y";

                        // uncomment this part when all transaction tables created --- LEA 
                        //if (StockItemsController.IsMovemented(StockID)) { chk3rdParty.Enabled = false; } else { chk3rdParty.Enabled = true; }


                        chkExpiryItem.Checked = clsStockItems.Stk_Expiry == "Y";
                        chkTransp.Checked = clsStockItems.Stk_Transp == "Y";
                        chkConsignmentItem.Checked = clsStockItems.Stk_Consignment == "Y";
                        chkDiscountExclude.Checked = StockDiscExclusionController.StockDiscExclusionExists(clsStockItems.Stk_StkId);
                        chkOnlineItem.Checked = StockOnlineController.StockOnlineExists(clsStockItems.Stk_StkId);

                        ////BARCODES
                        if (!string.IsNullOrEmpty(clsStockItems.Stk_Barcode1) && !DuplicateItem) cntBarcode1.txtBarcode1.Text = clsStockItems.Stk_Barcode1;
                        if (!string.IsNullOrEmpty(clsStockItems.Stk_Barcode2) && !DuplicateItem) cntBarcode2.txtBarcode1.Text = clsStockItems.Stk_Barcode2;
                        if (!string.IsNullOrEmpty(clsStockItems.Stk_AddOnRef)) txtAddonRef.Text = clsStockItems.Stk_AddOnRef;
                        if (clsStockItems.Stk_PLUnbr != (double?)null) txtPLU.Text = clsStockItems.Stk_PLUnbr.ToString();

                        ////Quantity
                        //SUM of WH Qty Details Record table 

                        ////Cost /Price
                        chkModSal.Checked = clsStockItems.Stk_ModSal == "Y";

                        //COSTS
                        if (!string.IsNullOrEmpty(clsStockItems.Stk_FCcurr)) cntFCCode.Code = clsStockItems.Stk_FCcurr;

                        if (!DuplicateItem)
                        {
                            if (clsStockItems.Stk_BBFCostLC != null) txtBBFCostLC.NumericText = clsStockItems.Stk_BBFCostLC;
                            if (clsStockItems.Stk_BBFCostCC != null) txtBBFCostCC.NumericText = clsStockItems.Stk_BBFCostCC;
                            if (clsStockItems.Stk_BBFCostFC != null) txtBBFCostFC.NumericText = clsStockItems.Stk_BBFCostFC;

                            if (clsStockItems.Stk_ActCostLL != null) txtActCostLC.NumericText = clsStockItems.Stk_ActCostLL;
                            if (clsStockItems.Stk_ActCostCC != null) txtActCostCC.NumericText = clsStockItems.Stk_ActCostCC;
                            if (clsStockItems.Stk_ActCostFC != null) txtActCostFC.NumericText = clsStockItems.Stk_ActCostFC;

                            if (clsStockItems.Stk_LstCostLL != null) txtLastCostLC.NumericText = clsStockItems.Stk_LstCostLL;
                            if (clsStockItems.Stk_LstCostCC != null) txtLastCostCC.NumericText = clsStockItems.Stk_LstCostCC;
                            if (clsStockItems.Stk_LstCostFC != null) txtLastCostFC.NumericText = clsStockItems.Stk_LstCostFC;
                        }
                        if (clsStockItems.stk_PotcostLL != null) txtPotCostLC.NumericText = clsStockItems.stk_PotcostLL;
                        if (clsStockItems.stk_PotCostCC != null) txtPotCostCC.NumericText = clsStockItems.stk_PotCostCC;
                        if (clsStockItems.stk_PotCostFC != null) txtPotCostFC.NumericText = clsStockItems.stk_PotCostFC;

                        //VAT Information
                        chkSubjectVAT.Checked = clsStockItems.Stk_SubjectVat == "Y";
                        switch (clsStockItems.Stk_VatType)
                        {
                            case "N":
                                rbVATTypeN.Checked = true;
                                break;
                            case "E":
                                rbVATTypeE.Checked = true;
                                rbVATTypeN.Checked = false;

                                break;
                            default:
                                break;
                        }
                        if (!string.IsNullOrEmpty(clsStockItems.Stk_VatCode)) cntVat1.Code = clsStockItems.Stk_VatCode;
                        switch (clsStockItems.Stk_VATStatus)
                        {
                            case "0":
                                rbVATStatus0.Checked = true;
                                break;
                            case "1":
                                rbVATStatus1.Checked = true;
                                rbVATStatus0.Checked = false;

                                break;
                            case "2":
                                rbVATStatus2.Checked = true;
                                rbVATStatus0.Checked = false;

                                break;
                            case "3":
                                rbVATStatus3.Checked = true;
                                rbVATStatus0.Checked = false;

                                break;
                            default:
                                break;
                        }

                        //Sales Prices
                        //if (!string.IsNullOrEmpty(clsStockItems.Stk_FCcurr)) txtFCurrSales.Text = clsStockItems.Stk_FCcurr; jad

                        if (clsStockItems.Stk_1SlPriceLC != null) txtPrice1LL.NumericText = clsStockItems.Stk_1SlPriceLC;
                        if (clsStockItems.Stk_1SlPriceCC != null) txtPrice1CC.NumericText = clsStockItems.Stk_1SlPriceCC;

                        if (clsStockItems.Stk_2SLPriceLC != null) txtPrice2LL.NumericText = clsStockItems.Stk_2SLPriceLC;
                        if (clsStockItems.Stk_2SLPriceCC != null) txtPrice2CC.NumericText = clsStockItems.Stk_2SLPriceCC;
                        if (clsStockItems.Stk_Qty1stPrice != null) txtDiscPrice2.NumericText = clsStockItems.Stk_Qty1stPrice;

                        if (clsStockItems.Stk_3SLPriceLC != null) txtPrice3LL.NumericText = clsStockItems.Stk_3SLPriceLC;
                        if (clsStockItems.Stk_3SLPriceCC != null) txtPrice3CC.NumericText = clsStockItems.Stk_3SLPriceCC;
                        if (clsStockItems.Stk_Qty2stPrice != null) txtDiscPrice3.NumericText = clsStockItems.Stk_Qty2stPrice;

                        if (clsStockItems.Stk_SLCoef != null) txtSalesCoefVal.NumericText = clsStockItems.Stk_SLCoef;
                        txtSalesCoefVal.LastText = txtSalesCoefVal.Text;
                        //switch (clsStockItems.Stk_LStatCoef)
                        //{
                        //    case "C":
                        //        rbLstStsCoefValC.Checked = true;
                        //        rbLstStsCoefValU.Checked = false;
                        //        break;
                        //    case "U":
                        //        rbLstStsCoefValU.Checked = true;
                        //        rbLstStsCoefValC.Checked = false;
                        //        break;
                        //    default:
                        //        break;
                        //}
                        if (!string.IsNullOrEmpty(clsStockItems.Stk_UserCoef)) txtUserCode.Text = clsStockItems.Stk_UserCoef;
                        if (clsStockItems.Stk_DiscWh != null) txtDiscWH.NumericText = clsStockItems.Stk_DiscWh;
                        if (clsStockItems.Stk_DiscRetail != null) txtDiscRetail.NumericText = clsStockItems.Stk_DiscRetail;
                        if (clsStockItems.stk_potchangeDate != null) txtPotChangeDate.Text = clsStockItems.stk_potchangeDate.ToString();

                        if (clsStockItems.Stk_BBFDate1 != null)  txtBBFDate.Text = clsStockItems.Stk_BBFDate1.ToString() ;
                        if (clsStockItems.Stk_BBFQty1 != null) txtBBFQty.Text = clsStockItems.Stk_BBFQty1.ToString();
                        if (clsStockItems.Stk_AvlbQty1 != null)  txtAvailableQty.Text = clsStockItems.Stk_AvlbQty1.ToString();
                        if (clsStockItems.Stk_Return1 != null)  txtQtyToReturn.Text = clsStockItems.Stk_Return1.ToString(); 

                        ////File price
                        if (clsStockItems.stk_FilePrice != null && clsStockItems.stk_FilePrice.Sfp_stkId != null)
                        {
                            //Potential Cost
                            if (clsStockItems.stk_FilePrice.Sfp_PotCurr != null) { rbLC.Checked = clsStockItems.stk_FilePrice.Sfp_PotCurr == ClsConfig.gbCurr1; }// rbCC.Checked = clsStockItems.stk_FilePrice.Sfp_PotCurr != ClsConfig.gbCurr1; };
                            if (clsStockItems.stk_FilePrice.Sfp_PotUnitPrice != null) txtUnitPrice.NumericText = clsStockItems.stk_FilePrice.Sfp_PotUnitPrice;
                            if (clsStockItems.stk_FilePrice.Sfp_PotItemDiscount != null) txtUnitDiscPotCost.NumericText = clsStockItems.stk_FilePrice.Sfp_PotItemDiscount;
                            if (clsStockItems.stk_FilePrice.Sfp_PotInvDiscount != null) txtAddInvDisc.NumericText = clsStockItems.stk_FilePrice.Sfp_PotInvDiscount;
                            if (clsStockItems.stk_FilePrice.Sfp_PotPurQty != null) txtPurchaseQty.NumericText = clsStockItems.stk_FilePrice.Sfp_PotPurQty;
                            if (clsStockItems.stk_FilePrice.Sfp_PotPromQty != null) txtPromotionQty.NumericText = clsStockItems.stk_FilePrice.Sfp_PotPromQty;
                            if (clsStockItems.stk_FilePrice.Sfp_PotFreeQty != null) txtFreeQty.NumericText = clsStockItems.stk_FilePrice.Sfp_PotFreeQty;
                            if (clsStockItems.stk_FilePrice.Sfp_PotExtraQty != null) txtExtraQty.NumericText = clsStockItems.stk_FilePrice.Sfp_PotExtraQty;
                            if (clsStockItems.stk_FilePrice.Sfp_PotContractQty != null) txtContractQty.NumericText = clsStockItems.stk_FilePrice.Sfp_PotContractQty;
                            if (clsStockItems.stk_FilePrice.Sfp_PotRemarks != null) txtRemarksFP.Text = clsStockItems.stk_FilePrice.Sfp_PotRemarks;
                            chkFollow.Checked = clsStockItems.stk_FilePrice.Sfp_PotFollowInvDisc == "Y";

                            //Discount Flags
                            chkUseItmDisc.Checked = clsStockItems.stk_FilePrice.Sfp_PotUseItmDisc == "Y";
                            chkUseInvDisc.Checked = clsStockItems.stk_FilePrice.Sfp_PotUseInvDisc == "Y";
                            chkUsePromQty.Checked = clsStockItems.stk_FilePrice.Sfp_PotUsePromQty == "Y";

                            //Params to use in Last Cost
                            switch (clsStockItems.stk_FilePrice.Sfp_LastCostParam)
                            {
                                case "1":
                                    rbParamsLastCost1.Checked = true;
                                    rbParamsLastCost2.Checked = false;
                                    rbParamsLastCost3.Checked = false;
                                    break;
                                case "2":
                                    rbParamsLastCost2.Checked = true;
                                    rbParamsLastCost1.Checked = false;
                                    rbParamsLastCost3.Checked = false;
                                    break;
                                case "3":
                                    rbParamsLastCost3.Checked = true;
                                    rbParamsLastCost1.Checked = false;
                                    rbParamsLastCost2.Checked = false;
                                    break;

                                default:
                                    break;
                            }

                            //LAST COST AREA , UPDATED FROM LAST PURCHASE INVOICE WHILE UPDATE
                            if (clsStockItems.stk_FilePrice.Sfp_InvCurr != null) cntCurrency2.Code = clsStockItems.stk_FilePrice.Sfp_InvCurr;
                            if (clsStockItems.stk_FilePrice.Sfp_LCOrginalCost != null) txtOriginalCost.NumericText = clsStockItems.stk_FilePrice.Sfp_LCOrginalCost;
                            if (clsStockItems.stk_FilePrice.Sfp_PINo != null) txtPIIntNo.Text = clsStockItems.stk_FilePrice.Sfp_PINo;
                            if (clsStockItems.stk_FilePrice.Sfp_SupInvNoOrdNo != null) txtSuppInvOrderNo.Text = clsStockItems.stk_FilePrice.Sfp_SupInvNoOrdNo;
                            if (clsStockItems.stk_FilePrice.Sfp_LCItmDiscount != null) txtUnitDiscLastCost.NumericText = clsStockItems.stk_FilePrice.Sfp_LCItmDiscount;
                            if (clsStockItems.stk_FilePrice.Sfp_LCInvDiscount != null) txtAddDiscLastCost.NumericText = clsStockItems.stk_FilePrice.Sfp_LCInvDiscount;
                            if (clsStockItems.stk_FilePrice.Sfp_LCPurQty != null) txtPurchaseQtyLastCost.NumericText = clsStockItems.stk_FilePrice.Sfp_LCPurQty;
                            if (clsStockItems.stk_FilePrice.Sfp_LCPromQty != null) txtPromotionQtyLastCost.NumericText = clsStockItems.stk_FilePrice.Sfp_LCPromQty;
                            if (clsStockItems.stk_FilePrice.Sfp_LCFreeQty != null) txtFreeQtyLastCost.NumericText = clsStockItems.stk_FilePrice.Sfp_LCFreeQty;
                            if (clsStockItems.stk_FilePrice.Sfp_LCExtraQty != null) txtExtraQtyLastCost.NumericText = clsStockItems.stk_FilePrice.Sfp_LCExtraQty;
                            if (clsStockItems.stk_FilePrice.Sfp_ContractQty != null) txtContractQtyLastCost.NumericText = clsStockItems.stk_FilePrice.Sfp_ContractQty;

                            if (clsStockItems.stk_FilePrice.Sfp_Image != null && !DuplicateItem) imgItmPhoto.pctPicture.AddImageFromByteArray(clsStockItems.stk_FilePrice.Sfp_Image);

                        }

                        ////// YTD
                        //if (clsStockItems.Stk_YTDQtyIn != null) txtYTDQtyIN.NumericText = clsStockItems.Stk_YTDQtyIn;
                        //if (clsStockItems.Stk_YTDQtyOut != null) txtYTDQtyOUT.NumericText = clsStockItems.Stk_YTDQtyOut;
                        //if (clsStockItems.Stk_YTDAmtInLL != null) txtYTDLLIn.NumericText = clsStockItems.Stk_YTDAmtInLL;
                        //if (clsStockItems.Stk_YTDAmtOutLL != null) txtYTDLLOut.NumericText = clsStockItems.Stk_YTDAmtOutLL;
                        //if (clsStockItems.Stk_YTDAmtInUSD != null) txtYTDUsdIn.NumericText = clsStockItems.Stk_YTDAmtInUSD;
                        //if (clsStockItems.Stk_YTDAmtOutUSD != null) txtYTDUsdOut.NumericText = clsStockItems.Stk_YTDAmtOutUSD;

                        //// O/H Info
                        if (clsStockItems.Stk_OHStockAdd != null && clsStockItems.Stk_OHStockAdd.Soh_Stkid != null)
                        {
                            if (clsStockItems.Stk_OHStockAdd.Soh_CustPer != null) txtCustomsPerc.NumericText = clsStockItems.Stk_OHStockAdd.Soh_CustPer;
                            if (clsStockItems.Stk_OHStockAdd.Soh_FixedOHPer != null) txtOHPerc.NumericText = clsStockItems.Stk_OHStockAdd.Soh_FixedOHPer;
                            if (clsStockItems.Stk_OHStockAdd.Soh_AppCostCoef != null) txtCostCoef.NumericText = clsStockItems.Stk_OHStockAdd.Soh_AppCostCoef;
                            if (!string.IsNullOrEmpty(clsStockItems.Stk_OHStockAdd.Soh_Remarks)) txtRemarks.Text = clsStockItems.Stk_OHStockAdd.Soh_Remarks;
                        }


                        ////Supp Ref
                        if (clsStockItems.Stk_AddenSupplierList != null && clsStockItems.Stk_AddenSupplierList.Count > 0 && !DuplicateItem)
                        {
                            foreach (AddenSupplier Ref in clsStockItems.Stk_AddenSupplierList)
                            {
                                cntSupplierReference cntRef = new cntSupplierReference();
                                cntRef.Size = new Size(750, 20);
                                cntRef.cntSuppliers1.Code = Ref.Ads_Supplier;
                                cntRef.chkReorder.Checked = Ref.Ads_AllowReOrder == "Y";
                                if (!string.IsNullOrEmpty(Ref.Ads_SuppRef)) cntRef.txtReference.Text = Ref.Ads_SuppRef;
                                FpnlSuppRef.Controls.Add(cntRef);
                            }

                        }

                        ////Stock Unit
                        flpStockUnits.Controls.Clear();
                        if (clsStockItems.stk_StockUnitList != null && clsStockItems.stk_StockUnitList.Count > 0)
                        {

                            foreach (StockUnit SU in clsStockItems.stk_StockUnitList)
                            {
                                cntStockUnit CntStkUnt = new cntStockUnit();

                                CntStkUnt.cntStockUnitHeader = cntStockUnitHeaderSI;

                                //conditions when modify mode for manipulation on StockUnit
                                CntStkUnt.btnDeleteUnit.Visible = false;
                                CntStkUnt.cntUnit.txtCode.ReadOnly = true;
                                CntStkUnt.cntParentUnit.txtCode.ReadOnly = true;

                                CntStkUnt.cntUnit.Code = SU.SUnit_Unit;
                                CntStkUnt.cntParentUnit.Code = SU.SUnit_ParentUnit;
                                CntStkUnt.chkBaseUnit.Checked = clsStockItems.Stk_UnitCode == SU.SUnit_Unit;
                                CntStkUnt.chkDefaultPIUnit.Checked = clsStockItems.Stk_Packing == SU.SUnit_Unit;
                                if (SU.SUnit_Packing != null) CntStkUnt.txtPacking.NumericText = SU.SUnit_Packing;
                                if (SU.SUnit_PackingBasic != null) CntStkUnt.txtPackingBasic.NumericText = SU.SUnit_PackingBasic;
                                if (SU.SUnit_SpecDesc != null) CntStkUnt.txtDescription.Text = SU.SUnit_SpecDesc;
                                if (SU.SUnit_Discount != null) CntStkUnt.txtDiscount.NumericText = SU.SUnit_Discount;
                                if (SU.SUnit_1SlPriceUSD != null)
                                {
                                    CntStkUnt.txtPrice1CC.NumericText = SU.SUnit_1SlPriceUSD;
                                    CntStkUnt.txtPrice1CC.LastText = CntStkUnt.txtPrice1CC.Text;
                                }
                                if (SU.SUnit_1SlPriceLC != null)
                                {
                                    CntStkUnt.txtPrice1LC.NumericText = SU.SUnit_1SlPriceLC;
                                    CntStkUnt.txtPrice1LC.LastText = CntStkUnt.txtPrice1LC.Text;

                                }
                                if (SU.SUnit_2SLPriceUSD != null)
                                {
                                    CntStkUnt.txtPrice3CC.NumericText = SU.SUnit_2SLPriceUSD;
                                    CntStkUnt.txtPrice3CC.LastText = CntStkUnt.txtPrice3CC.Text;

                                }
                                if (SU.SUnit_2SLPriceLC != null)
                                {
                                    CntStkUnt.txtPrice3LC.NumericText = SU.SUnit_2SLPriceLC;
                                    CntStkUnt.txtPrice3LC.LastText = CntStkUnt.txtPrice3LC.Text;

                                }
                                if (SU.SUnit_Qty1stPrice != null)
                                {
                                    CntStkUnt.txtP1P3Disc.NumericText = SU.SUnit_Qty1stPrice;
                                    CntStkUnt.txtP1P3Disc.LastText = CntStkUnt.txtP1P3Disc.Text;

                                }
                                if (SU.SUnit_3SLPriceUSD != null)
                                {
                                    CntStkUnt.txtPrice2CC.NumericText = SU.SUnit_3SLPriceUSD;
                                    CntStkUnt.txtPrice2CC.LastText = CntStkUnt.txtPrice2CC.Text;

                                }
                                if (SU.SUnit_3SLPriceLC != null)
                                {
                                    CntStkUnt.txtPrice2LC.NumericText = SU.SUnit_3SLPriceLC;
                                    CntStkUnt.txtPrice2LC.LastText = CntStkUnt.txtPrice2LC.Text;

                                }
                                if (SU.SUnit_Qty2stPrice != null)
                                {
                                    CntStkUnt.txtP1P2Disc.NumericText = SU.SUnit_Qty2stPrice;
                                    CntStkUnt.txtP1P2Disc.LastText = CntStkUnt.txtP1P2Disc.Text;
                                }
                                if (SU.SUnit_Volume != null) CntStkUnt.txtVolume.NumericText = SU.SUnit_Volume;
                                if (SU.SUnit_Weight != null) CntStkUnt.txtWeight.NumericText = SU.SUnit_Weight;

                                CntStkUnt.chkActiveUnit.Checked = SU.SUnit_Active == "Y";

                                foreach (BarcodeSyn BarCodeSyn in SU.Stk_BarcodeList)
                                {

                                    if (BarCodeSyn.BSy_UnitCode == SU.SUnit_Unit)
                                    {
                                        cntBarcode CntBarcode = new cntBarcode();
                                        CntBarcode.AllowRemove = true;
                                        CntBarcode.Size = new Size(250, 25);
                                        CntBarcode.txtBarcode1.Text = BarCodeSyn.Bsy_Barcode;
                                        CntStkUnt.flpSynBarcode.Controls.Add(CntBarcode);
                                    }
                                }
                                CntStkUnt.chkBaseUnit.AutoCheck = false;
                                flpStockUnits.Controls.Add(CntStkUnt);

                                AddStockUnitHandlers(CntStkUnt);

                            }

                        }

                        if (LinkStockDeptController.StockDeptExists(clsStockItems.Stk_StkId) && clsStockItems.Stk_LinkStockDept != null)
                        {
                            var link = clsStockItems.Stk_LinkStockDept;

                            if (link.lsd_SG4Recid.HasValue)
                            {
                                SubGroup4Controller sG4Controller = new SubGroup4Controller();
                                SubGroup4 sG4 = sG4Controller.GetSubGroup4(link.lsd_SG4Recid.Value);
                                cntDepGrpSub.cntSubGroup4.Code = sG4 != null ? sG4.SG4_Code : "";
                            }
                            else if (link.lsd_SG3RecID.HasValue)
                            {
                                SubGroup3Controller sG3Controller = new SubGroup3Controller();
                                SubGroup3 sG3 = sG3Controller.GetSubGroup3(link.lsd_SG3RecID.Value);
                                cntDepGrpSub.cntSubGroup3.Code = sG3 != null ? sG3.SG3_Code : "";
                            }
                            else if (link.lsd_SG2RecID.HasValue)
                            {
                                SubGroup2Controller sG2Controller = new SubGroup2Controller();
                                SubGroup2 sG2 = sG2Controller.GetSubGroup2(link.lsd_SG2RecID.Value);
                                cntDepGrpSub.cntSubGroup2.Code = sG2 != null ? sG2.SG2_Code : "";
                            }
                            else if (link.lsd_SG1RecID.HasValue)
                            {
                                SubGroup1Controller sG1Controller = new SubGroup1Controller();
                                SubGroup1 sG1 = sG1Controller.GetSubGroup1(link.lsd_SG1RecID.Value);
                                cntDepGrpSub.cntSubGroup1.Code = sG1 != null ? sG1.SG1_Code : "";
                            }
                            else if (link.lsd_GrpRecID.HasValue)
                            {
                                GroupController grpController = new GroupController();
                                Group grp = grpController.GetGroup(link.lsd_GrpRecID.Value);
                                cntDepGrpSub.cntGroup.Code = grp != null ? grp.grp_Code : "";
                            }
                            else if (link.lsd_DeptRecID.HasValue)
                            {
                                DepartmentController dptController = new DepartmentController();
                                Department dpt = dptController.GetDepartment(link.lsd_DeptRecID.Value);
                                cntDepGrpSub.cntDepartment.Code = dpt != null ? dpt.dep_Code : "";
                            }
                            else
                            {
                                cntDepGrpSub.cntDepartment.Code = "";
                            }

                            AvlbQty = Convert.ToDouble(clsStockItems.Stk_AvlbQty1);
                        }
                        if (!DuplicateItem)
                        {
                            LoadQuantityDetailsGrid(StockID);
                            clsDynamicFields.intEntity = StockID;
                            clsDynamicFields.LoadDynamicFields();

                        }


                    }
                }

            }
            catch (Exception ex)
            {
                ClsCommon.ShowException(StepName, ex);
            }

        }

        #endregion


        #region Main Buttons

        private void btnAddModifyDelete_Click(object sender, EventArgs e)
        {
            try
            {

                ClsCommon.AutoClearControls(this);
                ManageUserControls();
                LoadQuantityDetailsGrid(-1);

                stBar.lblMode.Text = ((WinApplications.Controls.ButtonMain)sender).Text;

                if (sender == btnAdd)
                {
                    lcFormMode = ClsCommon.FormMode.Add;
                    stBar.lblUserCode.Text = ClsConfig.LoginUser;
                    stBar.lblDateTime.Text = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt");
                    tabForm.UnLockPages();
                    chkSubjectVAT.Enabled = true;
                    ClsCommon.MakeReadOnly(this.CostPriceTab, false, txtBBFCostLC, txtBBFCostCC, txtBBFCostFC,
                        txtActCostLC, txtActCostCC, txtActCostFC, txtLastCostLC, txtLastCostCC, txtLastCostFC,
                        txtPotCostLC, txtPotCostCC, txtPotCostFC);
                    ClsCommon.MakeReadOnly(this.StockFilePriceTab, false);
                    //ClsCommon.MakeReadOnly(this.YTDTab, true);
                    chkFollow.Checked = true;

                    chkUseItmDisc.Checked = ClsConfig.locationDirectives.Ld_UseItemDiscount.ToUpper() == "Y" ;
                    chkUsePromQty.Checked = ClsConfig.locationDirectives.Ld_UsePromotionQuantity.ToUpper() == "Y" ;
                    chkUseInvDisc.Checked = ClsConfig.locationDirectives.Ld_UseInvoiceDiscount.ToUpper() == "Y";

                    chkConsignmentItem.Checked = false;

                    txtPrice1LL.Enabled = txtPrice1CC.Enabled =
                        txtPrice2CC.Enabled = txtPrice2LL.Enabled =
                        txtPrice3CC.Enabled = txtPrice3LL.Enabled =
                        txtDiscPrice2.Enabled = txtDiscPrice3.Enabled = true;

                    dteCrDate.Value = DateTime.Today.Date;

                    txtSalesCoefVal.Enabled = true;

                    rbVATStatus1.Checked = true;
                    cntVat1.txtCode.Text = ClsConfig.locationDirectives.Ld_VatCode;
                    cntVat1.txtCode_Validating(cntVat1.txtCode.Text, new System.ComponentModel.CancelEventArgs());
                    
                    //pnlCurrency_Validating(pnlCurrency, new System.ComponentModel.CancelEventArgs());

                    rbParamsLastCost1.Checked = ClsConfig.locationDirectives.Ld_ParametersToUseInLastCost == "1";
                    rbParamsLastCost2.Checked = ClsConfig.locationDirectives.Ld_ParametersToUseInLastCost == "2";
                    rbParamsLastCost3.Checked = ClsConfig.locationDirectives.Ld_ParametersToUseInLastCost == "3";

                    cntFCCode.Code = ClsConfig.gbCurr2;
                    rbCC.Text = ClsConfig.gbCurr2;
                }
                else if (sender == btnModify)
                {
                    lcFormMode = ClsCommon.FormMode.Modify;
                    tabForm.UnLockPages();
                    chkSubjectVAT.Enabled = false;
                    ClsCommon.MakeReadOnly(this.CostPriceTab, true, chkModSal);
                    ClsCommon.MakeReadOnly(this.OHInfoTab, true, txtCostCoef, txtRemarks);
                    ClsCommon.MakeReadOnly(this.StockFilePriceTab, true, txtRemarksFP);
                    //ClsCommon.MakeReadOnly(this.YTDTab, true);

                    PnlVatType.Enabled = false;
                    PnlVatStatus.Enabled = false;

                    chkConsignmentItem.Checked = false;

                    txtPrice1LL.Enabled = txtPrice1CC.Enabled =
                     txtPrice2CC.Enabled = txtPrice2LL.Enabled =
                     txtPrice3CC.Enabled = txtPrice3LL.Enabled =
                     txtDiscPrice2.Enabled = txtDiscPrice3.Enabled = txtSalesCoefVal.Enabled = false;

                    if (OpenedFromAnotherForm)
                    {
                        btnAdd.Enabled = false;
                        btnDelete.Enabled = false;
                        btnSearch.Enabled = false;
                        btnCancel.Enabled = false;
                        btnUndo.Enabled = false;
                        btnPreview.Enabled = false;
                        btnSave.Enabled = true;
                    }

                }
                else if (sender == btnDelete)
                {
                    lcFormMode = ClsCommon.FormMode.Delete;
                    tabForm.LockPages();
                }

                btnAdd.Enabled = false;
                btnModify.Enabled = false;
                btnDelete.Enabled = false;
                btnSave.Enabled = true;
                btnDuplicate.Visible = lcFormMode == ClsCommon.FormMode.Add;

                dteCrDate.Value = DateTime.Today.Date;
                btnGenSerial.Visible = lcFormMode == ClsCommon.FormMode.Add;

                ActiveControl = cntSupplier.txtCode;

            }
            catch (Exception ex)
            {
                ClsCommon.ShowException(StepName, ex);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            try
            {

                InitForm();

            }
            catch (Exception ex)
            {
                ClsCommon.ShowException(StepName, ex);
            }
        }

        private void btnUndo_Click(object sender, EventArgs e)
        {
            try
            {

                txtHOCode.LastText = "";
                if (lcFormMode == ClsCommon.FormMode.Add) { txtHOCode.Text = ""; }
                //txtCode_Validating(txtHOCode, new System.ComponentModel.CancelEventArgs());

                ActiveControl = txtHOCode;

            }
            catch (Exception ex)
            {
                ClsCommon.ShowException(StepName, ex);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {

                object objReturn = null;

                if (StockSearch.LoadStockSearch(this,
                                                 FrmStockSearch.StockSearchType.SearchOnly,
                                                 FrmStockSearch.StockSearchReturnValue.SSrv_DataRow,
                                                 ref objReturn,
                                                 null) == DialogResult.OK)
                {
                    if (lcFormMode != ClsCommon.FormMode.Add || DuplicateItem)
                    {

                        if (objReturn != null && objReturn is DataRowView)
                        {
                            cntSupplier.Code = ((DataRowView)objReturn)["StockHiddenStk_Supplier"].ToString();
                            cntCategory.Code = ((DataRowView)objReturn)["StockHiddenStk_Category"].ToString();
                            txtSerialNo.Text = ((DataRowView)objReturn)["StockHiddenSerial_No"].ToString();

                            Supp_Cat_Ser_Validating(txtSerialNo, new CancelEventArgs());

                        }

                    }

                }

            }
            catch (Exception ex)
            {
                ClsCommon.ShowException(StepName, ex);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (lcFormMode == ClsCommon.FormMode.None) { return; }
                //if (txtHOCode.Text.Trim() == "") { return; }

                if (SaveIsAllowed())
                {
                    SaveData();

                    ClsCommon.ShowNotification(this, "Saved Successfully");

                    InitForm();
                }
            }
            catch (Exception ex)
            {
                ClsCommon.ShowException(StepName, ex);
            }
        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            try
            {
                StockItemsController controller = new StockItemsController();
                string query = controller.GetStockItemsQuery(ClsCom.QueryType.Readable);

                using (frmPreview frmPreview = new frmPreview())
                {
                    frmPreview.ShowDialog(ClsConfig.SQLConnectionString, Text, query);
                }
            }
            catch (Exception ex)
            {
                ClsCommon.ShowException(StepName, ex);
            }
        }

        private void btnDuplicate_Click(object sender, EventArgs e)
        {
            try
            {
                DuplicateItem = true;
                btnSearch_Click(sender, new EventArgs());
            }
            catch (Exception ex)
            {
                ClsCommon.ShowException(StepName, ex);
            }
        }
        #endregion


        #region Common Buttons

        private void btnAddSuppRef_Click(object sender, EventArgs e)
        {
            try
            {
                cntSupplierReference newCntSuppRef = new cntSupplierReference();
                //int ControlCount = this.FpnlSuppRef.Controls.Count + 1;
                //newCntSuppRef.Name = "cntSuppRef" + ControlCount.ToString();
                newCntSuppRef.Size = new Size(750, 20);
                FpnlSuppRef.Controls.Add(newCntSuppRef);

                if (FpnlSuppRef.Controls.Count == 1) newCntSuppRef.cntSuppliers1.Code = cntSupplier.Code;

                newCntSuppRef.cntSuppliers1.txtCode.Validating += new CancelEventHandler(this.txtSuppRef_Validating);
                newCntSuppRef.txtReference.Validating += new CancelEventHandler(this.txtReference_Validating);

            }
            catch (Exception ex)
            {

                ClsCommon.ShowException(StepName, ex);
            }
        }

        private void btnAddUnit_Click(object sender, EventArgs e)
        {
            try
            {

                AddEmptyUnitRecord();

            }
            catch (Exception ex)
            {

                ClsCommon.ShowException(StepName, ex);
            }

        }

        private void btnGenSerial_Click(object sender, EventArgs e)
        {
            try
            {
                txtSerialNo.Text = StockItemsController.GetMaxSerial(!string.IsNullOrEmpty(cntSupplier.Code) ? cntSupplier.Code : null, !string.IsNullOrEmpty(cntCategory.Code) ? cntCategory.Code : null).ToString();
            }
            catch (Exception ex)
            {

                ClsCommon.ShowException(StepName, ex);
            }
        }

        private void btnGeneratePLU_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(txtPLU.Text) && Convert.ToDouble(txtPLU.Text) != 0) { return; }

                txtPLU.Text = StockItemsController.GeneratePLU().ToString();
            }
            catch (Exception ex)
            {
                ClsCommon.ShowException(StepName, ex);
            }
        }

        #endregion


        #region Manipulation

        private void chkActive_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (lcFormMode == ClsCommon.FormMode.Modify && !chkActive.Checked && AvlbQty > 0)
                {
                    chkActive.Checked = true;
                    ClsCommon.ShowNotification(this, "Cannot Deactivate An Item With Available Quantity!");
                }
            }
            catch (Exception ex)
            {

                ClsCommon.ShowException(StepName, ex);
            }
        }

        private void AddEmptyUnitRecord()
        {
            try
            {

                cntStockUnit newCntStkUnt = new cntStockUnit();

                newCntStkUnt.cntStockUnitHeader = cntStockUnitHeaderSI;
                newCntStkUnt.chkBaseUnit.Checked = !BaseUnitDefined();
                newCntStkUnt.chkDefaultPIUnit.Checked = !DefaultPIUnitDefined();
                newCntStkUnt.txtPacking.NumericText = 1;
                newCntStkUnt.txtPackingBasic.NumericText = 1;
                newCntStkUnt.chkActiveUnit.Checked = true;

                if (newCntStkUnt.chkBaseUnit.Checked)
                {
                    StockItemsController stkItms = new StockItemsController();
                    newCntStkUnt.cntUnit.txtCode.Text = newCntStkUnt.cntParentUnit.txtCode.Text = stkItms.GetBaseUnit();
                }

                flpStockUnits.Controls.Add(newCntStkUnt);

                AddStockUnitHandlers(newCntStkUnt);

            }
            catch (Exception)
            {
                throw;
            }
        }

        private void chkBaseUnit_CheckedChanged(object sender, EventArgs e)
        {

            try
            {
                CheckBox senderCheckbox = sender as CheckBox;

                if (CheckChecked("chkBaseUnit"))
                {
                    if (senderCheckbox != null && senderCheckbox.Checked)
                    {
                        foreach (var cntstockUnit in flpStockUnits.Controls.OfType<cntStockUnit>())
                        {
                            if (cntstockUnit.chkBaseUnit != senderCheckbox && cntstockUnit.chkBaseUnit.Checked)
                            {
                                cntstockUnit.chkBaseUnit.Checked = false;
                            }
                        }
                    }
                };
            }
            catch (Exception)
            {

                throw;
            }
         
        }

        private void chkDefaultPIUnit_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (CheckChecked("chkDefaultPIUnit"))
                {
                    CheckBox senderCheckbox = sender as CheckBox;
                    if (senderCheckbox != null && senderCheckbox.Checked)
                    {
                        foreach (var cntstockUnit in flpStockUnits.Controls.OfType<cntStockUnit>())
                        {
                            if (cntstockUnit.chkDefaultPIUnit != senderCheckbox && cntstockUnit.chkDefaultPIUnit.Checked)
                            {
                                cntstockUnit.chkDefaultPIUnit.Checked = false;
                            }
                        }
                    }
                };
            }
            catch (Exception)
            {

                throw;
            }
           
        }

        private void txtReference_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                var senderTextBox = (TextBox)sender;
                string senderText = senderTextBox.Text;

                if (string.IsNullOrEmpty(senderText))
                    return;

                foreach (cntSupplierReference cnt in FpnlSuppRef.Controls)
                {
                    if (cnt.txtReference == sender) continue;

                    if (!string.IsNullOrEmpty(cnt.cntSuppliers1.Code) &&
                        !string.IsNullOrEmpty(cnt.txtReference.Text) &&
                        cnt.txtReference.Text == senderText)
                    {
                        ClsCommon.ShowNotification(this, "Duplicate Reference.");
                        e.Cancel = true;
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                ClsCommon.ShowException(StepName, ex);
            }
        }

        private void txtSuppRef_Validating(object sender, CancelEventArgs e)
        {
            try
            {
              
                TextBox currentTextBox = sender as TextBox;            
                string currentCode = currentTextBox.Text?.Trim();
                if (string.IsNullOrEmpty(currentCode))
                    return;             

                List<string> supplierCodes = FpnlSuppRef.Controls
                    .OfType<cntSupplierReference>()
                    .Select(c => c.cntSuppliers1.Code)
                    .Where(code => !string.IsNullOrWhiteSpace(code))
                    .ToList();

                bool isDuplicate = supplierCodes.Count(code => string.Equals(code, currentCode, StringComparison.OrdinalIgnoreCase)) > 1;

                if (isDuplicate)
                {
                    currentTextBox.Text = string.Empty;
                    e.Cancel = true;
                    MessageBox.Show($"This supplier code already exists.");
                    
                }

            }
            catch (Exception ex)
            {
                ClsCommon.ShowException(StepName, ex);
            }
        }

        private void chkActiveUnit_CheckedChanged(object sender, EventArgs e)
        {

            CheckBox chkActive = sender as CheckBox;
            cntStockUnit cntStockUnit = chkActive.Parent as cntStockUnit;

            if (cntStockUnit != null && chkActive == cntStockUnit.chkActiveUnit)
            {
                if (cntStockUnit.chkBaseUnit.Checked && !chkActive.Checked)
                {
                    chkActive.Checked = true;
                }
            }
        }

        private bool CheckChecked(string checkBoxName)
        {
            try
            {
                switch (checkBoxName)
                {
                    case "chkBaseUnit":
                        return (Array.FindAll(flpStockUnits.Controls.Cast<Control>().OfType<cntStockUnit>().ToArray(),
                                cnt => !cnt.Delete && cnt.chkBaseUnit.Checked).Length > 1);
                        break;
                    case "chkDefaultPIUnit":
                        return (Array.FindAll(flpStockUnits.Controls.Cast<Control>().OfType<cntStockUnit>().ToArray(),
                                cnt => !cnt.Delete && cnt.chkDefaultPIUnit.Checked).Length > 1);
                        break;
                    default:
                        return false;
                        break;
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        private void flpStockUnits_ControlAddedRemoved(object sender, ControlEventArgs e)
        {
            try
            {

                txtUnitsLogging.Text = ValidateUnits();

            }
            catch (Exception ex)
            {
                ClsCommon.ShowException(StepName, ex);
            }
        }

        private bool BaseUnitDefined()
        {
            return (Array.FindAll(flpStockUnits.Controls.Cast<Control>().OfType<cntStockUnit>().ToArray(),
                         cnt => !cnt.Delete && cnt.chkBaseUnit.Checked).Length > 0);
        }

        private bool DefaultPIUnitDefined()
        {
            return (Array.FindAll(flpStockUnits.Controls.Cast<Control>().OfType<cntStockUnit>().ToArray(),
                         cnt => !cnt.Delete && cnt.chkDefaultPIUnit.Checked).Length > 0);
        }

        private void VAT_Validating(object sender, CancelEventArgs e)
                {
            try
            {
                if (sender == chkSubjectVAT
                    || sender == rbVATStatus0
                    || sender == rbVATStatus1
                    || sender == rbVATStatus2
                    || sender == rbVATStatus3
                    || sender == cntVat1
                    || sender == rbVATTypeE
                    || sender == rbVATTypeN)
                {

                    if (chkSubjectVAT.Checked && string.IsNullOrEmpty(cntVat1.Code))
                    {
                        tabForm.SelectTab(CostPriceTab);
                        ClsCommon.ShowNotification(this, "Please Fill VAT Code.");
                        ActiveControl = cntVat1;
                        e.Cancel = true;
                        return;
                    }

                    if (chkSubjectVAT.Checked && rbVATStatus0.Checked)
                    {
                        tabForm.SelectTab(CostPriceTab);
                        ClsCommon.ShowNotification(this, "Invalid Value Please select from options 1,2,3.");
                        e.Cancel = true;
                        return;
                    }


                    string Price1LL, Price2LL, Price3LL, Price1CC, Price2CC, Price3CC = "";
                    Price1LL = txtPrice1LL.Text;
                    Price2LL = txtPrice2LL.Text;
                    Price3LL = txtPrice3LL.Text;
                    Price1CC = txtPrice1CC.Text;
                    Price2CC = txtPrice2CC.Text;
                    Price3CC = txtPrice3CC.Text;

                    ClsPricing.CalculatePriceFromPotCost(rbBaseCurrencyLC.Checked ? Convert.ToDouble(txtPotCostLC.Text) : Convert.ToDouble(txtPotCostCC.Text),
                                                        txtSalesCoefVal.Text,
                                                        chkSubjectVAT.Checked, cntVat1.VatPerc,
                                                        txtDiscPrice2.Text, txtDiscPrice3.Text,
                                                        cntFCCode.Code, rbBaseCurrencyLC.Checked, rbBaseCurrencyCC.Checked,
                                                        ref Price1LL, ref Price2LL, ref Price3LL, ref Price1CC, ref Price2CC, ref Price3CC,
                                                        "");

                    txtPrice1LL.Text = Price1LL;
                    txtPrice2LL.Text = Price2LL;
                    txtPrice3LL.Text = Price3LL;
                    txtPrice1CC.Text = Price1CC;
                    txtPrice2CC.Text = Price2CC;
                    txtPrice3CC.Text = Price3CC;

                    CalculatePriceForUnit();
                }
            }
            catch (Exception ex)
            {

                ClsCommon.ShowException(StepName, ex);
            }
        }

        private void rbVATTypeE_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (rbVATTypeN.Checked == false)
                {
                    chkSubjectVAT.Checked = false;
                    rbVATTypeE.Checked = true;
                }
            }
            catch (Exception ex)
            {
                ClsCommon.ShowException(StepName, ex);
            }
        }

        private void chk3rdParty_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (chk3rdParty.Checked)
                {
                    chkAssembly.Checked = false;
                    chkTransp.Checked = false;
                    chkAssembly.Enabled = false;
                    chkTransp.Enabled = false;
                }
                else
                {
                    chkAssembly.Enabled = true;
                    chkTransp.Enabled = true;
                }
            }
            catch (Exception ex)
            {

                ClsCommon.ShowException(StepName, ex);
            }
        }

        private void txtCategory_Validated(object sender, EventArgs e)
        {
            try
            {
                clsDynamicFields.strSubGroup = cntCategory.txtCode.Text;
            }
            catch (Exception ex)
            {
                ClsCommon.ShowException(StepName, ex);
            }
        }

        private void chkSubjectVAT_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (chkSubjectVAT.Checked)
                {
                    cntVat1.Enabled = true;
                    PnlVatType.Enabled = true;
                    PnlVatStatus.Enabled = true;
                    rbVATTypeN.Checked = true;

                    rbVATStatus1.Checked = true;

                    rbVATStatus0.Enabled = false;

                    cntVat1.txtCode.Text = ClsConfig.locationDirectives.Ld_VatCode;
                    cntVat1.txtCode_Validating(cntVat1.txtCode.Text, new System.ComponentModel.CancelEventArgs());

                }
                else
                {
                    rbVATTypeN.Checked = true;
                    PnlVatType.Enabled = false;

                    cntVat1.txtCode.Clear();
                    cntVat1.Enabled = false;

                    rbVATStatus0.Enabled = true;
                    rbVATStatus0.Checked = true;
                    PnlVatStatus.Enabled = false;
                }

                string Price1LL, Price2LL, Price3LL, Price1CC, Price2CC, Price3CC = "";
                Price1LL = txtPrice1LL.Text;
                Price2LL = txtPrice2LL.Text;
                Price3LL = txtPrice3LL.Text;
                Price1CC = txtPrice1CC.Text;
                Price2CC = txtPrice2CC.Text;
                Price3CC = txtPrice3CC.Text;

                ClsPricing.CalculatePriceFromPotCost(rbBaseCurrencyLC.Checked ? Convert.ToDouble(txtPotCostLC.Text) : Convert.ToDouble(txtPotCostCC.Text),
                                                    txtSalesCoefVal.Text,
                                                    chkSubjectVAT.Checked, cntVat1.VatPerc,
                                                    txtDiscPrice2.Text, txtDiscPrice3.Text,
                                                    cntFCCode.Code, rbBaseCurrencyLC.Checked, rbBaseCurrencyCC.Checked,
                                                    ref Price1LL, ref Price2LL, ref Price3LL, ref Price1CC, ref Price2CC, ref Price3CC,
                                                    "");

                txtPrice1LL.Text = Price1LL;
                txtPrice2LL.Text = Price2LL;
                txtPrice3LL.Text = Price3LL;
                txtPrice1CC.Text = Price1CC;
                txtPrice2CC.Text = Price2CC;
                txtPrice3CC.Text = Price3CC;

                CalculatePriceForUnit();

            }
            catch (Exception)
            {

                throw;
            }
        }

        //private void StsCoefValC_CheckedChanged(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        if (sender == rbLstStsCoefValC)
        //        {
        //            txtSalesCoefVal.Enabled = true;
        //        }
        //        else if (sender == rbLstStsCoefValU)
        //        {
        //            txtSalesCoefVal.Text = "0";
        //            txtSalesCoefVal.Enabled = false;
        //            txtSalesCoefVal_Validated(txtSalesCoefVal, new EventArgs());
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ClsCommon.ShowException(StepName, ex);
        //    }

        //}

        private void txtAddonRef_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                var senderTextBox = (TextBox)sender;
                string senderText = senderTextBox.Text;

                if (string.IsNullOrEmpty(senderText))
                    return;

                if (StockItemsController.GetAddOnRef(senderText, lcFormMode == ClsCommon.FormMode.Modify ? $"and stk_stkid <> {StockID}" : ""))
                {
                    ClsCommon.ShowNotification(this.ParentForm, "AddOnRef Already Exists!");
                    e.Cancel = true;
                    return;
                }
            }
            catch (Exception ex)
            {
                ClsCommon.ShowException(StepName, ex);
            }
        }

        private void txtPLU_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtPLU.Text)) { return; }

                if (!ValidatePLU())
                {
                    e.Cancel = true;                
                }

                //txtPLU.Text = Convert.ToDouble(txtPLU.Text).ToString();

            }
            catch (Exception ex)
            {
                ClsCommon.ShowException(StepName, ex);
            }
        }
        private bool ValidatePLU()
        {
            try
            {
                if (txtPLU.Text.Any(c => !char.IsDigit(c)))
                {
                    ClsCommon.ShowNotification(this.ParentForm, "PLU # Accepts Only Digits!");
                    return false;

                }
                if (Convert.ToDouble(txtPLU.Text) != 0 && StockItemsController.GetPLU(txtPLU.Text, lcFormMode == ClsCommon.FormMode.Modify ? $"and stk_stkid <> {StockID}" : ""))
                {
                    ClsCommon.ShowNotification(this.ParentForm, "PLU # Already Exists!");
                    return false;
                }                
                return true;
            }
            catch (Exception ex)
            {

                ClsCommon.ShowException(StepName, ex);
                return false;
            }

        }
        private void CostCalculation_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                if (sender == cntFCCode)
                {
                    rbCC.Text = cntFCCode.Code;                
                }

                if (sender is TextBox textBox)
                {
                    if (!string.IsNullOrWhiteSpace(textBox.Text) && double.TryParse(textBox.Text.Trim(), out double result) && lcFormMode != ClsCommon.FormMode.Modify)
                    {
                        if (textBox == txtUnitPrice)
                        {
                            textBox.Text = result.ToString("N3");
                        }
                        else
                        {
                            textBox.Text = result.ToString("N");
                        }
                    }
                }

                if (sender == txtUnitPrice)
                {
                    if (rbLC.Checked)
                    {
                        txtUnitPrice.Text = Convert.ToDouble(txtUnitPrice.Text).ToString(ClsConfig.locationDirectives.Ld_CostLCFormat);
                    }
                    else if(rbCC.Checked && rbCC.Text == ClsConfig.gbCurr2)
                    {
                        txtUnitPrice.Text = Convert.ToDouble(txtUnitPrice.Text).ToString(ClsConfig.locationDirectives.Ld_CostCCFormat);
                    }                    
                    else
                    {
                        txtUnitPrice.Text = Convert.ToDouble(txtUnitPrice.Text).ToString(ClsConfig.locationDirectives.Ld_CostFCFormat);
                    }

                    string Op = "";
                    double rate = 0;
                    ClsCom.GetRate(cntFCCode.Code, DateTime.Today.ToString(), "LC", ref Op, ref rate, ClsConfig.locationDirectives.Ld_POSRate);
                    if (Op == "" && rate == -1)
                    {
                        txtUnitPrice.NumericText = 0;
                        ClsCommon.ShowNotification(this.ParentForm, "Rate for chosen currency isn't defined");
                        //txtUnitPrice.Focus();
                        return;

                    }
                }

                CalculatePotentialCost();
            }
            catch (Exception ex)
            {

                ClsCommon.ShowException(StepName, ex);
            }
        }

        private void rbBaseCurrency_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                //CalculatePotentialCost();

                foreach (cntStockUnit cntStockUnit in flpStockUnits.Controls)
                {
                    if (sender == rbBaseCurrencyLC)
                    {
                        cntStockUnit.txtPrice1LC.Enabled = true;
                        cntStockUnit.txtPrice2LC.Enabled = true;
                        cntStockUnit.txtPrice3LC.Enabled = true;

                        txtPrice1LL.Enabled = true;
                        txtPrice2LL.Enabled = true;
                        txtPrice3LL.Enabled = true;

                        cntStockUnit.txtPrice1CC.Enabled = false;
                        cntStockUnit.txtPrice2CC.Enabled = false;
                        cntStockUnit.txtPrice3CC.Enabled = false;

                        txtPrice1CC.Enabled = false;
                        txtPrice2CC.Enabled = false;
                        txtPrice3CC.Enabled = false;
                    }
                    else
                    {
                        cntStockUnit.txtPrice1LC.Enabled = false;
                        cntStockUnit.txtPrice2LC.Enabled = false;
                        cntStockUnit.txtPrice3LC.Enabled = false;

                        txtPrice1LL.Enabled = false;
                        txtPrice2LL.Enabled = false;
                        txtPrice3LL.Enabled = false;

                        cntStockUnit.txtPrice1CC.Enabled = true;
                        cntStockUnit.txtPrice2CC.Enabled = true;
                        cntStockUnit.txtPrice3CC.Enabled = true;

                        txtPrice1CC.Enabled = true;
                        txtPrice2CC.Enabled = true;
                        txtPrice3CC.Enabled = true;
                    }
                }


            }
            catch (Exception)
            {

                throw;
            }

        }

        private void txtSalesCoefVal_Validated(object sender, EventArgs e)
        {
            try
            {
                if (ClsConfig.locationDirectives.Ld_SalePriceCalculationMode?.ToUpper() == "X".ToUpper() || ClsConfig.locationDirectives.Ld_SalePriceCalculationMode?.ToUpper() == "Y".ToUpper())
                {
                    if (sender == txtSalesCoefVal) { CalculatePotentialCost("Price1"); }
                    else if (sender == txtDiscPrice2) { CalculatePotentialCost("Price2"); }
                    else if (sender == txtDiscPrice3) { CalculatePotentialCost("Price3"); }
                }
                else
                {
                    if (sender != txtDiscPrice2 & sender != txtDiscPrice3)
                    {
                        CalculatePotentialCost();
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void txtMinQty_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(txtMaxQty.Text) && !string.IsNullOrWhiteSpace(txtMinQty.Text))
                {
                    if (decimal.TryParse(txtMaxQty.Text.Trim(), out decimal maxQty) &&
                        decimal.TryParse(txtMinQty.Text.Trim(), out decimal minQty))
                    {
                        if (minQty > maxQty)
                        {
                            ClsCommon.ShowNotification(this.ParentForm, "Maximum Value Must be Greater than the Minimum Value");
                            e.Cancel = true;
                        }
                    }
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        private void pnlCurrency_Validating(object sender, CancelEventArgs e)
        {
            try
            {

                rbBaseCurrencyLC.Checked = rbLC.Checked;
                rbBaseCurrencyCC.Checked = rbCC.Checked;              

                if (rbCC.Checked)
                {
                    txtUnitPrice.Iscale = CCcostIscale;
                    txtUnitPrice.Text = Convert.ToDouble(txtUnitPrice.Text).ToString(ClsConfig.locationDirectives.Ld_CostCCFormat);
                }
                else if (rbLC.Checked)
                {
                    txtUnitPrice.Iscale = LCcostIscale;
                    txtUnitPrice.Text = Convert.ToDouble(txtUnitPrice.Text).ToString(ClsConfig.locationDirectives.Ld_CostLCFormat);
                }
                else
                {
                    txtUnitPrice.Iscale = FCcostIscale;
                    txtUnitPrice.Text = Convert.ToDouble(txtUnitPrice.Text).ToString(ClsConfig.locationDirectives.Ld_CostFCFormat);
                }

            }
            catch (Exception ex)
            {
                ClsCommon.ShowException(StepName, ex);
            }
        }      

        private void txtDiscPrice_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                if (lcFormMode == ClsCommon.FormMode.Add)
                {
                    if (ClsConfig.locationDirectives.Ld_SalePriceCalculationMode?.ToUpper() == "M" || ClsConfig.locationDirectives.Ld_SalePriceCalculationMode?.ToUpper() == "N")
                    {
                        if (txtDiscPrice2.Text == "9999" || string.IsNullOrEmpty(txtDiscPrice2.Text) )
                        {
                            txtPrice2LL.ReadOnly = false;
                            txtPrice2CC.ReadOnly = false;
                        }
                        else
                        {
                            txtPrice2LL.ReadOnly = true;
                            txtPrice2CC.ReadOnly = true;

                            string Price2CC, Price2LL = "";
                            Price2LL = txtPrice2LL.Text;
                            Price2CC = txtPrice2CC.Text;

                            //ClsPricing.CalculatePriceFromPotCost(rbBaseCurrencyLC.Checked ? Convert.ToDouble(txtPotCostLC.Text) : Convert.ToDouble(txtPotCostCC.Text),
                            //                                    txtSalesCoefVal.Text,
                            //                                    chkSubjectVAT.Checked, cntVat1.VatPerc,
                            //                                    txtDiscPrice2.Text, txtDiscPrice3.Text,
                            //                                    cntFCCode.Code, rbBaseCurrencyLC.Checked, rbBaseCurrencyCC.Checked,
                            //                                    ref Price1LL, ref Price2LL, ref Price3LL, ref Price1CC, ref Price2CC, ref Price3CC,
                            //                                    "");


                            ClsPricing.CalculatePriceAfterDiscount(txtDiscPrice2.Text, txtPrice1LL.Text, txtPrice1CC.Text, ref Price2LL, ref Price2CC);
                            txtPrice2LL.Text = Math.Round(Convert.ToDouble(Price2LL), ClsConfig.locationDirectives.Ld_PriceLC).ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat);
                            txtPrice2CC.Text = Math.Round(Convert.ToDouble(Price2CC), ClsConfig.locationDirectives.Ld_PriceCC).ToString(ClsConfig.locationDirectives.Ld_PriceCCFormat);

                            CalculatePriceForUnit();
                        }

                        if (txtDiscPrice3.Text == "9999" || string.IsNullOrEmpty(txtDiscPrice3.Text) )
                        {
                            txtPrice3LL.ReadOnly = false;
                            txtPrice3CC.ReadOnly = false;
                        }
                        else
                        {
                            txtPrice3LL.ReadOnly = true;
                            txtPrice3CC.ReadOnly = true;

                            string Price3LL, Price3CC = "";
                            Price3LL = txtPrice3LL.Text;
                            Price3CC = txtPrice3CC.Text;

                            //ClsPricing.CalculatePriceFromPotCost(rbBaseCurrencyLC.Checked ? Convert.ToDouble(txtPotCostLC.Text) : Convert.ToDouble(txtPotCostCC.Text),
                            //                                    txtSalesCoefVal.Text,
                            //                                    chkSubjectVAT.Checked, cntVat1.VatPerc,
                            //                                    txtDiscPrice2.Text, txtDiscPrice3.Text,
                            //                                    cntFCCode.Code, rbBaseCurrencyLC.Checked, rbBaseCurrencyCC.Checked,
                            //                                    ref Price1LL, ref Price2LL, ref Price3LL, ref Price1CC, ref Price2CC, ref Price3CC,
                            //                                    "");

                            ClsPricing.CalculatePriceAfterDiscount(txtDiscPrice3.Text, txtPrice1LL.Text, txtPrice1CC.Text, ref Price3LL, ref Price3CC);

                            txtPrice3LL.Text = Math.Round(Convert.ToDouble(Price3LL), ClsConfig.locationDirectives.Ld_PriceLC).ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat);
                            txtPrice3CC.Text = Math.Round(Convert.ToDouble(Price3CC), ClsConfig.locationDirectives.Ld_PriceCC).ToString(ClsConfig.locationDirectives.Ld_PriceCCFormat);

                            CalculatePriceForUnit();

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ClsCommon.ShowException(Text, ex);
            }
        }
        private void txtCustomsPerc_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                if (lcFormMode == ClsCommon.FormMode.Add && (sender is TextBox textBox && (textBox == txtCustomsPerc || textBox == txtOHPerc)))
                {
                    txtCostCoef.Text = "0.00";
                    if (!string.IsNullOrEmpty(txtCustomsPerc.Text))
                    {
                        txtCostCoef.Text = (double.Parse(txtCostCoef.Text) + double.Parse(txtCustomsPerc.Text)).ToString();
                    }
                    if (!string.IsNullOrEmpty(txtOHPerc.Text))
                    {
                        txtCostCoef.Text = (double.Parse(txtCostCoef.Text) + double.Parse(txtOHPerc.Text)).ToString();
                    }
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        private void chkFollow_CheckedChanged(object sender, EventArgs e)
        {
            try
            {

                if (((CheckBox)sender).Checked || !((CheckBox)sender).Checked)
                {
                    if (((CheckBox)sender) == chkUseInvDisc)
                    {
                        chkFollow.Checked = chkUseInvDisc.Checked;
                    }
                    else if (((CheckBox)sender) == chkFollow)
                    {
                        chkUseInvDisc.Checked = chkFollow.Checked;
                    }
                }


            }
            catch (Exception)
            {

                throw;
            }
        }

        private void txtPrice1LL_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                string Operator = "";
                double Rate = -1;

                ClsCom.GetRate(ClsConfig.gbCurr2, DateTime.Today.ToString(), "LC", ref Operator, ref Rate, ClsConfig.locationDirectives.Ld_POSRate);

                if (sender is TextBox textBox)
                {
                    if (sender == txtPrice1LL || sender == txtPrice2LL || sender == txtPrice3LL)
                    {
                        if (string.IsNullOrWhiteSpace(((TextBox)sender).Text))
                        {
                            ((TextBox)sender).Text = "0";
                        }
                        else
                        {
                            ((TextBox)sender).Text = ClsPricing.RoundPrice(double.Parse(((TextBox)sender).Text.Trim())).ToString();
                        }

                        if (Operator != "" && Rate != -1)
                        {
                            if (Operator == "*")
                            {
                                if (sender == txtPrice1LL)
                                {
                                    txtPrice1CC.Text = (Convert.ToDouble(txtPrice1LL.Text) / Rate).ToString(ClsConfig.locationDirectives.Ld_PriceCCFormat);
                                    txtPrice1LL.NumericText = Convert.ToDouble(txtPrice1LL.Text);
                                }
                                if (sender == txtPrice2LL)
                                {
                                    txtPrice2CC.Text = (Convert.ToDouble(txtPrice2LL.Text) / Rate).ToString(ClsConfig.locationDirectives.Ld_PriceCCFormat);
                                    txtPrice2LL.NumericText = Convert.ToDouble(txtPrice2LL.Text);
                                }
                                if (sender == txtPrice3LL)
                                {
                                    txtPrice3CC.Text = (Convert.ToDouble(txtPrice3LL.Text) / Rate).ToString(ClsConfig.locationDirectives.Ld_PriceCCFormat);
                                    txtPrice3LL.NumericText = Convert.ToDouble(txtPrice3LL.Text);
                                }
                            }
                            else
                            {
                                if (sender == txtPrice1LL)
                                {
                                    txtPrice1CC.Text = (Convert.ToDouble(txtPrice1LL.Text) * Rate).ToString(ClsConfig.locationDirectives.Ld_PriceCCFormat);
                                    txtPrice1LL.NumericText = Convert.ToDouble(txtPrice1LL.Text);
                                }
                                if (sender == txtPrice2LL)
                                {
                                    txtPrice2CC.Text = (Convert.ToDouble(txtPrice2LL.Text) * Rate).ToString(ClsConfig.locationDirectives.Ld_PriceCCFormat);
                                    txtPrice2LL.NumericText = Convert.ToDouble(txtPrice2LL.Text);
                                }
                                if (sender == txtPrice3LL)
                                {
                                    txtPrice3CC.Text = (Convert.ToDouble(txtPrice3LL.Text) * Rate).ToString(ClsConfig.locationDirectives.Ld_PriceCCFormat);
                                    txtPrice3LL.NumericText = Convert.ToDouble(txtPrice3LL.Text);
                                }
                            }
                        }
                        else
                        {
                            if (sender == txtPrice1LL)
                            {
                                txtPrice1CC.Text = 0.ToString(ClsConfig.locationDirectives.Ld_PriceCCFormat);
                            }
                            if (sender == txtPrice2LL)
                            {
                                txtPrice2CC.Text = 0.ToString(ClsConfig.locationDirectives.Ld_PriceCCFormat);
                            }
                            if (sender == txtPrice3LL)
                            {
                                txtPrice3CC.Text = 0.ToString(ClsConfig.locationDirectives.Ld_PriceCCFormat);
                            }

                        }

                    }
                    else if (sender == txtPrice1CC || sender == txtPrice2CC || sender == txtPrice3CC)
                    {
                        if (Operator != "" && Rate != -1)
                        {
                            if (Operator == "*")
                            {
                                if (sender == txtPrice1CC)
                                {
                                    txtPrice1LL.Text = (Convert.ToDouble(txtPrice1CC.Text) * Rate).ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat);
                                }
                                if (sender == txtPrice2CC)
                                {
                                    txtPrice2LL.Text = (Convert.ToDouble(txtPrice2CC.Text) * Rate).ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat);
                                }
                                if (sender == txtPrice3CC)
                                {
                                    txtPrice3LL.Text = (Convert.ToDouble(txtPrice3CC.Text) * Rate).ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat);
                                }
                            }
                            else
                            {
                                if (sender == txtPrice1CC)
                                {
                                    txtPrice1LL.Text = (Convert.ToDouble(txtPrice1CC.Text) / Rate).ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat);
                                }
                                if (sender == txtPrice2CC)
                                {
                                    txtPrice2LL.Text = (Convert.ToDouble(txtPrice2CC.Text) / Rate).ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat);
                                }
                                if (sender == txtPrice3CC)
                                {
                                    txtPrice3LL.Text = (Convert.ToDouble(txtPrice3CC.Text) / Rate).ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat);
                                }
                            }
                        }
                        else
                        {
                            if (sender == txtPrice1CC)
                            {
                                txtPrice1LL.Text = 0.ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat);
                            }
                            if (sender == txtPrice2CC)
                            {
                                txtPrice2LL.Text = 0.ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat);
                            }
                            if (sender == txtPrice3CC)
                            {
                                txtPrice3LL.Text = 0.ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat);
                            }

                        }
                    }
                }

                txtDiscPrice_Validating(txtDiscPrice2.Text, new System.ComponentModel.CancelEventArgs());
                CalculatePriceForUnit();

            }
            catch (Exception)
            {

                throw;
            }
        }

        private void cntCurrency2_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                txtOriginalCost.Iscale = cntCurrency2.Code == ClsConfig.gbCurr1 ? LCcostIscale : CCcostIscale;
            }
            catch (Exception ex)
            {

                ClsCommon.ShowException(StepName, ex);
            }
        }

        private void Barcode_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(cntBarcode1.txtBarcode1.Text) && !string.IsNullOrWhiteSpace(cntBarcode2.txtBarcode1.Text))
                {
                    if (cntBarcode1.txtBarcode1.Text.Trim() == cntBarcode2.txtBarcode1.Text.Trim())
                    {
                        ClsCommon.ShowNotification(this.ParentForm, "Barcode1 Must Be Different Than Barcode2");
                        ActiveControl = cntBarcode2.txtBarcode1;
                        e.Cancel = true;
                    }
                }

                if (flpStockUnits.Controls.Count > 0)
                {
                    foreach (cntStockUnit cnt in flpStockUnits.Controls)
                    {
                        foreach (cntBarcode barcode in cnt.flpSynBarcode.Controls)
                        {
                            if (!string.IsNullOrEmpty(barcode.txtBarcode1.Text))
                            {
                                if (cntBarcode1.txtBarcode1.Text.Trim() == barcode.txtBarcode1.Text || barcode.txtBarcode1.Text == cntBarcode2.txtBarcode1.Text.Trim())
                                {
                                    ClsCommon.ShowNotification(this.ParentForm, "Duplicate Barcode.");
                                    ActiveControl = barcode.txtBarcode1;
                                    e.Cancel = true;
                                }
                            }
                        }
                    }

                }

                if (cntBarcode1.txtBarcode1.LastText == "") { cntBarcode1.txtBarcode1.LastText = cntBarcode1.txtBarcode1.Text; }
                if (cntBarcode2.txtBarcode1.LastText == "") { cntBarcode2.txtBarcode1.LastText = cntBarcode2.txtBarcode1.Text; }

                if (lcFormMode == ClsCommon.FormMode.Modify && ((sender == cntBarcode1 && cntBarcode1.txtBarcode1.LastText == cntBarcode1.txtBarcode1.Text) ||
                        (sender == cntBarcode1 && cntBarcode1.txtBarcode1.LastText == cntBarcode1.txtBarcode1.Text)))
                {
                    return;
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(cntBarcode1.txtBarcode1.Text) | !string.IsNullOrWhiteSpace(cntBarcode2.txtBarcode1.Text))
                    {
                        if (sender == cntBarcode1 && !string.IsNullOrWhiteSpace(cntBarcode1.txtBarcode1.Text))
                        {
                            if (BarcodeSynController.CheckBarcodeSyn(cntBarcode1.txtBarcode1.Text, lcFormMode == ClsCommon.FormMode.Modify ? StockID : 0))
                            {
                                ClsCommon.ShowNotification(this, "Barcode Already Exists");
                                e.Cancel = true;
                                return;
                            }
                        }
                        cntBarcode1.txtBarcode1.LastText = cntBarcode1.txtBarcode1.Text;

                        if (sender == cntBarcode2 && !string.IsNullOrWhiteSpace(cntBarcode2.txtBarcode1.Text))
                        {
                            if (BarcodeSynController.CheckBarcodeSyn(cntBarcode2.txtBarcode1.Text, lcFormMode == ClsCommon.FormMode.Modify ? StockID : 0))
                            {
                                ClsCommon.ShowNotification(this, "Barcode Already Exists");
                                e.Cancel = true;
                                return;
                            }
                        }
                        cntBarcode2.txtBarcode1.LastText = cntBarcode2.txtBarcode1.Text;

                    }
                }

            }
            catch (Exception ex)
            {
                ClsCommon.ShowException(StepName, ex);
            }
        }

        private void CalculatePotentialCost(string ModPrice = "")
        {
            double X = 0;
            try
            {
                if (!string.IsNullOrEmpty(txtUnitPrice.Text) && lcFormMode == ClsCommon.FormMode.Add && !string.IsNullOrEmpty(cntFCCode.Code))
                {

                    string PotCostLC = "", PotCostCC = "";
                    PotCostLC = txtPotCostLC.Text;
                    PotCostCC = txtPotCostCC.Text;

                    ClsPricing.CalculatePotCost(txtUnitPrice.Text,
                                                cntSupplier.SupVatTypeCode,
                                                chkSubjectVAT.Checked,
                                                cntVat1.VatPerc,
                                                txtUnitDiscPotCost.Text,
                                                chkUseItmDisc.Checked,
                                                txtAddInvDisc.Text,
                                                chkUseInvDisc.Checked,
                                                txtPurchaseQty.Text,
                                                txtPromotionQty.Text,
                                                chkUsePromQty.Checked,
                                                txtCustomsPerc.Text,
                                                txtOHPerc.Text,
                                                rbBaseCurrencyLC.Checked ? ClsConfig.gbCurr1 : "",
                                                rbBaseCurrencyCC.Checked ? ClsConfig.gbCurr2 : "",
                                                ref PotCostLC,
                                                ref PotCostCC);

                    txtPotCostLC.Text = PotCostLC;
                    txtPotCostCC.Text = PotCostCC;

                    if (cntFCCode.Code.ToUpper() == ClsConfig.gbCurr1)
                    {
                        txtPotCostFC.Text = PotCostLC;
                    }
                    else if (cntFCCode.Code.ToUpper() == ClsConfig.gbCurr2)
                    {
                        txtPotCostFC.Text = PotCostCC;
                    }
                    else
                    {
                        string Operator = "";
                        double Rate = -1;

                        ClsCom.GetRate(cntFCCode.Code.ToUpper(), DateTime.Today.ToString(), "CC", ref Operator, ref Rate, ClsConfig.locationDirectives.Ld_BORate);


                        if (!string.IsNullOrEmpty(Operator) && Rate != -1)
                        {
                            if (Operator == "*")
                            {
                                txtPotCostFC.Text = (Convert.ToDouble(txtPotCostCC.Text) * Rate).ToString(ClsConfig.locationDirectives.Ld_CostFCFormat);
                            }
                            else
                            {
                                txtPotCostFC.Text = (Convert.ToDouble(txtPotCostCC.Text) / Rate).ToString(ClsConfig.locationDirectives.Ld_CostFCFormat);
                            }
                        }
                        else
                        {
                            ClsCommon.ShowNotification(this, "Rate From [" + cntFCCode.Code.ToUpper() + "] to [" + ClsConfig.gbCurr1 + "] not defined at #" + DateTime.Today.ToString() + "#.");
                            txtPotCostFC.Text = 0.ToString(ClsConfig.locationDirectives.Ld_CostFCFormat);
                        }

                    }

                    if (rbBaseCurrencyLC.Checked)
                    {
                        X = Convert.ToDouble(PotCostLC);
                    }
                    else if (rbBaseCurrencyCC.Checked)
                    {
                        X = Convert.ToDouble(PotCostCC);
                    }


                    string Price1LL, Price2LL, Price3LL, Price1CC, Price2CC, Price3CC = "";
                    Price1LL = txtPrice1LL.Text;
                    Price2LL = txtPrice2LL.Text;
                    Price3LL = txtPrice3LL.Text;
                    Price1CC = txtPrice1CC.Text;
                    Price2CC = txtPrice2CC.Text;
                    Price3CC = txtPrice3CC.Text;

                    ClsPricing.CalculatePriceFromPotCost(X, txtSalesCoefVal.Text,
                                                        chkSubjectVAT.Checked, cntVat1.VatPerc,
                                                        txtDiscPrice2.Text, txtDiscPrice3.Text,
                                                        cntFCCode.Code, rbBaseCurrencyLC.Checked, rbBaseCurrencyCC.Checked,
                                                        ref Price1LL, ref Price2LL, ref Price3LL, ref Price1CC, ref Price2CC, ref Price3CC,
                                                        ModPrice);

                    txtPrice1LL.Text = Price1LL;
                    txtPrice2LL.Text = Price2LL;
                    txtPrice3LL.Text = Price3LL;
                    txtPrice1CC.Text = Price1CC;
                    txtPrice2CC.Text = Price2CC;
                    txtPrice3CC.Text = Price3CC;

                    CalculatePriceForUnit();

                }

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        #endregion


        #region Saving

        private bool SaveIsAllowed()
        {
            try
            {
                if (lcFormMode == ClsCommon.FormMode.Delete)
                {
                    if (ClsCommon.ShowQuestion(StepName, "Are you sure that you want to delete the record ?") != DialogResult.Yes) { return false; }
                    else
                    {
                        if (StockItemsController.IsMovemented(StockID))
                        {
                            ClsCommon.ShowNotification(this.ParentForm, "This Item is Already Movemented, Delete Is Not Allowed.");

                            return false;
                        }
                    }
                }

                if (ClsCommon.ShowQuestion(StepName, "Do You Really Want To Save Changes?") == DialogResult.Yes)
                {
                    if (!ValidateQuantityDetails())
                    {
                        return false;
                    }
                    if (!ValidatePLU())
                    {
                        return false;
                    }
                    if (ClsCommon.ExistMissingMandatoryFields(this))
                    {
                        //ClsCommon.ShowNotification(this.ParentForm, "Incomplete Stock Item Definition!!!");
                        return false;
                    }
                    //Conditions related to the UI               
                    if (string.IsNullOrEmpty(cntSupplier.txtCode.Text.Trim()))
                    {
                        tabForm.SelectTab(DefinitionTab);
                        ActiveControl = cntSupplier.txtCode;
                        ClsCommon.ShowNotification(this.ParentForm, "Supplier Code Can Not Be Empty");
                        return false;
                    }
                    if (string.IsNullOrEmpty(cntCategory.txtCode.Text.Trim()))
                    {
                        tabForm.SelectTab(DefinitionTab);
                        ActiveControl = cntCategory.txtCode;
                        ClsCommon.ShowNotification(this.ParentForm, "Category Code Can Not Be Empty");
                        return false;
                    }
                    if (string.IsNullOrEmpty(txtSerialNo.Text.Trim()))
                    {
                        tabForm.SelectTab(DefinitionTab);
                        ActiveControl = txtSerialNo;
                        ClsCommon.ShowNotification(this.ParentForm, "Serial No. Can Not Be Empty");
                        return false;
                    }
                    if (chk3rdParty.Checked && chkTransp.Checked)
                    {
                        tabForm.SelectTab(DefinitionTab);
                        ActiveControl = chkTransp;
                        ClsCommon.ShowNotification(this.ParentForm, "Could Not Accept Allow Transposition Set To 'Y' While Third Party Item Is Set To 'Y'");
                        return false;

                    }
                    if (chk3rdParty.Checked && chkAssembly.Checked)
                    {
                        tabForm.SelectTab(DefinitionTab);
                        ActiveControl = chkAssembly;
                        ClsCommon.ShowNotification(this.ParentForm, "Could Not Accept Assembly Created Set To 'Y' While Third Party Item Is Set To 'Y'");
                        return false;
                    }
                    if (lcFormMode == ClsCommon.FormMode.Modify && ((chk3rdParty.Checked && ThirdParty == "N") || (!chk3rdParty.Checked && ThirdParty == "Y")))
                    {
                        if (StockItemsController.IsMovemented(StockID))
                        {
                            ClsCommon.ShowNotification(this.ParentForm, "This Item is Already Movemented, Change Is Not Allowed.");
                            ActiveControl = chk3rdParty;
                            return false;
                        }
                    }

                    if (string.IsNullOrEmpty(cntFCCode.txtCode.Text.Trim()))
                    {
                        tabForm.SelectTab(CostPriceTab);
                        ActiveControl = cntFCCode.txtCode;
                        ClsCommon.ShowNotification(this.ParentForm, "FC Code Can Not Be Empty");
                        return false;
                    }
                    if (chkSubjectVAT.Checked && string.IsNullOrEmpty(cntVat1.txtCode.Text.Trim()))
                    {
                        tabForm.SelectTab(CostPriceTab);
                        ActiveControl = cntVat1.txtCode;
                        ClsCommon.ShowNotification(this.ParentForm, "Please Fill VAT Code.");
                        return false;
                    }

                    if (flpStockUnits.Controls.Count == 0
                            ||
                        Array.FindAll(flpStockUnits.Controls.Cast<Control>().OfType<cntStockUnit>().ToArray(),
                             cnt => !cnt.Delete && !string.IsNullOrEmpty(cnt.cntUnit.txtCode.Text) && !string.IsNullOrEmpty(cnt.cntParentUnit.txtCode.Text)).Length == 0)

                    {
                        tabForm.SelectTab(tabStckUnits);
                        ActiveControl = btnAddUnit;
                        ClsCommon.ShowNotification(this.ParentForm, "Stock Unit Not Defined");
                        return false;
                    }

                    txtUnitsLogging.Text = ValidateUnits();

                    if (txtUnitsLogging.Text.Trim() != "")
                    {
                        ClsCommon.ShowNotification(this.ParentForm, txtUnitsLogging.Text);
                        return false;

                    }


                    if (FpnlSuppRef.Controls.Count != 0 &&
                 Array.FindAll(FpnlSuppRef.Controls.Cast<Control>().OfType<cntSupplierReference>().ToArray(),
                      cnt => !string.IsNullOrEmpty(cnt.txtReference.Text)).Length == 0)

                    {
                        tabForm.SelectTab(SupplierRefTab);
                        ClsCommon.ShowNotification(this.ParentForm, "Supplier Reference Not Defined");
                        return false;
                    }

                    if (!clsDynamicFields.SaveDynamicFieldsIsAllowed())
                    {
                        return false;
                    }

                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void SaveData()
        {
            try
            {

                StockItemsController SaveOperation = new StockItemsController();
                StockItems stkItm = new StockItems();

                //DEFINITION
                stkItm.Stk_Supplier = cntSupplier.Code;
                stkItm.Stk_Category = cntCategory.Code;
                stkItm.Stk_Serial = txtSerialNo.Text;
                stkItm.Stk_Status = chkActive.Checked ? "A" : "I";
                stkItm.Stk_StkId = lcFormMode == ClsCommon.FormMode.Add ? StockItemsController.GetLast() : StockID;
                //stkItm.Stk_HoStkId = !string.IsNullOrEmpty(txtHOCode.Text) ? Convert.ToInt32(txtHOCode.Text) : 0;
                stkItm.Stk_Desc = !string.IsNullOrEmpty(txtDesc.Text.Trim()) ? txtDesc.Text.Trim() : null;
                stkItm.Stk_DescPos = !string.IsNullOrEmpty(txtPOSDesc.Text.Trim()) ? txtPOSDesc.Text.Trim() : null;
                stkItm.Stk_Brand = !string.IsNullOrEmpty(cntBrand.Code) ? cntBrand.Code : null;
                stkItm.Stk_CrDate = !string.IsNullOrEmpty(dteCrDate.Text) ? Convert.ToDateTime(dteCrDate.Text) : (DateTime?)null;
                stkItm.Stk_ItmService = rbItmServiceI.Checked ? "I" : "S";
                stkItm.Stk_GoodsLoc = !string.IsNullOrEmpty(cntGoodsLoc.Code) ? cntGoodsLoc.Code : null;
                stkItm.Stk_ValidityDays = !string.IsNullOrEmpty(txtValidityDays.Text) ? Convert.ToInt32(txtValidityDays.Text) : (int?)null;
                stkItm.Stk_Gift = rbBaseCurrencyCC.Checked ? "C" : "L";
                stkItm.Stk_Assembly = chkAssembly.Checked ? "Y" : "N";
                stkItm.Stk_StopOrder = chkStopOrder.Checked ? "Y" : "N";
                stkItm.Stk_3rdParty = chk3rdParty.Checked ? "Y" : "N";
                stkItm.Stk_Consignment = chkConsignmentItem.Checked ? "Y" : "N";
                stkItm.Stk_Transp = chkTransp.Checked ? "Y" : "N";
                stkItm.Stk_Expiry = chkExpiryItem.Checked ? "Y" : "N";
                stkItm.Stk_DiscExc = chkDiscountExclude.Checked ? new StockDiscExclusion() { StkExcl_Stkid = stkItm.Stk_StkId } : null;
                stkItm.Stk_Online = chkOnlineItem.Checked ? new StockOnline() { Onl_Stkid = stkItm.Stk_StkId } : null;

                //BARCODES
                stkItm.Stk_Barcode1 = !string.IsNullOrEmpty(cntBarcode1.txtBarcode1.Text) ? cntBarcode1.txtBarcode1.Text : null;
                stkItm.Stk_Barcode2 = !string.IsNullOrEmpty(cntBarcode2.txtBarcode1.Text) ? cntBarcode2.txtBarcode1.Text : null;
                stkItm.Stk_AddOnRef = !string.IsNullOrEmpty(txtAddonRef.Text) ? txtAddonRef.Text : null;
                stkItm.Stk_PLUnbr = !string.IsNullOrEmpty(txtPLU.Text) ? Convert.ToDouble(txtPLU.Text) : (double?)null;

                // QUANTITY
                stkItm.Stk_MinQty = !string.IsNullOrEmpty(txtMinQty.Text) ? Convert.ToDouble(txtMinQty.Text) : (double?)null;
                stkItm.Stk_MaxQty = !string.IsNullOrEmpty(txtMaxQty.Text) ? Convert.ToDouble(txtMaxQty.Text) : (double?)null;
                stkItm.Stk_BBFDate1 = !string.IsNullOrEmpty(txtBBFDate.Text) ? Convert.ToDateTime(txtBBFDate.Text) : (DateTime?)null;
                stkItm.Stk_BBFQty1 = !string.IsNullOrEmpty(txtBBFQty.Text) ? Convert.ToDouble(txtBBFQty.Text) : (double?)null;
                stkItm.Stk_AvlbQty1 = !string.IsNullOrEmpty(txtAvailableQty.Text) ? Convert.ToDouble(txtAvailableQty.Text) : (double?)null;
                stkItm.Stk_Return1 = !string.IsNullOrEmpty(txtQtyToReturn.Text) ? Convert.ToDouble(txtQtyToReturn.Text) : (double?)null;

                //COST / PRICE
                stkItm.Stk_ModSal = chkModSal.Checked ? "Y" : "N";
                stkItm.Stk_FCcurr = !string.IsNullOrEmpty(cntFCCode.Code) ? cntFCCode.Code : null;
                stkItm.Stk_BBFCostLC = !string.IsNullOrEmpty(txtBBFCostLC.Text) ? Convert.ToDouble(txtBBFCostLC.Text) : (double?)null;
                stkItm.Stk_BBFCostCC = !string.IsNullOrEmpty(txtBBFCostCC.Text) ? Convert.ToDouble(txtBBFCostCC.Text) : (double?)null;
                stkItm.Stk_BBFCostFC = !string.IsNullOrEmpty(txtBBFCostFC.Text) ? Convert.ToDouble(txtBBFCostFC.Text) : (double?)null;
                stkItm.Stk_ActCostLL = !string.IsNullOrEmpty(txtActCostLC.Text) ? Convert.ToDouble(txtActCostLC.Text) : (double?)null;
                stkItm.Stk_ActCostCC = !string.IsNullOrEmpty(txtActCostCC.Text) ? Convert.ToDouble(txtActCostCC.Text) : (double?)null;
                stkItm.Stk_ActCostFC = !string.IsNullOrEmpty(txtActCostFC.Text) ? Convert.ToDouble(txtActCostFC.Text) : (double?)null;
                stkItm.Stk_LstCostLL = !string.IsNullOrEmpty(txtLastCostLC.Text) ? Convert.ToDouble(txtLastCostLC.Text) : (double?)null;
                stkItm.Stk_LstCostCC = !string.IsNullOrEmpty(txtLastCostCC.Text) ? Convert.ToDouble(txtLastCostCC.Text) : (double?)null;
                stkItm.Stk_LstCostFC = !string.IsNullOrEmpty(txtLastCostFC.Text) ? Convert.ToDouble(txtLastCostFC.Text) : (double?)null;
                stkItm.stk_PotcostLL = !string.IsNullOrEmpty(txtPotCostLC.Text) ? Convert.ToDouble(txtPotCostLC.Text) : (double?)null;
                stkItm.stk_PotCostCC = !string.IsNullOrEmpty(txtPotCostCC.Text) ? Convert.ToDouble(txtPotCostCC.Text) : (double?)null;
                stkItm.stk_PotCostFC = !string.IsNullOrEmpty(txtPotCostFC.Text) ? Convert.ToDouble(txtPotCostFC.Text) : (double?)null;
                stkItm.Stk_LDateCount = !string.IsNullOrEmpty(txtLDatePhyCnt.Text) ? Convert.ToDateTime(txtLDatePhyCnt.Text) : (DateTime?)null;
                stkItm.Stk_SubjectVat = chkSubjectVAT.Checked ? "Y" : "N";
                stkItm.Stk_VatType = rbVATTypeN.Checked ? "N" : "E";
                stkItm.Stk_VatCode = !string.IsNullOrEmpty(cntVat1.Code) ? cntVat1.Code : null;
                stkItm.Stk_VatPer = !string.IsNullOrEmpty(cntVat1.VatPerc) ? Convert.ToDouble(cntVat1.VatPerc) : (double?)null;
                stkItm.Stk_VATStatus = rbVATStatus0.Checked ? "0" : rbVATStatus1.Checked ? "1" : rbVATStatus2.Checked ? "2" : "3";
                stkItm.Stk_1SlPriceLC = !string.IsNullOrEmpty(txtPrice1LL.Text) ? Convert.ToDouble(txtPrice1LL.Text) : (double?)null;
                stkItm.Stk_1SlPriceCC = !string.IsNullOrEmpty(txtPrice1CC.Text) ? Convert.ToDouble(txtPrice1CC.Text) : (double?)null;
                stkItm.Stk_2SLPriceLC = !string.IsNullOrEmpty(txtPrice2LL.Text) ? Convert.ToDouble(txtPrice2LL.Text) : (double?)null;
                stkItm.Stk_2SLPriceCC = !string.IsNullOrEmpty(txtPrice2CC.Text) ? Convert.ToDouble(txtPrice2CC.Text) : (double?)null;
                stkItm.Stk_3SLPriceLC = !string.IsNullOrEmpty(txtPrice3LL.Text) ? Convert.ToDouble(txtPrice3LL.Text) : (double?)null;
                stkItm.Stk_3SLPriceCC = !string.IsNullOrEmpty(txtPrice3CC.Text) ? Convert.ToDouble(txtPrice3CC.Text) : (double?)null;
                stkItm.Stk_Qty1stPrice = !string.IsNullOrEmpty(txtDiscPrice2.Text) ? Convert.ToDouble(txtDiscPrice2.Text) : (double?)null;
                stkItm.Stk_Qty2stPrice = !string.IsNullOrEmpty(txtDiscPrice3.Text) ? Convert.ToDouble(txtDiscPrice3.Text) : (double?)null;
                stkItm.Stk_SLCoef = !string.IsNullOrEmpty(txtSalesCoefVal.Text) ? Convert.ToDouble(txtSalesCoefVal.Text) : (double?)null;
                //stkItm.Stk_LStatCoef = rbLstStsCoefValC.Checked ? "C" : "U";
                stkItm.Stk_UserCoef = !string.IsNullOrEmpty(txtUserCode.Text) ? txtUserCode.Text : null;
                stkItm.Stk_DiscWh = !string.IsNullOrEmpty(txtDiscWH.Text) ? Convert.ToDouble(txtDiscWH.Text) : (double?)null;
                stkItm.Stk_DiscRetail = !string.IsNullOrEmpty(txtDiscRetail.Text) ? Convert.ToDouble(txtDiscRetail.Text) : (double?)null;
                stkItm.stk_potchangeDate = !string.IsNullOrEmpty(txtPotChangeDate.Text) ? Convert.ToDateTime(txtPotChangeDate.Text) : (DateTime?)null;

                //if (lcFormMode == ClsCommon.FormMode.Add)
                //{
                WarehousesController WarehousesController = new WarehousesController();
                List<string> whCodes = WarehousesController.GetWarehousesList().Select(w => w.Wh_Code).ToList();
                stkItm.Stk_StockQtyList = new List<StockQty>();
                foreach (string WH in whCodes)
                {
                    StockQty stockQty = new StockQty()
                    {
                        Qty_Stkid = stkItm.Stk_StkId,
                        Qty_WH = Convert.ToInt32(WH),
                        Qty_Min = Convert.ToInt32(dtQuantityDetails.Select($" [Wh Code] = '{WH}'")[0]["Minimum Qty"] == DBNull.Value ? 0 : dtQuantityDetails.Select($" [Wh Code] = '{WH}'")[0]["Minimum Qty"]),
                        Qty_Max = Convert.ToInt32(dtQuantityDetails.Select($" [Wh Code] = '{WH}'")[0]["Maximum Qty"] == DBNull.Value ? 0 : dtQuantityDetails.Select($" [Wh Code] = '{WH}'")[0]["Maximum Qty"])

                    };
                    stkItm.Stk_StockQtyList.Add(stockQty);

                }
                //}
                //else
                //{
                //    StockQtyController qtyController = new StockQtyController();
                //    stkItm.Stk_StockQtyList = qtyController.GetStockQtyList(stkItm.Stk_StkId);
                //}

                StockQtyExpiryController qtyExpController = new StockQtyExpiryController();
                stkItm.Stk_StockQtyExpiryList = qtyExpController.GetStockQtyExpiry(stkItm.Stk_StkId);

                // FILE PRICE
                stkItm.stk_FilePrice = new StockFilePrice()
                {
                    Sfp_stkId = stkItm.Stk_StkId,
                    Sfp_Sup = cntSupplier.Code,
                    Sfp_PotUnitPrice = !string.IsNullOrEmpty(txtUnitPrice.Text) ? Convert.ToDouble(txtUnitPrice.Text) : (double?)null,
                    Sfp_PotItemDiscount = !string.IsNullOrEmpty(txtUnitDiscPotCost.Text) ? Convert.ToDouble(txtUnitDiscPotCost.Text) : (double?)null,
                    Sfp_PotCurr = rbLC.Checked ? ClsConfig.gbCurr1 : rbCC.Text,
                    Sfp_PotFollowInvDisc = chkFollow.Checked ? "Y" : "N",
                    Sfp_PotInvDiscount = !string.IsNullOrEmpty(txtAddInvDisc.Text) ? Convert.ToDouble(txtAddInvDisc.Text) : (double?)null,
                    Sfp_PotPurQty = !string.IsNullOrEmpty(txtPurchaseQty.Text) ? Convert.ToDouble(txtPurchaseQty.Text) : (double?)null,
                    Sfp_PotPromQty = !string.IsNullOrEmpty(txtPromotionQty.Text) ? Convert.ToDouble(txtPromotionQty.Text) : (double?)null,
                    Sfp_PotFreeQty = !string.IsNullOrEmpty(txtFreeQty.Text) ? Convert.ToDouble(txtFreeQty.Text) : (double?)null,
                    Sfp_PotExtraQty = !string.IsNullOrEmpty(txtExtraQty.Text) ? Convert.ToDouble(txtExtraQty.Text) : (double?)null,
                    Sfp_PotContractQty = !string.IsNullOrEmpty(txtContractQty.Text) ? Convert.ToDouble(txtContractQty.Text) : (double?)null,
                    Sfp_PotRemarks = !string.IsNullOrEmpty(txtRemarksFP.Text) ? txtRemarksFP.Text : null,
                    Sfp_PotUseItmDisc = chkUseItmDisc.Checked ? "Y" : "N",
                    Sfp_PotUseInvDisc = chkUseInvDisc.Checked ? "Y" : "N",
                    Sfp_PotUsePromQty = chkUsePromQty.Checked ? "Y" : "N",
                    Sfp_LastCostParam = rbParamsLastCost1.Checked ? "1" : rbParamsLastCost2.Checked ? "2" : "3",
                    Sfp_LCOrginalCost = !string.IsNullOrEmpty(txtOriginalCost.Text) ? Convert.ToDouble(txtOriginalCost.Text) : (double?)null,
                    Sfp_LCItmDiscount = !string.IsNullOrEmpty(txtUnitDiscLastCost.Text) ? Convert.ToDouble(txtUnitDiscLastCost.Text) : (double?)null,
                    Sfp_LCInvDiscount = !string.IsNullOrEmpty(txtAddDiscLastCost.Text) ? Convert.ToDouble(txtAddDiscLastCost.Text) : (double?)null,
                    Sfp_LCPurQty = !string.IsNullOrEmpty(txtPurchaseQtyLastCost.Text) ? Convert.ToDouble(txtPurchaseQtyLastCost.Text) : (double?)null,
                    Sfp_LCPromQty = !string.IsNullOrEmpty(txtPromotionQtyLastCost.Text) ? Convert.ToDouble(txtPromotionQtyLastCost.Text) : (double?)null,
                    Sfp_LCFreeQty = !string.IsNullOrEmpty(txtFreeQtyLastCost.Text) ? Convert.ToDouble(txtFreeQtyLastCost.Text) : (double?)null,
                    Sfp_LCExtraQty = !string.IsNullOrEmpty(txtExtraQtyLastCost.Text) ? Convert.ToDouble(txtExtraQtyLastCost.Text) : (double?)null,
                    Sfp_ContractQty = !string.IsNullOrEmpty(txtContractQtyLastCost.Text) ? Convert.ToDouble(txtContractQtyLastCost.Text) : (double?)null,
                    Sfp_InvCurr = !string.IsNullOrEmpty(cntCurrency2.Code) ? cntCurrency2.Code : null,
                    Sfp_PINo = !string.IsNullOrEmpty(txtPIIntNo.Text) ? txtPIIntNo.Text : null,
                    Sfp_SupInvNoOrdNo = !string.IsNullOrEmpty(txtSuppInvOrderNo.Text) ? txtSuppInvOrderNo.Text : null,
                    Sfp_Image = imgItmPhoto.pctPicture.Image != null ? imgItmPhoto.pctPicture.GetByteArray() : null
                };

                //YTD
                //stkItm.Stk_YTDQtyIn = !string.IsNullOrEmpty(txtYTDQtyIN.Text) ? Convert.ToDouble(txtYTDQtyIN.Text) : (double?)null;
                //stkItm.Stk_YTDQtyOut = !string.IsNullOrEmpty(txtYTDQtyOUT.Text) ? Convert.ToDouble(txtYTDQtyOUT.Text) : (double?)null;
                //stkItm.Stk_YTDAmtInLL = !string.IsNullOrEmpty(txtYTDLLIn.Text) ? Convert.ToDouble(txtYTDLLIn.Text) : (double?)null;
                //stkItm.Stk_YTDAmtOutLL = !string.IsNullOrEmpty(txtYTDLLOut.Text) ? Convert.ToDouble(txtYTDLLOut.Text) : (double?)null;
                //stkItm.Stk_YTDAmtInUSD = !string.IsNullOrEmpty(txtYTDUsdIn.Text) ? Convert.ToDouble(txtYTDUsdIn.Text) : (double?)null;
                //stkItm.Stk_YTDAmtOutUSD = !string.IsNullOrEmpty(txtYTDUsdOut.Text) ? Convert.ToDouble(txtYTDUsdOut.Text) : (double?)null;

                // O/H
                stkItm.Stk_OHStockAdd = new OHStockAdd()
                {
                    Soh_Stkid = stkItm.Stk_StkId,
                    Soh_CustPer = !string.IsNullOrEmpty(txtCustomsPerc.Text) ? Convert.ToDouble(txtCustomsPerc.Text) : (double?)null,
                    Soh_FixedOHPer = !string.IsNullOrEmpty(txtOHPerc.Text) ? Convert.ToDouble(txtOHPerc.Text) : (double?)null,
                    Soh_AppCostCoef = !string.IsNullOrEmpty(txtCostCoef.Text) ? Convert.ToDouble(txtCostCoef.Text) : (double?)null,
                    Soh_Remarks = !string.IsNullOrEmpty(txtRemarks.Text) ? txtRemarks.Text : null,
                };

                //SUPPLIER REFERENCE
                if (FpnlSuppRef.Controls.Count > 0)
                {
                    int countSRef = 0;
                    foreach (cntSupplierReference cnt in FpnlSuppRef.Controls)
                    {
                        if (!string.IsNullOrEmpty(cnt.cntSuppliers1.Code) && !string.IsNullOrEmpty(cnt.txtReference.Text))
                        {
                            countSRef++;
                            if (countSRef == 1) stkItm.Stk_AddenSupplierList = new List<AddenSupplier>();
                            stkItm.Stk_AddenSupplierList.Add(new AddenSupplier()
                            {
                                Delete = cnt.Delete,
                                Ads_StkId = stkItm.Stk_StkId,
                                Ads_Supplier = cnt.cntSuppliers1.Code,
                                Ads_SuppRef = cnt.txtReference.Text,
                                Ads_AllowReOrder = cnt.chkReorder.Checked ? "Y" : "N",

                            });
                        }
                    }
                }


                // STOCK UNIT
                if (flpStockUnits.Controls.Count > 0)
                {
                    int count = 0;
                    foreach (cntStockUnit cnt in flpStockUnits.Controls)
                    {
                        if (!string.IsNullOrEmpty(cnt.cntUnit.Code))
                        {
                            int countBarcodes = 0;

                            if (count == 0)
                            {
                                stkItm.stk_StockUnitList = new List<StockUnit>();
                                stkItm.stk_changeItmPriceList = new List<ChangeItmPrice>();
                            };
                            StockUnit stockUnit = new StockUnit()
                            {
                                Delete = cnt.Delete,

                                SUnit_StkId = stkItm.Stk_StkId,
                                SUnit_Unit = cnt.cntUnit.Code,
                                SUnit_ParentUnit = cnt.cntParentUnit.Code,
                                SUnit_SpecDesc = cnt.txtDescription.Text,
                                SUnit_Packing = !string.IsNullOrEmpty(cnt.txtPacking.Text) ? Convert.ToDouble(cnt.txtPacking.Text) : (double?)null,
                                SUnit_PackingBasic = !string.IsNullOrEmpty(cnt.txtPackingBasic.Text) ? Convert.ToDouble(cnt.txtPackingBasic.Text) : (double?)null,
                                SUnit_Discount = !string.IsNullOrEmpty(cnt.txtDiscount.Text) ? Convert.ToDouble(cnt.txtDiscount.Text) : (double?)null,
                                SUnit_1SlPriceLC = !string.IsNullOrEmpty(cnt.txtPrice1LC.Text) ? Convert.ToDouble(cnt.txtPrice1LC.Text) : (double?)null,
                                SUnit_1SlPriceUSD = !string.IsNullOrEmpty(cnt.txtPrice1CC.Text) ? Convert.ToDouble(cnt.txtPrice1CC.Text) : (double?)null,
                                SUnit_Qty1stPrice = !string.IsNullOrEmpty(cnt.txtP1P3Disc.Text) ? Convert.ToDouble(cnt.txtP1P3Disc.Text) : (double?)null,
                                SUnit_2SLPriceLC = !string.IsNullOrEmpty(cnt.txtPrice3LC.Text) ? Convert.ToDouble(cnt.txtPrice3LC.Text) : (double?)null,
                                SUnit_2SLPriceUSD = !string.IsNullOrEmpty(cnt.txtPrice3CC.Text) ? Convert.ToDouble(cnt.txtPrice3CC.Text) : (double?)null,
                                SUnit_Qty2stPrice = !string.IsNullOrEmpty(cnt.txtP1P2Disc.Text) ? Convert.ToDouble(cnt.txtP1P2Disc.Text) : (double?)null,
                                SUnit_3SLPriceLC = !string.IsNullOrEmpty(cnt.txtPrice2LC.Text) ? Convert.ToDouble(cnt.txtPrice2LC.Text) : (double?)null,
                                SUnit_3SLPriceUSD = !string.IsNullOrEmpty(cnt.txtPrice2CC.Text) ? Convert.ToDouble(cnt.txtPrice2CC.Text) : (double?)null,
                                SUnit_Volume = !string.IsNullOrEmpty(cnt.txtVolume.Text) ? Convert.ToDouble(cnt.txtVolume.Text) : (double?)null,
                                SUnit_Weight = !string.IsNullOrEmpty(cnt.txtWeight.Text) ? Convert.ToDouble(cnt.txtWeight.Text) : (double?)null,
                                SUnit_Active = cnt.chkActiveUnit.Checked ? "Y" : "N",
                            };


                            if ((lcFormMode == ClsCommon.FormMode.Modify
                                && ((txtSalesCoefVal.Text != null && txtSalesCoefVal.Text != txtSalesCoefVal.LastText)
                                || (!string.IsNullOrEmpty(cnt.txtPrice1LC.Text) && cnt.txtPrice1LC.Text != cnt.txtPrice1LC.LastText)
                                || (!string.IsNullOrEmpty(cnt.txtPrice1CC.Text) && cnt.txtPrice1CC.Text != cnt.txtPrice1CC.LastText)
                                || (!string.IsNullOrEmpty(cnt.txtP1P3Disc.Text) && cnt.txtP1P3Disc.Text != cnt.txtP1P3Disc.LastText)
                                || (!string.IsNullOrEmpty(cnt.txtPrice3LC.Text) && cnt.txtPrice3LC.Text != cnt.txtPrice3LC.LastText)
                                || (!string.IsNullOrEmpty(cnt.txtPrice3CC.Text) && cnt.txtPrice3CC.Text != cnt.txtPrice3CC.LastText)
                                || (!string.IsNullOrEmpty(cnt.txtP1P2Disc.Text) && cnt.txtP1P2Disc.Text != cnt.txtP1P2Disc.LastText)
                                || (!string.IsNullOrEmpty(cnt.txtPrice2LC.Text) && cnt.txtPrice2LC.Text != cnt.txtPrice2LC.LastText)
                                || (!string.IsNullOrEmpty(cnt.txtPrice2CC.Text) && cnt.txtPrice2CC.Text != cnt.txtPrice2CC.LastText)
                                ))
                                ||
                               lcFormMode == ClsCommon.FormMode.Add
                               ||
                               lcFormMode == ClsCommon.FormMode.Delete
                               )
                            {

                                ChangeItmPrice changeItmPrice = new ChangeItmPrice()
                                {
                                    Cip_StkId = stkItm.Stk_StkId,
                                    Cip_Type = "N",
                                    Cip_Source = "S",
                                    Cip_1stSPLL = !string.IsNullOrEmpty(cnt.txtPrice1LC.Text) ? Convert.ToDouble(cnt.txtPrice1LC.Text) : (double?)null,
                                    Cip_1stSP = !string.IsNullOrEmpty(cnt.txtPrice1CC.Text) ? Convert.ToDouble(cnt.txtPrice1CC.Text) : (double?)null,
                                    Cip_1stQtyPrice = !string.IsNullOrEmpty(cnt.txtP1P3Disc.Text) ? Convert.ToDouble(cnt.txtP1P3Disc.Text) : (double?)null,
                                    Cip_2dSPLL = !string.IsNullOrEmpty(cnt.txtPrice3LC.Text) ? Convert.ToDouble(cnt.txtPrice3LC.Text) : (double?)null,
                                    Cip_2dSP = !string.IsNullOrEmpty(cnt.txtPrice3CC.Text) ? Convert.ToDouble(cnt.txtPrice3CC.Text) : (double?)null,
                                    Cip_2dQtyPrice = !string.IsNullOrEmpty(cnt.txtP1P2Disc.Text) ? Convert.ToDouble(cnt.txtP1P2Disc.Text) : (double?)null,
                                    Cip_3dSPLL = !string.IsNullOrEmpty(cnt.txtPrice2LC.Text) ? Convert.ToDouble(cnt.txtPrice2LC.Text) : (double?)null,
                                    Cip_3dSP = !string.IsNullOrEmpty(cnt.txtPrice2CC.Text) ? Convert.ToDouble(cnt.txtPrice2CC.Text) : (double?)null,

                                    Cip_Unit = cnt.cntUnit.Code,
                                    Cip_SLCoef = !string.IsNullOrEmpty(txtSalesCoefVal.Text) ? Convert.ToDouble(txtSalesCoefVal.Text) : (double?)null
                                };
                                stkItm.stk_changeItmPriceList.Add(changeItmPrice);

                            }


                            stkItm.stk_StockUnitList.Add(stockUnit);

                            if (string.IsNullOrEmpty(stkItm.Stk_UnitCode))
                            { stkItm.Stk_UnitCode = cnt.chkBaseUnit.Checked ? cnt.cntUnit.Code : null; }

                            if (string.IsNullOrEmpty(stkItm.Stk_Packing))
                            { stkItm.Stk_Packing = cnt.chkDefaultPIUnit.Checked ? cnt.cntUnit.Code : null; }


                            foreach (cntBarcode barcode in cnt.flpSynBarcode.Controls)
                            {
                                if (!string.IsNullOrEmpty(barcode.txtBarcode1.Text))
                                {
                                    countBarcodes++;
                                    if (countBarcodes == 1) stkItm.stk_StockUnitList[count].Stk_BarcodeList = new List<BarcodeSyn>();
                                    stkItm.stk_StockUnitList[count].Stk_BarcodeList.Add(new BarcodeSyn()
                                    {
                                        Delete = cnt.Delete ? cnt.Delete : barcode.Delete,
                                        Bsy_Stkid = stkItm.Stk_StkId,
                                        BSy_UnitCode = cnt.cntUnit.Code,
                                        Bsy_Barcode = barcode.txtBarcode1.Text,
                                    });
                                }
                            }

                            count++;

                        }
                    }
                }

                //Groups And Dep
                if (cntDepGrpSub.cntDepartment != null && cntDepGrpSub.cntDepartment.DepartmentRecID != -1 && cntDepGrpSub.cntDepartment.DepartmentRecID != 0)
                {
                    stkItm.Stk_LinkStockDept = new LinkStockDept()
                    {
                        lsd_StkRecID = stkItm.Stk_StkId,
                        lsd_DeptRecID = cntDepGrpSub.cntDepartment.DepartmentRecID != -1 && cntDepGrpSub.cntDepartment.DepartmentRecID != 0 ? cntDepGrpSub.cntDepartment.DepartmentRecID : (int?)null,
                        lsd_GrpRecID = cntDepGrpSub.cntGroup.GroupRecID != -1 && cntDepGrpSub.cntGroup.GroupRecID != 0 ? cntDepGrpSub.cntGroup.GroupRecID : (int?)null,
                        lsd_SG1RecID = cntDepGrpSub.cntSubGroup1.SubGroup1RecID != -1 && cntDepGrpSub.cntSubGroup1.SubGroup1RecID != 0 ? cntDepGrpSub.cntSubGroup1.SubGroup1RecID : (int?)null,
                        lsd_SG2RecID = cntDepGrpSub.cntSubGroup2.Sub2RecID != -1 && cntDepGrpSub.cntSubGroup2.Sub2RecID != 0 ? cntDepGrpSub.cntSubGroup2.Sub2RecID : (int?)null,
                        lsd_SG3RecID = cntDepGrpSub.cntSubGroup3.Sub3RecID != -1 && cntDepGrpSub.cntSubGroup3.Sub3RecID != 0 ? cntDepGrpSub.cntSubGroup3.Sub3RecID : (int?)null,
                        lsd_SG4Recid = cntDepGrpSub.cntSubGroup4.Sub4RecID != -1 && cntDepGrpSub.cntSubGroup4.Sub4RecID != 0 ? cntDepGrpSub.cntSubGroup4.Sub4RecID : (int?)null,
                    };
                }

                //stkItm.ClsDynamicFields = clsDynamicFields;


                if (lcFormMode == ClsCommon.FormMode.Add)
                {
                    SaveOperation.InsertStockItems(stkItm);

                }
                else if (lcFormMode == ClsCommon.FormMode.Modify)
                {
                    SaveOperation.UpdateStockItems(stkItm);

                    if (OpenedFromAnotherForm)
                    {
                        this.DialogResult = DialogResult.OK;
                    
                    }

                }
                else if (lcFormMode == ClsCommon.FormMode.Delete)
                {
                    SaveOperation.DeleteStockItems(stkItm);
                }

                //Temporary done outside the transaction
                using (System.Data.SqlClient.SqlConnection sqlConnection = new System.Data.SqlClient.SqlConnection(ClsConfig.SQLConnectionString))
                {
                    sqlConnection.Open();

                    using (System.Data.SqlClient.SqlCommand sqlCommand = new System.Data.SqlClient.SqlCommand("", sqlConnection))
                    {
                        clsDynamicFields.SaveDynamicFields(sqlCommand);
                    }

                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        #endregion


        #region Quantity Details Grid

        private DataTable dtQuantityDetails;

        private void LoadQuantityDetailsGrid(int stockID)
        {
            try
            {

                if (dtQuantityDetails != null)
                {
                    dtQuantityDetails.Rows.Clear();
                    dtQuantityDetails.Dispose();
                    dtQuantityDetails = null;
                }

                StockItemsViews stockItemsViews = new StockItemsViews();
                dtQuantityDetails = stockItemsViews.GetQuantityDetails(stockID);

                grdQuantityDetails.DataSource = dtQuantityDetails;

                grdQuantityDetails.AllowAddNew = false;
                grdQuantityDetails.AllowDelete = false;

                grdQuantityDetails.ColumnFooters = true;

                grdQuantityDetails.RowHeight = 23;
                grdQuantityDetails.Splits[0].ColumnCaptionHeight = 40;

                grdQuantityDetails.Splits[0].DisplayColumns["WH Code"].DataColumn.Caption = "WH Code";
                grdQuantityDetails.Splits[0].DisplayColumns["WH Code"].Style.HorizontalAlignment = C1.Win.C1TrueDBGrid.AlignHorzEnum.Center;
                grdQuantityDetails.Splits[0].DisplayColumns["WH Code"].Locked = true;
                grdQuantityDetails.Splits[0].DisplayColumns["WH Code"].Width = 100;
                grdQuantityDetails.Splits[0].DisplayColumns["WH Code"].Merge = C1.Win.C1TrueDBGrid.ColumnMergeEnum.Free;

                grdQuantityDetails.Splits[0].DisplayColumns["WH Description"].DataColumn.Caption = "WH Description";
                grdQuantityDetails.Splits[0].DisplayColumns["WH Description"].Style.HorizontalAlignment = C1.Win.C1TrueDBGrid.AlignHorzEnum.Near;
                grdQuantityDetails.Splits[0].DisplayColumns["WH Description"].Locked = true;
                grdQuantityDetails.Splits[0].DisplayColumns["WH Description"].Width = 0; //Responsive
                grdQuantityDetails.Splits[0].DisplayColumns["WH Description"].Merge = C1.Win.C1TrueDBGrid.ColumnMergeEnum.Free;

                grdQuantityDetails.Splits[0].DisplayColumns["BBF Qty"].DataColumn.Caption = "BBF Qty";
                grdQuantityDetails.Splits[0].DisplayColumns["BBF Qty"].Style.HorizontalAlignment = C1.Win.C1TrueDBGrid.AlignHorzEnum.Center;
                grdQuantityDetails.Splits[0].DisplayColumns["BBF Qty"].Locked = true;
                grdQuantityDetails.Splits[0].DisplayColumns["BBF Qty"].Width = 100;
                grdQuantityDetails.Splits[0].DisplayColumns["BBF Qty"].DataColumn.NumberFormat = ClsConfig.locationDirectives.Ld_NoOfDecQtyFormat;
                grdQuantityDetails.Splits[0].DisplayColumns["BBF Qty"].Merge = C1.Win.C1TrueDBGrid.ColumnMergeEnum.Free;
                grdQuantityDetails.Splits[0].DisplayColumns["BBF Qty"].FooterStyle.HorizontalAlignment = C1.Win.C1TrueDBGrid.AlignHorzEnum.Center;

                grdQuantityDetails.Splits[0].DisplayColumns["Available Qty"].DataColumn.Caption = "Available Qty";
                grdQuantityDetails.Splits[0].DisplayColumns["Available Qty"].Style.HorizontalAlignment = C1.Win.C1TrueDBGrid.AlignHorzEnum.Center;
                grdQuantityDetails.Splits[0].DisplayColumns["Available Qty"].Locked = true;
                grdQuantityDetails.Splits[0].DisplayColumns["Available Qty"].Width = 100;
                grdQuantityDetails.Splits[0].DisplayColumns["Available Qty"].DataColumn.NumberFormat = ClsConfig.locationDirectives.Ld_NoOfDecQtyFormat;
                grdQuantityDetails.Splits[0].DisplayColumns["Available Qty"].Merge = C1.Win.C1TrueDBGrid.ColumnMergeEnum.Free;
                grdQuantityDetails.Splits[0].DisplayColumns["Available Qty"].FooterStyle.HorizontalAlignment = C1.Win.C1TrueDBGrid.AlignHorzEnum.Center;

                grdQuantityDetails.Splits[0].DisplayColumns["Return Qty"].DataColumn.Caption = "Qty to be Return";
                grdQuantityDetails.Splits[0].DisplayColumns["Return Qty"].Style.HorizontalAlignment = C1.Win.C1TrueDBGrid.AlignHorzEnum.Center;
                grdQuantityDetails.Splits[0].DisplayColumns["Return Qty"].Locked = true;
                grdQuantityDetails.Splits[0].DisplayColumns["Return Qty"].Width = 100;
                grdQuantityDetails.Splits[0].DisplayColumns["Return Qty"].DataColumn.NumberFormat = ClsConfig.locationDirectives.Ld_NoOfDecQtyFormat;
                grdQuantityDetails.Splits[0].DisplayColumns["Return Qty"].Merge = C1.Win.C1TrueDBGrid.ColumnMergeEnum.Free;
                grdQuantityDetails.Splits[0].DisplayColumns["Return Qty"].FooterStyle.HorizontalAlignment = C1.Win.C1TrueDBGrid.AlignHorzEnum.Center;

                grdQuantityDetails.Splits[0].DisplayColumns["Minimum Qty"].DataColumn.Caption = "Minimum Qty";
                grdQuantityDetails.Splits[0].DisplayColumns["Minimum Qty"].Style.HorizontalAlignment = C1.Win.C1TrueDBGrid.AlignHorzEnum.Center;
                grdQuantityDetails.Splits[0].DisplayColumns["Minimum Qty"].Locked = false;
                grdQuantityDetails.Splits[0].DisplayColumns["Minimum Qty"].AllowFocus = true;
                grdQuantityDetails.Splits[0].DisplayColumns["Minimum Qty"].Width = 100;
                grdQuantityDetails.Splits[0].DisplayColumns["Minimum Qty"].DataColumn.NumberFormat = ClsConfig.locationDirectives.Ld_NoOfDecQtyFormat;
                grdQuantityDetails.Splits[0].DisplayColumns["Minimum Qty"].FooterStyle.HorizontalAlignment = C1.Win.C1TrueDBGrid.AlignHorzEnum.Far;

                grdQuantityDetails.Splits[0].DisplayColumns["Maximum Qty"].DataColumn.Caption = "Maximum Qty";
                grdQuantityDetails.Splits[0].DisplayColumns["Maximum Qty"].Style.HorizontalAlignment = C1.Win.C1TrueDBGrid.AlignHorzEnum.Center;
                grdQuantityDetails.Splits[0].DisplayColumns["Maximum Qty"].Locked = false;
                grdQuantityDetails.Splits[0].DisplayColumns["Minimum Qty"].AllowFocus = true;
                grdQuantityDetails.Splits[0].DisplayColumns["Maximum Qty"].Width = 100;
                grdQuantityDetails.Splits[0].DisplayColumns["Maximum Qty"].DataColumn.NumberFormat = ClsConfig.locationDirectives.Ld_NoOfDecQtyFormat;
                grdQuantityDetails.Splits[0].DisplayColumns["Maximum Qty"].FooterStyle.HorizontalAlignment = C1.Win.C1TrueDBGrid.AlignHorzEnum.Far;

                grdQuantityDetails.Splits[0].DisplayColumns["Expiry Date"].DataColumn.Caption = "Expiry Date";
                grdQuantityDetails.Splits[0].DisplayColumns["Expiry Date"].Style.HorizontalAlignment = C1.Win.C1TrueDBGrid.AlignHorzEnum.Center;
                grdQuantityDetails.Splits[0].DisplayColumns["Expiry Date"].Locked = true;
                grdQuantityDetails.Splits[0].DisplayColumns["Expiry Date"].Width = 0; //Responsive

                grdQuantityDetails.Splits[0].DisplayColumns["Lot Number"].DataColumn.Caption = "Lot Number";
                grdQuantityDetails.Splits[0].DisplayColumns["Lot Number"].Style.HorizontalAlignment = C1.Win.C1TrueDBGrid.AlignHorzEnum.Center;
                grdQuantityDetails.Splits[0].DisplayColumns["Lot Number"].Locked = true;
                grdQuantityDetails.Splits[0].DisplayColumns["Lot Number"].Width = 0; //Responsive

                grdQuantityDetails.Splits[0].DisplayColumns["Exp Lot BBF Qty"].Visible = false;
                grdQuantityDetails.Splits[0].DisplayColumns["Exp Lot BBF Qty"].AllowSizing = false;
                grdQuantityDetails.Splits[0].DisplayColumns["Exp Lot BBF Qty"].Width = 0; //ALWAYS

                grdQuantityDetails.Splits[0].DisplayColumns["Exp Lot Available Qty"].DataColumn.Caption = "Exp Lot Available Qty";
                grdQuantityDetails.Splits[0].DisplayColumns["Exp Lot Available Qty"].Style.HorizontalAlignment = C1.Win.C1TrueDBGrid.AlignHorzEnum.Center;
                grdQuantityDetails.Splits[0].DisplayColumns["Exp Lot Available Qty"].Locked = true;
                grdQuantityDetails.Splits[0].DisplayColumns["Exp Lot Available Qty"].Width = 100;
                grdQuantityDetails.Splits[0].DisplayColumns["Exp Lot Available Qty"].DataColumn.NumberFormat = ClsConfig.locationDirectives.Ld_NoOfDecQtyFormat;
                grdQuantityDetails.Splits[0].DisplayColumns["Exp Lot Available Qty"].Merge = C1.Win.C1TrueDBGrid.ColumnMergeEnum.Free;

                grdQuantityDetails.Splits[0].DisplayColumns["Exp Lot Return Qty"].DataColumn.Caption = "Exp Lot Return Qty";
                grdQuantityDetails.Splits[0].DisplayColumns["Exp Lot Return Qty"].Style.HorizontalAlignment = C1.Win.C1TrueDBGrid.AlignHorzEnum.Center;
                grdQuantityDetails.Splits[0].DisplayColumns["Exp Lot Return Qty"].Locked = true;
                grdQuantityDetails.Splits[0].DisplayColumns["Exp Lot Return Qty"].Width = 100;
                grdQuantityDetails.Splits[0].DisplayColumns["Exp Lot Return Qty"].DataColumn.NumberFormat = ClsConfig.locationDirectives.Ld_NoOfDecQtyFormat;
                grdQuantityDetails.Splits[0].DisplayColumns["Exp Lot Return Qty"].Merge = C1.Win.C1TrueDBGrid.ColumnMergeEnum.Free;

                grdQuantityDetails.AutoFixGrid();

                ResponsiveQuantityDetailsGrid();
                CalculateQuantityDetailsGridTotals();

            }
            catch (Exception)
            {
                throw;
            }
        }

        private void ResponsiveQuantityDetailsGrid()
        {
            try
            {

                if (dtQuantityDetails != null && grdQuantityDetails.DataSource != null)
                {

                    int totalColumnsWidth = 0;
                    int RemainingWidth = 0;

                    grdQuantityDetails.Splits[0].DisplayColumns["WH Description"].Width = 0;
                    grdQuantityDetails.Splits[0].DisplayColumns["Expiry Date"].Width = 0;
                    grdQuantityDetails.Splits[0].DisplayColumns["Lot Number"].Width = 0;

                    foreach (C1.Win.C1TrueDBGrid.C1DisplayColumn column in grdQuantityDetails.Splits[0].DisplayColumns)
                    {
                        totalColumnsWidth += column.Width;
                    }

                    RemainingWidth = grdQuantityDetails.Width - totalColumnsWidth - 50;

                    grdQuantityDetails.Splits[0].DisplayColumns["WH Description"].Width = (int)(RemainingWidth * 0.6);
                    grdQuantityDetails.Splits[0].DisplayColumns["Expiry Date"].Width = (int)(RemainingWidth * 0.2);
                    grdQuantityDetails.Splits[0].DisplayColumns["Lot Number"].Width = (int)(RemainingWidth * 0.2);

                }

            }
            catch (Exception)
            {

            }
        }

        private void CalculateQuantityDetailsGridTotals()
        {
            try
            {

                object objBBF, objAVL, objRET;

                objBBF = dtQuantityDetails.Compute("sum([BBF Qty])", "1=1");
                if (objBBF == DBNull.Value) { objBBF = 0; }

                objAVL = dtQuantityDetails.Compute("sum([Available Qty])", "1=1");
                if (objAVL == DBNull.Value) { objAVL = 0; }

                objRET = dtQuantityDetails.Compute("sum([Return Qty])", "1=1");
                if (objRET == DBNull.Value) { objRET = 0; }

                grdQuantityDetails.Splits[0].DisplayColumns["BBF Qty"].DataColumn.FooterText = Convert.ToDouble(objBBF).ToString(ClsConfig.locationDirectives.Ld_NoOfDecQtyFormat);
                grdQuantityDetails.Splits[0].DisplayColumns["Available Qty"].DataColumn.FooterText = Convert.ToDouble(objAVL).ToString(ClsConfig.locationDirectives.Ld_NoOfDecQtyFormat);
                grdQuantityDetails.Splits[0].DisplayColumns["Return Qty"].DataColumn.FooterText = Convert.ToDouble(objRET).ToString(ClsConfig.locationDirectives.Ld_NoOfDecQtyFormat);



                object objMin, objMax;

                objMin = dtQuantityDetails.Compute("sum([Minimum Qty])", "1=1");
                if (objMin == DBNull.Value) { objMin = 0; }

                objMax = dtQuantityDetails.Compute("sum([MAximum Qty])", "1=1");
                if (objMax == DBNull.Value) { objMax = 0; }

                txtMinQty.Text = Convert.ToDouble(objMin).ToString(ClsConfig.locationDirectives.Ld_NoOfDecQtyFormat);
                txtMaxQty.Text = Convert.ToDouble(objMax).ToString(ClsConfig.locationDirectives.Ld_NoOfDecQtyFormat);

            }
            catch (Exception)
            {
                throw;
            }
        }

        private void grdQuantityDetails_AfterColUpdate(object sender, C1.Win.C1TrueDBGrid.ColEventArgs e)
        {
            try
            {
                CalculateQuantityDetailsGridTotals();
            }
            catch (Exception ex)
            {

                ClsCommon.ShowException(StepName, ex);
            }

        }

        private void grdQuantityDetails_SizeChanged(object sender, EventArgs e)
        {
            try
            {
                ResponsiveQuantityDetailsGrid();
            }
            catch (Exception)
            {

            }
        }

        private void grdQuantityDetails_BeforeColUpdate(object sender, C1.Win.C1TrueDBGrid.BeforeColUpdateEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(grdQuantityDetails.Columns["Minimum Qty"].Value?.ToString()) &&
                    double.TryParse(grdQuantityDetails.Columns["Minimum Qty"].Value.ToString(), out double minValue))
                {
                    if (minValue < 0)
                    {
                        ClsCommon.ShowNotification(this, "Value Cannot Be Negative!");
                        e.Cancel = true;
                    }
                }

                if (!string.IsNullOrEmpty(grdQuantityDetails.Columns["Maximum Qty"].Value?.ToString()) &&
                         double.TryParse(grdQuantityDetails.Columns["Maximum Qty"].Value.ToString(), out double maxValue))
                {
                    if (maxValue < 0)
                    {
                        ClsCommon.ShowNotification(this, "Value Cannot Be Negative!");
                        e.Cancel = true;
                    }
                }

                if (!ValidateQuantityDetails())
                {
                    if(grdQuantityDetails.Columns[grdQuantityDetails.Col].DataField == "Maximum Qty")
                    {
                        grdQuantityDetails.Columns["Maximum Qty"].Value = 0;
                    }
                    else
                    {
                        grdQuantityDetails.Columns["Minimum Qty"].Value = 0;
                    }
                    e.Cancel = true;
                    return;
                }



            }
            catch (Exception ex)
            {
                ClsCommon.ShowException(StepName, ex);
            }
        }

        private bool ValidateQuantityDetails()
        {

            try
            {
                string minText = grdQuantityDetails.Columns["Minimum Qty"].Text.Trim();
                string maxText = grdQuantityDetails.Columns["Maximum Qty"].Text.Trim();

                if (!string.IsNullOrEmpty(minText) &&
                    !string.IsNullOrEmpty(maxText) &&
                    double.TryParse(minText, out double minQty) &&
                    double.TryParse(maxText, out double maxQty) &&
                    maxQty != 0 &&
                    minQty > maxQty)
                {
                    ClsCommon.ShowNotification(
                        this,
                        "Minimum Qty cannot be greater than Maximum Qty!"
                    );

                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {

                ClsCommon.ShowException(StepName, ex);
                return false;
            }

        }

        private void tabForm_Selecting(object sender, TabControlCancelEventArgs e)
        {
            try
            {
                if (!ValidateQuantityDetails())
                {
                    e.Cancel = true;
                }
            }
            catch (Exception ex)
            {

                ClsCommon.ShowException(StepName, ex);
            }
        }

        #endregion


        #region Validate Units

        public string ValidateUnits()
        {
            DataTable dtUnits;
            string BasicUnit = "";

            try
            {

                dtUnits = new DataTable();

                dtUnits.Columns.Add("Unit", typeof(System.String));
                dtUnits.Columns.Add("UnitDescription", typeof(System.String));
                dtUnits.Columns.Add("IsBasicUnit", typeof(System.Boolean));
                dtUnits.Columns.Add("IsDefaultPIUnit", typeof(System.Boolean));
                dtUnits.Columns.Add("ParentUnit", typeof(System.String));
                dtUnits.Columns.Add("ParentUnitDescription", typeof(System.String));
                dtUnits.Columns.Add("Packing", typeof(System.Double));
                dtUnits.Columns.Add("PackingBasic", typeof(System.Double));

                foreach (cntStockUnit cntStockUnit in flpStockUnits.Controls)
                {
                    if (!cntStockUnit.Delete)
                    {
                        if (!cntStockUnit.IsValid())
                        {
                            return "Invalid data in units definition";
                        }
                    }
                }

                foreach (cntStockUnit cntStockUnit in flpStockUnits.Controls)
                {

                    if (!cntStockUnit.Delete)
                    {

                        DataRow dr = dtUnits.NewRow();

                        dr["Unit"] = cntStockUnit.cntUnit.Code?.ToUpper();
                        dr["UnitDescription"] = cntStockUnit.cntUnit.Description;
                        dr["ParentUnit"] = cntStockUnit.cntParentUnit.Code?.ToUpper();
                        dr["ParentUnitDescription"] = cntStockUnit.cntParentUnit.Description;
                        dr["IsBasicUnit"] = cntStockUnit.chkBaseUnit.Checked;
                        dr["IsDefaultPIUnit"] = cntStockUnit.chkDefaultPIUnit.Checked;
                        dr["Packing"] = Convert.ToDouble(cntStockUnit.txtPacking.Text);
                        dr["PackingBasic"] = Convert.ToDouble(cntStockUnit.txtPackingBasic.Text);

                        dtUnits.Rows.Add(dr);

                    }

                }

                // Check empty units

                bool hasEmptyUnits = dtUnits.AsEnumerable()
                    .Any(row => string.IsNullOrWhiteSpace(row.Field<string>("Unit")?.ToUpper()));

                if (hasEmptyUnits)
                {
                    return "There are records with an empty 'Unit'. Each unit must have a value.";
                }

                // Check for duplicated units

                var duplicateUnits = dtUnits.AsEnumerable()
                    .GroupBy(row => row.Field<string>("Unit")?.ToUpper())
                    .Where(group => !string.IsNullOrWhiteSpace(group.Key) && group.Count() > 1)
                    .Select(group => group.Key)
                    .ToList();

                if (duplicateUnits.Any())
                {
                    return "Duplicate 'Unit' values found: " + string.Join(", ", duplicateUnits);
                }

                // Check if The Basic Unit / Default PI Unit are Defined and Defined once

                if (dtUnits.AsEnumerable().Where(row => row.Field<bool?>("IsBasicUnit") == true).ToList().Count != 1)
                {
                    return "Basic unit should be defined for 1 unit";
                }

                if (dtUnits.AsEnumerable().Where(row => row.Field<bool?>("IsDefaultPIUnit") == true).ToList().Count != 1)
                {
                    return "Default PI unit should be defined for 1 unit";
                }

                // Check for invalid packing (Conversion Factor) - Basic Unit Factor Should be 1

                if (dtUnits.AsEnumerable()
                                            .Any(row =>
                                            {
                                                bool isValidPacking = row.Field<double>("Packing") > 0;

                                                if (row.Field<bool>("IsBasicUnit") && row.Field<double>("Packing") != 1)
                                                    return true;

                                                return !isValidPacking;
                                            }))
                {
                    return "Invalid Packing (Conversion Factor) found. It must be > 0 and 1 for the basic unit.";
                }

                // Check for circular references in the Unit/ParentUnit relationships

                HashSet<string> visitedUnits = new HashSet<string>();

                foreach (DataRow row in dtUnits.Select(" 1=1 ", " IsBasicUnit Desc "))
                {

                    string unit = row.Field<string>("Unit")?.ToUpper();

                    if (row.Field<bool>("IsBasicUnit") && (row.Field<string>("Unit")?.ToUpper() != row.Field<string>("ParentUnit")?.ToUpper()))
                    {
                        return "For the basic unit, 'Unit' and 'ParentUnit' must be the same";
                    }

                    if (row.Field<bool>("IsBasicUnit"))
                    {
                        BasicUnit = row.Field<string>("Unit")?.ToUpper();
                    }

                    // Detect circular references (loooop) and validate parent unit linkage

                    if (!visitedUnits.Contains(unit))
                    {
                        HashSet<string> unitVisited = new HashSet<string>();

                        string currentUnit = unit;
                        while (dtUnits.AsEnumerable().Any(r => r.Field<string>("Unit")?.ToUpper() == currentUnit?.ToUpper()))
                        {
                            if (unitVisited.Contains(currentUnit))
                            {
                                return $"Invalid Definition detected starting from unit '{unit?.ToUpper()}'.";
                            }

                            unitVisited.Add(currentUnit);

                            var currentRow = dtUnits.AsEnumerable().First(r => r.Field<string>("Unit")?.ToUpper() == currentUnit?.ToUpper());
                            string parentUnitFromRow = currentRow.Field<string>("ParentUnit")?.ToUpper();

                            if (parentUnitFromRow?.ToUpper() == BasicUnit?.ToUpper())
                            {
                                break;
                            }

                            currentUnit = parentUnitFromRow?.ToUpper();
                        }

                        visitedUnits.Add(unit); // Processed
                    }
                }

                // Ensure that all units have a route to the define basic unit

                if (string.IsNullOrEmpty(BasicUnit))
                {
                    return "Basic Unit not found"; // add on condition
                }

                // Check if every unit has a route to the basic unit
                foreach (DataRow row in dtUnits.Select(" 1=1 ", " IsBasicUnit Desc "))
                {

                    if (!HasRouteToBasicUnit(ref dtUnits, row.Field<string>("Unit")?.ToUpper(), BasicUnit?.ToUpper()))
                    {
                        // Invalid Unit definition
                        return $"Unit '{row.Field<string>("Unit")?.ToUpper()}' does not have a route to the basic unit.";
                    }

                    row["PackingBasic"] = GetConversionFactorToBasicUnit(ref dtUnits, row.Field<string>("Unit")?.ToUpper(), BasicUnit?.ToUpper());

                }

                // check if cnt description is empty 

                foreach (cntStockUnit cntStockUnit in flpStockUnits.Controls)
                {

                    if (lcFormMode == ClsCommon.FormMode.Add && txtDesc.Text != "")
                    {
                        if (cntStockUnit.cntUnit.Code != "" && cntStockUnit.txtDescription.Text == "")
                        {
                            if ((txtDesc.Text.Trim().Length + cntStockUnit.cntUnit.Code.Trim().Length + 3) > 200)
                            {
                                cntStockUnit.txtDescription.Text = txtDesc.Text.Trim();
                            }
                            else
                            {
                                cntStockUnit.txtDescription.Text = txtDesc.Text.Trim() + " " + cntStockUnit.cntUnit.Code.Trim();
                            }

                        }
                    }

                    if (rbBaseCurrencyLC.Checked)
                    {
                        cntStockUnit.txtPrice1LC.Enabled = true;
                        cntStockUnit.txtPrice2LC.Enabled = true;
                        cntStockUnit.txtPrice3LC.Enabled = true;

                        cntStockUnit.txtPrice1CC.Enabled = false;
                        cntStockUnit.txtPrice2CC.Enabled = false;
                        cntStockUnit.txtPrice3CC.Enabled = false;
                    }
                    else
                    {
                        cntStockUnit.txtPrice1LC.Enabled = false;
                        cntStockUnit.txtPrice2LC.Enabled = false;
                        cntStockUnit.txtPrice3LC.Enabled = false;

                        cntStockUnit.txtPrice1CC.Enabled = true;
                        cntStockUnit.txtPrice2CC.Enabled = true;
                        cntStockUnit.txtPrice3CC.Enabled = true;
                    }

                }


                // Calculate the Paking Basic (conversion factor to the basic unit)

                foreach (cntStockUnit cntStockUnit in flpStockUnits.Controls)
                {
                    if (cntStockUnit.Visible)
                    {
                        cntStockUnit.txtPackingBasic.Text = Convert.ToDouble(dtUnits.Select($" Unit = '{cntStockUnit.cntUnit.Code}' ")[0]["PackingBasic"]).ToString(ClsConfig.locationDirectives.Ld_NoOfDecQtyFormat);

                        foreach (cntStockUnit cnt in flpStockUnits.Controls)
                        {
                            if (cnt.chkBaseUnit.Checked)
                            {
                                if (Convert.ToDouble(cntStockUnit.txtPackingBasic.Text) != 0)
                                {
                                    string Price1LL, Price2LL, Price3LL, Price1CC, Price2CC, Price3CC = "";
                                    Price1LL = cnt.txtPrice1LC.Text;
                                    Price2LL = cnt.txtPrice2LC.Text;
                                    Price3LL = cnt.txtPrice3LC.Text;
                                    Price1CC = cnt.txtPrice1CC.Text;
                                    Price2CC = cnt.txtPrice2CC.Text;
                                    Price3CC = cnt.txtPrice3CC.Text;

                                    if (cntStockUnit.txtPackingBasic.LastText != cntStockUnit.txtPackingBasic.Text || ((cntStockUnit.txtDiscount.AllowEmptyNumeric == true && cntStockUnit.txtDiscount.Text == "") || Convert.ToDouble(cntStockUnit.txtDiscount.Text) == 0))
                                    {
                                        ClsPricing.CalculatePriceForUnits(Convert.ToDouble(cntStockUnit.txtPackingBasic.Text),
                                                                          cnt.txtPrice1LC.Text, cnt.txtPrice1CC.Text,
                                                                          "", "",
                                                                          "", "",
                                                                          ref Price1LL, ref Price1CC,
                                                                          ref Price2LL, ref Price2CC,
                                                                          ref Price3LL, ref Price3CC);

                                        cntStockUnit.txtPrice1LC.Text = cntStockUnit.txtPrice1LC.LastText = Price1LL;
                                        cntStockUnit.txtPrice1CC.Text = cntStockUnit.txtPrice1CC.LastText = Price1CC;

                                        cntStockUnit.txtDiscount.Text = cntStockUnit.txtDiscount.LastText = cnt.txtDiscount.Text;

                                    }

                                    if (cntStockUnit.txtPackingBasic.LastText != cntStockUnit.txtPackingBasic.Text || ((cntStockUnit.txtP1P2Disc.AllowEmptyNumeric == true && cntStockUnit.txtP1P2Disc.Text == "") || Convert.ToDouble(cntStockUnit.txtP1P2Disc.Text) == 0))
                                    {

                                        ClsPricing.CalculatePriceForUnits(Convert.ToDouble(cntStockUnit.txtPackingBasic.Text),
                                                                        "", "",
                                                                        cnt.txtPrice2LC.Text, cnt.txtPrice2CC.Text,
                                                                        "", "",
                                                                        ref Price1LL, ref Price1CC,
                                                                        ref Price2LL, ref Price2CC,
                                                                        ref Price3LL, ref Price3CC);

                                        cntStockUnit.txtPrice2LC.Text = cntStockUnit.txtPrice2LC.LastText = Price2LL;
                                        cntStockUnit.txtPrice2CC.Text = cntStockUnit.txtPrice2CC.LastText = Price2CC;

                                        cntStockUnit.txtP1P2Disc.Text = cntStockUnit.txtP1P2Disc.LastText = cnt.txtP1P2Disc.Text;
                                    }

                                    if (cntStockUnit.txtPackingBasic.LastText != cntStockUnit.txtPackingBasic.Text || ((cntStockUnit.txtP1P3Disc.AllowEmptyNumeric == true && cntStockUnit.txtP1P3Disc.Text == "") || Convert.ToDouble(cntStockUnit.txtP1P3Disc.Text) == 0))
                                    {

                                        ClsPricing.CalculatePriceForUnits(Convert.ToDouble(cntStockUnit.txtPackingBasic.Text),
                                                                     "", "",
                                                                     "", "",
                                                                     cnt.txtPrice3LC.Text, cnt.txtPrice3CC.Text,
                                                                     ref Price1LL, ref Price1CC,
                                                                     ref Price2LL, ref Price2CC,
                                                                     ref Price3LL, ref Price3CC);

                                        cntStockUnit.txtPrice3LC.Text = cntStockUnit.txtPrice3LC.LastText = Price3LL;
                                        cntStockUnit.txtPrice3CC.Text = cntStockUnit.txtPrice3CC.LastText = Price3CC;

                                        cntStockUnit.txtP1P3Disc.Text = cntStockUnit.txtP1P3Disc.LastText = cnt.txtP1P3Disc.Text;
                                    }

                                    cntStockUnit.txtPackingBasic.LastText = cntStockUnit.txtPackingBasic.Text;


                                }
                                else
                                {
                                    cntStockUnit.txtPrice1LC.Text = 0.ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat);
                                    cntStockUnit.txtPrice2LC.Text = 0.ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat);
                                    cntStockUnit.txtPrice3LC.Text = 0.ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat);

                                    cntStockUnit.txtPrice1CC.Text = 0.ToString(ClsConfig.locationDirectives.Ld_PriceCCFormat);
                                    cntStockUnit.txtPrice2CC.Text = 0.ToString(ClsConfig.locationDirectives.Ld_PriceCCFormat);
                                    cntStockUnit.txtPrice3CC.Text = 0.ToString(ClsConfig.locationDirectives.Ld_PriceCCFormat);
                                }

                            }

                            // uncomment this part when all transaction tables created --- LEA 
                            if (StockID != 0)
                            {
                                //if (StockItemsController.IsMovemented(StockID))
                                //{
                                //    cntStockUnit.txtPackingBasic.Enabled = false;
                                //}
                                //else
                                //{
                                //    cntStockUnit.txtPackingBasic.Enabled = true;
                                //}
                            }

                        }

                    }
                }

                PopulateUnitTreeView(dtUnits, BasicUnit?.ToUpper());
                return "";

            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CntStockUnit_BarcodeValidatedFromUnit(object sender, string barcode)
        {
            try
            {
                if (string.IsNullOrEmpty(barcode)) { return; }

                string barcode1 = cntBarcode1.txtBarcode1.Text.Trim();
                string barcode2 = cntBarcode2.txtBarcode1.Text.Trim();

                if (barcode == barcode1 || barcode == barcode2)
                {
                    ClsCommon.ShowNotification(this, $"Barcode '{barcode}' Already Exists In Tab Barcode.");

                    if (sender is cntBarcode barcodeControl)
                    {
                        RefocusSender(sender);
                    }
                }
                else if (BarcodeSynController.CheckBarcodeSyn(barcode, lcFormMode == ClsCommon.FormMode.Modify ? StockID : 0))
                { 
                    ClsCommon.ShowNotification(this, "Barcode Already Exists");
                    RefocusSender(sender);
                }


                // Check against other barcodes in all cntStockUnits
                foreach (cntStockUnit unit in flpStockUnits.Controls)
                {
                    foreach (cntBarcode b in unit.flpSynBarcode.Controls)
                    {
                        // Exclude the sender itself to prevent false positive
                        if (b == sender) continue;

                        string existing = b.txtBarcode1.Text.Trim();
                        if (!string.IsNullOrEmpty(existing) && existing == barcode)
                        {
                            ClsCommon.ShowNotification(this, $"Duplicate Barcode.");
                            RefocusSender(sender);
                            return;
                        }
                    }
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        private void RefocusSender(object sender)
        {
            if (sender is cntBarcode barcodeControl)
            {
                barcodeControl.txtBarcode1.Focus();
            }
        }

        private void AddStockUnitHandlers(cntStockUnit cntStockUnit)
        {
            try
            {

                cntStockUnit.chkBaseUnit.CheckedChanged += new System.EventHandler(this.chkBaseUnit_CheckedChanged);
                cntStockUnit.chkDefaultPIUnit.CheckedChanged += new System.EventHandler(this.chkDefaultPIUnit_CheckedChanged);

                cntStockUnit.cntUnit.Validated += new System.EventHandler(this.UnitsControls_Validated);
                cntStockUnit.cntParentUnit.Validated += new System.EventHandler(this.UnitsControls_Validated);
                cntStockUnit.txtPacking.Validated += new System.EventHandler(this.UnitsControls_Validated);

                cntStockUnit.txtPrice1LC.Validating += new System.ComponentModel.CancelEventHandler(this.Pricing_Validating);
                cntStockUnit.txtPrice2LC.Validating += new System.ComponentModel.CancelEventHandler(this.Pricing_Validating);
                cntStockUnit.txtPrice3LC.Validating += new System.ComponentModel.CancelEventHandler(this.Pricing_Validating);
                cntStockUnit.txtPrice1CC.Validating += new System.ComponentModel.CancelEventHandler(this.Pricing_Validating);
                cntStockUnit.txtPrice2CC.Validating += new System.ComponentModel.CancelEventHandler(this.Pricing_Validating);
                cntStockUnit.txtPrice3CC.Validating += new System.ComponentModel.CancelEventHandler(this.Pricing_Validating);

                cntStockUnit.txtDiscount.Validating += new System.ComponentModel.CancelEventHandler(this.Discount_Validating);
                cntStockUnit.txtP1P2Disc.Validating += new System.ComponentModel.CancelEventHandler(this.Discount_Validating);
                cntStockUnit.txtP1P3Disc.Validating += new System.ComponentModel.CancelEventHandler(this.Discount_Validating);

                cntStockUnit.chkActiveUnit.CheckedChanged += new System.EventHandler(this.chkActiveUnit_CheckedChanged);

                cntStockUnit.BarcodeValidatedFromUnit += CntStockUnit_BarcodeValidatedFromUnit;

            }
            catch (Exception)
            {

                throw;
            }
        }

        bool HasRouteToBasicUnit(ref DataTable dtUnits, string unit, string basicUnit)
        {
            try
            {

                string currentUnit = unit?.ToUpper();

                while (dtUnits.AsEnumerable().Any(row => row.Field<string>("Unit")?.ToUpper() == currentUnit?.ToUpper()))
                {
                    var currentRow = dtUnits.AsEnumerable().First(row => row.Field<string>("Unit")?.ToUpper() == currentUnit?.ToUpper());
                    string parentUnit = currentRow.Field<string>("ParentUnit")?.ToUpper();

                    if (parentUnit?.ToUpper() == basicUnit?.ToUpper())
                    {
                        return true;
                    }

                    if (parentUnit?.ToUpper() == currentUnit?.ToUpper()) // Prevent inifinite loooop
                    {
                        break;
                    }

                    currentUnit = parentUnit?.ToUpper();
                }

                return false;

            }
            catch (Exception)
            {
                throw;
            }
        }

        double GetConversionFactor(ref DataTable dtUnits, string unit, string basicUnit)
        {
            try
            {

                string currentUnit = unit?.ToUpper();
                double conversionFactor = 1;

                while (dtUnits.AsEnumerable().Any(row => row.Field<string>("Unit")?.ToUpper() == currentUnit?.ToUpper()))
                {
                    var currentRow = dtUnits.AsEnumerable().First(row => row.Field<string>("Unit")?.ToUpper() == currentUnit?.ToUpper());
                    string parentUnit = currentRow.Field<string>("ParentUnit")?.ToUpper();

                    double packingBasic = currentRow.Field<double>("PackingBasic");

                    if (parentUnit?.ToUpper() == basicUnit?.ToUpper())
                    {
                        break;
                    }

                    conversionFactor *= packingBasic;

                    if (parentUnit?.ToUpper() == currentUnit?.ToUpper())
                    {
                        return 0; // Error - CR
                    }

                    currentUnit = parentUnit?.ToUpper();
                }

                return conversionFactor;

            }
            catch (Exception)
            {
                throw;
            }
        }
       

        double GetConversionFactorToBasicUnit(ref DataTable dtUnits, string unit, string basicUnit)
        {
            try
            {

                string currentUnit = unit;
                double conversionFactor = 1.0;
                HashSet<string> visitedUnits = new HashSet<string>(); // To prevent CR - LOOP

                while (!string.IsNullOrEmpty(currentUnit))
                {

                    if (visitedUnits.Contains(currentUnit))
                    {
                        throw new Exception($"Circular reference detected for unit: {currentUnit}");
                    }

                    visitedUnits.Add(currentUnit);

                    var currentRow = dtUnits.AsEnumerable()
                        .FirstOrDefault(row => row.Field<string>("Unit")?.ToUpper() == currentUnit?.ToUpper());

                    if (currentRow == null)
                    {
                        throw new Exception($"Unit '{currentUnit}' not found in the data table.");
                    }

                    string parentUnit = currentRow.Field<string>("ParentUnit")?.ToUpper();
                    if (!double.TryParse(currentRow["Packing"].ToString(), out double packing) || packing <= 0)
                    {
                        throw new Exception($"Invalid Packing value for unit '{currentUnit}'.");
                    }

                    conversionFactor *= packing;

                    if (currentUnit?.ToUpper() == basicUnit?.ToUpper())
                    {
                        break;
                    }

                    currentUnit = parentUnit;
                }

                return conversionFactor;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return 0; // Or handle appropriately
            }
        }

        void PopulateUnitTreeView(DataTable dtUnits, string basicUnit)
        {
            try
            {



                treeViewUnits.Nodes.Clear();

                // Find the basic unit row
                var basicRow = dtUnits.AsEnumerable()
                    .FirstOrDefault(row => row.Field<string>("Unit")?.ToUpper() == basicUnit?.ToUpper());

                if (basicRow == null)
                {
                    throw new Exception("Basic unit not found in the data table.");
                }


                TreeNode rootNode = new TreeNode($" {basicUnit} - {basicRow.Field<string>("UnitDescription")}");
                rootNode.Tag = basicUnit;
                treeViewUnits.Nodes.Add(rootNode);


                HashSet<string> visitedUnits = new HashSet<string>(StringComparer.OrdinalIgnoreCase);


                AddChildNodes(dtUnits, rootNode, basicUnit, basicUnit, visitedUnits);

                treeViewUnits.ExpandAll();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void AddChildNodes(DataTable dtUnits, TreeNode parentNode, string parentUnit, string basicUnit, HashSet<string> visitedUnits)
        {

            if (!visitedUnits.Add(parentUnit))
            {
                parentNode.Nodes.Add(new TreeNode("⚠️ Problem Detected"));
                return;
            }

            var childRows = dtUnits.AsEnumerable()
                .Where(row => row.Field<string>("ParentUnit")?.ToUpper() == parentUnit?.ToUpper() &&
                              row.Field<string>("Unit")?.ToUpper() != parentUnit?.ToUpper());

            foreach (var row in childRows)
            {
                string unit = row.Field<string>("Unit");

                TreeNode childNode = new TreeNode($" {row.Field<string>("Unit")} - {row.Field<string>("UnitDescription")} ( {row.Field<double>("PackingBasic").ToString(ClsConfig.locationDirectives.Ld_NoOfDecQtyFormat)} {basicUnit} )");
                childNode.Tag = unit;
                parentNode.Nodes.Add(childNode);

                AddChildNodes(dtUnits, childNode, unit, basicUnit, visitedUnits);
            }

            visitedUnits.Remove(parentUnit);
        }

        private void UnitsControls_Validated(object sender, EventArgs e)
        {

            try
            {

                txtUnitsLogging.Text = ValidateUnits();

                //double PriceLC = 0;
                //double PriceCC = 0;
                //double Price2LC = 0;
                //double Price2CC = 0;
                //double Price3LC = 0;
                //double Price3CC = 0;


                double PriceLC = Convert.ToDouble(txtPrice1LL.Text);
                double PriceCC = Convert.ToDouble(txtPrice1CC.Text);
                double Price2LC = Convert.ToDouble(txtPrice2LL.Text);
                double Price2CC = Convert.ToDouble(txtPrice2CC.Text);
                double Price3LC = Convert.ToDouble(txtPrice3LL.Text);
                double Price3CC = Convert.ToDouble(txtPrice3CC.Text);

                try
                {

                    ClsPricing.CalculatePrice(rbBaseCurrencyCC.Checked ? ClsConfig.gbCurr2 : ClsConfig.gbCurr1,
                                          rbLC.Checked ? ClsConfig.gbCurr1 : rbCC.Text ,
                                          Convert.ToDouble(txtSalesCoefVal.Text),
                                          Convert.ToDouble(txtPotCostLC.Text),
                                          Convert.ToDouble(txtPotCostCC.Text),
                                          Convert.ToDouble(cntVat1.VatPerc),
                                          "",
                                          0,
                                          0,
                                          ref PriceLC,
                                          ref PriceCC,
                                          ref Price2LC,
                                          ref Price2CC,
                                          ref Price3LC,
                                          ref Price3CC
                                          );

                    foreach (cntStockUnit cntStockUnit in flpStockUnits.Controls)
                    {
                        cntStockUnit.txtPrice1LC.NumericText = Convert.ToDouble(cntStockUnit.txtPrice1LC.LastText) == 0 ? (PriceLC * Convert.ToDouble(cntStockUnit.txtPackingBasic.Text)) : Convert.ToDouble(cntStockUnit.txtPrice1LC.Text);
                        cntStockUnit.txtPrice1LC.LastText = cntStockUnit.txtPrice1LC.Text;
                        cntStockUnit.txtPrice1CC.NumericText = Convert.ToDouble(cntStockUnit.txtPrice1CC.LastText) == 0 ? (PriceCC * Convert.ToDouble(cntStockUnit.txtPackingBasic.Text)) : Convert.ToDouble(cntStockUnit.txtPrice1CC.Text);
                        cntStockUnit.txtPrice1CC.LastText = cntStockUnit.txtPrice1CC.Text;

                        cntStockUnit.txtPrice2LC.NumericText = Convert.ToDouble(cntStockUnit.txtPrice2LC.LastText) == 0 ? (Price2LC * Convert.ToDouble(cntStockUnit.txtPackingBasic.Text)) : Convert.ToDouble(cntStockUnit.txtPrice2LC.Text);
                        cntStockUnit.txtPrice2LC.LastText = cntStockUnit.txtPrice2LC.Text;
                        cntStockUnit.txtPrice2CC.NumericText = Convert.ToDouble(cntStockUnit.txtPrice2CC.LastText) == 0 ? (Price2CC * Convert.ToDouble(cntStockUnit.txtPackingBasic.Text)) : Convert.ToDouble(cntStockUnit.txtPrice2CC.Text);
                        cntStockUnit.txtPrice2CC.LastText = cntStockUnit.txtPrice2CC.Text;

                        cntStockUnit.txtPrice3LC.NumericText = Convert.ToDouble(cntStockUnit.txtPrice3LC.LastText) == 0 ? (Price3LC * Convert.ToDouble(cntStockUnit.txtPackingBasic.Text)) : Convert.ToDouble(cntStockUnit.txtPrice3LC.Text);
                        cntStockUnit.txtPrice3LC.LastText = cntStockUnit.txtPrice3LC.Text;
                        cntStockUnit.txtPrice3CC.NumericText = Convert.ToDouble(cntStockUnit.txtPrice3CC.LastText) == 0 ? (Price3CC * Convert.ToDouble(cntStockUnit.txtPackingBasic.Text)) : Convert.ToDouble(cntStockUnit.txtPrice3CC.Text);
                        cntStockUnit.txtPrice3CC.LastText = cntStockUnit.txtPrice3CC.Text;

                        cntStockUnit.txtP1P2Disc.Text = cntStockUnit.txtP1P2Disc.LastText = txtDiscPrice2.Text == "" ? "0" : txtDiscPrice2.Text;
                        cntStockUnit.txtP1P3Disc.Text = cntStockUnit.txtP1P3Disc.LastText = txtDiscPrice3.Text == "" ? "0" : txtDiscPrice3.Text;

                    }

                }
                catch (Exception)
                {

                }

                if (txtUnitsLogging.Text.Trim() != "")
                {

                    treeViewUnits.Nodes.Clear();

                    foreach (cntStockUnit cntStockUnit in flpStockUnits.Controls)
                    {
                        if (cntStockUnit.Visible)
                        {
                            ((cntStockUnit)cntStockUnit).txtPackingBasic.Text = 1.ToString(ClsConfig.locationDirectives.Ld_NoOfDecQtyFormat);
                        }
                    }

                }

            }
            catch (Exception)
            {

            }

        }

        private void treeViewUnits_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {

                if (!string.IsNullOrEmpty(e.Node.Tag.ToString()))
                {
                    foreach (cntStockUnit cntStockUnit in flpStockUnits.Controls)
                    {

                        if (cntStockUnit.cntUnit.Code?.ToUpper() == e.Node.Tag.ToString()?.ToUpper())
                        {
                            ActiveControl = cntStockUnit;
                            break;
                        }

                    }
                }

            }
            catch (Exception)
            {

            }
        }

        #endregion


        #region Pricing Manipulation

        private void Discount_Validating(object sender, CancelEventArgs e)
        {
            try
            {

                if (((Control)sender).Parent.GetType() == typeof(cntStockUnit))
                {

                    cntStockUnit cntStockUnit = (cntStockUnit)((Control)sender).Parent;

                    if (sender == cntStockUnit.txtDiscount)
                    {
                        if (cntStockUnit.txtDiscount.Text == cntStockUnit.txtDiscount.LastText) { return; }

                        if (Convert.ToDouble(cntStockUnit.txtPrice1LC.LastText) == 0) { cntStockUnit.txtPrice1LC.LastText = cntStockUnit.txtPrice1LC.Text; }
                        if (Convert.ToDouble(cntStockUnit.txtPrice1CC.LastText) == 0) { cntStockUnit.txtPrice1CC.LastText = cntStockUnit.txtPrice1CC.Text; }

                        string Price1LL, Price1CC = "";
                        Price1LL = cntStockUnit.txtPrice1LC.Text;
                        Price1CC = cntStockUnit.txtPrice1CC.Text;

                        ClsPricing.CalculatePriceAfterDiscount(cntStockUnit.txtDiscount.Text, cntStockUnit.txtPrice1LC.Text, cntStockUnit.txtPrice1CC.Text, ref Price1LL, ref Price1CC);

                        cntStockUnit.txtPrice1LC.Text = Price1LL;
                        cntStockUnit.txtPrice1CC.Text = Price1CC;

                        cntStockUnit.txtDiscount.LastText = cntStockUnit.txtDiscount.Text;

                        if (cntStockUnit.txtPrice1CC.LastText != cntStockUnit.txtPrice1CC.Text)
                        {
                            Discount_Validating(cntStockUnit.txtP1P2Disc, new System.ComponentModel.CancelEventArgs());
                            Discount_Validating(cntStockUnit.txtP1P3Disc, new System.ComponentModel.CancelEventArgs());
                        }

                        cntStockUnit.txtPrice1LC.LastText = Price1LL;
                        cntStockUnit.txtPrice1CC.LastText = Price1CC;

                    }
                    else if (sender == cntStockUnit.txtP1P2Disc)
                    {
                        if (cntStockUnit.txtP1P2Disc.Text == cntStockUnit.txtP1P2Disc.LastText && cntStockUnit.txtPrice1CC.LastText == cntStockUnit.txtPrice1CC.Text) { return; }

                        if (Convert.ToDouble(cntStockUnit.txtPrice2LC.LastText) == 0) { cntStockUnit.txtPrice2LC.LastText = cntStockUnit.txtPrice2LC.Text; }
                        if (Convert.ToDouble(cntStockUnit.txtPrice2CC.LastText) == 0) { cntStockUnit.txtPrice2CC.LastText = cntStockUnit.txtPrice2CC.Text; }

                        string Price2LL, Price2CC = "";
                        Price2LL = cntStockUnit.txtPrice2LC.Text;
                        Price2CC = cntStockUnit.txtPrice2CC.Text;

                        ClsPricing.CalculatePriceAfterDiscount(cntStockUnit.txtP1P2Disc.Text, cntStockUnit.txtPrice1LC.Text, cntStockUnit.txtPrice1CC.Text, ref Price2LL, ref Price2CC);


                        txtPrice2LL.Text = Price2LL;
                        txtPrice2CC.Text = Price2CC;

                        cntStockUnit.txtPrice2LC.Text = cntStockUnit.txtPrice2LC.LastText = Price2LL;
                        cntStockUnit.txtPrice2CC.Text = cntStockUnit.txtPrice2CC.LastText = Price2CC;

                        cntStockUnit.txtP1P2Disc.LastText = cntStockUnit.txtP1P2Disc.Text;
                    }
                    else if (sender == cntStockUnit.txtP1P3Disc)
                    {
                        if (cntStockUnit.txtP1P3Disc.Text == cntStockUnit.txtP1P3Disc.LastText && cntStockUnit.txtPrice1CC.LastText == cntStockUnit.txtPrice1CC.Text) { return; }

                        if (Convert.ToDouble(cntStockUnit.txtPrice3LC.LastText) == 0) { cntStockUnit.txtPrice3LC.LastText = cntStockUnit.txtPrice3LC.Text; }
                        if (Convert.ToDouble(cntStockUnit.txtPrice3CC.LastText) == 0) { cntStockUnit.txtPrice3CC.LastText = cntStockUnit.txtPrice3CC.Text; }

                        string Price3LL, Price3CC = "";
                        Price3LL = cntStockUnit.txtPrice3LC.Text;
                        Price3CC = cntStockUnit.txtPrice3CC.Text;

                        ClsPricing.CalculatePriceAfterDiscount(cntStockUnit.txtP1P3Disc.Text, cntStockUnit.txtPrice1LC.Text, cntStockUnit.txtPrice1CC.Text, ref Price3LL, ref Price3CC);


                        cntStockUnit.txtPrice3LC.Text = cntStockUnit.txtPrice3LC.LastText = Price3LL;
                        cntStockUnit.txtPrice3CC.Text = cntStockUnit.txtPrice3CC.LastText = Price3CC;

                        cntStockUnit.txtP1P3Disc.LastText = cntStockUnit.txtP1P3Disc.Text;
                    }

                }
            }
            catch (Exception ex)
            {

                ClsCommon.ShowException(StepName, ex);
            }
        }

        private void Pricing_Validating(object sender, CancelEventArgs e)   
        {
            try
            {
                string discount = "";
                double Price = 0;

                foreach (cntStockUnit cntStockUnit in flpStockUnits.Controls)
                {
                    if (cntStockUnit.chkBaseUnit.Checked)   
                    {
                        if (sender != cntStockUnit.txtPrice1CC || sender != cntStockUnit.txtPrice1LC)
                        {
                            if (rbBaseCurrencyLC.Checked)
                            {
                                if (sender == cntStockUnit.txtPrice2LC &&
                                    cntStockUnit.txtPrice2LC.Enabled == true &&
                                    Convert.ToDouble(cntStockUnit.txtPrice2LC.Text) != 0 &&
                                    Convert.ToDouble(cntStockUnit.txtPrice2LC.Text) != Convert.ToDouble(cntStockUnit.txtPrice2LC.LastText))
                                {

                                    if (cntStockUnit.txtP1P2Disc.AllowEmptyNumeric && cntStockUnit.txtP1P2Disc.Text == "")
                                    {
                                        cntStockUnit.txtP1P2Disc.LastText = "";
                                        cntStockUnit.txtPrice2LC.Text = txtPrice2LL.Text;
                                        cntStockUnit.txtPrice2CC.Text = txtPrice2CC.Text;
                                    }
                                    else if (Convert.ToDouble(cntStockUnit.txtP1P2Disc.Text) != 0 && double.TryParse(cntStockUnit.txtP1P2Disc.Text, out double result))
                                    {

                                        //ClsPricing.CalculateDiscountAfterChangingPrice(cntStockUnit.txtPacking.Text, txtPrice2LL.Text, cntStockUnit.txtPrice2LC.Text, txtPrice2CC.Text, ref discount, ref Price);

                                        ClsPricing.CalculateDiscountAfterChangingPrice(
                                                                          cntStockUnit.txtPrice2LC.LastText,
                                                                          cntStockUnit.txtPrice2LC.Text,
                                                                          cntStockUnit.txtPrice2CC.LastText, ref discount, ref Price, cntStockUnit.txtP1P2Disc.Text);

                                        cntStockUnit.txtP1P2Disc.Text = discount;
                                        cntStockUnit.txtP1P2Disc.LastText = cntStockUnit.txtP1P2Disc.Text;
                                        cntStockUnit.txtPrice2CC.Text = Price.ToString(ClsConfig.locationDirectives.Ld_PriceCCFormat);

                                    }

                                }

                                if (sender == cntStockUnit.txtPrice3LC &&
                                    cntStockUnit.txtPrice3LC.Enabled == true &&
                                    Convert.ToDouble(cntStockUnit.txtPrice3LC.Text) != 0 &&
                                    Convert.ToDouble(cntStockUnit.txtPrice3LC.Text) != Convert.ToDouble(cntStockUnit.txtPrice3LC.LastText))
                                {
                                    if (cntStockUnit.txtP1P3Disc.AllowEmptyNumeric && cntStockUnit.txtP1P3Disc.Text == "")
                                    {
                                        cntStockUnit.txtP1P3Disc.LastText = "";
                                        cntStockUnit.txtPrice3LC.Text = txtPrice3LL.Text;
                                        cntStockUnit.txtPrice3CC.Text = txtPrice3CC.Text;
                                    }
                                    else if (Convert.ToDouble(cntStockUnit.txtP1P3Disc.Text) != 0 && double.TryParse(cntStockUnit.txtP1P3Disc.Text, out double result))
                                    {

                                        //ClsPricing.CalculateDiscountAfterChangingPrice(cntStockUnit.txtPacking.Text, txtPrice3LL.Text, cntStockUnit.txtPrice3LC.Text, txtPrice3CC.Text, ref discount, ref Price);

                                        ClsPricing.CalculateDiscountAfterChangingPrice(
                                                                          cntStockUnit.txtPrice3LC.LastText,
                                                                          cntStockUnit.txtPrice3LC.Text,
                                                                          cntStockUnit.txtPrice3CC.LastText, ref discount, ref Price, cntStockUnit.txtP1P3Disc.Text);

                                        cntStockUnit.txtP1P3Disc.Text = discount;
                                        cntStockUnit.txtP1P3Disc.LastText = cntStockUnit.txtP1P3Disc.Text;
                                        cntStockUnit.txtPrice3CC.Text = Price.ToString(ClsConfig.locationDirectives.Ld_PriceCCFormat);

                                    }

                                }

                            }
                            else
                            {
                                if (sender == cntStockUnit.txtPrice2CC &&
                                    cntStockUnit.txtPrice2CC.Enabled == true &&
                                    Convert.ToDouble(cntStockUnit.txtPrice2CC.Text) != 0 &&
                                    Convert.ToDouble(cntStockUnit.txtPrice2CC.Text) != Convert.ToDouble(cntStockUnit.txtPrice2CC.LastText))
                                {
                                    if (cntStockUnit.txtP1P2Disc.AllowEmptyNumeric && cntStockUnit.txtP1P2Disc.Text == "")
                                    {
                                        cntStockUnit.txtP1P2Disc.LastText = "";
                                        cntStockUnit.txtPrice2LC.Text = txtPrice2LL.Text;
                                        cntStockUnit.txtPrice2CC.Text = txtPrice2CC.Text;
                                    }
                                    else if (Convert.ToDouble(cntStockUnit.txtP1P2Disc.Text) != 0 && double.TryParse(cntStockUnit.txtP1P2Disc.Text, out double result))
                                    {
                                        //ClsPricing.CalculateDiscountAfterChangingPrice(cntStockUnit.txtPacking.Text, txtPrice2CC.Text, cntStockUnit.txtPrice2CC.Text, txtPrice2LL.Text, ref discount, ref Price);

                                        ClsPricing.CalculateDiscountAfterChangingPrice(
                                                                          cntStockUnit.txtPrice2CC.LastText,
                                                                          cntStockUnit.txtPrice2CC.Text,
                                                                          cntStockUnit.txtPrice2LC.LastText, ref discount, ref Price, cntStockUnit.txtP1P2Disc.Text);

                                        cntStockUnit.txtP1P2Disc.Text = discount;
                                        cntStockUnit.txtP1P2Disc.LastText = cntStockUnit.txtP1P2Disc.Text;
                                        cntStockUnit.txtPrice2LC.Text = Price.ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat);
                                    }

                                }


                                if (sender == cntStockUnit.txtPrice3CC &&
                                    cntStockUnit.txtPrice3CC.Enabled == true &&
                                    Convert.ToDouble(cntStockUnit.txtPrice3CC.Text) != 0 &&
                                    Convert.ToDouble(cntStockUnit.txtPrice3CC.Text) != Convert.ToDouble(cntStockUnit.txtPrice3CC.LastText))
                                {

                                    if (cntStockUnit.txtP1P3Disc.AllowEmptyNumeric && cntStockUnit.txtP1P3Disc.Text == "")
                                    {
                                        cntStockUnit.txtP1P3Disc.LastText = "";
                                        cntStockUnit.txtPrice3LC.Text = txtPrice3LL.Text;
                                        cntStockUnit.txtPrice3CC.Text = txtPrice3CC.Text;
                                    }
                                    else if (Convert.ToDouble(cntStockUnit.txtP1P3Disc.Text) != 0 && double.TryParse(cntStockUnit.txtP1P3Disc.Text, out double result))
                                    {
                                        //ClsPricing.CalculateDiscountAfterChangingPrice(cntStockUnit.txtPacking.Text, txtPrice3CC.Text, cntStockUnit.txtPrice3CC.Text, txtPrice3LL.Text, ref discount, ref Price);

                                        ClsPricing.CalculateDiscountAfterChangingPrice(
                                                                          cntStockUnit.txtPrice3CC.LastText,
                                                                          cntStockUnit.txtPrice3CC.Text,
                                                                          cntStockUnit.txtPrice3LC.LastText, ref discount, ref Price, cntStockUnit.txtP1P3Disc.Text);

                                        cntStockUnit.txtP1P3Disc.Text = discount;
                                        cntStockUnit.txtP1P3Disc.LastText = cntStockUnit.txtP1P3Disc.Text;
                                        cntStockUnit.txtPrice3LC.Text = Price.ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat);

                                    }

                                }

                            }


                        }

                        string Operator = "";
                        double Rate = -1;

                        ClsCom.GetRate(ClsConfig.gbCurr2, DateTime.Today.ToString(), "LC", ref Operator, ref Rate, ClsConfig.locationDirectives.Ld_POSRate);

                        if (sender == cntStockUnit.txtPrice1LC || sender == cntStockUnit.txtPrice2LC || sender == cntStockUnit.txtPrice3LC)
                        {
                            if (Operator != "" && Rate != -1)
                            {
                                if (Operator == "*")
                                {
                                    if (sender == cntStockUnit.txtPrice1LC && Convert.ToDouble(cntStockUnit.txtPrice1LC.Text) != Convert.ToDouble(cntStockUnit.txtPrice1LC.LastText))
                                    {
                                        cntStockUnit.txtPrice1CC.Text = cntStockUnit.txtPrice1CC.LastText = (Convert.ToDouble(cntStockUnit.txtPrice1LC.Text) / Rate).ToString(ClsConfig.locationDirectives.Ld_PriceCCFormat);
                                        cntStockUnit.txtPrice1LC.LastText = cntStockUnit.txtPrice1LC.Text;
                                        ValidateUnits();
                                    }
                                    if (sender == cntStockUnit.txtPrice2LC && Convert.ToDouble(cntStockUnit.txtPrice2LC.Text) != Convert.ToDouble(cntStockUnit.txtPrice2LC.LastText))
                                    {
                                        cntStockUnit.txtPrice2CC.Text = cntStockUnit.txtPrice2CC.LastText = (Convert.ToDouble(cntStockUnit.txtPrice2LC.Text) / Rate).ToString(ClsConfig.locationDirectives.Ld_PriceCCFormat);
                                        cntStockUnit.txtPrice2LC.LastText = cntStockUnit.txtPrice2LC.Text;
                                        ValidateUnits();
                                    }
                                    if (sender == cntStockUnit.txtPrice3LC && Convert.ToDouble(cntStockUnit.txtPrice3LC.Text) != Convert.ToDouble(cntStockUnit.txtPrice3LC.LastText))
                                    {
                                        cntStockUnit.txtPrice3CC.Text = cntStockUnit.txtPrice3CC.LastText = (Convert.ToDouble(cntStockUnit.txtPrice3LC.Text) / Rate).ToString(ClsConfig.locationDirectives.Ld_PriceCCFormat);
                                        cntStockUnit.txtPrice3LC.LastText = cntStockUnit.txtPrice3LC.Text;
                                        ValidateUnits();
                                    }
                                }
                                else
                                {
                                    if (sender == cntStockUnit.txtPrice1LC && Convert.ToDouble(cntStockUnit.txtPrice1LC.Text) != Convert.ToDouble(cntStockUnit.txtPrice1LC.LastText))
                                    {
                                        cntStockUnit.txtPrice1CC.Text = cntStockUnit.txtPrice1CC.LastText = (Convert.ToDouble(cntStockUnit.txtPrice1LC.Text) * Rate).ToString(ClsConfig.locationDirectives.Ld_PriceCCFormat);
                                        cntStockUnit.txtPrice1LC.LastText = cntStockUnit.txtPrice1LC.Text;
                                        ValidateUnits();
                                    }
                                    if (sender == cntStockUnit.txtPrice2LC && Convert.ToDouble(cntStockUnit.txtPrice2LC.Text) != Convert.ToDouble(cntStockUnit.txtPrice2LC.LastText))
                                    {
                                        cntStockUnit.txtPrice2CC.Text = cntStockUnit.txtPrice2CC.LastText = (Convert.ToDouble(cntStockUnit.txtPrice2LC.Text) * Rate).ToString(ClsConfig.locationDirectives.Ld_PriceCCFormat);
                                        cntStockUnit.txtPrice2LC.LastText = cntStockUnit.txtPrice2LC.Text;
                                        ValidateUnits();
                                    }
                                    if (sender == cntStockUnit.txtPrice3LC && Convert.ToDouble(cntStockUnit.txtPrice3LC.Text) != Convert.ToDouble(cntStockUnit.txtPrice3LC.LastText))
                                    {
                                        cntStockUnit.txtPrice3CC.Text = cntStockUnit.txtPrice3CC.LastText = (Convert.ToDouble(cntStockUnit.txtPrice3LC.Text) * Rate).ToString(ClsConfig.locationDirectives.Ld_PriceCCFormat);
                                        cntStockUnit.txtPrice3LC.LastText = cntStockUnit.txtPrice3LC.Text;
                                        ValidateUnits();
                                    }
                                }
                            }
                            else
                            {
                                if (sender == cntStockUnit.txtPrice1LC && Convert.ToDouble(cntStockUnit.txtPrice1LC.Text) != Convert.ToDouble(cntStockUnit.txtPrice1LC.LastText))
                                {
                                    cntStockUnit.txtPrice1CC.Text = cntStockUnit.txtPrice1CC.LastText = 0.ToString(ClsConfig.locationDirectives.Ld_PriceCCFormat);
                                    cntStockUnit.txtPrice1LC.LastText = cntStockUnit.txtPrice1LC.Text;
                                    ValidateUnits();
                                }
                                if (sender == cntStockUnit.txtPrice2LC && Convert.ToDouble(cntStockUnit.txtPrice2LC.Text) != Convert.ToDouble(cntStockUnit.txtPrice2LC.LastText))
                                {
                                    cntStockUnit.txtPrice2CC.Text = cntStockUnit.txtPrice2CC.LastText = 0.ToString(ClsConfig.locationDirectives.Ld_PriceCCFormat);
                                    cntStockUnit.txtPrice2LC.LastText = cntStockUnit.txtPrice2LC.Text;
                                    ValidateUnits();
                                }
                                if (sender == cntStockUnit.txtPrice3LC && Convert.ToDouble(cntStockUnit.txtPrice3LC.Text) != Convert.ToDouble(cntStockUnit.txtPrice3LC.LastText))
                                {
                                    cntStockUnit.txtPrice3CC.Text = cntStockUnit.txtPrice3CC.LastText = 0.ToString(ClsConfig.locationDirectives.Ld_PriceCCFormat);
                                    cntStockUnit.txtPrice3LC.LastText = cntStockUnit.txtPrice3LC.Text;
                                    ValidateUnits();
                                }

                            }

                        }
                        else if (sender == cntStockUnit.txtPrice1CC || sender == cntStockUnit.txtPrice2CC || sender == cntStockUnit.txtPrice3CC)
                        {
                            if (Operator != "" && Rate != -1)
                            {
                                if (Operator == "*")
                                {
                                    if (sender == cntStockUnit.txtPrice1CC && Convert.ToDouble(cntStockUnit.txtPrice1CC.Text) != Convert.ToDouble(cntStockUnit.txtPrice1CC.LastText))
                                    {
                                        cntStockUnit.txtPrice1LC.Text = cntStockUnit.txtPrice1LC.LastText = (Convert.ToDouble(cntStockUnit.txtPrice1CC.Text) * Rate).ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat);
                                        cntStockUnit.txtPrice1CC.LastText = cntStockUnit.txtPrice1CC.Text;
                                        ValidateUnits();

                                    }
                                    if (sender == cntStockUnit.txtPrice2CC && Convert.ToDouble(cntStockUnit.txtPrice2CC.Text) != Convert.ToDouble(cntStockUnit.txtPrice2CC.LastText))
                                    {
                                        cntStockUnit.txtPrice2LC.Text = cntStockUnit.txtPrice2LC.LastText = (Convert.ToDouble(cntStockUnit.txtPrice2CC.Text) * Rate).ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat);
                                        cntStockUnit.txtPrice2CC.LastText = cntStockUnit.txtPrice2CC.Text;
                                        ValidateUnits();
                                    }
                                    if (sender == cntStockUnit.txtPrice3CC && Convert.ToDouble(cntStockUnit.txtPrice3CC.Text) != Convert.ToDouble(cntStockUnit.txtPrice3CC.LastText))
                                    {
                                        cntStockUnit.txtPrice3LC.Text = cntStockUnit.txtPrice3LC.LastText = (Convert.ToDouble(cntStockUnit.txtPrice3CC.Text) * Rate).ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat);
                                        cntStockUnit.txtPrice3CC.LastText = cntStockUnit.txtPrice3CC.Text;
                                        ValidateUnits();
                                    }
                                }
                                else
                                {
                                    if (sender == cntStockUnit.txtPrice1CC && Convert.ToDouble(cntStockUnit.txtPrice1CC.Text) != Convert.ToDouble(cntStockUnit.txtPrice1CC.LastText))
                                    {
                                        cntStockUnit.txtPrice1LC.Text = cntStockUnit.txtPrice1LC.LastText = (Convert.ToDouble(cntStockUnit.txtPrice1CC.Text) / Rate).ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat);
                                        cntStockUnit.txtPrice1CC.LastText = cntStockUnit.txtPrice1CC.Text;
                                        ValidateUnits();
                                    }
                                    if (sender == cntStockUnit.txtPrice2CC && Convert.ToDouble(cntStockUnit.txtPrice2CC.Text) != Convert.ToDouble(cntStockUnit.txtPrice2CC.LastText))
                                    {
                                        cntStockUnit.txtPrice2LC.Text = cntStockUnit.txtPrice2LC.LastText = (Convert.ToDouble(cntStockUnit.txtPrice2CC.Text) / Rate).ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat);
                                        cntStockUnit.txtPrice2CC.LastText = cntStockUnit.txtPrice2CC.Text;
                                        ValidateUnits();
                                    }
                                    if (sender == cntStockUnit.txtPrice3CC && Convert.ToDouble(cntStockUnit.txtPrice3CC.Text) != Convert.ToDouble(cntStockUnit.txtPrice3CC.LastText))
                                    {   
                                        cntStockUnit.txtPrice3LC.Text = cntStockUnit.txtPrice3LC.LastText = (Convert.ToDouble(cntStockUnit.txtPrice3CC.Text) / Rate).ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat);
                                        cntStockUnit.txtPrice3CC.LastText = cntStockUnit.txtPrice3CC.Text;
                                        ValidateUnits();
                                    }
                                }
                            }
                            else
                            {
                                if (sender == cntStockUnit.txtPrice1CC && Convert.ToDouble(cntStockUnit.txtPrice1CC.Text) != Convert.ToDouble(cntStockUnit.txtPrice1CC.LastText))
                                {
                                    cntStockUnit.txtPrice1LC.Text = cntStockUnit.txtPrice1LC.LastText = 0.ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat);
                                    cntStockUnit.txtPrice1CC.LastText = cntStockUnit.txtPrice1CC.Text;
                                    ValidateUnits();
                                }
                                if (sender == cntStockUnit.txtPrice2CC && Convert.ToDouble(cntStockUnit.txtPrice2CC.Text) != Convert.ToDouble(cntStockUnit.txtPrice2CC.LastText))
                                {
                                    cntStockUnit.txtPrice2LC.Text = cntStockUnit.txtPrice2LC.LastText = 0.ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat);
                                    cntStockUnit.txtPrice2CC.LastText = cntStockUnit.txtPrice2CC.Text;
                                    ValidateUnits();
                                }
                                if (sender == cntStockUnit.txtPrice3CC && Convert.ToDouble(cntStockUnit.txtPrice3CC.Text) != Convert.ToDouble(cntStockUnit.txtPrice3CC.LastText))
                                {
                                    cntStockUnit.txtPrice3LC.Text = cntStockUnit.txtPrice3LC.LastText = 0.ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat);
                                    cntStockUnit.txtPrice3CC.LastText = cntStockUnit.txtPrice3CC.Text;
                                    ValidateUnits();
                                }

                            }
                        }

                        txtPrice1CC.Text = cntStockUnit.txtPrice1CC.Text;
                        txtPrice1LL.Text = cntStockUnit.txtPrice1LC.Text;
                        txtPrice2CC.Text = cntStockUnit.txtPrice2CC.Text;
                        txtPrice2LL.Text = cntStockUnit.txtPrice2LC.Text;
                        txtPrice3CC.Text = cntStockUnit.txtPrice3CC.Text;
                        txtPrice3LL.Text = cntStockUnit.txtPrice3LC.Text;
                        txtDiscPrice2.Text = cntStockUnit.txtP1P2Disc.Text;
                        txtDiscPrice3.Text = cntStockUnit.txtP1P3Disc.Text;

                        //txtUnitPrice.Text = rbLC.Checked ?  
                    }
                    else
                    {
                        if (rbBaseCurrencyLC.Checked)
                        {
                            if (sender == cntStockUnit.txtPrice1LC &&
                                    cntStockUnit.txtPrice1LC.Enabled == true &&
                                    Convert.ToDouble(cntStockUnit.txtPrice1LC.Text) != 0 &&
                                    Convert.ToDouble(cntStockUnit.txtPrice1LC.Text) != Convert.ToDouble(cntStockUnit.txtPrice1LC.LastText))
                            {

                                if (cntStockUnit.txtDiscount.AllowEmptyNumeric && cntStockUnit.txtDiscount.Text == "")
                                {
                                    cntStockUnit.txtDiscount.LastText = "";
                                    cntStockUnit.txtPrice1LC.Text = txtPrice1LL.Text;
                                    cntStockUnit.txtPrice1CC.Text = txtPrice1CC.Text;
                                }
                                else if (Convert.ToDouble(cntStockUnit.txtDiscount.Text) != 0 && double.TryParse(cntStockUnit.txtDiscount.Text, out double result))
                                {

                                    ClsPricing.CalculateDiscountAfterChangingPrice(txtPrice1LL.Text, cntStockUnit.txtPrice1LC.Text, txtPrice1CC.Text, ref discount, ref Price);


                                    cntStockUnit.txtDiscount.Text = discount;
                                    cntStockUnit.txtDiscount.LastText = cntStockUnit.txtDiscount.Text;
                                    cntStockUnit.txtPrice1CC.Text = Price.ToString(ClsConfig.locationDirectives.Ld_PriceCCFormat);

                                }

                            }

                            if (sender == cntStockUnit.txtPrice2LC &&
                                   cntStockUnit.txtPrice2LC.Enabled == true &&
                                   Convert.ToDouble(cntStockUnit.txtPrice2LC.Text) != 0 &&
                                   Convert.ToDouble(cntStockUnit.txtPrice2LC.Text) != Convert.ToDouble(cntStockUnit.txtPrice2LC.LastText))
                            {

                                if (cntStockUnit.txtP1P2Disc.AllowEmptyNumeric && cntStockUnit.txtP1P2Disc.Text == "")
                                {
                                    cntStockUnit.txtP1P2Disc.LastText = "";
                                    cntStockUnit.txtPrice2LC.Text = txtPrice2LL.Text;
                                    cntStockUnit.txtPrice2CC.Text = txtPrice2CC.Text;
                                }
                                else if (Convert.ToDouble(cntStockUnit.txtP1P2Disc.Text) != 0 && double.TryParse(cntStockUnit.txtP1P2Disc.Text, out double result))
                                {

                                    //ClsPricing.CalculateDiscountAfterChangingPrice(cntStockUnit.txtPacking.Text, txtPrice2LL.Text, cntStockUnit.txtPrice2LC.Text, txtPrice2CC.Text, ref discount, ref Price);

                                    ClsPricing.CalculateDiscountAfterChangingPrice(
                                                                                 cntStockUnit.txtPrice2LC.LastText,
                                                                                 cntStockUnit.txtPrice2LC.Text,
                                                                                 cntStockUnit.txtPrice2CC.LastText, ref discount, ref Price, cntStockUnit.txtP1P2Disc.Text);

                                    cntStockUnit.txtP1P2Disc.Text = discount;
                                    cntStockUnit.txtP1P2Disc.LastText = cntStockUnit.txtP1P2Disc.Text;
                                    cntStockUnit.txtPrice2CC.Text = Price.ToString(ClsConfig.locationDirectives.Ld_PriceCCFormat);

                                }

                            }
                            
                            if (sender == cntStockUnit.txtPrice3LC &&
                                cntStockUnit.txtPrice3LC.Enabled == true &&
                                Convert.ToDouble(cntStockUnit.txtPrice3LC.Text) != 0 &&
                                Convert.ToDouble(cntStockUnit.txtPrice3LC.Text) != Convert.ToDouble(cntStockUnit.txtPrice3LC.LastText))
                            {
                                if (cntStockUnit.txtP1P3Disc.AllowEmptyNumeric && cntStockUnit.txtP1P3Disc.Text == "")
                                {
                                    cntStockUnit.txtP1P3Disc.LastText = "";
                                    cntStockUnit.txtPrice3LC.Text = txtPrice3LL.Text;
                                    cntStockUnit.txtPrice3CC.Text = txtPrice3CC.Text;
                                }
                                else if (Convert.ToDouble(cntStockUnit.txtP1P3Disc.Text) != 0 && double.TryParse(cntStockUnit.txtP1P3Disc.Text, out double result))
                                {
                                    // ClsPricing.CalculateDiscountAfterChangingPrice(cntStockUnit.txtPacking.Text, txtPrice3LL.Text, cntStockUnit.txtPrice3LC.Text, txtPrice3CC.Text, ref discount, ref Price);


                                    ClsPricing.CalculateDiscountAfterChangingPrice(
                                                                                   cntStockUnit.txtPrice3LC.LastText,
                                                                                   cntStockUnit.txtPrice3LC.Text,
                                                                                   cntStockUnit.txtPrice3CC.LastText, ref discount, ref Price, cntStockUnit.txtP1P3Disc.Text);

                                    cntStockUnit.txtP1P3Disc.Text = discount;
                                    cntStockUnit.txtP1P3Disc.LastText = cntStockUnit.txtP1P3Disc.Text;
                                    cntStockUnit.txtPrice3CC.Text = Price.ToString(ClsConfig.locationDirectives.Ld_PriceCCFormat);

                                }

                            }
                        }
                        else
                        {

                            if (sender == cntStockUnit.txtPrice1CC &&
                                  cntStockUnit.txtPrice1CC.Enabled == true &&
                                  Convert.ToDouble(cntStockUnit.txtPrice1CC.Text) != 0 &&
                                  Convert.ToDouble(cntStockUnit.txtPrice1CC.Text) != Convert.ToDouble(cntStockUnit.txtPrice1CC.LastText))
                            {
                                if (cntStockUnit.txtDiscount.AllowEmptyNumeric && cntStockUnit.txtDiscount.Text == "")
                                {
                                    cntStockUnit.txtDiscount.LastText = "";
                                    cntStockUnit.txtPrice1LC.Text = txtPrice1LL.Text;
                                    cntStockUnit.txtPrice1CC.Text = txtPrice1CC.Text;
                                }
                                else if (Convert.ToDouble(cntStockUnit.txtDiscount.Text) != 0 && double.TryParse(cntStockUnit.txtDiscount.Text, out double result))
                                {
                                    ClsPricing.CalculateDiscountAfterChangingPrice(
                                                                                   cntStockUnit.txtPrice1CC.LastText,
                                                                                   cntStockUnit.txtPrice1CC.Text,
                                                                                   cntStockUnit.txtPrice1LC.LastText, ref discount, ref Price, cntStockUnit.txtDiscount.Text);

                                    cntStockUnit.txtDiscount.Text = discount;
                                    cntStockUnit.txtDiscount.LastText = cntStockUnit.txtDiscount.Text;
                                    cntStockUnit.txtPrice1LC.Text = Price.ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat);
                                }

                            }

                            if (sender == cntStockUnit.txtPrice2CC &&
                                   cntStockUnit.txtPrice2CC.Enabled == true &&
                                   Convert.ToDouble(cntStockUnit.txtPrice2CC.Text) != 0 &&
                                   Convert.ToDouble(cntStockUnit.txtPrice2CC.Text) != Convert.ToDouble(cntStockUnit.txtPrice2CC.LastText))
                            {
                                if (cntStockUnit.txtP1P2Disc.AllowEmptyNumeric && cntStockUnit.txtP1P2Disc.Text == "")
                                {
                                    cntStockUnit.txtP1P2Disc.LastText = "";
                                    cntStockUnit.txtPrice2LC.Text = txtPrice2LL.Text;
                                    cntStockUnit.txtPrice2CC.Text = txtPrice2CC.Text;
                                }
                                else if (Convert.ToDouble(cntStockUnit.txtP1P2Disc.Text) != 0 && double.TryParse(cntStockUnit.txtP1P2Disc.Text, out double result))
                                {

                                    //ClsPricing.CalculateDiscountAfterChangingPrice(cntStockUnit.txtPacking.Text, txtPrice2CC.Text, cntStockUnit.txtPrice2CC.Text, txtPrice2LL.Text, ref discount, ref Price);

                                    ClsPricing.CalculateDiscountAfterChangingPrice(
                                                                                   cntStockUnit.txtPrice2CC.LastText,
                                                                                   cntStockUnit.txtPrice2CC.Text,
                                                                                   cntStockUnit.txtPrice2LC.LastText, ref discount, ref Price, cntStockUnit.txtP1P2Disc.Text);

                                    cntStockUnit.txtP1P2Disc.Text = discount;
                                    cntStockUnit.txtP1P2Disc.LastText = cntStockUnit.txtP1P2Disc.Text;
                                    cntStockUnit.txtPrice2LC.Text = Price.ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat);
                                }

                            }


                            if (sender == cntStockUnit.txtPrice3CC &&
                                cntStockUnit.txtPrice3CC.Enabled == true &&
                                Convert.ToDouble(cntStockUnit.txtPrice3CC.Text) != 0 &&
                                Convert.ToDouble(cntStockUnit.txtPrice3CC.Text) != Convert.ToDouble(cntStockUnit.txtPrice3CC.LastText))
                            {

                                if (cntStockUnit.txtP1P3Disc.AllowEmptyNumeric && cntStockUnit.txtP1P3Disc.Text == "")
                                {
                                    cntStockUnit.txtP1P3Disc.LastText = "";
                                    cntStockUnit.txtPrice3LC.Text = txtPrice3LL.Text;
                                    cntStockUnit.txtPrice3CC.Text = txtPrice3CC.Text;
                                }
                                else if (Convert.ToDouble(cntStockUnit.txtP1P3Disc.Text) != 0 && double.TryParse(cntStockUnit.txtP1P3Disc.Text, out double result))
                                {
                                    //ClsPricing.CalculateDiscountAfterChangingPrice(cntStockUnit.txtPacking.Text, txtPrice3CC.Text, cntStockUnit.txtPrice3CC.Text, txtPrice3LL.Text, ref discount, ref Price);

                                    ClsPricing.CalculateDiscountAfterChangingPrice(
                                                                                  cntStockUnit.txtPrice3CC.LastText,
                                                                                  cntStockUnit.txtPrice3CC.Text,
                                                                                  cntStockUnit.txtPrice3LC.LastText, ref discount, ref Price, cntStockUnit.txtP1P3Disc.Text);

                                    cntStockUnit.txtP1P3Disc.Text = discount;
                                    cntStockUnit.txtP1P3Disc.LastText = cntStockUnit.txtP1P3Disc.Text;
                                    cntStockUnit.txtPrice3LC.Text = Price.ToString(ClsConfig.locationDirectives.Ld_PriceLCFormat);

                                }

                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {

                ClsCommon.ShowException(StepName, ex);
            }
        }

        private void CalculatePriceForUnit()
        {
            try
            {
                if (lcFormMode != ClsCommon.FormMode.Add)
                {
                    return;
                }

                if (flpStockUnits.Controls.Count != 0
                      ||
                  Array.FindAll(flpStockUnits.Controls.Cast<Control>().OfType<cntStockUnit>().ToArray(),
                       cnt => !cnt.Delete && !string.IsNullOrEmpty(cnt.cntUnit.txtCode.Text) && !string.IsNullOrEmpty(cnt.cntParentUnit.txtCode.Text)).Length != 0)

                {
                    foreach (cntStockUnit cnt in flpStockUnits.Controls)
                    {
                        if (!string.IsNullOrEmpty(cnt.cntUnit.Code) && !string.IsNullOrEmpty(cnt.txtPackingBasic.Text))
                        {

                            string Price1LL, Price2LL, Price3LL, Price1CC, Price2CC, Price3CC = "";
                            Price1LL = cnt.txtPrice1LC.Text;
                            Price2LL = cnt.txtPrice2LC.Text;
                            Price3LL = cnt.txtPrice3LC.Text;
                            Price1CC = cnt.txtPrice1CC.Text;
                            Price2CC = cnt.txtPrice2CC.Text;
                            Price3CC = cnt.txtPrice3CC.Text;

                            ClsPricing.CalculatePriceForUnits(Convert.ToDouble(cnt.txtPackingBasic.Text),
                                                              txtPrice1LL.Text, txtPrice1CC.Text,
                                                              txtPrice2LL.Text, txtPrice2CC.Text,
                                                              txtPrice3LL.Text, txtPrice3CC.Text,
                                                              ref Price1LL, ref Price1CC,
                                                              ref Price2LL, ref Price2CC,
                                                              ref Price3LL, ref Price3CC);

                            cnt.txtPrice1LC.Text = cnt.txtPrice1LC.LastText = Price1LL;
                            cnt.txtPrice2LC.Text = cnt.txtPrice2LC.LastText = Price2LL;
                            cnt.txtPrice3LC.Text = cnt.txtPrice3LC.LastText = Price3LL;
                            cnt.txtPrice1CC.Text = cnt.txtPrice1CC.LastText = Price1CC;
                            cnt.txtPrice2CC.Text = cnt.txtPrice2CC.LastText = Price2CC;
                            cnt.txtPrice3CC.Text = cnt.txtPrice3CC.LastText = Price3CC;

                            cnt.txtP1P2Disc.Text = cnt.txtP1P2Disc.LastText = txtDiscPrice2.Text == "" ? "0" : txtDiscPrice2.Text;
                            cnt.txtP1P3Disc.Text = cnt.txtP1P3Disc.LastText = txtDiscPrice3.Text == "" ? "0" : txtDiscPrice3.Text;

                        }
                    }

                }
            }
            catch (Exception)
            {

                throw;
            }

        }

        #endregion

        #region Header

        private void pctItemPic_Click(object sender, EventArgs e)
        {
            try
            {

                if (pctItemPic.Image != null)
                {

                    string tempFileName = $"tempFileSI{Name}_{DateTime.Now.ToString("ddMMyyyyHHmmss")}.png";

                    try
                    {
                        System.IO.Directory.Delete(tempFileName);
                    }
                    catch (Exception)
                    {
                        //No Exp
                    }

                    string tempFilePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), tempFileName);

                    pctItemPic.Image.Save(tempFilePath, System.Drawing.Imaging.ImageFormat.Png);

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = tempFilePath,
                        UseShellExecute = true
                    });

                }

            }
            catch (Exception ex)
            {
                ClsCommon.ShowException(StepName, ex);
            }
        }

        private void ItemImageChanged(object sender, EventArgs e)
        {
            try
            {

                pctItemPic.Image = imgItmPhoto.pctPicture.Image;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void txtDesc_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtDesc.Text)) return;
                lblFullDescription.Text = txtDesc.Text;
                foreach (cntStockUnit cntStockUnit in flpStockUnits.Controls)
                {

                    if (lcFormMode == ClsCommon.FormMode.Add && txtDesc.Text != "")
                    {
                        if (cntStockUnit.cntUnit.Code != "")
                        {
                            if ((txtDesc.Text.Trim().Length + cntStockUnit.cntUnit.Code.Trim().Length + 3) > 200)
                            {
                                cntStockUnit.txtDescription.Text = txtDesc.Text.Trim();
                            }
                            else
                            {
                                cntStockUnit.txtDescription.Text = txtDesc.Text.Trim() + " " + cntStockUnit.cntUnit.Code.Trim();
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ClsCommon.ShowException(StepName, ex);
            }
        }

        #endregion


        #region Stock Dep Grp SubGrp


        //private void cntDepGrpSub.cntDepartment_Validating(object sender, CancelEventArgs e)
        //{
        //    try
        //    {
        //        cntDepGrpSub.cntSubGroup1.DepartmentCode = cntDepGrpSub.cntDepartment.Code;
        //        cntDepGrpSub.cntSubGroup2.DepartmentCode = cntDepGrpSub.cntDepartment.Code;
        //        cntDepGrpSub.cntSubGroup3.DepartmentCode = cntDepGrpSub.cntDepartment.Code;
        //        cntDepGrpSub.cntSubGroup4.DepartmentCode = cntDepGrpSub.cntDepartment.Code;

        //        cntDepGrpSub.cntSubGroup1.GroupCode = cntDepGrpSub.cntGroup.Code;
        //        cntDepGrpSub.cntSubGroup2.GroupCode = cntDepGrpSub.cntGroup.Code;
        //        cntDepGrpSub.cntSubGroup3.GroupCode = cntDepGrpSub.cntGroup.Code;
        //        cntDepGrpSub.cntSubGroup4.GroupCode = cntDepGrpSub.cntGroup.Code;

        //        cntDepGrpSub.cntSubGroup1.SelectedSubGroup1GroupCode = cntDepGrpSub.cntSubGroup1.Code;
        //        cntDepGrpSub.cntSubGroup2.SubGroup1Code = cntDepGrpSub.cntSubGroup1.Code;
        //        cntDepGrpSub.cntSubGroup3.SubGroup1Code = cntDepGrpSub.cntSubGroup1.Code;
        //        cntDepGrpSub.cntSubGroup4.SubGroup1Code = cntDepGrpSub.cntSubGroup1.Code;

        //        cntDepGrpSub.cntSubGroup3.SubGroup2Code = cntDepGrpSub.cntSubGroup2.Code;
        //        cntDepGrpSub.cntSubGroup4.SubGroup2Code = cntDepGrpSub.cntSubGroup2.Code;

        //        cntDepGrpSub.cntSubGroup2.SubGroup3Code = cntDepGrpSub.cntSubGroup3.Code;
        //        cntDepGrpSub.cntSubGroup4.SubGroup3Code = cntDepGrpSub.cntSubGroup3.Code;

        //        cntDepGrpSub.cntSubGroup2.SubGroup4Code = cntDepGrpSub.cntSubGroup4.Code;

        //        FillInfo = 0;
        //        cntDepGrpSub.cntDepartment.isFilled = FillInfo;

        //        if (string.IsNullOrEmpty(cntDepGrpSub.cntDepartment.Code))
        //        {
        //            cntDepGrpSub.cntGroup.DepartmentCode = cntDepGrpSub.cntDepartment.Code;
        //            cntDepGrpSub.cntDepartment.DepartmentRecID = -1;
        //        }

        //        string cntDepGrpSub.cntDepartmentValue = cntDepGrpSub.cntDepartment.Code;
        //        cntDepGrpSub.cntGroup.DepartmentCode = cntDepGrpSub.cntDepartmentValue;
        //        Department department = new Department();
        //        DepartmentController dptController = new DepartmentController();

        //        if (cntDepGrpSub.cntDepartment.DepartmentRecID != -1)
        //            department = dptController.GetDepartment(cntDepGrpSub.cntDepartment.DepartmentRecID);

        //        else
        //            department = dptController.GetDepartment(cntDepGrpSub.cntDepartment.Code);

        //        FillInfo = 0;
        //    }
        //    catch (Exception ex)
        //    {
        //        ClsCommon.ShowException(StepName, ex);
        //    }
        //}

        //private void cntDepGrpSub.cntGroup_Validating(object sender, CancelEventArgs e)
        //{
        //    try
        //    {
        //        cntDepGrpSub.cntSubGroup1.GroupCode = cntDepGrpSub.cntGroup.Code;
        //        cntDepGrpSub.cntSubGroup2.GroupCode = cntDepGrpSub.cntGroup.Code;
        //        cntDepGrpSub.cntSubGroup3.GroupCode = cntDepGrpSub.cntGroup.Code;
        //        cntDepGrpSub.cntSubGroup4.GroupCode = cntDepGrpSub.cntGroup.Code;

        //        FillInfo = 0;
        //        cntDepGrpSub.cntGroup.isFilled = FillInfo;

        //        if (string.IsNullOrEmpty(cntDepGrpSub.cntGroup.Code))
        //            cntDepGrpSub.cntGroup.GroupRecID = -1;

        //        if (string.IsNullOrEmpty(cntDepGrpSub.cntDepartment.Code))
        //            cntDepGrpSub.cntDepartment.DepartmentRecID = -1;

        //        cntDepGrpSub.cntDepartment.TextGroupCode = cntDepGrpSub.cntGroup.Code;

        //        if (string.IsNullOrEmpty(cntDepGrpSub.cntGroup.Code)) { return; }

        //        Group group = new Group();
        //        GroupController grpcontroller = new GroupController();

        //        if (string.IsNullOrEmpty(cntDepGrpSub.cntDepartment.Code))
        //        {
        //            if (cntDepGrpSub.cntGroup.GroupRecID != -1)
        //                group = grpcontroller.GetGroup(cntDepGrpSub.cntGroup.GroupRecID);

        //            else
        //                group = grpcontroller.GetGroup(cntDepGrpSub.cntGroup.Code?.ToUpper());


        //        }

        //        else if (!string.IsNullOrEmpty(cntDepGrpSub.cntDepartment.Code))
        //        {
        //            if (cntDepGrpSub.cntGroup.GroupRecID != -1)
        //                group = grpcontroller.GetGroup(cntDepGrpSub.cntGroup.GroupRecID);

        //            else
        //                group = grpcontroller.GetGroup(cntDepGrpSub.cntGroup.Code, cntDepGrpSub.cntDepartment.Code);


        //        }
        //        FillInfo = 0;
        //        cntDepGrpSub.cntGroup_Validated(cntDepGrpSub.cntGroup.Code?.ToUpper(), new CancelEventArgs());
        //    }
        //    catch (Exception ex)
        //    {
        //        ClsCommon.ShowException(StepName, ex);
        //    }
        //}

        //private void cntDepGrpSub.cntSubGroup1_Validating(object sender, CancelEventArgs e)
        //{
        //    try
        //    {
        //        cntDepGrpSub.cntSubGroup2.SubGroup1Code = cntDepGrpSub.cntSubGroup1.Code;
        //        FillInfo = 0;
        //        cntDepGrpSub.cntSubGroup1.isFilled = FillInfo;
        //        cntDepGrpSub.cntSubGroup2.SubGroup1Code = cntDepGrpSub.cntSubGroup1.Code;
        //        cntDepGrpSub.cntSubGroup3.SubGroup1Code = cntDepGrpSub.cntSubGroup1.Code;
        //        cntDepGrpSub.cntSubGroup4.SubGroup1Code = cntDepGrpSub.cntSubGroup1.Code;

        //        if (string.IsNullOrEmpty(cntDepGrpSub.cntSubGroup1.Code))
        //            cntDepGrpSub.cntSubGroup1.SubGroup1RecID = -1;

        //        if (string.IsNullOrEmpty(cntDepGrpSub.cntGroup.Code))
        //            cntDepGrpSub.cntGroup.GroupRecID = -1;

        //        if (string.IsNullOrEmpty(cntDepGrpSub.cntDepartment.Code))
        //            cntDepGrpSub.cntDepartment.DepartmentRecID = -1;

        //        if (string.IsNullOrEmpty(cntDepGrpSub.cntSubGroup1.Code)) { return; }

        //        SubGroup1 subgroup1 = new SubGroup1();
        //        SubGroup1Controller subgroup1Controller = new SubGroup1Controller();

        //        if (string.IsNullOrEmpty(cntDepGrpSub.cntGroup.Code))
        //        {
        //            if (cntDepGrpSub.cntSubGroup1.SubGroup1RecID != -1)
        //                subgroup1 = subgroup1Controller.GetSubGroup1(cntDepGrpSub.cntSubGroup1.SubGroup1RecID);

        //            else
        //                subgroup1 = subgroup1Controller.GetSubGroup1(cntDepGrpSub.cntSubGroup1.Code);

        //        }

        //        else if (!string.IsNullOrEmpty(cntDepGrpSub.cntGroup.Code))
        //        {
        //            if (cntDepGrpSub.cntSubGroup1.SubGroup1RecID != -1)
        //                subgroup1 = subgroup1Controller.GetSubGroup1(cntDepGrpSub.cntSubGroup1.SubGroup1RecID);

        //            else
        //                subgroup1 = subgroup1Controller.GetSubGroup1(cntDepGrpSub.cntSubGroup1.Code, cntDepGrpSub.cntGroup.Code);


        //        }
        //        FillInfo = 0;
        //        cntDepGrpSub.cntSubGroup1_Validated(cntDepGrpSub.cntSubGroup1.Code, new CancelEventArgs());
        //    }
        //    catch (Exception ex)
        //    {
        //        ClsCommon.ShowException(StepName, ex);
        //    }
        //}

        //private void cntDepGrpSub.cntSubGroup2_Validating(object sender, CancelEventArgs e)
        //{
        //    try
        //    {
        //        cntDepGrpSub.cntSubGroup3.SubGroup2Code = cntDepGrpSub.cntSubGroup2.Code;
        //        FillInfo = 0;
        //        cntDepGrpSub.cntSubGroup2.isFilled = FillInfo;
        //        cntDepGrpSub.cntSubGroup3.SubGroup2Code = cntDepGrpSub.cntSubGroup2.Code;
        //        cntDepGrpSub.cntSubGroup4.SubGroup2Code = cntDepGrpSub.cntSubGroup2.Code;

        //        if (string.IsNullOrEmpty(cntDepGrpSub.cntSubGroup2.Code))
        //            cntDepGrpSub.cntSubGroup2.Sub2RecID = -1;

        //        if (string.IsNullOrEmpty(cntDepGrpSub.cntSubGroup1.Code))
        //            cntDepGrpSub.cntSubGroup1.SubGroup1RecID = -1;

        //        if (string.IsNullOrEmpty(cntDepGrpSub.cntGroup.Code))
        //            cntDepGrpSub.cntGroup.GroupRecID = -1;

        //        if (string.IsNullOrEmpty(cntDepGrpSub.cntDepartment.Code))
        //            cntDepGrpSub.cntDepartment.DepartmentRecID = -1;

        //        if (string.IsNullOrEmpty(cntDepGrpSub.cntSubGroup2.Code)) { return; }

        //        SubGroup2 subgroup2 = new SubGroup2();
        //        SubGroup2Controller subgroup2Controller = new SubGroup2Controller();

        //        if (string.IsNullOrEmpty(cntDepGrpSub.cntSubGroup1.Code))
        //        {
        //            if (cntDepGrpSub.cntSubGroup2.Sub2RecID != -1)
        //                subgroup2 = subgroup2Controller.GetSubGroup2(cntDepGrpSub.cntSubGroup2.Sub2RecID);

        //            else
        //                subgroup2 = subgroup2Controller.GetSubGroup2(cntDepGrpSub.cntSubGroup2.Code);


        //        }

        //        else if (!string.IsNullOrEmpty(cntDepGrpSub.cntSubGroup1.Code))
        //        {
        //            if (cntDepGrpSub.cntSubGroup2.Sub2RecID != -1)
        //                subgroup2 = subgroup2Controller.GetSubGroup2(cntDepGrpSub.cntSubGroup2.Sub2RecID);

        //            else
        //                subgroup2 = subgroup2Controller.GetSubGroup2(cntDepGrpSub.cntSubGroup2.Code, cntDepGrpSub.cntSubGroup1.Code);


        //        }
        //        FillInfo = 0;
        //        cntDepGrpSub.cntSubGroup2_Validated(cntDepGrpSub.cntSubGroup2.Code, new CancelEventArgs());
        //    }
        //    catch (Exception ex)
        //    {
        //        ClsCommon.ShowException(StepName, ex);
        //    }
        //}

        //private void cntDepGrpSub.cntSubGroup3_Validating(object sender, CancelEventArgs e)
        //{
        //    try
        //    {
        //        FillInfo = 0;
        //        cntDepGrpSub.cntSubGroup3.isFilled = FillInfo;
        //        cntDepGrpSub.cntSubGroup4.SubGroup3Code = cntDepGrpSub.cntSubGroup3.Code;

        //        if (string.IsNullOrEmpty(cntDepGrpSub.cntSubGroup3.Code))
        //            cntDepGrpSub.cntSubGroup3.Sub3RecID = -1;

        //        if (string.IsNullOrEmpty(cntDepGrpSub.cntSubGroup2.Code))
        //            cntDepGrpSub.cntSubGroup2.Sub2RecID = -1;

        //        if (string.IsNullOrEmpty(cntDepGrpSub.cntSubGroup1.Code))
        //            cntDepGrpSub.cntSubGroup1.SubGroup1RecID = -1;

        //        if (string.IsNullOrEmpty(cntDepGrpSub.cntGroup.Code))
        //            cntDepGrpSub.cntGroup.GroupRecID = -1;

        //        if (string.IsNullOrEmpty(cntDepGrpSub.cntDepartment.Code))
        //            cntDepGrpSub.cntDepartment.DepartmentRecID = -1;

        //        if (string.IsNullOrEmpty(cntDepGrpSub.cntSubGroup3.Code)) { return; }

        //        SubGroup3 subgroup3 = new SubGroup3();
        //        SubGroup3Controller subgroup3Controller = new SubGroup3Controller();

        //        if (string.IsNullOrEmpty(cntDepGrpSub.cntSubGroup2.Code))
        //        {
        //            if (cntDepGrpSub.cntSubGroup3.Sub3RecID != -1)
        //                subgroup3 = subgroup3Controller.GetSubGroup3(cntDepGrpSub.cntSubGroup3.Sub3RecID);

        //            else
        //                subgroup3 = subgroup3Controller.GetSubGroup3(cntDepGrpSub.cntSubGroup3.Code);


        //        }

        //        else if (!string.IsNullOrEmpty(cntDepGrpSub.cntSubGroup2.Code))
        //        {
        //            if (cntDepGrpSub.cntSubGroup3.Sub3RecID != -1)
        //                subgroup3 = subgroup3Controller.GetSubGroup3(cntDepGrpSub.cntSubGroup3.Sub3RecID);

        //            else
        //                subgroup3 = subgroup3Controller.GetSubGroup3(cntDepGrpSub.cntSubGroup3.Code, cntDepGrpSub.cntSubGroup2.Code, cntDepGrpSub.cntSubGroup1.Code, cntDepGrpSub.cntGroup.Code, cntDepGrpSub.cntDepartment.Code);


        //        }
        //        FillInfo = 0;
        //        cntDepGrpSub.cntSubGroup3_Validated(cntDepGrpSub.cntSubGroup3.Code, new CancelEventArgs());
        //    }
        //    catch (Exception ex)
        //    {
        //        ClsCommon.ShowException(StepName, ex);
        //    }
        //}

        //private void cntDepGrpSub.cntSubGroup4_Validating(object sender, CancelEventArgs e)
        //{
        //    try
        //    {
        //        FillInfo = 0;
        //        cntDepGrpSub.cntSubGroup4.isFilled = FillInfo;

        //        if (string.IsNullOrEmpty(cntDepGrpSub.cntSubGroup4.Code))
        //            cntDepGrpSub.cntSubGroup4.Sub4RecID = -1;

        //        if (string.IsNullOrEmpty(cntDepGrpSub.cntSubGroup3.Code))
        //            cntDepGrpSub.cntSubGroup3.Sub3RecID = -1;

        //        if (string.IsNullOrEmpty(cntDepGrpSub.cntSubGroup2.Code))
        //            cntDepGrpSub.cntSubGroup2.Sub2RecID = -1;

        //        if (string.IsNullOrEmpty(cntDepGrpSub.cntSubGroup1.Code))
        //            cntDepGrpSub.cntSubGroup1.SubGroup1RecID = -1;

        //        if (string.IsNullOrEmpty(cntDepGrpSub.cntGroup.Code))
        //            cntDepGrpSub.cntGroup.GroupRecID = -1;

        //        if (string.IsNullOrEmpty(cntDepGrpSub.cntDepartment.Code))
        //            cntDepGrpSub.cntDepartment.DepartmentRecID = -1;

        //        if (string.IsNullOrEmpty(cntDepGrpSub.cntSubGroup4.Code)) { return; }

        //        SubGroup4 subgroup4 = new SubGroup4();
        //        SubGroup4Controller subgroup4Controller = new SubGroup4Controller();

        //        if (string.IsNullOrEmpty(cntDepGrpSub.cntSubGroup3.Code))
        //        {
        //            if (cntDepGrpSub.cntSubGroup4.Sub4RecID != -1)
        //                subgroup4 = subgroup4Controller.GetSubGroup4(cntDepGrpSub.cntSubGroup4.Sub4RecID);

        //            else
        //                subgroup4 = subgroup4Controller.GetSubGroup4(cntDepGrpSub.cntSubGroup4.Code);


        //        }
        //        else if (!string.IsNullOrEmpty(cntDepGrpSub.cntSubGroup3.Code))
        //        {
        //            if (cntDepGrpSub.cntSubGroup4.Sub4RecID != -1)
        //                subgroup4 = subgroup4Controller.GetSubGroup4(cntDepGrpSub.cntSubGroup4.Sub4RecID);

        //            else
        //                subgroup4 = subgroup4Controller.GetSubGroup4(cntDepGrpSub.cntSubGroup4.Code, cntDepGrpSub.cntSubGroup3.Code, cntDepGrpSub.cntSubGroup2.Code, cntDepGrpSub.cntSubGroup1.Code, cntDepGrpSub.cntGroup.Code, cntDepGrpSub.cntDepartment.Code);


        //        }
        //        FillInfo = 0;
        //        cntDepGrpSub.cntSubGroup4_Validated(cntDepGrpSub.cntSubGroup4.Code, new CancelEventArgs());
        //    }
        //    catch (Exception ex)
        //    {
        //        ClsCommon.ShowException(StepName, ex);
        //    }
        //}

        //private void cntDepGrpSub.cntGroup_Validated(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        Department department = new Department();
        //        DepartmentController depController = new DepartmentController();
        //        department = depController.GetDepartment(cntDepGrpSub.cntGroup.DepartmentRecID);

        //        if (department != null && !string.IsNullOrEmpty(department.dep_Code))
        //        {
        //            FillInfo = 1;
        //            cntDepGrpSub.cntDepartment.isFilled = FillInfo;
        //            cntDepGrpSub.cntGroup.isFilled = FillInfo;

        //            cntDepGrpSub.cntDepartment.Code = department.dep_Code;
        //            cntDepGrpSub.cntGroup.DepartmentCode = department.dep_Code;
        //            cntDepGrpSub.cntSubGroup1.DepartmentCode = department.dep_Code;
        //            cntDepGrpSub.cntSubGroup2.DepartmentCode = department.dep_Code;
        //            cntDepGrpSub.cntSubGroup3.DepartmentCode = department.dep_Code;
        //            cntDepGrpSub.cntSubGroup4.DepartmentCode = department.dep_Code;

        //            FillInfo = 0;
        //            cntDepGrpSub.cntGroup.isFilled = FillInfo;
        //            cntDepGrpSub.cntDepartment.isFilled = FillInfo;
        //        }
        //        cntDepGrpSub.cntDepartment_Validating(cntDepGrpSub.cntDepartment.Code, new CancelEventArgs());
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        //private void cntDepGrpSub.cntSubGroup1_Validated(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        Group group = new Group();
        //        GroupController groupController = new GroupController();
        //        group = groupController.GetGroup(cntDepGrpSub.cntSubGroup1.GroupRecID);

        //        if (group != null && !string.IsNullOrEmpty(group.grp_Code))
        //        {
        //            FillInfo = 1;
        //            cntDepGrpSub.cntDepartment.isFilled = FillInfo;
        //            cntDepGrpSub.cntGroup.isFilled = FillInfo;
        //            cntDepGrpSub.cntSubGroup1.isFilled = FillInfo;

        //            cntDepGrpSub.cntGroup.Code = group.grp_Code;
        //            cntDepGrpSub.cntSubGroup1.GroupCode = group.grp_Code;
        //            cntDepGrpSub.cntSubGroup2.GroupCode = group.grp_Code;
        //            cntDepGrpSub.cntSubGroup3.GroupCode = group.grp_Code;
        //            cntDepGrpSub.cntSubGroup4.GroupCode = group.grp_Code;

        //            FillInfo = 0;
        //            cntDepGrpSub.cntDepartment.isFilled = FillInfo;
        //            cntDepGrpSub.cntGroup.isFilled = FillInfo;
        //            cntDepGrpSub.cntSubGroup1.isFilled = FillInfo;

        //            DepartmentController depController = new DepartmentController();
        //            Department department = depController.GetDepartment(group.grp_DeptRecID);
        //            if (department != null && !string.IsNullOrEmpty(department.dep_Code))
        //            {
        //                FillInfo = 1;
        //                cntDepGrpSub.cntGroup.isFilled = FillInfo;
        //                cntDepGrpSub.cntDepartment.isFilled = FillInfo;

        //                cntDepGrpSub.cntDepartment.Code = department.dep_Code;
        //                cntDepGrpSub.cntSubGroup1.DepartmentCode = department.dep_Code;
        //                cntDepGrpSub.cntSubGroup2.DepartmentCode = department.dep_Code;
        //                cntDepGrpSub.cntSubGroup3.DepartmentCode = department.dep_Code;
        //                cntDepGrpSub.cntSubGroup4.DepartmentCode = department.dep_Code;

        //                FillInfo = 0;
        //                cntDepGrpSub.cntGroup.isFilled = FillInfo;
        //                cntDepGrpSub.cntDepartment.isFilled = FillInfo;
        //            }
        //        }
        //        cntDepGrpSub.cntDepartment_Validating(cntDepGrpSub.cntDepartment.Code, new CancelEventArgs());
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        //private void cntDepGrpSub.cntSubGroup2_Validated(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        SubGroup1 sub1 = new SubGroup1();
        //        SubGroup1Controller sub1Controller = new SubGroup1Controller();
        //        sub1 = sub1Controller.GetSubGroup1(cntDepGrpSub.cntSubGroup2.Sub1RecID);

        //        if (sub1 != null && !string.IsNullOrEmpty(sub1.SG1_Code))
        //        {
        //            FillInfo = 1;
        //            cntDepGrpSub.cntDepartment.isFilled = FillInfo;
        //            cntDepGrpSub.cntGroup.isFilled = FillInfo;
        //            cntDepGrpSub.cntSubGroup1.isFilled = FillInfo;
        //            cntDepGrpSub.cntSubGroup2.isFilled = FillInfo;

        //            FillInfo = 0;
        //            cntDepGrpSub.cntDepartment.isFilled = FillInfo;
        //            cntDepGrpSub.cntGroup.isFilled = FillInfo;
        //            cntDepGrpSub.cntSubGroup1.isFilled = FillInfo;
        //            cntDepGrpSub.cntSubGroup2.isFilled = FillInfo;

        //            cntDepGrpSub.cntSubGroup1.SubGroup1RecID = sub1.SG1_RecID;
        //            cntDepGrpSub.cntSubGroup2.Sub1RecID = cntDepGrpSub.cntSubGroup1.SubGroup1RecID;
        //            cntDepGrpSub.cntSubGroup3.Sub1RecID = cntDepGrpSub.cntSubGroup1.SubGroup1RecID;
        //            cntDepGrpSub.cntSubGroup2.SubGroup1Code = sub1.SG1_Code;
        //            cntDepGrpSub.cntSubGroup3.SubGroup1Code = sub1.SG1_Code;
        //            cntDepGrpSub.cntSubGroup4.SubGroup1Code = sub1.SG1_Code;

        //            Group group = new Group();
        //            GroupController groupController = new GroupController();
        //            group = groupController.GetGroup(sub1.SG1_GrpRecID);

        //            if (group != null && !string.IsNullOrEmpty(group.grp_Code))
        //            {
        //                FillInfo = 1;
        //                cntDepGrpSub.cntDepartment.isFilled = FillInfo;
        //                cntDepGrpSub.cntGroup.isFilled = FillInfo;
        //                cntDepGrpSub.cntSubGroup1.isFilled = FillInfo;

        //                cntDepGrpSub.cntGroup.Code = group.grp_Code;
        //                cntDepGrpSub.cntSubGroup1.GroupCode = group.grp_Code;
        //                cntDepGrpSub.cntSubGroup2.GroupCode = group.grp_Code;
        //                cntDepGrpSub.cntSubGroup3.GroupCode = group.grp_Code;
        //                cntDepGrpSub.cntSubGroup4.GroupCode = group.grp_Code;
        //                cntDepGrpSub.cntSubGroup1.Code = sub1.SG1_Code;

        //                FillInfo = 0;
        //                cntDepGrpSub.cntDepartment.isFilled = FillInfo;
        //                cntDepGrpSub.cntGroup.isFilled = FillInfo;
        //                cntDepGrpSub.cntSubGroup1.isFilled = FillInfo;

        //                DepartmentController depController = new DepartmentController();
        //                Department department = depController.GetDepartment(group.grp_DeptRecID);
        //                if (department != null && !string.IsNullOrEmpty(department.dep_Code))
        //                {
        //                    FillInfo = 1;
        //                    cntDepGrpSub.cntGroup.isFilled = FillInfo;
        //                    cntDepGrpSub.cntDepartment.isFilled = FillInfo;

        //                    cntDepGrpSub.cntDepartment.Code = department.dep_Code;
        //                    cntDepGrpSub.cntSubGroup1.DepartmentCode = department.dep_Code;
        //                    cntDepGrpSub.cntSubGroup2.DepartmentCode = department.dep_Code;
        //                    cntDepGrpSub.cntSubGroup3.DepartmentCode = department.dep_Code;
        //                    cntDepGrpSub.cntSubGroup4.DepartmentCode = department.dep_Code;

        //                    FillInfo = 0;
        //                    cntDepGrpSub.cntGroup.isFilled = FillInfo;
        //                    cntDepGrpSub.cntDepartment.isFilled = FillInfo;
        //                }
        //            }
        //            cntDepGrpSub.cntDepartment_Validating(cntDepGrpSub.cntDepartment.Code, new CancelEventArgs());
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        //private void cntDepGrpSub.cntSubGroup3_Validated(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        SubGroup2 sub2 = new SubGroup2();
        //        SubGroup2Controller sub2Controller = new SubGroup2Controller();

        //        sub2 = sub2Controller.GetSubGroup2(cntDepGrpSub.cntSubGroup3.Sub2RecID);

        //        if (sub2 != null && !string.IsNullOrEmpty(sub2.SG2_Code))
        //        {
        //            FillInfo = 1;
        //            cntDepGrpSub.cntDepartment.isFilled = FillInfo;
        //            cntDepGrpSub.cntGroup.isFilled = FillInfo;
        //            cntDepGrpSub.cntSubGroup1.isFilled = FillInfo;
        //            cntDepGrpSub.cntSubGroup2.isFilled = FillInfo;
        //            cntDepGrpSub.cntSubGroup3.isFilled = FillInfo;

        //            FillInfo = 0;

        //            cntDepGrpSub.cntDepartment.isFilled = FillInfo;
        //            cntDepGrpSub.cntGroup.isFilled = FillInfo;
        //            cntDepGrpSub.cntSubGroup1.isFilled = FillInfo;
        //            cntDepGrpSub.cntSubGroup2.isFilled = FillInfo;
        //            cntDepGrpSub.cntSubGroup3.isFilled = FillInfo;

        //            cntDepGrpSub.cntSubGroup2.Sub2RecID = cntDepGrpSub.cntSubGroup2.Sub2RecID;
        //            cntDepGrpSub.cntSubGroup3.Sub2RecID = cntDepGrpSub.cntSubGroup2.Sub2RecID;

        //            cntDepGrpSub.cntSubGroup3.SubGroup2Code = sub2.SG2_Code;
        //            cntDepGrpSub.cntSubGroup4.SubGroup2Code = sub2.SG2_Code;

        //            SubGroup1 sub1 = new SubGroup1();
        //            SubGroup1Controller sub1Controller = new SubGroup1Controller();
        //            sub1 = sub1Controller.GetSubGroup1(sub2.SG2_SG1RecID);

        //            if (sub1 != null && !string.IsNullOrEmpty(sub1.SG1_Code))
        //            {
        //                FillInfo = 1;

        //                cntDepGrpSub.cntDepartment.isFilled = FillInfo;
        //                cntDepGrpSub.cntGroup.isFilled = FillInfo;
        //                cntDepGrpSub.cntSubGroup1.isFilled = FillInfo;
        //                cntDepGrpSub.cntSubGroup2.isFilled = FillInfo;
        //                cntDepGrpSub.cntSubGroup2.Code = sub2.SG2_Code;

        //                FillInfo = 0;

        //                cntDepGrpSub.cntDepartment.isFilled = FillInfo;
        //                cntDepGrpSub.cntGroup.isFilled = FillInfo;
        //                cntDepGrpSub.cntSubGroup1.isFilled = FillInfo;
        //                cntDepGrpSub.cntSubGroup2.isFilled = FillInfo;

        //                cntDepGrpSub.cntSubGroup1.SubGroup1RecID = sub1.SG1_RecID;
        //                cntDepGrpSub.cntSubGroup2.Sub1RecID = cntDepGrpSub.cntSubGroup1.SubGroup1RecID;
        //                cntDepGrpSub.cntSubGroup3.Sub1RecID = cntDepGrpSub.cntSubGroup1.SubGroup1RecID;
        //                cntDepGrpSub.cntSubGroup2.SubGroup1Code = sub1.SG1_Code;
        //                cntDepGrpSub.cntSubGroup3.SubGroup1Code = sub1.SG1_Code;
        //                cntDepGrpSub.cntSubGroup4.SubGroup1Code = sub1.SG1_Code;

        //                Group group = new Group();
        //                GroupController groupController = new GroupController();
        //                group = groupController.GetGroup(sub1.SG1_GrpRecID);

        //                if (group != null && !string.IsNullOrEmpty(group.grp_Code))
        //                {
        //                    FillInfo = 1;

        //                    cntDepGrpSub.cntDepartment.isFilled = FillInfo;
        //                    cntDepGrpSub.cntGroup.isFilled = FillInfo;
        //                    cntDepGrpSub.cntSubGroup1.isFilled = FillInfo;

        //                    cntDepGrpSub.cntGroup.Code = group.grp_Code;
        //                    cntDepGrpSub.cntSubGroup1.GroupCode = group.grp_Code;
        //                    cntDepGrpSub.cntSubGroup2.GroupCode = group.grp_Code;
        //                    cntDepGrpSub.cntSubGroup3.GroupCode = group.grp_Code;
        //                    cntDepGrpSub.cntSubGroup4.GroupCode = group.grp_Code;
        //                    cntDepGrpSub.cntSubGroup1.Code = sub1.SG1_Code;

        //                    FillInfo = 0;

        //                    cntDepGrpSub.cntDepartment.isFilled = FillInfo;
        //                    cntDepGrpSub.cntGroup.isFilled = FillInfo;
        //                    cntDepGrpSub.cntSubGroup1.isFilled = FillInfo;

        //                    DepartmentController depController = new DepartmentController();
        //                    Department department = depController.GetDepartment(group.grp_DeptRecID);
        //                    if (department != null && !string.IsNullOrEmpty(department.dep_Code))
        //                    {
        //                        FillInfo = 1;

        //                        cntDepGrpSub.cntGroup.isFilled = FillInfo;
        //                        cntDepGrpSub.cntDepartment.isFilled = FillInfo;

        //                        cntDepGrpSub.cntDepartment.Code = department.dep_Code;
        //                        cntDepGrpSub.cntSubGroup1.DepartmentCode = department.dep_Code;
        //                        cntDepGrpSub.cntSubGroup2.DepartmentCode = department.dep_Code;
        //                        cntDepGrpSub.cntSubGroup3.DepartmentCode = department.dep_Code;
        //                        cntDepGrpSub.cntSubGroup4.DepartmentCode = department.dep_Code;

        //                        FillInfo = 0;

        //                        cntDepGrpSub.cntGroup.isFilled = FillInfo;
        //                        cntDepGrpSub.cntDepartment.isFilled = FillInfo;
        //                    }
        //                }
        //            }
        //        }

        //        FillInfo = 0;
        //        cntDepGrpSub.cntDepartment.isFilled = FillInfo;
        //        cntDepGrpSub.cntGroup.isFilled = FillInfo;
        //        cntDepGrpSub.cntSubGroup1.isFilled = FillInfo;
        //        cntDepGrpSub.cntSubGroup2.isFilled = FillInfo;
        //        cntDepGrpSub.cntSubGroup2.isFilled = FillInfo;
        //        cntDepGrpSub.cntSubGroup1.isFilled = FillInfo;
        //        cntDepGrpSub.cntSubGroup2.isFilled = FillInfo;

        //        cntDepGrpSub.cntSubGroup2.SubGroup1Code = cntDepGrpSub.cntSubGroup1.Code;
        //        cntDepGrpSub.cntSubGroup2.GroupCode = cntDepGrpSub.cntGroup.Code;
        //        cntDepGrpSub.cntSubGroup2.DepartmentCode = cntDepGrpSub.cntDepartment.Code;
        //        cntDepGrpSub.cntSubGroup1.GroupCode = cntDepGrpSub.cntGroup.Code;
        //        cntDepGrpSub.cntSubGroup1.DepartmentCode = cntDepGrpSub.cntDepartment.Code;
        //        cntDepGrpSub.cntGroup.DepartmentCode = cntDepGrpSub.cntDepartment.Code;
        //        cntDepGrpSub.cntDepartment_Validating(cntDepGrpSub.cntDepartment.Code, new CancelEventArgs());
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        //private void cntDepGrpSub.cntSubGroup4_Validated(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        SubGroup3 sub3 = new SubGroup3();
        //        SubGroup3Controller sub3Controller = new SubGroup3Controller();

        //        sub3 = sub3Controller.GetSubGroup3(cntDepGrpSub.cntSubGroup4.Sub3RecID);

        //        if (sub3 != null && !string.IsNullOrEmpty(sub3.SG3_Code))
        //        {
        //            FillInfo = 1;
        //            cntDepGrpSub.cntDepartment.isFilled = FillInfo;
        //            cntDepGrpSub.cntGroup.isFilled = FillInfo;
        //            cntDepGrpSub.cntSubGroup1.isFilled = FillInfo;
        //            cntDepGrpSub.cntSubGroup2.isFilled = FillInfo;
        //            cntDepGrpSub.cntSubGroup3.isFilled = FillInfo;
        //            cntDepGrpSub.cntSubGroup4.isFilled = FillInfo;

        //            FillInfo = 0;

        //            cntDepGrpSub.cntDepartment.isFilled = FillInfo;
        //            cntDepGrpSub.cntGroup.isFilled = FillInfo;
        //            cntDepGrpSub.cntSubGroup1.isFilled = FillInfo;
        //            cntDepGrpSub.cntSubGroup2.isFilled = FillInfo;
        //            cntDepGrpSub.cntSubGroup3.isFilled = FillInfo;
        //            cntDepGrpSub.cntSubGroup4.isFilled = FillInfo;

        //            cntDepGrpSub.cntSubGroup3.Sub3RecID = cntDepGrpSub.cntSubGroup3.Sub3RecID;
        //            cntDepGrpSub.cntSubGroup4.Sub3RecID = cntDepGrpSub.cntSubGroup3.Sub3RecID;

        //            cntDepGrpSub.cntSubGroup4.SubGroup3Code = sub3.SG3_Code;

        //            SubGroup2 sub2 = new SubGroup2();
        //            SubGroup2Controller sub2Controller = new SubGroup2Controller();

        //            sub2 = sub2Controller.GetSubGroup2(sub3.SG3_SG2RecID);

        //            if (sub2 != null && !string.IsNullOrEmpty(sub2.SG2_Code))
        //            {
        //                FillInfo = 1;
        //                cntDepGrpSub.cntDepartment.isFilled = FillInfo;
        //                cntDepGrpSub.cntGroup.isFilled = FillInfo;
        //                cntDepGrpSub.cntSubGroup1.isFilled = FillInfo;
        //                cntDepGrpSub.cntSubGroup2.isFilled = FillInfo;
        //                cntDepGrpSub.cntSubGroup3.isFilled = FillInfo;
        //                cntDepGrpSub.cntSubGroup3.Code = sub3.SG3_Code;

        //                FillInfo = 0;

        //                cntDepGrpSub.cntDepartment.isFilled = FillInfo;
        //                cntDepGrpSub.cntGroup.isFilled = FillInfo;
        //                cntDepGrpSub.cntSubGroup1.isFilled = FillInfo;
        //                cntDepGrpSub.cntSubGroup2.isFilled = FillInfo;
        //                cntDepGrpSub.cntSubGroup3.isFilled = FillInfo;

        //                cntDepGrpSub.cntSubGroup2.Sub2RecID = cntDepGrpSub.cntSubGroup2.Sub2RecID;
        //                cntDepGrpSub.cntSubGroup3.Sub2RecID = cntDepGrpSub.cntSubGroup2.Sub2RecID;
        //                cntDepGrpSub.cntSubGroup4.Sub2RecID = cntDepGrpSub.cntSubGroup2.Sub2RecID;

        //                cntDepGrpSub.cntSubGroup3.SubGroup2Code = sub2.SG2_Code;
        //                cntDepGrpSub.cntSubGroup4.SubGroup2Code = sub2.SG2_Code;

        //                SubGroup1 sub1 = new SubGroup1();
        //                SubGroup1Controller sub1Controller = new SubGroup1Controller();
        //                sub1 = sub1Controller.GetSubGroup1(sub2.SG2_SG1RecID);

        //                if (sub1 != null && !string.IsNullOrEmpty(sub1.SG1_Code))
        //                {
        //                    FillInfo = 1;

        //                    cntDepGrpSub.cntDepartment.isFilled = FillInfo;
        //                    cntDepGrpSub.cntGroup.isFilled = FillInfo;
        //                    cntDepGrpSub.cntSubGroup1.isFilled = FillInfo;
        //                    cntDepGrpSub.cntSubGroup2.isFilled = FillInfo;
        //                    cntDepGrpSub.cntSubGroup2.Code = sub2.SG2_Code;

        //                    FillInfo = 0;

        //                    cntDepGrpSub.cntDepartment.isFilled = FillInfo;
        //                    cntDepGrpSub.cntGroup.isFilled = FillInfo;
        //                    cntDepGrpSub.cntSubGroup1.isFilled = FillInfo;
        //                    cntDepGrpSub.cntSubGroup2.isFilled = FillInfo;

        //                    cntDepGrpSub.cntSubGroup1.SubGroup1RecID = sub1.SG1_RecID;
        //                    cntDepGrpSub.cntSubGroup2.Sub1RecID = cntDepGrpSub.cntSubGroup1.SubGroup1RecID;
        //                    cntDepGrpSub.cntSubGroup3.Sub1RecID = cntDepGrpSub.cntSubGroup1.SubGroup1RecID;
        //                    cntDepGrpSub.cntSubGroup4.Sub1RecID = cntDepGrpSub.cntSubGroup1.SubGroup1RecID;
        //                    cntDepGrpSub.cntSubGroup2.SubGroup1Code = sub1.SG1_Code;
        //                    cntDepGrpSub.cntSubGroup3.SubGroup1Code = sub1.SG1_Code;
        //                    cntDepGrpSub.cntSubGroup4.SubGroup1Code = sub1.SG1_Code;

        //                    Group group = new Group();
        //                    GroupController groupController = new GroupController();
        //                    group = groupController.GetGroup(sub1.SG1_GrpRecID);

        //                    if (group != null && !string.IsNullOrEmpty(group.grp_Code))
        //                    {
        //                        FillInfo = 1;

        //                        cntDepGrpSub.cntDepartment.isFilled = FillInfo;
        //                        cntDepGrpSub.cntGroup.isFilled = FillInfo;
        //                        cntDepGrpSub.cntSubGroup1.isFilled = FillInfo;

        //                        cntDepGrpSub.cntGroup.Code = group.grp_Code;
        //                        cntDepGrpSub.cntSubGroup1.GroupCode = group.grp_Code;
        //                        cntDepGrpSub.cntSubGroup2.GroupCode = group.grp_Code;
        //                        cntDepGrpSub.cntSubGroup3.GroupCode = group.grp_Code;
        //                        cntDepGrpSub.cntSubGroup4.GroupCode = group.grp_Code;
        //                        cntDepGrpSub.cntSubGroup1.Code = sub1.SG1_Code;

        //                        FillInfo = 0;

        //                        cntDepGrpSub.cntDepartment.isFilled = FillInfo;
        //                        cntDepGrpSub.cntGroup.isFilled = FillInfo;
        //                        cntDepGrpSub.cntSubGroup1.isFilled = FillInfo;

        //                        DepartmentController depController = new DepartmentController();
        //                        Department department = depController.GetDepartment(group.grp_DeptRecID);
        //                        if (department != null && !string.IsNullOrEmpty(department.dep_Code))
        //                        {
        //                            FillInfo = 1;

        //                            cntDepGrpSub.cntGroup.isFilled = FillInfo;
        //                            cntDepGrpSub.cntDepartment.isFilled = FillInfo;
        //                            cntDepGrpSub.cntDepartment.Code = department.dep_Code;
        //                            cntDepGrpSub.cntSubGroup1.DepartmentCode = department.dep_Code;
        //                            cntDepGrpSub.cntSubGroup2.DepartmentCode = department.dep_Code;
        //                            cntDepGrpSub.cntSubGroup3.DepartmentCode = department.dep_Code;
        //                            cntDepGrpSub.cntSubGroup4.DepartmentCode = department.dep_Code;

        //                            FillInfo = 0;

        //                            cntDepGrpSub.cntGroup.isFilled = FillInfo;
        //                            cntDepGrpSub.cntDepartment.isFilled = FillInfo;
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        FillInfo = 0;
        //        cntDepGrpSub.cntDepartment.isFilled = FillInfo;
        //        cntDepGrpSub.cntGroup.isFilled = FillInfo;
        //        cntDepGrpSub.cntSubGroup1.isFilled = FillInfo;
        //        cntDepGrpSub.cntSubGroup2.isFilled = FillInfo;
        //        cntDepGrpSub.cntSubGroup3.isFilled = FillInfo;

        //        cntDepGrpSub.cntSubGroup3.SubGroup2Code = cntDepGrpSub.cntSubGroup2.Code;
        //        cntDepGrpSub.cntSubGroup3.SubGroup1Code = cntDepGrpSub.cntSubGroup1.Code;
        //        cntDepGrpSub.cntSubGroup3.GroupCode = cntDepGrpSub.cntGroup.Code;
        //        cntDepGrpSub.cntSubGroup3.DepartmentCode = cntDepGrpSub.cntDepartment.Code;

        //        cntDepGrpSub.cntSubGroup2.SubGroup1Code = cntDepGrpSub.cntSubGroup1.Code;
        //        cntDepGrpSub.cntSubGroup2.GroupCode = cntDepGrpSub.cntGroup.Code;
        //        cntDepGrpSub.cntSubGroup2.DepartmentCode = cntDepGrpSub.cntDepartment.Code;

        //        cntDepGrpSub.cntSubGroup1.GroupCode = cntDepGrpSub.cntGroup.Code;
        //        cntDepGrpSub.cntSubGroup1.DepartmentCode = cntDepGrpSub.cntDepartment.Code;

        //        cntDepGrpSub.cntGroup.DepartmentCode = cntDepGrpSub.cntDepartment.Code;
        //        cntDepGrpSub.cntDepartment_Validating(cntDepGrpSub.cntDepartment.Code, new CancelEventArgs());
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        //public void ClearSubGroup4Code()
        //{
        //    if (!string.IsNullOrEmpty(cntDepGrpSub.cntSubGroup4.Code))
        //    {
        //        cntDepGrpSub.cntSubGroup4.Sub3RecID = -1;
        //        cntDepGrpSub.cntSubGroup4.Sub4RecID = -1;
        //        cntDepGrpSub.cntSubGroup3.Sub2RecID = -1;
        //        cntDepGrpSub.cntSubGroup2.Sub1RecID = -1;
        //        cntDepGrpSub.cntSubGroup1.GroupRecID = -1;
        //        cntDepGrpSub.cntGroup.DepartmentRecID = -1;
        //        cntDepGrpSub.cntSubGroup2.SubGroup4Code = string.Empty;
        //        cntDepGrpSub.cntSubGroup4.Code = string.Empty;
        //    }
        //}

        //public void ClearSubGroup3Code()
        //{
        //    if (!string.IsNullOrEmpty(cntDepGrpSub.cntSubGroup3.Code))
        //    {
        //        cntDepGrpSub.cntSubGroup3.Sub2RecID = -1;
        //        cntDepGrpSub.cntSubGroup3.Sub3RecID = -1;
        //        cntDepGrpSub.cntSubGroup2.Sub1RecID = -1;
        //        cntDepGrpSub.cntSubGroup1.GroupRecID = -1;
        //        cntDepGrpSub.cntGroup.DepartmentRecID = -1;
        //        cntDepGrpSub.cntSubGroup3.Code = string.Empty;
        //        cntDepGrpSub.cntSubGroup4.SubGroup3Code = string.Empty;
        //        cntDepGrpSub.cntSubGroup2.SubGroup3Code = string.Empty;
        //    }
        //}

        //public void ClearSubGroup2Code()
        //{
        //    if (!string.IsNullOrEmpty(cntDepGrpSub.cntSubGroup2.Code))
        //    {
        //        cntDepGrpSub.cntSubGroup2.Sub1RecID = -1;
        //        cntDepGrpSub.cntSubGroup2.Sub2RecID = -1;
        //        cntDepGrpSub.cntGroup.DepartmentRecID = -1;
        //        cntDepGrpSub.cntSubGroup1.GroupRecID = -1;
        //        cntDepGrpSub.cntSubGroup2.Code = string.Empty;
        //        cntDepGrpSub.cntSubGroup3.SubGroup2Code = string.Empty;
        //        cntDepGrpSub.cntSubGroup4.SubGroup2Code = string.Empty;
        //    }
        //}

        //public void ClearSubGroup1Code()
        //{
        //    if (cntDepGrpSub.cntSubGroup1 != null)
        //    {
        //        cntDepGrpSub.cntSubGroup1.Code = string.Empty;
        //        cntDepGrpSub.cntSubGroup2.SubGroup1Code = string.Empty;
        //        cntDepGrpSub.cntSubGroup3.SubGroup1Code = string.Empty;
        //        cntDepGrpSub.cntSubGroup4.SubGroup1Code = string.Empty;
        //    }
        //}

        //public void ClearcntDepGrpSub.cntGroup()
        //{
        //    if (cntDepGrpSub.cntGroup != null)
        //    {
        //        cntDepGrpSub.cntGroup.GroupRecID = -1;
        //        cntDepGrpSub.cntGroup.DepartmentRecID = -1;
        //        cntDepGrpSub.cntSubGroup1.GroupRecID = -1;
        //        cntDepGrpSub.cntGroup.Code = string.Empty;
        //        cntDepGrpSub.cntDepartment.TextGroupCode = string.Empty;
        //        cntDepGrpSub.cntSubGroup1.GroupCode = string.Empty;
        //        cntDepGrpSub.cntSubGroup2.GroupCode = string.Empty;
        //        cntDepGrpSub.cntSubGroup3.GroupCode = string.Empty;
        //        cntDepGrpSub.cntSubGroup4.GroupCode = string.Empty;
        //    }
        //}

        //public void ClearCode()
        //{
        //    FillInfo = 1;

        //    cntDepGrpSub.cntDepartment.isFilled = FillInfo;
        //    cntDepGrpSub.cntGroup.isFilled = FillInfo;
        //    cntDepGrpSub.cntSubGroup1.isFilled = FillInfo;
        //    cntDepGrpSub.cntSubGroup2.isFilled = FillInfo;
        //    cntDepGrpSub.cntSubGroup3.isFilled = FillInfo;
        //    cntDepGrpSub.cntSubGroup4.isFilled = FillInfo;

        //    cntDepGrpSub.cntDepartment.DepartmentRecID = -1;

        //    cntDepGrpSub.cntGroup.DepartmentRecID = -1;
        //    cntDepGrpSub.cntGroup.GroupRecID = -1;

        //    cntDepGrpSub.cntSubGroup1.GroupRecID = -1;
        //    cntDepGrpSub.cntSubGroup1.SubGroup1RecID = -1;
        //    cntDepGrpSub.cntSubGroup1.DepartmentCode = string.Empty;
        //    cntDepGrpSub.cntSubGroup1.GroupCode = string.Empty;

        //    cntDepGrpSub.cntSubGroup2.GroupRecID = -1;
        //    cntDepGrpSub.cntSubGroup2.Sub1RecID = -1;
        //    cntDepGrpSub.cntSubGroup2.Sub2RecID = -1;
        //    cntDepGrpSub.cntSubGroup2.SubGroup1RecID = -1;
        //    cntDepGrpSub.cntSubGroup2.DepartmentCode = string.Empty;
        //    cntDepGrpSub.cntSubGroup2.GroupCode = string.Empty;
        //    cntDepGrpSub.cntSubGroup2.SubGroup1Code = string.Empty;
        //    cntDepGrpSub.cntSubGroup2.SubGroup3Code = string.Empty;

        //    cntDepGrpSub.cntSubGroup3.GroupRecID = -1;
        //    cntDepGrpSub.cntSubGroup3.SubGroup1RecID = -1;
        //    cntDepGrpSub.cntSubGroup3.Sub1RecID = -1;
        //    cntDepGrpSub.cntSubGroup3.Sub2RecID = -1;
        //    cntDepGrpSub.cntSubGroup3.Sub3RecID = -1;
        //    cntDepGrpSub.cntSubGroup3.DepartmentCode = string.Empty;
        //    cntDepGrpSub.cntSubGroup3.GroupCode = string.Empty;
        //    cntDepGrpSub.cntSubGroup3.SubGroup1Code = string.Empty;
        //    cntDepGrpSub.cntSubGroup3.SubGroup2Code = string.Empty;

        //    cntDepGrpSub.cntSubGroup4.GroupRecID = -1;
        //    cntDepGrpSub.cntSubGroup4.SubGroup1RecID = -1;
        //    cntDepGrpSub.cntSubGroup4.Sub1RecID = -1;
        //    cntDepGrpSub.cntSubGroup4.Sub2RecID = -1;
        //    cntDepGrpSub.cntSubGroup4.Sub3RecID = -1;
        //    cntDepGrpSub.cntSubGroup4.Sub4RecID = -1;

        //    cntDepGrpSub.cntSubGroup4.DepartmentCode = string.Empty;
        //    cntDepGrpSub.cntSubGroup4.GroupCode = string.Empty;
        //    cntDepGrpSub.cntSubGroup4.SubGroup1Code = string.Empty;
        //    cntDepGrpSub.cntSubGroup4.SubGroup2Code = string.Empty;
        //    cntDepGrpSub.cntSubGroup4.SubGroup3Code = string.Empty;

        //    cntDepGrpSub.cntDepartment.Code = string.Empty;
        //    cntDepGrpSub.cntGroup.Code = string.Empty;
        //    cntDepGrpSub.cntSubGroup1.Code = string.Empty;
        //    cntDepGrpSub.cntSubGroup2.Code = string.Empty;
        //    cntDepGrpSub.cntSubGroup3.Code = string.Empty;

        //    FillInfo = 0;

        //    cntDepGrpSub.cntDepartment.isFilled = FillInfo;
        //    cntDepGrpSub.cntGroup.isFilled = FillInfo;
        //    cntDepGrpSub.cntSubGroup1.isFilled = FillInfo;
        //    cntDepGrpSub.cntSubGroup2.isFilled = FillInfo;
        //    cntDepGrpSub.cntSubGroup3.isFilled = FillInfo;
        //    cntDepGrpSub.cntSubGroup4.isFilled = FillInfo;
        //}

        #endregion


        #region Reports

        private void btnStockCard_Click(object sender, EventArgs e)
        {
            try
            {
                if (StockID != 0)
                {

                    FrmStockCard frm = new FrmStockCard();
                    frm.PreloadItem(StockID.ToString(), cntSupplier.Code + "/" + cntCategory.Code + "/" + txtSerialNo.Text);
                    frm.Show(StepCode, "Stock Card");
                }
            }
            catch (Exception ex)
            {

                ClsCommon.ShowException(StepName, ex);
            }
        }

        private void btnDN_Click(object sender, EventArgs e)
        {
            try
            {
                if (StockID != 0)
                {
                    DebitNoteController controller = new DebitNoteController();
                    string query = controller.GetDNReportQuerySerialModifications(
                        StockID, ClsConfig.LocationCode);

                    if (controller.GetQueryRowCount(query) == 0)
                    {
                        ClsCommon.ShowNotification(this, "No Data Were Found.");
                        return;
                    }

                    var aliases = new Dictionary<string, string>
        {
            { "Debit Note No",   "Debit Note No" },
            { "Debit Note Date", "Debit Note Date" },
            {"Item reference", "Item Reference" },
            {"Description", "Description" },
            { "Supplier",        "Supplier" },
            { "Debit Note Qty",  "Debit Note Qty" },
            { "Curr",            "Currency" },
            { "Old Cost",        "Old Cost" },
            { "New Cost",        "New Cost" },
            { "Reduction",       "Reduction" },
            { "Rate",            "Rate $" },
        };

                    using (WinApplications.Forms.frmPreview preview = new WinApplications.Forms.frmPreview())
                    {
                        preview.ShowDialog(ClsConfig.SQLConnectionString, "Debit Note Report", query,
                            new SuperStoreReports.ReportClasses.ClsSerialModificationsReport(
                                query, "ItemDNDetails.rpt").Preview,
                            aliases);
                    }
                }
                
            }
            catch (Exception ex)
            {

                ClsCommon.ShowException(StepName, ex);
            }
        }

        private void btnPurchases_Click(object sender, EventArgs e)
        {
            try
            {
                if (StockID != 0)
                {
                    PurchaseInvoiceController controller = new PurchaseInvoiceController();
                    string query = controller.GetPIReportQuerySerialModifications(
                        StockID, ClsConfig.LocationCode);

                    if (controller.GetQueryRowCount(query) == 0)
                    {
                        ClsCommon.ShowNotification(this, "No Data Were Found.");
                        return;
                    }

                    var aliases = new Dictionary<string, string>
        {
            { "PI No",              "PI No" },
            { "PI Date",            "PI Date" },
            {"Item reference", "Item Reference" },
            {"Description", "Description" },
            { "Supplier",           "Supplier" },
            { "Sign",               "Sign" },
            { "Purchased Quantity", "Purchased Qty" },
            { "Curr",               "Currency" },
            { "Real Cost",          "Real Cost" },
            { "Unit Discount",      "Unit Discount" },
            { "Type",               "Type" },
            { "Rate",               "Rate $" },
        };

                    using (WinApplications.Forms.frmPreview preview = new WinApplications.Forms.frmPreview())
                    {
                        preview.ShowDialog(ClsConfig.SQLConnectionString, "Purchasing Report", query,
                            new SuperStoreReports.ReportClasses.ClsSerialModificationsReport(
                                query, "ItemPurchasesDetails.rpt").Preview,
                            aliases);
                    }
                }
   
            }
            catch (Exception ex)
            {

                ClsCommon.ShowException(StepName, ex);
            }
        }

        #endregion
    }
}
