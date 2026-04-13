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

Løsningen er **ikke** peer-to-peer, fordi klienterne ikke kommunikerer direkte med hinanden.

Løsningen er **ikke** en egentlig microservice-arkitektur, fordi backend-funktionaliteten stadig er samlet i én API-service og ikke opdelt i flere små, uafhængigt deploybare services med hver deres database.

## Kørsel lokalt

Løsningen kan køres både uden Docker og med Docker. I begge tilfælde er databasen ekstern.

### Uden Docker

Frontend læser API-baseurl fra `Api:BaseUrl`.
API og frontend kan læse databaseforbindelse fra `ConnectionStrings:SlottetDb`.

Relevante filer:

- `src/Slottet.Api/appsettings.json`
- `src/Slottet.Api/appsettings.Development.json`
- `src/Slottet.Web/appsettings.json`
- `src/Slottet.Web/appsettings.Development.json`

Eksempel på lokal kørsel af API med environment variable:

```bash
ConnectionStrings__SlottetDb="<jeres connection string>" dotnet run --project src/Slottet.Api
```

### Med Docker Compose

1. Kopiér `.env.example` til `.env`
2. Udfyld mindst `SLOTTET_DB_CONNECTION` og `SLOTTET_JWT_SIGNING_KEY`
3. Start løsningen:

```bash
docker compose up --build
```

Frontend eksponeres på `http://localhost:8080`
API eksponeres på `http://localhost:8081`
Swagger findes i development-opsætningen på `http://localhost:8081/swagger`

Health endpoints:

- `http://localhost:8080/health`
- `http://localhost:8081/health`

Relevante filer:

- `docker-compose.yml`
- `docker-compose.prod.yml`
- `src/Slottet.Api/Dockerfile`
- `src/Slottet.Web/Dockerfile`
- `.env.example`
- `.dockerignore`

### Produktionslignende Docker-kørsel

Hvis løsningen skal køres lokalt i en mere produktionslignende konfiguration:

```bash
docker compose -f docker-compose.yml -f docker-compose.prod.yml up --build
```

Dette gør blandt andet:

- sætter `ASPNETCORE_ENVIRONMENT=Production`
- slår Swagger fra som standard
- aktiverer reverse proxy-understøttelse via forwarded headers

## CI/CD

GitHub Actions-workflowen ligger i:

- `.github/workflows/ci.yml`

Workflowen kører:

- `dotnet restore Slottet.sln`
- `dotnet build Slottet.sln --configuration Release --no-restore`
- `dotnet test Slottet.sln --configuration Release --no-build`

## Test

Tests kan køres med:

```bash
dotnet test Slottet.sln
```

Der findes tests i:

- `tests/Slottet.Application.Tests`
- `tests/Slottet.Domain.Tests`

## Teknologi- og arkitekturkrav

Dette afsnit beskriver, hvordan løsningen opfylder kravene til teknologi, arkitektur, cloud readiness og distribueret drift.

### 1. Framework og sprog

Krav:

- ASP.NET Core
- Blazor til frontend

Opfyldelse:

- API'et er implementeret i ASP.NET Core i `src/Slottet.Api/Program.cs`
- frontenden er implementeret som Blazor Web App i `src/Slottet.Web/Program.cs`

### 2. Database og persistence

Krav:

- Microsoft SQL Server eller MySQL
- Entity Framework

Opfyldelse:

- databasen er Microsoft SQL Server / Azure SQL
- EF Core bruges via `ApplicationDbContext` i `src/Slottet.Infrastructure/Data/ApplicationDbContext.cs`
- migrations findes i `src/Slottet.Infrastructure/Data/Migrations`
- connection strings læses fra konfiguration og environment variables

### 3. API-eksponering

Krav:

- systemets funktioner skal eksponeres som et API

Opfyldelse:

- API'et ligger i `src/Slottet.Api`
- controllers findes i `src/Slottet.Api/Controllers`
- login endpoint findes i `src/Slottet.Api/Controllers/AuthController.cs`
- forretningslogik kaldes via application services, ikke direkte fra controllerne

### 4. Containerisering

Krav:

- Docker i passende omfang

Opfyldelse:

- frontend har egen containerdefinition i `src/Slottet.Web/Dockerfile`
- API har egen containerdefinition i `src/Slottet.Api/Dockerfile`
- lokal development-opsætning findes i `docker-compose.yml`
- produktionsoverride findes i `docker-compose.prod.yml`

