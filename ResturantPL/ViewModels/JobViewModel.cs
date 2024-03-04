using Restaurant.Data.Models;

namespace ResturantPL.ViewModels
{
    public class JobViewModel
    {
        public int JobId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }


        public  List<Employee> Employees { get; set; } = new List<Employee>();
    }
}
