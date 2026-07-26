# Enterprise Architecture Specification (ADRs, C4 Model & Sequence Diagrams)

---

## 1. Architecture Decision Records (ADRs)

### ADR-001: Selection of Native C# Mediator over External MediatR Library
*   **Status**: Accepted
*   **Date**: 2026-07-26
*   **Context**: MediatR v13+ moved to a commercial dual-license model. To comply with strict open-source mandates and ensure MatdarSathi can be freely hosted, modified, and redistributed by public civic volunteers without licensing fees, external MediatR dependencies had to be eliminated.
*   **Decision**: Implement a native C# `IMediator` pattern in the Application layer using reflection-based assembly scanning (`services.AddScoped(interface, implementation)`).
*   **Consequences**: 
    *   Zero reliance on commercial NuGet packages (100% MIT-licensed codebase).
    *   Reduced memory footprint and zero external dependency risk.
    *   Maintains strict CQRS Command/Query isolation and SOLID principles.

---

### ADR-002: Selection of PostgreSQL over MongoDB
*   **Status**: Accepted
*   **Date**: 2026-07-26
*   **Context**: Electoral roll data requires strict relational integrity (Constituency -> Polling Station -> Section -> Voter), transactional consistency (ACID) for BLO visit scheduling, and spatial indexing for booth geolocation map lookups.
*   **Decision**: Select PostgreSQL with Entity Framework Core instead of MongoDB or NoSQL document stores.
*   **Consequences**: 
    *   Enforces strict foreign key constraints between voters, polling stations, and visit slips.
    *   Supports PostGIS spatial queries for proximity matching of neutral polling booths.
    *   ACID transactions prevent race conditions when scheduling physical BLO visits.

---

### ADR-003: Zero Raw PII Persistence via AES-256 Encryption & HMAC-SHA256 Blind Indexes
*   **Status**: Accepted
*   **Date**: 2026-07-26
*   **Context**: Storing raw unencrypted voter PII (Names, Addresses, Phone Numbers) exposes the system to legal liabilities and data privacy leaks if a database dump is compromised.
*   **Decision**: All PII fields are encrypted at rest using AES-256. Exact-match queries (EPIC, Phone) use deterministic `HMAC-SHA256` blind indexes with a server secret salt.
*   **Consequences**: 
    *   Stolen database backups reveal no plain-text personal information.
    *   Enables $O(1)$ fast indexed queries for exact matches without decrypting the entire database.

---

### ADR-004: Decoupled Python FastAPI Microservice for PDF Electoral Roll Parsing
*   **Status**: Accepted
*   **Date**: 2026-07-26
*   **Context**: Heavy PDF parsing of 100+ page Indian Electoral Rolls in the main Web API process causes memory spikes and thread pool starvation.
*   **Decision**: Isolate PDF parsing into a lightweight Python 3.11 FastAPI microservice using stream extraction with `pypdf` (BSD) and `PyMuPDF`.
*   **Consequences**: 
    *   Isolates heavy regex and text processing memory overhead to an isolated container.
    *   Page-by-page stream generator keeps RAM consumption strictly under 100MB even for 500-page rolls.

---

## 2. C4 Model Diagrams

### Level 1: System Context Diagram
High-level view showing users and external systems interacting with MatdarSathi.

```mermaid
graph TD
    classDef citizenFill fill:#0f2027,stroke:#203a43,color:#fff;
    classDef sysFill fill:#11998e,stroke:#38ef7d,color:#fff;
    classDef extFill fill:#1f1c2c,stroke:#928dab,color:#fff;

    Citizen["👤 Voter / Citizen<br/>(Checks roll status, surname discrepancies)"]:::citizenFill
    Volunteer["🧑‍💼 Community Volunteer<br/>(Logs BLO notice slips, field verification)"]:::citizenFill

    MatdarSathi["🏛️ MatdarSathi Civic Suite<br/>(Privacy-first voter companion, SIR wizard, map locator)"]:::sysFill

    ECIPortal["🌐 ECI Voters Service Portal<br/>(voters.eci.gov.in)"]:::extFill
    OpenStreetMap["🗺️ OpenStreetMap / Leaflet<br/>(Neutral booth tile map)"]:::extFill

    Citizen -->|"1. Searches voter status & generates AERO dossiers"| MatdarSathi
    Volunteer -->|"2. Schedules BLO visit slips & verifies booth records"| MatdarSathi
    MatdarSathi -->|"3. Directs pre-filled Form 6/7/8 submissions"| ECIPortal
    MatdarSathi -->|"4. Fetches neutral booth location tiles"| OpenStreetMap
```

---

### Level 2: Container Diagram
Applications, microservices, databases, and network boundaries.

