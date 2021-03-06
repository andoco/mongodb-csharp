﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MongoDB.Driver;
using MongoDB.Framework.Configuration.Fluent.Mapping;
using MongoDB.Framework.Configuration.Mapping;
using MongoDB.Framework.Mapping;

using NUnit.Framework;

namespace MongoDB.Framework.Updates
{
    [TestFixture]
    public class NestedClassDictionary : TestCase
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

        protected override void BeforeTest()
        {
            using (var mongoSession = this.OpenMongoSession())
            {
                mongoSession.Database.GetCollection("Entity")
                    .Insert(new Document()
                        .Append("_id", new Binary(Guid.NewGuid().ToByteArray()) { Subtype = Binary.TypeCode.Uuid })
                        .Append("SubEntities", new Document()
                            .Append("one", new Document()
                                .Append("Integer", 1)
                                .Append("Double", 1.1))
                            .Append("two", new Document()
                                .Append("Integer", 2)
                                .Append("Double", 2.2))
                            .Append("three", new Document()
                                .Append("Integer", 3)
                                .Append("Double", 3.3))));
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
        public void Should_update()
        {
            using (var mongoSession = this.OpenMongoSession())
            {
                var entity = mongoSession.FindOne<Entity>(null);
                entity.SubEntities.Remove("two");
                entity.SubEntities.Remove("three");
                entity.SubEntities.Add("four", new SubEntity()
                {
                    Double = 4.4,
                    Integer = 4
                });
                mongoSession.SubmitChanges();
            }

            Document insertedDocument;
            using (var mongoSession = this.OpenMongoSession())
            {
                insertedDocument = mongoSession.Database.GetCollection("Entity").FindOne(null);
            }

            Assert.IsNotNull(insertedDocument);
            var subEntities = (Document)insertedDocument["SubEntities"];
            Assert.AreEqual(2, subEntities.Count);
            Assert.AreEqual(1, ((Document)subEntities["one"])["Integer"]);
            Assert.AreEqual(1.1, ((Document)subEntities["one"])["Double"]);
            Assert.AreEqual(4, ((Document)subEntities["four"])["Integer"]);
            Assert.AreEqual(4.4, ((Document)subEntities["four"])["Double"]);
        }

        public class Entity
        {
            public Guid Id { get; private set; }

            public Dictionary<string, SubEntity> SubEntities { get; set; }
        }

        public class SubEntity
        {
            public int Integer { get; set; }

            public double Double { get; set; }
        }

        public class EntityMap : FluentRootClass<Entity>
        {
            public EntityMap()
            {
                Id(x => x.Id);
                Map(x => x.SubEntities);
            }
        }

        public class SubEntityMap : FluentNestedClass<SubEntity>
        {
            public SubEntityMap()
            {
                Map(x => x.Double);
                Map(x => x.Integer);
            }
        }
    }
}