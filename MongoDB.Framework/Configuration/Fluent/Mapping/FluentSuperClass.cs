﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

using MongoDB.Framework.Linq.Visitors;
using MongoDB.Framework.Reflection;
using MongoDB.Framework.Configuration.Mapping;

namespace MongoDB.Framework.Configuration.Fluent.Mapping
{
    public abstract class FluentSuperClass<TModel, TClass> : FluentClass<TModel, TClass> where TModel : SuperClassMapModel
    {
        public string DiscrimatorKey
        {
            get { return this.Model.DiscriminatorKey; }
            set { this.Model.DiscriminatorKey = value; }
        }

        public FluentSuperClass(TModel model)
            : base(model)
        { }

        public void ExtendedProperties(string memberName)
        {
            var memberInfo = ReflectionUtil.GetSingleMember<TClass>(memberName);
            this.ExtendedProperties(memberInfo);
        }

        public void ExtendedProperties(MemberInfo memberInfo)
        {
            this.Model.ExtendedPropertiesMap = new ExtendedPropertiesMapModel()
            {
                Getter = memberInfo,
                Setter = memberInfo
            };
        }

        public void ExtendedProperties(Expression<Func<TClass, IDictionary<string, object>>> member)
        {
            var memberInfo = ReflectionUtil.GetSingleMember(member);
            this.ExtendedProperties(memberInfo);
        }

        public FluentId Id(string memberName)
        {
            var memberInfo = ReflectionUtil.GetSingleMember<TClass>(memberName);
            return this.Id(memberInfo);
        }

        public FluentId Id(MemberInfo memberInfo)
        {
            var fluentIdMap = new FluentId();
            fluentIdMap.Model.Getter = memberInfo;
            fluentIdMap.Model.Setter = memberInfo;
            this.Model.IdMap = fluentIdMap.Model;
            return fluentIdMap;
        }

        public FluentId Id(Expression<Func<TClass, object>> idMember)
        {
            var memberInfo = ReflectionUtil.GetSingleMember(idMember);
            return this.Id(memberInfo);
        }

        public void UseActivator<T>()
        {
            this.UseActivator(typeof(T));
        }

        public void UseActivator(Type activatorType)
        {
            this.Model.ClassActivatorType = activatorType;
        }
    }
}