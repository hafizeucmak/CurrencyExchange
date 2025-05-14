using CurrencyExchange.Domain.Constants;
using CurrencyExchange.Domain.Entites;
using CurrencyExchange.Infrastructure.DbContext;
using CurrencyExchange.Infrastructure.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CurrencyExchange.Application.CQRS.Queries.Users
{
    public class ValidateUserCredentialsQuery : IRequest<bool>
    {
        private readonly ValidateUserCredentialsQueryValidator _validator = new();
        public ValidateUserCredentialsQuery(string email, string password)
        {
            Email = email;
            Password = password;

            _validator.ValidateAndThrow(this);
        }

        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class ValidateUserCredentialsQueryValidator : AbstractValidator<ValidateUserCredentialsQuery>
    {
        public ValidateUserCredentialsQueryValidator()
        {
            RuleFor(x => x.Email).NotEmpty()
                                  .MaximumLength(DbContextConstants.MAX_LENGTH_FOR_EMAIL)
                                  .EmailAddress()
                                  .WithMessage("Invalid email format.");
            RuleFor(x => x.Password).NotEmpty().MaximumLength(DbContextConstants.MAX_LENGTH_FOR_PASSWORD)
                                               .MinimumLength(DbContextConstants.MAX_LENGTH_FOR_PASSWORD)
                                               .WithMessage("Password must be in 8 digits");
        }

        public class ValidateUserCredentialsQueryHandler : IRequestHandler<ValidateUserCredentialsQuery, bool>
        {
            private readonly IGenericReadRepository<BaseDbContext> _genericReadRepository;
            public ValidateUserCredentialsQueryHandler(IGenericReadRepository<BaseDbContext> genericReadRepository)
            {
                _genericReadRepository = genericReadRepository;
            }

            public async Task<bool> Handle(ValidateUserCredentialsQuery query, CancellationToken cancellationToken)
            {
                var user = await _genericReadRepository.GetAll<User>().FirstOrDefaultAsync(x => x.Email == query.Email, cancellationToken);

                if (user == null)
                {
                    throw new Exception($"User with email {query.Email} not found.");
                }

                return BCrypt.Net.BCrypt.Verify(query.Password, user.HashedPassword);
            }
        }
    }
}