using CurrencyExchange.Application.CQRS.Queries.Currency;
using CurrencyExchange.Application.DTOs.Currency;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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

        /// <summary>
        /// Converts an amount from one currency to another using the latest exchange rate.
        /// </summary>
        /// <param name="fromCurrency">The source currency code (e.g., USD).</param>
        /// <param name="toCurrency">The target currency code (e.g., EUR).</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>The converted currency amount and related information.</returns>
        [HttpGet("convertCurrency")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(CurrencyConversionOutputDto))]
        public async Task<IActionResult> ConvertCurrency([FromQuery] string fromCurrency, [FromQuery] string toCurrency, CancellationToken cancellationToken)
        {
            return Ok(await _mediator.Send(new GetConvertedCurrencyRate(fromCurrency, toCurrency), cancellationToken));
        }

        /// <summary>
        /// Retrieves the latest exchange rates for the specified base currency.
        /// </summary>
        /// <param name="baseCurrency">The base currency code (e.g., USD, EUR).</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>The latest exchange rates for the given base currency.</returns>
        [HttpGet("getLatestExchangeRates")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(LatestExchangeRatesOutputDto))]
        public async Task<IActionResult> GetLatestExchangeRates([FromQuery] string baseCurrency, CancellationToken cancellationToken)
        {
            return Ok(await _mediator.Send(new GetLatestExchangeRatesQuery(baseCurrency), cancellationToken));
        }

        /// <summary>
        /// Retrieves historical exchange rates between two dates with pagination.
        /// </summary>
        /// <param name="baseCurrency">Base currency (e.g., EUR)</param>
        /// <param name="startDate">Start date in yyyy-MM-dd</param>
        /// <param name="endDate">End date in yyyy-MM-dd</param>
        /// <param name="page">Page number (default = 1)</param>
        /// <param name="pageSize">Page size (default = 10)</param>
        /// <returns>Paged list of historical exchange rates</returns>
        [HttpGet("GetHistoricalExchangeRates")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(PaginatedHistoricalRatesOutputDto))]
        public async Task<IActionResult> GetHistoricalExchangeRates( [FromQuery] string baseCurrency,
                                                                     [FromQuery] DateTime startDate,
                                                                     [FromQuery] DateTime endDate,
                                                                     CancellationToken cancellationToken,
                                                                     [FromQuery] int page = 1,
                                                                     [FromQuery] int pageSize = 10)
        {
            return Ok(await _mediator.Send(new GetHistoricalExchangeRatesQuery(baseCurrency, startDate, endDate, page, pageSize), cancellationToken));
        }
    }
}