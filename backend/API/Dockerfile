FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy all projects and restore
COPY ["API/MatdanSathi.API.csproj", "API/"]
COPY ["Infrastructure/MatdanSathi.API.Infrastructure.csproj", "Infrastructure/"]
COPY ["Application/MatdanSathi.API.Application.csproj", "Application/"]
COPY ["Domain/MatdanSathi.API.Domain.csproj", "Domain/"]
RUN dotnet restore "API/MatdanSathi.API.csproj"

# Copy everything else and build the app
COPY . .
WORKDIR "/src/API"
RUN dotnet build "MatdanSathi.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MatdanSathi.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage runtime setup
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
EXPOSE 8081
ENTRYPOINT ["dotnet", "MatdanSathi.API.dll"]
