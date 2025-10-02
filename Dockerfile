# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ./BoardHoard/*.csproj BoardHoard/
COPY ./Boardgames.cs/*.csproj Boardgames.cs/
RUN dotnet restore BoardHoard/BoardHoard.csproj
COPY ./BoardHoard/ BoardHoard/
COPY ./Boardgames.cs/ Boardgames.cs/
WORKDIR /src/BoardHoard
RUN dotnet publish BoardHoard.csproj -c Release -o /app/out

# Run
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080
COPY --from=build /app/out ./
ENTRYPOINT ["dotnet", "BoardHoard.dll"]
