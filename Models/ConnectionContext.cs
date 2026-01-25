using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    public class ConnectionContext : DbContext
    {
        //public ConnectionContext() : base("ConnectionContext")
        //{
        //    this.Database.CommandTimeout = 1000;
        //}

        static ConnectionContext()
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }

        public DbSet<ClsRole> DbClsRole { get; set; }
        public DbSet<ClsUser> DbClsUser { get; set; }
        public DbSet<ClsLoginDetails> DbClsLoginDetails { get; set; }
        public DbSet<ClsBranch> DbClsBranch { get; set; }
        public DbSet<ClsCountry> DbClsCountry { get; set; }
        public DbSet<ClsState> DbClsState { get; set; }
        public DbSet<ClsCity> DbClsCity { get; set; }
        public DbSet<ClsUserGroup> DbClsUserGroup { get; set; }
        public DbSet<ClsCurrency> DbClsCurrency { get; set; }
        public DbSet<ClsBrand> DbClsBrand { get; set; }
        public DbSet<ClsWarranty> DbClsWarranty { get; set; }
        public DbSet<ClsCategory> DbClsCategory { get; set; }
        public DbSet<ClsSubCategory> DbClsSubCategory { get; set; }
        public DbSet<ClsSubSubCategory> DbClsSubSubCategory { get; set; }
        public DbSet<ClsReligion> DbClsReligion { get; set; }
        public DbSet<ClsVariation> DbClsVariation { get; set; }
        public DbSet<ClsVariationDetails> DbClsVariationDetails { get; set; }
        public DbSet<ClsTax> DbClsTax { get; set; }
        public DbSet<ClsUnit> DbClsUnit { get; set; }
        public DbSet<ClsPaymentType> DbClsPaymentType { get; set; }
        public DbSet<ClsItem> DbClsItem { get; set; }
        public DbSet<ClsItemDetails> DbClsItemDetails { get; set; }
        public DbSet<ClsPurchaseQuotation> DbClsPurchaseQuotation { get; set; }
        public DbSet<ClsPurchaseQuotationDetails> DbClsPurchaseQuotationDetails { get; set; }
        public DbSet<ClsPurchaseOrder> DbClsPurchaseOrder { get; set; }
        public DbSet<ClsPurchaseOrderDetails> DbClsPurchaseOrderDetails { get; set; }
        public DbSet<ClsPurchase> DbClsPurchase { get; set; }
        public DbSet<ClsPurchaseDetails> DbClsPurchaseDetails { get; set; }
        public DbSet<ClsExpense> DbClsExpense { get; set; }
        public DbSet<ClsPurchaseReturn> DbClsPurchaseReturn { get; set; }
        public DbSet<ClsPurchaseReturnDetails> DbClsPurchaseReturnDetails { get; set; }
        public DbSet<ClsStockAdjustment> DbClsStockAdjustment { get; set; }
        public DbSet<ClsStockAdjustmentDetails> DbClsStockAdjustmentDetails { get; set; }
        public DbSet<ClsStockTransfer> DbClsStockTransfer { get; set; }
        public DbSet<ClsStockTransferDetails> DbClsStockTransferDetails { get; set; }
        public DbSet<ClsItemBranchMap> DbClsItemBranchMap { get; set; }
        public DbSet<ClsItemDetailsVariationMap> DbClsItemDetailsVariationMap { get; set; }
        public DbSet<ClsSalesQuotation> DbClsSalesQuotation { get; set; }
        public DbSet<ClsSalesQuotationDetails> DbClsSalesQuotationDetails { get; set; }
        public DbSet<ClsSalesOrder> DbClsSalesOrder { get; set; }
        public DbSet<ClsSalesOrderDetails> DbClsSalesOrderDetails { get; set; }
        public DbSet<ClsSalesProforma> DbClsSalesProforma { get; set; }
        public DbSet<ClsSalesProformaDetails> DbClsSalesProformaDetails { get; set; }
        public DbSet<ClsDeliveryChallan> DbClsDeliveryChallan { get; set; }
        public DbSet<ClsDeliveryChallanDetails> DbClsDeliveryChallanDetails { get; set; }
        public DbSet<ClsSales> DbClsSales { get; set; }
        public DbSet<ClsSalesDetails> DbClsSalesDetails { get; set; }
        public DbSet<ClsOpeningStock> DbClsOpeningStock { get; set; }
        public DbSet<ClsSalesReturn> DbClsSalesReturn { get; set; }
        public DbSet<ClsSalesReturnDetails> DbClsSalesReturnDetails { get; set; }
        public DbSet<ClsMenu> DbClsMenu { get; set; }
        public DbSet<ClsMenuPermission> DbClsMenuPermission { get; set; }
        public DbSet<ClsUserBranchMap> DbClsUserBranchMap { get; set; }
        public DbSet<ClsExceptionLogger> DbClsExceptionLogger { get; set; }
        public DbSet<ClsPrefixMaster> DbClsPrefixMaster { get; set; }
        public DbSet<ClsPrefixUserMap> DbClsPrefixUserMap { get; set; }
        public DbSet<ClsForgotPassword> DbClsForgotPassword { get; set; }
        public DbSet<ClsBranchPaymentTypeMap> DbClsBranchPaymentTypeMap { get; set; }
        public DbSet<ClsBusinessSettings> DbClsBusinessSettings { get; set; }
        public DbSet<ClsItemSettings> DbClsItemSettings { get; set; }
        public DbSet<ClsSaleSettings> DbClsSaleSettings { get; set; }
        public DbSet<ClsPosSettings> DbClsPosSettings { get; set; }
        public DbSet<ClsPurchaseSettings> DbClsPurchaseSettings { get; set; }
        public DbSet<ClsRewardPointSettings> DbClsRewardPointSettings { get; set; }
        public DbSet<ClsRewardPointTier> DbClsRewardPointTier { get; set; }
        public DbSet<ClsShortCutKeySetting> DbClsShortCutKeySetting { get; set; }
        public DbSet<ClsOtp> DbClsOtp { get; set; }
        public DbSet<ClsAddress> DbClsAddress { get; set; }
        public DbSet<ClsSellingPriceGroup> DbClsSellingPriceGroup { get; set; }
        public DbSet<ClsItemSellingPriceGroupMap> DbClsItemSellingPriceGroupMap { get; set; }
        public DbSet<ClsPlan> DbClsPlan { get; set; }
        public DbSet<ClsTermLength> DbClsTermLength { get; set; }
        public DbSet<ClsPlanAddons> DbClsPlanAddons { get; set; }
        public DbSet<ClsTransaction> DbClsTransaction { get; set; }
        public DbSet<ClsTransactionDetails> DbClsTransactionDetails { get; set; }
        public DbSet<ClsFeedback> DbClsFeedback { get; set; }
        public DbSet<ClsSupportTicket> DbClsSupportTicket { get; set; }
        public DbSet<ClsAccountType> DbClsAccountType { get; set; }
        public DbSet<ClsAccountSubType> DbClsAccountSubType { get; set; }
        public DbSet<ClsAccount> DbClsAccount { get; set; }
        public DbSet<ClsAccountDetails> DbClsAccountDetails { get; set; }
        public DbSet<ClsCashRegister> DbClsCashRegister { get; set; }
        public DbSet<ClsTimeZone> DbClsTimeZone { get; set; }
        public DbSet<ClsSecondaryUnit> DbClsSecondaryUnit { get; set; }
        public DbSet<ClsTertiaryUnit> DbClsTertiaryUnit { get; set; }
        public DbSet<ClsQuaternaryUnit> DbClsQuaternaryUnit { get; set; }
        public DbSet<ClsNotificationModules> DbClsNotificationModules { get; set; }
        public DbSet<ClsNotificationModulesDetails> DbClsNotificationModulesDetails { get; set; }
        public DbSet<ClsNotificationModulesSettings> DbClsNotificationModulesSettings { get; set; }
        public DbSet<ClsSmsSettings> DbClsSmsSettings { get; set; }
        public DbSet<ClsEmailSettings> DbClsEmailSettings { get; set; }
        public DbSet<ClsWhatsappSettings> DbClsWhatsappSettings { get; set; }
        public DbSet<ClsWebsiteViewer> DbClsWebsiteViewer { get; set; }
        public DbSet<ClsDomain> DbClsDomain { get; set; }
        public DbSet<ClsUserCurrencyMap> DbClsUserCurrencyMap { get; set; }
        public DbSet<ClsCountryCurrencyMap> DbClsCountryCurrencyMap { get; set; }
        public DbSet<ClsCountryTimeZoneMap> DbClsCountryTimeZoneMap { get; set; }
        public DbSet<ClsOnlinePaymentSettings> DbClsOnlinePaymentSettings { get; set; }
        public DbSet<ClsCompetitor> DbClsCompetitor { get; set; }
        public DbSet<ClsIndustryType> DbClsIndustryType { get; set; }
        public DbSet<ClsBusinessType> DbClsBusinessType { get; set; }
        public DbSet<ClsSmsUsed> DbClsSmsUsed { get; set; }
        public DbSet<ClsEmailUsed> DbClsEmailUsed { get; set; }
        public DbSet<ClsUserLog> DbClsUserLog { get; set; }
        public DbSet<ClsItemLog> DbClsItemLog { get; set; }
        public DbSet<ClsPurchaseLog> DbClsPurchaseLog { get; set; }
        public DbSet<ClsSaleLog> DbClsSaleLog { get; set; }
        public DbSet<ClsPlaceLog> DbClsPlaceLog { get; set; }
        public DbSet<ClsAccountLog> DbClsAccountLog { get; set; }
        public DbSet<ClsBankLog> DbClsBankLog { get; set; }
        public DbSet<ClsSettingLog> DbClsSettingLog { get; set; }
        public DbSet<ClsOtherLog> DbClsOtherLog { get; set; }
        public DbSet<ClsSupportTicketDetails> DbClsSupportTicketDetails { get; set; }
        public DbSet<ClsTaxMap> DbClsTaxMap { get; set; }
        public DbSet<ClsEnquiry> DbClsEnquiry { get; set; }
        public DbSet<ClsSellingPriceBranchMap> DbClsSellingPriceBranchMap { get; set; }
        public DbSet<ClsCoupon> DbClsCoupon { get; set; }
        public DbSet<ClsCouponTermLength> DbClsCouponTermLength { get; set; }
        public DbSet<ClsCouponAddon> DbClsCouponAddon { get; set; }
        public DbSet<ClsUserCountryMap> DbClsUserCountryMap { get; set; }
        public DbSet<ClsResellerPayment> DbClsResellerPayment { get; set; }
        public DbSet<ClsResellerPaymentMethod> DbClsResellerPaymentMethod { get; set; }
        public DbSet<ClsStockAdjustmentDeductionId> DbClsStockAdjustmentDeductionId { get; set; }
        public DbSet<ClsSalesDeductionId> DbClsSalesDeductionId { get; set; }
        public DbSet<ClsStockTransferDeductionId> DbClsStockTransferDeductionId { get; set; }
        public DbSet<ClsSalesReturnDeductionId> DbClsSalesReturnDeductionId { get; set; }
        public DbSet<ClsLedgerDiscount> DbClsLedgerDiscount { get; set; }
        public DbSet<ClsSupplierPayment> DbClsSupplierPayment { get; set; }
        public DbSet<ClsCustomerPayment> DbClsCustomerPayment { get; set; }
        public DbSet<ClsBankPayment> DbClsBankPayment { get; set; }
        public DbSet<ClsInvoiceTemplate> DbClsInvoiceTemplates { get; set; }
        public DbSet<ClsInvoiceTemplatesMaster> DbClsInvoiceTemplatesMaster { get; set; }
        public DbSet<ClsPrintLabel> DbClsPrintLabel { get; set; }
        // Template Master Defaults
        public DbSet<ClsInvoiceTemplateColorCategoryMaster> DbClsInvoiceTemplateColorCategoryMasters { get; set; }
        public DbSet<ClsInvoiceTemplateColorMaster> DbClsInvoiceTemplateColorMasters { get; set; }
        public DbSet<ClsInvoiceTemplateLabelCategoryMaster> DbClsInvoiceTemplateLabelCategoryMasters { get; set; }
        public DbSet<ClsInvoiceTemplateLabelMaster> DbClsInvoiceTemplateLabelMasters { get; set; }
        //public DbSet<ClsCustomerPaymentDeductionId> DbClsCustomerPaymentDeductionId { get; set; }
        //public DbSet<ClsSupplierPaymentDeductionId> DbClsSupplierPaymentDeductionId { get; set; }
        public DbSet<ClsOtherSoftwareImport> DbClsOtherSoftwareImport { get; set; }
        public DbSet<ClsAccountSubTypeMaster> DbClsAccountSubTypeMaster { get; set; }
        public DbSet<ClsAccountMaster> DbClsAccountMaster { get; set; }
        public DbSet<ClsExpensePayment> DbClsExpensePayment { get; set; }
        public DbSet<ClsContra> DbClsContra { get; set; }
        public DbSet<ClsJournal> DbClsJournal { get; set; }
        public DbSet<ClsJournalPayment> DbClsJournalPayment { get; set; }
        public DbSet<ClsAccountSettings> DbClsAccountSettings { get; set; }
        public DbSet<ClsAccountOpeningBalance> DbClsAccountOpeningBalance { get; set; }
        public DbSet<ClsHeader> DbClsHeader { get; set; }
        public DbSet<ClsPaymentLink> DbClsPaymentLink { get; set; }
        public DbSet<ClsPaymentTerm> DbClsPaymentTerm { get; set; }
        public DbSet<ClsReminderModules> DbClsReminderModules { get; set; }
        public DbSet<ClsReminderModulesDetails> DbClsReminderModulesDetails { get; set; }
        public DbSet<ClsReminderModulesSettings> DbClsReminderModulesSettings { get; set; }
        public DbSet<ClsReminderExceptionContacts> DbClsReminderExceptionContacts { get; set; }
        public DbSet<ClsTaxMaster> DbClsTaxMaster { get; set; }
        public DbSet<ClsTaxMasterMap> DbClsTaxMasterMap { get; set; }
        public DbSet<ClsTaxType> DbClsTaxType { get; set; }
        public DbSet<ClsCountryTaxTypeMap> DbClsCountryTaxTypeMap { get; set; }
        public DbSet<ClsItemCode> DbClsItemCode { get; set; }
        public DbSet<ClsBusinessRegistrationName> DbClsBusinessRegistrationName { get; set; }
        public DbSet<ClsTaxExemption> DbClsTaxExemption { get; set; }
        public DbSet<ClsTaxSetting> DbClsTaxSetting { get; set; }
        public DbSet<ClsPrefix> DbClsPrefix { get; set; }
        public DbSet<ClsPwaSettings> DbClsPwaSettings { get; set; }
        public DbSet<ClsVehicle> DbClsVehicle { get; set; }
        public DbSet<ClsExpenseSettings> DbClsExpenseSettings { get; set; }
        public DbSet<ClsExpenseTaxJournal> DbClsExpenseTaxJournal { get; set; }
        public DbSet<ClsPurchaseTaxJournal> DbClsPurchaseTaxJournal { get; set; }
        public DbSet<ClsSalesTaxJournal> DbClsSalesTaxJournal { get; set; }
        public DbSet<ClsCustomerPaymentTaxJournal> DbClsCustomerPaymentTaxJournal { get; set; }
        public DbSet<ClsSupplierPaymentTaxJournal> DbClsSupplierPaymentTaxJournal { get; set; }
        public DbSet<ClsSalesReturnTaxJournal> DbClsSalesReturnTaxJournal { get; set; }
        public DbSet<ClsPurchaseReturnTaxJournal> DbClsPurchaseReturnTaxJournal { get; set; }
        public DbSet<ClsShippingBill> DbClsShippingBill { get; set; }
        public DbSet<ClsShippingBillDetails> DbClsShippingBillDetails { get; set; }
        public DbSet<ClsSalt> DbClsSalt { get; set; }
        public DbSet<ClsBillOfEntry> DbClsBillOfEntry { get; set; }
        public DbSet<ClsBillOfEntryDetails> DbClsBillOfEntryDetails { get; set; }
        public DbSet<ClsStockAdjustmentReason> DbClsStockAdjustmentReason { get; set; }
        public DbSet<ClsStockTransferReason> DbClsStockTransferReason { get; set; }
        public DbSet<ClsSalesCreditNoteReason> DbClsSalesCreditNoteReason { get; set; }
        public DbSet<ClsSalesDebitNoteReason> DbClsSalesDebitNoteReason { get; set; }
        public DbSet<ClsPurchaseDebitNoteReason> DbClsPurchaseDebitNoteReason { get; set; }
        public DbSet<ClsNotificationTemplates> DbClsNotificationTemplates { get; set; }
        public DbSet<ClsUnitMaster> DbClsUnitMaster { get; set; }
        public DbSet<ClsAdditionalCharge> DbClsAdditionalCharge { get; set; }        
        public DbSet<ClsSalesQuotationAdditionalCharges> DbClsSalesQuotationAdditionalCharges { get; set; }
        public DbSet<ClsSalesOrderAdditionalCharges> DbClsSalesOrderAdditionalCharges { get; set; }
        public DbSet<ClsSalesProformaAdditionalCharges> DbClsSalesProformaAdditionalCharges { get; set; }
        public DbSet<ClsDeliveryChallanAdditionalCharges> DbClsDeliveryChallanAdditionalCharges { get; set; }
        public DbSet<ClsSalesAdditionalCharges> DbClsSalesAdditionalCharges { get; set; }
        public DbSet<ClsSalesReturnAdditionalCharges> DbClsSalesReturnAdditionalCharges { get; set; }
        public DbSet<ClsPurchaseQuotationAdditionalCharges> DbClsPurchaseQuotationAdditionalCharges { get; set; }
        public DbSet<ClsPurchaseOrderAdditionalCharges> DbClsPurchaseOrderAdditionalCharges { get; set; }
        public DbSet<ClsPurchaseAdditionalCharges> DbClsPurchaseAdditionalCharges { get; set; }
        public DbSet<ClsPurchaseReturnAdditionalCharges> DbClsPurchaseReturnAdditionalCharges { get; set; }
        public DbSet<ClsPurchaseAdditionalTaxJournal> DbClsPurchaseAdditionalTaxJournal { get; set; }
        public DbSet<ClsPurchaseReturnAdditionalTaxJournal> DbClsPurchaseReturnAdditionalTaxJournal { get; set; }
        public DbSet<ClsSalesAdditionalTaxJournal> DbClsSalesAdditionalTaxJournal { get; set; }
        public DbSet<ClsSalesReturnAdditionalTaxJournal> DbClsSalesReturnAdditionalTaxJournal { get; set; }
        public DbSet<ClsItemCodeMaster> DbClsItemCodeMaster { get; set; }
        public DbSet<ClsRecurringSales> DbClsRecurringSales { get; set; }
        public DbSet<ClsRecurringSalesDetails> DbClsRecurringSalesDetails { get; set; }
        public DbSet<ClsRecurringSalesAdditionalCharges> DbClsRecurringSalesAdditionalCharges { get; set; }
        public DbSet<ClsCatalogue> DbClsCatalogue { get; set; }
        public DbSet<ClsCatalogueItem> DbClsCatalogueItem { get; set; }
        public DbSet<ClsCatalogueCategory> DbClsCatalogueCategory { get; set; }
        public DbSet<ClsCatalogueBrand> DbClsCatalogueBrand { get; set; }
        public DbSet<ClsPageSeoSettings> DbClsPageSeoSettings { get; set; }
        // Note: Reward points balance fields are now stored in tblUser (ClsUser model)
        // Only transaction history remains in separate table
        public DbSet<ClsRewardPointTransaction> DbClsRewardPointTransaction { get; set; }
        
        // KOT System Tables
        public DbSet<ClsRestaurantTable> DbClsRestaurantTable { get; set; }
        public DbSet<ClsTableType> DbClsTableType { get; set; }
        public DbSet<ClsStationType> DbClsStationType { get; set; }
        public DbSet<ClsRestaurantFloor> DbClsRestaurantFloor { get; set; }
        public DbSet<ClsTableBooking> DbClsTableBooking { get; set; }
        public DbSet<ClsTableBookingTable> DbClsTableBookingTable { get; set; }
        public DbSet<ClsRecurringBooking> DbClsRecurringBooking { get; set; }
        public DbSet<ClsRecurringBookingTable> DbClsRecurringBookingTable { get; set; }
        public DbSet<ClsRecurringBookingDay> DbClsRecurringBookingDay { get; set; }
        public DbSet<ClsKotMaster> DbClsKotMaster { get; set; }
        public DbSet<ClsKotDetails> DbClsKotDetails { get; set; }
        public DbSet<ClsKitchenStation> DbClsKitchenStation { get; set; }
        public DbSet<ClsKitchenStationCategoryMap> DbClsKitchenStationCategoryMap { get; set; }
        public DbSet<ClsRestaurantSettings> DbClsRestaurantSettings { get; set; }
        public DbSet<ClsRestaurantOperatingDay> DbClsRestaurantOperatingDay { get; set; }
        public DbSet<ClsRestaurantBookingTimeSlot> DbClsRestaurantBookingTimeSlot { get; set; }
        public DbSet<ClsRestaurantBookingDateOverride> DbClsRestaurantBookingDateOverride { get; set; }
        public DbSet<ClsRestaurantBookingDateOverrideSlot> DbClsRestaurantBookingDateOverrideSlot { get; set; }
        
        // Blog System Tables
        public DbSet<ClsBlog> DbClsBlog { get; set; }
        public DbSet<ClsBlogCategory> DbClsBlogCategory { get; set; }
        public DbSet<ClsBlogView> DbClsBlogView { get; set; }
    }
}