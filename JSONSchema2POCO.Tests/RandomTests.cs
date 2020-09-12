using NUnit.Framework;
using JSONSchema2POCO;
using System;
using ClassGenerator;
using System.Diagnostics;

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
            A a = 2;
            Console.WriteLine(a.I);
            int i = a+1;
            Console.WriteLine(i);
        }
    }
}