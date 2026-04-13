FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Slottet.sln", "./"]
COPY ["src/Slottet.Web/Slottet.csproj", "src/Slottet.Web/"]
RUN dotnet restore "src/Slottet.Web/Slottet.csproj"

COPY . .
RUN dotnet publish "src/Slottet.Web/Slottet.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_HTTP_PORTS=8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Slottet.dll"]
