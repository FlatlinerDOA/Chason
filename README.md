Chason
======

On a project I was working on, we noticed 30% of all our code execution was spent either serializing or deserializing JSON.
Clearly there was room for improvement. The goal of Chason (pronounced 'chase on') is to see if I can catch and even beat 
the best of the best JSON serializers out there.

Goals for the project are:
- Fast! (as fast as .NET will let us go)
- Easily customizable type output with pluggable formatters (including support for your own value types converted to and from JSON)
- Full polymorphic support with internal type to external name mapping.
- Optimize for strongly typed data contracts with an explicitly specified member order (to improve perf even more)
- Portable Library support (initially only .NET 4.5 and WinRT)

Initial Performance Results 
---------------------------
|Test|Iterations|Time (ms)|Objects / sec|
|:---|---------:|--------:|------------:|
|SerializeChason|1,000,000|2,904|344,270.2|
|SerializeServiceStack|1,000,000|3,186|313,813.0|
|SerializeJsonNet|1,000,000|5,756|173,708.6|
|SerializeFastJson|10,000|13,137|76,115.6|
|SerializeDataContractJson|1,000,000|5,961|167,747.2|
|DeserializeChason|1,000,000|2,827|353,664.2|
|DeserializeServiceStack|1,000,000|3,981|251,167.1|
|DeserializeJsonNet|1,000,000|10,835|92,289.5|
|DeserializeFastJson|1,000,000|3,245|308,082.7|
|DeserializeDataContractJson|1,000,000|16,214|61,675.1|

Note
----
- Tests are run with a thread-affinity set to a single core.
- Chason is unfinished and different test scenarios would probably fail.
- fastJSON serialization is quite fast in small iterations but became exponentially slow at 1,000,000 iterations so the iteration count was reduced to 10,000.
- Test is a single instance of a simple object being serialized / deserialized against common implementations
- Tests were run on a Core i7 2.66GHz MacBook Pro with 4G RAM and an SSD, on Windows 8 64bit. (Experience Index 5.9)


Some of the code is based on a heavily modified version of fastJSON for deserialization.

- http://fastjson.codeplex.com/ - For fastJSON's code.
- http://www.codeproject.com/KB/IP/fastJSON.aspx - Article outlining how fastJSON works.

The license for this is MIT.

The chase is on!

Andrew Chisholm