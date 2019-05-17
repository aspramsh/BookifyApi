using Bookify.DataAccess.DbContexts.Interfaces;
using Bookify.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Bookify.DataAccess.Repositories
{
    public abstract class Repository<T> : IRepository<T>
        where T : class
    {
        protected readonly IDbContext DbContext;
        protected readonly DbSet<T> DbSet;
        protected readonly ILogger Logger;

        protected Repository(IDbContext context, ILogger logger)
        {
            DbContext = context ?? throw new ArgumentNullException(nameof(context));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            DbSet = DbContext.Set<T>();
            Logger.LogInformation($"{nameof(T)}");

        }

        /// <summary>
        /// Get Single Entity based on predicate
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public async Task<T> GetSingleAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await DbContext.Set<T>().FirstOrDefaultAsync(predicate, cancellationToken);
        }

        /// <summary>
        /// Adds an entity
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
            {
                Logger.LogCritical($"{nameof(AddAsync)} - {nameof(entity)} is null");
                throw new ArgumentNullException(nameof(entity));
            }

            return (await DbSet.AddAsync(entity, cancellationToken)).Entity;
        }

        /// <summary>
        /// Adds an entity if it is not present
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="predicate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task AddIfNotExistsAsync(T entity, Expression<Func<T, bool>> predicate = null, CancellationToken cancellationToken = default)
        {
            var exists = DbSet.Any(predicate ?? throw new ArgumentNullException(nameof(predicate)));

            if (!exists)
            {
                await DbSet.AddAsync(entity, cancellationToken);
            }
        }


        /// <summary>
        /// Adds a range of entities
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            if (entities == null)
            {
                Logger.LogCritical($"{nameof(AddRangeAsync)} - The collection {nameof(entities)} is null");
                throw new ArgumentNullException(nameof(entities));
            }

            await DbSet.AddRangeAsync(entities, cancellationToken);
        }

        /// <summary>
        /// Adds entities if they are not present
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="predicate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task AddRangeIfNotExistsAsync(IEnumerable<T> entities, Func<T, object> predicate, CancellationToken cancellationToken = default)
        {
            var entitiesExist = from ent in DbSet
                                where entities.Any(add => predicate(ent).Equals(predicate(add)))
                                select ent;

            await DbSet.AddRangeAsync(entities.Except(entitiesExist), cancellationToken);
        }

        /// <summary>
        /// True if there are entities that satisfy the predicate
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await DbSet
                .AsNoTracking()
                .AnyAsync(predicate, cancellationToken);
        }

        /// <summary>
        /// The number of entites for the predicate
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return predicate != null
                ? await DbSet.CountAsync(predicate, cancellationToken)
                : await DbSet.CountAsync(cancellationToken);
        }

        /// <summary>
        /// Finds entities that satisfy a predicate
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ICollection<T>> FindByAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            ICollection<T> result = null;

            try
            {
                return result = await DbSet
                    .AsNoTracking()
                    .Where(predicate ?? throw new ArgumentNullException(nameof(predicate)))
                    .ToListAsync(cancellationToken);
            }
            finally
            {
                Logger.LogInformation($"Result of {nameof(FindByAsync)}", result);
            }
        }

        /// <summary>
        /// Gets all columns from the table
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ICollection<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            ICollection<T> result = null;

            try
            {
                return result = await DbSet
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
            }
            finally
            {
                Logger.LogInformation($"Result of {nameof(GetAllAsync)}", result);
            }
        }

        /// <summary>
        /// Removes an entity
        /// </summary>
        /// <param name="entity"></param>
        public void Remove(T entity)
        {
            if (entity == null)
            {
                Logger.LogCritical($"{nameof(Remove)} - The {nameof(entity)} is null");
                throw new ArgumentNullException(nameof(entity));
            }

            DbSet.Remove(entity);
        }

        /// <summary>
        /// Removes a range of entites
        /// </summary>
        public void RemoveRange(IEnumerable<T> entities)
        {
            if (entities == null)
            {
                Logger.LogCritical($"{nameof(RemoveRange)} - The {nameof(entities)} collection is null");
                throw new ArgumentNullException(nameof(entities));
            }

            var romoveRange = entities as T[] ?? entities.ToArray();
            DbSet.RemoveRange(romoveRange);
        }

        /// <summary>
        /// Saves changes for a context
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var id = await DbContext.SaveChangesAsync(cancellationToken);

            DbContext.Database.CloseConnection();

            return id > 0;
        }

        /// <summary>
        /// Updates an entity
        /// </summary>
        /// <param name="entity"></param>
        public void Update(T entity)
        {
            if (entity == null)
            {
                Logger.LogCritical($"{nameof(Update)} - The {nameof(entity)} is null");
                throw new ArgumentNullException(nameof(entity));
            }

            DbSet.Update(entity);
        }

        /// <summary>
        /// Updates a range of entities
        /// </summary>
        /// <param name="entities"></param>
        public void UpdateRange(IEnumerable<T> entities)
        {
            if (entities == null)
            {
                Logger.LogCritical($"{nameof(UpdateRange)} - The {nameof(entities)} collection is null");
                throw new ArgumentNullException(nameof(entities));
            }

            var updateRange = entities as T[] ?? entities.ToArray();
            DbSet.UpdateRange(updateRange);
        }
    }
}
