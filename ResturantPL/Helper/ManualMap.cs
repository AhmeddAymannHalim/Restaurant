using Restaurant.Data.Models;
using ResturantPL.ViewModels;

namespace ResturantPL.Helper
{
    public class ManualMap
    {
        public static void MenuItemMap(ref MenuItem menuItem ,ref MenuItemViewModel menuItemViewModel)
        {

            menuItem.Name = menuItemViewModel.Name;
            menuItem.Description = menuItemViewModel.Description;
           
          

        }
    }
}
