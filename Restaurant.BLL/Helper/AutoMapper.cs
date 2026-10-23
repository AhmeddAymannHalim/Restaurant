using Restaurant.DAL.Helpers;
using Shared.Enums;
using Shared.ViewModels.Commons;
using AutoMapper;
using Restaurant.DAL.Models;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Helper;

public class AutoMapper : Profile
{
    public AutoMapper()
    {
        #region Definitions

        CreateMap<CompanyBranch, BranchViewModel>()
            .ForMember(dest => dest.CompanyArName, opt => opt.MapFrom(src => src.Company.ArName))
            .ForMember(dest => dest.CompanyEnName, opt => opt.MapFrom(src => src.Company.EnName))
            .ForMember(dest => dest.CurrencyArName, opt => opt.MapFrom(src => src.Currency.ArName))
            .ForMember(dest => dest.CurrencyEnName, opt => opt.MapFrom(src => src.Currency.EnName))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.ArAddress))
            .ForMember(dest => dest.Phone1, opt => opt.MapFrom(src => src.PhoneNumber1))
            .ForMember(dest => dest.Phone2, opt => opt.MapFrom(src => src.PhoneNumber2))
            .ForMember(dest => dest.Mobile1, opt => opt.MapFrom(src => src.PhoneNumber3));
        CreateMap<BranchViewModel, CompanyBranch>()
            .ForMember(dest => dest.Code, opt => opt.Ignore())
            .ForMember(dest => dest.Company, opt => opt.Ignore())
            .ForMember(dest => dest.Currency, opt => opt.Ignore())
            .ForMember(dest => dest.ArAddress, opt => opt.MapFrom(src => src.Address))
            .ForMember(dest => dest.EnAddress, opt => opt.MapFrom(src => src.Address))
            .ForMember(dest => dest.PhoneNumber1, opt => opt.MapFrom(src => src.Phone1))
            .ForMember(dest => dest.PhoneNumber2, opt => opt.MapFrom(src => src.Phone2))
            .ForMember(dest => dest.PhoneNumber3, opt => opt.MapFrom(src => src.Mobile1));

        CreateMap<Job, JobViewModel>()
            .ForMember(dest => dest.JobTypeArName, opt => opt.MapFrom(src => src.JobType.ArName))
            .ForMember(dest => dest.JobTypeEnName, opt => opt.MapFrom(src => src.JobType.EnName));
        CreateMap<JobViewModel, Job>()
            .ForMember(dest => dest.Code, opt => opt.Ignore())
            .ForMember(dest => dest.JobType, opt => opt.Ignore())
            .ForMember(dest => dest.Employees, opt => opt.Ignore());

        CreateMap<Restaurant.DAL.Models.Table, TableViewModel>();

        CreateMap<TableReservation, TableReservationViewModel>()
            .ForMember(dest => dest.TableArName, opt => opt.MapFrom(src => src.Table == null ? null : src.Table.ArName))
            .ForMember(dest => dest.TableEnName, opt => opt.MapFrom(src => src.Table == null ? null : src.Table.EnName))
            .ForMember(dest => dest.Tables, opt => opt.Ignore());
        CreateMap<TableReservationViewModel, TableReservation>()
            .ForMember(dest => dest.Table, opt => opt.Ignore())
            .ForMember(dest => dest.Customer, opt => opt.Ignore())
            .ForMember(dest => dest.CustomerId, opt => opt.Ignore());
        CreateMap<TableViewModel, Restaurant.DAL.Models.Table>()
            .ForMember(dest => dest.IsPending, opt => opt.Ignore());

        CreateMap<UnitPrice, UnitPriceViewModel>().ReverseMap();
        CreateMap<ProductCategory, ProductCategoryViewModel>().ReverseMap();

        CreateMap<MenuSection, MenuSectionViewModel>()
            .ForMember(dest => dest.KitchenId, opt => opt.MapFrom(src => src.KitchenId))
            .ForMember(dest => dest.KitchenArName, opt => opt.MapFrom(src => src.Kitchen.ArName))
            .ForMember(dest => dest.KitchenEnName, opt => opt.MapFrom(src => src.Kitchen.EnName))
            .ForMember(dest => dest.Kitchens, opt => opt.Ignore());
        CreateMap<MenuSectionViewModel, MenuSection>()
            .ForMember(dest => dest.KitchenId, opt => opt.MapFrom(src => src.KitchenId))
            .ForMember(dest => dest.Kitchen, opt => opt.Ignore())
            .ForMember(dest => dest.MenuItems, opt => opt.Ignore());

        CreateMap<ProductStock, ProductStockViewModel>()
            .ForMember(dest => dest.StoreArName, opt => opt.MapFrom(src => src.Store.ArName))
            .ForMember(dest => dest.StoreEnName, opt => opt.MapFrom(src => src.Store.EnName));

        CreateMap<ProductUnit, ProductUnitViewModel>()
            .ForMember(dest => dest.UnitArName, opt => opt.MapFrom(src => src.UnitPrice.ArName))
            .ForMember(dest => dest.UnitEnName, opt => opt.MapFrom(src => src.UnitPrice.EnName));
        CreateMap<ProductUnitPrice, ProductUnitPriceViewModel>().ReverseMap();
        CreateMap<ProductUnitViewModel, ProductUnit>()
            .ForMember(dest => dest.UnitPrice, opt => opt.Ignore());

        CreateMap<Product, ProductViewModel>()
            .ForMember(dest => dest.UnitList, opt => opt.Ignore())
            .ForMember(dest => dest.SellTypes, opt => opt.Ignore())
            .ForMember(dest => dest.Stocks, opt => opt.MapFrom(src => src.Stocks))
            .ForMember(dest => dest.TotalStock, opt => opt.MapFrom(src => src.Stocks.Sum(a => a.Quantity)))
            .ForMember(dest => dest.ProductCategoryId, opt => opt.MapFrom(src => src.ProductCategoryId ?? 0))
            .ForMember(dest => dest.ProductCategoryArName, opt => opt.MapFrom(src => src.ProductCategory.ArName))
            .ForMember(dest => dest.ProductCategoryEnName, opt => opt.MapFrom(src => src.ProductCategory.EnName))
            .ForMember(dest => dest.Categories, opt => opt.Ignore());
        CreateMap<ProductViewModel, Product>()
            .ForMember(dest => dest.ProductCategory, opt => opt.Ignore())
            .ForMember(dest => dest.MenuItemComponents, opt => opt.Ignore())
            .ForMember(dest => dest.Stocks, opt => opt.Ignore());

        CreateMap<Store, StoreViewModel>()
            .ForMember(dest => dest.BranchArName, opt => opt.MapFrom(src => src.Branch.ArName))
            .ForMember(dest => dest.BranchEnName, opt => opt.MapFrom(src => src.Branch.EnName))
            .ForMember(dest => dest.GovernorateId, opt => opt.MapFrom(src => src.GovId ?? src.Town.GovernorateId))
            .ForMember(dest => dest.CountryId, opt => opt.MapFrom(src => src.CountryId ?? src.Town.Governorate.CountryId))
            .ForMember(dest => dest.Stores, opt => opt.Ignore())
            .ForMember(dest => dest.Branches, opt => opt.Ignore())
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber1))
            .ForMember(dest => dest.StorekeeperId, opt => opt.MapFrom(src => src.EmployeeId))
            .ForMember(dest => dest.StorekeeperName,
                opt => opt.MapFrom(src => LanguageHelper.IsArabic() ? src.Employee.ArName : src.Employee.EnName))
            .ForMember(dest => dest.Storekeepers, opt => opt.Ignore());
        CreateMap<StoreViewModel, Store>()
            .ForMember(dest => dest.GovId, opt => opt.MapFrom(src => src.GovernorateId))
            .ForMember(dest => dest.PhoneNumber1, opt => opt.MapFrom(src => src.PhoneNumber))
            .ForMember(dest => dest.EmployeeId, opt => opt.MapFrom(src => src.StorekeeperId))
            .ForMember(dest => dest.Employee, opt => opt.Ignore())
            .ForMember(dest => dest.Parent, opt => opt.Ignore())
            .ForMember(dest => dest.Branch, opt => opt.Ignore())
            .ForMember(dest => dest.Country, opt => opt.Ignore())
            .ForMember(dest => dest.Gov, opt => opt.Ignore())
            .ForMember(dest => dest.Town, opt => opt.Ignore())
            .ForMember(dest => dest.StoreItems, opt => opt.Ignore())
            .ForMember(dest => dest.ItemLocations, opt => opt.Ignore())
            .ForMember(dest => dest.BillDetails, opt => opt.Ignore());

        CreateMap<Microsoft.AspNetCore.Identity.IdentityRole, RoleViewModel>();
        CreateMap<RoleViewModel, Microsoft.AspNetCore.Identity.IdentityRole>();

        CreateMap<ApplicationUser, UserViewModel>()
            .ForMember(dest => dest.Password, opt => opt.Ignore())
            .ForMember(dest => dest.ConfirmPassword, opt => opt.Ignore())
            .ForMember(dest => dest.Roles, opt => opt.Ignore());
        CreateMap<UserViewModel, ApplicationUser>();

        CreateMap<ApplicationUser, EmployeeAccountViewModel>()
            .ForMember(dest => dest.Password, opt => opt.Ignore())
            .ForMember(dest => dest.ConfirmPassword, opt => opt.Ignore())
            .ForMember(dest => dest.Employees, opt => opt.Ignore());
        CreateMap<EmployeeAccountViewModel, ApplicationUser>();
        CreateMap(typeof(ResultMessage<>), typeof(ResultMessage<>));
        CreateMap<ApplicationUser, AccountToken>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Token, opt => opt.Ignore());
        CreateMap<ApplicationUser, ProfileViewModel>()
            .ForMember(dest => dest.TownArName, opt => opt.MapFrom(src => src.Town.ArName))
            .ForMember(dest => dest.TownEnName, opt => opt.MapFrom(src => src.Town.EnName))
            .ForMember(dest => dest.GovernorateId, opt => opt.MapFrom(src => src.Town == null ? (int?)null : src.Town.GovernorateId))
            .ForMember(dest => dest.CountryId, opt => opt.MapFrom(src => src.Town == null || src.Town.Governorate == null ? (int?)null : src.Town.Governorate.CountryId));

        CreateMap<Account, AccountDefinitionViewModel>()
            .ForMember(dest => dest.ParentArName, opt => opt.MapFrom(src => src.ParentAccount.ArName))
            .ForMember(dest => dest.ParentEnName, opt => opt.MapFrom(src => src.ParentAccount.EnName))
            .ForMember(dest => dest.Balance, opt => opt.MapFrom(src => src.BalanceValue))
            .ForMember(dest => dest.IsOwned, opt => opt.MapFrom(src => src.Kind == AccountKind.Owned))
            .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.AccountDetails.Location))
            .ForMember(dest => dest.BuyValue, opt => opt.MapFrom(src => src.AccountDetails.BuyValue))
            .ForMember(dest => dest.BuyDate, opt => opt.MapFrom(src => src.AccountDetails.BuyDate))
            .ForMember(dest => dest.StartUsingDate, opt => opt.MapFrom(src => src.AccountDetails.StartUsingDate))
            .ForMember(dest => dest.ProductAge, opt => opt.MapFrom(src => src.AccountDetails.ProductAge))
            .ForMember(dest => dest.DepreciationType, opt => opt.MapFrom(src => src.AccountDetails == null ? DepreciationType.FixedInstallment : src.AccountDetails.DepreciationType))
            .ForMember(dest => dest.ScrapValue, opt => opt.MapFrom(src => src.AccountDetails.ScrapValue))
            .ForMember(dest => dest.DepreciationAccountId, opt => opt.MapFrom(src => src.AccountDetails.DepreciationAccountId))
            .ForMember(dest => dest.DepreciationExpensesAccountId, opt => opt.MapFrom(src => src.AccountDetails.DepreciationExpensesAccountId));
        CreateMap<AccountDefinitionViewModel, Account>()
            .ForMember(dest => dest.Kind, opt => opt.Ignore());
        CreateMap<AccountDefinitionViewModel, AccountDetails>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());
        CreateMap<PaymentInvoiceCustomer, PaymentRefundRequestViewModel>()
            .ForMember(dest => dest.OriginalPaymentId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.OriginalPaymentCode, opt => opt.MapFrom(src => src.ProcessCode))
            .ForMember(dest => dest.OriginalPaymentAmount, opt => opt.MapFrom(src => src.Paid));
        CreateMap<PaymentInvoiceSupplier, PaymentRefundRequestViewModel>()
            .ForMember(dest => dest.OriginalPaymentId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.OriginalPaymentCode, opt => opt.MapFrom(src => src.ProcessCode))
            .ForMember(dest => dest.OriginalPaymentAmount, opt => opt.MapFrom(src => src.Paid));
        CreateMap<PaymentRefundRequestViewModel, PaymentRefund>();
        CreateMap<Employee, EmployeeAccountViewModel>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.EmployeeId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.ArName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.EnName));
        CreateMap<Employee, LookupItem>();
        CreateMap<ProductStockViewModel, ProductStock>()
            .ForMember(dest => dest.Store, opt => opt.Ignore());
        CreateMap<ApplicationSetting, ApplicationSettingViewModel>()
            .ForMember(dest => dest.Password, opt => opt.Ignore());
        CreateMap<ApplicationSettingViewModel, ApplicationSetting>();
        CreateMap<PrinterSetting, PrinterSettingViewModel>()
            .ForMember(dest => dest.Printers, opt => opt.Ignore());
        CreateMap<PrinterSettingViewModel, PrinterSetting>();

        CreateMap<RegisterViewModel, ApplicationUser>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.UserType, opt => opt.MapFrom(src => "User"));

        CreateMap<PaymentInvoiceSupplier, PaymentViewModel>()
            .ForMember(dest => dest.PartnerId, opt => opt.MapFrom(src => src.SupplierId))
            .ForMember(dest => dest.PartnerArName, opt => opt.MapFrom(src => src.Supplier.Account.ArName))
            .ForMember(dest => dest.PartnerEnName, opt => opt.MapFrom(src => src.Supplier.Account.EnName))
            .ForMember(dest => dest.BillMasterId, opt => opt.MapFrom(src => src.BillMasterId ?? 0))
            .ForMember(dest => dest.InvoiceCode, opt => opt.MapFrom(src => src.BillMaster != null ? src.BillMaster.Code : null))
            .ForMember(dest => dest.Partners, opt => opt.Ignore())
            .ForMember(dest => dest.OpenInvoices, opt => opt.Ignore());
        CreateMap<PaymentViewModel, PaymentInvoiceSupplier>()
            .ForMember(dest => dest.SupplierId, opt => opt.MapFrom(src => src.PartnerId))
            .ForMember(dest => dest.Supplier, opt => opt.Ignore())
            .ForMember(dest => dest.BillMaster, opt => opt.Ignore());

        CreateMap<PaymentInvoiceCustomer, PaymentViewModel>()
            .ForMember(dest => dest.PartnerId, opt => opt.MapFrom(src => src.CustomerId))
            .ForMember(dest => dest.PartnerArName, opt => opt.MapFrom(src => src.Customer.Account.ArName))
            .ForMember(dest => dest.PartnerEnName, opt => opt.MapFrom(src => src.Customer.Account.EnName))
            .ForMember(dest => dest.BillMasterId, opt => opt.MapFrom(src => src.BillMasterId ?? 0))
            .ForMember(dest => dest.InvoiceCode, opt => opt.MapFrom(src => src.BillMaster != null ? src.BillMaster.Code : null))
            .ForMember(dest => dest.Partners, opt => opt.Ignore())
            .ForMember(dest => dest.OpenInvoices, opt => opt.Ignore());
        CreateMap<PaymentViewModel, PaymentInvoiceCustomer>()
            .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.PartnerId))
            .ForMember(dest => dest.Customer, opt => opt.Ignore())
            .ForMember(dest => dest.BillMaster, opt => opt.Ignore());

        CreateMap<PaymentRefund, PaymentRefundViewModel>()
            .ForMember(dest => dest.OriginalPaymentId, opt => opt.MapFrom(src => src.OriginalPaymentId ?? src.SupplierPaymentId ?? 0))
            .ForMember(dest => dest.OriginalPaymentCode, opt => opt.MapFrom(src => src.OriginalPayment != null
                ? src.OriginalPayment.ProcessCode
                : src.SupplierPayment.ProcessCode))
            .ForMember(dest => dest.RequestedByUserName, opt => opt.MapFrom(src => src.RequestedByUser.FullName))
            .ForMember(dest => dest.ApprovedByUserName, opt => opt.MapFrom(src => src.ApprovedByUser.FullName));

        CreateMap<StoreProductMovement, StockMovementViewModel>()
            .ForMember(dest => dest.StoreArName, opt => opt.MapFrom(src => src.Store.ArName))
            .ForMember(dest => dest.StoreEnName, opt => opt.MapFrom(src => src.Store.EnName))
            .ForMember(dest => dest.ProductArName, opt => opt.MapFrom(src => src.Product.ArName))
            .ForMember(dest => dest.ProductEnName, opt => opt.MapFrom(src => src.Product.EnName));

        CreateMap<StoreConvert, StoreConvertViewModel>()
            .ForMember(dest => dest.FromStoreArName, opt => opt.MapFrom(src => src.FromStore.ArName))
            .ForMember(dest => dest.FromStoreEnName, opt => opt.MapFrom(src => src.FromStore.EnName))
            .ForMember(dest => dest.ToStoreArName, opt => opt.MapFrom(src => src.ToStore.ArName))
            .ForMember(dest => dest.ToStoreEnName, opt => opt.MapFrom(src => src.ToStore.EnName))
            .ForMember(dest => dest.ProductsCount, opt => opt.MapFrom(src => src.StoreConvertProducts.Count))
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.StoreConvertProducts))
            .ForMember(dest => dest.Stores, opt => opt.Ignore())
            .ForMember(dest => dest.Categories, opt => opt.Ignore());
        CreateMap<StoreConvertProduct, StoreConvertItemViewModel>()
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
            .ForMember(dest => dest.ProductArName, opt => opt.MapFrom(src => src.Product.ArName))
            .ForMember(dest => dest.ProductEnName, opt => opt.MapFrom(src => src.Product.EnName));
        CreateMap<StoreConvertViewModel, StoreConvert>()
            .ForMember(dest => dest.StoreConvertProducts, opt => opt.MapFrom(src => src.Items))
            .ForMember(dest => dest.FromStore, opt => opt.Ignore())
            .ForMember(dest => dest.ToStore, opt => opt.Ignore());
        CreateMap<StoreConvertItemViewModel, StoreConvertProduct>()
            .ForMember(dest => dest.Product, opt => opt.Ignore())
            .ForMember(dest => dest.StoreConvert, opt => opt.Ignore());

        CreateMap<Order, OrderViewModel>()
            .ForMember(dest => dest.SellerName, opt => opt.MapFrom(src => src.Seller.FullName))
            .ForMember(dest => dest.ReceiverName, opt => opt.MapFrom(src => src.Receiver.FullName))
            .ForMember(dest => dest.TableArName, opt => opt.MapFrom(src => src.Table.ArName))
            .ForMember(dest => dest.TableEnName, opt => opt.MapFrom(src => src.Table.EnName))
            .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => (LanguageHelper.IsArabic() ? src.Employee.ArName : src.Employee.EnName)))
            .ForMember(dest => dest.ItemsCount, opt => opt.MapFrom(src => src.Lines.Count))
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Lines))
            .ForMember(dest => dest.Customers, opt => opt.Ignore())
            .ForMember(dest => dest.Tables, opt => opt.Ignore())
            .ForMember(dest => dest.DeliveryDrivers, opt => opt.Ignore())
            .ForMember(dest => dest.DeliveryZones, opt => opt.Ignore())
            .ForMember(dest => dest.DeliveryZoneArName, opt => opt.MapFrom(src => src.DeliveryZone.ArName))
            .ForMember(dest => dest.DeliveryZoneEnName, opt => opt.MapFrom(src => src.DeliveryZone.EnName));
        CreateMap<OrderMenuItem, OrderItemViewModel>()
            .ForMember(dest => dest.MenuItemArName, opt => opt.MapFrom(src => src.MenuItem.ArName))
            .ForMember(dest => dest.MenuItemEnName, opt => opt.MapFrom(src => src.MenuItem.EnName));
        CreateMap<OrderViewModel, Order>()
            .ForMember(dest => dest.Bill, opt => opt.MapFrom(src => new BillMaster { BillDiscountValue = src.Discount }))
            .ForMember(dest => dest.Lines, opt => opt.MapFrom(src => src.Items));
        CreateMap<OrderItemViewModel, OrderMenuItem>()
            .ForMember(dest => dest.BillDetail, opt => opt.MapFrom(src => new BillDetail { Quantity = src.Quantity }));
        CreateMap<ApplicationSetting, OrderViewModel>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.TaxPercent, opt => opt.MapFrom(src => src.OrderTax))
            .ForMember(dest => dest.DefaultService, opt => opt.MapFrom(src => src.OrderService))
            .ForMember(dest => dest.Service, opt => opt.MapFrom(src => src.OrderService));
        CreateMap<MenuItem, CashierMenuItemViewModel>()
            .ForMember(dest => dest.Available, opt => opt.MapFrom(src => src.AvailablePortions))
            .ForMember(dest => dest.ArIngredients, opt => opt.MapFrom(src => src.Components == null ? null : string.Join(" - ", src.Components.OrderBy(c => c.Id).Where(c => c.Product != null).Select(c => c.Product.ArName))))
            .ForMember(dest => dest.EnIngredients, opt => opt.MapFrom(src => src.Components == null ? null : string.Join(" - ", src.Components.OrderBy(c => c.Id).Where(c => c.Product != null).Select(c => c.Product.EnName))))
            .ForMember(dest => dest.SectionId, opt => opt.MapFrom(src => src.MenuSectionId))
            .ForMember(dest => dest.SectionArName, opt => opt.MapFrom(src => src.MenuSection.ArName))
            .ForMember(dest => dest.SectionEnName, opt => opt.MapFrom(src => src.MenuSection.EnName));
        CreateMap<Table, LookupItem>();

        CreateMap<Supplier, SupplierViewModel>()
            .ForMember(dest => dest.GovernorateId, opt => opt.MapFrom(src => src.GovId ?? src.Town.GovernorateId))
            .ForMember(dest => dest.CountryId, opt => opt.MapFrom(src => src.CountryId ?? src.Town.Governorate.CountryId))
            .ForMember(dest => dest.Branches, opt => opt.Ignore())
            .ForMember(dest => dest.Employees, opt => opt.Ignore())
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Account.Code))
            .ForMember(dest => dest.ArName, opt => opt.MapFrom(src => src.Account.ArName))
            .ForMember(dest => dest.EnName, opt => opt.MapFrom(src => src.Account.EnName))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.DetailedAddress))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber1))
            .ForMember(dest => dest.Balance, opt => opt.MapFrom(src => src.Account.BalanceValue))
            .ForMember(dest => dest.Debit, opt => opt.MapFrom(src => src.Account.DebitBalance > src.Account.CreditBalance ? src.Account.DebitBalance - src.Account.CreditBalance : 0))
            .ForMember(dest => dest.Credit, opt => opt.MapFrom(src => src.Account.CreditBalance > src.Account.DebitBalance ? src.Account.CreditBalance - src.Account.DebitBalance : 0));
        CreateMap<SupplierViewModel, Supplier>()
            .ForMember(dest => dest.GovId, opt => opt.MapFrom(src => src.GovernorateId))
            .ForMember(dest => dest.DetailedAddress, opt => opt.MapFrom(src => src.Address))
            .ForMember(dest => dest.PhoneNumber1, opt => opt.MapFrom(src => src.PhoneNumber))
            .ForMember(dest => dest.AccountId, opt => opt.Ignore())
            .ForMember(dest => dest.Account, opt => opt.MapFrom(src => new Account
            {
                ArName = src.ArName,
                EnName = src.EnName,
                CreditBalance = src.Credit,
                DebitBalance = src.Debit
            }))
            .ForMember(dest => dest.LastBalance, opt => opt.Ignore())
            .ForMember(dest => dest.Branch, opt => opt.Ignore())
            .ForMember(dest => dest.CostCenter, opt => opt.Ignore())
            .ForMember(dest => dest.Employee, opt => opt.Ignore())
            .ForMember(dest => dest.Country, opt => opt.Ignore())
            .ForMember(dest => dest.Gov, opt => opt.Ignore())
            .ForMember(dest => dest.Town, opt => opt.Ignore())
            .ForMember(dest => dest.PaymentInvoices, opt => opt.Ignore());

        CreateMap<Customer, CustomerViewModel>()
            .ForMember(dest => dest.ImageFile, opt => opt.Ignore())
            .ForMember(dest => dest.Nationalities, opt => opt.Ignore())
            .ForMember(dest => dest.Branches, opt => opt.Ignore())
            .ForMember(dest => dest.Employees, opt => opt.Ignore())
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Account.Code))
            .ForMember(dest => dest.ArName, opt => opt.MapFrom(src => src.Account.ArName))
            .ForMember(dest => dest.EnName, opt => opt.MapFrom(src => src.Account.EnName))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.DetailedAddress))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber1))
            .ForMember(dest => dest.Balance, opt => opt.MapFrom(src => src.Account.BalanceValue))
            .ForMember(dest => dest.Debit, opt => opt.MapFrom(src => src.Account.DebitBalance > src.Account.CreditBalance ? src.Account.DebitBalance - src.Account.CreditBalance : 0))
            .ForMember(dest => dest.Credit, opt => opt.MapFrom(src => src.Account.CreditBalance > src.Account.DebitBalance ? src.Account.CreditBalance - src.Account.DebitBalance : 0))
            .ForMember(dest => dest.TownArName, opt => opt.MapFrom(src => src.Town.ArName))
            .ForMember(dest => dest.TownEnName, opt => opt.MapFrom(src => src.Town.EnName))
            .ForMember(dest => dest.GovernorateId, opt => opt.MapFrom(src => src.Town.GovernorateId))
            .ForMember(dest => dest.CountryId, opt => opt.MapFrom(src => src.Town.Governorate.CountryId));
        CreateMap<CustomerViewModel, Customer>()
            .ForMember(dest => dest.DetailedAddress, opt => opt.MapFrom(src => src.Address))
            .ForMember(dest => dest.PhoneNumber1, opt => opt.MapFrom(src => src.PhoneNumber))
            .ForMember(dest => dest.GovId, opt => opt.MapFrom(src => src.GovernorateId))
            .ForMember(dest => dest.AccountId, opt => opt.Ignore())
            .ForMember(dest => dest.Account, opt => opt.MapFrom(src => new Account
            {
                ArName = src.ArName,
                EnName = src.EnName,
                DebitBalance = src.Debit,
                CreditBalance = src.Credit
            }))
            .ForMember(dest => dest.CompanyBranch, opt => opt.Ignore())
            .ForMember(dest => dest.SellType, opt => opt.Ignore())
            .ForMember(dest => dest.CostCenter, opt => opt.Ignore())
            .ForMember(dest => dest.Employee, opt => opt.Ignore())
            .ForMember(dest => dest.Nationality, opt => opt.Ignore())
            .ForMember(dest => dest.Country, opt => opt.Ignore())
            .ForMember(dest => dest.Gov, opt => opt.Ignore())
            .ForMember(dest => dest.Town, opt => opt.Ignore())
            .ForMember(dest => dest.Classification, opt => opt.Ignore())
            .ForMember(dest => dest.Specialization, opt => opt.Ignore())
            .ForMember(dest => dest.CustomerTier, opt => opt.Ignore())
            .ForMember(dest => dest.PaymentInvoices, opt => opt.Ignore());

        CreateMap<DeliveryZone, DeliveryZoneViewModel>().ReverseMap();

        CreateMap<SellType, SellTypeViewModel>();
        CreateMap<SellTypeViewModel, SellType>();

        CreateMap<CostCenter, CostCenterViewModel>()
            .ForMember(dest => dest.ParentArName, opt => opt.MapFrom(src => src.Parent.ArName))
            .ForMember(dest => dest.ParentEnName, opt => opt.MapFrom(src => src.Parent.EnName))
            .ForMember(dest => dest.Parents, opt => opt.Ignore());
        CreateMap<CostCenterViewModel, CostCenter>()
            .ForMember(dest => dest.Code, opt => opt.Ignore())
            .ForMember(dest => dest.Parent, opt => opt.Ignore());

        CreateMap<Classification, ClassificationViewModel>();
        CreateMap<ClassificationViewModel, Classification>()
            .ForMember(dest => dest.Customers, opt => opt.Ignore());

        CreateMap<CustomerTier, CustomerTierViewModel>();
        CreateMap<CustomerTierViewModel, CustomerTier>();

        CreateMap<Specialization, SpecializationViewModel>()
            .ForMember(dest => dest.ParentArName, opt => opt.MapFrom(src => src.Parent.ArName))
            .ForMember(dest => dest.ParentEnName, opt => opt.MapFrom(src => src.Parent.EnName))
            .ForMember(dest => dest.Parents, opt => opt.Ignore());
        CreateMap<SpecializationViewModel, Specialization>()
            .ForMember(dest => dest.Code, opt => opt.Ignore())
            .ForMember(dest => dest.Parent, opt => opt.Ignore())
            .ForMember(dest => dest.Employees, opt => opt.Ignore())
            .ForMember(dest => dest.Technicians, opt => opt.Ignore());

        CreateMap<ContractType, ContractTypeViewModel>();
        CreateMap<ContractTypeViewModel, ContractType>();

        CreateMap<Department, DepartmentViewModel>();
        CreateMap<DepartmentViewModel, Department>()
            .ForMember(dest => dest.Employees, opt => opt.Ignore());

        CreateMap<JobType, JobTypeViewModel>();
        CreateMap<JobTypeViewModel, JobType>();

        CreateMap<InsuranceCategory, InsuranceCategoryViewModel>();
        CreateMap<InsuranceCategoryViewModel, InsuranceCategory>();

        CreateMap<InsuranceCompany, InsuranceCompanyViewModel>();
        CreateMap<InsuranceCompanyViewModel, InsuranceCompany>();
        CreateMap<AllowanceType, AllowanceTypeViewModel>();
        CreateMap<AllowanceTypeViewModel, AllowanceType>();
        CreateMap<DeductionType, DeductionTypeViewModel>();
        CreateMap<DeductionTypeViewModel, DeductionType>();

        CreateMap<LeaveType, LeaveTypeViewModel>();
        CreateMap<LeaveTypeViewModel, LeaveType>();

        CreateMap<Company, CompanyViewModel>()
            .ForMember(dest => dest.CurrencyArName, opt => opt.MapFrom(src => src.Currency.ArName))
            .ForMember(dest => dest.CurrencyEnName, opt => opt.MapFrom(src => src.Currency.EnName))
            .ForMember(dest => dest.Currencies, opt => opt.Ignore());
        CreateMap<CompanyViewModel, Company>()
            .ForMember(dest => dest.Currency, opt => opt.Ignore());

        CreateMap<Currency, CurrencyViewModel>();
        CreateMap<CurrencyViewModel, Currency>()
            .ForMember(dest => dest.Code, opt => opt.Ignore())
            .ForMember(dest => dest.Companies, opt => opt.Ignore())
            .ForMember(dest => dest.CompanyBranches, opt => opt.Ignore())
            .ForMember(dest => dest.Accounts, opt => opt.Ignore());

        CreateMap<Kitchen, KitchenViewModel>();
        CreateMap<KitchenViewModel, Kitchen>()
            .ForMember(dest => dest.MenuSections, opt => opt.Ignore());

        CreateMap<Expense, ExpenseViewModel>();
        CreateMap<ExpenseViewModel, Expense>()
            .ForMember(dest => dest.Expenses, opt => opt.Ignore());

        CreateMap<EmployeePhone, PhoneViewModel>();
        CreateMap<CustomerPhone, PhoneViewModel>();
        CreateMap<PhoneViewModel, CustomerPhone>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Customer, opt => opt.Ignore())
            .ForMember(dest => dest.CustomerId, opt => opt.Ignore());
        CreateMap<SupplierPhone, PhoneViewModel>();
        CreateMap<PhoneViewModel, SupplierPhone>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Supplier, opt => opt.Ignore())
            .ForMember(dest => dest.SupplierId, opt => opt.Ignore());
        CreateMap<StorePhone, PhoneViewModel>();
        CreateMap<PhoneViewModel, StorePhone>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Store, opt => opt.Ignore())
            .ForMember(dest => dest.StoreId, opt => opt.Ignore());

        CreateMap<EmployeeDependent, EmployeeDependentViewModel>();
        CreateMap<EmployeeDependentViewModel, EmployeeDependent>()
            .ForMember(dest => dest.EmployeeId, opt => opt.Ignore())
            .ForMember(dest => dest.Employee, opt => opt.Ignore());

        CreateMap<EmployeeAllowance, EmployeeAllowanceViewModel>();
        CreateMap<EmployeeAllowanceViewModel, EmployeeAllowance>()
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount ?? 0))
            .ForMember(dest => dest.AllowanceType, opt => opt.Ignore())
            .ForMember(dest => dest.EmployeeId, opt => opt.Ignore())
            .ForMember(dest => dest.Employee, opt => opt.Ignore());

        CreateMap<EmployeeFixedDeduction, EmployeeFixedDeductionViewModel>();
        CreateMap<EmployeeFixedDeductionViewModel, EmployeeFixedDeduction>()
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount ?? 0))
            .ForMember(dest => dest.DeductionType, opt => opt.Ignore())
            .ForMember(dest => dest.EmployeeId, opt => opt.Ignore())
            .ForMember(dest => dest.Employee, opt => opt.Ignore());

        CreateMap<EmployeeLeave, EmployeeLeaveViewModel>();
        CreateMap<EmployeeLeaveViewModel, EmployeeLeave>()
            .ForMember(dest => dest.FromDate, opt => opt.MapFrom(src => src.FromDate ?? default))
            .ForMember(dest => dest.ToDate, opt => opt.MapFrom(src => src.ToDate ?? default))
            .ForMember(dest => dest.Days, opt => opt.MapFrom(src => src.Days ?? 0))
            .ForMember(dest => dest.EmployeeId, opt => opt.Ignore())
            .ForMember(dest => dest.Employee, opt => opt.Ignore())
            .ForMember(dest => dest.LeaveType, opt => opt.Ignore());

        CreateMap<EmployeeDocument, EmployeeDocumentViewModel>()
            .ForMember(dest => dest.File, opt => opt.Ignore());
        CreateMap<EmployeeDocumentViewModel, EmployeeDocument>()
            .ForMember(dest => dest.EmployeeId, opt => opt.Ignore())
            .ForMember(dest => dest.Employee, opt => opt.Ignore());

        CreateMap<Employee, EmployeeViewModel>()
            .ForMember(dest => dest.Phones, opt => opt.MapFrom(src => src.Phones.Where(a => a.PhoneNumber != src.PhoneNumber).OrderBy(a => a.Id)))
            .ForMember(dest => dest.WeeklyOffDaysSelected, opt => opt.MapFrom(src => Enum.GetValues<WeeklyOffDays>()
                .Where(a => a != WeeklyOffDays.None && src.WeeklyOffDaysOverride.HasFlag(a))
                .ToList()))
            .ForMember(dest => dest.Nationalities, opt => opt.Ignore())
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
            .ForMember(dest => dest.ImageFile, opt => opt.Ignore())
            .ForMember(dest => dest.JobArName, opt => opt.MapFrom(src => src.Job.ArName))
            .ForMember(dest => dest.JobEnName, opt => opt.MapFrom(src => src.Job.EnName))
            .ForMember(dest => dest.BranchId, opt => opt.MapFrom(src => src.BranchId ?? 0))
            .ForMember(dest => dest.JobId, opt => opt.MapFrom(src => src.JobId ?? 0))
            .ForMember(dest => dest.TownId, opt => opt.MapFrom(src => src.TownId ?? 0))
            .ForMember(dest => dest.BranchArName, opt => opt.MapFrom(src => src.Branch.ArName))
            .ForMember(dest => dest.BranchEnName, opt => opt.MapFrom(src => src.Branch.EnName))
            .ForMember(dest => dest.TownArName, opt => opt.MapFrom(src => src.Town.ArName))
            .ForMember(dest => dest.TownEnName, opt => opt.MapFrom(src => src.Town.EnName))
            .ForMember(dest => dest.GovernorateId, opt => opt.MapFrom(src => src.Town.GovernorateId))
            .ForMember(dest => dest.CountryId, opt => opt.MapFrom(src => src.Town.Governorate.CountryId))
            .ForMember(dest => dest.Branches, opt => opt.Ignore())
            .ForMember(dest => dest.Jobs, opt => opt.Ignore());
        CreateMap<EmployeeViewModel, Employee>()
            .ForMember(dest => dest.Phones, opt => opt.MapFrom(src => src.Phones
                .Where(a => !string.IsNullOrWhiteSpace(a.PhoneNumber))
                .Select(a => new EmployeePhone { PhoneNumber = a.PhoneNumber.Trim() })))
            .ForMember(dest => dest.WeeklyOffDaysOverride, opt => opt.MapFrom(src => src.WeeklyOffDaysSelected
                .Aggregate(WeeklyOffDays.None, (current, day) => current | day)))
            .ForMember(dest => dest.Code, opt => opt.Ignore())
            .ForMember(dest => dest.Image, opt => opt.Ignore())
            .ForMember(dest => dest.GovId, opt => opt.MapFrom(src => src.GovernorateId))
            .ForMember(dest => dest.HasLoginAccount, opt => opt.Ignore())
            .ForMember(dest => dest.Branch, opt => opt.Ignore())
            .ForMember(dest => dest.Job, opt => opt.Ignore())
            .ForMember(dest => dest.Town, opt => opt.Ignore())
            .ForMember(dest => dest.Salaries, opt => opt.Ignore())
            .ForMember(dest => dest.Scrounges, opt => opt.Ignore());

        CreateMap<Deposit, DepositViewModel>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FirstName + " " + src.User.LastName));
        CreateMap<DepositViewModel, Deposit>()
            .ForMember(dest => dest.ProcessCode, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedTime, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore());

        CreateMap<ExitExpense, ExitExpenseViewModel>()
            .ForMember(dest => dest.ExpenseArName, opt => opt.MapFrom(src => src.Expense.ArName))
            .ForMember(dest => dest.ExpenseEnName, opt => opt.MapFrom(src => src.Expense.EnName))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FirstName + " " + src.User.LastName))
            .ForMember(dest => dest.Expenses, opt => opt.Ignore());
        CreateMap<ExitExpenseViewModel, ExitExpense>()
            .ForMember(dest => dest.Date, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Expense, opt => opt.Ignore());

        CreateMap<Scrounge, ScroungeViewModel>()
            .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => (LanguageHelper.IsArabic() ? src.Employee.ArName : src.Employee.EnName)))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FirstName + " " + src.User.LastName))
            .ForMember(dest => dest.Employees, opt => opt.Ignore());
        CreateMap<ScroungeViewModel, Scrounge>()
            .ForMember(dest => dest.Date, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Employee, opt => opt.Ignore());

        CreateMap<Salary, SalaryViewModel>()
            .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => (LanguageHelper.IsArabic() ? src.Employee.ArName : src.Employee.EnName)))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FirstName + " " + src.User.LastName))
            .ForMember(dest => dest.Employees, opt => opt.Ignore());
        CreateMap<SalaryViewModel, Salary>()
            .ForMember(dest => dest.SalaryCode, opt => opt.Ignore())
            .ForMember(dest => dest.SalaryDate, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Employee, opt => opt.Ignore());

        CreateMap<MenuItem, MenuItemViewModel>()
            .ForMember(dest => dest.ArIngredients, opt => opt.MapFrom(src => src.Components == null ? null : string.Join(" - ", src.Components.OrderBy(c => c.Id).Where(c => c.Product != null).Select(c => c.Product.ArName))))
            .ForMember(dest => dest.EnIngredients, opt => opt.MapFrom(src => src.Components == null ? null : string.Join(" - ", src.Components.OrderBy(c => c.Id).Where(c => c.Product != null).Select(c => c.Product.EnName))))
            .ForMember(dest => dest.MenuSectionArName, opt => opt.MapFrom(src => src.MenuSection.ArName))
            .ForMember(dest => dest.MenuSectionEnName, opt => opt.MapFrom(src => src.MenuSection.EnName))
            .ForMember(dest => dest.StoreArName, opt => opt.MapFrom(src => src.Store.ArName))
            .ForMember(dest => dest.StoreEnName, opt => opt.MapFrom(src => src.Store.EnName))
            .ForMember(dest => dest.ImageFile, opt => opt.Ignore())
            .ForMember(dest => dest.MenuSections, opt => opt.Ignore())
            .ForMember(dest => dest.Stores, opt => opt.Ignore())
            .ForMember(dest => dest.Products, opt => opt.Ignore())
            .ForMember(dest => dest.Categories, opt => opt.Ignore());
        CreateMap<MenuItemViewModel, MenuItem>()
            .ForMember(dest => dest.Store, opt => opt.Ignore())
            .ForMember(dest => dest.MenuSection, opt => opt.Ignore());

        CreateMap<MenuItemComponent, MenuItemComponentViewModel>()
            .ForMember(dest => dest.ProductArName, opt => opt.MapFrom(src => src.Product.ArName))
            .ForMember(dest => dest.ProductEnName, opt => opt.MapFrom(src => src.Product.EnName))
            .ForMember(dest => dest.UnitArName, opt => opt.MapFrom(src => src.UnitPrice.ArName))
            .ForMember(dest => dest.UnitEnName, opt => opt.MapFrom(src => src.UnitPrice.EnName))
            .ForMember(dest => dest.UnitCost, opt => opt.MapFrom(src => src.Product.Cost * src.TransRate))
            .ForMember(dest => dest.Cost, opt => opt.MapFrom(src => src.Quantity * src.TransRate * src.Product.Cost));
        CreateMap<MenuItemComponentViewModel, MenuItemComponent>()
            .ForMember(dest => dest.UnitPrice, opt => opt.Ignore())
            .ForMember(dest => dest.TransRate, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.MenuItemId, opt => opt.Ignore())
            .ForMember(dest => dest.Product, opt => opt.Ignore())
            .ForMember(dest => dest.MenuItem, opt => opt.Ignore());

        #endregion
    }
}
