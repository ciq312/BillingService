# Build context is the repository root (Contracts is shared between services)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Contracts/Contracts.csproj Contracts/
COPY PaymentService/PaymentService.API/PaymentService.API.csproj PaymentService/PaymentService.API/
COPY PaymentService/PaymentService.Application/PaymentService.Application.csproj PaymentService/PaymentService.Application/
COPY PaymentService/PaymentService.Domain/PaymentService.Domain.csproj PaymentService/PaymentService.Domain/
COPY PaymentService/PaymentService.Infrastructure/PaymentService.Infrastructure.csproj PaymentService/PaymentService.Infrastructure/

RUN dotnet restore PaymentService/PaymentService.API/PaymentService.API.csproj

COPY Contracts/ Contracts/
COPY PaymentService/ PaymentService/

RUN dotnet publish PaymentService/PaymentService.API/PaymentService.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "PaymentService.API.dll"]
