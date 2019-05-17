using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Bookify.Business.Services.Interfaces
{
    public interface IService<out T, T1>
        where T : class
        where T1 : class
    {
        /// <summary>
        /// Adds an object
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<T1> AddAsync(T1 entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a range of objects
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task AddRangeAsync(IEnumerable<T1> entities, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds an object if it is not present
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="predicate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task AddIfNotExistsAsync(T1 entity, Expression<Func<T1, bool>> predicate = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a range if it is not present
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="predicate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task AddRangeIfNotExistsAsync(IEnumerable<T1> entities, Func<T, object> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an object
        /// </summary>
        /// <param name="entity"></param>
        void Update(T1 entity);

        /// <summary>
        /// Updates a range of objects
        /// </summary>
        /// <param name="entities"></param>
        void UpdateRange(IEnumerable<T1> entities);

        /// <summary>
        /// Removes an object
        /// </summary>
        /// <param name="entity"></param>
        void Remove(T1 entity);

        /// <summary>
        /// Removes a range of objects
        /// </summary>
        /// <param name="entities"></param>
        void RemoveByRange(IEnumerable<T1> entities);

        /// <summary>
        /// Gets all
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IReadOnlyCollection<T1>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<T1> GetSingleAsync(Expression<Func<T1, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds objects that satisfy the predicate
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IEnumerable<T1>> FindByAsync(Expression<Func<T1, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if there is any object that satisfy the predicate
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> AnyAsync(Expression<Func<T1, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Counts the number of objects
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<int> CountAsync(Expression<Func<T1, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves changes to the objects
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
