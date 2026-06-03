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
            return _purchaseOrderRepository.CreatePurchaseOrderAsync(
                NormalizeAndValidate(order),
                cancellationToken);
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

        private static PurchaseOrder NormalizeAndValidate(PurchaseOrder order)
        {
            var seatClass = order.SeatClass.Trim();
            if (string.IsNullOrWhiteSpace(seatClass))
            {
                throw new ArgumentException("seatClass is required.");
            }

            if (order.Routes.Count == 0)
            {
                throw new ArgumentException("At least one route is required.");
            }

            if (order.Passengers.Count == 0)
            {
                throw new ArgumentException("At least one passenger is required.");
            }

            var orderedRoutes = order.Routes
                .OrderBy(route => route.SequenceNumber)
                .Select((route, index) =>
                {
                    if (route.RouteId <= 0)
                    {
                        throw new ArgumentException("routeId must be greater than 0.");
                    }

                    if (route.IntendedDate is null)
                    {
                        throw new ArgumentException("intendedDate is required for every route.");
                    }

                    return new PurchaseOrderRoute
                    {
                        PurchaseOrderId = route.PurchaseOrderId,
                        SequenceNumber = index + 1,
                        RouteId = route.RouteId,
                        IntendedDate = route.IntendedDate
                    };
                })
                .ToList();

            return new PurchaseOrder
            {
                Id = order.Id,
                SeatClass = seatClass,
                Routes = orderedRoutes,
                Passengers = order.Passengers
            };
        }
    }
}
