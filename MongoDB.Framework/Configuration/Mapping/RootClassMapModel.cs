﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Framework.Configuration.Mapping
{
    public class RootClassMapModel : SuperClassMapModel
    {
        public string CollectionName { get; set; }

        public List<IndexModel> Indexes { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RootClassMapModel"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public RootClassMapModel(Type type)
            : base(type)
        {
            this.Indexes = new List<IndexModel>();
        }
    }
}