using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.Data.Models
{
    public class Table
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NumOfPersons { get; set; }
        public bool IsPending { get; set; }
    }
}
