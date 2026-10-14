using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.Repositories.Definitions;

public class LookupCrudRepository<T> : ILookupCrudRepository<T> where T : BaseEntity
{
    private static readonly bool HasCode = typeof(T).GetProperty("Code") is not null;

    private readonly ApplicationDbContext _context;

    public LookupCrudRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<T>> GetPagedAsync(PageRequest request)
    {
        var query = _context.Set<T>().AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchString))
        {
            var search = request.SearchString.Trim().ToLower();
            query = HasCode
                ? query.Where(a => EF.Property<string>(a, "ArName").ToLower().Contains(search)
                                || EF.Property<string>(a, "EnName").ToLower().Contains(search)
                                || EF.Property<string>(a, "Code").ToLower().Contains(search))
                : query.Where(a => EF.Property<string>(a, "ArName").ToLower().Contains(search)
                                || EF.Property<string>(a, "EnName").ToLower().Contains(search));
        }

        var total = await query.CountAsync();
        var data = await query
            .OrderBy(a => a.Id)
            .Skip(request.Offset)
            .Take(request.PageSize)
            .ToListAsync();

        return PaginatedResult<T>.Create(data, total, request);
    }

    public async Task<T> GetByIdAsync(int id)
    {
        return await _context.Set<T>().FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<string> NewCodeAsync()
    {
        if (!HasCode)
        {
            return null;
        }

        var codes = await _context.Set<T>().AsNoTracking().Select(a => EF.Property<string>(a, "Code")).ToListAsync();

        var next = codes
            .Where(code => int.TryParse(code, out _))
            .Select(int.Parse)
            .DefaultIfEmpty(0)
            .Max() + 1;

        return next.ToString();
    }

    public async Task<IEnumerable<LookupItem>> GetOptionsAsync(int? excludeId)
    {
        return await _context.Set<T>().AsNoTracking()
            .Where(a => !excludeId.HasValue || a.Id != excludeId.Value)
            .OrderBy(a => a.Id)
            .Select(a => new LookupItem
            {
                Id = a.Id,
                ArName = EF.Property<string>(a, "ArName"),
                EnName = EF.Property<string>(a, "EnName")
            })
            .ToListAsync();
    }

    public async Task<ResultMessage> UpsertAsync(T entity)
    {
        try
        {
            if (entity.Id == 0)
            {
                entity.CreatedAt = DateTime.Now;
                await _context.Set<T>().AddAsync(entity);
            }
            else
            {
                var existing = await _context.Set<T>().FirstOrDefaultAsync(a => a.Id == entity.Id);
                if (existing is null)
                {
                    return new ResultMessage
                    {
                        Success = false,
                        Message = "RecordNotFound"
                    };
                }

                // The generated code is never wiped by a form that arrives without it.
                var code = typeof(T).GetProperty("Code");
                if (code is not null && string.IsNullOrWhiteSpace(code.GetValue(entity) as string))
                {
                    code.SetValue(entity, code.GetValue(existing));
                }

                entity.State = existing.State;
                entity.CreatedAt = existing.CreatedAt;
                entity.CreatedById = existing.CreatedById;
                entity.UpdatedAt = DateTime.Now;
                _context.Entry(existing).CurrentValues.SetValues(entity);
            }

            await _context.SaveChangesAsync();

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

    public async Task<ResultMessage> DeleteAsync(int id)
    {
        try
        {
            var entity = await _context.Set<T>().FirstOrDefaultAsync(a => a.Id == id);
            if (entity is null)
            {
                return new ResultMessage { Success = true };
            }

            _context.Set<T>().Remove(entity);
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
