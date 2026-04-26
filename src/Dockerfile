FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["BiblioRate.sln", "./"]
COPY ["BiblioRate.API/BiblioRate.API.csproj", "BiblioRate.API/"]
COPY ["BiblioRate.Application/BiblioRate.Application.csproj", "BiblioRate.Application/"]
COPY ["BiblioRate.Domain/BiblioRate.Domain.csproj", "BiblioRate.Domain/"]
COPY ["BiblioRate.Infrastructure/BiblioRate.Infrastructure.csproj", "BiblioRate.Infrastructure/"]

RUN dotnet restore "BiblioRate.sln"
COPY . .
WORKDIR "/src/BiblioRate.API"
RUN dotnet build "BiblioRate.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BiblioRate.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BiblioRate.API.dll"]
