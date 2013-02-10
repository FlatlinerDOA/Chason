//--------------------------------------------------------------------------------------------------
// <copyright file="TryParseDelegate.cs" company="Andrew Chisholm">
//   Copyright (c) 2013 Andrew Chisholm All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------
namespace Chason
{
    /// <summary>
    /// Delegate contract for .NET's standard try parse methods
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="s"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    internal delegate bool TryParseDelegate<T>(string s, out T value) where T : struct;
}