using CurrencyExchange.Domain.Entites;

namespace CurrencyExchange.Infrastructure.Repositories
{
    public interface IGenericWriteRepository<TContext> where TContext : Microsoft.EntityFrameworkCore.DbContext
    {
        IQueryable<TEntity> GetAll<TEntity>()
            where TEntity : DomainEntity;

        Task<TEntity> GetByIdAsync<TEntity>(int id, CancellationToken cancellationToken)
            where TEntity : DomainEntity;

        Task<TEntity> AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken, bool saveChanges = false)
            where TEntity : DomainEntity;

        Task<int> RemoveAsync<TEntity>(TEntity entity, CancellationToken cancellationToken, bool saveChanges = false)
            where TEntity : DomainEntity;

        Task<int> UpdateAsync<TEntity>(TEntity entity, CancellationToken cancellationToken, bool saveChanges = false)
            where TEntity : DomainEntity;

        void BeginTransaction();

        void CommitTransaction();

        void RollbackTransaction();
    }
}
