Chason
======

On a project I was working on, we noticed 30% of all our code execution was spent either serializing or deserializing JSON.
Clearly there was room for improvement. The goal of Chason (pronounced 'chase on') is to see if I can catch and even beat 
the best of the best JSON serializers out there.

Goals for the project are:
- Fast! (as fast as .NET will let us go)
- Easily customizable type output with pluggable formatters (including support for your own value types converted to and from JSON)
- Full polymorphic support with internal to external type mapping.
- Optimize for strongly typed data contracts with an explicitly specified member order (to improve perf even more)
- Portable Library support (initially only .NET 4.5 and WinRT)

Initial Performance Results 
---------------------------
|Test|Iterations|Time (ms)|Objects / sec|
|:---|---------:|--------:|------------:|
|SerializeChason|1,000,000|2,151|464,738.2|
|SerializeServiceStack|1,000,000|3,409|293,298.3|
|SerializeJsonNet|1,000,000|5,995|166,780.6|
|SerializeFastJson|10,000|12,593|79,408.6|
|SerializeDataContractJson|1,000,000|5,285|189,183.8|
|DeserializeChason|1,000,000|2,540|393,692.2|
|DeserializeServiceStack|1,000,000|3,347|298,753.0|
|DeserializeJsonNet|1,000,000|9,663|103,481.7|
|DeserializeFastJson|1,000,000|3,119|320,532.2|
|DeserializeDataContractJson|1,000,000|16,118|62,041.2|

Note
----
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