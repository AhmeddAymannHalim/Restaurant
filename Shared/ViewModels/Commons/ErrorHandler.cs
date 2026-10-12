using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Shared.ViewModels.Commons;

public static class ErrorHandler
{
    public static ViewResult ErrorView(string errorMessage)
    {
        var errorViewModel = new ErrorViewModel
        {
            ErrorMessage = errorMessage
        };

        return new ViewResult
        {
            ViewName = "~/Views/Shared/Error.cshtml",
            ViewData = new ViewDataDictionary<ErrorViewModel>(new EmptyModelMetadataProvider(),new ModelStateDictionary())
            {
                Model = errorViewModel
            }
        };
    }
    public static ViewResult ErrorView(Exception ex)
    {
        var errorViewModel = new ErrorViewModel
        {
            ErrorMessage = ex?.Message ?? "An unexpected error occurred. Please try again later."
        };

        return new ViewResult
        {
            ViewName = "~/Views/Shared/Error.cshtml",
            ViewData = new ViewDataDictionary<ErrorViewModel>(new EmptyModelMetadataProvider(),new ModelStateDictionary())
            {
                Model = errorViewModel
            }
        };
    }
    public static ErrorViewModel Error(string errorMessage)
    {
        ErrorViewModel viewModel = new ErrorViewModel()
        {
            ErrorMessage = errorMessage
        };
        return viewModel;
    }
}

public class ErrorViewModel
{
    public string ErrorMessage { get; set; }

    public bool ShowErrorMessage => !string.IsNullOrEmpty(ErrorMessage);
}
