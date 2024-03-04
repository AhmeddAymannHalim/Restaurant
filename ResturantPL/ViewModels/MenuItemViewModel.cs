
namespace ResturantPL.ViewModels
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.AspNetCore.Http;

    public class MenuItemViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "The name is required.")]
        [StringLength(100, ErrorMessage = "The name must be at most 100 characters long.")]
        [Display(Name = "اسم المنتج")]
        public string Name { get; set; }

        [Display(Name = "القسم")]
        public int MenuSectionId { get; set; }

        [StringLength(500, ErrorMessage = "The description must be at most 500 characters long.")]
        [Display(Name = "وصف المنتج")]
        public string Description { get; set; }

        [Display(Name = "صورة المنتج")]
        public IFormFile Image { get; set; }

        [Display(Name = "الصورة")]
        public string ImageUrl { get; set; }


        [Required(ErrorMessage = "The Store Id is required.")]
        [Display(Name = "المخزن")]
        public int StoreId { get; set; }


        [Required(ErrorMessage = "The price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "The price must be greater than 0.")]
        [Display(Name = "التكلفة الكلية")]
        [DataType(DataType.Currency)]
        public decimal TotalCost { get; set; }


        [Required(ErrorMessage = "The price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "The price must be greater than 0.")]
        [Display(Name = "سعر البيع")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }


        [Required(ErrorMessage = "The price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "The price must be greater than 0.")]
        [Display(Name = "الربح")]
        [DataType(DataType.Currency)]
        public decimal Benefit { get; set; }


        public List<MenuItemComponentViewModel> MenuItemComponents { get; set; }

    }

    public class MenuItemComponentViewModel
    {
        [Required(ErrorMessage = "Please select a store product.")]
        [Display(Name = "اسم الصنف")]
        public int StoreProductId { get; set; }


        [Display(Name = "الكمية المطلوبة")]
        public decimal Quantity { get; set; }


        [Required(ErrorMessage = "The Cost is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "The Cost must be greater than 0.")]
        [Display(Name = "تكلفة المكون")]
        [DataType(DataType.Currency)]
        public decimal Cost { get; set; }



    }



}
