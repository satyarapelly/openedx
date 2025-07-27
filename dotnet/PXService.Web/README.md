# PXService.Web

This folder contains the beginning of the .NET 8 migration for the legacy `PXService` web application.

The project is a minimal ASP.NET Core Web API targeting **.NET 8**. It exposes a `/probe` endpoint which returns the service status and build version from configuration, matching the behavior of the original `ProbeController`.

Further migration work is required to port the remaining controllers, filters, and configuration from the existing .NET Framework project.
