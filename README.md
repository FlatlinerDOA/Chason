Chason
======

On a project I was working on, we noticed 30% of all our code execution was spent either serializing or deserializing JSON.
Clearly there was room for improvement. The goal of Chason (pronounced 'chase on') is to see if I can catch and even beat 
the best of the best JSON serializers out there.

Goals for the project are:
- Fast (as fast as .NET will go)
- Pluggable formatters (support for your own value types converted to and from JSON)
- Full polymorphic support with internal to external type mapping.


Initial Performance Results 
---------------------------
|Test|Time (ms)|Objects / sec|
|:---|--------:|------------:|
|SerializeChason(1000000)|2146|465,846.3|
|SerializeServiceStack(1000000)|3333|299,974.4|
|SerializeJsonNet(1000000)|6202|161,218.6|
|SerializeFastJson((1000000 / 100))|12415|80,545.3|
|SerializeDataContractJson(1000000)|5301|188,627.4|
|DeserializeChason(1000000)|2435|410,542.9|
|DeserializeServiceStack(1000000)|3300|302,942.9|
|DeserializeJsonNet(1000000)|9370|106,723.6|
|DeserializeFastJson(1000000)|3046|328,225.3|
|DeserializeDataContractJson(1000000)|16141|61,950.5|

Note
----
- Chason is unfinished and different test scenarios would probably fail.
- fastJSON serialization is quite fast in small iterations but became exponentially slow at 1000,000 iterations so the iteration count was reduced for it.
- Test is a single instance of a simple object being serialized / deserialized against common implementations
- Tests were run on a Core i7 2.66GHz MacBook Pro with 4G RAM and an SSD, on Windows 8 64bit. (Experience Index 5.9)


Some of the code is based on a heavily modified version of fastJSON for deserialization.
http://fastjson.codeplex.com/ - For fastJSON's code.
http://www.codeproject.com/KB/IP/fastJSON.aspx - Article outlining how fastJSON works.


The license for this is MIT.

The chase is on!
Andrew Chisholm