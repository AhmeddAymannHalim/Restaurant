using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.Data.Models
{
    public class PrinterSetting
    {
        public int Id { get; set; }
        public string PrinterForAllSections { get; set; }
        public string PrinterForCashier { get; set; }
        public string PrinterForEachSection { get; set; }
    }

    public class PrinterForSection
    {
        public int Id { get; set; }
        public int SectionId { get; set; }
        public string Printer { get; set; }


        public MenuSection Section { get; set; }
    }
}
