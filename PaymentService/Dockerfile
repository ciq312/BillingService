FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src


COPY PaymentService.API/PaymentService.API.csproj PaymentService.API/
COPY PaymentService.Domain/PaymentService.Domain.csproj PaymentService.Domain/
COPY PaymentService.Infrastructure/PaymentService.Infrastructure.csproj PaymentService.Infrastructure/
COPY PaymentService.Application/PaymentService.Application.csproj PaymentService.Application/

RUN dotnet restore PaymentService.API/PaymentService.API.csproj
RUN dotnet restore PaymentService.Domain/PaymentService.Domain.csproj
RUN dotnet restore PaymentService.Infrastructure/PaymentService.Infrastructure.csproj
RUN dotnet restore PaymentService.Application/PaymentService.Application.csproj

COPY . .

WORKDIR /src
RUN dotnet publish PaymentService.API/PaymentService.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "PaymentService.API.dll"]