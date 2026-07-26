# Architecture Decision Records (ADRs) & System Design for MatdanSathi

## 📌 Index of Architecture Decision Records

| ADR ID | Title | Status | Date |
|---|---|---|---|
| [ADR-001](#adr-001-native-mit-c-mediator-cqrs-pattern) | Native MIT C# Mediator CQRS Pattern over Commercial Dependencies | Accepted | 2026-07-26 |
| [ADR-002](#adr-002-zero-raw-pii-persistence-via-aes-256--hmac-sha256-blind-indexes) | Zero Raw PII Persistence via AES-256 Encryption & Deterministic Blind Indexes | Accepted | 2026-07-26 |
| [ADR-003](#adr-003-decoupled-streaming-python-parser-microservice) | Decoupled Streaming Python Parser Microservice (pypdf / PyMuPDF) | Accepted | 2026-07-26 |
| [ADR-004](#adr-004-angular-17-signals--capacitor-native-hybrid-wrapper) | Angular 17+ Standalone Signals & Capacitor Cross-Platform Architecture | Accepted | 2026-07-26 |

---

### ADR-001: Native MIT C# Mediator CQRS Pattern

*   **Status**: Accepted
*   **Context**: MediatR v13+ moved to a paid dual-license model. To guarantee that MatdanSathi remains 100% open-source, permissive (MIT/Apache 2.0), and cost-free for public civic deployment, all commercial OR dual-licensed packages must be eliminated.
*   **Decision**: Implement a zero-dependency, native C# `IMediator`, `IRequest<TResponse>`, and `IRequestHandler<TRequest, TResponse>` pattern. Handlers are registered via assembly reflection scan (`services.AddScoped(interface, implementation)`) and executed dynamically via `NativeMediator`.
*   **Consequences**: Eliminates legal licensing risks, reduces runtime memory footprint, eliminates external package bloat, and preserves 100% MediatR CQRS decoupling semantics.

---

### ADR-002: Zero Raw PII Persistence via AES-256 & HMAC-SHA256 Blind Indexes

*   **Status**: Accepted
*   **Context**: Storing cleartext voter PII (Names, Phone Numbers, Email, DOB, House Numbers) creates severe data privacy and legal compliance liabilities.
*   **Decision**: 
    1. All sensitive voter PII fields are encrypted at rest using AES-256 field encryption.
    2. For exact-match queries (e.g. checking EPIC or Phone), generate a deterministic `HMAC-SHA256` blind index with a server secret salt.
*   **Consequences**: Databases containing compromised backups reveal zero readable PII. Enables exact-match lookup speed without sacrificing privacy. Partial fuzzy searches are restricted to non-sensitive fields.

---

### ADR-003: Decoupled Streaming Python Parser Microservice

*   **Status**: Accepted
*   **Context**: Parsing 100+ page Indian Electoral Roll PDFs requires heavy text extraction and regex matching that would bloat the main Web API process.
*   **Decision**: Decouple PDF extraction into a dedicated FastAPI Python microservice using stream-based `pypdf` (BSD 3-Clause) and `PyMuPDF` engines.
*   **Consequences**: Prevents memory spikes on the .NET API container. Memory-bounded page-by-page streaming allows handling large PDF rolls under 100MB RAM usage.

---

### ADR-004: Angular 17+ Signals & Capacitor Native Hybrid Wrapper

*   **Status**: Accepted
*   **Context**: Grassroots volunteers require cross-platform availability across Web Browsers, PWAs, Android, and iOS devices with offline local storage sync.
*   **Decision**: Use Angular 17 Standalone Components with Signals for fine-grained reactive state management. Wrap the web build with `@capacitor/core` for native iOS (WKWebView) and Android deployment.
*   **Consequences**: Single TypeScript codebase targets Web, PWA, Android APK, and iOS IPA targets without duplicate code maintenance.

---

## 🏛️ C4 Architecture Model

```mermaid
C4Context
    title Level 1: System Context Diagram for MatdanSathi

    Person(citizen, "Voter / Citizen", "Searches roll status, checks deletions/transfers, and finds prescribed SIR proof documents.")
    Person(volunteer, "Community Volunteer", "Logs physical BLO notice slips, generates AERO hearing dossiers, and conducts field verification.")

    System(matdansathi, "MatdanSathi Suite", "Privacy-first voter companion suite providing roll verification, anomaly guidance, and BLO locator services.")

    System_Ext(eci, "ECI Voters Service Portal", "Official Election Commission of India Portal (voters.eci.gov.in) for Form 6/7/8 submissions.")
    System_Ext(maps, "OpenStreetMap / Leaflet", "Tile servers for rendering neutral government booth facility locations.")

    Rel(citizen, matdansathi, "Searches voter roll & generates SIR hearing dossiers", "HTTPS / Angular PWA")
    Rel(volunteer, matdansathi, "Logs BLO visit notice slips & verifies booth records", "HTTPS / Capacitor Mobile")
    Rel(matdansathi, eci, "Generates pre-filled direct application links", "HTTPS Redirect")
    Rel(matdansathi, maps, "Fetches neutral booth map tiles", "HTTPS / OpenStreetMap API")
```

```mermaid
C4Container
    title Level 2: Container Diagram for MatdanSathi

    Person(user, "User (Citizen / Volunteer)", "Accesses suite via Web Browser, PWA, or Native Mobile App")

    Container(frontend, "Angular 17 Standalone PWA", "TypeScript, Signals, TailwindCSS, Capacitor", "Delivers reactive UI, SIR wizard steps, offline storage sync, and map locators")
    Container(backend, "C# ASP.NET Core API", ".NET 9/10, Clean Architecture, Native CQRS", "Processes business logic, JWT validation, rate limiting, and encrypted queries")
    Container(parser, "FastAPI Parser Microservice", "Python 3.11, pypdf, PyMuPDF", "Streams and extracts tabular voter records from uploaded PDF rolls")
    ContainerDb(database, "PostgreSQL Database", "PostgreSQL 15+", "Stores AES-256 encrypted voter records, HMAC blind indexes, and visit slips")

    Rel(user, frontend, "Uses", "HTTPS / Port 4200")
    Rel(frontend, backend, "Makes API calls", "JSON / HTTPS / Port 5103")
    Rel(backend, parser, "Delegates PDF roll ingestion", "REST / HTTP / Port 8000")
    Rel(backend, database, "Reads/Writes encrypted data", "Npgsql / Port 5432")
```

```mermaid
C4Component
    title Level 3: Component Diagram for Backend C# .NET Core API

    Container(frontend, "Angular 17 PWA", "Frontend Client")
    ContainerDb(database, "PostgreSQL", "Database")

    Container_Boundary(backend_boundary, "Backend C# .NET API Boundary") {
        Component(wizard_ctrl, "WizardController", "ASP.NET Core Controller", "Exposes /api/v1/wizard/ endpoints")
        Component(blo_ctrl, "BloController", "ASP.NET Core Controller", "Exposes /api/v1/blo/ endpoints")
        Component(voters_ctrl, "VotersController", "ASP.NET Core Controller", "Exposes /api/v1/voters/ endpoints")
        
        Component(mediator, "NativeMediator", "Native C# Service", "Zero-dependency MIT mediator routing queries & validating inputs")
        
        Component(guidance_handler, "GetAnomalyGuidanceQueryHandler", "CQRS Handler", "Calculates ECI era cutoff rules (Pre-1987, 1987-2004, Post-2004)")
        Component(dossier_handler, "GenerateHearingDossierCommandHandler", "CQRS Handler", "Formats official AERO hearing cover sheet payloads")
        Component(crypto, "CryptographyService", "Infrastructure Service", "Performs AES-256 encryption & HMAC-SHA256 blind indexing")
        Component(efcore, "ApplicationDbContext", "EF Core Layer", "Executes parameterized SQL queries against PostgreSQL")
    }

    Rel(frontend, wizard_ctrl, "Requests guidance & dossiers", "HTTP POST/GET")
    Rel(frontend, blo_ctrl, "Schedules BLO visit slips", "HTTP POST")
    Rel(frontend, voters_ctrl, "Queries voter check", "HTTP POST")

    Rel(wizard_ctrl, mediator, "Sends GetAnomalyGuidanceQuery", "In-Process Call")
    Rel(blo_ctrl, mediator, "Sends ScheduleBloVisitCommand", "In-Process Call")
    Rel(voters_ctrl, mediator, "Sends CheckVoterRegistrationQuery", "In-Process Call")

    Rel(mediator, guidance_handler, "Dispatches", "In-Process Call")
    Rel(mediator, dossier_handler, "Dispatches", "In-Process Call")

    Rel(guidance_handler, crypto, "Encrypts/Decrypts PII", "In-Process Call")
    Rel(guidance_handler, efcore, "Queries DB", "In-Process Call")
    Rel(efcore, database, "Executes SQL", "Npgsql Connection")
```

---

## 🔄 Sequence Diagrams (Runtime Flows)

### Sequence 1: Deterministic Blind Index Voter Search & Verification

```mermaid
sequenceDiagram
    autonumber
    actor Volunteer as Community Volunteer
    participant SPA as Angular 17 SPA
    participant API as C# .NET API
    participant Crypto as CryptographyService
    participant DB as PostgreSQL DB

    Volunteer->>SPA: Enters Voter EPIC Number (e.g. SLD1234567)
    SPA->>API: POST /api/v1/voters/check { epicNumber: "SLD1234567" }
    API->>Crypto: ComputeBlindIndex("SLD1234567", Salt)
    Crypto-->>API: Returns HMAC-SHA256 Hash ("a8f9c2...")
    API->>DB: SELECT * FROM Voters WHERE EpicBlindIndex = 'a8f9c2...'
    DB-->>API: Returns Encrypted Voter Record
    API->>Crypto: DecryptPii(EncryptedName, EncryptedHouse)
    Crypto-->>API: Returns Decrypted ("Saraswati Khan", "42-A/1")
    API-->>SPA: 200 OK { Status: "Active", Name: "Saraswati Khan", HouseNo: "42-A/1" }
    SPA-->>Volunteer: Displays Active Verification Badge on UI Map
```

### Sequence 2: SIR Anomaly Guidance & AERO Hearing Dossier Flow

```mermaid
sequenceDiagram
    autonumber
    actor Voter as Elector / Citizen
    participant Assistant as Angular Anomaly Assistant
    participant API as C# .NET API
    participant Handler as Guidance & Dossier Handlers

    Voter->>Assistant: Selects Anomaly ("SurnameMarriageChange") & Year of Birth (1982)
    Assistant->>API: GET /api/v1/wizard/guidance?age=44&birthYear=1982&anomalyType=SurnameMarriageChange
    API->>Handler: Send(GetAnomalyGuidanceQuery)
    Handler-->>API: Returns Guidance { Era: "Pre1987", RequiredSelfProofs: 1, RequiredParentProofs: 0 }
    API-->>Assistant: 200 OK { 12 Prescribed Proof Checklist }
    Assistant-->>Voter: Renders Self Proof selector (Aadhaar, Passport, SLC)
    
    Voter->>Assistant: Clicks "Generate AERO Hearing Dossier"
    Assistant->>API: POST /api/v1/wizard/generate-hearing-dossier { VoterDetails, SelectedProofs }
    API->>Handler: Send(GenerateHearingDossierCommand)
    Handler-->>API: Formats Official Cover Sheet { DossierRef: "AERO-DOSSIER-991203" }
    API-->>Assistant: 200 OK { HearingNoticeText, DossierRef, IsReadyForPrint: true }
    Assistant-->>Voter: Renders Printable AERO Dossier Sheet & triggers window.print()
```
