﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MongoDB.Driver;
using MongoDB.Framework.Mapping;
using MongoDB.Framework.Tracking;

namespace MongoDB.Framework.Persistence
{
    public abstract class FindActionBase : PersistenceAction
    {
        #region Private Static Methods

        /// <summary>
        /// Determines whether the query is an identity query.
        /// </summary>
        /// <param name="classMap">The class map.</param>
        /// <param name="conditions">The conditions.</param>
        /// <returns>
        /// 	<c>true</c> if [is find by id] [the specified class map]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsFindById(ClassMap classMap, Document conditions)
        {
            return classMap.HasId && conditions.Count == 1 && conditions[classMap.IdMap.Key] != null;
        }

        /// <summary>
        /// Determines whether the query can be called without a cursor.
        /// </summary>
        /// <param name="classMap">The class map.</param>
        /// <param name="conditions">The conditions.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="skip">The skip.</param>
        /// <param name="orderBy">The order by.</param>
        /// <returns>
        /// 	<c>true</c> if [is find one] [the specified class map]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsFindOne(int limit, int skip, Document orderBy)
        {
            return orderBy.Count == 0 && limit == 1 && skip == 0;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FindAction"/> class.
        /// </summary>
        /// <param name="mappingStore">The mapping store.</param>
        /// <param name="changeTracker">The change tracker.</param>
        /// <param name="collection">The collection.</param>
        public FindActionBase(MappingStore mappingStore, ChangeTracker changeTracker, IMongoCollection collection)
            : base(mappingStore, changeTracker, collection)
        {  }

        #endregion

        #region Protected Methods

        protected IEnumerable<object> Find(ClassMap classMap, Document conditions, int limit, int skip, Document orderBy, Document fields)
        {
            if (classMap == null)
                throw new ArgumentNullException("classMap");
            if (conditions == null)
                throw new ArgumentNullException("conditions");
            if (orderBy == null)
                throw new ArgumentNullException("orderBy");
            if (fields == null)
                throw new ArgumentNullException("fields");

            var query = this.CreateQuery(conditions, orderBy);
            conditions = (Document)query["query"] ?? query;

            IEnumerable<Document> documents;
            if (IsFindById(classMap, conditions))
            {
                string id = (string)classMap.IdMap.ValueType.ConvertFromDocumentValue(conditions[classMap.IdMap.Key], null);
                TrackedObject trackedObject = null;
                if (this.ChangeTracker.TryGetTrackedObjectById(id, out trackedObject))
                    return new[] { trackedObject.Current };

                documents = new[] { this.Collection.FindOne(conditions) };
            }
            else if (IsFindOne(limit, skip, orderBy))
            {
                //if the particular type we need has a discriminator, we need to filter on it...
                if (classMap.IsPolymorphic && classMap.Discriminator != null)
                    conditions[classMap.DiscriminatorKey] = classMap.Discriminator;
                var document = this.Collection.FindOne(conditions);
                if (document == null)
                    documents = Enumerable.Empty<Document>();
                else
                    documents = new[] {  document };
            }
            else
            {
                if (classMap.IsPolymorphic)
                {
                    //if we are projecting, we need to make sure we get the discriminator back as well...
                    if (fields.Count != 0)
                        fields[classMap.DiscriminatorKey] = 1;

                    //if the particular type we need has a discriminator, we need to filter on it...
                    if (classMap.Discriminator != null)
                        conditions[classMap.DiscriminatorKey] = classMap.Discriminator;
                }

                documents = this.Collection.Find(query, limit, skip, fields).Documents;
            }

            throw new NotImplementedException();

            //don't track entities returned from a projection
            //DocumentToEntityTranslator translator;
            //if (fields.Count == 0)
            //    translator = new ChangeTrackingDocumentToEntityTranslator(this.MappingStore, this.ChangeTracker);
            //else
            //    translator = new DocumentToEntityTranslator(this.MappingStore);
            //return this.CreateEntities(translator, classMap, documents);
        }

        #endregion

        #region Private Methods


        /// <summary>
        /// Creates the full query.
        /// </summary>
        /// <returns></returns>
        private Document CreateQuery(Document conditions, Document orderBy)
        {
            if (orderBy.Count == 0)
                return conditions;

            return new Document()
                .Append("query", conditions)
                .Append("orderby", orderBy);
        }

        #endregion
    }
}