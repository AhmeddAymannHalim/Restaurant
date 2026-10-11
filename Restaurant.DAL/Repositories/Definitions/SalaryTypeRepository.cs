using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.Repositories.Definitions;

public class SalaryTypeRepository : ISalaryTypeRepository
{
    private readonly ApplicationDbContext _context;

    public SalaryTypeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<SalaryType>> GetPagedSalaryTypesAsync(PageRequest request)
    {
        var query = _context.SalaryTypes.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchString))
        {
            var search = request.SearchString.Trim().ToLower();
            query = query.Where(a => a.ArName.ToLower().Contains(search) || a.EnName.ToLower().Contains(search));
        }

        var total = await query.CountAsync();
        var data = await query
            .OrderBy(a => a.Id)
            .Skip(request.Offset)
            .Take(request.PageSize)
            .ToListAsync();

        return PaginatedResult<SalaryType>.Create(data, total, request);
    }

    public async Task<SalaryType> GetSalaryTypeByIdAsync(int id)
    {
        return await _context.SalaryTypes.FindAsync(id);
    }

    public async Task<ResultMessage> UpsertSalaryTypeAsync(SalaryType entity)
    {
        try
        {
            if (entity.Id == 0)
            {
                await _context.SalaryTypes.AddAsync(entity);
                await _context.SaveChangesAsync();
            }
            else
            {
                var existing = await _context.SalaryTypes.AsNoTracking().FirstOrDefaultAsync(a => a.Id == entity.Id);
                if (existing is null)
                {
                    return new ResultMessage { Success = false, Message = "RecordNotFound" };
                }

                _context.SalaryTypes.Update(entity);
                await _context.SaveChangesAsync();
            }

            return new ResultMessage { Success = true };
        }
        catch (Exception ex)
        {
            return new ResultMessage { Success = false, Message = ex.Message };
        }
    }

    public async Task<ResultMessage> DeleteSalaryTypeAsync(int id)
    {
        try
        {
            var entity = await _context.SalaryTypes.FindAsync(id);
            if (entity is null)
            {
                return new ResultMessage { Success = true };
            }

            _context.SalaryTypes.Remove(entity);
            await _context.SaveChangesAsync();

            return new ResultMessage { Success = true };
        }
        catch (DbUpdateException)
        {
            return new ResultMessage { Success = false, Message = "CannotDeleteLinkedRecord" };
        }
        catch (Exception ex)
        {
            return new ResultMessage { Success = false, Message = ex.Message };
        }
    }
}
