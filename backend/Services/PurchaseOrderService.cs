using snoopy_airlines_backend.Domain;
using snoopy_airlines_backend.Repositories;
using SnoopyAirlines.domain;
using SnoopyAirlines.Domain.View;
using SnoopyAirlines.Repositories;
using SnoopyAirlines.Domain.EmailTemplate;

namespace snoopy_airlines_backend.Services
{
    public class PurchaseOrderService
    {
        private readonly PurchaseOrderRepository _purchaseOrderRepository;
        private readonly IEmailSender _emailSender;

        public PurchaseOrderService(PurchaseOrderRepository purchaseOrderRepository, IEmailSender emailSender)
        {
            _purchaseOrderRepository = purchaseOrderRepository;
            _emailSender = emailSender;
        }

        public Task<PurchaseOrder> CreatePurchaseOrderAsync(PurchaseOrder order, CancellationToken cancellationToken)
        {
            return _purchaseOrderRepository.CreatePurchaseOrderAsync(order, cancellationToken);
        }

        public Task<IReadOnlyCollection<PurchaseOrder>> GetPurchaseOrdersAsync(CancellationToken cancellationToken)
        {
            return _purchaseOrderRepository.GetPurchaseOrdersAsync(cancellationToken);
        }

        public async Task SendPurchaseEmailAsync(
            string recipientEmail,
            int purchaseOrderId,
            CancellationToken cancellationToken)
        {
            var data = await _purchaseOrderRepository.GetPurchaseOrderDetailsAsync(purchaseOrderId, cancellationToken);

            if (data is null)
            {
                throw new InvalidOperationException(
                    $"Purchase order {purchaseOrderId} not found.");
            }

            var confirmationHtml = PurchaseOrderEmailTemplate.BuildConfirmation(data);
            var itineraryHtml = PurchaseOrderEmailTemplate.BuildItinerary(data);

            await _emailSender.SendAsync(
                recipientEmail,
                "Confirmación de compra - Snoopy Airlines",
                confirmationHtml,
                cancellationToken,
                isHtml: true);

            await _emailSender.SendAsync(
                recipientEmail,
                "Itinerario de viaje - Snoopy Airlines",
                itineraryHtml,
                cancellationToken,
                isHtml: true);
        }
    }
}
