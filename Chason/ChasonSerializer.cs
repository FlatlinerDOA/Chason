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

    public sealed class ChasonSerializer<T> where T : new()
    {
        private readonly Encoding encoding;

        private readonly Action<T, StreamWriter> serializeMethod;

        private readonly MethodInfo writeStringMethod;

        private readonly Dictionary<string, Action<T, StreamReader>> deserializeMethods;

        private readonly Action<T, StreamReader> deserializeMethod;

        public ChasonSerializer()
        {
            this.encoding = Encoding.UTF8;
            this.writeStringMethod = typeof(StreamWriter).GetMethod("Write", new[] { typeof(string) });
            this.serializeMethod = WriteObjectBlock().Compile();
            ////this.deserializeMethod = this.ReadObjectBlock().Compile();
        }

        public void Serialize(T data, Stream target)
        {
            var b = new StreamWriter(target, this.encoding);
            this.serializeMethod(data, b);
            b.Flush();
        }

        public void Serialize(T data, StreamWriter target)
        {
            this.serializeMethod(data, target);
        }

        public T Deserialize(Stream source)
        {
            var target = new T();
            Deserialize(source, target);
            return target;
        }

        private void Deserialize(Stream source, T target)
        {
            var r = new StreamReader(source, this.encoding);
            this.deserializeMethod(target, r);
            r.DiscardBufferedData();
        }

        private Expression<Action<T, StreamReader>> ReadObjectBlock()
        {
            var instance = Expression.Parameter(typeof(T), "i");
            var reader = Expression.Parameter(typeof(StreamReader), "b");
            var block = Expression.Block(this.ReadObject(instance, reader));
            return (Expression<Action<T, StreamReader>>)Expression.Lambda(block, instance, reader);
        }

        private IEnumerable<Expression> ReadObject(ParameterExpression instance, ParameterExpression reader)
        {
            var members = from p in typeof(T).GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public)
                          from c in p.GetCustomAttributes(true).OfType<DataMemberAttribute>()
                          where c != null
                          orderby c.Order, c.Name ?? p.Name 
                          select new { Property = p, Contract = c };

            bool first = true;
            
            yield return this.ReadConstant("{", reader);
            foreach (var m in members)
            {
                yield return WriteStartProperty(m.Property, m.Contract, first, reader);
                if (m.Property.PropertyType == typeof(string))
                {
                    yield return this.ReadConstant("\"", reader);
                    yield return this.ReadString(m.Property, instance, reader);
                    yield return this.ReadConstant("\"", reader);
                }
                else
                {
                    yield return ReadLiteral(m.Property, instance, reader);
                }

                first = false;
            }

            yield return this.ReadConstant("}", reader);
        }

        private Expression ReadLiteral(PropertyInfo property, ParameterExpression instance, ParameterExpression reader)
        {
            throw new NotImplementedException();
        }

        private Expression ReadString(PropertyInfo property, ParameterExpression instance, ParameterExpression reader)
        {
            throw new NotImplementedException();
        }

        private Expression ReadConstant(string text, ParameterExpression reader)
        {
            throw new NotImplementedException();
        }

        private Expression<Action<T, StreamWriter>> WriteObjectBlock()
        {
            var instance = Expression.Parameter(typeof(T), "i");
            var writer = Expression.Parameter(typeof(StreamWriter), "b");
            var block = Expression.Block(this.WriteObject(instance, writer));
            return (Expression<Action<T, StreamWriter>>)Expression.Lambda(block, instance, writer);
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
                    yield return this.WriteConstant("\"", writer);
                    yield return this.WriteString(m.Property, instance, writer);
                    yield return this.WriteConstant("\"", writer);
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

        private Expression WriteStartProperty(
            PropertyInfo property, DataMemberAttribute contract, bool first, ParameterExpression writer)
        {
            var buffer = first ? "\"" + (contract.Name ?? property.Name) + "\":" : ",\"" + (contract.Name ?? property.Name) + "\":";
            ////var buffer = this.encoding.GetBytes(start);
            return Expression.Call(writer, this.writeStringMethod, Expression.Constant(buffer));
        }

        private Expression WriteString(PropertyInfo property, ParameterExpression instance, ParameterExpression writer)
        {
            var getterCall = Expression.Call(instance, property.GetGetMethod());
            var replaceMethod = typeof(string).GetMethod("Replace", new[] { typeof(string), typeof(string) });
            var replaceCall = Expression.Call(getterCall, replaceMethod, Expression.Constant("\\"), Expression.Constant("\\\\"));
            var secondReplaceCall = Expression.Call(replaceCall, replaceMethod, Expression.Constant("\""), Expression.Constant("\\\""));
            return Expression.Call(writer, this.writeStringMethod, secondReplaceCall);
        }

        private Expression WriteLiteral(PropertyInfo property, ParameterExpression instance, ParameterExpression writer)
        {
            var getterCall = Expression.Call(instance, property.GetGetMethod());
            var writeMethod = typeof(StreamWriter).GetMethod("Write", new[] { property.PropertyType });
            return Expression.Call(writer, writeMethod, getterCall);
        }
    }
}
