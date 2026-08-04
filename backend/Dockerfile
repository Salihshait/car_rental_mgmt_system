FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/CarRent.Api/CarRent.Api.csproj", "src/CarRent.Api/"]
COPY ["src/CarRent.Application/CarRent.Application.csproj", "src/CarRent.Application/"]
COPY ["src/CarRent.Domain/CarRent.Domain.csproj", "src/CarRent.Domain/"]
COPY ["src/CarRent.Infrastructure/CarRent.Infrastructure.csproj", "src/CarRent.Infrastructure/"]
RUN dotnet restore "src/CarRent.Api/CarRent.Api.csproj"
COPY . .
WORKDIR "/src/src/CarRent.Api"
RUN dotnet publish "CarRent.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV DOTNET_hostBuilder__reloadConfigOnChange=false
ENTRYPOINT ["dotnet", "CarRent.Api.dll"]
