using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.Repositories.Definitions;

public class DeliveryZoneRepository : IDeliveryZoneRepository
{
    private readonly ApplicationDbContext _context;

    public DeliveryZoneRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<DeliveryZone>> GetPagedDeliveryZonesAsync(PageRequest request)
    {
        var query = _context.DeliveryZones.AsNoTracking().AsQueryable();

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

        return PaginatedResult<DeliveryZone>.Create(data, total, request);
    }

    public async Task<List<DeliveryZone>> GetAllDeliveryZonesAsync()
    {
        return await _context.DeliveryZones
            .AsNoTracking()
            .OrderBy(a => a.Fee)
            .ThenBy(a => a.ArName)
            .ToListAsync();
    }

    public async Task<DeliveryZone> GetDeliveryZoneByIdAsync(int id)
    {
        return await _context.DeliveryZones.FindAsync(id);
    }

    public async Task<ResultMessage> UpsertDeliveryZoneAsync(DeliveryZone entity)
    {
        try
        {
            if (entity.Id == 0)
            {
                await _context.DeliveryZones.AddAsync(entity);
                await _context.SaveChangesAsync();
            }
            else
            {
                var existing = await _context.DeliveryZones.AsNoTracking().FirstOrDefaultAsync(a => a.Id == entity.Id);
                if (existing is null)
                {
                    return new ResultMessage
                    {
                        Success = false,
                        Message = "RecordNotFound"
                    };
                }

                _context.DeliveryZones.Update(entity);
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

    public async Task<ResultMessage> DeleteDeliveryZoneAsync(int id)
    {
        try
        {
            var entity = await _context.DeliveryZones.FindAsync(id);
            if (entity is null)
            {
                return new ResultMessage { Success = true };
            }

            _context.DeliveryZones.Remove(entity);
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
