using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface ILookupCrudService<T> where T : BaseEntity, new()
{
    Task<PaginatedResult<LookupEditViewModel>> GetPagedAsync(PageRequest request);
    Task<LookupEditViewModel> GetByIdAsync(int id, IReadOnlyList<LookupField> fields);
    Task<string> NewCodeAsync();
    Task<IEnumerable<LookupItem>> GetParentOptionsAsync(int? excludeId);

    /// <summary>Returns the error message of every invalid field (empty when the form is valid).</summary>
    Dictionary<string, string> Validate(LookupEditViewModel model, IReadOnlyList<LookupField> fields);

    Task<ResultMessage> UpsertAsync(LookupEditViewModel model, IReadOnlyList<LookupField> fields);
    Task<ResultMessage> DeleteAsync(int id);
}
