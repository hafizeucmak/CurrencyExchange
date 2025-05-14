using CurrencyExchange.Application.CQRS.Commands.Users;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CurrencyExchange.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;
        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("createUser")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> CreateUser(CreateUserCommand userCommand, CancellationToken cancellationToken)
        {
            return Ok(await _mediator.Send(userCommand, cancellationToken));
        }
    }
}