```mermaid
graph TB
    subgraph ClientLayer ["Client Devices & Browsers"]
        Browser["🌐 Web Browser / PWA<br/>(Angular 17 Standalone)"]
        MobileApp["📱 Capacitor Mobile App<br/>(Android APK / iOS IPA)"]
    end

    subgraph BackendLayer ["Application & Microservices Boundary"]
        DotNetAPI["⚡ C# ASP.NET Core 9/10 API<br/>(Clean Architecture, Native CQRS, JWT Auth)<br/>Port: 5103"]
        PythonParser["🐍 Python FastAPI Microservice<br/>(pypdf / PyMuPDF Roll Extractor)<br/>Port: 8000"]
    end

    subgraph DatabaseLayer ["Data Persistence"]
        PostgreSQL[("🗄️ PostgreSQL Database<br/>(AES-256 Encrypted PII, HMAC Blind Indexes)<br/>Port: 5432")]
    end

    Browser -->|"JSON / HTTPS"| DotNetAPI
    MobileApp -->|"JSON / HTTPS"| DotNetAPI
    DotNetAPI -->|"REST / HTTP"| PythonParser
    DotNetAPI -->|"Npgsql SQL Connection"| PostgreSQL
```

---

### Level 3: Component Diagram (.NET Core Backend API)
Internal software components inside the C# Web API.

```mermaid
graph LR
    subgraph Presentation ["Presentation Layer"]
        VotersCtrl["VotersController"]
        WizardCtrl["WizardController"]
        BloCtrl["BloController"]
        AuthCtrl["AuthController"]
    end

    subgraph CoreApplication ["Application Layer (Native CQRS)"]
        Mediator["NativeMediator<br/>(Zero-dependency MIT Mediator)"]
        CheckVoterQuery["CheckVoterRegistrationQuery"]
        GuidanceQuery["GetAnomalyGuidanceQuery"]
        DossierCommand["GenerateHearingDossierCommand"]
        ScheduleVisitCmd["ScheduleBloVisitCommand"]
    end

    subgraph InfrastructureLayer ["Infrastructure & Persistence Layer"]
        CryptoService["CryptographyService<br/>(AES-256 / HMAC-SHA256)"]
        AppDbContext["ApplicationDbContext<br/>(EF Core 9/10)"]
    end

    VotersCtrl --> Mediator
    WizardCtrl --> Mediator
    BloCtrl --> Mediator
    AuthCtrl --> CryptoService

    Mediator --> CheckVoterQuery
    Mediator --> GuidanceQuery
    Mediator --> DossierCommand
    Mediator --> ScheduleVisitCmd

    CheckVoterQuery --> CryptoService
    CheckVoterQuery --> AppDbContext
    GuidanceQuery --> AppDbContext
    ScheduleVisitCmd --> CryptoService
    ScheduleVisitCmd --> AppDbContext
```

---

### Level 4: Code Diagram (Class & Entity Relationship Diagram)
Database Entities and Domain Objects.

```mermaid
erDiagram
    POLLING_STATION ||--o{ VOTER_RECORD : "contains"
    POLLING_STATION ||--o{ VISIT_SLIP : "assigned_to"
    VOTER_RECORD ||--o{ VISIT_SLIP : "logs_notice_for"

    POLLING_STATION {
        int Id PK
        string StationName
        string StationLocation
        string AssemblyConstituency
        string PartNumber
        double Latitude
        double Longitude
        string BloName
        string BloContact
        bool IsNeutralGovtFacility
    }

    VOTER_RECORD {
        int Id PK
        string EpicBlindIndex SK
        string EncryptedFullName
        string EncryptedHouseNo
        int Age
        string Gender
        string SectionNumber
        int SerialNumber
        string Status
    }

    VISIT_SLIP {
        int Id PK
        string NoticeSlipNumber UK
        string EncryptedVoterName
        string EncryptedContactNumber
        string PreferredDate
        string PreferredTimeSlot
        string HouseNo
        string PollingStationName
        string Notes
        string Status
        DateTime CreatedAt
    }

    USER_VERIFIER {
        int Id PK
        string Email UK
        string PasswordHash
        string Role
        string AssemblyConstituency
    }
```

---

## 3. Sequence Diagrams (UML Runtime Flows)

### Sequence 1: Volunteer Authentication & JWT Token Flow

```mermaid
sequenceDiagram
    autonumber
    actor Volunteer as Community Volunteer
    participant SPA as Angular 17 SPA
    participant Auth as AuthController
    participant Config as App Settings / Secrets

    Volunteer->>SPA: Enters credentials (verifier@matdarsathi.org)
    SPA->>Auth: POST /api/v1/auth/login { email, password }
    Auth->>Config: Validate verifier credentials & get JWT Secret Key
    Config-->>Auth: Secret Key ("super-secret-secure-key...")
    Auth->>Auth: Generate Signed JWT Token (Expiry: 120 mins, Role: Verifier)
    Auth-->>SPA: 200 OK { token: "eyJhbGciOi...", expiry: "..." }
    SPA->>SPA: Stores JWT token in memory / Auth State
    SPA-->>Volunteer: Displays Authenticated Volunteer Portal Dashboard
```

