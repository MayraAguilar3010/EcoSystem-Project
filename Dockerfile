FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["EcoSystem.API/EcoSystem.API.csproj", "EcoSystem.API/"]
COPY ["EcoSystem.Data/EcoSystem.Data.csproj", "EcoSystem.Data/"]
RUN dotnet restore "EcoSystem.API/EcoSystem.API.csproj"

COPY . .
WORKDIR "/src/EcoSystem.API"
RUN dotnet publish "EcoSystem.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Production
ENV PORT=10000
ENV RENDER=true
ENV DOTNET_hostBuilder__reloadConfigOnChange=false
EXPOSE 10000
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EcoSystem.API.dll"]
