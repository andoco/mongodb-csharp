﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MongoDB.Driver;
using MongoDB.Framework.Configuration.Fluent.Mapping;
using MongoDB.Framework.Configuration.Mapping;
using MongoDB.Framework.Mapping;

using NUnit.Framework;
using System.Threading;

namespace MongoDB.Framework.Updates
{
    [TestFixture]
    public class ManyToOne : TestCase
    {
        protected override IMapModelRegistry MapModelRegistry
        {
            get 
            {
                return new FluentMapModelRegistry()
                    .AddMap(new EntityMap())
                    .AddMap(new EntityRefMap());
            }
        }

        protected override void BeforeTest()
        {
            using (var mongoSession = this.OpenMongoSession())
            {
                var refId = new Binary(Guid.NewGuid().ToByteArray()) { Subtype = Binary.TypeCode.Uuid };
                mongoSession.Database.GetCollection("EntityRef")
                    .Insert(new Document().Append("_id", refId).Append("Name", "Jack"));
                mongoSession.Database.GetCollection("Entity")
                    .Insert(new Document()
                        .Append("_id", new Binary(Guid.NewGuid().ToByteArray()) { Subtype = Binary.TypeCode.Uuid })
                        .Append("Reference", new DBRef("EntityRef", refId)));
            }
        }

        protected override void AfterTest()
        {
            using (var mongoSession = this.OpenMongoSession())
            {
                mongoSession.Database.MetaData.DropCollection("Entity");
                mongoSession.Database.MetaData.DropCollection("EntityRef");
            }
        }

        [Test]
        public void Should_update()
        {
            using (var mongoSession = this.OpenMongoSession())
            {
                var entity = mongoSession.FindOne<Entity>(null);
                entity.Reference = null;
                mongoSession.SubmitChanges();
            }

            Document updatedDocument;
            using (var mongoSession = this.OpenMongoSession())
            {
                updatedDocument = mongoSession.Database.GetCollection("Entity").FindOne(null);
            }

            Assert.IsNotNull(updatedDocument);
            Assert.IsNull(updatedDocument["Reference"]);
        }

        public class Entity
        {
            public Guid Id { get; private set; }

            public EntityRef Reference { get; set; }
        }

        public class EntityRef
        {
            public Guid Id { get; private set; }
            public string Name { get; private set; }
        }

        public class EntityMap : FluentRootClass<Entity>
        {
            public EntityMap()
            {
                Id(x => x.Id);
                Map(x => x.Reference).AsReference().NotLazy();
            }
        }

        public class EntityRefMap : FluentRootClass<EntityRef>
        {
            public EntityRefMap()
            {
                Id(x => x.Id);
                Map(x => x.Name);
            }
        }

    }
}