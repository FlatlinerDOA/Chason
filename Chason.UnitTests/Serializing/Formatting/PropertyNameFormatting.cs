//--------------------------------------------------------------------------------------------------
// <copyright file="PropertyNameFormatting.cs" company="Andrew Chisholm">
//   Copyright (c) 2013 Andrew Chisholm All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------
namespace Chason.UnitTests.Serializing.Formatting
{
    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class PropertyNameFormatting
    {
        [TestMethod]
        public void CamelCasingAbbreviationsUpToFourCharactersLong()
        {
            ChasonSerializer.CamelCase("ACLSettings").Should().Be("aclSettings");
        }

        [TestMethod]
        public void CamelCasingASingleWord()
        {
            ChasonSerializer.CamelCase("Test").Should().Be("test");
        }

        [TestMethod]
        public void CamelCasingAllCapitals()
        {
            ChasonSerializer.CamelCase("TEST").Should().Be("test");
        }

        [TestMethod]
        public void CamelCasingPascalCasing()
        {
            ChasonSerializer.CamelCase("PascalCasing").Should().Be("pascalCasing");
        }

        [TestMethod]
        public void CamelCasingShortPascalCasing()
        {
            ChasonSerializer.CamelCase("MyCasing").Should().Be("myCasing");
        }

        [TestMethod]
        public void CamelCasingSomethingAlreadyCamelCased()
        {
            ChasonSerializer.CamelCase("camelCasing").Should().Be("camelCasing");
        }
    }
}
