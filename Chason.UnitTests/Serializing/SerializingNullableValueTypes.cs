//--------------------------------------------------------------------------------------------------
// <copyright file="SerializingNullableValueTypes.cs" company="Andrew Chisholm">
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
    public sealed class SerializingNullableValueTypes
    {
        private readonly SupportedNullableValueTypesContract Data = new SupportedNullableValueTypesContract()
                                                                        {
                                                                            SignedInt32 = 12345, 
                                                                            SignedInt64 = 123456789, 
                                                                            UnsignedInt32 = 123456, 
                                                                            UnsignedInt64 = 1234567890, 
                                                                            Decimal = 1.23456789M, 
                                                                            Double = 1.123D, 
                                                                            Float = 123.45f, 
                                                                            DateTime = new DateTime(1997, 07, 16, 19, 20, 30, DateTimeKind.Unspecified), 
                                                                            DateTimeOffset = new DateTimeOffset(2013, 02, 07, 23, 33, 27, 465, new TimeSpan(0,11,0,0)), 
                                                                            TimeSpan= new TimeSpan(1, 1, 2, 3, 456)
                                                                        };

        private ChasonSerializer<SupportedNullableValueTypesContract> serializer;

        private string result;

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
            this.result = this.serializer.Serialize(Data);
        }

        [TestMethod]
        public void TheNullableSignedInt32IsSerialized()
        {
            this.result.Should().Contain("\"SignedInt32\":12345");
        }

        [TestMethod]
        public void TheNullableSignedInt64IsSerialized()
        {
            this.result.Should().Contain("\"SignedInt64\":123456789");
        }

        [TestMethod]
        public void TheNullableUnsignedInt32IsSerialized()
        {
            this.result.Should().Contain("\"UnsignedInt32\":123456");
        }

        [TestMethod]
        public void TheNullableUnsignedInt64IsSerialized()
        {
            this.result.Should().Contain("\"UnsignedInt64\":1234567890");
        }

        [TestMethod]
        public void TheNullableDecimalIsSerialized()
        {
            this.result.Should().Contain("\"Decimal\":1.23456789");
        }

        [TestMethod]
        public void TheNullableDateTimeIsSerializedInISO8601Format()
        {
            this.result.Should().Contain("\"DateTime\":\"1997-07-16T19:20:30\"");
        }
    }

     [TestClass]
    public sealed class SerializingNullableValueTypesWithNulls
    {
        private const string JsonText = @"{""SignedInt32"":null,""SignedInt64"":null,""UnsignedInt32"":null,""UnsignedInt64"":null,""Decimal"":null,""Double"":null,""Float"":null,""Single"":null,""DateTime"":null,""DateTimeOffset"":null,""TimeSpan"":null}";

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
    }
}
