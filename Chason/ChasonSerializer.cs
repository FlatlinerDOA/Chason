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

    /// <summary>
    /// Global static functions and data used by the <see cref="ChasonSerializer{T}"/> instance
    /// </summary>
    public static class ChasonSerializer
    {
        #region Static Fields

        /// <summary>
        /// The list of escaped characters in JSON
        /// </summary>
        internal static readonly char[] CharsToEscape = new[] { '"', '\\', '\r', '\n', '\t', '/', '\b', '\f' };

        /// <summary>
        /// A cache to the method to write with
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

            return EscapeString(input.ToString());
        }

        /// <summary>
        /// </summary>
        /// <param name="input">
        /// </param>
        /// <returns>
        /// </returns>
        public static string EscapeString(string input)
        {
            if (input == null)
            {
                return "null";
            }

            var sb = new StringBuilder();
            sb.Append('"');
            int lastIndex = 0;
            int index = input.IndexOfAny(CharsToEscape);
            if (index == -1)
            {
                sb.Append(input);
            }
            else
            {
                while (index != -1)
                {
                    sb.Append(input, lastIndex, index - lastIndex);
                    var c = input[index];
                    if (c == '"')
                    {
                        sb.Append(@"\""");
                    }
                    else if (c == '\\')
                    {
                        sb.Append(@"\\");
                    }
                    else if (c == '\r')
                    {
                        sb.Append(@"\r");
                    }
                    else if (c == '\n')
                    {
                        sb.Append(@"\n");
                    }
                    else if (c == '\t')
                    {
                        sb.Append(@"\t");
                    }
                    else if (c == '/')
                    {
                        sb.Append(@"\/");
                    }
                    else if (c == '\b')
                    {
                        sb.Append(@"\b");
                    }
                    else if (c == '\f')
                    {
                        sb.Append(@"\f");
                    }

                    index++;
                    lastIndex = index;
                    index = input.IndexOfAny(CharsToEscape, index);
                }

                sb.Append(input, lastIndex, input.Length - lastIndex);
            }

            sb.Append('"');
            return sb.ToString();
        }

        /// <summary>
        /// Converts a string to camel case (also ensures that abbreviations up to 3 characters long are camel cased correctly)
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string CamelCase(string text)
        {
            if (text == null)
            {
                return null;
            }

            var sb = new StringBuilder();
            bool lowering = true;
            for (int i = 0; i < text.Length; i++)
            {
                if (lowering && char.IsUpper(text[i]) && i < 4)
                {
                    // Have we lowered at least one character and is the next char lower (then don't lower this one)
                    if (i > 0 && text.Length > i + 1 && char.IsLower(text[i + 1]))
                    {
                        lowering = false;
                        sb.Append(text[i]);
                    }
                    else
                    {
                        sb.Append(char.ToLower(text[i]));
                    }
                }
                else
                {
                    lowering = false;
                    sb.Append(text[i]);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets the full name of the data contract type (or just the name of the type if it is not a DataContract)
        /// </summary>
        /// <param name="dataContractType"></param>
        /// <returns></returns>
        public static string GetDataContractFullName(Type dataContractType)
        {
            var att = dataContractType.GetCustomAttributes(typeof(DataContractAttribute), true);
            if (att == null || att.Length == 0)
            {
                return dataContractType.Name;
            }

            var sb = new StringBuilder();
            var ns = ((DataContractAttribute)att[0]).Namespace;
            if (!string.IsNullOrEmpty(ns))
            {
                sb.Append(ns);
                sb.Append("/");
            }

            sb.Append(((DataContractAttribute)att[0]).Name);
            return sb.ToString();
        }

        public static string SerializeToString<T>(T data)
        {
            return ChasonSerializer<T>.Instance.Value.Serialize(data);
        }

        public static T DeserializeFromString<T>(string json)
        {
            return ChasonSerializer<T>.Instance.Value.Deserialize(json);
        }
    }
}