using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnoopyAirlines.Domain;
using SnoopyAirlines.Domain.Intake;
using SnoopyAirlines.Services;

namespace SnoopyAirlines.Controllers
{
    [ApiController]
    [Route("Booking")]
    public class BookingController : ControllerBase
    {
        private readonly BookingService _bookingService;

        public BookingController(BookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet("{bookingGuid:guid}")]
        [AllowAnonymous]
        public async Task<ActionResult<Booking>> GetByGuid(Guid bookingGuid, CancellationToken cancellationToken)
        {
            var booking = await _bookingService.GetByGuidAsync(bookingGuid, cancellationToken);
            return booking is null ? NotFound() : Ok(booking);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<Booking>> Post(BookingIntake bookingIntake, CancellationToken cancellationToken)
        {
            try
            {
                var booking = await _bookingService.BookAsync(
                    new BookingRequest
                    {
                        PurchaseOrderId = bookingIntake.PurchaseOrderId,
                        Email = bookingIntake.Email,
                        CardBrand = bookingIntake.CardBrand,
                        CardLastFour = bookingIntake.CardLastFour,
                        CardHolderName = bookingIntake.CardHolderName
                    },
                    cancellationToken);

                return Ok(booking);
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new { Message = exception.Message });
            }
            catch (InvalidOperationException exception)
            {
                return BadRequest(new { Message = exception.Message });
            }
        }
    }
}
