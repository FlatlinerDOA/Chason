using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Chason.UnitTests.Serializing
{
    [TestClass]
    public class SerializingGenericLists
    {
        [DataContract]
        public sealed class ListContract<T>
        {
            [DataMember]
            public List<T> List { get; set; }
        }

        [TestMethod]
        public void SerializingAnIntArray()
        {
            var t = new ListContract<int>() { List = new List<int> { 10, 20, 30 } };
            var s = new ChasonSerializer<ListContract<int>>();
            s.Serialize(t).Should().Be("{\"List\":[10,20,30]}");
        }

        [TestMethod]
        public void SerializingANullableIntArray()
        {
            var t = new ListContract<int?> { List = new List<int?> { 10, 20, 30, (int?)null } };
            var s = new ChasonSerializer<ListContract<int?>>();
            s.Serialize(t).Should().Be("{\"List\":[10,20,30,null]}");
        }

        [TestMethod]
        public void SerializingADecimalArray()
        {
            var t = new ListContract<decimal>() { List = new List<decimal> { 10.1M, 20.22M, 33.333M } };
            var s = new ChasonSerializer<ListContract<decimal>>();
            s.Serialize(t).Should().Be("{\"List\":[10.1,20.22,33.333]}");
        }

        [TestMethod]
        public void SerializingANullableDecimalArray()
        {
            var t = new ListContract<decimal?> { List = new List<decimal?> { 10.1M, 20.22M, 33.333M, (decimal?)null } };
            var s = new ChasonSerializer<ListContract<decimal?>>();
            s.Serialize(t).Should().Be("{\"List\":[10.1,20.22,33.333,null]}");
        }

        [TestMethod]
        public void SerializingAByteArray()
        {
            var t = new ListContract<byte> { List = new List<byte> { (byte)10, (byte)20, (byte)30 } };
            var s = new ChasonSerializer<ListContract<byte>>();
            s.Serialize(t).Should().Be("{\"List\":[10,20,30]}");
        }
    }
}
