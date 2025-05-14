using CurrencyExchange.Domain.Constants;
using FluentValidation;

namespace CurrencyExchange.Domain.Entites
{
    public class User : DomainEntity
    {
        public User() { }
        public User(string firstName, string lastName, string email, int userRoleId, string hashedPassword, UserRole userRole)
        {
            ClientId = Guid.NewGuid();
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            HashedPassword = hashedPassword;
            UserRole = userRole;
            UserRoleId = userRoleId;
        }

        public Guid ClientId { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string Email { get; private set; }
        public string HashedPassword { get; private set; }
        public virtual UserRole UserRole { get; private set; }
        public int UserRoleId { get; private set; }
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
