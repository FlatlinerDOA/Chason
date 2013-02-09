namespace Chason
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Text;

    /// <summary>
    /// A fast and lightweight strongly typed JSON serializer.
    /// </summary>
    /// <typeparam name="T">The data contract or object primitive type to be serialized and deserialized.</typeparam>
    public sealed class ChasonSerializer<T>
        where T : new()
    {
        /// <summary>
        /// The text encoding to use when reading and writing to streams.
        /// </summary>
        private readonly Encoding encoding;

        /// <summary>
        /// The serialize method
        /// </summary>
        private readonly Action<T, TextWriter> serializeMethod;

        /// <summary>
        /// A cache to the method to write with
        /// </summary>
        private readonly MethodInfo writeStringMethod;

        /// <summary>
        /// The serializer settings
        /// </summary>
        private readonly ChasonSerializerSettings settings;

        /// <summary>
        /// The culture to use
        /// </summary>
        private CultureInfo culture;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChasonSerializer{T}"/> class.
        /// </summary>
        public ChasonSerializer() : this(ChasonSerializerSettings.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChasonSerializer{T}"/> class.
        /// </summary>
        /// <param name="settings">The settings to use for serialization and deserialization</param>
        public ChasonSerializer(ChasonSerializerSettings settings)
        {
            settings.Lock();
            this.settings = settings;
            this.encoding = settings.TextEncoding;
            this.culture = settings.CultureInfo;
            this.writeStringMethod = typeof(TextWriter).GetMethod("Write", new[] { typeof(string) });
            this.serializeMethod = WriteObjectBlock().Compile();
        }

        /// <summary>
        /// Serializes an object directly to a stream (does not close the stream).
        /// </summary>
        /// <param name="data"></param>
        /// <param name="target"></param>
        public void Serialize(T data, Stream target)
        {
            var b = new StreamWriter(target, this.encoding);
            this.serializeMethod(data, b);
            b.Flush();
        }

        /// <summary>
        /// Serializes an object to a <see cref="TextWriter"/> implementation such as a <see cref="StreamWriter"/> or a <see cref="StringWriter"/>.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="target"></param>
        public void Serialize(T data, TextWriter target)
        {
            this.serializeMethod(data, target);
        }

        /// <summary>
        /// Serializes an object to a string
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string Serialize(T data)
        {
            var sb =new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                this.serializeMethod(data, writer);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Deserializes 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public T Deserialize(string source)
        {
            var p = new ChasonParser(source, this.settings);
            return p.Parse<T>();
        }

        public T Deserialize(Stream source)
        {
            using (var r = new StreamReader(source, this.encoding))
            {
                var p = new ChasonParser(r.ReadToEnd(), this.settings);
                return p.Parse<T>();
            }
        }

        private Expression<Action<T, TextWriter>> WriteObjectBlock()
        {
            var instance = Expression.Parameter(typeof(T), "i");
            var writer = Expression.Parameter(typeof(TextWriter), "b");
            var block = Expression.Block(this.WriteObject(instance, writer));
            return (Expression<Action<T, TextWriter>>)Expression.Lambda(block, instance, writer);
        }

        private IEnumerable<Expression> WriteObject(ParameterExpression instance, ParameterExpression writer)
        {
            var members = from p in typeof(T).GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public)
                          from c in p.GetCustomAttributes(true).OfType<DataMemberAttribute>()
                          where c != null
                          orderby c.Name ?? p.Name, c.Order
                          select new { Property = p, Contract = c };

            bool first = true;
            yield return this.WriteConstant("{", writer);
            foreach (var m in members)
            {
                yield return WriteStartProperty(m.Property, m.Contract, first, writer);
                if (m.Property.PropertyType == typeof(string))
                {
                    yield return this.WriteString(m.Property, instance, writer);
                }
                else
                {
                    yield return WriteLiteral(m.Property, instance, writer);
                }

                first = false;
            }

            yield return this.WriteConstant("}", writer);
        }

        private Expression WriteConstant(string constant, ParameterExpression writer)
        {
            return Expression.Call(writer, this.writeStringMethod, Expression.Constant(constant));
        }

        private Expression WriteStartProperty(PropertyInfo property, DataMemberAttribute contract, bool first, ParameterExpression writer)
        {
            var buffer = first ? "\"" + (contract.Name ?? property.Name) + "\":" : ",\"" + (contract.Name ?? property.Name) + "\":";
            ////var buffer = this.encoding.GetBytes(start);
            return Expression.Call(writer, this.writeStringMethod, Expression.Constant(buffer));
        }

        private Expression WriteString(PropertyInfo property, ParameterExpression instance, ParameterExpression writer)
        {
            var getterCall = Expression.Property(instance, property);
            var replaceMethod = this.GetType().GetMethod("EscapeString", new[] { typeof(string) });
            var replaceCall = Expression.Call(null, replaceMethod, new Expression[] { getterCall });
            return Expression.Call(writer, this.writeStringMethod, new Expression[] { replaceCall });
        }

        private Expression WriteLiteral(PropertyInfo property, ParameterExpression instance, ParameterExpression writer)
        {
            var getterCall = Expression.Call(instance, property.GetGetMethod());
            var writeMethod = typeof(TextWriter).GetMethod("Write", new[] { property.PropertyType });
            return Expression.Call(writer, writeMethod, getterCall);
        }

        private static char[] charsToEscape = new[] { '"', '\\', '\r', '\n', '\t', '/', '\b', '\f' };
        
        public static string EscapeString(string input)
        {
            if (input == null)
            {
                return "null";
            }


            var sb = new StringBuilder();
            sb.Append('"');
            int lastIndex = 0;
            int index = input.IndexOfAny(charsToEscape);
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
                    index = input.IndexOfAny(charsToEscape, index);
                }

                sb.Append(input, lastIndex, input.Length - lastIndex);
            }

            sb.Append('"');
            return sb.ToString();
        }
    }
}
