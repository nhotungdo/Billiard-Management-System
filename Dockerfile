FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["BilliardManagement.API/BilliardManagement.API.csproj", "BilliardManagement.API/"]
COPY ["BilliardManagement.Business/BilliardManagement.Business.csproj", "BilliardManagement.Business/"]
COPY ["BilliardManagement.Data/BilliardManagement.Data.csproj", "BilliardManagement.Data/"]
COPY ["BilliardManagement.Models/BilliardManagement.Models.csproj", "BilliardManagement.Models/"]
COPY ["BilliardManagement.Common/BilliardManagement.Common.csproj", "BilliardManagement.Common/"]
RUN dotnet restore "BilliardManagement.API/BilliardManagement.API.csproj"
COPY . .
WORKDIR "/src/BilliardManagement.API"
RUN dotnet build "BilliardManagement.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "BilliardManagement.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BilliardManagement.API.dll"]
