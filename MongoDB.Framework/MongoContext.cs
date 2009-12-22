﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MongoDB.Driver;
using MongoDB.Framework.Configuration;
using MongoDB.Framework.Linq;
using MongoDB.Framework.Mapping;
using MongoDB.Framework.Persistence;
using MongoDB.Framework.Tracking;

namespace MongoDB.Framework
{
    public class MongoContext : IDisposable
    {
        #region Private Fields

        private ChangeTracker changeTracker;
        private Database database;
        private MappingStore mappingStore;
        private Mongo mongo;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the database.
        /// </summary>
        /// <value>The database.</value>
        public Database Database
        {
            get
            {
                this.EnsureNotDisposed();

                return this.database;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoContext"/> class.
        /// </summary>
        /// <param name="mappingStore">The mapping store.</param>
        /// <param name="changeTracker">The change tracker.</param>
        /// <param name="mongo">The mongo.</param>
        /// <param name="database">The database.</param>
        public MongoContext(MappingStore mappingStore, ChangeTracker changeTracker, Mongo mongo, Database database)
        {
            if (mappingStore == null)
                throw new ArgumentNullException("mappingStore");
            if (changeTracker == null)
                throw new ArgumentNullException("changeTracker");
            if (mongo == null)
                throw new ArgumentNullException("mongo");
            if (database == null)
                throw new ArgumentNullException("database");

            this.changeTracker = changeTracker;
            this.database = database;
            this.mappingStore = mappingStore;
            this.mongo = mongo;
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="MongoContext"/> is reclaimed by garbage collection.
        /// </summary>
        ~MongoContext()
        {
            this.Dispose(false);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Deletes the on submit.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        public void DeleteOnSubmit(object entity)
        {
            this.EnsureNotDisposed();

            this.DeleteAllOnSubmit(new[] { entity });
        }

        /// <summary>
        /// Deletes all on submit.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entities">The entities.</param>
        public void DeleteAllOnSubmit(params object[] entities)
        {
            this.EnsureNotDisposed();

            this.DeleteAllOnSubmit((IEnumerable<object>)entities);
        }

        /// <summary>
        /// Deletes all on submit.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entities">The entities.</param>
        public void DeleteAllOnSubmit(IEnumerable<object> entities)
        {
            this.EnsureNotDisposed();

            if (entities == null)
                throw new ArgumentNullException("entities");

            foreach (var entity in entities)
                this.changeTracker.GetTrackedObject(entity).MoveToDeleted();
        }

        /// <summary>
        /// Inserts the entity.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        public void InsertOnSubmit(object entity)
        {
            this.EnsureNotDisposed();

            this.InsertAllOnSubmit(new[] { entity });
        }

        /// <summary>
        /// Inserts all the entities.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entities">The entities.</param>
        public void InsertAllOnSubmit(params object[] entities)
        {
            this.EnsureNotDisposed();

            this.InsertAllOnSubmit((IEnumerable<object>)entities);
        }

        /// <summary>
        /// Inserts all the entities.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entities">The entities.</param>
        public void InsertAllOnSubmit(IEnumerable<object> entities)
        {
            this.EnsureNotDisposed();

            if (entities == null)
                throw new ArgumentNullException("entities");

            foreach (var entity in entities)
                this.changeTracker.Track(null, entity).MoveToInserted();
        }

        /// <summary>
        /// Queries this instance.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <returns></returns>
        public IQueryable<TEntity> Query<TEntity>()
        {
            this.EnsureNotDisposed();

            return new MongoQueryable<TEntity>(this.mappingStore, this.changeTracker, this.database);
        }

        /// <summary>
        /// Submits the changes.
        /// </summary>
        public void SubmitChanges()
        {
            this.EnsureNotDisposed();

            ChangeSet changeSet = this.changeTracker.GetChangeSet();
            this.PerformInserts(changeSet.Inserted);
            this.PerformUpdates(changeSet.Modified);
            this.PerformDeletes(changeSet.Deleted);
        }

        #endregion

        #region Protected Members

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            this.changeTracker = null;
            this.database = null;
            this.mappingStore = null;
            this.mongo.Disconnect();
            this.mongo = null;
        }

        /// <summary>
        /// Ensures the not disposed.
        /// </summary>
        protected void EnsureNotDisposed()
        {
            if (this.mongo == null)
                throw new ObjectDisposedException("MongoContext");
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Performs the adds.
        /// </summary>
        /// <param name="added">The added.</param>
        private void PerformInserts(IList<object> inserted)
        {
            foreach (var entityGroup in inserted.GroupBy(a => a.GetType()))
            {
                var documentMap = this.mappingStore.GetDocumentMapFor(entityGroup.Key);
                var collection = this.Database.GetCollection(documentMap.CollectionName);
                foreach (var entity in entityGroup)
                {
                    new InsertAction(this.mappingStore, this.changeTracker, collection)
                        .Insert(entity);
                }
            }
        }

        /// <summary>
        /// Performs the updates.
        /// </summary>
        /// <param name="updated">The updated.</param>
        private void PerformUpdates(IList<object> updated)
        {
            foreach (var entityGroup in updated.GroupBy(a => a.GetType()))
            {
                var documentMap = this.mappingStore.GetDocumentMapFor(entityGroup.Key);
                var collection = this.Database.GetCollection(documentMap.CollectionName);
                foreach (var entity in entityGroup)
                {
                    new UpdateAction(this.mappingStore, this.changeTracker, collection)
                        .Update(entity);
                }
            }
        }

        /// <summary>
        /// Performs the removes.
        /// </summary>
        /// <param name="removed">The removed.</param>
        private void PerformDeletes(IList<object> deleted)
        {
            foreach (var entityGroup in deleted.GroupBy(a => a.GetType()))
            {
                var documentMap = this.mappingStore.GetDocumentMapFor(entityGroup.Key);
                var collection = this.Database.GetCollection(documentMap.CollectionName);
                foreach (var entity in entityGroup)
                {
                    new DeleteAction(this.mappingStore, this.changeTracker, collection)
                        .Delete(entity);
                }
            }
        }

        #endregion
    }
}