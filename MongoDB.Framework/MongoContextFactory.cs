﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Framework.Configuration;
using MongoDB.Driver;
using MongoDB.Framework.Tracking;

namespace MongoDB.Framework
{
    public class MongoContextFactory
    {
        #region Private Fields

        private bool initialized;
        private object initializeObject = new object();

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>The configuration.</value>
        public MongoConfiguration Configuration { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoContextFactory"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public MongoContextFactory(MongoConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            this.Configuration = configuration;
            this.initialized = false;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Opens the context.
        /// </summary>
        /// <returns></returns>
        public MongoContext OpenContext()
        {
            var mongo = new Mongo();
            mongo.Connect();
            var database = mongo.getDB(this.Configuration.DatabaseName);

            if(!this.initialized)
            {
                lock(this.initializeObject)
                {
                    if(!this.initialized)
                    {
                        this.Initialize(database);
                        this.initialized = true;
                    }
                }
            }
            var entityMapper = new EntityMapper(this.Configuration);
            return new MongoContext(entityMapper, new DefaultChangeTracker(entityMapper), mongo, database);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Initializes the specified database.
        /// </summary>
        /// <param name="database">The database.</param>
        private void Initialize(Database database)
        {
            this.EnsureIndexes(database);
        }

        /// <summary>
        /// Ensures that the specified database has the appropriate indexes.
        /// </summary>
        /// <param name="database">The database.</param>
        private void EnsureIndexes(Database database)
        {
            foreach (var rootEntityMap in this.Configuration.RootEntityMaps)
            {
                IMongoCollection collection = database.GetCollection(rootEntityMap.CollectionName);

                foreach (var index in rootEntityMap.Indexes)
                {
                    Document fieldsAndDirections = new Document();
                    foreach (var pair in index.DocumentKeys)
                        fieldsAndDirections.Add(pair.Key, pair.Value == IndexDirection.Ascending ? 1 : -1);

                    collection.MetaData.CreateIndex(fieldsAndDirections, index.IsUnique);
                }
            }
        }

        #endregion
    }
}