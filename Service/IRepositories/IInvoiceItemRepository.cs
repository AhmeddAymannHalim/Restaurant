using Restaurant.Data.Models;

namespace Service.IRepositories
{
    public interface IInvoiceItemRepository : IGenericRepository<InvoiceItem>
    {
        public Task<List<InvoiceItem>> GetByInvoiceId(int invoiceId);
        public List<StoreProduct> GetByStoreId(int storeId);
    }
}
