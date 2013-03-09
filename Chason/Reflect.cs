//--------------------------------------------------------------------------------------------------
// <copyright file="Reflect.cs" company="Andrew Chisholm">
//   Copyright (c) 2013 Andrew Chisholm All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------
namespace Chason
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Text;

    /// <summary>
    /// Provides common reflection functions
    /// </summary>
    internal static class Reflect
    {
        /// <summary>
        /// Gets the public fields and properties decorated with [DataMember] or not decorated if the object type is not a [DataContract] type.
        /// Excludes anything marked [NonDataMember]. All members are returned sorted by Order then Name.
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public static IEnumerable<MemberContractMap> GetObjectMemberContracts(Type objectType)
        {
            var isOptOut = !objectType.GetCustomAttributes(true).OfType<DataContractAttribute>().Any();            
            if (isOptOut)
            {
                int sortOrder = 0;
                return from m in objectType.GetFields(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public)
                              .Cast<MemberInfo>()
                              .Concat(objectType.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanWrite && p.CanRead))
                        where !m.GetCustomAttributes(true).OfType<NonDataMemberAttribute>().Any()
                       select new MemberContractMap(m, sortOrder++);
            }

            var members = from m in objectType.GetFields(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public)
                              .Cast<MemberInfo>()
                              .Concat(objectType.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public))
                          from c in m.GetCustomAttributes(true).OfType<DataMemberAttribute>()
                          where c != null
                          where !m.GetCustomAttributes(true).OfType<NonDataMemberAttribute>().Any()
                          orderby c.Order, c.Name ?? m.Name 
                          select new MemberContractMap(m, c);
            return members;
        }

        public static Type MemberType(this MemberInfo member)
        {
            return member is PropertyInfo ? ((PropertyInfo)member).PropertyType : ((FieldInfo)member).FieldType;   
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

        public static bool IsDictionary(this Type type)
        {
            if (!type.IsGenericType)
            {
                return false;
            }

            return typeof(IDictionary<,>).IsAssignableFrom(type.GetGenericTypeDefinition()) || 
                type.GetInterfaces().Any(t => typeof(IDictionary<,>).IsAssignableFrom(t.GetGenericTypeDefinition()));
        }

        public static bool IsCollection(this Type type)
        {
            if (!type.IsGenericType)
            {
                return false;
            }
            
            var generic = type.GetGenericTypeDefinition();
            return typeof(ICollection<>).IsAssignableFrom(generic) ||
                type.GetInterfaces().Any(t => typeof(ICollection<>).IsAssignableFrom(t.GetGenericTypeDefinition()));
        }
    }
}
