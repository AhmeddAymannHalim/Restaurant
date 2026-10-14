using System.Globalization;
using Microsoft.Extensions.Localization;
using Restaurant.BLL.Helper;
using Restaurant.BLL.Services.IServices.Commons;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared;
using Shared.Enums;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class LookupCrudService<T> : ILookupCrudService<T> where T : BaseEntity, new()
{
    private static readonly bool HasCode = typeof(T).GetProperty("Code") is not null;

    private readonly ILookupCrudRepository<T> _repository;
    private readonly IValidatorService<T> _validator;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public LookupCrudService(
        ILookupCrudRepository<T> repository,
        IValidatorService<T> validator,
        IStringLocalizer<SharedResource> localizer)
    {
        _repository = repository;
        _validator = validator;
        _localizer = localizer;
    }

    public async Task<PaginatedResult<LookupEditViewModel>> GetPagedAsync(PageRequest request)
    {
        var result = await _repository.GetPagedAsync(request);
        var rows = result.Data.Select(a => LookupEntityMapper.ToViewModel(a, Array.Empty<LookupField>())).ToList();

        return PaginatedResult<LookupEditViewModel>.Create(rows, result.TotalRecords, request);
    }

    public async Task<LookupEditViewModel> GetByIdAsync(int id, IReadOnlyList<LookupField> fields)
    {
        var entity = await _repository.GetByIdAsync(id);

        return entity is null ? null : LookupEntityMapper.ToViewModel(entity, fields);
    }

    public Task<string> NewCodeAsync()
    {
        return _repository.NewCodeAsync();
    }

    public Task<IEnumerable<LookupItem>> GetParentOptionsAsync(int? excludeId)
    {
        return _repository.GetOptionsAsync(excludeId);
    }

    public Dictionary<string, string> Validate(LookupEditViewModel model, IReadOnlyList<LookupField> fields)
    {
        var errors = new Dictionary<string, string>();
        var required = _localizer["Val.RequiredField"].Value;
        string MaxLength(int length) => string.Format(_localizer["Val.MaxLength"].Value, length);

        if (string.IsNullOrWhiteSpace(model.ArName))
        {
            errors[nameof(model.ArName)] = required;
        }
        else if (model.ArName.Trim().Length > 150)
        {
            errors[nameof(model.ArName)] = MaxLength(150);
        }
        else if (_validator.IsArNameExist(model.Id, model.ArName))
        {
            errors[nameof(model.ArName)] = _localizer["Val.NameAlreadyExist"].Value;
        }

        if (string.IsNullOrWhiteSpace(model.EnName))
        {
            errors[nameof(model.EnName)] = required;
        }
        else if (model.EnName.Trim().Length > 150)
        {
            errors[nameof(model.EnName)] = MaxLength(150);
        }
        else if (_validator.IsEnNameExist(model.Id, model.EnName))
        {
            errors[nameof(model.EnName)] = _localizer["Val.NameAlreadyExist"].Value;
        }

        if (HasCode && !string.IsNullOrWhiteSpace(model.Code))
        {
            if (model.Code.Trim().Length > 20)
            {
                errors[nameof(model.Code)] = MaxLength(20);
            }
            else if (_validator.IsCodeExist(model.Id, model.Code))
            {
                errors[nameof(model.Code)] = _localizer["Val.CodeAlreadyExist"].Value;
            }
        }

        foreach (var field in fields)
        {
            model.Extras.TryGetValue(field.Name, out var raw);
            var key = "Extras[" + field.Name + "]";

            switch (field.Kind)
            {
                case LookupFieldKind.Text when raw is { Length: > 500 }:
                    errors[key] = MaxLength(500);
                    break;

                case LookupFieldKind.Decimal when !string.IsNullOrWhiteSpace(raw):
                    if (!decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var number) || number < 0)
                    {
                        errors[key] = required;
                    }

                    break;

                case LookupFieldKind.Int when !string.IsNullOrWhiteSpace(raw):
                    if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var whole) || whole < 0)
                    {
                        errors[key] = required;
                    }

                    break;

                case LookupFieldKind.Parent when model.Id > 0
                                                  && int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parent)
                                                  && parent == model.Id:
                    errors[key] = _localizer["Val.CannotBeOwnParent"].Value;
                    break;
            }
        }

        return errors;
    }

    public async Task<ResultMessage> UpsertAsync(LookupEditViewModel model, IReadOnlyList<LookupField> fields)
    {
        var entity = LookupEntityMapper.ToEntity<T>(model, fields);

        if (model.Id == 0 && HasCode && string.IsNullOrWhiteSpace(model.Code))
        {
            typeof(T).GetProperty("Code")!.SetValue(entity, await _repository.NewCodeAsync());
        }

        return await _repository.UpsertAsync(entity);
    }

    public Task<ResultMessage> DeleteAsync(int id)
    {
        return _repository.DeleteAsync(id);
    }
}
