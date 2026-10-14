namespace Restaurant.DAL.Models
{
    public class Employee
    {
        public int EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get { return FirstName + " " + LastName; } }
        public string PhoneNumber { get; set; }
        public int CityId { get; set; }
        public string Street { get; set; }
        public string Image { get; set; }
        public byte Age { get; set; }
        public string UserType { get; set; }
        public string Code { get; set; }
        public bool HasAccount { get; set; }

        public string NationalID { get; set; }
        public int SalaryTypeId { get; set; }
        public decimal Salary { get; set; }
        public int BrancheId { get; set; }
        public int JobId { get; set; }

        public virtual City City { get; set; }
        public virtual Branche Branche { get; set; }
        public virtual SalaryType SalaryType { get; set; }
        public virtual Job Job { get; set; }
        public virtual ICollection<Salary> Salaries { get; set; }
        public virtual ICollection<Scrounge> Scrounges { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
}
