using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.Repositories.Definitions;

public class SpecializationRepository : ISpecializationRepository
{
    private readonly ApplicationDbContext _context;

    public SpecializationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<Specialization>> GetPagedSpecializationsAsync(PageRequest request)
    {
        var query = _context.Specializations.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchString))
        {
            var search = request.SearchString.Trim().ToLower();
            query = query.Where(a => a.ArName.ToLower().Contains(search)
                || a.EnName.ToLower().Contains(search)
                || a.Code.ToLower().Contains(search));
        }

        var total = await query.CountAsync();
        var data = await query
            .OrderBy(a => a.Id)
            .Skip(request.Offset)
            .Take(request.PageSize)
            .ToListAsync();

        return PaginatedResult<Specialization>.Create(data, total, request);
    }

    public async Task<Specialization> GetSpecializationByIdAsync(int id)
    {
        return await _context.Specializations.FindAsync(id);
    }

    public async Task<string> NewSpecializationCodeAsync()
    {
        var codes = await _context.Specializations.AsNoTracking().Select(a => a.Code).ToListAsync();

        var nextCode = codes
            .Where(code => int.TryParse(code, out _))
            .Select(int.Parse)
            .DefaultIfEmpty(0)
            .Max() + 1;

        return nextCode.ToString();
    }

    public async Task<ResultMessage> UpsertSpecializationAsync(Specialization entity)
    {
        try
        {
            if (entity.Id == 0)
            {
                if (string.IsNullOrWhiteSpace(entity.Code))
                {
                    entity.Code = await NewSpecializationCodeAsync();
                }

                entity.CreatedAt = DateTime.Now;
                await _context.Specializations.AddAsync(entity);
                await _context.SaveChangesAsync();
            }
            else
            {
                var existing = await _context.Specializations.FirstOrDefaultAsync(a => a.Id == entity.Id);
                if (existing is null)
                {
                    return new ResultMessage
                    {
                        Success = false,
                        Message = "RecordNotFound"
                    };
                }

                entity.Code = string.IsNullOrWhiteSpace(entity.Code) ? existing.Code : entity.Code;
                entity.State = existing.State;
                entity.CreatedAt = existing.CreatedAt;
                entity.CreatedById = existing.CreatedById;
                entity.UpdatedAt = DateTime.Now;
                _context.Entry(existing).CurrentValues.SetValues(entity);
                await _context.SaveChangesAsync();
            }

            return new ResultMessage { Success = true };
        }
        catch (Exception ex)
        {
            return new ResultMessage
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ResultMessage> DeleteSpecializationAsync(int id)
    {
        try
        {
            var entity = await _context.Specializations.FindAsync(id);
            if (entity is null)
            {
                return new ResultMessage { Success = true };
            }

            _context.Specializations.Remove(entity);
            await _context.SaveChangesAsync();

            return new ResultMessage { Success = true };
        }
        catch (DbUpdateException)
        {
            return new ResultMessage
            {
                Success = false,
                Message = "CannotDeleteLinkedRecord"
            };
        }
        catch (Exception ex)
        {
            return new ResultMessage
            {
                Success = false,
                Message = ex.Message
            };
        }
    }
}
