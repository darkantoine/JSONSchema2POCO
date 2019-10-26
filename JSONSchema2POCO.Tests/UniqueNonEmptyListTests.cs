using NUnit.Framework;
using JSONSchema2POCO;
using System;
using System.Collections.Generic;

namespace JSONSchema2POCO.Tests
{
    public class UniqueNonEmptyListTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            UniqueNonEmptyList<string> list = new UniqueNonEmptyList<string>( new List<string> { "hello", "world" } );
            Assert.Throws<ArgumentException>(() => list.Add("hello"));
            Assert.IsTrue(list.Remove("hello"));
            Assert.IsFalse(list.Remove("world"));

            Assert.Throws<ArgumentException>(() => new UniqueNonEmptyList<string>(new List<string>()));
           
        }
    }
}