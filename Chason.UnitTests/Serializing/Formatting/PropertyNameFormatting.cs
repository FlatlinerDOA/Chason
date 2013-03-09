//--------------------------------------------------------------------------------------------------
// <copyright file="PropertyNameFormatting.cs" company="Andrew Chisholm">
//   Copyright (c) 2013 Andrew Chisholm All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------
namespace Chason.UnitTests.Serializing.Formatting
{
    using Chason.Extensions;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class PropertyNameFormatting
    {
        [TestMethod]
        public void CamelCasingAbbreviationsUpToFourCharactersLong()
        {
            StringExtensions.CamelCase("ACLSettings").Should().Be("aclSettings");
        }

        [TestMethod]
        public void CamelCasingASingleWord()
        {
            StringExtensions.CamelCase("Test").Should().Be("test");
        }

        [TestMethod]
        public void CamelCasingAllCapitals()
        {
            StringExtensions.CamelCase("TEST").Should().Be("test");
        }

        [TestMethod]
        public void CamelCasingPascalCasing()
        {
            StringExtensions.CamelCase("PascalCasing").Should().Be("pascalCasing");
        }

        [TestMethod]
        public void CamelCasingShortPascalCasing()
        {
            StringExtensions.CamelCase("MyCasing").Should().Be("myCasing");
        }

        [TestMethod]
        public void CamelCasingSomethingAlreadyCamelCased()
        {
            StringExtensions.CamelCase("camelCasing").Should().Be("camelCasing");
        }
    }
}
