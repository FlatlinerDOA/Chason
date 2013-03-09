//--------------------------------------------------------------------------------------------------
// <copyright file="ChasonSerializer.cs" company="Andrew Chisholm">
//   Copyright (c) 2013 Andrew Chisholm All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------
namespace Chason
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Text;

    using Chason.Extensions;

    /// <summary>
    /// Global static functions and data used by the <see cref="ChasonSerializer{T}"/> instance
    /// </summary>
    public static class ChasonSerializer
    {
        #region Static Fields


        /// <summary>
        /// A cache to the text writer method to write a string with
        /// </summary>
        internal static readonly MethodInfo WriteStringMethod = typeof(TextWriter).GetMethod("Write", new[] { typeof(string) });

        internal static readonly HashSet<Type> LiteralTypes = new HashSet<Type>
                                                                  {
                                                                      typeof(bool),
                                                                      typeof(byte),
                                                                      typeof(short),
                                                                      typeof(int),
                                                                      typeof(long), 
                                                                      typeof(ushort),
                                                                      typeof(uint), 
                                                                      typeof(ulong), 
                                                                      typeof(decimal),  
                                                                      typeof(float),
                                                                      typeof(double),
                                                                  };


        #endregion

        public static string EscapeObjectString(object input)
        {
            if (ReferenceEquals(input, null))
            {
                return "null";
            }

            return input.ToString().JsonEscapeString();
        }        


        public static string SerializeToString<T>(T data)
        {
            return ChasonSerializer<T>.Instance.Value.Serialize(data);
        }

        public static string SerializeToString<T>(T data, ChasonSerializerSettings settings)
        {
            return new ChasonSerializer<T>(settings).Serialize(data);
        }

        public static T DeserializeFromString<T>(string json)
        {
            return ChasonSerializer<T>.Instance.Value.Deserialize(json);
        }

        public static T DeserializeFromString<T>(string json, ChasonSerializerSettings settings)
        {
            return new ChasonSerializer<T>(settings).Deserialize(json);
        }
    }
}