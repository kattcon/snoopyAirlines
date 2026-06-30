using snoopy_airlines_backend.Domain;
using SnoopyAirlines.Util.Email;

namespace SnoopyAirlines.Util.Pdf
{
    public interface IInvoicePdfGenerator
    {
        Attachment GenerateInvoice(PurchaseOrderEmailData data);
    }
}
