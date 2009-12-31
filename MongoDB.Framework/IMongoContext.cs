﻿using System;
using System.Collections.Generic;
using System.Linq;

using MongoDB.Driver;
using MongoDB.Framework.Configuration;

namespace MongoDB.Framework
{
    public interface IMongoContext : IDisposable
    {
        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>The configuration.</value>
        IMongoConfiguration Configuration { get; }

        /// <summary>
        /// Gets the database.
        /// </summary>
        /// <value>The database.</value>
        Database Database { get; }

        /// <summary>
        /// Deletes all the entities on submit.
        /// </summary>
        /// <param name="entities">The entities.</param>
        void DeleteAllOnSubmit(params object[] entities);

        /// <summary>
        /// Deletes all the entities on submit.
        /// </summary>
        /// <param name="entities">The entities.</param>
        void DeleteAllOnSubmit(IEnumerable<object> entities);

        /// <summary>
        /// Deletes the entity on submit.
        /// </summary>
        /// <param name="entity">The entity.</param>
        void DeleteOnSubmit(object entity);

        /// <summary>
        /// Gets the entity specified by the id.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        TEntity GetById<TEntity>(object id);

        /// <summary>
        /// Finds one of the specified entity.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        TEntity FindOne<TEntity>(Document conditions);

        /// <summary>
        /// Finds all the entities.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <returns></returns>
        IEnumerable<TEntity> FindAll<TEntity>();

        /// <summary>
        /// Finds all the entities.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="orderBy">The order by.</param>
        /// <returns></returns>
        IEnumerable<TEntity> FindAll<TEntity>(Document orderBy);

        /// <summary>
        /// Finds all the entities.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="limit">The limit.</param>
        /// <param name="skip">The skip.</param>
        /// <returns></returns>
        IEnumerable<TEntity> FindAll<TEntity>(int limit, int skip);

        /// <summary>
        /// Finds all the entities.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="limit">The limit.</param>
        /// <param name="skip">The skip.</param>
        /// <param name="orderBy">The order by.</param>
        /// <returns></returns>
        IEnumerable<TEntity> FindAll<TEntity>(int limit, int skip, Document orderBy);

        /// <summary>
        /// Finds the specified entities.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        IEnumerable<TEntity> Find<TEntity>(Document conditions);

        /// <summary>
        /// Finds the specified entities.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        IEnumerable<TEntity> Find<TEntity>(Document conditions, Document orderBy);

        /// <summary>
        /// Finds the specified entities.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="conditions">The conditions.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="skip">The skip.</param>
        /// <returns></returns>
        IEnumerable<TEntity> Find<TEntity>(Document conditions, int limit, int skip);

        /// <summary>
        /// Finds the specified entities.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="conditions">The conditions.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="skip">The skip.</param>
        /// <param name="orderBy">The order by.</param>
        /// <returns></returns>
        IEnumerable<TEntity> Find<TEntity>(Document conditions, int limit, int skip, Document orderBy);

        /// <summary>
        /// Inserts all the entities on submit.
        /// </summary>
        /// <param name="entities">The entities.</param>
        void InsertAllOnSubmit(IEnumerable<object> entities);

        /// <summary>
        /// Inserts all the entities on submit.
        /// </summary>
        /// <param name="entities">The entities.</param>
        void InsertAllOnSubmit(params object[] entities);

        /// <summary>
        /// Inserts the entity on submit.
        /// </summary>
        /// <param name="entity">The entity.</param>
        void InsertOnSubmit(object entity);

        /// <summary>
        /// Creates a queryable for the entity.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <returns></returns>
        IQueryable<TEntity> Query<TEntity>();

        /// <summary>
        /// Submits the changes to the database.
        /// </summary>
        void SubmitChanges();
    }
}
