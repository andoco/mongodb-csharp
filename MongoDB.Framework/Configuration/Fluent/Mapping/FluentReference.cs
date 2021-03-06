﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Framework.Configuration.Mapping;

namespace MongoDB.Framework.Configuration.Fluent.Mapping
{
    public class FluentReference : FluentBase<ManyToOneMapModel>
    {
        public FluentReference()
            : base(new ManyToOneMapModel())
        { }

        public FluentReference Key(string key)
        {
            this.Model.Key = key;
            return this;
        }

        public FluentReference NotLazy()
        {
            this.Model.IsLazy = false;
            return this;
        }
    }
}