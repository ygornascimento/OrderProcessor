# -------- build --------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore ./src/Orders.Worker/Orders.Worker.csproj
RUN dotnet publish ./src/Orders.Worker/Orders.Worker.csproj -c Release -o /app/publish --no-restore

# -------- runtime --------
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Orders.Worker.dll"]