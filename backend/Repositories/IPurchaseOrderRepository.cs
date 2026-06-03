using snoopy_airlines_backend.Domain;

namespace snoopy_airlines_backend.Repositories
{
    public interface IPurchaseOrderRepository
    {
        Task<PurchaseOrder> CreatePurchaseOrderAsync(PurchaseOrder order, CancellationToken cancellationToken);
        Task<IReadOnlyCollection<PurchaseOrder>> GetPurchaseOrdersAsync(CancellationToken cancellationToken);
        Task<PurchaseOrder?> GetPurchaseOrderByIdAsync(int id, CancellationToken cancellationToken);
        Task<PurchaseOrderEmailData?> GetPurchaseOrderDetailsAsync(int purchaseOrderId, CancellationToken cancellationToken);
    }
}
