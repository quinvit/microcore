FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 8080
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["Sample/Angular/Services/ReportService/ReportService.csproj", "Sample/Angular/Services/ReportService/"]
RUN dotnet restore "Sample/Angular/Services/ReportService/ReportService.csproj"
COPY . .
WORKDIR "/src/Sample/Angular/Services/ReportService"
RUN dotnet build "ReportService.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "ReportService.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "ReportService.dll"]