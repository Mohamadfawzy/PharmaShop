using Contracts;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class EfTransaction : ITransaction
    {
        private readonly IDbContextTransaction _transaction;

        public EfTransaction(IDbContextTransaction transaction)
        {
            _transaction = transaction;
        }

        public Task CommitAsync(CancellationToken ct = default)
            => _transaction.CommitAsync(ct);

        public Task RollbackAsync(CancellationToken ct = default)
            => _transaction.RollbackAsync(ct);

        public ValueTask DisposeAsync()
            => _transaction.DisposeAsync();
    }
}
