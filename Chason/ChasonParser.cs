// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChasonParser.cs" company="">
//   
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Chason
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Text;

    /// <summary>
    /// This class decodes JSON strings.
    /// Spec. details, see http://www.json.org/
    /// </summary>
    /// <remarks>Code taken from fastJSON implementation at fastjson.codeplex.com</remarks>
    internal sealed class ChasonParser
    {
        #region Static Fields

        /// <summary>
        /// </summary>
        internal static readonly Dictionary<Type, string> ListParseMethods = new Dictionary<Type, string> 
        {
            { typeof(IList<string>), "ParseStringList" }, 
            { typeof(IList<int>), "ParseInt32List" }, 
            { typeof(IList<long>), "ParseInt64List" }, 
            { typeof(IList<decimal>), "ParseDecimalList" }
        };

        /// <summary>
        /// </summary>
        internal static readonly Dictionary<Type, string> TypeParseMethods = new Dictionary<Type, string> 
        {
            { typeof(string), "ParseString" }, 
            { typeof(string[]), "ParseStringArray" }, 
            { typeof(short), "ParseInt16" }, 
            { typeof(short[]), "ParseInt16Array" }, 
            { typeof(int), "ParseInt32" }, 
            { typeof(int[]), "ParseInt32Array" }, 
            { typeof(long), "ParseInt64" }, 
            { typeof(long[]), "ParseInt64Array" }, 
            { typeof(ushort), "ParseUInt16" }, 
            { typeof(ushort[]), "ParseUInt16Array" },
            { typeof(uint), "ParseUInt32" }, 
            { typeof(uint[]), "ParseUInt32Array" },
            { typeof(ulong), "ParseUInt64" },
            { typeof(ulong[]), "ParseUInt64Array" },
            { typeof(float), "ParseFloat" }, 
            { typeof(float[]), "ParseFloatArray" }, 
            { typeof(double), "ParseDouble" }, 
            { typeof(double[]), "ParseDoubleArray" }, 
            { typeof(decimal), "ParseDecimal" }, 
            { typeof(decimal[]), "ParseDecimalArray" }, 
            { typeof(DateTime), "ParseDateTime" },
            { typeof(DateTime[]), "ParseDateTimeArray" },
            { typeof(DateTimeOffset), "ParseDateTimeOffset" },
            { typeof(DateTimeOffset[]), "ParseDateTimeOffsetArray" },
            { typeof(TimeSpan), "ParseTimeSpan" },
            { typeof(TimeSpan[]), "ParseTimeSpanArray" },
            { typeof(TimeZoneInfo), "ParseTimeZoneInfo" },
            { typeof(TimeZoneInfo[]), "ParseTimeZoneInfoArray" },
        };

        #endregion

        #region Fields

        /// <summary>
        /// </summary>
        private readonly char[] json;

        /// <summary>
        /// </summary>
        private readonly StringBuilder s = new StringBuilder();

        /// <summary>
        /// </summary>
        private readonly ChasonSerializerSettings settings;

        /// <summary>
        /// </summary>
        private int index;

        /// <summary>
        /// </summary>
        private Token lookAheadToken = Token.None;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// </summary>
        /// <param name="json">
        /// </param>
        /// <param name="settings">
        /// </param>
        public ChasonParser(string json, ChasonSerializerSettings settings)
        {
            this.settings = settings;
            this.json = json.ToCharArray();
        }

        #endregion

        #region Enums

        /// <summary>
        /// Represents the type of token being parsed
        /// </summary>
        private enum Token
        {
            /// <summary>
            /// Used to denote no Lookahead available
            /// </summary>
            None = -1,

            /// <summary>
            /// A curly open brace for the start of an object
            /// </summary>
            CurlyOpen, 

            /// <summary>
            /// A curly close brace for the end of an object
            /// </summary>
            CurlyClose, 

            /// <summary>
            /// A square open bracket for the start of an array
            /// </summary>
            SquaredOpen, 

            /// <summary>
            /// A square close bracket for the end of an array
            /// </summary>
            SquaredClose, 

            /// <summary>
            /// A colon between the property name and it's assigned value
            /// </summary>
            Colon, 

            /// <summary>
            /// The separator between array elements and object properties
            /// </summary>
            Comma, 

            /// <summary>
            /// A " symbol
            /// </summary>
            String, 

            /// <summary>
            /// A numeric digit or + or -
            /// </summary>
            Number, 

            /// <summary>
            /// The constant 'true'
            /// </summary>
            True, 

            /// <summary>
            /// The constant 'false'
            /// </summary>
            False, 

            /// <summary>
            /// The constant 'null'
            /// </summary>
            Null
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// </summary>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// </returns>
        public T Parse<T>()
        {
            ////var p = Expression.Parameter(typeof(ChasonParser));
            ////var methodCall = GetParseMethodCall(typeof(T), p);
            ////var lambda = Expression.Lambda<Func<ChasonParser, T>>(methodCall, p).Compile();
            ////return lambda(this);
            if (typeof(T).IsGenericType)
            {
                var generic = typeof(T).GetGenericTypeDefinition();
                if (typeof(IList<>).IsAssignableFrom(generic))
                {
                    var itemType = typeof(T).GetGenericArguments()[0];
                    var m = this.GetType().GetMethod(itemType.IsClass ? "ParseObjectArray" : "TODO");
                    var arrayParse = m.MakeGenericMethod(typeof(T), itemType);
                    return (T)arrayParse.Invoke(this, new object[0]);
                }
            }

            return this.ParseObject<T>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// </summary>
        /// <param name="type">
        /// </param>
        /// <param name="parserParameter">
        /// </param>
        /// <returns>
        /// </returns>
        internal static MethodCallExpression GetParseMethodCall(Type type, ParameterExpression parserParameter)
        {
            var underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)
            {
                // Convert(Call CreateDelegate(Type, null, MethodInfo))
                var tryParseDelegateType = typeof(TryParseDelegate<>).MakeGenericType(underlyingType);
                var methodInfo = underlyingType.GetMethods().FirstOrDefault(m => m.Name == "TryParse" && m.GetParameters().Length == 2);
                var createDelegate = Expression.Call(typeof(Delegate), "CreateDelegate", new Type[0], Expression.Constant(tryParseDelegateType), Expression.Constant(methodInfo));
                var convert = Expression.Convert(createDelegate, tryParseDelegateType);
                ////var arg = Expression.Constant(Expression.Convert(, methodInfo), tryParseDelegateType);
                Expression<Func<ChasonParser, int?>> x = p => p.ParseNullable(new TryParseDelegate<int>(int.TryParse));
                return Expression.Call(parserParameter, "ParseNullable", new[] { underlyingType }, convert);
            }

            if (TypeParseMethods.ContainsKey(type))
            {
                return Expression.Call(parserParameter, TypeParseMethods[type], new Type[0]);
            }

            if (type.IsGenericType)
            {
                var generic = type.GetGenericTypeDefinition();
                if (typeof(IList<>).IsAssignableFrom(generic))
                {
                    return Expression.Call(parserParameter, "ParseList", new[] { type });
                }
            }

            return Expression.Call(parserParameter, "ParseObject", new[] { type });
        }

        /// <summary>
        /// </summary>
        /// <param name="parse">
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// </returns>
        internal T[] ParseArray<T>(Func<T> parse)
        {
            return this.ParseList<List<T>, T>(parse).ToArray();
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        internal decimal ParseDecimal()
        {
            return decimal.Parse(this.ParseNumber());
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        internal decimal[] ParseDecimalArray()
        {
            return this.ParseArray(this.ParseDecimal);
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        internal int ParseInt32()
        {
            return int.Parse(this.ParseNumber());
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        internal uint ParseUInt32()
        {
            return uint.Parse(this.ParseNumber());
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        internal uint[] ParseUInt32Array()
        {
            return this.ParseArray(this.ParseUInt32);
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        internal ulong ParseUInt64()
        {
            return ulong.Parse(this.ParseNumber());
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        internal ulong[] ParseUInt64Array()
        {
            return this.ParseArray(this.ParseUInt64);
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        internal float ParseFloat()
        {
            return float.Parse(this.ParseNumber());
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        internal float[] ParseFloatArray()
        {
            return this.ParseArray(this.ParseFloat);
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        internal double ParseDouble()
        {
            return double.Parse(this.ParseNumber());
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        internal double[] ParseDoubleArray()
        {
            return this.ParseArray(this.ParseDouble);
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        internal short ParseInt16()
        {
            return short.Parse(this.ParseNumber());
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        internal short[] ParseInt16Array()
        {
            return this.ParseArray(this.ParseInt16);
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        internal ushort ParseUInt16()
        {
            return ushort.Parse(this.ParseNumber());
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        internal ushort[] ParseUInt16Array()
        {
            return this.ParseArray(this.ParseUInt16);
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        internal int[] ParseInt32Array()
        {
            return this.ParseArray(this.ParseInt32);
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        internal long ParseInt64()
        {
            return long.Parse(this.ParseNumber());
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        internal long[] ParseInt64Array()
        {
            return this.ParseArray(this.ParseInt64);
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        internal DateTime ParseDateTime()
        {
            var st = this.ParseString();
            return DateTime.ParseExact(st, this.settings.DateTimeFormat, this.settings.CultureInfo, this.settings.DateTimeStyles);
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        internal DateTime[] ParseDateTimeArray()
        {
            return this.ParseArray(this.ParseDateTime);
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        internal DateTimeOffset ParseDateTimeOffset()
        {
            var dt = this.ParseString();
            return DateTimeOffset.ParseExact(dt, this.settings.DateTimeOffsetFormat, this.settings.CultureInfo, this.settings.DateTimeStyles);
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        internal DateTimeOffset[] ParseDateTimeOffsetArray()
        {
            return this.ParseArray(this.ParseDateTimeOffset);
        }


        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        internal TimeSpan ParseTimeSpan()
        {
            var ts = this.ParseString();
            return TimeSpan.ParseExact(ts, this.settings.TimeSpanFormat, this.settings.CultureInfo, this.settings.TimeSpanStyles);
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        internal TimeSpan[] ParseTimeSpanArray()
        {
            return this.ParseArray(this.ParseTimeSpan);
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        internal TimeZoneInfo ParseTimeZoneInfo()
        {
            throw new NotSupportedException("Not supported by WinRT");
            ////return TimeZoneInfo.froms(this.ParseString());
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        internal TimeZoneInfo[] ParseTimeZoneInfoArray()
        {
            return this.ParseArray(this.ParseTimeZoneInfo);
        }

        /// <summary>
        /// </summary>
        /// <param name="parse">
        /// </param>
        /// <typeparam name="TList">
        /// </typeparam>
        /// <typeparam name="TItem">
        /// </typeparam>
        /// <returns>
        /// </returns>
        internal TList ParseList<TList, TItem>(Func<TItem> parse) where TList : IList<TItem>
        {
            var array = PropertyParseList<TList>.Instance.New();
            this.ConsumeToken(); // [

            while (true)
            {
                switch (this.LookAhead())
                {
                    case Token.Comma:
                        this.ConsumeToken();
                        break;

                    case Token.SquaredClose:
                        this.ConsumeToken();
                        return array;

                    default:
                        array.Add(parse());
                        break;
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="tryParseMethod">
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// </returns>
        internal T? ParseNullable<T>(TryParseDelegate<T> tryParseMethod) where T : struct
        {
            var s = this.ParseString();
            if (s == null)
            {
                return null;
            }

            T output;
            if (tryParseMethod(s, out output))
            {
                return output;
            }

            return null;
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        internal string[] ParseStringArray()
        {
            return this.ParseArray(this.ParseString);
        }

        /// <summary>
        /// </summary>
        private void ConsumeToken()
        {
            this.lookAheadToken = Token.None;
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        private Token LookAhead()
        {
            if (this.lookAheadToken != Token.None)
            {
                return this.lookAheadToken;
            }

            return this.lookAheadToken = this.NextTokenCore();
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        private Token NextToken()
        {
            var result = this.lookAheadToken != Token.None ? this.lookAheadToken : this.NextTokenCore();
            this.lookAheadToken = Token.None;
            return result;
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        /// <exception cref="SerializationException">
        /// </exception>
        private Token NextTokenCore()
        {
            char c;

            // Skip past whitespace
            do
            {
                c = this.json[this.index];

                if (c > ' ')
                {
                    break;
                }

                if (c != ' ' && c != '\t' && c != '\n' && c != '\r')
                {
                    break;
                }
            }
            while (++this.index < this.json.Length);

            if (this.index == this.json.Length)
            {
                throw new SerializationException("Reached end of string unexpectedly");
            }

            c = this.json[this.index];

            this.index++;

            // if (c >= '0' && c <= '9')
            // return Token.Number;
            switch (c)
            {
                case '{':
                    return Token.CurlyOpen;

                case '}':
                    return Token.CurlyClose;

                case '[':
                    return Token.SquaredOpen;

                case ']':
                    return Token.SquaredClose;

                case ',':
                    return Token.Comma;

                case '"':
                    return Token.String;

                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case '-':
                case '+':
                case '.':
                    return Token.Number;

                case ':':
                    return Token.Colon;

                case 'f':
                    if (this.json.Length - this.index >= 4 && this.json[this.index + 0] == 'a' && this.json[this.index + 1] == 'l' && this.json[this.index + 2] == 's' && this.json[this.index + 3] == 'e')
                    {
                        this.index += 4;
                        return Token.False;
                    }

                    break;

                case 't':
                    if (this.json.Length - this.index >= 3 && this.json[this.index + 0] == 'r' && this.json[this.index + 1] == 'u' && this.json[this.index + 2] == 'e')
                    {
                        this.index += 3;
                        return Token.True;
                    }

                    break;

                case 'n':
                    if (this.json.Length - this.index >= 3 && this.json[this.index + 0] == 'u' && this.json[this.index + 1] == 'l' && this.json[this.index + 2] == 'l')
                    {
                        this.index += 3;
                        return Token.Null;
                    }

                    break;
            }

            throw new SerializationException("Could not find token at index " + --this.index);
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        /// <exception cref="SerializationException">
        /// </exception>
        private string ParseNumber()
        {
            this.ConsumeToken();

            // Need to start back one place because the first digit is also a token and would have been consumed
            var startIndex = this.index - 1;

            do
            {
                var c = this.json[this.index];

                if ((c >= '0' && c <= '9') || c == '.' || c == '-' || c == '+' || c == 'e' || c == 'E')
                {
                    if (++this.index == this.json.Length)
                    {
                        throw new SerializationException("Unexpected end of string whilst parsing number");
                    }

                    continue;
                }

                break;
            }
            while (true);

            return new string(this.json, startIndex, this.index - startIndex);
        }

        /// <summary>
        /// </summary>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// </returns>
        /// <exception cref="SerializationException">
        /// </exception>
        private T ParseObject<T>()
        {
            var instanceParser = PropertyParseList<T>.Instance;

            var instance = instanceParser.New();
            if (this.LookAhead() != Token.CurlyOpen)
            {
                throw new SerializationException("Expected curly open at index " + this.index);
            }

            this.ConsumeToken(); // {

            while (true)
            {
                switch (this.LookAhead())
                {
                    case Token.Comma:
                        this.ConsumeToken();
                        break;

                    case Token.CurlyClose:
                        this.ConsumeToken();
                        return instance;

                    default:
                        {
                            // name
                            string name = this.ParseString();

                            // :
                            if (this.NextToken() != Token.Colon)
                            {
                                throw new SerializationException("Expected colon at index " + this.index);
                            }

                            this.LookAhead();
                            instanceParser.Parse(this, instance, name);

                            //// value
                            ////object value = ParseValue();

                            ////table[name] = value;
                        }

                        break;
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="c1">
        /// </param>
        /// <param name="multipliyer">
        /// </param>
        /// <returns>
        /// </returns>
        private uint ParseSingleChar(char c1, uint multipliyer)
        {
            uint p1 = 0;
            if (c1 >= '0' && c1 <= '9')
            {
                p1 = (uint)(c1 - '0') * multipliyer;
            }
            else if (c1 >= 'A' && c1 <= 'F')
            {
                p1 = (uint)((c1 - 'A') + 10) * multipliyer;
            }
            else if (c1 >= 'a' && c1 <= 'f')
            {
                p1 = (uint)((c1 - 'a') + 10) * multipliyer;
            }

            return p1;
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        /// <exception cref="SerializationException">
        /// </exception>
        private string ParseString()
        {
            this.ConsumeToken(); // "

            this.s.Length = 0;

            int runIndex = -1;

            while (this.index < this.json.Length)
            {
                var c = this.json[this.index++];

                if (c == '"')
                {
                    if (runIndex != -1)
                    {
                        if (this.s.Length == 0)
                        {
                            return new string(this.json, runIndex, this.index - runIndex - 1);
                        }

                        this.s.Append(this.json, runIndex, this.index - runIndex - 1);
                    }

                    return this.s.ToString();
                }

                if (c != '\\')
                {
                    if (runIndex == -1)
                    {
                        runIndex = this.index - 1;
                    }

                    continue;
                }

                if (this.index == this.json.Length)
                {
                    break;
                }

                if (runIndex != -1)
                {
                    this.s.Append(this.json, runIndex, this.index - runIndex - 1);
                    runIndex = -1;
                }

                switch (this.json[this.index++])
                {
                    case '"':
                        this.s.Append('"');
                        break;

                    case '\\':
                        this.s.Append('\\');
                        break;

                    case '/':
                        this.s.Append('/');
                        break;

                    case 'b':
                        this.s.Append('\b');
                        break;

                    case 'f':
                        this.s.Append('\f');
                        break;

                    case 'n':
                        this.s.Append('\n');
                        break;

                    case 'r':
                        this.s.Append('\r');
                        break;

                    case 't':
                        this.s.Append('\t');
                        break;

                    case 'u':
                        {
                            int remainingLength = this.json.Length - this.index;
                            if (remainingLength < 4)
                            {
                                break;
                            }

                            // parse the 32 bit hex into an integer codepoint
                            uint codePoint = this.ParseUnicode(this.json[this.index], this.json[this.index + 1], this.json[this.index + 2], this.json[this.index + 3]);
                            this.s.Append((char)codePoint);

                            // skip 4 chars
                            this.index += 4;
                        }

                        break;
                }
            }

            throw new SerializationException("Unexpectedly reached end of string");
        }

        /// <summary>
        /// </summary>
        /// <param name="c1">
        /// </param>
        /// <param name="c2">
        /// </param>
        /// <param name="c3">
        /// </param>
        /// <param name="c4">
        /// </param>
        /// <returns>
        /// </returns>
        private uint ParseUnicode(char c1, char c2, char c3, char c4)
        {
            uint p1 = this.ParseSingleChar(c1, 0x1000);
            uint p2 = this.ParseSingleChar(c2, 0x100);
            uint p3 = this.ParseSingleChar(c3, 0x10);
            uint p4 = this.ParseSingleChar(c4, 1);
            return p1 + p2 + p3 + p4;
        }

        #endregion
    }
}