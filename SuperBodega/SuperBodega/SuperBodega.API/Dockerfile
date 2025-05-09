FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["SuperBodega.API/SuperBodega.API.csproj", "SuperBodega.API/"]
RUN dotnet restore "SuperBodega.API/SuperBodega.API.csproj"

COPY . .
WORKDIR "/src/SuperBodega.API"
RUN dotnet build "SuperBodega.API.csproj" -c Release -o /app/build
RUN dotnet publish "SuperBodega.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

ENV CACHE_BUST=timestamp_value

COPY --from=build /app/publish .

COPY ["SuperBodega.API/Views", "/app/Views/"]
COPY ["SuperBodega.API/wwwroot", "/app/wwwroot/"]
RUN mkdir -p /app/wwwroot/images/productos

RUN chmod -R 777 /app/wwwroot/images

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:880
ENTRYPOINT ["dotnet", "SuperBodega.API.dll"]