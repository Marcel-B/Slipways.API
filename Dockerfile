#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 8095

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src

COPY ["Slipways.API/Slipways.API.csproj", "Slipways.API/"]
COPY ["NuGet.config", "Slipways.API/"]

RUN dotnet restore "Slipways.API/Slipways.API.csproj" --configfile ./Slipways.API/NuGet.config
COPY . .
WORKDIR "/src/Slipways.API"
RUN dotnet build "Slipways.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Slipways.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Slipways.API.dll"]