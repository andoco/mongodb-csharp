﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

using MongoDB.Framework.Linq.Visitors;
using MongoDB.Framework.Mapping.Models;
using MongoDB.Framework.Reflection;

namespace MongoDB.Framework.Mapping.Fluent
{
    public class FluentSubClass<TSubClass> : FluentClass<SubClassMapModel, TSubClass>
    {
        public FluentSubClass()
            : base(new SubClassMapModel(typeof(TSubClass)))
        { }
    }
}