using NUnit.Framework;
using JSONSchema2POCO;
using System;
using ClassGenerator;
using System.Diagnostics;
using System.Collections.Generic;

namespace JSONSchema2POCO.Tests
{
    public class RandomTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            HashSet<Nullable<bool>> set = new HashSet<Nullable<bool>>() { true, null };
        }
    }
}