#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0-bullseye-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0-bullseye-slim AS build
WORKDIR /src
COPY ["Abyss.Web/Abyss.Web.csproj", "Abyss.Web/"]
RUN dotnet restore "Abyss.Web/Abyss.Web.csproj"
COPY . .
WORKDIR "/src/Abyss.Web"
RUN dotnet build "Abyss.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN apt-get update -yq && apt-get upgrade -yq && apt-get install -yq ca-certificates curl gnupg && \
    mkdir -p /etc/apt/keyrings && \
    curl -fsSL https://deb.nodesource.com/gpgkey/nodesource-repo.gpg.key | gpg --dearmor -o /etc/apt/keyrings/nodesource.gpg && \
    echo "deb [signed-by=/etc/apt/keyrings/nodesource.gpg] https://deb.nodesource.com/node_18.x nodistro main" | tee /etc/apt/sources.list.d/nodesource.list && \
    apt-get update -yq && apt-get install -yq nodejs && \
    npm install -g yarn
RUN dotnet publish "Abyss.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Abyss.Web.dll"]