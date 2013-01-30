using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chason
{
    using System.IO;

    /// <summary>
    /// This class encodes and decodes JSON strings.
    /// Spec. details, see http://www.json.org/
    /// 
    /// JSON uses Arrays and Objects. These correspond here to the datatypes ArrayList and Hashtable.
    /// All numbers are parsed to doubles.
    /// </summary>
    public class JsonParser
    {
        enum Token
        {
            None = -1,           // Used to denote no Lookahead available
            Curly_Open,
            Curly_Close,
            Squared_Open,
            Squared_Close,
            Colon,
            Comma,
            String,
            Number,
            True,
            False,
            Null
        }

        private readonly char[] json;
        
        readonly StringBuilder s = new StringBuilder();

        private Token lookAheadToken = Token.None;

        private const int KeepAhead = 5;

        private char lastChar = '\0';

        int index;

        private readonly TextReader reader;

        private int bufferSize;

        private bool endOfStream;

        public JsonParser(TextReader reader) : this(reader, 2000)
        {
        }

        public JsonParser(TextReader reader, int bufferSize)
        {
            if (bufferSize <= KeepAhead)
            {
                throw new ArgumentOutOfRangeException("bufferSize", "Buffer size must be greater than " + KeepAhead + " characters");
            }

            this.reader = reader;
            this.json = new char[bufferSize];
            this.FillBuffer();
        }

        public object Parse()
        {
            return ParseValue();
        }

        
        private bool FillBuffer()
        {
            if (this.endOfStream)
            {
                return false;
            }

            if (this.bufferSize != 0)
            {
                for (int i = 0; i < KeepAhead; i++)
                {
                    this.json[i] = this.json[this.bufferSize + i];
                }

                var readCount = this.json.Length - KeepAhead;
                this.bufferSize = this.reader.Read(this.json, KeepAhead, readCount);
                if (this.bufferSize < readCount)
                {
                    this.endOfStream = true;
                }
            }
            else
            {
                var readCount = this.json.Length - this.bufferSize;
                this.bufferSize = this.reader.Read(this.json, 0, readCount);
                if (this.bufferSize < readCount)
                {
                    this.endOfStream = true;
                }
            }

            if (this.bufferSize != -1)
            {
                this.bufferSize = this.bufferSize > KeepAhead ? this.bufferSize - KeepAhead : this.bufferSize;
                this.index = 0;
                return true;
            }

            return false;
        }

        private Dictionary<string, object> ParseObject()
        {
            Dictionary<string, object> table = new Dictionary<string, object>();

            ConsumeToken(); // {

            while (true)
            {
                switch (LookAhead())
                {

                    case Token.Comma:
                        ConsumeToken();
                        break;

                    case Token.Curly_Close:
                        ConsumeToken();
                        return table;

                    default:
                        {

                            // name
                            string name = ParseString();

                            // :
                            if (NextToken() != Token.Colon)
                            {
                                throw new Exception("Expected colon at index " + index);
                            }

                            // value
                            object value = ParseValue();

                            table[name] = value;
                        }
                        break;
                }
            }
        }

        private List<object> ParseArray()
        {
            var array = new List<object>();
            ConsumeToken(); // [

            while (true)
            {
                switch (LookAhead())
                {
                    case Token.Comma:
                        ConsumeToken(); // ,
                        break;

                    case Token.Squared_Close:
                        ConsumeToken(); // ]
                        return array;

                    default:
                        {
                            array.Add(ParseValue());
                        }
                        break;
                }
            }
        }

        private object ParseValue()
        {
            switch (LookAhead())
            {
                case Token.Number:
                    return ParseNumber();

                case Token.String:
                    return ParseString();

                case Token.Curly_Open:
                    return ParseObject();

                case Token.Squared_Open:
                    return ParseArray();

                case Token.True:
                    ConsumeToken();
                    return true;

                case Token.False:
                    ConsumeToken();
                    return false;

                case Token.Null:
                    ConsumeToken();
                    return null;
            }

            throw new Exception("Unrecognized token at index" + index);
        }

        public char[] Buffer
        {
            get
            {
                var b = new char[this.bufferSize + KeepAhead];
                Array.ConstrainedCopy(this.json, 0, b, 0, this.bufferSize + KeepAhead);
                return b;
            }
        }

        public bool MoveNext()
        {
            this.lastChar = this.json[this.index];
            this.index++;
            if (this.index >= this.bufferSize)
            {
                if (!this.FillBuffer())
                {
                    return this.index < this.bufferSize + KeepAhead;
                }
            }

            return true;
        }

        public bool MoveNext(int offset)
        {
            if (offset > KeepAhead)
            {
                throw new ArgumentOutOfRangeException("offset", "Offset cannot be further than the number of characters we read ahead.");
            }

            this.lastChar = this.json[this.index];
            this.index += offset;
            if (this.index >= this.bufferSize)
            {
                if (!this.FillBuffer())
                {
                    this.index += offset;
                    return this.index < this.bufferSize + KeepAhead;
                }

                return true;
            }

            return true;
        }

        private string ParseString()
        {
            ConsumeToken(); // "

            s.Length = 0;

            int runIndex = -1;
            while (true)
            {
                var c = json[index];
                if (!this.MoveNext())
                {
                    break;
                }

                if (c == '"')
                {
                    if (runIndex != -1)
                    {
                        if (s.Length == 0)
                        {
                            return new string(json, runIndex, index - runIndex - 1);
                        }

                        s.Append(json, runIndex, index - runIndex - 1);
                    }

                    return s.ToString();
                }

                if (c != '\\')
                {
                    if (runIndex == -1)
                    {
                        runIndex = index - 1;
                    }

                    continue;
                }

                if (index == json.Length) break;

                if (runIndex != -1)
                {
                    s.Append(json, runIndex, index - runIndex - 1);
                    runIndex = -1;
                }

                switch (json[index++])
                {
                    case '"':
                        s.Append('"');
                        break;

                    case '\\':
                        s.Append('\\');
                        break;

                    case '/':
                        s.Append('/');
                        break;

                    case 'b':
                        s.Append('\b');
                        break;

                    case 'f':
                        s.Append('\f');
                        break;

                    case 'n':
                        s.Append('\n');
                        break;

                    case 'r':
                        s.Append('\r');
                        break;

                    case 't':
                        s.Append('\t');
                        break;

                    case 'u':
                        {
                            int remainingLength = json.Length - index;
                            if (remainingLength < 4) break;

                            // parse the 32 bit hex into an integer codepoint
                            uint codePoint = ParseUnicode(json[index], json[index + 1], json[index + 2], json[index + 3]);
                            s.Append((char)codePoint);

                            // skip 4 chars
                            index += 4;
                        }
                        break;
                }
            }

            throw new Exception("Unexpectedly reached end of string");
        }

        private uint ParseSingleChar(char c1, uint multiplyer)
        {
            uint p1 = 0;
            if (c1 >= '0' && c1 <= '9')
                p1 = (uint)(c1 - '0') * multiplyer;
            else if (c1 >= 'A' && c1 <= 'F')
                p1 = (uint)((c1 - 'A') + 10) * multiplyer;
            else if (c1 >= 'a' && c1 <= 'f')
                p1 = (uint)((c1 - 'a') + 10) * multiplyer;
            return p1;
        }

        private uint ParseUnicode(char c1, char c2, char c3, char c4)
        {
            uint p1 = ParseSingleChar(c1, 0x1000);
            uint p2 = ParseSingleChar(c2, 0x100);
            uint p3 = ParseSingleChar(c3, 0x10);
            uint p4 = ParseSingleChar(c4, 1);

            return p1 + p2 + p3 + p4;
        }

        private string ParseNumber()
        {
            ConsumeToken();
            var sb = new StringBuilder(10);

            // Need to get the last char because the first digit is also a token and would have been consumed
            sb.Append(this.lastChar);

            do
            {
                var c = json[index];
                if ((c >= '0' && c <= '9') || c == '.' || c == '-' || c == '+' || c == 'e' || c == 'E')
                {
                    sb.Append(c);
                    if (!this.MoveNext())
                    {
                        throw new Exception("Unexpected end of string whilst parsing number");
                    }

                    continue;
                }

                break;
            } while (true);

            return sb.ToString();
        }

        private Token LookAhead()
        {
            if (lookAheadToken != Token.None) return lookAheadToken;

            return lookAheadToken = NextTokenCore();
        }

        private void ConsumeToken()
        {
            lookAheadToken = Token.None;
        }

        private Token NextToken()
        {
            var result = lookAheadToken != Token.None ? lookAheadToken : NextTokenCore();

            lookAheadToken = Token.None;

            return result;
        }

        private Token NextTokenCore()
        {
            char c;

            // Skip past whitespace
            do
            {
                c = json[index];
                if (c > ' ') break;
                if (c != ' ' && c != '\n' && c != '\r' && c != '\t') break;

            } while (this.MoveNext());

            this.MoveNext();
            
            switch (c)
            {
                case '{':
                    return Token.Curly_Open;

                case '}':
                    return Token.Curly_Close;

                case '[':
                    return Token.Squared_Open;

                case ']':
                    return Token.Squared_Close;

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
                    if (json.Length - index >= 4 &&
                        json[index] == 'a' &&
                        json[index + 1] == 'l' &&
                        json[index + 2] == 's' &&
                        json[index + 3] == 'e')
                    {
                        index += 4;
                        return Token.False;
                    }
                    break;

                case 't':
                    if (json.Length - index >= 3 &&
                        json[index] == 'r' &&
                        json[index + 1] == 'u' &&
                        json[index + 2] == 'e')
                    {
                        index += 3;
                        return Token.True;
                    }
                    break;

                case 'n':
                    if (json.Length - index >= 3 &&
                        json[index] == 'u' &&
                        json[index + 1] == 'l' &&
                        json[index + 2] == 'l')
                    {
                        index += 3;
                        return Token.Null;
                    }
                    break;

            }

            throw new Exception("Could not find token at index " + --index);
        }
    }
}
