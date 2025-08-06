# Multi-stage build for PXService.Web
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore PXService.Web/PXService.Web.csproj
RUN dotnet publish PXService.Web/PXService.Web.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Microsoft.Commerce.Payments.PXService.dll"]
