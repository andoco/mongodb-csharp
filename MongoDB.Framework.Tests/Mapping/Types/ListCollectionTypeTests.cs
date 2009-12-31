﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MongoDB.Driver;

using Moq;
using NUnit.Framework;

namespace MongoDB.Framework.Mapping.Types
{
    public class ListCollectionTypeTests
    {
        [TestFixture]
        public class When_getting_collection_type
        {
            [Test]
            public void should_return_generic_list_type_using_elements_type()
            {
                var collectionType = new ListCollectionType();
                var elementValueType = new Mock<IValueType>();
                elementValueType.SetupGet(evt => evt.Type).Returns(typeof(string));

                Assert.AreEqual(typeof(List<string>), collectionType.GetCollectionType(elementValueType.Object));
            }
        }

        [TestFixture]
        public class When_converting_to_a_document
        {
            private IMappingContext mappingContext;

            [SetUp]
            public void SetUp()
            {
                mappingContext = new Mock<IMappingContext>().Object;
            }

            [Test]
            public void should_return_null_when_value_is_null()
            {
                var collectionType = new ListCollectionType();
                var elementValueType = new Mock<IValueType>();

                var result = collectionType.ConvertToDocumentValue(elementValueType.Object, null, mappingContext);

                Assert.IsNull(result);
            }

            [Test]
            public void should_return_an_empty_array_when_value_is_empty()
            {
                var collectionType = new ListCollectionType();
                var elementValueType = new Mock<IValueType>();
                elementValueType.SetupGet(evt => evt.Type).Returns(typeof(string));

                var result = collectionType.ConvertToDocumentValue(elementValueType.Object, new List<string>(), mappingContext);

                Assert.AreEqual(new string[0], result);
            }

            [Test]
            public void should_return_an_array_when_value_is_not_null()
            {
                var collectionType = new ListCollectionType();
                var elementValueType = new Mock<IValueType>();
                elementValueType.SetupGet(evt => evt.Type).Returns(typeof(string));
                elementValueType.Setup(evt => evt.ConvertToDocumentValue(It.IsAny<string>(), mappingContext)).Returns<string, IMappingContext>((s, mc) => s);

                var result = collectionType.ConvertToDocumentValue(elementValueType.Object, new List<string> { { "one" }, { "two" }, { "three" } }, mappingContext);

                Assert.AreEqual(new[] { "one", "two", "three" }, result);
            }
        }

        [TestFixture]
        public class When_converting_from_a_document
        {
            private IMappingContext mappingContext;

            [SetUp]
            public void SetUp()
            {
                mappingContext = new Mock<IMappingContext>().Object;
            }

            [Test]
            public void should_return_null_when_value_is_null()
            {
                var collectionType = new ListCollectionType();
                var elementValueType = new Mock<IValueType>();

                var result = collectionType.ConvertFromDocumentValue(elementValueType.Object, null, mappingContext);

                Assert.IsNull(result);
            }

            [Test]
            public void should_return_a_list_when_value_exists()
            {
                var collectionType = new ListCollectionType();
                var elementValueType = new Mock<IValueType>();
                elementValueType.SetupGet(evt => evt.Type).Returns(typeof(string));
                elementValueType.Setup(evt => evt.ConvertFromDocumentValue(It.IsAny<string>(), mappingContext)).Returns<string, IMappingContext>((s, mc) => s);

                var result = collectionType.ConvertFromDocumentValue(elementValueType.Object, new[] { "one", "two", "three" }, mappingContext);

                Assert.AreEqual(new List<string> { { "one" }, { "two" }, { "three" } }, result);
            }
        }
    }
}