using CurrencyExchange.Domain.Constants;
using FluentValidation;

namespace CurrencyExchange.Domain.Entites
{
    public class User(string firstName, string lastName, string email, int userRoleId, string hashedPassword, UserRole userRole) : DomainEntity
    {
        public Guid ClientId { get; private set; } = Guid.NewGuid();
        public string FirstName { get; private set; } = firstName;
        public string LastName { get; private set; } = lastName;
        public string Email { get; private set; } = email;
        public string HashedPassword { get; private set; } = hashedPassword;
        public virtual UserRole UserRole { get; private set; } = userRole;
        public int UserRoleId { get; private set; } = userRoleId;
    }

    public class ProductValidator : AbstractValidator<User>
    {
        public ProductValidator()
        {
            RuleFor(c => c.FirstName).NotEmpty()
                                     .MaximumLength(DbContextConstants.MAX_LENGTH_FOR_FIRSTNAME);
            RuleFor(c => c.LastName).NotEmpty()
                                    .MaximumLength(DbContextConstants.MAX_LENGTH_FOR_LASTNAME);
            RuleFor(c => c.Email).NotEmpty().MaximumLength(DbContextConstants.MAX_LENGTH_FOR_EMAIL);
            RuleFor(c => c.HashedPassword).NotEmpty();
        }
    }
}
