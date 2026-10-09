FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Ecommerce.sln ./
COPY src/Ecommerce.Api/Ecommerce.Api.csproj src/Ecommerce.Api/
COPY src/Ecommerce.Application/Ecommerce.Application.csproj src/Ecommerce.Application/
COPY src/Ecommerce.Core/Ecommerce.Core.csproj src/Ecommerce.Core/
COPY src/Ecommerce.Infrastructure/Ecommerce.Infrastructure.csproj src/Ecommerce.Infrastructure/
RUN dotnet restore src/Ecommerce.Api/Ecommerce.Api.csproj

COPY src ./src
RUN dotnet publish src/Ecommerce.Api/Ecommerce.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production

CMD ["sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT:-10000} exec dotnet Ecommerce.Api.dll"]