using CurrencyExchange.Application.CQRS.Queries;
using CurrencyExchange.Application.CQRS.Queries.Currency;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CurrencyExchange.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CurrencyController : ControllerBase
    {
        private readonly IMediator _mediator;
        public CurrencyController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("createProduct")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> CreateProduct([FromQuery] string fromCurrency, [FromQuery] string toCurrency, CancellationToken cancellationToken)
        {
            await _mediator.Send(new GetCurrencyRateQuery(fromCurrency, toCurrency), cancellationToken);
            return NoContent();
        }
    }
}
