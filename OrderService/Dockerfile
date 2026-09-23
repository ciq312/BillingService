FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src


COPY OrderService.API/OrderService.API.csproj OrderService.API/
COPY OrderService.BLL/OrderService.BLL.csproj OrderService.BLL/
COPY OrderService.DAL/OrderService.DAL.csproj OrderService.DAL/

RUN dotnet restore OrderService.API/OrderService.API.csproj
RUN dotnet restore OrderService.BLL/OrderService.BLL.csproj
RUN dotnet restore OrderService.DAL/OrderService.DAL.csproj
COPY . .

WORKDIR /src
RUN dotnet publish OrderService.API/OrderService.API.csproj -c Production -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "OrderService.API.dll"]