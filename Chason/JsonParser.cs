namespace Chason
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Runtime.Serialization;
    using System.Text;

    /// <summary>
    /// This class encodes and decodes JSON strings.
    /// Spec. details, see http://www.json.org/
    /// 
    /// JSON uses Arrays and Objects. These correspond here to the datatypes ArrayList and Hashtable.
    /// All numbers are parsed to doubles.
    /// </summary>
    /// <summary>
    /// This class encodes and decodes JSON strings.
    /// Spec. details, see http://www.json.org/
    /// 
    /// JSON uses Arrays and Objects. These correspond here to the datatypes ArrayList and Hashtable.
    /// All numbers are parsed to doubles.
    /// </summary>
    public sealed class JsonParser
    {
        private enum Token
        {
            None = -1,           // Used to denote no Lookahead available
            CurlyOpen,
            CurlyClose,
            SquaredOpen,
            SquaredClose,
            Colon,
            Comma,
            String,
            Number,
            True,
            False,
            Null
        }

        private readonly char[] json;
        
        private readonly StringBuilder s = new StringBuilder();
        
        private Token lookAheadToken = Token.None;
        
        private int index;

        public JsonParser(string json)
        {
            this.json = json.ToCharArray();
        }

        public T Parse<T>() where T : class
        {
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

        private T ParseObject<T>() where T : class
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

        internal long[] ParseInt64Array()
        {
            return this.ParseArray(this.ParseInt64);
        }

        internal int[] ParseInt32Array()
        {
            return this.ParseArray(this.ParseInt32);
        }

        internal decimal[] ParseDecimalArray()
        {
            return this.ParseArray(this.ParseDecimal);
        }

        internal string[] ParseStringArray()
        {
            return this.ParseArray(this.ParseString);
        }

        internal T[] ParseArray<T>(Func<T> parse)
        {
            return ParseList<List<T>, T>(parse).ToArray();
        }

        internal TList ParseList<TList, TItem>(Func<TItem> parse) 
            where TList : IList<TItem>
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

        /*
        private T ParseValue<T>()
        {
            switch (this.LookAhead())
            {
                case Token.Number:
                    return (T)Convert.ChangeType(this.ParseNumber(), typeof(T));

                case Token.String:
                    return (T)Convert.ChangeType(this.ParseString(), typeof(T));

                case Token.CurlyOpen:
                    return ParseObject<T>();

                case Token.SquaredOpen:
                    return ParseArray<T>();

                case Token.True:
                    this.ConsumeToken();
                    return true;

                case Token.False:
                    this.ConsumeToken();
                    return false;

                case Token.Null:
                    this.ConsumeToken();
                    return default(T);
            }

            throw new SerializationException("Unrecognized token at index" + this.index);
        }
        */

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

                if (this.index == this.json.Length) break;

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
                            if (remainingLength < 4) break;

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

        private uint ParseUnicode(char c1, char c2, char c3, char c4)
        {
            uint p1 = this.ParseSingleChar(c1, 0x1000);
            uint p2 = this.ParseSingleChar(c2, 0x100);
            uint p3 = this.ParseSingleChar(c3, 0x10);
            uint p4 = this.ParseSingleChar(c4, 1);
            return p1 + p2 + p3 + p4;
        }

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
            } while (true);

            return new string(this.json, startIndex, this.index - startIndex);
        }

        internal long ParseInt64()
        {
            return long.Parse(this.ParseNumber());
        }

        internal int ParseInt32()
        {
            return int.Parse(this.ParseNumber());
        }

        internal decimal ParseDecimal()
        {
            return decimal.Parse(this.ParseNumber());
        }

        private Token LookAhead()
        {
            if (this.lookAheadToken != Token.None) return this.lookAheadToken;

            return this.lookAheadToken = this.NextTokenCore();
        }

        private void ConsumeToken()
        {
            this.lookAheadToken = Token.None;
        }

        private Token NextToken()
        {
            var result = this.lookAheadToken != Token.None ? this.lookAheadToken : this.NextTokenCore();
            this.lookAheadToken = Token.None;
            return result;
        }

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
            } while (++this.index < this.json.Length);

            if (this.index == this.json.Length)
            {
                throw new SerializationException("Reached end of string unexpectedly");
            }

            c = this.json[this.index];

            this.index++;

            //if (c >= '0' && c <= '9')
            //    return Token.Number;

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
                    if (this.json.Length - this.index >= 4 &&
                        this.json[this.index + 0] == 'a' &&
                        this.json[this.index + 1] == 'l' &&
                        this.json[this.index + 2] == 's' &&
                        this.json[this.index + 3] == 'e')
                    {
                        this.index += 4;
                        return Token.False;
                    }

                    break;

                case 't':
                    if (this.json.Length - this.index >= 3 &&
                        this.json[this.index + 0] == 'r' &&
                        this.json[this.index + 1] == 'u' &&
                        this.json[this.index + 2] == 'e')
                    {
                        this.index += 3;
                        return Token.True;
                    }

                    break;

                case 'n':
                    if (this.json.Length - this.index >= 3 &&
                        this.json[this.index + 0] == 'u' &&
                        this.json[this.index + 1] == 'l' &&
                        this.json[this.index + 2] == 'l')
                    {
                        this.index += 3;
                        return Token.Null;
                    }

                    break;

            }

            throw new SerializationException("Could not find token at index " + --this.index);
        }
    }
}