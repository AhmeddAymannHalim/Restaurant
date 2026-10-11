using AutoMapper;
using Restaurant.DAL.Models;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Helper;

public class AutoMapper : Profile
{
    public AutoMapper()
    {
        #region Definitions

        // Branch - الفروع
        CreateMap<Branche, BranchViewModel>();
        CreateMap<BranchViewModel, Branche>()
            .ForMember(dest => dest.Code, opt => opt.Ignore())
            .ForMember(dest => dest.Employees, opt => opt.Ignore())
            .ForMember(dest => dest.ApplicationUsers, opt => opt.Ignore());

        // Job - الوظائف (the key column is JobId)
        CreateMap<Job, JobViewModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.JobId));
        CreateMap<JobViewModel, Job>()
            .ForMember(dest => dest.JobId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Code, opt => opt.Ignore())
            .ForMember(dest => dest.Employees, opt => opt.Ignore());

        // Table - الطاولات
        CreateMap<Restaurant.DAL.Models.Table, TableViewModel>();
        CreateMap<TableViewModel, Restaurant.DAL.Models.Table>()
            .ForMember(dest => dest.IsPending, opt => opt.Ignore());

        // UnitPrice - الوحدات
        CreateMap<UnitPrice, UnitPriceViewModel>().ReverseMap();

        // SalaryType - انواع الرواتب
        CreateMap<SalaryType, SalaryTypeViewModel>();
        CreateMap<SalaryTypeViewModel, SalaryType>()
            .ForMember(dest => dest.Users, opt => opt.Ignore());

        // MenuSection - اقسام المنيو
        CreateMap<MenuSection, MenuSectionViewModel>()
            .ForMember(dest => dest.KitchenId, opt => opt.MapFrom(src => src.kitchenId))
            .ForMember(dest => dest.KitchenArName, opt => opt.MapFrom(src => src.kitchen.ArName))
            .ForMember(dest => dest.KitchenEnName, opt => opt.MapFrom(src => src.kitchen.EnName))
            .ForMember(dest => dest.Kitchens, opt => opt.Ignore());
        CreateMap<MenuSectionViewModel, MenuSection>()
            .ForMember(dest => dest.kitchenId, opt => opt.MapFrom(src => src.KitchenId))
            .ForMember(dest => dest.kitchen, opt => opt.Ignore())
            .ForMember(dest => dest.MenuItems, opt => opt.Ignore());

        // Product - الاصناف
        CreateMap<Product, ProductViewModel>();
        CreateMap<ProductViewModel, Product>()
            .ForMember(dest => dest.InvoiceItems, opt => opt.Ignore())
            .ForMember(dest => dest.StoreProducts, opt => opt.Ignore());

        // Store - المخازن
        CreateMap<Store, StoreViewModel>()
            .ForMember(dest => dest.StorekeeperName, opt => opt.MapFrom(src => src.Storekeeper.FirstName + " " + src.Storekeeper.LastName))
            .ForMember(dest => dest.Storekeepers, opt => opt.Ignore());
        CreateMap<StoreViewModel, Store>()
            .ForMember(dest => dest.Storekeeper, opt => opt.Ignore())
            .ForMember(dest => dest.Invoices, opt => opt.Ignore())
            .ForMember(dest => dest.StoreProducts, opt => opt.Ignore());

        // Supplier - الموردين
        CreateMap<Supplier, SupplierViewModel>();
        CreateMap<SupplierViewModel, Supplier>()
            .ForMember(dest => dest.LastBalance, opt => opt.Ignore())
            .ForMember(dest => dest.Invoices, opt => opt.Ignore())
            .ForMember(dest => dest.PaymentInvoices, opt => opt.Ignore());

        // Customer - العملاء
        CreateMap<Customer, CustomerViewModel>();
        CreateMap<CustomerViewModel, Customer>()
            .ForMember(dest => dest.PaymentInvoices, opt => opt.Ignore());

        // Kitchen - المطابخ
        CreateMap<kitchen, KitchenViewModel>();
        CreateMap<KitchenViewModel, kitchen>()
            .ForMember(dest => dest.MenuSections, opt => opt.Ignore());

        // Expense - المصروفات
        CreateMap<Expense, ExpenseViewModel>();
        CreateMap<ExpenseViewModel, Expense>()
            .ForMember(dest => dest.Expenses, opt => opt.Ignore());

        // Employee - الموظفين (the key column is EmployeeId)
        CreateMap<Employee, EmployeeViewModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.EmployeeId))
            .ForMember(dest => dest.ImageFile, opt => opt.Ignore())
            .ForMember(dest => dest.JobArName, opt => opt.MapFrom(src => src.Job.ArName))
            .ForMember(dest => dest.JobEnName, opt => opt.MapFrom(src => src.Job.EnName))
            .ForMember(dest => dest.BranchArName, opt => opt.MapFrom(src => src.Branche.ArName))
            .ForMember(dest => dest.BranchEnName, opt => opt.MapFrom(src => src.Branche.EnName))
            .ForMember(dest => dest.SalaryTypeArName, opt => opt.MapFrom(src => src.SalaryType.ArName))
            .ForMember(dest => dest.SalaryTypeEnName, opt => opt.MapFrom(src => src.SalaryType.EnName))
            .ForMember(dest => dest.Branches, opt => opt.Ignore())
            .ForMember(dest => dest.Jobs, opt => opt.Ignore())
            .ForMember(dest => dest.SalaryTypes, opt => opt.Ignore());
        CreateMap<EmployeeViewModel, Employee>()
            .ForMember(dest => dest.EmployeeId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Code, opt => opt.Ignore())
            .ForMember(dest => dest.UserType, opt => opt.Ignore())
            .ForMember(dest => dest.Image, opt => opt.Ignore())
            .ForMember(dest => dest.HasAccount, opt => opt.Ignore())
            .ForMember(dest => dest.Branche, opt => opt.Ignore())
            .ForMember(dest => dest.SalaryType, opt => opt.Ignore())
            .ForMember(dest => dest.Job, opt => opt.Ignore())
            .ForMember(dest => dest.Salaries, opt => opt.Ignore())
            .ForMember(dest => dest.Orders, opt => opt.Ignore());

        // Deposit - الايداعات
        CreateMap<Deposit, DepositViewModel>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FirstName + " " + src.User.LastName));
        CreateMap<DepositViewModel, Deposit>()
            .ForMember(dest => dest.ProcessCode, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedTime, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore());

        // ExitExpense - المصروفات الخارجة
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

        // Scrounge - السلف والحوافز والخصومات
        CreateMap<Scrounge, ScroungeViewModel>()
            .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.Employee.FirstName + " " + src.Employee.LastName))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FirstName + " " + src.User.LastName))
            .ForMember(dest => dest.Employees, opt => opt.Ignore());
        CreateMap<ScroungeViewModel, Scrounge>()
            .ForMember(dest => dest.Date, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Employee, opt => opt.Ignore());

        // Salary - المرتبات
        CreateMap<Salary, SalaryViewModel>()
            .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.Employee.FirstName + " " + src.Employee.LastName))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FirstName + " " + src.User.LastName))
            .ForMember(dest => dest.Employees, opt => opt.Ignore());
        CreateMap<SalaryViewModel, Salary>()
            .ForMember(dest => dest.SalaryCode, opt => opt.Ignore())
            .ForMember(dest => dest.SalaryDate, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Employee, opt => opt.Ignore());

        // MenuItem - عناصر المنيو (cost, benefit and the image are worked out by the repository)
        CreateMap<MenuItem, MenuItemViewModel>()
            .ForMember(dest => dest.MenuSectionArName, opt => opt.MapFrom(src => src.MenuSection.ArName))
            .ForMember(dest => dest.MenuSectionEnName, opt => opt.MapFrom(src => src.MenuSection.EnName))
            .ForMember(dest => dest.StoreArName, opt => opt.MapFrom(src => src.Store.ArName))
            .ForMember(dest => dest.StoreEnName, opt => opt.MapFrom(src => src.Store.EnName))
            .ForMember(dest => dest.ImageFile, opt => opt.Ignore())
            .ForMember(dest => dest.MenuSections, opt => opt.Ignore())
            .ForMember(dest => dest.Stores, opt => opt.Ignore());
        CreateMap<MenuItemViewModel, MenuItem>()
            .ForMember(dest => dest.TotalCost, opt => opt.Ignore())
            .ForMember(dest => dest.Benefit, opt => opt.Ignore())
            .ForMember(dest => dest.IsReady, opt => opt.Ignore())
            .ForMember(dest => dest.Store, opt => opt.Ignore())
            .ForMember(dest => dest.MenuSection, opt => opt.Ignore())
            .ForMember(dest => dest.OnlineOrderMenuItems, opt => opt.Ignore())
            .ForMember(dest => dest.OrderMenuItems, opt => opt.Ignore());

        CreateMap<MenuItemComponent, MenuItemComponentViewModel>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.StoreProduct.ProductName));
        CreateMap<MenuItemComponentViewModel, MenuItemComponent>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.MenuItemId, opt => opt.Ignore())
            .ForMember(dest => dest.Cost, opt => opt.Ignore())
            .ForMember(dest => dest.StoreProduct, opt => opt.Ignore())
            .ForMember(dest => dest.MenuItem, opt => opt.Ignore());

        // Website - the public home page (everything comes from the dashboard)
        CreateMap<IndexSettings, WebsiteViewModel>()
            .ForMember(dest => dest.MenuSections, opt => opt.Ignore())
            .ForMember(dest => dest.Chefs, opt => opt.Ignore())
            .ForMember(dest => dest.Gallery, opt => opt.Ignore());
        CreateMap<MenuSection, WebsiteMenuSectionViewModel>();
        CreateMap<MenuItem, WebsiteMenuItemViewModel>()
            .ForMember(dest => dest.Size, opt => opt.MapFrom(src => src.Size.HasValue ? src.Size.Value.ToString() : null));

        // Chef - فريق المطبخ
        CreateMap<ChefItem, ChefViewModel>()
            .ForMember(dest => dest.ImageFile, opt => opt.Ignore());
        CreateMap<ChefViewModel, ChefItem>()
            .ForMember(dest => dest.ImageUrl, opt => opt.Ignore());

        // Gallery - معرض الصور
        CreateMap<GalleryItem, GalleryViewModel>()
            .ForMember(dest => dest.ImageFile, opt => opt.Ignore());
        CreateMap<GalleryViewModel, GalleryItem>()
            .ForMember(dest => dest.PictureUrl, opt => opt.Ignore());

        #endregion
    }
}
