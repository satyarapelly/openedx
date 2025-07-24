# TokenMiseValidator

This folder contains a .NET 8.0 project with the `TokenMiseValidator` class.
The implementation uses `dynamic` types for the SAL and MISE dependencies to
allow it to compile on .NET 8. It also replaces the legacy
`HttpRequestMessage` usage with a lightweight `HttpRequest` class so the code
does not depend on ASP.NET types.
