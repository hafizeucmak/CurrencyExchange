using CurrencyExchange.Domain.Constants;
using CurrencyExchange.Domain.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CurrencyExchange.Infrastructure.EntityConfigurations
{
    public class UserEntityConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.ClientId);
            builder.HasIndex(u => u.Email).IsUnique();
            builder.HasIndex(u => u.ClientId).IsUnique();

            builder.Property(u => u.FirstName)
                   .IsRequired()
                   .HasMaxLength(DbContextConstants.MAX_LENGTH_FOR_FIRSTNAME);

            builder.Property(u => u.LastName)
                   .IsRequired()
                   .HasMaxLength(DbContextConstants.MAX_LENGTH_FOR_LASTNAME);

            builder.Property(u => u.Email)
                   .IsRequired()
                   .HasMaxLength(DbContextConstants.MAX_LENGTH_FOR_EMAIL);

            builder.Property(u => u.HashedPassword)
                   .IsRequired();

            builder.HasOne(u => u.UserRole)
                   .WithMany()
                   .HasForeignKey(u => u.UserRoleId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