Det er et bevidst designvalg, at databasen **ikke** containeriseres lokalt i løsningen, fordi projektet anvender en ekstern Azure SQL-database. Det understøtter bedre den faktiske driftssituation.

## Cloud-ready og distribueret system

Dette afsnit beskriver de konkrete valg, der gør løsningen cloud-ready, og hvor der stadig findes arkitektoniske forbehold.

### 1. Kørsel som standalone installation på én PC

Opfyldelse:

- frontend og API kan køres lokalt på samme maskine
- de kan også køres samlet via Docker Compose

Relevant kode og opsætning:

- `src/Slottet.Web/Program.cs`
- `src/Slottet.Api/Program.cs`
- `docker-compose.yml`

### 2. Kørsel som client/server-løsning i lokalt netværk

Opfyldelse:

- løsningen er bygget som client-server
- flere klienter kan tilgå samme backend og database over netværk

Relevant kode og opsætning:

- `src/Slottet.Web`
- `src/Slottet.Api`
- `src/Slottet.Web/Auth/AuthService.cs`

### 3. Kørsel som cloud-baseret løsning

Opfyldelse:

- frontend og API er containeriserede som separate services
- databasen er ekstern i Azure SQL
- central runtime-konfiguration sker via environment variables
- health checks og proxy-awareness er tilføjet til cloud-drift

Relevant kode og opsætning:

- `docker-compose.yml`
- `docker-compose.prod.yml`
- `src/Slottet.Web/Program.cs`
- `src/Slottet.Api/Program.cs`
- `.env.example`

### 4. Central konfiguration via miljøvariabler og secrets

Opfyldelse:

- connection strings kan sættes via `ConnectionStrings__SlottetDb`
- JWT-indstillinger kan sættes via `Jwt__...`
- CORS, Swagger, reverse proxy og Data Protection styres via config
- `.env` er ignoreret i Git

Relevant kode og opsætning:

- `docker-compose.yml`
- `docker-compose.prod.yml`
- `.env.example`
- `.gitignore`
- `src/Slottet.Api/Program.cs`
- `src/Slottet.Web/Program.cs`

### 5. Ingen binding til lokalt filsystem til delt data

Opfyldelse:

- domænedata ligger i ekstern SQL-database
- Data Protection keys deles via databasen i stedet for lokalt filsystem
- både frontend og API migrerer databasen ved opstart, så nødvendige tabeller findes uanset hvilken service der starter først

Relevant kode:

- `src/Slottet.Web/Program.cs`
- `src/Slottet.Infrastructure/Data/ApplicationDbContext.cs`
- `src/Slottet.Infrastructure/Data/ApplicationDbSeeder.cs`
- `src/Slottet.Infrastructure/Data/Migrations/20260413120000_AddDataProtectionKeys.cs`

Hvorfor det er vigtigt:

- hvis hver frontend-instans havde sine egne lokale keys, ville beskyttede payloads og antiforgery kunne fejle ved scale-out
- en fælles key store er vigtig i et distribueret setup

### Hvorfor `Slottet.Web` har databaseadgang

Det er et bevidst arkitekturvalg, at `Slottet.Web` har en begrænset databaseadgang, men **kun** til framework-infrastruktur og ikke til systemets forretningsdata.

Det betyder:

- frontend læser og skriver **ikke** domænedata som borgere, medarbejdere, overlap eller medicin direkte i databasen
- den type data går stadig gennem API'et i `src/Slottet.Api`
- frontendens databaseadgang bruges kun til **Data Protection key storage**

Det ses konkret i:

- `src/Slottet.Web/Program.cs`
  Her registreres `ApplicationDbContext`, og Data Protection konfigureres med `.PersistKeysToDbContext<ApplicationDbContext>()`
- `src/Slottet.Infrastructure/Data/ApplicationDbContext.cs`
  Her findes `DbSet<DataProtectionKey> DataProtectionKeys`
- `src/Slottet.Infrastructure/Data/Migrations/20260413120000_AddDataProtectionKeys.cs`
  Her oprettes tabellen `DataProtectionKey`

Formålet er at understøtte distribueret drift for Blazor Server:

