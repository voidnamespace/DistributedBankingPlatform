# ---------- RUNTIME ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# ---------- BUILD ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .

RUN dotnet publish src/AccountService.API/AccountService.API.csproj \
    -c Release \
    -o /app

# ---------- FINAL ----------
FROM runtime
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["dotnet", "AccountService.API.dll"]
