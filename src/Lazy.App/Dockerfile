#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
ENV DOCKER_REGISTRY lazyinc.azurecr.io
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Lazy.App/Lazy.App.csproj", "Lazy.App/"]
COPY ["Lazy.Application/Lazy.Application.csproj", "Lazy.Application/"]
COPY ["Lazy.Domain/Lazy.Domain.csproj", "Lazy.Domain/"]
COPY ["Lazy.Infrastructure/Lazy.Infrastructure.csproj", "Lazy.Infrastructure/"]
COPY ["Lazy.Persistence/Lazy.Persistence.csproj", "Lazy.Persistence/"]
COPY ["Lazy.Presentation/Lazy.Presentation.csproj", "Lazy.Presentation/"]
RUN dotnet restore "Lazy.App/Lazy.App.csproj"
COPY . .
WORKDIR "/src/Lazy.App"
RUN dotnet build "Lazy.App.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Lazy.App.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Lazy.App.dll"]