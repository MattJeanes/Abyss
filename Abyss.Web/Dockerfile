#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0@sha256:d5507d8292fb2469ec0b9913f3cd4bb8d5a014bd6dc00c33fd4c4e7310229f07 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0@sha256:c5b188baf837b0180a14f988815b9cc7a55b836dd6cbe1b6e6523cf3098faaa8 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Abyss.Web/Abyss.Web.csproj", "Abyss.Web/"]
RUN dotnet restore "./Abyss.Web/Abyss.Web.csproj"
COPY . .
WORKDIR "/src/Abyss.Web"
RUN dotnet build "./Abyss.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN curl -fsSL https://deb.nodesource.com/setup_22.x | bash - && \
    apt-get install -y nodejs && \
    npm install -g yarn
RUN dotnet publish "./Abyss.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Abyss.Web.dll"]