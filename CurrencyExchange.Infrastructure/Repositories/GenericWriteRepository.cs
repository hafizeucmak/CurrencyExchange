using CurrencyExchange.Domain.Entites;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyExchange.Infrastructure.Repositories
{
    public class GenericWriteRepository<TContext> : IGenericWriteRepository<TContext> where TContext :  Microsoft.EntityFrameworkCore.DbContext
    {
        protected readonly TContext _context;

        public GenericWriteRepository(TContext context)
        {
            _context = context;
        }

        public virtual IQueryable<TEntity> GetAll<TEntity>()
             where TEntity : DomainEntity
        {
            return _context.Set<TEntity>();
        }

        public virtual async Task<TEntity> GetByIdAsync<TEntity>(int id, CancellationToken cancellationToken)
            where TEntity : DomainEntity
        {
            TEntity? entity = await GetAll<TEntity>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (entity == null)
            {
                throw new ArgumentNullException($"Entity of type {typeof(TEntity).Name} with ID {id} not found.");
            }
            return entity;
        }

     
        public virtual async Task<TEntity> AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken, bool saveChanges = false)
                where TEntity : DomainEntity
        {
            await _context.Set<TEntity>().AddAsync(entity, cancellationToken);

            if (saveChanges)
            {
                await _context.SaveChangesAsync(cancellationToken);
            }

            return entity;
        }

        public virtual async Task<int> RemoveAsync<TEntity>(TEntity entity, CancellationToken cancellationToken, bool saveChanges = false)
            where TEntity : DomainEntity
        {
            int resultCount = 0;
            _context.Entry(entity).State = EntityState.Deleted;
            if (saveChanges)
            {
                resultCount = await _context.SaveChangesAsync(cancellationToken);
            }

            return resultCount;
        }

        public virtual async Task<int> UpdateAsync<TEntity>(TEntity entity, CancellationToken cancellationToken, bool saveChanges = false)
            where TEntity : DomainEntity
        {
            int resultCount = 0;
            entity.Update();
            _context.Entry(entity).State = EntityState.Modified;
            _context.ChangeTracker.DetectChanges();

            if (saveChanges)
            {
                resultCount = await _context.SaveChangesAsync(cancellationToken);
            }

            return resultCount;
        }

        public void BeginTransaction()
        {
            _context.Database.BeginTransaction();
        }

        public void CommitTransaction()
        {
            if (_context.Database.CurrentTransaction != null)
            {
                _context.SaveChanges();
                _context.Database.CurrentTransaction.Commit();
            }
        }

        public void RollbackTransaction()
        {
            _context.Database.CurrentTransaction?.Rollback();
        }
    }
}
