using CurrencyExchange.Domain.Entites;

namespace CurrencyExchange.Infrastructure.Repositories
{
    public interface IGenericReadRepository<TContext> where TContext : Microsoft.EntityFrameworkCore.DbContext
    {
        IQueryable<TEntity> GetAll<TEntity>()
            where TEntity : DomainEntity;

        Task<TEntity> GetByIdThrowsAsync<TEntity>(int id, CancellationToken cancellationToken)
            where TEntity : DomainEntity;
    }
}
