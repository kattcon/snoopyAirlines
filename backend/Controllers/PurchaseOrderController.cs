using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using snoopy_airlines_backend.Domain;
using snoopy_airlines_backend.Services;
using SnoopyAirlines.Domain.Intake;

namespace SnoopyAirlines.Controllers
{
    [ApiController]
    [Route("PurchaseOrder")]
    public class PurchaseOrderController : ControllerBase
    {
        private readonly PurchaseOrderService _purchaseOrderService;
        
        public PurchaseOrderController(PurchaseOrderService purchaseOrderService)
        {
            _purchaseOrderService = purchaseOrderService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IReadOnlyCollection<PurchaseOrder>>> GetPurchaseOrdersAsync(CancellationToken cancellationToken)
        {
            var orders = await _purchaseOrderService.GetPurchaseOrdersAsync(cancellationToken);
            return Ok(orders);
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<PurchaseOrder>> GetPurchaseOrderByIdAsync(int id, CancellationToken cancellationToken)
        {
            var order = await _purchaseOrderService.GetPurchaseOrderByIdAsync(id, cancellationToken);
            return order is null ? NotFound() : Ok(order);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<PurchaseOrder>> Post([FromBody] PurchaseOrderIntake purchaseIntake, CancellationToken cancellationToken)
        {
            try
            {
                var order = new PurchaseOrder
                {
                    SeatClass = purchaseIntake.SeatClass,
                    Routes = CreateRoutes(purchaseIntake).ToList(),
                    Passengers = purchaseIntake.Passengers.Select(passenger => new Passenger
                    {
                        Gender = passenger.Gender,
                        FirstName = passenger.FirstName,
                        LastName = passenger.LastName,
                        BirthDay = passenger.BirthDay,
                        BirthMonth = passenger.BirthMonth,
                        BirthYear = passenger.BirthYear,
                        Nationality = passenger.Nationality
                    }).ToList()
                };

                var savedPurchaseOrder = await _purchaseOrderService.CreatePurchaseOrderAsync(order, cancellationToken);
                return Ok(savedPurchaseOrder);
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new { Message = exception.Message });
            }
        }

        private static IEnumerable<PurchaseOrderRoute> CreateRoutes(PurchaseOrderIntake purchaseIntake)
        {
            return purchaseIntake.Routes.Select((route, index) => new PurchaseOrderRoute
            {
                SequenceNumber = route.SequenceNumber ?? index + 1,
                RouteId = route.RouteId,
                IntendedDate = route.IntendedDate
            });
        }
    }
}
