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
        /// The string format for dates when parsing.
        /// </summary>
        private string dateTimeFormatString;

        /// <summary>
        /// The date time styles to use when parsing.
        /// </summary>
        private DateTimeStyles dateTimeStyles;

        /// <summary>
        /// The time span styles to use when parsing.
        /// </summary>
        private TimeSpanStyles timeSpanStyles;

        /// <summary>
        /// The string format for parsing time spans.
        /// </summary>
        private string timeSpanFormat;

        /// <summary>
        /// The string format for parsing date time offsets.
        /// </summary>
        private string dateTimeOffsetFormat;

        private IEqualityComparer<string> propertyNameComparer;

        private string typePropertyName;

        private string typeMarkerName;

        /// <summary>
        /// Initalizes a new instance of the <see cref="ChasonSerializerSettings"/> class.
        /// </summary>
        public ChasonSerializerSettings()
        {
            this.DateTimeFormat = "yyyy-MM-dd\\THH:mm:ss";
            this.DateTimeOffsetFormat = "yyyy-MM-dd\\THH:mm:ss.ffffffzzz";
            this.CultureInfo = CultureInfo.InvariantCulture;
            this.TextEncoding = Encoding.UTF8;
            this.KnownTypes = new HashSet<Type>();
            this.TimeSpanFormat = "c";
            this.TimeSpanStyles = TimeSpanStyles.None;
            this.DateTimeStyles = DateTimeStyles.None;
            this.PropertyNameComparer = StringComparer.Ordinal;
        }

        /// <summary>
        /// Initalizes a new instance of the <see cref="ChasonSerializerSettings"/> class.
        /// </summary>
        /// <param name="knownTypes">The list of known types supported for polymorphic data contracts</param>
        public ChasonSerializerSettings(params Type[] knownTypes)
            : this()
        {
            this.KnownTypes = new HashSet<Type>(knownTypes);
        }

        /// <summary>
        /// Gets the list of known polymorphic types that can be deserialized to.
        /// </summary>
        public HashSet<Type> KnownTypes { get; private set; }

        /// <summary>
        /// Gets the mapping from type name to strong type
        /// </summary>
        public IDictionary<string, Type> NameToTypeMapping { get; private set; }

        /// <summary>
        /// Gets the mapping from type to type name
        /// </summary>
        public IDictionary<Type, string> TypeToNameMapping { get; private set; } 

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
        /// Gets or sets the format string to output dates with. Defaults to 'yyyy-MM-ddThh:mm:ss' which is ISO-8601 format without offset information)
        /// </summary>
        public string DateTimeFormat
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

        public string DateTimeOffsetFormat
        {
            get
            {
                return this.dateTimeOffsetFormat;
            }

            set
            {
                this.EnsureNotReadOnly();
                this.dateTimeOffsetFormat = value;
            }
        }

        public DateTimeStyles DateTimeStyles
        {
            get
            {
                return this.dateTimeStyles;
            }
            set
            {
                this.EnsureNotReadOnly();
                this.dateTimeStyles = value;
            }
        }

        /// <summary>
        /// Gets or sets the time span styles (defaults to <see cref="TimeSpanStyles.None"/>).
        /// </summary>
        public TimeSpanStyles TimeSpanStyles
        {
            get
            {
                return this.timeSpanStyles;
            }

            set
            {
                this.EnsureNotReadOnly();
                this.timeSpanStyles = value;
            }
        }

        /// <summary>
        /// Gets or sets the time span format (defaults to 'c')
        /// </summary>
        public string TimeSpanFormat
        {
            get
            {
                return this.timeSpanFormat;
            }

            set
            {
                this.EnsureNotReadOnly();
                this.timeSpanFormat = value;
            }
        }

        /// <summary>
        /// Gets or sets the string comparer to use when reading property names (defaults to <see cref="StringComparer.Ordinal"/>)
        /// </summary>
        public IEqualityComparer<string> PropertyNameComparer
        {
            get
            {
                return this.propertyNameComparer;
            }

            set
            {
                this.EnsureNotReadOnly();
                this.propertyNameComparer = value;
            }
        }

        /// <summary>
        /// Gets or sets the member in the JSON that is used to identify the type to be deserialized. (defaults to '$type')
        /// </summary>
        public string TypeMarkerName
        {
            get
            {
                return this.typeMarkerName;
            }

            set
            {
                this.EnsureNotReadOnly();
                this.typeMarkerName = value;
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
            throw new NotImplementedException("This feature is coming!");
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
            throw new NotImplementedException("This feature is coming!");
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
            throw new NotImplementedException("This feature is coming!");
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