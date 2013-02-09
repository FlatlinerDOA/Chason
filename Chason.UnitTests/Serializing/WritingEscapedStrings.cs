//--------------------------------------------------------------------------------------------------
// <copyright file="WritingEscapedStrings.cs" company="Andrew Chisholm">
//   Copyright (c) 2013 Andrew Chisholm All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------
namespace Chason.UnitTests.Serializing
{
    using System;
    using System.Runtime.Serialization;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class WritingEscapedStrings
    {
        private ChasonSerializer<SimpleStringContract> serializer;

        [DataContract]
        public sealed class SimpleStringContract
        {
            [DataMember]
            public string Text { get; set; }
        }

        [TestInitialize]
        public void InitializeTest()
        {
            this.serializer = new ChasonSerializer<SimpleStringContract>();
        }

        [TestMethod]
        public void CarriageReturn()
        {
            var result = this.serializer.Serialize(new SimpleStringContract() { Text = "\r" });
            result.Should().Be(@"{""Text"":""\r""}");
        }

        [TestMethod]
        public void LineFeed()
        {
            var result = this.serializer.Serialize(new SimpleStringContract() { Text = "\n" });
            result.Should().Be("{\"Text\":\"\\n\"}");
        }

        [TestMethod]
        public void Tab()
        {
            var result = this.serializer.Serialize(new SimpleStringContract() { Text = "\t" });
            result.Should().Be("{\"Text\":\"\\t\"}");
        }

        [TestMethod]
        public void CarriageReturnLineFeed()
        {
            var result = this.serializer.Serialize(new SimpleStringContract() { Text = "\r\n" });
            result.Should().Be("{\"Text\":\"\\r\\n\"}");
        }

        [TestMethod]
        public void SlashSlashCarriageReturnLineFeedSlash()
        {
            var result = this.serializer.Serialize(new SimpleStringContract() { Text = "\\\\\r\n\\" });
            result.Should().Be("{\"Text\":\"\\\\\\\\\\r\\n\\\"}");
        }
    }
}
