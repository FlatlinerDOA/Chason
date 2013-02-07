
namespace Chason.UnitTests.Parsing
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    /*
    [TestClass]
    public sealed class GivenABufferFifteenCharactersLongAndABufferSizeOfTenCharacters
    {
        private StringReader reader;

        private JsonParser parser;

        [TestInitialize]
        public void InitializeTest()
        {
            this.reader = new StringReader("{\"A\":1234567890}");
            this.parser = new JsonParser(this.reader, 10);
        }

        [TestMethod]
        public void WhenMoveNextIsCalled_ThenTheResultIsTrue()
        {
            this.parser.MoveNext().Should().Be(true);
        }


        [TestMethod]
        public void WhenMoveNextIsCalled_TheBufferShouldStillBeTheFirstTenCharacters()
        {
            this.parser.MoveNext();
            this.parser.Buffer.Should().BeEquivalentTo("{\"A\":12345");
        }

        [TestMethod]
        public void WhenMoveNextIsCalledFiveTimes_TheBufferWindowShouldHaveShiftedByFiveCharacters()
        {
            for (int i = 0; i < 4; i++)
            {
                this.parser.MoveNext();
            }

            this.parser.MoveNext();

            this.parser.Buffer.Should().BeEquivalentTo("1234567890");
        }


        [TestMethod]
        public void WhenMoveNextIsCalledTenTimes_TheBufferWindowShouldHaveShiftedByFiveCharacters()
        {
            for (int i = 0; i < 9; i++)
            {
                this.parser.MoveNext();
            }

            this.parser.MoveNext();
            this.parser.Buffer.Should().BeEquivalentTo("67890}");
        }

        [TestMethod]
        public void WhenMoveNextIsCalledFifteenTimes_ThenTheResultIsStillTrue()
        {
            for (int i = 0; i < 15; i++)
            {
                this.parser.MoveNext().Should().Be(true);
            }
        }

        [TestMethod]
        public void WhenMoveNextIsCalledSixteenTimes_ThenTheResultIsFalse()
        {
            for (int i = 0; i < 15; i++)
            {
                this.parser.MoveNext();
            }

            this.parser.MoveNext().Should().Be(false);
        }

        [TestMethod]
        public void WhenMoveNextIsCalledSixteenTimes_ThenTheBufferDoesNotMove()
        {
            for (int i = 0; i < 16; i++)
            {
                this.parser.MoveNext();
            }

            this.parser.Buffer.Should().BeEquivalentTo("67890}");
        }

        [TestMethod]
        public void WhenDecoding_TheADictionaryWithASingleNumberIsReturned()
        {
            var d = this.parser.Parse() as IDictionary<string, object>;
            d.Should().NotBeNull();
            d["A"].Should().Be("1234567890");
        }
    }
    */

    [TestClass]
    public sealed class GivenAJsonString
    {
        private JsonParser parser;

        private TestDataContract result;

        [TestInitialize]
        public void InitializeTest()
        {
            this.parser = new JsonParser(@"{""FirstString"":""First \""String\"" "",""FirstInt"":34}");
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
