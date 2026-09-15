# Contexto de build é a raiz do repositório, não esta pasta.
# Na Railway, configure o serviço com Dockerfile Path = Lawllit.Finance.Site/Dockerfile.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props NuGet.config ./
COPY Lawllit.Model/Lawllit.Model.csproj Lawllit.Model/
COPY Lawllit.Repository/Lawllit.Repository.csproj Lawllit.Repository/
COPY Lawllit.Finance.Site/Lawllit.Finance.Site.csproj Lawllit.Finance.Site/
RUN dotnet restore Lawllit.Finance.Site/Lawllit.Finance.Site.csproj

COPY Lawllit.Model/ Lawllit.Model/
COPY Lawllit.Repository/ Lawllit.Repository/
COPY Lawllit.Finance.Site/ Lawllit.Finance.Site/
RUN dotnet publish Lawllit.Finance.Site/Lawllit.Finance.Site.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

COPY --from=build /app/publish .

# Roda sem privilégio de root. APP_UID já vem definido na imagem base da Microsoft.
USER $APP_UID

ENTRYPOINT ["dotnet", "Lawllit.Finance.Site.dll"]
