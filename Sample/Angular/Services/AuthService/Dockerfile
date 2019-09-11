FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 8080
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["Sample/Angular/Services/AuthService/AuthService.csproj", "Sample/Angular/Services/AuthService/"]
RUN dotnet restore "Sample/Angular/Services/AuthService/AuthService.csproj"
COPY . .
WORKDIR "/src/Sample/Angular/Services/AuthService"
RUN dotnet build "AuthService.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "AuthService.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "AuthService.dll"]