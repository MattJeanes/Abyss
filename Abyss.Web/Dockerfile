#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0-bullseye-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0-bullseye-slim AS build
WORKDIR /src
COPY ["Abyss.Web/Abyss.Web.csproj", "Abyss.Web/"]
RUN dotnet restore "Abyss.Web/Abyss.Web.csproj"
COPY . .
WORKDIR "/src/Abyss.Web"
RUN dotnet build "Abyss.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN apt-get update -yq && apt-get upgrade -yq && apt-get install -yq curl git nano
RUN curl -sL https://deb.nodesource.com/setup_12.x | bash - && apt-get install -yq nodejs build-essential
RUN npm install -g yarn
RUN dotnet publish "Abyss.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Abyss.Web.dll"]