---

### Sequence 2: Deterministic Blind Index Voter Lookup Flow

```mermaid
sequenceDiagram
    autonumber
    actor User as Citizen / Volunteer
    participant SPA as Angular 17 SPA
    participant API as VotersController
    participant Crypto as CryptographyService
    participant DB as PostgreSQL DB

    User->>SPA: Types EPIC Number ("SLD1234567")
    SPA->>API: POST /api/v1/voters/check { epicNumber: "SLD1234567" }
    API->>Crypto: ComputeBlindIndex("SLD1234567", Salt)
    Crypto-->>API: Returns HMAC-SHA256 Hash ("e3b0c442...")
    API->>DB: SELECT * FROM Voters WHERE EpicBlindIndex = 'e3b0c442...'
    DB-->>API: Returns Encrypted Record { EncryptedName: "...", EncryptedHouse: "..." }
    API->>Crypto: DecryptPii(EncryptedName, EncryptedHouse)
    Crypto-->>API: Returns Plaintext ("Saraswati Khan", "42-A/1")
    API-->>SPA: 200 OK { Status: "Active", Name: "Saraswati Khan", HouseNo: "42-A/1" }
    SPA-->>User: Renders Green Active Verification Badge on Booth Map
```

---

### Sequence 3: Physical BLO Notice Visit Slip Scheduling Flow

```mermaid
sequenceDiagram
    autonumber
    actor Voter as Elector / Resident
    participant SPA as Angular 17 SPA
    participant API as BloController
    participant NativeMediator as NativeMediator (MIT CQRS)
    participant CmdHandler as ScheduleBloVisitCommandHandler
    participant DB as PostgreSQL DB

    Voter->>SPA: Fills physical BLO notice slip form (Slip # SLIP-2026-8891)
    SPA->>API: POST /api/v1/blo/schedule-visit { NoticeSlipNumber, PreferredDate, TimeSlot }
    API->>NativeMediator: Send(ScheduleBloVisitCommand)
    NativeMediator->>NativeMediator: Executes FluentValidation checks
    NativeMediator->>CmdHandler: Handle(ScheduleBloVisitCommand)
    CmdHandler->>DB: Saves encrypted VisitSlip record
    DB-->>CmdHandler: VisitSlip Entity Saved (Id: 104)
    CmdHandler-->>API: Returns VisitSlipDto { ConfirmationCode: "SLIP-2026-8891", Status: "Scheduled" }
    API-->>SPA: 200 OK { Confirmation Summary }
    SPA-->>Voter: Renders Confirmation Card with Assigned BLO Details
```

---

### Sequence 4: SIR Anomaly Guidance & AERO Hearing Dossier Flow

```mermaid
sequenceDiagram
    autonumber
    actor Citizen as Elector / Citizen
    participant Wizard as Angular Anomaly Wizard
    participant API as WizardController
    participant GuidanceHandler as GetAnomalyGuidanceQueryHandler
    participant DossierHandler as GenerateHearingDossierCommandHandler

    Citizen->>Wizard: Selects Anomaly ("SurnameMarriageChange") & Birth Year (1982)
    Wizard->>API: GET /api/v1/wizard/guidance?age=44&birthYear=1982&anomalyType=SurnameMarriageChange
    API->>GuidanceHandler: Handle(GetAnomalyGuidanceQuery)
    GuidanceHandler-->>API: Returns GuidanceResponseDto { Era: "Pre1987", RequiredSelfProofs: 1, RequiredParentProofs: 0 }
    API-->>Wizard: 200 OK { 12 Prescribed Proof Checklist }
    Wizard-->>Citizen: Renders Self Proof selector (Aadhaar, Passport, SLC)
    
    Citizen->>Wizard: Clicks "Generate AERO Hearing Dossier"
    Wizard->>API: POST /api/v1/wizard/generate-hearing-dossier { VoterName, SelectedProofs }
    API->>DossierHandler: Handle(GenerateHearingDossierCommand)
    DossierHandler-->>API: Formats Official AERO Cover Sheet { DossierRef: "AERO-DOSSIER-991203" }
    API-->>Wizard: 200 OK { HearingNoticeText, DossierRef, IsReadyForPrint: true }
    Wizard-->>Citizen: Displays Formatted AERO Cover Sheet & triggers window.print()
```
