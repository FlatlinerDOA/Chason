//--------------------------------------------------------------------------------------------------
// <copyright file="ChasonSerializer{T}.cs" company="Andrew Chisholm">
//   Copyright (c) 2013 Andrew Chisholm All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------
namespace Chason
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Text;

    /// <summary>
    /// A fast and lightweight strongly typed JSON serializer.
    /// </summary>
    /// <typeparam name="T">
    /// The data contract or object primitive type to be serialized and deserialized.
    /// </typeparam>
    public sealed class ChasonSerializer<T>
    {
        internal static readonly Lazy<ChasonSerializer<T>> Instance = new Lazy<ChasonSerializer<T>>(); 
        
        #region Fields

        /// <summary>
        /// The serialize method pre-compiled for this specific type from an expression tree
        /// </summary>
        private readonly Action<T, TextWriter> serializeMethod;

        /// <summary>
        /// The serializer settings
        /// </summary>
        private readonly ChasonSerializerSettings settings;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ChasonSerializer{T}"/> class.
        /// </summary>
        public ChasonSerializer()
            : this(ChasonSerializerSettings.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChasonSerializer{T}"/> class.
        /// </summary>
        /// <param name="settings">
        /// The settings to use for serialization and deserialization
        /// </param>
        public ChasonSerializer(ChasonSerializerSettings settings)
        {
            settings.Lock();
            this.settings = settings;
            this.serializeMethod = this.WriteObjectBlock().Compile();
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Deserializes 
        /// </summary>
        /// <param name="source">
        /// </param>
        /// <returns>
        /// </returns>
        public T Deserialize(string source)
        {
            var p = new ChasonParser(source, this.settings);
            return p.Parse<T>();
        }

        /// <summary>
        /// </summary>
        /// <param name="source">
        /// </param>
        /// <returns>
        /// </returns>
        public T Deserialize(Stream source)
        {
            using (var r = new StreamReader(source, this.settings.TextEncoding))
            {
                var p = new ChasonParser(r.ReadToEnd(), this.settings);
                return p.Parse<T>();
            }
        }

        /// <summary>
        /// Serializes an object directly to a stream (does not close the stream).
        /// </summary>
        /// <param name="data">
        /// </param>
        /// <param name="target">
        /// </param>
        public void Serialize(T data, Stream target)
        {
            var b = new StreamWriter(target, this.settings.TextEncoding);
            this.serializeMethod(data, b);
            b.Flush();
        }

        /// <summary>
        /// Serializes an object to a <see cref="TextWriter"/> implementation such as a <see cref="StreamWriter"/> or a <see cref="StringWriter"/>.
        /// </summary>
        /// <param name="data">
        /// </param>
        /// <param name="target">
        /// </param>
        public void Serialize(T data, TextWriter target)
        {
            this.serializeMethod(data, target);
        }

        /// <summary>
        /// Serializes an object to a string
        /// </summary>
        /// <param name="data">
        /// </param>
        /// <returns>
        /// </returns>
        public string Serialize(T data)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                this.serializeMethod(data, writer);
            }

            return sb.ToString();
        }

        #endregion

        #region Methods

        /// <summary>
        /// </summary>
        /// <param name="constant">
        /// </param>
        /// <param name="writer">
        /// </param>
        /// <returns>
        /// </returns>
        private Expression WriteConstant(string constant, ParameterExpression writer)
        {
            return Expression.Call(writer, ChasonSerializer.WriteStringMethod, new Expression[] { Expression.Constant(constant) });
        }

        /// <summary>
        /// </summary>
        /// <param name="property">
        /// </param>
        /// <param name="instance">
        /// </param>
        /// <param name="writer">
        /// </param>
        /// <returns>
        /// </returns>
        private Expression WriteLiteral(PropertyInfo property, ParameterExpression instance, ParameterExpression writer)
        {
            var getterCall = Expression.Call(instance, property.GetGetMethod());
            var writeMethod = typeof(TextWriter).GetMethod("Write", new[] { property.PropertyType });
            var paramType = writeMethod.GetParameters().First().ParameterType;
            if (paramType != property.PropertyType)
            {
                return Expression.Call(writer, writeMethod, new Expression[] { Expression.Convert(getterCall, paramType) });
            }

            return Expression.Call(writer, writeMethod, new Expression[] { getterCall });
        }


        private Expression WriteBoolean(PropertyInfo property, ParameterExpression instance, ParameterExpression writer)
        {
            var getterCall = Expression.Call(instance, property.GetGetMethod());
            var convert = Expression.Condition(getterCall, Expression.Constant("true"), Expression.Constant("false"));
            return Expression.Call(writer, ChasonSerializer.WriteStringMethod, new Expression[] { convert });
        }

        private Expression WriteNullableBoolean(PropertyInfo property, ParameterExpression instance, ParameterExpression writer)
        {
            var getterCall = Expression.Property(instance, property);
            var toString = Expression.Condition(Expression.Property(getterCall, "Value"), Expression.Constant("true"), Expression.Constant("false"));
            var coalesce = Expression.Condition(
                Expression.Equal(getterCall, Expression.Constant(null)),
                Expression.Constant("null"),
                toString);

            return Expression.Call(writer, ChasonSerializer.WriteStringMethod, new Expression[] { coalesce });
        }


        /// <summary>
        /// </summary>
        /// <param name="property">
        /// </param>
        /// <param name="instance">
        /// </param>
        /// <param name="writer">
        /// </param>
        /// <returns>
        /// </returns>
        private Expression WriteNullableLiteral(PropertyInfo property, ParameterExpression instance, ParameterExpression writer, Expression convertToString = null)
        {
            var getterCall = Expression.Property(instance, property);
            var toString = convertToString == null ?
                Expression.Call(getterCall, "ToString", new Type[0]) :
                (Expression)Expression.Invoke(convertToString, new Expression[] { Expression.Property(getterCall, "Value") });
            var coalesce = Expression.Condition(
                Expression.Equal(getterCall, Expression.Constant(null)),
                Expression.Constant("null"),
                toString);

            return Expression.Call(writer, ChasonSerializer.WriteStringMethod, new Expression[] { coalesce });
        }
        /// <summary>
        /// </summary>
        /// <param name="instance">
        /// </param>
        /// <param name="writer">
        /// </param>
        /// <returns>
        /// </returns>
        private IEnumerable<Expression> WriteObject(ParameterExpression instance, ParameterExpression writer)
        {
            return this.WriteObject(typeof(T), instance, writer);
        }

        private IEnumerable<Expression> WriteObject(Type objectType, ParameterExpression instance, ParameterExpression writer)
        {
            var members =
               from p in objectType.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public)
               from c in p.GetCustomAttributes(true).OfType<DataMemberAttribute>()
               where c != null
               orderby c.Name ?? p.Name, c.Order
               select new { Property = p, Contract = c };

            bool first = true;
            yield return this.WriteConstant("{", writer);
            foreach (var m in members)
            {
                yield return this.WriteStartProperty(m.Property, m.Contract, first, writer);

                foreach (var expression in this.WritePropertyValue(m.Property, instance, writer, m))
                {
                    yield return expression;
                }

                first = false;
            }

            yield return this.WriteConstant("}", writer);
        }

        private IEnumerable<Expression> WritePropertyValue(PropertyInfo property, ParameterExpression instance, ParameterExpression writer, object m)
        {
            var type = property.PropertyType;
            if (type == typeof(string))
            {
                yield return this.WriteString(property, instance, writer);
            }
            else if (type == typeof(bool))
            {
                yield return this.WriteBoolean(property, instance, writer);
            }
            else if (ChasonSerializer.LiteralTypes.Contains(type))
            {
                yield return this.WriteLiteral(property, instance, writer);
            }
            else if (this.settings.CustomStringWriters.ContainsKey(type))
            {
                yield return this.WriteObjectAsString(property, instance, writer, this.settings.CustomStringWriters[type]);
            } 
            else if (this.settings.CustomNumberWriters.ContainsKey(type))
            {
                yield return this.WriteObjectAsNumber(property, instance, writer, this.settings.CustomNumberWriters[type]);
            }
            else if (type.IsArray)
            {
                yield return this.WriteArray(property, instance, writer);
            }
            else
            {
                var elementType = Nullable.GetUnderlyingType(type);
                if (elementType != null)
                {
                    if (elementType == typeof(bool))
                    {
                        yield return this.WriteNullableBoolean(property, instance, writer);
                    } 
                    else if (ChasonSerializer.LiteralTypes.Contains(elementType))
                    {
                        yield return this.WriteNullableLiteral(property, instance, writer);
                    }
                    else if (this.settings.CustomStringWriters.ContainsKey(elementType))
                    {
                        yield return this.WriteNullableObjectAsString(property, instance, writer, this.settings.CustomStringWriters[elementType]);
                    }
                    else if (this.settings.CustomNumberWriters.ContainsKey(elementType))
                    {
                        yield return this.WriteNullableObjectAsNumber(property, instance, writer, this.settings.CustomNumberWriters[elementType]);
                    }
                    else
                    {
                        yield return this.WriteNullableObjectAsString(property, instance, writer, null);
                    }
                }
                else
                {
                    foreach (var ex in this.WriteObject(property.PropertyType, instance, writer))
                    {
                        yield return ex;
                    }
                }
            }
        }

        private Expression WriteArray(PropertyInfo property, ParameterExpression instance, ParameterExpression writer)
        {
            var elementType = property.PropertyType.GetElementType();
            var enumerableType = typeof(IEnumerable<>).MakeGenericType(elementType);
            var getterCall = Expression.Property(instance, property);
            var writeMethod = this.GetType().GetMethod("WriteArrayValues", BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(elementType);
            var writeValues = Expression.Call(
                null, 
                writeMethod, 
                new Expression[]
                {
                    Expression.Convert(getterCall, enumerableType),
                    writer
                });

            return writeValues;
        }

        public static void WriteArrayValues<TValue>(IEnumerable<TValue> values, TextWriter writer)
        {
            if (values == null)
            {
                writer.Write("null");
                return;
            }

            writer.Write("[");
            bool first = true;
            foreach (var value in values)
            {
                if (!first)
                {
                    writer.Write(",");
                }

                first = false;
                if (ReferenceEquals(value, null))
                {
                    writer.Write("null");
                }
                else if (typeof(TValue) == typeof(string))
                {
                    writer.Write(ChasonSerializer.EscapeString(value.ToString()));
                }
                else
                {
                    writer.Write(value);
                }
            }

            writer.Write("]");
        }

        /// <summary>
        /// </summary>
        /// <param name="property">
        /// </param>
        /// <param name="instance">
        /// </param>
        /// <param name="writer">
        /// </param>
        /// <param name="convertToString">
        /// </param>
        /// <returns>
        /// </returns>
        private Expression WriteNullableObjectAsString(
            PropertyInfo property,
            ParameterExpression instance,
            ParameterExpression writer,
            Expression convertToString)
        {
            var getterCall = Expression.Property(instance, property);
            var toString = convertToString == null ? 
                Expression.Call(getterCall, "ToString", new Type[0]) : 
                (Expression)Expression.Invoke(convertToString, new Expression[] { Expression.Property(getterCall, "Value") });
            var coalesce = Expression.Condition(
                Expression.Equal(getterCall, Expression.Constant(null)), 
                Expression.Constant("null"), 
                toString);

            var escapeMethod = typeof(ChasonSerializer).GetMethod("EscapeString", new[] { typeof(string) });
            var escapeCall = Expression.Call(null, escapeMethod, new Expression[] { coalesce });
            var result = Expression.Call(writer, ChasonSerializer.WriteStringMethod, new Expression[] { escapeCall });
            return result;
        }

        /// <summary>
        /// </summary>
        /// <param name="property">
        /// </param>
        /// <param name="instance">
        /// </param>
        /// <param name="writer">
        /// </param>
        /// <param name="convertToString">
        /// </param>
        /// <returns>
        /// </returns>
        private Expression WriteObjectAsString(
            PropertyInfo property, 
            ParameterExpression instance, 
            ParameterExpression writer,
            Expression convertToString)
        {
            var getterCall = Expression.Property(instance, property);
            var toString = convertToString == null ? Expression.Call(getterCall, "ToString", new Type[0]) : (Expression)Expression.Invoke(convertToString, new Expression[] { getterCall });
            
            var escapeMethod = typeof(ChasonSerializer).GetMethod("EscapeString", new[] { typeof(string) });
            var escapeCall = Expression.Call(null, escapeMethod, new Expression[] { toString });
            return Expression.Call(writer, ChasonSerializer.WriteStringMethod, new Expression[] { escapeCall });
        }

        /// <summary>
        /// </summary>
        /// <param name="property">
        /// </param>
        /// <param name="instance">
        /// </param>
        /// <param name="writer">
        /// </param>
        /// <param name="convertToDecimal">
        /// </param>
        /// <returns>
        /// </returns>
        private Expression WriteObjectAsNumber(
            PropertyInfo property,
            ParameterExpression instance,
            ParameterExpression writer,
            Expression convertToDecimal = null)
        {
            var getterCall = Expression.Property(instance, property);
            var toDecimal = convertToDecimal == null ? Expression.Convert(getterCall, typeof(decimal)) : (Expression)Expression.Invoke(convertToDecimal, new Expression[] { getterCall });
            return Expression.Call(writer, "Write", new Type[0], new Expression[] { toDecimal });
        }

        private Expression WriteNullableObjectAsNumber(
            PropertyInfo property,
            ParameterExpression instance,
            ParameterExpression writer,
            Expression convertToDecimal = null)
        {
            var getterCall = Expression.Property(instance, property);
            var toDecimal = convertToDecimal == null ?
                Expression.Convert(getterCall, typeof(decimal)) :
                (Expression)Expression.Invoke(convertToDecimal, new Expression[] { Expression.Property(getterCall, "Value") });
            var coalesce = Expression.Condition(
                Expression.Equal(getterCall, Expression.Constant(null)),
                Expression.Constant("null"),
                toDecimal);

            var result = Expression.Call(writer, "Write", new Type[0], new Expression[] { coalesce });
            return result;
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        private Expression<Action<T, TextWriter>> WriteObjectBlock()
        {
            var instance = Expression.Parameter(typeof(T), "i");
            var writer = Expression.Parameter(typeof(TextWriter), "b");
            var block = Expression.Block(this.WriteObject(instance, writer));
            return (Expression<Action<T, TextWriter>>)Expression.Lambda(block, instance, writer);
        }

        /// <summary>
        /// Creates an expression that when called will output the property name
        /// </summary>
        /// <param name="property">
        /// </param>
        /// <param name="contract">
        /// </param>
        /// <param name="first">
        /// </param>
        /// <param name="writer">
        /// </param>
        /// <returns>
        /// </returns>
        private Expression WriteStartProperty(
            PropertyInfo property, DataMemberAttribute contract, bool first, ParameterExpression writer)
        {
            var buffer = first
                             ? "\"" + (contract.Name ?? property.Name) + "\":"
                             : ",\"" + (contract.Name ?? property.Name) + "\":";

            return Expression.Call(writer, ChasonSerializer.WriteStringMethod, new Expression[] { Expression.Constant(buffer) });
        }

        /// <summary>
        /// </summary>
        /// <param name="property">
        /// </param>
        /// <param name="instance">
        /// </param>
        /// <param name="writer">
        /// </param>
        /// <returns>
        /// </returns>
        private Expression WriteString(PropertyInfo property, ParameterExpression instance, ParameterExpression writer)
        {
            var getterCall = Expression.Property(instance, property);
            var escapeMethod = typeof(ChasonSerializer).GetMethod("EscapeString", new[] { typeof(string) });
            var escapeCall = Expression.Call(null, escapeMethod, new Expression[] { getterCall });
            return Expression.Call(writer, ChasonSerializer.WriteStringMethod, new Expression[] { escapeCall });
        }

        #endregion
    }
}