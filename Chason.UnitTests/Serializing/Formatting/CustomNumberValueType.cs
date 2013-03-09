using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chason.UnitTests.Serializing.Formatting
{
    using System.Runtime.Serialization;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class CustomNumberValueType
    {
        private ChasonSerializer<ContractWithCustomNumberType> serializer;

        private string result;

        public struct TimeStamp
        {
            private readonly long ticks;

            public TimeStamp(long ticks)
            {
                this.ticks = ticks;
            }

            public long Ticks
            {
                get
                {
                    return this.ticks;
                }
            }

            public static TimeStamp Now()
            {
                return new TimeStamp(DateTime.UtcNow.Ticks);
            }
        }

        [DataContract]
        public sealed class ContractWithCustomNumberType
        {
            [DataMember]
            public string Name { get; set; }

            [DataMember]
            public TimeStamp Created { get; set; }
        }

        [TestInitialize]
        public void InitializeTest()
        {
            var settings = new ChasonSerializerSettings();
            settings.SetNumberFormatter(d => d.Ticks, d => new TimeStamp((long)d));
            this.serializer = new ChasonSerializer<ContractWithCustomNumberType>(settings);
            this.result = this.serializer.Serialize(new ContractWithCustomNumberType() {
                                                                                           Name = "Frank",
                                                                                           Created = new TimeStamp(1234567890)
                                                                                       });
        }

        [TestMethod]
        public void TheCustomNumberTypeIsOutputAsANumber()
        {
            this.result.Should().Contain("\"Created\":1234567890");
        }

        [TestMethod]
        public void TheCustomNumberTypeIsDeserializedCorrectly()
        {
            var d = this.serializer.Deserialize(this.result);
            d.Created.Ticks.Should().Be(1234567890);
        }
    }
}
