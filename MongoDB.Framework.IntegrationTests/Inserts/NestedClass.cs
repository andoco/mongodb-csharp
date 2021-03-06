﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MongoDB.Driver;
using MongoDB.Framework.Configuration.Fluent.Mapping;
using MongoDB.Framework.Configuration.Mapping;
using MongoDB.Framework.Mapping;

using NUnit.Framework;

namespace MongoDB.Framework.Inserts
{
    [TestFixture]
    public class NestedClass : TestCase
    {
        protected override IMapModelRegistry MapModelRegistry
        {
            get
            {
                return new FluentMapModelRegistry()
                    .AddMap(new EntityMap())
                    .AddMap(new SubEntityMap());
            }
        }

        protected override void AfterTest()
        {
            using (var mongoSession = this.OpenMongoSession())
            {
                mongoSession.Database.MetaData.DropCollection("Entity");
            }
        }

        [Test]
        public void Should_insert()
        {
            var entity = new Entity();
            entity.SubEntity = new SubEntity()
            {
                Integer = 42,
                Double = 123.456
            };
            using (var mongoSession = this.OpenMongoSession())
            {
                mongoSession.InsertOnSubmit(entity);
                mongoSession.SubmitChanges();
            }

            Document insertedDocument;
            using (var mongoSession = this.OpenMongoSession())
            {
                insertedDocument = mongoSession.Database.GetCollection("Entity").FindOne(null);
            }

            Assert.IsNotNull(insertedDocument);
            Assert.AreEqual(entity.Id, new Guid(((Binary)insertedDocument["_id"]).Bytes));
            Assert.AreEqual(entity.SubEntity.Id, new Guid(((Binary)((Document)insertedDocument["SubEntity"])["_id"]).Bytes));
            Assert.AreEqual(42, ((Document)insertedDocument["SubEntity"])["Integer"]);
            Assert.AreEqual(123.456, ((Document)insertedDocument["SubEntity"])["Double"]);
        }

        public class Entity
        {
            public Guid Id { get; private set; }

            public SubEntity SubEntity { get; set; }
        }

        public class SubEntity
        {
            public Guid Id { get; private set; }

            public double Double { get; set; }

            public int Integer { get; set; }
        }

        public class EntityMap : FluentRootClass<Entity>
        {
            public EntityMap()
            {
                Id(x => x.Id);
                Map(x => x.SubEntity);
            }
        }

        public class SubEntityMap : FluentNestedClass<SubEntity>
        {
            public SubEntityMap()
            {
                Id(x => x.Id);
                Map(x => x.Double);
                Map(x => x.Integer);
            }
        }
    }
}