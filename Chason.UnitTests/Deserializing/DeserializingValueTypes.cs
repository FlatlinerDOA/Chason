﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chason.UnitTests.Deserializing
{
    using System.Runtime.Serialization;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class DeserializingValueTypes
    {
        private const string JsonText = @"{""String"":""String"",""SignedInt16"":255,""SignedInt32"":12345,""SignedInt64"":123456789,""UnsignedInt16"":1255,""UnsignedInt32"":123456,""UnsignedInt64"":1234567890,""Decimal"":1.23456789,""Double"":1.123456,""Float"":123.45,""DateTime"":""1997-07-16T19:20:30"",""DateTimeOffset"":""2013-02-07T23:33:27.988000+08:00"",""TimeSpan"":""1.02:03:04.567""}";

        private SupportedValueTypesContract result;

        private ChasonSerializer<SupportedValueTypesContract> serializer;

        [DataContract]
        public sealed class SupportedValueTypesContract
        {
            [DataMember]
            public string String { get; set; }

            [DataMember]
            public short SignedInt16 { get; set; }

            [DataMember]
            public int SignedInt32 { get; set; }

            [DataMember]
            public long SignedInt64 { get; set; }

            [DataMember]
            public ushort UnsignedInt16 { get; set; }

            [DataMember]
            public uint UnsignedInt32 { get; set; }

            [DataMember]
            public ulong UnsignedInt64 { get; set; }

            [DataMember]
            public decimal Decimal { get; set; }

            [DataMember]
            public float Float { get; set; }

            [DataMember]
            public double Double { get; set; }

            [DataMember]
            public DateTime DateTime { get; set; }

            [DataMember]
            public DateTimeOffset DateTimeOffset { get; set; }

            [DataMember]
            public TimeSpan TimeSpan { get; set; }

            ////[DataMember]
            ////public TimeZoneInfo TimeZoneInfo { get; set; }
        }

        [TestInitialize]
        public void InitializeTest()
        {
            this.serializer = new ChasonSerializer<SupportedValueTypesContract>();
            this.result = this.serializer.Deserialize(JsonText);
        }

        [TestMethod]
        public void TheStringIsDeserializedCorrectly()
        {
            this.result.String.Should().Be("String");
        }

        [TestMethod]
        public void TheSignedInt32IsDeserializedCorrectly()
        {
            this.result.SignedInt32.Should().Be(12345);
        }

        [TestMethod]
        public void TheSignedInt64IsDeserializedCorrectly()
        {
            this.result.SignedInt64.Should().Be(123456789);
        }

        [TestMethod]
        public void TheUnsignedInt32IsDeserializedCorrectly()
        {
            this.result.UnsignedInt32.Should().Be(123456);
        }

        [TestMethod]
        public void TheUnsignedInt64IsDeserializedCorrectly()
        {
            this.result.UnsignedInt64.Should().Be(1234567890);
        }

        [TestMethod]
        public void TheDecimalIsDeserializedCorrectly()
        {
            this.result.Decimal.Should().Be(1.23456789M);
        }

        [TestMethod]
        public void TheDoubleIsDeserializedCorrectly()
        {
            this.result.Double.Should().Be(1.123456D);
        }

        [TestMethod]
        public void TheFloatIsDeserializedCorrectly()
        {
            this.result.Float.Should().Be(123.45F);
        }

        [TestMethod]
        public void TheSignedInt16IsDeserializedCorrectly()
        {
            this.result.SignedInt16.Should().Be(255);
        }

        [TestMethod]
        public void TheUnsignedInt16IsDeserializedCorrectly()
        {
            this.result.UnsignedInt16.Should().Be(1255);
        }

        [TestMethod]
        public void TheDateTimeIsDeserializedAsUnspecifiedWithOffsetTruncated()
        {
            this.result.DateTime.Should().Be(new DateTime(1997, 7, 16, 19, 20, 30, 0, DateTimeKind.Unspecified));
        }

        [TestMethod]
        public void TheDateTimeOffsetIsDeserializedCorrectly()
        {
            this.result.DateTimeOffset.Should().Be(new DateTimeOffset(2013, 2, 7, 23, 33, 27, 988, TimeSpan.FromHours(8)));
        }

        [TestMethod]
        public void TheTimeSpanIsDeserializedCorrectly()
        {
            this.result.TimeSpan.Should().Be(new TimeSpan(1, 2, 3, 4, 567));
        }
      }
}
