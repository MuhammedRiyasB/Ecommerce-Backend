using Ecommerce.Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace Ecommerce.Infrastructure.Data
{
    /// <summary>
    /// SRP fix: UnitOfWork extracted to its own file (was previously combined with Repository).
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(AppDbContext context) => _context = context;

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => await _context.SaveChangesAsync(cancellationToken);

        public async Task BeginTransactionAsync()
            => _transaction = await _context.Database.BeginTransactionAsync();

        public async Task CommitAsync()
        {
            if (_transaction != null) await _transaction.CommitAsync();
        }

        public async Task RollbackAsync()
        {
            if (_transaction != null) await _transaction.RollbackAsync();
        }
    }
}
