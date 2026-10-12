using FluentValidation;
using Microsoft.Extensions.Localization;
using Restaurant.DAL.IRepositories.Definitions;
using Shared;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Validations.Role;

public class RoleVmValidator : AbstractValidator<RoleViewModel>
{
    public RoleVmValidator(IStringLocalizer<SharedResource> localizer, IRoleRepository repository)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(localizer["Val.RequiredField"].Value)
            .MaximumLength(50)
            .WithMessage(string.Format(localizer["Val.MaxLength"].Value, 50))
            .Must((model, name) => !repository.IsNameExist(model.Id, name))
            .WithMessage(localizer["Val.NameAlreadyExist"].Value);
    }
}
