# -------- build --------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia tudo
COPY . .

# Restaura e publica
RUN dotnet restore ./src/Orders.Api/Orders.Api.csproj
RUN dotnet publish ./src/Orders.Api/Orders.Api.csproj -c Release -o /app/publish --no-restore

# -------- runtime --------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Porta padrão dentro do container
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Orders.Api.dll"]