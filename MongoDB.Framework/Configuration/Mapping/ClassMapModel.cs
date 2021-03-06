﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Framework.Configuration.Mapping
{
    public abstract class ClassMapModel : ModelNode
    {
        /// <summary>
        /// Gets or sets the conventions.
        /// </summary>
        /// <value>The conventions.</value>
        public MappingConventions Conventions { get; set; }

        /// <summary>
        /// Gets or sets the discriminator.
        /// </summary>
        /// <value>The discriminator.</value>
        public object Discriminator { get; set; }

        /// <summary>
        /// Gets or sets the persistent member maps.
        /// </summary>
        /// <value>The persistent member maps.</value>
        public List<PersistentMemberMapModel> PersistentMemberMaps { get; private set; }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>The type.</value>
        public Type Type { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassMapModel"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public ClassMapModel(Type type)
        {
            this.Conventions = new MappingConventions();
            this.PersistentMemberMaps = new List<PersistentMemberMapModel>();
            this.Type = type;
        }
    }
}