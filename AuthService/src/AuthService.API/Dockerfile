# ---------- RUNTIME ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# ---------- BUILD ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY . .

RUN dotnet publish src/AuthService.API/AuthService.API.csproj \
    -c Release \
    -o /app

# ---------- FINAL ----------
FROM runtime
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["dotnet", "AuthService.API.dll"]
