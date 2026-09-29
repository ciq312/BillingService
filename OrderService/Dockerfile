# Build context is the repository root (Contracts is shared between services)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Contracts/Contracts.csproj Contracts/
COPY OrderService/OrderService.API/OrderService.API.csproj OrderService/OrderService.API/
COPY OrderService/OrderService.BLL/OrderService.BLL.csproj OrderService/OrderService.BLL/
COPY OrderService/OrderService.DAL/OrderService.DAL.csproj OrderService/OrderService.DAL/

RUN dotnet restore OrderService/OrderService.API/OrderService.API.csproj

COPY Contracts/ Contracts/
COPY OrderService/ OrderService/

RUN dotnet publish OrderService/OrderService.API/OrderService.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "OrderService.API.dll"]
