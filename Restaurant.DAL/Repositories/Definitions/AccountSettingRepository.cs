using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.Helpers;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.Repositories.Definitions;

public class AccountSettingRepository : IAccountSettingRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IBillSupportRepository _billSupport;

    public AccountSettingRepository(ApplicationDbContext context)
    {
        _context = context;
        _billSupport = new BillSupportRepository(context);
    }

    public async Task<AccountSettingViewModel> GetAccountSettingAsync()
    {
        var model = new AccountSettingViewModel();

        var setting = await _context.AccountSettings.AsNoTracking().OrderByDescending(a => a.Id).FirstOrDefaultAsync();
        if (setting is not null)
        {
            model.Id = setting.Id;
            model.ParentCustomerId = setting.ParentCustomerId;
            model.ParentSupplierId = setting.ParentSupplierId;
            model.ParentEmployeeId = setting.ParentEmployeeId;
            model.ParentExpensesId = setting.ParentExpensesId;
            model.ParentBankId = setting.ParentBankId;
            model.ParentBoxId = setting.ParentBoxId;
            model.ParentFixedAssetsId = setting.ParentFixedAssetsId;
        }

        var sales = await _billSupport.GetBillSettingAsync(BillType.Sales, "مبيعات", "Sales");
        model.SalesItemAccountId = sales.AccountItemId;
        model.SalesPayAccountId = sales.AccountPayId;

        var salesReturn = await _billSupport.GetBillSettingAsync(BillType.SalesReturn, "مرتجع مبيعات", "Sales Return");
        model.SalesReturnItemAccountId = salesReturn.AccountItemId;
        model.SalesReturnPayAccountId = salesReturn.AccountPayId;

        var purchases = await _billSupport.GetBillSettingAsync(BillType.Purchases, "مشتريات", "Purchases");
        model.PurchasesItemAccountId = purchases.AccountItemId;
        model.PurchasesPayAccountId = purchases.AccountPayId;

        var purchaseReturn = await _billSupport.GetBillSettingAsync(BillType.PurchaseReturn, "مرتجع مشتريات", "Purchases Return");
        model.PurchaseReturnItemAccountId = purchaseReturn.AccountItemId;
        model.PurchaseReturnPayAccountId = purchaseReturn.AccountPayId;

        var customerPayment = await _billSupport.GetEntrySettingAsync(EntryType.CustomerPayment, "دفعات عملاء", "Customer payment");
        model.CustomerPaymentAccountId = customerPayment.AccountId;

        var supplierPayment = await _billSupport.GetEntrySettingAsync(EntryType.SupplierPayment, "دفعات موردين", "Supplier payment");
        model.SupplierPaymentAccountId = supplierPayment.AccountId;

        var customerRefund = await _billSupport.GetEntrySettingAsync(EntryType.CashPayment, "سند صرف نقدي", "Cash Pay Entry");
        model.CustomerRefundAccountId = customerRefund.AccountId;

        var supplierRefund = await _billSupport.GetEntrySettingAsync(EntryType.CashReceipt, "سند قبض نقدي", "Cash Receipt Entry");
        model.SupplierRefundAccountId = supplierRefund.AccountId;

        var customerOpening = await _billSupport.GetEntrySettingAsync(EntryType.CustomerOpeningBalance, "رصيد افتتاحى للعملاء", "Opening Balance of Customers");
        model.CustomerOpeningBalanceAccountId = customerOpening.AccountId;

        var supplierOpening = await _billSupport.GetEntrySettingAsync(EntryType.SupplierOpeningBalance, "رصيد افتتاحى للموردين", "Opening Balance for Suppliers");
        model.SupplierOpeningBalanceAccountId = supplierOpening.AccountId;

        model.Accounts = await _context.Accounts.AsNoTracking()
            .OrderBy(a => a.Code)
            .Select(a => new LookupItem { Id = a.Id, ArName = a.Code + " - " + a.ArName, EnName = a.Code + " - " + a.EnName })
            .ToListAsync();

        return model;
    }

    public async Task<ResultMessage> UpdateAccountSettingAsync(AccountSettingViewModel model)
    {
        try
        {
            var setting = await _context.AccountSettings.OrderByDescending(a => a.Id).FirstOrDefaultAsync();
            if (setting is null)
            {
                setting = new AccountSetting { CreatedAt = DateTime.Now };
                await _context.AccountSettings.AddAsync(setting);
            }

            setting.ParentCustomerId = model.ParentCustomerId;
            setting.ParentSupplierId = model.ParentSupplierId;
            setting.ParentEmployeeId = model.ParentEmployeeId;
            setting.ParentExpensesId = model.ParentExpensesId;
            setting.ParentBankId = model.ParentBankId;
            setting.ParentBoxId = model.ParentBoxId;
            setting.ParentFixedAssetsId = model.ParentFixedAssetsId;
            setting.UpdatedAt = DateTime.Now;

            var sales = await _billSupport.GetBillSettingAsync(BillType.Sales, "مبيعات", "Sales");
            sales.AccountItemId = model.SalesItemAccountId;
            sales.AccountPayId = model.SalesPayAccountId;

            var salesReturn = await _billSupport.GetBillSettingAsync(BillType.SalesReturn, "مرتجع مبيعات", "Sales Return");
            salesReturn.AccountItemId = model.SalesReturnItemAccountId;
            salesReturn.AccountPayId = model.SalesReturnPayAccountId;

            var purchases = await _billSupport.GetBillSettingAsync(BillType.Purchases, "مشتريات", "Purchases");
            purchases.AccountItemId = model.PurchasesItemAccountId;
            purchases.AccountPayId = model.PurchasesPayAccountId;

            var purchaseReturn = await _billSupport.GetBillSettingAsync(BillType.PurchaseReturn, "مرتجع مشتريات", "Purchases Return");
            purchaseReturn.AccountItemId = model.PurchaseReturnItemAccountId;
            purchaseReturn.AccountPayId = model.PurchaseReturnPayAccountId;

            var customerPayment = await _billSupport.GetEntrySettingAsync(EntryType.CustomerPayment, "دفعات عملاء", "Customer payment");
            customerPayment.AccountId = model.CustomerPaymentAccountId;

            var supplierPayment = await _billSupport.GetEntrySettingAsync(EntryType.SupplierPayment, "دفعات موردين", "Supplier payment");
            supplierPayment.AccountId = model.SupplierPaymentAccountId;

            var customerRefund = await _billSupport.GetEntrySettingAsync(EntryType.CashPayment, "سند صرف نقدي", "Cash Pay Entry");
            customerRefund.AccountId = model.CustomerRefundAccountId;

            var supplierRefund = await _billSupport.GetEntrySettingAsync(EntryType.CashReceipt, "سند قبض نقدي", "Cash Receipt Entry");
            supplierRefund.AccountId = model.SupplierRefundAccountId;

            var customerOpening = await _billSupport.GetEntrySettingAsync(EntryType.CustomerOpeningBalance, "رصيد افتتاحى للعملاء", "Opening Balance of Customers");
            customerOpening.AccountId = model.CustomerOpeningBalanceAccountId;

            var supplierOpening = await _billSupport.GetEntrySettingAsync(EntryType.SupplierOpeningBalance, "رصيد افتتاحى للموردين", "Opening Balance for Suppliers");
            supplierOpening.AccountId = model.SupplierOpeningBalanceAccountId;

            await _context.SaveChangesAsync();

            return new ResultMessage { Success = true };
        }
        catch (Exception ex)
        {
            return new ResultMessage { Success = false, Message = ErrorLog.Message(ex) };
        }
    }
}
