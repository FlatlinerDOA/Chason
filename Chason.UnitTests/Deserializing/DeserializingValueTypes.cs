using System;
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
        public string JsonText = @"{""String"":""String"",""SignedInt32"":12345,""SignedInt64"":123456789,""UnsignedInt32"":123456,""UnsignedInt64"":1234567890,""Decimal"":1.23456789,""Double"":1.123,""Float"":123.45,""Single"":1.2,""DateTime"":""1997-07-16T19:20:30.45+01:00"",""DateTimeInfo"":""2013-02-07T23:33:27.4655239+11:00"",""TimeSpan"":""1.01:02:03.456"",""TimeZoneInfo"":""AUS Eastern Standard Time""}";

        private SupportedValueTypesContract result;

        private ChasonSerializer<SupportedValueTypesContract> serializer;

        private TimeZoneInfo expectedTimeZone;

        [DataContract]
        public sealed class SupportedValueTypesContract
        {
            [DataMember]
            public string String { get; set; }

            [DataMember]
            public int SignedInt32 { get; set; }

            [DataMember]
            public long SignedInt64 { get; set; }

            [DataMember]
            public uint UnsignedInt32 { get; set; }

            [DataMember]
            public ulong UnsignedInt64 { get; set; }

            [DataMember]
            public decimal Decimal { get; set; }

            [DataMember]
            public double Double { get; set; }

            [DataMember]
            public float Float { get; set; }

            [DataMember]
            public Single Single { get; set; }

            [DataMember]
            public DateTime DateTime { get; set; }

            [DataMember]
            public DateTimeOffset DateTimeOffset { get; set; }

            [DataMember]
            public TimeSpan TimeSpan { get; set; }

            [DataMember]
            public TimeZoneInfo TimeZoneInfo { get; set; }
        }

        [TestInitialize]
        public void InitializeTest()
        {
            this.expectedTimeZone = TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time");
            this.serializer = new ChasonSerializer<SupportedValueTypesContract>();
            this.result = this.serializer.Deserialize(JsonText);
        }

        [TestMethod]
        public void ThenTheStringIsDeserializedCorrectly()
        {
            this.result.String.Should().Be("String");
        }

        [TestMethod]
        public void ThenTheSignedInt32IsDeserializedCorrectly()
        {
            this.result.SignedInt32.Should().Be(12345);
        }

        [TestMethod]
        public void ThenTheSignedInt64IsDeserializedCorrectly()
        {
            this.result.SignedInt64.Should().Be(123456789);
        }

        [TestMethod]
        public void ThenTheTimeZoneInfoIsDeserializedToTheCorrectTimeZone()
        {
            this.result.TimeZoneInfo.Should().Be(this.expectedTimeZone);
        }
    }
}
