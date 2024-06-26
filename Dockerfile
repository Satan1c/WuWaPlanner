﻿FROM ubuntu:lunar
RUN apt-get update -qy
RUN apt-get upgrade -qy
RUN apt-get install -y libfontconfig1 pkg-config fontconfig

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /
COPY ["WuWaPlanner/bin/Localizations", "Localizations/"]
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["WuWaPlanner/WuWaPlanner.csproj", "WuWaPlanner/"]
RUN dotnet restore "WuWaPlanner/WuWaPlanner.csproj"
COPY . .
WORKDIR "/src/WuWaPlanner"
RUN dotnet build "WuWaPlanner.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "WuWaPlanner.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
ARG BUILD_CONFIGURATION=Release
ENV ASPNETCORE_ENVIRONMENT=$BUILD_CONFIGURATION
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WuWaPlanner.dll"]
