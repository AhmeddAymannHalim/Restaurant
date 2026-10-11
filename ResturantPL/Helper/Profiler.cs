using AutoMapper;
using Restaurant.DAL.Models;
using ResturantPL.ViewModels;

namespace ResturantPL.Helper
{
    public class Profiler:Profile
    {
        public Profiler()
        {
            
            CreateMap<IndexSettings,IndexSettingsViewModel>().ReverseMap();
        }
    }
}
