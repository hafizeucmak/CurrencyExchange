namespace CurrencyExchange.Domain.Entites
{
    public class UserRole : DomainEntity
    {
        public UserRole(string roleName, string description)
        {
            RoleName = roleName;
            Description = description;
        }

        public string RoleName { get; set; }
        public string Description { get; set; }
    }
}
