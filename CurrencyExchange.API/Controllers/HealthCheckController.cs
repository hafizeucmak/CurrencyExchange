using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CurrencyExchange.API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class HealthCheckController : ControllerBase
    {
        public HealthCheckController()
        {
        }

        [HttpPost("isHealty")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public IActionResult IsHealty(CancellationToken cancellationToken)
        {
            return Ok();
        }
    }
}
