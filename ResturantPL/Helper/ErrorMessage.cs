using ResturantPL.ViewModels;

namespace ResturantPL.Helper
{
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
}
