using Microsoft.EntityFrameworkCore;
using Restaurant.DAL;
using Restaurant.DAL.Models;
using Restaurant.DAL.IRepositories;

namespace Restaurant.DAL.Repositories
{
    public class InvoiceItemRepository : GenericRepository<InvoiceItem>, IInvoiceItemRepository
    {
        private readonly ApplicationDbContext _context;

        public InvoiceItemRepository(ApplicationDbContext context) : base(context)
        {
            this._context = context;
        }

        public Task<List<InvoiceItem>> GetByInvoiceId(int invoiceId)
        {
            return _context.Set<InvoiceItem>().
                Where(a => a.InvoiceId == invoiceId).
                Include(a => a.Invoice).
                Include(a => a.StoreProduct).ToListAsync();
        }


        public List<StoreProduct> GetByStoreId(int storeId)
        {
            return _context.Set<StoreProduct>().
                Where(a => a.StoreId == storeId).
                Include(a => a.Store).
                Include(a => a.Product).ToList();
        }
    }
}
