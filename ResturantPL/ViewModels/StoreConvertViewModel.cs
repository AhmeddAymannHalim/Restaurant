using System.ComponentModel.DataAnnotations;

namespace ResturantPL.ViewModels
{
    public class StoreConvertViewModel
    {


        [Display(Name = "كود الاذن")]
        public string ConvertCode { get; set; }

        [Required(ErrorMessage = "The Store is required.")]
        [Display(Name = "المخزن المنقول منه")]
        public int FromStoreId { get; set; }


        [Required(ErrorMessage = "The Store is required.")]
        [Display(Name = "المخزن المنقول اليه")]
        public int ToStoreId { get; set; }


        public List<StoreConvertProductViewModel> StoreConvertProducts { get; set; } = new List<StoreConvertProductViewModel>();

    }

    public class StoreConvertProductViewModel
    {
        [Required(ErrorMessage = "Please select a product.")]
        [Display(Name = "اسم الصنف")]
        public int StoreProductId { get; set; }
        

        [Required(ErrorMessage = "The quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "The quantity must be at least 1.")]
        [Display(Name = "الكمية المنقولة")]
        public decimal Quantity { get; set; }


       


    }

}
