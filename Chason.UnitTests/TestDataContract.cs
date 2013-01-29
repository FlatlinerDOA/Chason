namespace Chason.UnitTests
{
    using System;
    using System.Runtime.Serialization;

    [DataContract(Name = "Test", Namespace = "abc")]
    public sealed class TestDataContract
    {
        [DataMember]
        public string FirstString { get; set; }

        [DataMember]
        public string SecondString { get; set; }

        [DataMember]
        public int FirstInt { get; set; }

        ////[DataMember]
        ////public int? NullableInt { get; set; }

        ////[DataMember]
        ////public DateTime FirstDate { get; set; }

        ////[DataMember]
        ////public DateTime SecondDate { get; set; }
    }
}