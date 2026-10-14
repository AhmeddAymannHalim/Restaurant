using Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.Repositories.Definitions;

public class InsuranceCompanyRepository : IInsuranceCompanyRepository
{
    private readonly ApplicationDbContext _context;

    public InsuranceCompanyRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<InsuranceCompany>> GetPagedInsuranceCompaniesAsync(PageRequest request)
    {
        var query = _context.InsuranceCompanies.AsNoTracking().AsQueryable();

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

        return PaginatedResult<InsuranceCompany>.Create(data, total, request);
    }

    public async Task<InsuranceCompany> GetInsuranceCompanyByIdAsync(int id)
    {
        return await _context.InsuranceCompanies.FindAsync(id);
    }

    public async Task<ResultMessage> UpsertInsuranceCompanyAsync(InsuranceCompany entity)
    {
        try
        {
            if (entity.Id == 0)
            {
                entity.CreatedAt = DateTime.Now;
                await _context.InsuranceCompanies.AddAsync(entity);
                await _context.SaveChangesAsync();
            }
            else
            {
                var existing = await _context.InsuranceCompanies.FirstOrDefaultAsync(a => a.Id == entity.Id);
                if (existing is null)
                {
                    return new ResultMessage
                    {
                        Success = false,
                        Message = "RecordNotFound"
                    };
                }

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
                Message = ErrorLog.Message(ex)
            };
        }
    }

    public async Task<ResultMessage> DeleteInsuranceCompanyAsync(int id)
    {
        try
        {
            var entity = await _context.InsuranceCompanies.FindAsync(id);
            if (entity is null)
            {
                return new ResultMessage { Success = true };
            }

            _context.InsuranceCompanies.Remove(entity);
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