- Blazor Server bruger beskyttede payloads og antiforgery-mekanismer
- hvis flere frontend-instanser kører bag load balancing, må de ikke være afhængige af hver deres lokale key storage
- derfor deles Data Protection keys centralt i databasen

Arkitektonisk vurdering:

- hvis man ser på forretningslogik og domænedata, er løsningen stadig korrekt opdelt med frontend -> API -> database
- hvis man ser på runtime-infrastruktur, har frontend direkte databaseadgang til en teknisk hjælpefunktion

Det er derfor vigtigt at kunne forklare til eksamen, at databaseadgangen i `Web` **ikke** bruges til at omgå API'et, men kun til at understøtte sikker og stabil drift af Blazor Server i et distribueret setup.

### Spørgsmål til underviser

Følgende spørgsmål kan bruges direkte til at få en afklaring:

“Vi har beholdt al adgang til domænedata bag vores eksterne API i `src/Slottet.Api`, men i `src/Slottet.Web/Program.cs` bruger vi også databasen til ASP.NET Core Data Protection keys via `.PersistKeysToDbContext<ApplicationDbContext>()`. Det gør vi for at understøtte Blazor Server i et distribueret setup, så flere frontend-instanser kan dele de samme keys, jf. `src/Slottet.Infrastructure/Data/ApplicationDbContext.cs` og migrationen `src/Slottet.Infrastructure/Data/Migrations/20260413120000_AddDataProtectionKeys.cs`. Vil I vurdere, at dette stadig er inden for kravet om separat frontend og ekstern API, når databaseadgangen i frontend kun bruges til framework-infrastruktur og ikke til forretningsdata?” 

### 6. Ingen binding til fast IP eller maskinspecifik konfiguration

Opfyldelse:

- løsningen bruger konfigurationsnøgler og environment variables
- compose-opsætningen er ikke bundet til specifik maskine eller fast IP

Relevant opsætning:

- `docker-compose.yml`
- `.env.example`
- `src/Slottet.Api/appsettings.json`
- `src/Slottet.Web/appsettings.json`

### 7. Ingen binding til en enkelt fast server

Opfyldelse:

- arkitekturen er opdelt i frontend, API og ekstern database
- services kan i princippet flyttes mellem miljøer og containere

Forbehold:

- frontenden er bygget med Blazor Server og holder interaktive circuits på serversiden
- det betyder, at deployment i flere instanser kræver korrekt load balancing-strategi

Relevant kode:

- `src/Slottet.Web/Program.cs`
- `src/Slottet.Web/Components/Layout/ReconnectModal.razor`
- `src/Slottet.Web/Components/Layout/ReconnectModal.razor.js`

### 8. Horisontal skalering

Status:

- API: i høj grad opfyldt
- frontend: delvist opfyldt og kræver korrekt driftsopsætning

API:

- API'et er tæt på stateless
- auth sker via JWT
- data ligger i ekstern database
- API'et har health endpoint og kan derfor lettere skaleres og overvåges

Relevant kode:

- `src/Slottet.Api/Program.cs`
- `src/Slottet.Infrastructure/Auth/JwtTokenGenerator.cs`
- `src/Slottet.Api/Controllers`

Frontend:

- frontenden bruger Blazor Server via `AddInteractiveServerComponents()` i `src/Slottet.Web/Program.cs`
- derfor findes server-side circuit state
- Data Protection er nu delt centralt, hvilket er en vigtig forudsætning for multi-instance drift
- men en aktiv brugerforbindelse bør fortsætte mod samme instans

Relevant kode:

- `src/Slottet.Web/Program.cs`
- `src/Slottet.Web/Auth/BrowserSessionAuthStore.cs`
- `src/Slottet.Web/wwwroot/auth-storage.js`

Forsvar til eksamen:

- API'en kan beskrives som egnet til horisontal skalering
- frontenden kan indgå i et distribueret setup, men kræver sticky sessions eller tilsvarende ved load balancing

### 9. Load balancing og høj tilgængelighed

Status:

- teknisk forberedt
- ikke fuldt demonstreret i et rigtigt cloudmiljø i projektet

Det der er implementeret:

- health endpoints i både frontend og API
- Docker health checks i `docker-compose.yml`
- reverse proxy-understøttelse via forwarded headers i både frontend og API
- central Data Protection key store i databasen

Relevant kode og opsætning:

- `src/Slottet.Api/Program.cs`
- `src/Slottet.Web/Program.cs`
- `docker-compose.yml`
- `docker-compose.prod.yml`

