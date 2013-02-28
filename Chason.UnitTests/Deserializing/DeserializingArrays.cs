//--------------------------------------------------------------------------------------------------
// <copyright file="DeserializingArrays.cs" company="Andrew Chisholm">
//   Copyright (c) 2013 Andrew Chisholm All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------
namespace Chason.UnitTests.Deserializing
{
    using System;
    using System.Linq;
    using System.Runtime.Serialization;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class DeserializingArrays
    {
        [DataContract]
        public sealed class ArrayContract<T> 
        {
            [DataMember]
            public T[] Array { get; set; }

            public override bool Equals(object obj)
            {
                var a = obj as ArrayContract<T>;
                if (a == null)
                {
                    return false;
                }

                return this.Array.SequenceEqual(a.Array);
            }
            
        }

        [TestMethod]
        public void DeserializingAnIntArray()
        {
            var t = "{\"Array\":[10,20,30]}";
            var s = new ChasonSerializer<ArrayContract<int>>();
            s.Deserialize(t).Should().Be(new ArrayContract<int>() { Array = new[] { 10, 20, 30 } });
        }

        [TestMethod]
        public void DeserializingANullableIntArray()
        {
            var t = "{\"Array\":[10,20,30,null]}";
            var s = new ChasonSerializer<ArrayContract<int?>>();
            s.Deserialize(t).Should().Be(new ArrayContract<int?> { Array = new[] { 10, 20, 30, (int?)null } });
        }

        [TestMethod]
        public void DeserializingADecimalArray()
        {
            var t = "{\"Array\":[10.1,20.22,33.333]}";
            var s = new ChasonSerializer<ArrayContract<decimal>>();
            s.Deserialize(t).Should().Be(new ArrayContract<decimal>() { Array = new[] { 10.1M, 20.22M, 33.333M } });
        }

        [TestMethod]
        public void DeserializingANullableDecimalArray()
        {
            var t = "{\"Array\":[10.1,20.22,33.333,null]}";
            var s = new ChasonSerializer<ArrayContract<decimal?>>();
            s.Deserialize(t).Should().Be(new ArrayContract<decimal?> { Array = new[] { 10.1M, 20.22M, 33.333M, (decimal?)null } });
        }

        [TestMethod]
        public void DeserializingAByteArray()
        {
            var t = "{\"Array\":[10,20,30]}";
            var s = new ChasonSerializer<ArrayContract<byte>>();
            s.Deserialize(t).Should().Be(new ArrayContract<byte> { Array = new[] { (byte)10, (byte)20, (byte)30 } });
        }
    }
}
