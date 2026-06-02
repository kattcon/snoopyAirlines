using snoopy_airlines_backend.Domain;
using snoopy_airlines_backend.Repositories;

namespace snoopy_airlines_backend.Services
{
    public class PurchaseOrderService
    {
        private readonly IPurchaseOrderRepository _purchaseOrderRepository;

        public PurchaseOrderService(IPurchaseOrderRepository purchaseOrderRepository)
        {
            _purchaseOrderRepository = purchaseOrderRepository;
        }

        public Task<PurchaseOrder> CreatePurchaseOrderAsync(
            PurchaseOrder order,
            CancellationToken cancellationToken)
        {
            return _purchaseOrderRepository.CreatePurchaseOrderAsync(order, cancellationToken);
        }

        public Task<IReadOnlyCollection<PurchaseOrder>> GetPurchaseOrdersAsync(
            CancellationToken cancellationToken)
        {
            return _purchaseOrderRepository.GetPurchaseOrdersAsync(cancellationToken);
        }

        public Task<PurchaseOrder?> GetPurchaseOrderByIdAsync(
            int id,
            CancellationToken cancellationToken)
        {
            return _purchaseOrderRepository.GetPurchaseOrderByIdAsync(id, cancellationToken);
        }
    }
}
