# Slottet

Slottet er et eksamensprojekt udviklet i ASP.NET Core og Blazor Web App. Løsningen er bygget som et distribueret client-server system med separat frontend, separat API og ekstern SQL-database.

## Arkitektur

Løsningen består af følgende projekter:

- `src/Slottet.Web` - Blazor Web App frontend
- `src/Slottet.Api` - backend API
- `src/Slottet.Application` - applikationslogik og use cases
- `src/Slottet.Domain` - domænemodeller og forretningsregler
- `src/Slottet.Infrastructure` - database, persistence, auth og integrationer
- `tests/Slottet.Application.Tests` - unit tests for application-laget
- `tests/Slottet.Domain.Tests` - unit tests for domain-laget

Arkitekturen følger Clean Architecture og repository pattern. Det ses blandt andet ved:

- interfaces i `src/Slottet.Application/Interfaces`
- use cases/services i `src/Slottet.Application/Services`
- konkrete repositories i `src/Slottet.Infrastructure/Repositories`
- domæneentiteter i `src/Slottet.Domain/Entities`

## Teknologier

- .NET 10
- ASP.NET Core
- Blazor Web App
- ASP.NET Core Web API
- Entity Framework Core
- Microsoft SQL Server / Azure SQL Database
- Docker

## Systemtype

Løsningen er et distribueret **client-server system**.

Det betyder i denne løsning:

- klienten er frontend-applikationen i `src/Slottet.Web`
- serveren er backend-API'et i `src/Slottet.Api`
- databasen er en central ekstern ressource i Azure SQL

## Systembeskrivelse
Slottet Overlapsystem er udviklet til at støtte det daglige arbejde i afdelingerne Slottet og Skoven, hvor flere medarbejdere skal koordinere borgere, medicin, opgaver og ansvar på tværs af vagtskift. Systemet bruges som et fælles arbejdsredskab i løbet af vagten: brugeren vælger afdeling, vagt og dato og får derefter et samlet overblik over aktive borgere. For hver borger vises centrale oplysninger som medicinstatus, næste medicintidspunkt, særlige hændelser og tilknyttede medarbejdere. Derudover giver systemet mulighed for at arbejde med vagtopgaver, ansvarsopgaver og telefonfordeling, så vigtige driftsopgaver er synlige og fordelt. For vagtansvarlige og administratorer understøtter løsningen også borgerfordeling, hvor medarbejdere kan tildeles borgere på tværs af dag-, aften- og nattevagt, så overleveringen mellem hold bliver mere ensartet og gennemskuelig.

Teknisk er løsningen bygget som en distribueret client-server applikation med Blazor som frontend og et separat ASP.NET Core API til forretningslogik og dataadgang. Systemet er struktureret i lag, så ansvar er opdelt mellem domæne, applikationslogik og infrastruktur, hvilket giver en tydelig og vedligeholdbar arkitektur. API’et håndterer systemets funktioner via dedikerede endpoints, mens data persisteres i en ekstern SQL-database. Adgangen er rollebaseret for at sikre, at brugere kun ser og udfører de handlinger, der passer til deres ansvar: en Medarbejder kan arbejde i den daglige overlapvisning og registrere den løbende drift, en Vagtansvarlig kan derudover fordele medarbejdere på borgere og vagter samt styre centrale vagtopgaver, og en Admin har udvidede rettigheder til administrative funktioner som oprettelse af borgere og medarbejdere, konfiguration samt logs/GDPR-håndtering. Opdelingen giver både bedre datasikkerhed og en mere enkel brugeroplevelse, fordi hver rolle møder relevante funktioner uden unødig kompleksitet. Autentifikation er implementeret sikkert med JWT, så kun godkendte brugere kan tilgå systemet, og autorisation håndhæves konsekvent på tværs af funktioner. Samlet fremstår løsningen som en driftsegnet platform til daglig koordinering, dokumentation og sikker adgang til data i en professionel plejekontekst.


## Guide til lærere: lokal opsætning og kørsel

Denne guide viser, hvordan I kører systemet på jeres egne maskiner.

### Krav

- .NET 10 SDK
- Adgang til SQL Server/Azure SQL (connection string)
- (Valgfrit) Docker Desktop, hvis I vil køre med Docker Compose

### 1) Klargør konfigurationsfiler (uden Docker)

Der skal oprettes to lokale filer, som **ikke** ligger i Git:

- `src/Slottet.Api/appsettings.Development.json`
- `src/Slottet.Web/appsettings.Development.json`

#### `src/Slottet.Api/appsettings.Development.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "SlottetDb": "Server=tcp:datamatikerdatabase.database.windows.net,1433;Initial Catalog=Slottet;Persist Security Info=False;User ID=DMO13;Password=Chr991511;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  },
  "Swagger": {
    "Enabled": true
  }
}
```

#### `src/Slottet.Web/appsettings.Development.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "SlottetDb": "Server=tcp:datamatikerdatabase.database.windows.net,1433;Initial Catalog=Slottet;Persist Security Info=False;User ID=DMO13;Password=Chr991511;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  },
  "Api": {
    "BaseUrl": "http://localhost:5032/"
  },
  "DataProtection": {
    "ApplicationName": "Slottet"
  }
}
```

### 2) Kør systemet uden Docker

Åbn to terminaler i projektets rodmappe.

Terminal 1 (API):

```bash
dotnet run --project src/Slottet.Api
```

Terminal 2 (Web):

```bash
dotnet run --project src/Slottet.Web
```

Standardadresser i development:

- Web: `https://localhost:7169` (eller `http://localhost:5150`)
- API: `https://localhost:7079` (eller `http://localhost:5032`)
- Swagger: `https://localhost:7079/swagger`

### 3) Kør systemet med Docker Compose (valgfrit)

1. Kopiér `.env.example` til `.env`
2. Udfyld mindst:
   - `SLOTTET_DB_CONNECTION`
   - `SLOTTET_JWT_SIGNING_KEY`
3. Start:

```bash
docker compose up --build
```

Adresser:

- Web: `http://localhost:8080`
- API: `http://localhost:8081`
- Swagger: `http://localhost:8081/swagger`

## Fejlsøgning

- Fejl om `Connection string 'SlottetDb' was not found`:
  Tjek at `ConnectionStrings:SlottetDb` er sat i begge `appsettings.Development.json`.
- Fejl om JWT-konfiguration:
  Tjek at `Jwt`-sektionen er udfyldt i API-konfigurationen.
- CORS-fejl i browser:
  Tjek at `Cors:AllowedOrigins` i API matcher web-adressen (`https://localhost:7169`).
