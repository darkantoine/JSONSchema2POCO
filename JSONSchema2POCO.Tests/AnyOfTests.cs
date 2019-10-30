using NUnit.Framework;
using JSONSchema2POCO;
using System;

namespace JSONSchema2POCO.Tests
{
    public class AnyOfTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            AnyOf<string, bool> anyOf = new AnyOf<string, bool>(true);
            Assert.AreEqual(typeof(bool), anyOf.GetUnderlyingType());
            Assert.AreEqual(true, anyOf.GetValue());

            AnyOf<string, bool> anyOf2 = new AnyOf<string, bool>("hello");
            Assert.AreEqual(typeof(string), anyOf2.GetUnderlyingType());
            Assert.AreEqual("hello", anyOf2.GetValue());

            var ex = Assert.Throws<ArgumentException>(() => new AnyOf<string,bool>(3));
        }
    }
}