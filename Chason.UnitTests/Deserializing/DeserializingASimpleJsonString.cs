//--------------------------------------------------------------------------------------------------
// <copyright file="DeserializingASimpleJsonString.cs" company="Andrew Chisholm">
//   Copyright (c) 2013 Andrew Chisholm All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------
namespace Chason.UnitTests.Deserializing
{
    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class DeserializingASimpleJsonString
    {
        private ChasonSerializer<TestDataContract> parser;

        private TestDataContract result;

        [TestInitialize]
        public void InitializeTest()
        {
            this.parser = new ChasonSerializer<TestDataContract>();
            this.result = this.parser.Deserialize(@"{""FirstString"":""First \""String\"" "",""FirstInt"":34}");
        }

        [TestMethod]
        public void TheResultIsNotNull()
        {
            this.result.Should().NotBeNull();
        }

        [TestMethod]
        public void TheResultFirstStringIsLoaded()
        {
            this.result.FirstString.Should().Be("First \"String\" ");
        }

        [TestMethod]
        public void TheResultFirstIntIsLoaded()
        {
            this.result.FirstInt.Should().Be(34);
        }
    }
}
