using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.DAL.Models
{
    
    public class IndexSettings
    {
        public int Id {  get; set; }
        public string ArName { get; set; }
        public string EnName { get; set; }
        public string Description { get; set; }
        public string Phone { get; set; }
        public string StartDay { get; set; }
        public string EndDay { get; set; }
        public string StartHour { get; set; }
        public string EndHour { get; set; }
        public string VedioUrl { get; set; }
        public string ImageUrl { get; set; }
        public string Email { get; set; }
        public string Locaion { get; set; }
    }
}
