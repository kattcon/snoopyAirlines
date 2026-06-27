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

        [HttpPost("{confirmationCode}/cancellation-request")]
        [AllowAnonymous]
        public async Task<IActionResult> RequestCancellation(
            string confirmationCode,
            CancellationToken cancellationToken)
        {
            await _bookingService.RequestCancellationAsync(confirmationCode, cancellationToken);
            return Ok(new { message = "Si la reservación existe, se ha enviado un correo de confirmación." });
        }

        [AllowAnonymous]
        [HttpGet("cancel-booking")]
        public async Task<IActionResult> ConfirmCancellation(
            [FromQuery] string token,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest();
            }

            var success = await _bookingService.ConfirmCancellationAsync(token, cancellationToken);

            return Redirect(success
                ? "https://snoopyairlines.com/cancellation-result?success=true"
                : "https://snoopyairlines.com/cancellation-result?success=false");
        }
    }
}
