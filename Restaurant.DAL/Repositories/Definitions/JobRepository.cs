using Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.Repositories.Definitions;

public class JobRepository : IJobRepository
{
    private readonly ApplicationDbContext _context;

    public JobRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<Job>> GetPagedJobsAsync(PageRequest request)
    {
        var query = _context.Jobs.AsNoTracking().AsQueryable();

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

        return PaginatedResult<Job>.Create(data, total, request);
    }

    public async Task<Job> GetJobByIdAsync(int id)
    {
        return await _context.Jobs.FindAsync(id);
    }

    public async Task<string> NewJobCodeAsync()
    {
        var codes = await _context.Jobs.AsNoTracking().Select(a => a.Code).ToListAsync();

        var nextCode = codes
            .Where(code => int.TryParse(code, out _))
            .Select(int.Parse)
            .DefaultIfEmpty(0)
            .Max() + 1;

        return nextCode.ToString();
    }

    public async Task<ResultMessage> UpsertJobAsync(Job entity)
    {
        try
        {
            if (entity.Id == 0)
            {
                entity.Code = await NewJobCodeAsync();
                await _context.Jobs.AddAsync(entity);
                await _context.SaveChangesAsync();
            }
            else
            {
                var existing = await _context.Jobs.AsNoTracking().FirstOrDefaultAsync(a => a.Id == entity.Id);
                if (existing is null)
                {
                    return new ResultMessage
                    {
                        Success = false,
                        Message = "RecordNotFound"
                    };
                }

                entity.Code = existing.Code;
                entity.State = existing.State;
                entity.CreatedAt = existing.CreatedAt;
                entity.CreatedById = existing.CreatedById;
                entity.UpdatedAt = DateTime.Now;
                _context.Jobs.Update(entity);
                await _context.SaveChangesAsync();
            }

            return new ResultMessage { Success = true };
        }
        catch (Exception ex)
        {
            return new ResultMessage
            {
                Success = false,
                Message = ErrorLog.Message(ex)
            };
        }
    }

    public async Task<ResultMessage> DeleteJobAsync(int id)
    {
        try
        {
            var entity = await _context.Jobs.FindAsync(id);
            if (entity is null)
            {
                return new ResultMessage { Success = true };
            }

            if (await _context.Employees.AnyAsync(a => a.Id == id))
            {
                return new ResultMessage
                {
                    Success = false,
                    Message = "CannotDeleteLinkedRecord"
                };
            }
            _context.Jobs.Remove(entity);
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
                Message = ErrorLog.Message(ex)
            };
        }
    }
}
