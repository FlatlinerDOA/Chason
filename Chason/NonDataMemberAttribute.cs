//--------------------------------------------------------------------------------------------------
// <copyright file="NonDataMemberAttribute.cs" company="Andrew Chisholm">
//   Copyright (c) 2013 Andrew Chisholm All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------
namespace Chason
{
    using System;

    /// <summary>
    /// Marks a member that is explicitly not a data member and will not be considered for serialization.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class NonDataMemberAttribute : Attribute
    {
    }
}
