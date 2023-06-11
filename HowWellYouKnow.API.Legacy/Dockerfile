#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["HowWellYouKnow.API/HowWellYouKnow.API.csproj", "HowWellYouKnow.API/"]
RUN dotnet restore "HowWellYouKnow.API/HowWellYouKnow.API.csproj"
COPY . .
WORKDIR "/src/HowWellYouKnow.API"
RUN dotnet build "HowWellYouKnow.API.csproj" -c Release -o /app/build
RUN apt-get update && \
    apt-get install -y wget && \
    apt-get install -y gnupg2 && \
    wget -qO- https://deb.nodesource.com/setup_10.x | bash - && \
    apt-get install -y build-essential nodejs

FROM build AS publish
RUN dotnet publish "HowWellYouKnow.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
RUN touch ./questions.db
RUN chmod -R a+rwX ./questions.db
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HowWellYouKnow.API.dll"]