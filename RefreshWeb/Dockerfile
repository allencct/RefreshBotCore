#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

RUN apt-get update && apt-get install -y curl
RUN curl -sL https://deb.nodesource.com/setup_lts.x | bash -
RUN apt-get install -y nodejs

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["RefreshWeb/RefreshWeb.csproj", "RefreshWeb/"]
RUN dotnet restore "RefreshWeb/RefreshWeb.csproj"
COPY . .
WORKDIR "/src/RefreshWeb"
RUN dotnet build "RefreshWeb.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RefreshWeb.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RefreshWeb.dll"]
