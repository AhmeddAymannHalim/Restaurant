using AutoMapper;
using Restaurant.Data.Models;
using ResturantPL.ViewModels;

namespace ResturantPL.Helper
{
    public class Profiler:Profile
    {
        public Profiler()
        {
            
            CreateMap<IndexSettings,IndexSettingsViewModel>().ReverseMap();
            CreateMap<MenuItem,MenuItemViewModel>().ReverseMap();
            CreateMap<ChefItem,ChefItemViewModel>().ReverseMap();
            CreateMap<GalleryItem, GalleryItemViewModel>().ReverseMap();
            CreateMap<ApplicationUser,EmployeeViewModel>().ReverseMap();
            CreateMap<Branche,BranchViewModel>().ReverseMap();
            CreateMap<SalaryType,SalaryTypeViewModel>().ReverseMap();
        }
    }
}
