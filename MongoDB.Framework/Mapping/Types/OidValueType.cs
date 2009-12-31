﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;

namespace MongoDB.Framework.Mapping.Types
{
    public class OidValueType : NullSafeValueType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IdValueType"/> class.
        /// </summary>
        public OidValueType()
            : base(typeof(string))
        { }

        /// <summary>
        /// Converts from document value.
        /// </summary>
        /// <param name="documentValue">The document value.</param>
        /// <returns></returns>
        public override object ConvertFromDocumentValue(object documentValue, IMappingContext mappingContext)
        {
            documentValue = base.ConvertFromDocumentValue(documentValue, mappingContext);
            var oid = documentValue as Oid;
            if (oid == null)
                return null;

            return BitConverter.ToString(oid.Value).Replace("-", "").ToLower();
        }

        /// <summary>
        /// Converts to document value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="mappingContext">The mapping context.</param>
        /// <returns></returns>
        public override object ConvertToDocumentValue(object value, IMappingContext mappingContext)
        {
            value = base.ConvertToDocumentValue(value, mappingContext);
            if (value == MongoDBNull.Value)
                return value;

            return new Oid((string)value);
        }
    }
}