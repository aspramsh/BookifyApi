using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Threading;
using System.Threading.Tasks;

namespace Bookify.DataAccess.DbContexts.Interfaces
{
    public interface IDbContext
    {
        DbSet<T> Set<T>() where T : class;

        EntityEntry<T> Entry<T>(T entity) where T : class;

        DatabaseFacade Database { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        string FullName();
    }
}
