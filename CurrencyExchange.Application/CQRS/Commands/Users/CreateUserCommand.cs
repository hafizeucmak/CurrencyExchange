using CurrencyExchange.Domain.Constants;
using CurrencyExchange.Domain.Entites;
using CurrencyExchange.Infrastructure.DbContext;
using CurrencyExchange.Infrastructure.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CurrencyExchange.Application.CQRS.Commands.Users
{
    public class CreateUserCommand : IRequest<Unit>
    {
        private readonly CreateUserCommandValidator _validator = new();
        public CreateUserCommand(string firstName, string lastName, string email ,string password, string userRole)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Password = password;
            UserRole = userRole;

            _validator.ValidateAndThrow(this);
        }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Password { get; set; }

        public string Email { get; set; }

        public string UserRole { get; set; }
    }

    public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(DbContextConstants.MAX_LENGTH_FOR_FIRSTNAME);
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(DbContextConstants.MAX_LENGTH_FOR_LASTNAME);
            RuleFor(x => x.Email).NotEmpty()
                                 .MaximumLength(DbContextConstants.MAX_LENGTH_FOR_EMAIL)
                                 .EmailAddress()
                                 .WithMessage("Invalid email format.");
            RuleFor(x => x.Password).NotEmpty().MaximumLength(DbContextConstants.MAX_LENGTH_FOR_PASSWORD)
                                               .MinimumLength(DbContextConstants.MAX_LENGTH_FOR_PASSWORD)
                                               .WithMessage("Password must be in 8 digits");
            RuleFor(x => x.UserRole).NotEmpty()
                                    .MaximumLength(DbContextConstants.MAX_LENGTH_FOR_EMAIL)
                                    .Must(role => role == "User" || role == "Admin")
                                    .WithMessage("UserRole must be either 'User' or 'Admin'.");
        }
    }

    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Unit>
    {
        private readonly IGenericWriteRepository<BaseDbContext> _genericWriteRepository;
        private readonly ILogger<CreateUserCommandHandler> _logger;

        public CreateUserCommandHandler(IGenericWriteRepository<BaseDbContext> genericWriteRepository,
                                           ILogger<CreateUserCommandHandler> logger)
        {
            _genericWriteRepository = genericWriteRepository;
            _logger = logger;
        }

        public async Task<Unit> Handle(CreateUserCommand command, CancellationToken cancellationToken)
        {
            _genericWriteRepository.BeginTransaction();

            var isUserAlreadyExists = await _genericWriteRepository.GetAll<User>().AnyAsync(x => x.Email.Equals(command.Email), cancellationToken);

            if (isUserAlreadyExists)
            {
                throw new Exception($"User with email {command.Email} already exists.");
            }

            var userRole = await _genericWriteRepository.GetAll<UserRole>().FirstOrDefaultAsync(x => x.RoleName.Equals(command.UserRole), cancellationToken);

            if (userRole == null)
            { 
              throw new Exception($"UserRole with name {command.UserRole} does not exist.");    
            }
            
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(command.Password);

            var user = new User(command.FirstName, command.LastName, command.Email, userRole.Id, hashedPassword, userRole);

            await _genericWriteRepository.AddAsync(user, cancellationToken);

            return Unit.Value;
        }
    }
}
