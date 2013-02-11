//--------------------------------------------------------------------------------------------------
// <copyright file="DeserializingNullableValueTypes.cs" company="Andrew Chisholm">
//   Copyright (c) 2013 Andrew Chisholm All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------
namespace Chason.UnitTests.Deserializing
{
    using System;
    using System.Runtime.Serialization;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class DeserializingNullableValueTypes
    {
        private const string JsonText = @"{""SignedInt32"":12345,""SignedInt64"":123456789,""UnsignedInt32"":123456,""UnsignedInt64"":1234567890,""Decimal"":1.23456789,""Double"":1.123,""Float"":123.45,""Single"":1.2,""DateTime"":""1997-07-16T19:20:30"",""DateTimeOffset"":""2013-02-07T23:33:27.4655239+11:00"",""TimeSpan"":""1.01:02:03.456""}";

        private ChasonSerializer<SupportedNullableValueTypesContract> serializer;

        private SupportedNullableValueTypesContract result;

        [DataContract]
        public sealed class SupportedNullableValueTypesContract
        {
            [DataMember]
            public int? SignedInt32 { get; set; }

            [DataMember]
            public long? SignedInt64 { get; set; }

            [DataMember]
            public uint? UnsignedInt32 { get; set; }

            [DataMember]
            public ulong? UnsignedInt64 { get; set; }

            [DataMember]
            public decimal? Decimal { get; set; }

            [DataMember]
            public double? Double { get; set; }

            [DataMember]
            public float? Float { get; set; }

            [DataMember]
            public Single? Single { get; set; }

            [DataMember]
            public DateTime? DateTime { get; set; }

            [DataMember]
            public DateTimeOffset? DateTimeOffset { get; set; }

            [DataMember]
            public TimeSpan? TimeSpan { get; set; }
        }

        [TestInitialize]
        public void InitializeTest()
        {
            this.serializer = new ChasonSerializer<SupportedNullableValueTypesContract>();
            this.result = this.serializer.Deserialize(JsonText);
        }

        [TestMethod]
        public void TheSignedInt32Then()
        {
            this.result.SignedInt32.HasValue.Should().Be(true);
            this.result.SignedInt32.Value.Should().Be(12345);
        }
    }
}
