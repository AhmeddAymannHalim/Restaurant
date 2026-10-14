using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.Repositories.Definitions;

public class StockMovementRepository : IStockMovementRepository
{
    private readonly ApplicationDbContext _context;

    public StockMovementRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<StoreProductMovement>> GetPagedAsync(PageRequest request, int? storeId, int? productId)
    {
        var query = _context.StoreProductMovements.AsNoTracking()
            .Include(a => a.Store)
            .Include(a => a.Product)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchString))
        {
            var search = request.SearchString.Trim().ToLower();
            query = query.Where(a => a.ReferenceCode.ToLower().Contains(search)
                || a.Product.ArName.ToLower().Contains(search)
                || a.Product.EnName.ToLower().Contains(search));
        }

        if (storeId.HasValue && storeId.Value > 0)
        {
            query = query.Where(a => a.StoreId == storeId.Value);
        }

        if (productId.HasValue && productId.Value > 0)
        {
            query = query.Where(a => a.ProductId == productId.Value);
        }

        if (Enum.TryParse<StockMovementSource>(request.Type, out var source))
        {
            query = query.Where(a => a.Source == source);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(a => a.MovementDate >= request.FromDate.Value.Date);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(a => a.MovementDate < request.ToDate.Value.Date.AddDays(1));
        }

        var total = await query.CountAsync();
        var data = await query
            .OrderByDescending(a => a.MovementDate)
            .ThenByDescending(a => a.Id)
            .Skip(request.Offset)
            .Take(request.PageSize)
            .ToListAsync();

        return PaginatedResult<StoreProductMovement>.Create(data, total, request);
    }
}
