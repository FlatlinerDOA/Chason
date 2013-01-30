using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chason.PerformanceTests
{
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;
    using System.Threading;

    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();

            long seed = Environment.TickCount; 	// Prevents the JIT Compiler 
            // from optimizing Fkt calls away
            long result = 0;
            int count = 1000000;

            Console.WriteLine("2 Tests without correct preparation");
            Console.WriteLine("Warmup");
            for (int repeat = 0; repeat < 2; ++repeat)
            {
                stopwatch.Reset();
                stopwatch.Start();
                SerializeDataContract(seed, count);
                SerializeChason(seed, count);
                stopwatch.Stop();
                Console.WriteLine("Ticks: " + stopwatch.ElapsedTicks +
                " mS: " + stopwatch.ElapsedMilliseconds);
            }

            Process.GetCurrentProcess().ProcessorAffinity = new IntPtr(2); // Uses the second Core or Processor for the Test
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;  	// Prevents "Normal" processes 
            // from interrupting Threads
            Thread.CurrentThread.Priority = ThreadPriority.Highest;  	// Prevents "Normal" Threads 
            // from interrupting this thread

            stopwatch.Reset();
            stopwatch.Start();
            SerializeChason(seed, count);
            stopwatch.Stop();
            Console.WriteLine("SerializeChason: {0}", stopwatch.ElapsedMilliseconds);

            stopwatch.Reset();
            stopwatch.Start();
            SerializeDataContract(seed, count);
            stopwatch.Stop();

            Console.WriteLine("SerializeDataContractJson: {0}", stopwatch.ElapsedMilliseconds);
            stopwatch.Reset();
            stopwatch.Start();
            SerializeServiceStack(seed, count);
            stopwatch.Stop();
            Console.WriteLine("SerializeServiceStack: {0}", stopwatch.ElapsedMilliseconds);

            stopwatch.Reset();
            stopwatch.Start();
            SerializeFastJson(seed, count);
            stopwatch.Stop();
            Console.WriteLine("SerializeFastJSON: {0}", stopwatch.ElapsedMilliseconds);

            stopwatch.Reset();
            stopwatch.Start();
            ParseChason(seed, count);
            stopwatch.Stop();
            Console.WriteLine("ParseChason: {0}", stopwatch.ElapsedMilliseconds);

            stopwatch.Reset();
            stopwatch.Start();
            ParseFastJson(seed, count);
            stopwatch.Stop();
            Console.WriteLine("ParseFastJSON: {0}", stopwatch.ElapsedMilliseconds);

            Console.ReadKey();
        }

        private static void SerializeServiceStack(long seed, int count)
        {
            var s2 = new ServiceStack.Text.JsonSerializer<TestDataContract>();
            var m = new MemoryStream(8000);
            for (int i = 0; i < count; i++)
            {
                var t = new StreamWriter(m);
                s2.SerializeToWriter(testData, t);
                t.Flush();
                m.Position = 0;
            }
        }

        private static void ParseFastJson(long seed, int count)
        {
            for (int i = 0; i < 100000; i++)
            {
                var s2 = new fastJSON.JsonParser(TestJson);
                var x = s2.Decode();
            }
        }

        private static void ParseChason(long seed, int count)
        {
            for (int i = 0; i < 100000; i++)
            {
                var s2 = new Chason.JsonParser(new StringReader(TestJson));
                var x = s2.Parse();
            }
        }


        private static void SerializeFastJson(long seed, int count)
        {
            var s2 = new fastJSON.JSONSerializer(false, false, false, true, false);
            var m = new MemoryStream(8000);
            for (int i = 0; i < 10000; i++)
            {
                var t = new StreamWriter(m);
                t.Write(s2.ConvertToJSON(testData));
                t.Flush();
                m.Position = 0;
            }
        }

        private static readonly TestDataContract testData = new TestDataContract { FirstString = "First \"String\" ", SecondString = "Second \\ 'String' ", FirstInt = 1 };

        private static readonly string TestJson = "{\"FirstString\":\"First \\\"String\\\" \"}";

        private static void SerializeDataContract(long seed, int count)
        {
            var s2 = new DataContractJsonSerializer(typeof(TestDataContract));
            var m = new MemoryStream(8000);
            for (int i = 0; i < count; i++)
            {
                s2.WriteObject(m, testData);
                m.Position = 0;
            }
        }
        
        private static readonly Random RandomGenerator = new Random((int)DateTime.UtcNow.Ticks);

        private const int ObjectsCount = 100000;

        private static SimpleObject GetSimpleObject(long id)
        {
            return new SimpleObject
            {
                Name = string.Format("Simple-{0}", id),
                Id = RandomGenerator.Next(1, ObjectsCount),
                Address = "Planet Earth",
                Scores = Enumerable.Range(0, 10).Select(i => RandomGenerator.Next(1, 100)).ToArray()
            };
        }

        private static void SerializeChason(long seed, int count)
        {
            var s1 = new ChasonSerializer<TestDataContract>();
            var m = new MemoryStream(8000);
            for (int i = 0; i < count; i++)
            {
                var t = new StreamWriter(m);
                s1.Serialize(testData, t);
                t.Flush();
                m.Position = 0;
            }
        }
    }

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

    [DataContract]
    public class SimpleObject
    {
        [DataMember(Order = 1)]
        public int Id { get; set; }

        [DataMember(Order = 2)]
        public string Name { get; set; }

        [DataMember(Order = 3)]
        public string Address { get; set; }

        [DataMember(Order = 4)]
        public int[] Scores { get; set; }
    }
}
