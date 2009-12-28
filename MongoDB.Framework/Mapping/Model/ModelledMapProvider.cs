﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Framework.Reflection;
using MongoDB.Framework.Mapping.Types;
using System.Text.RegularExpressions;

namespace MongoDB.Framework.Mapping.Model
{
    public class ModelledMapProvider : IMapProvider
    {
        #region Private Fields

        private Dictionary<Type, Func<Type, Type>> elementTypeFactories = new Dictionary<Type, Func<Type, Type>>()
        {
            { typeof(ICollection<>), mt => mt.GetGenericArguments()[0] },
            { typeof(IList<>), mt => mt.GetGenericArguments()[0] },
            { typeof(List<>), mt => mt.GetGenericArguments()[0] },
            { typeof(HashSet<>), mt => mt.GetGenericArguments()[0] },
        };

        private Dictionary<Type, RootClassMapModel> rootClassMapModels;
        private Dictionary<Type, NestedClassMapModel> nestedClassMapModels;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelledMapProvider"/> class.
        /// </summary>
        public ModelledMapProvider()
        {
            this.rootClassMapModels = new Dictionary<Type, RootClassMapModel>();
            this.nestedClassMapModels = new Dictionary<Type, NestedClassMapModel>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds the root class map model.
        /// </summary>
        /// <param name="rootClassMapModel">The root class map model.</param>
        public void AddRootClassMapModel(RootClassMapModel rootClassMapModel)
        {
            if (rootClassMapModel == null)
                throw new ArgumentNullException("rootClassMapModel");

            this.rootClassMapModels.Add(rootClassMapModel.Type, rootClassMapModel);
            foreach (var subClassMapModel in rootClassMapModel.SubClassMaps)
                this.rootClassMapModels.Add(subClassMapModel.Type, rootClassMapModel);
        }

        /// <summary>
        /// Adds the nested class map model.
        /// </summary>
        /// <param name="nestedClassMapModel">The nested class map model.</param>
        public void AddNestedClassMapModel(NestedClassMapModel nestedClassMapModel)
        {
            if (nestedClassMapModel == null)
                throw new ArgumentNullException("nestedClassMapModel");

            this.nestedClassMapModels.Add(nestedClassMapModel.Type, nestedClassMapModel);
        }

        /// <summary>
        /// Gets the root class map for.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// The RootClassMap if it exists; otherwise <c>null</c>.
        /// </returns>
        public RootClassMap GetRootClassMapFor(Type type)
        {
            RootClassMapModel rootClassMapModel;
            if (!this.rootClassMapModels.TryGetValue(type, out rootClassMapModel))
                return null;

            return this.BuildRootClassMap(rootClassMapModel);
        }

        #endregion

        #region Private Methods

        private RootClassMap BuildRootClassMap(RootClassMapModel model)
        {
            var memberMaps = new List<MemberMap>();
            memberMaps.AddRange(model.MemberMaps.Select(mm => this.BuildMemberMap(mm)));
            var extPropMap = this.BuildExtendedPropertiesMap(model.ExtendedPropertiesMap);
            var idMap = this.BuildIdMap(model.IdMap);
            string collectionName = model.CollectionName ?? model.Type.Name;

            var subClassMaps = model.SubClassMaps
                .Select(sc => this.BuildSubClassMap(
                    sc,
                    idMap,
                    collectionName,
                    model.DiscriminatorKey,
                    memberMaps,
                    extPropMap));

            var rootClassMap = new RootClassMap(
                model.Type,
                collectionName,
                idMap,
                memberMaps,
                model.DiscriminatorKey,
                model.Discriminator,
                subClassMaps,
                extPropMap);

            return rootClassMap;
        }

        private NestedClassMap BuildNestedClassMap(NestedClassMapModel model)
        {
            var memberMaps = new List<MemberMap>();
            memberMaps.AddRange(model.MemberMaps.Select(mm => this.BuildMemberMap(mm)));
            var extPropMap = this.BuildExtendedPropertiesMap(model.ExtendedPropertiesMap);

            var subClassMaps = model.SubClassMaps
                .Select(sc => this.BuildSubClassMap(
                    sc,
                    null,
                    null,
                    model.DiscriminatorKey,
                    memberMaps,
                    extPropMap));

            var nestedClassMap = new NestedClassMap(
                model.Type,
                memberMaps,
                model.DiscriminatorKey,
                model.Discriminator,
                subClassMaps,
                extPropMap);

            return nestedClassMap;
        }

        private ExtendedPropertiesMap BuildExtendedPropertiesMap(MemberMapModel model)
        {
            if (model == null)
                return null;

            var getter = LateBoundReflection.GetGetter(model.Getter);
            var setter = LateBoundReflection.GetSetter(model.Setter);
            return new ExtendedPropertiesMap(model.Getter.Name,
                getter,
                setter);
        }

        private MemberMap BuildIdMap(MemberMapModel model)
        {
            var getter = LateBoundReflection.GetGetter(model.Getter);
            var setter = LateBoundReflection.GetSetter(model.Setter);
            return new MemberMap("_id", model.Getter.Name, getter, setter, new IdValueType());
        }

        private MemberMap BuildMemberMap(KeyMemberMapModel model)
        {
            var getter = LateBoundReflection.GetGetter(model.Getter);
            var setter = LateBoundReflection.GetSetter(model.Setter);
            string name = model.Getter.Name;
            string key = model.Key ?? name;

            if(model is KeyValueMemberMapModel)
            {
                var kvModel = (KeyValueMemberMapModel)model;
                var valueType = kvModel.CustomValueType ?? this.GetValueTypeFromType(LateBoundReflection.GetMemberValueType(model.Getter));
                return new MemberMap(
                    key,
                    name,
                    getter,
                    setter,
                    valueType);
            }
            else if (model is HasManyMemberMapModel)
            {
                var hmModel = (HasManyMemberMapModel)model;
                var valueType = this.GetCollectionValueType(LateBoundReflection.GetMemberValueType(model.Getter), hmModel.CollectionType, hmModel.ElementType, hmModel.ElementValueType);

                return new MemberMap(
                    key,
                    name,
                    getter,
                    setter,
                    valueType);
            }

            throw new NotSupportedException();
        }

        private SubClassMap BuildSubClassMap(SubClassMapModel model, MemberMap idMap, string collectionName, string discriminatorKey, IEnumerable<MemberMap> superClassMemberMaps, ExtendedPropertiesMap extendedPropertiesMap)
        {
            var subClassMemberMaps = model.MemberMaps.Select(mm => this.BuildMemberMap(mm));
            return new SubClassMap(
                model.Type,
                collectionName,
                idMap,
                superClassMemberMaps.Concat(subClassMemberMaps),
                discriminatorKey,
                model.Discriminator,
                extendedPropertiesMap);
        }

        private IValueType GetValueTypeFromType(Type type)
        {
            NestedClassMapModel nestedClassMapModel;
            if (this.nestedClassMapModels.TryGetValue(type, out nestedClassMapModel))
            {
                return new NestedClassValueType(
                    this.BuildNestedClassMap(nestedClassMapModel));
            }

            if (type == typeof(Regex))
            {
                //TODO: create a regex value type
            }

            return new NullSafeValueType(type);
        }

        private IValueType GetCollectionValueType(Type memberType, ICollectionType collectionType, Type elementType, IValueType elementValueType)
        {
            if (collectionType == null)
            {
                if (elementValueType == null)
                {
                    if (elementType == null)
                        elementType = this.DiscoverElementType(memberType);
                    elementValueType = this.GetValueTypeFromType(elementType);
                }
                collectionType = this.GetCollectionType(memberType, elementValueType);
            }

            return new CollectionValueType(collectionType);
        }

        private Type DiscoverElementType(Type memberType)
        {
            Type elementType;
            if (memberType.IsGenericType)
            {
                Func<Type, Type> elementTypeFactory;
                if(this.elementTypeFactories.TryGetValue(memberType.GetGenericTypeDefinition(), out elementTypeFactory))
                    return elementTypeFactory(memberType);
            }

            throw new NotSupportedException(string.Format("Could not discover element type from {0}.", memberType));
        }

        private ICollectionType GetCollectionType(Type memberType, IValueType elementValueType)
        {
            if (memberType.IsGenericType)
            {
                var genType = memberType.GetGenericTypeDefinition();
                if (genType == typeof(IList<>) || genType == typeof(List<>) || genType == typeof(ICollection<>))
                    return new ListCollectionType(elementValueType);
                if (genType == typeof(HashSet<>))
                    return new SetCollectionType(elementValueType);
            }

            throw new NotSupportedException(string.Format("Could not create collection type from {0}.", memberType));
        }


        #endregion
    }
}