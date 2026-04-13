FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Slottet.sln", "./"]
COPY ["src/Slottet.Api/Slottet.Api.csproj", "src/Slottet.Api/"]
COPY ["src/Slottet.Application/Slottet.Application.csproj", "src/Slottet.Application/"]
COPY ["src/Slottet.Domain/Slottet.Domain.csproj", "src/Slottet.Domain/"]
COPY ["src/Slottet.Infrastructure/Slottet.Infrastructure.csproj", "src/Slottet.Infrastructure/"]
RUN dotnet restore "src/Slottet.Api/Slottet.Api.csproj"

COPY . .
RUN dotnet publish "src/Slottet.Api/Slottet.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_HTTP_PORTS=8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Slottet.Api.dll"]
