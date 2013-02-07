namespace Chason
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;

    /// <summary>
    /// Used to configure Chason to your specific needs.
    /// </summary>
    public sealed class ChasonSerializerSettings
    {
        /// <summary>
        /// Gets the default settings
        /// </summary>
        public static readonly ChasonSerializerSettings Default = new ChasonSerializerSettings();

        /// <summary>
        /// A value indicating whether these settings are now read only
        /// </summary>
        private bool readOnly;

        /// <summary>
        /// The text encoding
        /// </summary>
        private Encoding textEncoding;

        /// <summary>
        /// The culture to use
        /// </summary>
        private CultureInfo cultureInfo;

        /// <summary>
        /// The format string for dates
        /// </summary>
        private string dateTimeFormatString;

        /// <summary>
        /// Initalizes a new instance of the <see cref="ChasonSerializerSettings"/> class.
        /// </summary>
        public ChasonSerializerSettings()
        {
            this.DateTimeFormatString = "o";
            this.CultureInfo = CultureInfo.InvariantCulture;
            this.TextEncoding = Encoding.UTF8;
            this.KnownTypes = new List<Type>();
        }

        /// <summary>
        /// Initalizes a new instance of the <see cref="ChasonSerializerSettings"/> class.
        /// </summary>
        /// <param name="knownTypes">The list of known types supported for polymorphic data contracts</param>
        public ChasonSerializerSettings(params Type[] knownTypes)
            : this()
        {
            this.KnownTypes = new List<Type>(knownTypes);
        }

        /// <summary>
        /// Gets the list of known polymorphic types that can be deserialized to.
        /// </summary>
        public IList<Type> KnownTypes { get; private set; }

        /// <summary>
        /// Gets or sets the text encoding to use when working with streams (defaults to UTF8)
        /// </summary>
        public Encoding TextEncoding
        {
            get
            {
                return this.textEncoding;
            }

            set
            {
                this.EnsureNotReadOnly();
                this.textEncoding = value;
            }
        }

        /// <summary>
        /// Gets or sets the culture info to use when parsing and formatting culture specific values such as dates (defaults to InvariantCulture)
        /// </summary>
        public CultureInfo CultureInfo
        {
            get
            {
                return this.cultureInfo;
            }

            set
            {
                this.EnsureNotReadOnly();
                this.cultureInfo = value;
            }
        }

        /// <summary>
        /// Gets or sets the format string to output dates with (defaults to 'o' or ISO-8601 format)
        /// </summary>
        public string DateTimeFormatString
        {
            get
            {
                return this.dateTimeFormatString;
            }

            set
            {
                this.EnsureNotReadOnly();
                this.dateTimeFormatString = value;
            }
        }

        /// <summary>
        /// Override how Chason outputs a particular type to and from Json by specifying the JSON output or parsing the text input yourself.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toString">Expression to convert a instance of a type to the JSON literal string.</param>
        /// <param name="fromString">Expression to convert a instance of a type to the JSON literal string.</param>
        public void AddCustomStringFormatter<T>(Expression<Func<T, string>> toString, Expression<Func<string, T>> fromString)
        {
            this.EnsureNotReadOnly();
            throw new NotSupportedException("This feature is coming!");
        }

        /// <summary>
        /// Override how Chason outputs a particular type to and from Json by specifying the JSON output or parsing the number input yourself.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toNumber"></param>
        /// <param name="fromNumber"></param>
        public void AddCustomNumberFormatter<T>(Expression<Func<T, decimal>> toNumber, Expression<Func<decimal, T>> fromNumber)
        {
            this.EnsureNotReadOnly();
            throw new NotSupportedException("This feature is coming!");
        }

        /// <summary>
        /// Override how Chason outputs a particular type to and from Json by specifying the JSON output or parsing the JSON input yourself.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toString">Expression to convert a instance of a type to the JSON literal string. Note: You must output the Quotes or Curly braces yourself if your type is string or object based etc.</param>
        /// <param name="fromString">Expression to convert a instance of a type to the JSON literal string. Note: You must parse the Quotes or Curly braces yourself if your type is string or object based etc.</param>
        public void AddCustomObjectFormatter<T>(Expression<Func<T, string>> toString, Expression<Func<string, T>> fromString)
        {
            this.EnsureNotReadOnly();
            throw new NotSupportedException("This feature is coming!");
        }

        /// <summary>
        /// Locks the settings so that no further modifications can be made to them.
        /// </summary>
        public void Lock()
        {
            if (!this.readOnly)
            {
                this.KnownTypes = new ReadOnlyCollection<Type>(this.KnownTypes);
                this.readOnly = true;
            }
        }

        /// <summary>
        /// Verifies this instance hasn't been marked read-only
        /// </summary>
        private void EnsureNotReadOnly()
        {
            if (this.readOnly)
            {
                throw new InvalidOperationException("The serializer settings have been locked for use, please make sure you finish configuring the settings before using them, or create a new instance.");
            }
        }
    }
}