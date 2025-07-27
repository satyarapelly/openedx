# PIDLResourceFactory

This folder contains a minimal .NET 8.0 project that builds the `PIDLResourceFactory` class.
The source was ported from an older framework and no longer relies on `System.Web` types.
Any previous usage of `HttpRequestMessage` has been removed so the code can compile
without ASP.NET dependencies.
