using FluentValidation;
using Microsoft.Extensions.Localization;
using Shared;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Validations.Gallery;

public class GalleryVmValidator : AbstractValidator<GalleryViewModel>
{
    public GalleryVmValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.ImageFile).NotNull().When(x => x.Id == 0).WithMessage(localizer["Val.RequiredField"].Value);
    }
}
