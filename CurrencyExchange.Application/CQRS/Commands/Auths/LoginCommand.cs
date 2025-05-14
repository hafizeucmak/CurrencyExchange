using CurrencyExchange.Application.CQRS.Queries.Users;
using CurrencyExchange.Application.DTOs.Auths;
using CurrencyExchange.Application.Interfaces;
using CurrencyExchange.Domain.Constants;
using CurrencyExchange.Domain.Entites;
using CurrencyExchange.Infrastructure.DbContext;
using CurrencyExchange.Infrastructure.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CurrencyExchange.Application.CQRS.Commands.Auths
{
    public class LoginCommand : IRequest<AuthResponseOutputDto>
    {
        private readonly LoginCommandValidator _validator = new();
        public LoginCommand(string email, string password)
        {
            Email = email;
            Password = password;

            _validator.ValidateAndThrow(this);
        }

        public string Password { get; set; }

        public string Email { get; set; }
    }

    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Email).NotEmpty()
                                 .MaximumLength(DbContextConstants.MAX_LENGTH_FOR_EMAIL)
                                 .EmailAddress()
                                 .WithMessage("Invalid email format.");
            RuleFor(x => x.Password).NotEmpty().MaximumLength(DbContextConstants.MAX_LENGTH_FOR_PASSWORD)
                                               .MinimumLength(DbContextConstants.MAX_LENGTH_FOR_PASSWORD)
                                               .WithMessage("Password must be in 8 digits");
        }
    }

    public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseOutputDto>
    {
        private readonly IGenericWriteRepository<BaseDbContext> _genericWriteRepository;
        private readonly ILogger<LoginCommandHandler> _logger;
        private readonly IMediator _mediator;
        private readonly ITokenService _tokenService;

        public LoginCommandHandler(IGenericWriteRepository<BaseDbContext> genericWriteRepository,
                                   ILogger<LoginCommandHandler> logger,
                                   IMediator mediator,
                                   ITokenService tokenService)
        {
            _genericWriteRepository = genericWriteRepository;
            _logger = logger;
            _mediator = mediator;
            _tokenService = tokenService;
        }

        public async Task<AuthResponseOutputDto> Handle(LoginCommand command, CancellationToken cancellationToken)
        {
            _genericWriteRepository.BeginTransaction();

            var user = await _genericWriteRepository.GetAll<User>().FirstOrDefaultAsync(x => x.Email.Equals(command.Email), cancellationToken);

            if (user == null)
            {
                throw new UnauthorizedAccessException($"User not found with email {command.Email}.");
            }

            var isValidatedUser = await _mediator.Send(new ValidateUserCredentialsQuery(command.Email, command.Password), cancellationToken);

            if (!isValidatedUser)
            {
                throw new UnauthorizedAccessException($"Wrong password.");
            }

            var userRole = await _genericWriteRepository.GetAll<UserRole>().FirstOrDefaultAsync(x => x.Id == user.UserRoleId, cancellationToken);

            if (userRole == null)
            {
                throw new InvalidOperationException($"User role not found for role ID {user.UserRoleId}.");
            }

            var token = await _tokenService.GenerateToken(user.Email, user.ClientId.ToString(), userRole.RoleName);

            return new AuthResponseOutputDto
            {
                Token = token,
                UserRole = userRole.RoleName,
                FullName = $"{user.FirstName} {user.LastName}",
            };
        }
    }
}
