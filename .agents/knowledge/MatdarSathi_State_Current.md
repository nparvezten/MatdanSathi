# MatdarSathi Project State & Architecture Hand-off (MatdarSathi_State_Current)

## 1. Current State
- **Backend (.NET 10 Web API)**:
  - `POST /api/v1/ingestion/upload` (NEW): Accepts multipart PDF file + `boothId` under `[Authorize]` filter to persist `RollIngestionBatch` records and store raw files in local `.gitignore`'d `backend/API/UploadedRolls/` directory.
  - `RollIngestionBackgroundService` (NEW): Background hosted service (`IHostedService`) that processes pending draft roll batches, extracts records via Python parser microservice, computes HMAC-SHA256 blind indexes and AES-256 encrypted fields, and runs comparative matching via `IWatchdogComparisonService`.
  - `POST /api/v1/anomalies`: Accepts `SubmitLegacyAnomalyCommand` to log certified extract receipt numbers, deceased elector exceptions, historical roll mappings, and family household bundles.
  - `POST /api/v1/auth/register-volunteer`, `GET /api/v1/admin/volunteers`, `POST /api/v1/admin/approve-volunteer`, `POST /api/v1/admin/reject-volunteer`: Super Admin volunteer authorization & signup flow.
  - Native CQRS Infrastructure: Powered by native `IRequestHandler<TRequest, TResponse>` reflection scanning (zero `MediatR` v13+ or `AutoMapper` v15+ dependencies). 16 of 16 xUnit backend tests passing.
- **Frontend (Angular 17 Standalone Components)**:
  - `RollIngestionUploadComponent` (`app-roll-ingestion-upload`, NEW): Standalone Signals + TailwindCSS component for volunteers to upload booth-wise PDF draft rolls and track Pending/Parsing/Parsed/Failed Watchdog status.
  - `AnomalyWizardComponent` (`app-anomaly-wizard`): Dynamic Reactive Form & Signals component for logging legacy extracts and dynamic family household bundles.
  - `Historical Electoral Roll Guide` (`app.component.html`): Features prominent pre-2002 deceased elector exception alert banner, online PDF links (`https://electiondata.mcgm.gov.in/`), CEO Maharashtra portal (`https://ceoelection.maharashtra.gov.in/`), and physical directions to the BMC Election Head Office (Masjid Bunder) & District Collectorates/Tehsildars.
  - GitHub Pages Live Deployment: Built with `--base-href=/MatdarSathi/` and published from `frontend/dist/frontend/browser` with SPA `404.html` fallback at **`https://nparvezten.github.io/MatdarSathi/`**.
- **Python FastAPI Service**: PDF extraction microservice (`POST /api/v1/parser/parse`) running on port 8000 using PyMuPDF (fitz) and pypdf, protected by `X-API-KEY`. Added defensive fallbacks for malformed/corrupted PDFs (8 of 8 pytests passing, 0 Bandit security findings).

## 2. Context & Security
- **Strict PII Protection**: Zero raw unencrypted PII persistence. PII fields (Voter Names, Deceased Names, Death Cert Reg Numbers, Family Bundle JSON, Contacts) encrypted via AES-256 before EF Core persistence.
- **Deterministic Blind Indexing**: Uses HMAC-SHA256 blind indexes (`EpicNumberBlindIndex`, `DeceasedNameBlindIndex`) to allow exact-match database queries without cleartext exposure.
- **Internal Microservice Security**: Python parser service requires `X-API-KEY: matdarsathi-secure-internal-token-2026` header.
- **CORS & API Versioning**: All controllers annotated with `[ApiVersion("1.0")]` and `[Route("api/v{version:apiVersion}/[controller]")]`. Global CORS policy configured via `builder.Services.AddCors(...)` before authentication middleware.
- **Local Dev Database Fallback**: EF Core automatically falls back to SQLite (`matdarsathi_dev.db`) if PostgreSQL is unconfigured or offline.
- **Deployment Script**: Integrated `npm --prefix frontend run deploy` command targeting `frontend/dist/frontend/browser` to publish clean gh-pages builds.

## 3. Blockers / Deferred
- **Government Portal Web Scraping**: Direct automated scraping of `ceoelection.maharashtra.gov.in` intentionally avoided due to ToS and portal fragility; replaced by volunteer bulk PDF upload workflow.
- **Offline Sync Queue**: Volunteer registrations and anomaly records save locally (`localStorage`) during backend offline states; background sync mechanism can be expanded for full service worker caching.

## 4. Next Steps
1. **Verify Live GitHub Pages Site**: Confirm **`https://nparvezten.github.io/MatdarSathi/`** renders styled dark-mode UI with draft roll bulk ingestion upload form under Volunteer Portal.
2. **Expand E2E Playwright Tests**: Write Playwright E2E browser automation scripts verifying end-to-end draft roll upload, background Watchdog processing, and anomaly submission flows.
3. **PWA Offline Caching & Service Worker**: Enhance `ngsw-config.json` for offline asset caching across Capacitor mobile iOS/Android webviews.
