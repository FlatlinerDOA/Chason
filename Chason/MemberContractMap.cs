//--------------------------------------------------------------------------------------------------
// <copyright file="MemberContractMap.cs" company="Andrew Chisholm">
//   Copyright (c) 2013 Andrew Chisholm All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------
namespace Chason
{
    using System;
    using System.Reflection;
    using System.Runtime.Serialization;

    /// <summary>
    /// Mapping between a Member and a DataContract
    /// </summary>
    public sealed class MemberContractMap
    {
        public MemberInfo Member { get; set; }

        public DataMemberAttribute Contract { get; set; }
    }
}
