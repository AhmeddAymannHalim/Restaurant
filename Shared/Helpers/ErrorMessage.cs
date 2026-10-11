using Shared.ViewModels.Commons;

namespace Shared.Helpers;

public static class ErrorMessage
{
    public static ErrorViewModel Error(string errorMessage)
    {
        ErrorViewModel viewModel = new ErrorViewModel()
        {
            ErrorMessage = errorMessage
        };
        return viewModel;
    }
}
