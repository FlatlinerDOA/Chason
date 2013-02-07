
namespace Chason.UnitTests.Parsing
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class GivenASimpleJsonString
    {
        private ChasonParser parser;

        private TestDataContract result;

        [TestInitialize]
        public void InitializeTest()
        {
            this.parser = new ChasonParser(@"{""FirstString"":""First \""String\"" "",""FirstInt"":34}");
            this.result = this.parser.Parse<TestDataContract>();
        }

        [TestMethod]
        public void ThenTheResultIsNotNull()
        {
            this.result.Should().NotBeNull();
        }

        [TestMethod]
        public void ThenTheResultFirstStringIsLoaded()
        {
            this.result.FirstString.Should().Be("First \"String\" ");
        }

        [TestMethod]
        public void ThenTheResultFirstIntIsLoaded()
        {
            this.result.FirstInt.Should().Be(34);
        }
    }
}
