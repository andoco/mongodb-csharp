﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MongoDB.Driver;
using MongoDB.Framework.Mapping;
using MongoDB.Framework.Tracking;

namespace MongoDB.Framework.Persistence
{
    public abstract class PersistenceAction
    {
        protected IMongoContextCache MongoContextCache { get; private set; }
        protected IMongoContext MongoContext{ get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistenceAction"/> class.
        /// </summary>
        /// <param name="mongoContext">The mongo context.</param>
        /// <param name="mongoContextCache">The mongo context cache.</param>
        public PersistenceAction(IMongoContext mongoContext, IMongoContextCache mongoContextCache)
        {
            if (mongoContext == null)
                throw new ArgumentNullException("mongoContext");
            if (mongoContextCache == null)
                throw new ArgumentNullException("mongoContextCache");

            this.MongoContextCache = mongoContextCache;
            this.MongoContext = mongoContext;
        }

        protected IMongoCollection GetCollectionForClassMap(ClassMap classMap)
        {
            var db = this.MongoContext.Database;
            return db.GetCollection(classMap.CollectionName);
        }
    }
}