//--------------------------------------------------------------------------------------------------
// <copyright file="SerializingArrays.cs" company="Andrew Chisholm">
//   Copyright (c) 2013 Andrew Chisholm All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------
namespace Chason.UnitTests.Serializing
{
    using System.Runtime.Serialization;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class SerializingArrays
    {
        [DataContract]
        public sealed class ArrayContract<T>
        {
            [DataMember]
            public T[] Array { get; set; }
        }
        
        [TestMethod]
        public void SerializingAnIntArray()
        {
            var t = new ArrayContract<int>() { Array = new[] { 10, 20, 30 } };
            var s = new ChasonSerializer<ArrayContract<int>>();
            s.Serialize(t).Should().Be("{\"Array\":[10,20,30]}");
        }

        [TestMethod]
        public void SerializingANullableIntArray()
        {
            var t = new ArrayContract<int?> { Array = new[] { 10, 20, 30, (int?)null } };
            var s = new ChasonSerializer<ArrayContract<int?>>();
            s.Serialize(t).Should().Be("{\"Array\":[10,20,30,null]}");
        }

        [TestMethod]
        public void SerializingADecimalArray()
        {
            var t = new ArrayContract<decimal>() { Array = new[] { 10.1M, 20.22M, 33.333M } };
            var s = new ChasonSerializer<ArrayContract<decimal>>();
            s.Serialize(t).Should().Be("{\"Array\":[10.1,20.22,33.333]}");
        }

        [TestMethod]
        public void SerializingANullableDecimalArray()
        {
            var t = new ArrayContract<decimal?> { Array = new[] { 10.1M, 20.22M, 33.333M, (decimal?)null } };
            var s = new ChasonSerializer<ArrayContract<decimal?>>();
            s.Serialize(t).Should().Be("{\"Array\":[10.1,20.22,33.333,null]}");
        }

        [TestMethod]
        public void SerializingAByteArray()
        {
            var t = new ArrayContract<byte> { Array = new[] { (byte)10, (byte)20, (byte)30 } };
            var s = new ChasonSerializer<ArrayContract<byte>>();
            s.Serialize(t).Should().Be("{\"Array\":[10,20,30]}");
        }

    }
}