Det der stadig afhænger af driftsmiljø:

- konkret load balancer eller ingress
- session affinity for Blazor Server-frontend
- egentlig høj tilgængelighed på platformniveau

Det er vigtigt i forsvaret at forklare forskellen mellem:

- “løsningen er teknisk forberedt til load balancing”
- og “vi har implementeret en konkret cloud load balancer i projektet”

## Sikkerheds- og driftsrelevante elementer

### JWT og login

- JWT-konfiguration i `src/Slottet.Api/Program.cs`
- token-generering i `src/Slottet.Infrastructure/Auth/JwtTokenGenerator.cs`
- login-service i `src/Slottet.Application/Services/Auth/LoginService.cs`
- login-controller i `src/Slottet.Api/Controllers/AuthController.cs`

### Rollebaseret adgangskontrol

Eksempler:

- `src/Slottet.Api/Controllers/AdminLogsController.cs`
- `src/Slottet.Api/Controllers/EmployeesController.cs`
- `src/Slottet.Api/Controllers/CitizensController.cs`

Her bruges `[Authorize]` og `[Authorize(Roles = ...)]` til at beskytte endpoints.

### Audit og adgangslogning

- adgangslogning via middleware i `src/Slottet.Infrastructure/Logging/AccessLogMiddleware.cs`
- auditdata gemmes via `src/Slottet.Infrastructure/Data/ApplicationDbContext.cs`
- audit endpoints findes i `src/Slottet.Api/Controllers/AdminLogsController.cs`

### Health checks

- API health endpoint mappes i `src/Slottet.Api/Program.cs`
- frontend health endpoint mappes i `src/Slottet.Web/Program.cs`
- Docker health checks findes i `docker-compose.yml`

## Hvad der er fuldt opfyldt og hvad der kræver forsvar

### Fuldt eller meget tæt på fuldt opfyldt

- ASP.NET Core og Blazor
- SQL Server og Entity Framework
- ekstern API
- Docker-containerisering
- standalone kørsel
- client-server i lokalt netværk
- cloud-ready retning
- central konfiguration via environment variables og secrets
- ingen binding til delt lokalt filsystem
- ingen fast IP eller maskinspecifik konfiguration

### Delvist opfyldt eller afhængigt af driftsmiljø

- horisontal skalering af frontend
- load balancing
- høj tilgængelighed

Det skyldes ikke, at løsningen er forkert, men at Blazor Server har nogle driftsmæssige konsekvenser, som kræver korrekt hostingopsætning.

## Teori til forsvar

Følgende formuleringer kan bruges direkte eller næsten direkte i eksamensforsvaret.

### Om systemtypen

“Løsningen er et distribueret client-server system. Frontenden er klientlaget, API'et er serverlaget, og databasen er en central ekstern ressource. Det er derfor ikke peer-to-peer, og det er heller ikke en egentlig microservice-arkitektur.”

### Om cloud readiness

“Løsningen er designet i en cloud-ready retning, fordi frontend og API er separate deploybare services, databasen er ekstern, og konfiguration sker via environment variables og secrets frem for maskinspecifik opsætning.”

### Om horisontal skalering

“API'et er det mest oplagte lag at skalere horisontalt, fordi det er tæt på stateless. Frontenden er bygget med Blazor Server og holder circuits på serversiden, så scale-out kræver ekstra hensyn som session affinity.”

### Om load balancing

“Vi har forberedt løsningen til load balancing gennem health checks, reverse proxy-understøttelse og delt Data Protection key storage. En egentlig produktionsopsætning kræver derudover en konkret load balancer eller ingress-konfiguration.”

### Om Data Protection

“For at undgå binding til lokalt filsystem i et multi-instance setup deler frontend-instanserne Data Protection keys via databasen. Det er vigtigt for beskyttede payloads og antiforgery i Blazor Server.”

## Resume

Løsningen opfylder langt størstedelen af kravene under teknologi og arkitektur. Det vigtigste, der kræver præcis mundtlig forklaring, er ikke om løsningen er distribueret, men hvordan frontendens Blazor Server-model påvirker load balancing og horisontal skalering. Det er derfor vigtigt at forklare, at API'en er lettest at skalere, mens frontend kræver sticky sessions eller tilsvarende ved flere instanser.
