#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Tgs.GamePlayerBalance/Tgs.GamePlayerBalance.csproj", "Tgs.GamePlayerBalance/"]
COPY ["Tgs.GamePlayerBalance.Common/Tgs.GamePlayerBalance.Common.csproj", "Tgs.GamePlayerBalance.Common/"]
COPY ["Tgs.GamePlayerBalance.Core/Tgs.GamePlayerBalance.Core.csproj", "Tgs.GamePlayerBalance.Core/"]
COPY ["Tgs.GamePlayerBalance.Repositories/Tgs.GamePlayerBalance.Repositories.csproj", "Tgs.GamePlayerBalance.Repositories/"]
RUN dotnet restore "./Tgs.GamePlayerBalance/./Tgs.GamePlayerBalance.csproj"
COPY . .
WORKDIR "/src/Tgs.GamePlayerBalance"
RUN dotnet build "./Tgs.GamePlayerBalance.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Tgs.GamePlayerBalance.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.GamePlayerBalance.dll"]