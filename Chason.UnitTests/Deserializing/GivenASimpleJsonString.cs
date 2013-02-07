
namespace Chason.UnitTests.Deserializing
{
    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class GivenASimpleJsonString
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
