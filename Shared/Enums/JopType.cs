using System.ComponentModel.DataAnnotations;


namespace Shared.Enums;

public enum JopType
{
    [Display(Name = "الكل")]
    الكل = 0,

    [Display(Name = "عامل معمل")]
    عامل_معمل = 1
}
