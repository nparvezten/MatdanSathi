# Contributing to MatdarSathi (मतदार साथी)

Thank you for your interest in contributing to **MatdarSathi**! This open-source civic utility suite is built to help citizens and grassroots volunteers navigate electoral roll verification drives (such as India's Special Intensive Revision - SIR) while preserving 100% individual voter data privacy.

---

## 🚀 Quickstart: Setting Up Your Local Development Environment

Anyone can clone, run, and extend this project locally on Windows, macOS, or Linux.

### 1. Clone the Repository
```bash
git clone https://github.com/nparvezten/MatdarSathi.git
cd MatdarSathi
```

### 2. One-Command Bootstrap
Run the setup script to restore all C# packages, install Angular dependencies, and set up the Python virtual environment:
```bash
./setup.sh
```

---

## 🛠️ Architecture & Extension Guidelines

The codebase follows Clean Architecture and decoupled microservices:

```text
MatdarSathi/
├── backend/                  # C# ASP.NET Core 9/10 Web API
│   ├── API/                  # Controllers, Middlewares, Program.cs
│   ├── Application/          # Native MIT CQRS Queries, Commands, Validators
│   ├── Domain/               # Entities, Enums, Value Objects
│   └── Infrastructure/       # EF Core, AES-256 Encryption, HMAC Blind Indexes
├── frontend/                 # Angular 17+ Standalone Components (Signals + TailwindCSS)
│   └── src/app/
│       ├── components/       # Form Wizard, BLO Map Locator, Decoders
│       └── services/         # State Signal Stores & API Http Clients
├── parser-service/           # Python 3.11+ FastAPI Microservice (PDF Roll Extractor)
├── docs/                     # Architecture Decision Records (ADRs) & C4 Models
└── docker-compose.yml        # Full Multi-Container Orchestration
```

---

## ⚡ How to Extend Functionality

### 1. Adding a New Backend API Feature (Native CQRS)
MatdarSathi uses a **100% MIT-licensed Native C# Mediator pattern** (no paid or dual-licensed packages).

To add a new feature (e.g., `GetConstituencyStatsQuery`):
1. **Define Request & Response DTO**: Create a record implementing `IRequest<MyResponseDto>` in `backend/Application/<Feature>/Queries/`.
2. **Implement Handler**: Create a class implementing `IRequestHandler<MyQuery, MyResponseDto>`. The native mediator automatically registers it via assembly scanning.
3. **Expose Controller Endpoint**: Add an action method in `backend/API/Controllers/v1/` invoking `_mediator.Send(query)`.

### 2. Adding a New Frontend UI Feature (Angular Signals)
1. Components are Angular 17+ standalone components under `frontend/src/app/components/`.
2. Use Angular `signal()`, `computed()`, and reactive state stores instead of legacy RxJS subscriptions where applicable.

### 3. Extending the PDF Parser (Python FastAPI)
1. Add custom regex patterns or table extraction logic in `parser-service/parser.py`.
2. Ensure PDF parsing remains stream-based (`pypdf` / `PyMuPDF`) to keep memory usage under 100MB RAM.

---

## 🧪 Verification & Testing Rules

Before submitting a Pull Request, execute the automated build verification pipeline:

```bash
./run-pipeline.sh
```

This script automatically verifies:
- ✅ C# .NET API compilation & static code formatting (`dotnet build`, `dotnet format`).
- ✅ xUnit backend unit tests (`dotnet test`).
- ✅ Python FastAPI pytest & Bandit SAST security scans (`pytest`, `bandit`).
- ✅ Angular standalone app compilation (`npm run build`).
- ✅ Native Capacitor mobile asset synchronization (`npx cap sync`).

---

## 🔒 Security & Licensing Policies
- **Licensing Mandate**: Only permissive open-source packages (MIT, Apache 2.0, BSD) are allowed. Banned: Commercial or dual-licensed packages (`MediatR` v13+, `AutoMapper` v15+, `iTextSharp`, `QuestPDF`).
- **Privacy Guardrail**: Never persist unencrypted voter PII (Names, Phone Numbers, DOB, House Numbers) in database tables. Always use AES-256 field encryption and HMAC-SHA256 blind indexing for exact-match lookups.
