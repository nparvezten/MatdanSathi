# MatdanSathi (मतदान साथी) — Voter Companion Suite

MatdanSathi is an open-source, privacy-first civic utility application built to empower volunteers and citizens during electoral roll verification drives (such as India's Special Intensive Revision - SIR). It enables voters and grass-roots volunteers to search local rolls, detect unexpected deletions/transfers, locate nearby Booth Level Officers (BLOs), decode legacy voter IDs, and prepare Form 6, 7, and 8 applications without compromising individual personal data privacy.

---

## 🏗️ System Architecture

The codebase is organized into four decoupled layers:

*   `frontend/`: **Angular 17+ Standalone Application** using Signals, RxJS, and TailwindCSS. Includes PWA capabilities and Capacitor wrappers for iOS (WKWebView) and Android (WebView) native deployment.
*   `backend/`: **C# ASP.NET Core 9/10 Web API** following Clean Architecture, CQRS pattern via MediatR, FluentValidation, Serilog, rate-limiting middlewares, and PostgreSQL persistence.
*   `parser-service/`: **Python 3.11+ FastAPI Microservice** using PyMuPDF (`fitz`) for secure, streaming extraction of electoral roll PDF documents.
*   `docker-compose.yml`: Full multi-container orchestration for one-command deployment.

---

## ⚡ Quickstart Guide: How to Run the Project

Anyone who clones or forks this repository can run the entire suite locally using either Docker or direct native execution.

### Method 1: One-Command Setup with Docker (Recommended)

If you have [Docker Desktop](https://www.docker.com/) installed:

```bash
# 1. Clone the repository
git clone https://github.com/nparvezten/MatdanSathi.git
cd MatdanSathi

# 2. Build and start all services (PostgreSQL, .NET API, Python Parser, Angular Frontend)
docker compose up --build
```

Access the application in your browser:
*   **Web Dashboard & Public Utilities:** `http://localhost:4200`
*   **C# .NET API Backend:** `http://localhost:5103`
*   **Python Parser Service:** `http://localhost:8000`

---

### Method 2: Manual Local Development

If you prefer to run services manually on your local system:

#### Prerequisites
- **.NET 9.0/10 SDK**: [Download .NET](https://dotnet.microsoft.com/download)
- **Node.js 20+ & npm**: [Download Node.js](https://nodejs.org/)
- **Python 3.11+**: [Download Python](https://www.python.org/)
- **PostgreSQL 15+** (Running locally on port `5432` with username/password `postgres`/`postgres`)

#### Step 1: Start the Backend .NET API
```bash
dotnet run --project backend/API/MatdanSathi.API.csproj --launch-profile http
```
*(The backend automatically runs EF Core migrations and seeds baseline test records on startup).*

#### Step 2: Start the Python Parser Microservice
```bash
cd parser-service
python3 -m venv .venv
source .venv/bin/activate  # On Windows use: .venv\Scripts\activate
pip install -r requirements.txt
uvicorn main:app --host 0.0.0.0 --port 8000
```

#### Step 3: Start the Angular Frontend Application
```bash
cd frontend
npm install
npx ng serve --port 4200 --proxy-config proxy.conf.json
```

---

## 🔑 Demo Sandbox Credentials & Test Data

The application comes pre-loaded with sample baseline voter profiles and volunteer credentials for immediate testing:

### Volunteer Login Credentials
*   **Email:** `verifier@matdansathi.org`
*   **Password:** `SecurePassword123!`

### Sample Active EPIC Numbers (Form 8 Correction Lookup)
*   `SLD1234567` — Khan Saidnabi (Age 78, House `42-A/1`)
*   `SLD9876543` — Ramesh Sawant (Age 45, House `42-A/2`)
*   `SLD2345678` — Deepa Joshi (Age 29, House `43`)
*   `SLD5544332` — Imran Shaikh (Age 42, House `45/A`)
*   `SLD6677889` — Farida Begum (Age 38, House `48`)

### Sample Legacy EPIC ID (Pre-2008 Archival Decoder)
*   `MT/05/025/180293` — Decodes state segment (`MT` / Maharashtra), Lok Sabha 05, Assembly part 25, and Serial 180293 for historical searches.

---

## 🧪 Automated Verification & Test Suite

To verify system integrity, run the automated pipeline script from the root directory:

```bash
./run-pipeline.sh
```

This script automatically executes:
1. Backend C# build, `dotnet format` static code analysis, and xUnit test suite.
2. Python pytest unit tests and Bandit SAST security analysis.
3. Angular standalone production compilation.
4. Native Capacitor iOS/Android asset synchronization.

---

## 🛡️ Security, Privacy & DPDP Act 2023 Compliance

To eliminate data leakage liability, MatdanSathi adheres to strict security standards:
1. **AES-256 Field Encryption:** Cleartext names, phone numbers, and dates of birth are encrypted before database persistence.
2. **HMAC-SHA256 Blind Indexing:** Verification lookups query one-way cryptographic hashes, preventing database exposure during SQL execution.
3. **Explicit Consent & Rate Limiting:** All verification queries require user privacy consent and are rate-limited (`strict-limit`) against web scrapers.

---

## 📝 License

Distributed under the **MIT License**. See `LICENSE` for more information.