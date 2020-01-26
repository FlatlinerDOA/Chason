using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Chason.UnitTests.Deserializing
{
    [TestClass]
    public class DeserializingLists
    {
        [DataContract]
        public sealed class ListContract<T>
        {
            [DataMember]
            public List<T> List { get; set; }
        }
        [DataContract]
        public sealed class BookContract
        {
            [DataMember]
            public int Id { get; set; }

            [DataMember]
            public string Name { get; set; }
        }

        [TestMethod]
        public void DeserializingAnIntList()
        {
            var s = new ChasonSerializer<ListContract<int>>();
            s.Deserialize("{\"List\":[10,20,30]}").List.SequenceEqual(new List<int> { 10, 20, 30 }).Should().Be(true);
        }

        [TestMethod]
        public void DeserializingANullableIntList()
        {
            var s = new ChasonSerializer<ListContract<int?>>();
            s.Deserialize("{\"List\":[10,20,30,null]}").List.SequenceEqual(new List<int?> { 10, 20, 30, (int?)null }).Should().Be(true);
        }

        [TestMethod]
        public void DeserializingADecimalList()
        {
            var s = new ChasonSerializer<ListContract<decimal>>();
            s.Deserialize("{\"List\":[10.1,20.22,33.333]}").List.SequenceEqual(new List<decimal> { 10.1M, 20.22M, 33.333M }).Should().Be(true);
        }

        [TestMethod]
        public void DeserializingANullableDecimalList()
        {
            var s = new ChasonSerializer<ListContract<decimal?>>();
            s.Deserialize("{\"List\":[10.1,20.22,33.333,null]}").List.SequenceEqual(new List<decimal?> { 10.1M, 20.22M, 33.333M, (decimal?)null }).Should().Be(true);
        }

        [TestMethod]
        public void DeserializingAByteList()
        {
            var s = new ChasonSerializer<ListContract<byte>>();
            s.Deserialize("{\"List\":[10,20,30]}").List.SequenceEqual(new List<byte> { (byte)10, (byte)20, (byte)30 }).Should().Be(true);
        }

        [TestMethod]
        public void DeserializingABookList()
        {
            var s = new ChasonSerializer<ListContract<BookContract>>();
            var r = s.Deserialize("{\"List\":[{\"Id\":10,\"Name\":\"A\"},{\"Id\":20,\"Name\":\"B\"},{\"Id\":30,\"Name\":\"C\"}]}").List;
            r.Select(i => i.Id).SequenceEqual(new[] { 10, 20, 30 }).Should().Be(true);
            r.Select(i => i.Name).SequenceEqual(new[] { "A", "B", "C" }).Should().Be(true);
        }
    }
}
