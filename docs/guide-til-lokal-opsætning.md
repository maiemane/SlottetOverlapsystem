# Guide til lærere: lokal opsætning og kørsel

Denne guide viser, hvordan I kører systemet på jeres egne maskiner.

## Krav

- .NET 10 SDK
- Adgang til SQL Server/Azure SQL (connection string)
- (Valgfrit) Docker Desktop, hvis I vil køre med Docker Compose

## 1) Klargør konfigurationsfiler (uden Docker)

Der skal oprettes to lokale filer, som **ikke** ligger i Git:

- `src/Slottet.Api/appsettings.Development.json`
- `src/Slottet.Web/appsettings.Development.json`

### `src/Slottet.Api/appsettings.Development.json`

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

### `src/Slottet.Web/appsettings.Development.json`

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

## 2) Kør systemet uden Docker

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

## 3) Kør systemet med Docker Compose (valgfrit)

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
