//--------------------------------------------------------------------------------------------------
// <copyright file="StringExtensions.cs" company="Andrew Chisholm">
//   Copyright (c) 2013 Andrew Chisholm All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------
namespace Chason.Extensions
{
    using System;
    using System.Linq.Expressions;
    using System.Text;

    /// <summary>
    /// Extension methods for working with strings
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// The list of escaped characters in JSON
        /// </summary>
        internal static readonly char[] CharsToEscape = new[] { '"', '\\', '\r', '\n', '\t', '/', '\b', '\f' };

        /// <summary>
        /// Converts a string to camel case (also ensures that abbreviations up to 3 characters long are camel cased correctly)
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string CamelCase(this string text)
        {
            if (text == null)
            {
                return null;
            }

            var sb = new StringBuilder();
            bool lowering = true;
            for (int i = 0; i < text.Length; i++)
            {
                if (lowering && char.IsUpper(text[i]) && i < 4)
                {
                    // Have we lowered at least one character and is the next char lower (then don't lower this one)
                    if (i > 0 && text.Length > i + 1 && char.IsLower(text[i + 1]))
                    {
                        lowering = false;
                        sb.Append(text[i]);
                    }
                    else
                    {
                        sb.Append(char.ToLower(text[i]));
                    }
                }
                else
                {
                    lowering = false;
                    sb.Append(text[i]);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// </summary>
        /// <param name="input">
        /// </param>
        /// <returns>
        /// </returns>
        public static string JsonEscapeString(this string input)
        {
            if (input == null)
            {
                return "null";
            }

            var sb = new StringBuilder();
            sb.Append('"');
            int lastIndex = 0;
            int index = input.IndexOfAny(CharsToEscape);
            if (index == -1)
            {
                sb.Append(input);
            }
            else
            {
                while (index != -1)
                {
                    sb.Append(input, lastIndex, index - lastIndex);
                    var c = input[index];
                    if (c == '"')
                    {
                        sb.Append(@"\""");
                    }
                    else if (c == '\\')
                    {
                        sb.Append(@"\\");
                    }
                    else if (c == '\r')
                    {
                        sb.Append(@"\r");
                    }
                    else if (c == '\n')
                    {
                        sb.Append(@"\n");
                    }
                    else if (c == '\t')
                    {
                        sb.Append(@"\t");
                    }
                    else if (c == '/')
                    {
                        sb.Append(@"\/");
                    }
                    else if (c == '\b')
                    {
                        sb.Append(@"\b");
                    }
                    else if (c == '\f')
                    {
                        sb.Append(@"\f");
                    }

                    index++;
                    lastIndex = index;
                    index = input.IndexOfAny(CharsToEscape, index);
                }

                sb.Append(input, lastIndex, input.Length - lastIndex);
            }

            sb.Append('"');
            return sb.ToString();
        }

        public static Expression CallJsonEscapeString(this Expression stringExpression)
        {
            var escapeMethod = typeof(StringExtensions).GetMethod("JsonEscapeString", new[] { typeof(string) });
            var escapeCall = Expression.Call(null, escapeMethod, new Expression[] { stringExpression });
            return escapeCall;
        }

        public static string Indent(int depth)
        {
            return new string(' ', depth * 2);
        }

        public static string LineFeedIndent(int depth)
        {
            return Environment.NewLine + new string(' ', depth * 2);
        }

    }
}
