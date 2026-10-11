using Restaurant.DAL.Models;

namespace Restaurant.DAL.IRepositories
{
    public interface IInvoiceItemRepository : IGenericRepository<InvoiceItem>
    {
        public Task<List<InvoiceItem>> GetByInvoiceId(int invoiceId);
        public List<StoreProduct> GetByStoreId(int storeId);
    }
}
