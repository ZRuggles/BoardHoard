# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ./BHAPI/*.csproj BHAPI/
COPY ./Boardgames.cs/*.csproj Boardgames.cs/
RUN dotnet restore BHAPI/BoardHoardAPI.csproj
COPY ./BHAPI/ BHAPI/
COPY ./Boardgames.cs/ Boardgames.cs/
WORKDIR /src/BHAPI
RUN dotnet publish BoardHoardAPI.csproj -c Release -o /app/out

# Run
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080
COPY --from=build /app/out ./
ENTRYPOINT ["dotnet", "BoardHoardAPI.dll"]
