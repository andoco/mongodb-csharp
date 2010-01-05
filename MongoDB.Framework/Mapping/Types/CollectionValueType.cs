﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using MongoDB.Driver;

namespace MongoDB.Framework.Mapping.Types
{
    public class CollectionValueType : NullSafeValueType
    {
        private ICollectionType collectionType;
        private IValueType elementValueType;

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>The type.</value>
        public Type Type
        {
            get { return collectionType.GetCollectionType(this.elementValueType); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionValueType"/> class.
        /// </summary>
        /// <param name="collectionType">Type of the collection.</param>
        /// <param name="elementValueType">Type of the element value.</param>
        public CollectionValueType(ICollectionType collectionType, IValueType elementValueType)
            : base(collectionType.GetCollectionType(elementValueType))
        {
            if (collectionType == null)
                throw new ArgumentNullException("collectionType");
            if (elementValueType == null)
                throw new ArgumentNullException("elementValueType");

            this.collectionType = collectionType;
            this.elementValueType = elementValueType;
        }

        /// <summary>
        /// Converts from document value.
        /// </summary>
        /// <param name="documentValue">The document value.</param>
        /// <param name="mongoSession">The mongo session.</param>
        /// <returns></returns>
        public override object ConvertFromDocumentValue(object documentValue, IMongoSessionImplementor mongoSession)
        {
            documentValue = base.ConvertFromDocumentValue(documentValue, mongoSession);
            if (documentValue == null)
                return documentValue;

            return this.collectionType.ConvertFromDocumentValue(this.elementValueType, documentValue, mongoSession);
        }

        /// <summary>
        /// Converts to document value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="mongoSession">The mongo session.</param>
        /// <returns></returns>
        public override object ConvertToDocumentValue(object value, IMongoSessionImplementor mongoSession)
        {
            value = base.ConvertToDocumentValue(value, mongoSession);
            if (value == MongoDBNull.Value)
                return value;

            return this.collectionType.ConvertToDocumentValue(this.elementValueType, value, mongoSession);
        }
    }
}