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
            var members = from m in objectType.GetFields(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public)
                              .Cast<MemberInfo>()
                              .Concat(objectType.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public))
                          from c in m.GetCustomAttributes(true).OfType<DataMemberAttribute>()
                          where isOptOut || c != null
                          where !m.GetCustomAttributes(true).OfType<NonDataMemberAttribute>().Any()
                          orderby c.Order, c.Name ?? m.Name 
                          select new MemberContractMap { Member = m, Contract = c };
            return members;
        }

        public static Type MemberType(this MemberInfo member)
        {
            return member is PropertyInfo ? ((PropertyInfo)member).PropertyType : ((FieldInfo)member).FieldType;   
        }
    }
}
