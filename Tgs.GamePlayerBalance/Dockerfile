#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Tgs.GamePlayerBalance/Tgs.GamePlayerBalance.csproj", "Tgs.GamePlayerBalance/"]
COPY ["Tgs.GamePlayerBalance.Common/Tgs.GamePlayerBalance.Common.csproj", "Tgs.GamePlayerBalance.Common/"]
COPY ["Tgs.GamePlayerBalance.Core/Tgs.GamePlayerBalance.Core.csproj", "Tgs.GamePlayerBalance.Core/"]
COPY ["Tgs.GamePlayerBalance.Repositories/Tgs.GamePlayerBalance.Repositories.csproj", "Tgs.GamePlayerBalance.Repositories/"]
RUN dotnet restore "Tgs.GamePlayerBalance/Tgs.GamePlayerBalance.csproj"
COPY . .
WORKDIR "/src/Tgs.GamePlayerBalance"
RUN dotnet build "Tgs.GamePlayerBalance.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Tgs.GamePlayerBalance.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.GamePlayerBalance.dll"]