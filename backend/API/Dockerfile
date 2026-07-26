FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy all projects and restore
COPY ["API/MatdarSathi.API.csproj", "API/"]
COPY ["Infrastructure/MatdarSathi.API.Infrastructure.csproj", "Infrastructure/"]
COPY ["Application/MatdarSathi.API.Application.csproj", "Application/"]
COPY ["Domain/MatdarSathi.API.Domain.csproj", "Domain/"]
RUN dotnet restore "API/MatdarSathi.API.csproj"

# Copy everything else and build the app
COPY . .
WORKDIR "/src/API"
RUN dotnet build "MatdarSathi.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MatdarSathi.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage runtime setup
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
EXPOSE 8081
ENTRYPOINT ["dotnet", "MatdarSathi.API.dll"]
