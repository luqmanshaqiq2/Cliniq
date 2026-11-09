FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY backend/Cliniq.API/Cliniq.API.csproj backend/Cliniq.API/
COPY backend/Cliniq.DataAccess/Cliniq.DataAccess.csproj backend/Cliniq.DataAccess/
COPY backend/Cliniq.Models/Cliniq.Models.csproj backend/Cliniq.Models/
COPY backend/Cliniq.Services/Cliniq.Services.csproj backend/Cliniq.Services/
RUN dotnet restore backend/Cliniq.API/Cliniq.API.csproj

COPY backend/ backend/
RUN dotnet publish backend/Cliniq.API/Cliniq.API.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN mkdir -p /app/data /app/StaticFiles \
    && chown -R app:app /app

ENV ASPNETCORE_HTTP_PORTS=8080 \
    ConnectionStrings__DefaultConnection="Data Source=/app/data/cliniq.db"

EXPOSE 8080
VOLUME ["/app/data"]

COPY --from=build --chown=app:app /app/publish .

USER app
ENTRYPOINT ["dotnet", "Cliniq.API.dll"]
