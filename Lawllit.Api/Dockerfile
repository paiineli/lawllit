# Contexto de build é a raiz do repositório, não esta pasta.
# Na Railway, configure o serviço com Dockerfile Path = Lawllit.Api/Dockerfile.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia só o que o restore precisa, para a camada de pacote ficar em cache
# enquanto o código-fonte muda.
COPY Directory.Build.props Directory.Packages.props NuGet.config global.json ./
COPY Lawllit.Model/Lawllit.Model.csproj Lawllit.Model/
COPY Lawllit.Api/Lawllit.Api.csproj Lawllit.Api/
RUN dotnet restore Lawllit.Api/Lawllit.Api.csproj

COPY Lawllit.Model/ Lawllit.Model/
COPY Lawllit.Api/ Lawllit.Api/
RUN dotnet publish Lawllit.Api/Lawllit.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

COPY --from=build /app/publish .

# Roda sem privilégio de root. APP_UID já vem definido na imagem base da Microsoft.
USER $APP_UID

ENTRYPOINT ["dotnet", "Lawllit.Api.dll"]
