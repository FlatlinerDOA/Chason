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

    using Chason.Extensions;

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
            var block = this.WriteObjectBlock();
            this.serializeMethod = block.Compile();
////#if DEBUG
////            this.serializeMethod(Activator.CreateInstance<T>(), new StringWriter());
////#endif
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
        /// <param name="getterCall">
        /// </param>
        /// <param name="writer">
        /// </param>
        /// <returns>
        /// </returns>
        private Expression WriteLiteral(Expression getterCall, Type memberType, ParameterExpression writer)
        {
            var writeMethod = typeof(TextWriter).GetMethod("Write", new[] { memberType });
            var paramType = writeMethod.GetParameters().First().ParameterType;
            if (paramType != memberType)
            {
                return Expression.Call(writer, writeMethod, new Expression[] { Expression.Convert(getterCall, paramType) });
            }

            return Expression.Call(writer, writeMethod, new Expression[] { getterCall });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="getterCall"></param>
        /// <param name="writer"></param>
        /// <returns></returns>
        private Expression WriteBoolean(Expression getterCall, ParameterExpression writer)
        {
            var convert = Expression.Condition(getterCall, Expression.Constant("true"), Expression.Constant("false"));
            return Expression.Call(writer, ChasonSerializer.WriteStringMethod, new Expression[] { convert });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="getterCall"></param>
        /// <param name="writer"></param>
        /// <returns></returns>
        private Expression WriteNullableBoolean(Expression getterCall, ParameterExpression writer)
        {
            var toString = Expression.Condition(Expression.Property(getterCall, "Value"), Expression.Constant("true"), Expression.Constant("false"));
            var coalesce = Expression.Condition(
                Expression.Equal(getterCall, Expression.Constant(null)),
                Expression.Constant("null"),
                toString);

            return Expression.Call(writer, ChasonSerializer.WriteStringMethod, new Expression[] { coalesce });
        }


        /// <summary>
        /// </summary>
        /// <param name="getterCall">
        /// </param>
        /// <param name="writer">
        /// </param>
        /// <returns>
        /// </returns>
        private Expression WriteNullableLiteral(Expression getterCall, ParameterExpression writer, Expression convertToString = null)
        {
            var toString = convertToString == null ?
                Expression.Call(getterCall, "ToString", new Type[0]) :
                (Expression)Expression.Invoke(convertToString, new Expression[] { Expression.Property(getterCall, "Value") });
            var coalesce = Expression.Condition(
                Expression.Equal(getterCall, Expression.Constant(null)),
                Expression.Constant("null"),
                toString);

            return Expression.Call(writer, ChasonSerializer.WriteStringMethod, new Expression[] { coalesce });
        }

        private IEnumerable<Expression> WriteObject(Type objectType, Expression instance, ParameterExpression writer, int depth)
        {
            var members = Reflect.GetObjectMemberContracts(objectType);

            bool first = true;
            yield return this.WriteConstant("{", writer);

            foreach (var m in members)
            {
                yield return this.WriteStartMember(m, first, writer, depth + 1);

                foreach (var expression in this.WriteMemberValue(m.Member, instance, writer, depth + 1))
                {
                    yield return expression;
                }

                first = false;
            }

            if (this.settings.OutputFormattedJson)
            {
                yield return this.WriteConstant(StringExtensions.LineFeedIndent(depth) + "}", writer);
            }
            else
            {
                yield return this.WriteConstant("}", writer);
            }
        }

        private IEnumerable<Expression> WriteMemberValue(MemberInfo member, Expression instance, ParameterExpression writer, int depth)
        {
            var type = member.MemberType();
            var getterCall = Expression.MakeMemberAccess(instance, member);
            return this.WriteValue(getterCall, type, writer, depth);
        }

        private IEnumerable<Expression> WriteValue(Expression getterCall, Type type, ParameterExpression writer, int depth)
        {
            if (type == typeof(string))
            {
                yield return this.WriteString(getterCall, writer);
            }
            else if (type == typeof(bool))
            {
                yield return this.WriteBoolean(getterCall, writer);
            }
            else if (ChasonSerializer.LiteralTypes.Contains(type))
            {
                yield return this.WriteLiteral(getterCall, type, writer);
            }
            else if (this.settings.CustomStringWriters.ContainsKey(type))
            {
                yield return this.WriteObjectAsString(getterCall, writer, this.settings.CustomStringWriters[type]);
            } 
            else if (this.settings.CustomNumberWriters.ContainsKey(type))
            {
                yield return this.WriteObjectAsNumber(getterCall, writer, this.settings.CustomNumberWriters[type]);
            }
            else if (this.settings.CustomLiteralWriters.ContainsKey(type))
            {
                yield return this.WriteObjectAsLiteral(getterCall, writer, this.settings.CustomLiteralWriters[type]);
            }
            else if (this.settings.CustomDictionaryWriters.ContainsKey(type))
            {
                yield return this.WriteObjectAsDictionary(getterCall, type, writer, depth, this.settings.CustomDictionaryWriters[type]);
            }
            else if (type.IsArray || type.IsCollection())
            {
                yield return this.WriteArray(getterCall, type, writer, depth);
            }
            else if (type.IsDictionary())
            {
                yield return this.WriteDictionary(getterCall, type, writer, depth);
            }
            else
            {
                var elementType = Nullable.GetUnderlyingType(type);
                if (elementType != null)
                {
                    if (elementType == typeof(bool))
                    {
                        yield return this.WriteNullableBoolean(getterCall, writer);
                    } 
                    else if (ChasonSerializer.LiteralTypes.Contains(elementType))
                    {
                        yield return this.WriteNullableLiteral(getterCall, writer);
                    }
                    else if (this.settings.CustomStringWriters.ContainsKey(elementType))
                    {
                        yield return this.WriteNullableObjectAsString(getterCall, writer, this.settings.CustomStringWriters[elementType]);
                    }
                    else if (this.settings.CustomNumberWriters.ContainsKey(elementType))
                    {
                        yield return this.WriteNullableObjectAsNumber(getterCall, writer, this.settings.CustomNumberWriters[elementType]);
                    }
                    else
                    {
                        yield return this.WriteNullableObjectAsString(getterCall, writer, null);
                    }
                }
                else
                {
                    foreach (var ex in this.WriteObject(type, getterCall, writer, depth))
                    {
                        yield return ex;
                    }
                }
            }
        }

        private Expression WriteDictionary(Expression getterCall, Type memberType, ParameterExpression writer, int depth)
        {
            var elementTypes = memberType.GetGenericArguments();
            var dictionaryType = typeof(IDictionary<,>).MakeGenericType(elementTypes);
            var writeMethod = this.GetType().GetMethod("WriteDictionaryValues", BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(elementTypes);
            var writeValues = Expression.Call(
                null,
                writeMethod,
                new Expression[]
                {
                    Expression.Convert(getterCall, dictionaryType),
                    writer,
                    Expression.Constant(this.settings.OutputCamelCasePropertyNames),
                    Expression.Constant(this.settings.OutputFormattedJson),
                    Expression.Constant(depth)
                });

            return writeValues;
        }

        private Expression WriteArray(Expression getterCall, Type memberType, ParameterExpression writer, int depth)
        {
            var elementType = memberType.IsArray ? memberType.GetElementType() : memberType.GetGenericArguments()[0];
            var enumerableType = typeof(IEnumerable<>).MakeGenericType(elementType);
            var writeMethod = this.GetType().GetMethod("WriteArrayValues", BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(elementType);
            var writeValues = Expression.Call(
                null, 
                writeMethod, 
                new Expression[]
                {
                    Expression.Convert(getterCall, enumerableType),
                    writer,
                    Expression.Constant(this.settings.OutputFormattedJson),
                    Expression.Constant(depth)
                });

            return writeValues;
        }

        public static void WriteDictionaryValues<TKey, TValue>(IDictionary<TKey, TValue> values, TextWriter writer, bool camelCase, bool formatted, int depth)
        {
            if (values == null)
            {
                writer.Write("null");
                return;
            }

            writer.Write("{");

            bool first = true;
            foreach (var value in values)
            {
                if (!first)
                {
                    writer.Write(",");
                }

                if (formatted)
                {
                    writer.Write(StringExtensions.LineFeedIndent(depth + 1));
                }

                first = false;
                if (!ReferenceEquals(value.Key, null))
                {
                    writer.Write(camelCase ? value.Key.ToString().CamelCase().JsonEscapeString() : value.Key.ToString().JsonEscapeString());
                    writer.Write(formatted ? ": " : ":");

                    if (ReferenceEquals(value.Value, null))
                    {
                        writer.Write("null");
                    }
                    else if (typeof(TValue) == typeof(string))
                    {
                        writer.Write(value.Value.ToString().JsonEscapeString());
                    }
                    else
                    {
                        writer.Write(value);
                    }
                }
            }

            if (formatted)
            {
                writer.Write(StringExtensions.LineFeedIndent(depth));
            }

            writer.Write("}");
        }

        public static void WriteArrayValues<TValue>(IEnumerable<TValue> values, TextWriter writer, bool formatted, int depth)
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

                if (formatted)
                {
                    writer.Write(StringExtensions.LineFeedIndent(depth + 1));
                }

                first = false;
                if (ReferenceEquals(value, null))
                {
                    writer.Write("null");
                }
                else if (typeof(TValue) == typeof(string))
                {
                    writer.Write(value.ToString().JsonEscapeString());
                }
                else
                {
                    writer.Write(value);
                }
            }

            if (formatted)
            {
                writer.Write(StringExtensions.LineFeedIndent(depth));
            }

            writer.Write("]");
        }

        /// <summary>
        /// </summary>
        /// <param name="getterCall">
        /// </param>
        /// <param name="writer">
        /// </param>
        /// <param name="convertToString">
        /// </param>
        /// <returns>
        /// </returns>
        private Expression WriteNullableObjectAsString(
            Expression getterCall,
            ParameterExpression writer,
            Expression convertToString)
        {
            var toString = convertToString == null ? 
                Expression.Call(getterCall, "ToString", new Type[0]) : 
                (Expression)Expression.Invoke(convertToString, new Expression[] { Expression.Property(getterCall, "Value") });
            var coalesce = Expression.Condition(
                Expression.Equal(getterCall, Expression.Constant(null)), 
                Expression.Constant("null"), 
                toString);

            var escapeCall = coalesce.CallJsonEscapeString();
            var result = Expression.Call(writer, ChasonSerializer.WriteStringMethod, new Expression[] { escapeCall });
            return result;
        }

        /// <summary>
        /// </summary>
        /// <param name="member">
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
            Expression getterCall,
            ParameterExpression writer,
            Expression convertToString)
        {
            var toString = convertToString == null ? Expression.Call(getterCall, "ToString", new Type[0]) : (Expression)Expression.Invoke(convertToString, new Expression[] { getterCall });

            var escapeCall = toString.CallJsonEscapeString();
            return Expression.Call(writer, ChasonSerializer.WriteStringMethod, new Expression[] { escapeCall });
        }

        /// <summary>
        /// </summary>
        /// <param name="member">
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
            Expression getterCall,
            ParameterExpression writer,
            Expression convertToDecimal = null)
        {
            var toDecimal = convertToDecimal == null ? Expression.Convert(getterCall, typeof(decimal)) : (Expression)Expression.Invoke(convertToDecimal, new Expression[] { getterCall });
            return Expression.Call(writer, "Write", new Type[0], new Expression[] { toDecimal });
        }

        /// <summary>
        /// </summary>
        /// <param name="member">
        /// </param>
        /// <param name="instance">
        /// </param>
        /// <param name="writer">
        /// </param>
        /// <param name="convertToDecimal">
        /// </param>
        /// <returns>
        /// </returns>
        private Expression WriteObjectAsLiteral(
            Expression getterCall,
            ParameterExpression writer,
            Expression convertToString = null)
        {
            var toString = convertToString == null ? Expression.Convert(getterCall, typeof(string)) : (Expression)Expression.Invoke(convertToString, new Expression[] { getterCall });
            return Expression.Call(writer, ChasonSerializer.WriteStringMethod, new Expression[] { toString });
        }

        /// <summary>
        /// </summary>
        /// <param name="member">
        /// </param>
        /// <param name="instance">
        /// </param>
        /// <param name="writer">
        /// </param>
        /// <param name="convertToDecimal">
        /// </param>
        /// <returns>
        /// </returns>
        private Expression WriteObjectAsDictionary(
            Expression getterCall,
            Type type,
            ParameterExpression writer,
            int depth,
            Expression convertToDictionary = null)
        {
            var toDictionary = convertToDictionary == null ? Expression.Convert(getterCall, typeof(IDictionary<string, string>).MakeGenericType(type.GetGenericArguments())) : (Expression)Expression.Invoke(convertToDictionary, new Expression[] { getterCall });
            return this.WriteDictionary(toDictionary, type, writer, depth);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="member"></param>
        /// <param name="instance"></param>
        /// <param name="writer"></param>
        /// <param name="convertToDecimal"></param>
        /// <returns></returns>
        private Expression WriteNullableObjectAsNumber(
            Expression getterCall,
            ParameterExpression writer,
            Expression convertToDecimal = null)
        {
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
            var block = Expression.Block(this.WriteValue(instance, typeof(T), writer, 0));
            return (Expression<Action<T, TextWriter>>)Expression.Lambda(block, instance, writer);
        }

        /// <summary>
        /// Creates an expression that when called will output the member name
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
        private Expression WriteStartMember(MemberContractMap memberContract, bool first, ParameterExpression writer, int depth)
        {
            var memberName = memberContract.Name;
            if (this.settings.OutputCamelCasePropertyNames)
            {
                memberName = memberName.CamelCase();
            }

            Expression buffer;
            if (this.settings.OutputFormattedJson)
            {
                buffer = Expression.Constant(first ? StringExtensions.LineFeedIndent(depth) + "\"" + memberName + "\": " : "," + StringExtensions.LineFeedIndent(depth) + "\"" + memberName + "\": ");
            }
            else
            {
                buffer = Expression.Constant(first ? "\"" + memberName + "\":" : ",\"" + memberName + "\":");
            }

            return Expression.Call(writer, ChasonSerializer.WriteStringMethod, new Expression[] { buffer });
        }

        /// <summary>
        /// </summary>
        /// <param name="member">
        /// </param>
        /// <param name="instance">
        /// </param>
        /// <param name="writer">
        /// </param>
        /// <returns>
        /// </returns>
        private Expression WriteString(Expression getterCall, ParameterExpression writer)
        {
            var escapeCall = getterCall.CallJsonEscapeString();
            return Expression.Call(writer, ChasonSerializer.WriteStringMethod, new Expression[] { escapeCall });
        }

        #endregion
    }
}