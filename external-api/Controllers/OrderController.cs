using Microsoft.AspNetCore.Mvc;
using SnoopyAirlines.External.Domain.Intake;
using SnoopyAirlines.External.Domain.View;
using SnoopyAirlines.External.Services;

namespace SnoopyAirlines.External.Controllers
{
    [ApiController]
    [Route("api/external/order")]
    public class OrderController : ControllerBase
    {
        private readonly OrderService _orderService;

        public OrderController(OrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<ActionResult<ExternalOrderResponse>> Post(
            [FromBody] ExternalOrderIntake order,
            CancellationToken cancellationToken)
        {
            try
            {
                return Ok(await _orderService.CreateOrderAsync(order, cancellationToken));
            }
            catch (OrderException exception)
            {
                return BadRequest(
                    new ExternalOrderErrorResponse
                    {
                        ErrorCode = StatusCodes.Status400BadRequest,
                        Message = exception.Message,
                        FieldName = exception.FieldName
                    });
            }
        }
    }
}
