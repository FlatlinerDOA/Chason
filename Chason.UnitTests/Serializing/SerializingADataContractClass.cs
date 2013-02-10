
namespace Chason.UnitTests.Serializing
{
    using System.IO;
    using System.Runtime.Serialization.Json;
    using System.Text;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SerializingADataContractClass
    {
        private readonly TestDataContract test = new TestDataContract { FirstString = "First \"String\" ", SecondString = "Second \\ 'String' ", FirstInt = 1,  };
        //FirstDate = new DateTime(2000, 1, 1), SecondDate = new DateTime(2001, 1, 1)
        private readonly MemoryStream result = new MemoryStream();

        private ChasonSerializer<TestDataContract> serializer;

        private string textResult;

        [TestInitialize]
        public void IntializeTest()
        {
            this.serializer = new ChasonSerializer<TestDataContract>();
            this.serializer.Serialize(this.test, this.result);
            this.textResult = Encoding.UTF8.GetString(this.result.ToArray());
        }

        [TestMethod]
        public void TheOutputStreamIsNotEmpty()
        {
            this.result.Length.Should().NotBe(0);
        }

        [TestMethod]
        public void TheTextResultEqualsTheCorrectJson()
        {
            var m = new MemoryStream();
            var s = new DataContractJsonSerializer(typeof(TestDataContract));
            s.WriteObject(m, this.test);
            
            var expected = Encoding.UTF8.GetString(m.ToArray());
            Assert.AreEqual(expected, this.textResult, false);
        }
    }
}
