using AutoMapper;
using Bookify.Business.Services.Interfaces;
using Bookify.DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Bookify.Business.Services
{
    public abstract class Service<T, T1> : IService<T, T1>
        where T : class
        where T1 : class
    {
        protected readonly IMapper Mapper;
        protected readonly ILogger Logger;
        protected readonly IRepository<T> Repository;

        protected Service(IMapper mapper, ILogger logger, IRepository<T> repository)
        {
            Mapper = mapper;
            Logger = logger;
            Repository = repository;
        }

        /// <summary>
        /// Adds an object
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual async Task<T1> AddAsync(T1 entity, CancellationToken cancellationToken = default)
        {
            var dataEntity = Mapper.Map<T>(entity);
            var result = await Repository.AddAsync(dataEntity ?? throw new ArgumentNullException(nameof(entity)), cancellationToken);
            return Mapper.Map<T1>(result);
        }

        /// <summary>
        /// Adds an object if it is not present
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="predicate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual async Task AddIfNotExistsAsync(T1 entity, Expression<Func<T1, bool>> predicate = null, CancellationToken cancellationToken = default)
        {
            Expression<Func<T, bool>> dataEntitiesPredicate = null;

            if (predicate != null)
                dataEntitiesPredicate = Mapper.Map<Expression<Func<T, bool>>>(predicate);

            var dataEntity = Mapper.Map<T>(entity);
            await Repository.AddIfNotExistsAsync(dataEntity, dataEntitiesPredicate, cancellationToken);
        }

        /// <summary>
        /// Adds a range of objects
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual async Task AddRangeAsync(IEnumerable<T1> entities, CancellationToken cancellationToken = default)
        {
            var dataEntities = Mapper.Map<ICollection<T>>(entities);
            await Repository.AddRangeAsync(dataEntities as IReadOnlyCollection<T>, cancellationToken); ;
        }

        /// <summary>
        /// Adds a range if it is not present
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="predicate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual async Task AddRangeIfNotExistsAsync(IEnumerable<T1> entities, Func<T, object> predicate, CancellationToken cancellationToken = default)
        {
            var dataEntities = Mapper.Map<ICollection<T>>(entities);
            await Repository.AddRangeIfNotExistsAsync(dataEntities, predicate, cancellationToken);
        }

        /// <summary>
        /// Checks if there is any object that satisfy the predicate
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual async Task<bool> AnyAsync(Expression<Func<T1, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var dataEntitiesPredicate = Mapper.Map<Expression<Func<T, bool>>>(predicate);
            return await Repository.AnyAsync(dataEntitiesPredicate, cancellationToken);
        }

        /// <summary>
        /// Counts the number of objects
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual async Task<int> CountAsync(Expression<Func<T1, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var dataEntitiesPredicate = Mapper.Map<Expression<Func<T, bool>>>(predicate);
            return await Repository.CountAsync(dataEntitiesPredicate, cancellationToken);
        }

        /// <summary>
        /// Finds objects that satisfy the predicate
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<T1>> FindByAsync(Expression<Func<T1, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var dataEntitiesPredicate = Mapper.Map<Expression<Func<T, bool>>>(predicate);
            var result = await Repository.FindByAsync(dataEntitiesPredicate, cancellationToken);
            return Mapper.Map<IReadOnlyCollection<T1>>(result);
        }

        /// <summary>
        /// Gets all
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual async Task<IReadOnlyCollection<T1>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var entities = await Repository.GetAllAsync(cancellationToken);
            var collection = Mapper.Map<IReadOnlyCollection<T1>>(entities);
            return collection;
        }

        public virtual async Task<T1> GetSingleAsync(Expression<Func<T1, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var dataEntitiesPredicate = Mapper.Map<Expression<Func<T, bool>>>(predicate);
            var result = await Repository.GetSingleAsync(dataEntitiesPredicate, cancellationToken);
            return Mapper.Map<T1>(result);
        }

        /// <summary>
        /// Removes an object
        /// </summary>
        /// <param name="entity"></param>
        public virtual void Remove(T1 entity)
        {
            var dataEntity = Mapper.Map<T>(entity);
            Repository.Remove(dataEntity ?? throw new ArgumentNullException(nameof(entity)));
        }

        /// <summary>
        /// Removes a range of objects
        /// </summary>
        /// <param name="entities"></param>
        public virtual void RemoveByRange(IEnumerable<T1> entities)
        {
            var dataEntities = Mapper.Map<ICollection<T>>(entities);
            Repository.RemoveRange(dataEntities as IReadOnlyCollection<T>);
        }

        /// <summary>
        /// Saves changes to the objects
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await Repository.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Updates an object
        /// </summary>
        /// <param name="entity"></param>
        public virtual void Update(T1 entity)
        {
            var dataEntity = Mapper.Map<T>(entity);
            Repository.Update(dataEntity ?? throw new ArgumentNullException(nameof(entity)));
        }

        /// <summary>
        /// Updates a range of objects
        /// </summary>
        /// <param name="entities"></param>
        public virtual void UpdateRange(IEnumerable<T1> entities)
        {
            var dataEntities = Mapper.Map<ICollection<T>>(entities);
            Repository.UpdateRange(dataEntities as IReadOnlyCollection<T>);
        }
    }
}
