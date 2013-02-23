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
        private const string JsonText = @"{""Boolean"":true,""Byte"":60,""SignedInt16"":1234,""SignedInt32"":12345,""SignedInt64"":123456789,""UnsignedInt16"":2345,""UnsignedInt32"":123456,""UnsignedInt64"":1234567890,""Decimal"":1.23456789,""Double"":1.123,""Float"":123.45,""DateTime"":""1997-07-16T19:20:30"",""DateTimeOffset"":""2013-02-07T23:33:27.465+11:00"",""TimeSpan"":""1.01:02:03.456""}";

        private ChasonSerializer<SupportedNullableValueTypesContract> serializer;

        private SupportedNullableValueTypesContract result;

        [DataContract]
        public sealed class SupportedNullableValueTypesContract
        {
            [DataMember]
            public bool? Boolean { get; set; }

            [DataMember]
            public byte? Byte { get; set; }

            [DataMember]
            public short? SignedInt16 { get; set; }

            [DataMember]
            public int? SignedInt32 { get; set; }

            [DataMember]
            public long? SignedInt64 { get; set; }

            [DataMember]
            public ushort? UnsignedInt16 { get; set; }

            [DataMember]
            public uint? UnsignedInt32 { get; set; }

            [DataMember]
            public ulong? UnsignedInt64 { get; set; }

            [DataMember]
            public decimal? Decimal { get; set; }

            [DataMember]
            public float? Float { get; set; }

            [DataMember]
            public double? Double { get; set; }

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
        public void TheNullableBooleanIsDeserialized()
        {
            this.result.Boolean.HasValue.Should().Be(true);
            this.result.Boolean.Value.Should().Be(true);
        }

        [TestMethod]
        public void TheNullableByteIsDeserialized()
        {
            this.result.Byte.HasValue.Should().Be(true);
            this.result.Byte.Value.Should().Be(60);
        }

        [TestMethod]
        public void TheNullableSignedInt16IsDeserialized()
        {
            this.result.SignedInt16.HasValue.Should().Be(true);
            this.result.SignedInt16.Value.Should().Be(1234);
        }

        [TestMethod]
        public void TheNullableSignedInt32IsDeserialized()
        {
            this.result.SignedInt32.HasValue.Should().Be(true);
            this.result.SignedInt32.Value.Should().Be(12345);
        }

        [TestMethod]
        public void TheNullableSignedInt64IsDeserialized()
        {
            this.result.SignedInt64.HasValue.Should().Be(true);
            this.result.SignedInt64.Value.Should().Be(123456789);
        }

        [TestMethod]
        public void TheNullableUnsignedInt16IsDeserialized()
        {
            this.result.UnsignedInt16.HasValue.Should().Be(true);
            this.result.UnsignedInt16.Value.Should().Be(2345);
        }

        [TestMethod]
        public void TheNullableUnsignedInt32IsDeserialized()
        {
            this.result.UnsignedInt32.HasValue.Should().Be(true);
            this.result.UnsignedInt32.Value.Should().Be(123456);
        }

        [TestMethod]
        public void TheNullableUnsignedInt64IsDeserialized()
        {
            this.result.UnsignedInt64.HasValue.Should().Be(true);
            this.result.UnsignedInt64.Value.Should().Be(1234567890);
        }

        [TestMethod]
        public void TheNullableDecimalIsDeserialized()
        {
            this.result.Decimal.HasValue.Should().Be(true);
            this.result.Decimal.Value.Should().Be(1.23456789M);
        }

        [TestMethod]
        public void TheNullableDateTimeIsDeserialized()
        {
            this.result.DateTime.HasValue.Should().Be(true);
            this.result.DateTime.Value.Should().Be(new DateTime(1997, 07, 16, 19, 20, 30, DateTimeKind.Unspecified));
        }

        [TestMethod]
        public void TheNullableDateTimeOffsetIsDeserialized()
        {
            //2013-02-07T23:33:27.4655239+11:00
            this.result.DateTimeOffset.HasValue.Should().Be(true);
            this.result.DateTimeOffset.Value.Should().Be(new DateTimeOffset(2013, 02, 07, 23, 33, 27, 465, new TimeSpan(11, 0, 0)));
        }
    }

     [TestClass]
    public sealed class DeserializingNullableValueTypesWithNulls
    {
         private const string JsonText = @"{""Boolean"":null,""Byte"":null,""SignedInt16"":null,""SignedInt32"":null,""SignedInt64"":null,""UnsignedInt16"":null,""UnsignedInt32"":null,""UnsignedInt64"":null,""Decimal"":null,""Double"":null,""Float"":null,""DateTime"":null,""DateTimeOffset"":null,""TimeSpan"":null}";

        private ChasonSerializer<SupportedNullableValueTypesContract> serializer;

        private SupportedNullableValueTypesContract result;

        [DataContract]
        public sealed class SupportedNullableValueTypesContract
        {
            public SupportedNullableValueTypesContract()
            {
                this.Boolean = true;
                this.Byte = 1;
                this.SignedInt16 = 1;
                this.SignedInt32 = 1;
                this.SignedInt64 = 1;
                this.UnsignedInt16 = 1;
                this.UnsignedInt32 = 1;
                this.UnsignedInt64 = 1;
                this.Float = 1.1F;
                this.Double = 1.1D;
                this.Decimal = 1.1M;
                this.DateTime = System.DateTime.Now;
                this.DateTimeOffset = System.DateTimeOffset.Now;
                this.TimeSpan = new TimeSpan(1,0, 0);
            }

            [DataMember]
            public bool? Boolean { get; set; }

            [DataMember]
            public byte? Byte { get; set; }

            [DataMember]
            public short? SignedInt16 { get; set; }

            [DataMember]
            public int? SignedInt32 { get; set; }

            [DataMember]
            public long? SignedInt64 { get; set; }

            [DataMember]
            public ushort? UnsignedInt16 { get; set; }

            [DataMember]
            public uint? UnsignedInt32 { get; set; }

            [DataMember]
            public ulong? UnsignedInt64 { get; set; }

            [DataMember]
            public decimal? Decimal { get; set; }

            [DataMember]
            public float? Float { get; set; }

            [DataMember]
            public double? Double { get; set; }

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
        public void TheNullableBooleanIsDeserialized()
        {
            this.result.Boolean.HasValue.Should().Be(false);
        }

        [TestMethod]
        public void TheNullableByteIsDeserialized()
        {
            this.result.Byte.HasValue.Should().Be(false);
        }

        [TestMethod]
        public void TheNullableSignedInt16IsDeserialized()
        {
            this.result.SignedInt16.HasValue.Should().Be(false);
        }

        [TestMethod]
        public void TheNullableSignedInt32IsDeserialized()
        {
            this.result.SignedInt32.HasValue.Should().Be(false);
        }

        [TestMethod]
        public void TheNullableSignedInt64IsDeserialized()
        {
            this.result.SignedInt64.HasValue.Should().Be(false);
        }

        [TestMethod]
        public void TheNullableUnsignedInt16IsDeserialized()
        {
            this.result.UnsignedInt16.HasValue.Should().Be(false);
        }

        [TestMethod]
        public void TheNullableUnsignedInt32IsDeserialized()
        {
            this.result.UnsignedInt32.HasValue.Should().Be(false);
        }

        [TestMethod]
        public void TheNullableUnsignedInt64IsDeserialized()
        {
            this.result.UnsignedInt64.HasValue.Should().Be(false);
        }

        [TestMethod]
        public void TheNullableDecimalIsDeserialized()
        {
            this.result.Decimal.HasValue.Should().Be(false);
        }

        [TestMethod]
        public void TheNullableDateTimeIsDeserialized()
        {
            this.result.DateTime.HasValue.Should().Be(false);
        }

        [TestMethod]
        public void TheNullableDateTimeOffsetIsDeserialized()
        {
            this.result.DateTimeOffset.HasValue.Should().Be(false);
        }
    }
}
