In order to use the GIS mapping library SharpMap v2 on mobile devices supporting the .NET Compact Framework, it has to be adapted to this reduced version of the full .NET Framework

A long term goal would be improving the library for this platform.

The changes are (being) documented on the [wiki](PortingToSharpMap2.md). If you download it right now you'll see many errors (DbaseReader.cs, NtsGeometryConverter.cs, and the providers). Well, that's the goal now, make the libraty work on .net Compact Framework.

### About [SharpMap](http://www.codeplex.com/SharpMap): ###
"SharpMap is an easy-to-use mapping library for use in web and desktop applications. It provides access to many types of GIS data, enables spatial querying of that data, and renders beautiful maps. The engine is written in C# and based on the .Net 2.0 framework. SharpMap is released under GNU Lesser General Public License."