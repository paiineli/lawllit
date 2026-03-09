FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Lawllit.slnx ./
COPY Lawllit.Data/Lawllit.Data.csproj Lawllit.Data/
COPY Lawllit.Web/Lawllit.Web.csproj Lawllit.Web/
RUN dotnet restore

COPY . .
RUN dotnet publish Lawllit.Web/Lawllit.Web.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Lawllit.Web.dll"]
