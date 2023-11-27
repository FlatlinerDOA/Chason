Chason
======
On a project I was working on (back in 2013), we noticed 30% of all our code execution was spent either serializing or deserializing JSON.
Clearly there was room for improvement. The goal of Chason (pronounced 'chase on') is to see if I can catch and even beat 
the best of the best JSON serializers out there.

When I first developed this serializers were MUCH slower than they are today, 
having said that it's good to see my code still keeping up with some of the best of them.

Goals for the project
---------------------
- Prove the concept that JSON serializers could be faster! (see how fast .NET will let us go)
- Easily customizable type output with pluggable formatters (including support for your own value types converted to and from JSON)
- Full polymorphic support with internal type to external name mapping.
- Optimize for strongly typed data contracts with an explicitly specified member order (to improve perf even more)
- .NET Standard 2.1

Non goals
---------
- Feature parity with all other serializers.
- AOT support with code generators, there are better methods for that these days.

Initial Performance Results 
---------------------------
|Test|Iterations|Time (ms)|Objects / sec|
|:---|---------:|--------:|------------:|
|SerializeChason|1,000,000|948|1,054,526.4|
|SerializeServiceStack|1,000,000|2,864|349,117.8|
|SerializeJsonNet|1,000,000|3,292|303,759.8|
|SerializeFastJson|1,000,000|2,983|335,128.1|
|SerializeDataContractJson|1,000,000|1,694|590,081.8|
|DeserializeChason|1,000,000|1,886|530,207.5|
|DeserializeServiceStack|1,000,000|201,340|4,966.7|
|DeserializeJsonNet|1,000,000|1,710|584,574.8|
|DeserializeFastJson|1,000,000|1,631|613,094.7|
|DeserializeDataContractJson|1,000,000|5,332|187,531.7|

Note
----
- Tests are run with a thread-affinity set to a single core.
- Chason is unfinished and different test scenarios would probably fail.
- fastJSON serialization is quite fast in small iterations but became exponentially slow at 1,000,000 iterations so the iteration count was reduced to 10,000.
- Test is a single instance of a simple object being serialized / deserialized against common implementations
- Tests were run on a AMD Ryzen 9 5950x 16-core processor on Windows 11


Some of the code is based on a heavily modified version of fastJSON for deserialization.

- http://fastjson.codeplex.com/ - For fastJSON's code.
- http://www.codeproject.com/KB/IP/fastJSON.aspx - Article outlining how fastJSON works.

The license for this is MIT.


> The chase is on!
>
> Andrew Chisholm
