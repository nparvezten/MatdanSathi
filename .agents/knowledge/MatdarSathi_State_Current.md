# MatdarSathi Project State & Architecture Hand-off (MatdarSathi_State_Current)

## 1. Current State
- **Backend (.NET 10 Web API)**:
  - `POST /api/v1/anomalies`: Accepts `SubmitLegacyAnomalyCommand` to log certified extract receipt numbers, deceased elector exceptions, historical roll mappings, and family household bundles.
  - `POST /api/v1/auth/register-volunteer`: Public volunteer signup creating pending applications.
  - `GET /api/v1/admin/volunteers`: Fetches pending volunteer applications for Super Admin review.
  - `POST /api/v1/admin/approve-volunteer` & `POST /api/v1/admin/reject-volunteer`: Approves/rejects volunteer access.
  - Native CQRS Infrastructure: Powered by native `IRequestHandler<TRequest, TResponse>` reflection scanning (zero `MediatR` v13+ or `AutoMapper` v15+ dependencies).
- **Frontend (Angular 17 Standalone Components)**:
  - `AnomalyWizardComponent` (`app-anomaly-wizard`): Dynamic Reactive Form & Signals component for logging legacy extracts and dynamic family household bundles.
  - `Historical Electoral Roll Guide` (`app.component.html`): Features prominent pre-2002 deceased elector exception alert banner, online PDF links (`https://electiondata.mcgm.gov.in/`), CEO Maharashtra portal (`https://ceoelection.maharashtra.gov.in/`), and physical directions to the BMC Election Head Office (Masjid Bunder) & District Collectorates/Tehsildars.
  - Preserved Public Utilities: Legacy EPIC Card Decoder, Delimitation Time Machine, English-to-Marathi Phonetic Transliterator, Join Voter Drive form, and ECI official portals.
  - GitHub Pages Deployment: Configured and published to `https://nparvezten.github.io/MatdarSathi/`.
- **Python FastAPI Service**: PDF extraction microservice running on port 8000 using PyMuPDF (fitz) and pypdf.

## 2. Context & Security
- **Strict PII Protection**: Zero raw unencrypted PII persistence. PII fields (Deceased Names, Death Cert Reg Numbers, Family Bundle JSON) encrypted via AES-256 before EF Core persistence.
- **Deterministic Blind Indexing**: Uses HMAC-SHA256 blind indexes (`DeceasedNameBlindIndex`) to allow exact-match database queries without cleartext exposure.
- **CORS & API Versioning**: All controllers annotated with `[ApiVersion("1.0")]` and `[Route("api/v{version:apiVersion}/[controller]")]`. Global CORS policy configured via `builder.Services.AddCors(...)` before authentication middleware.
- **Local Dev Database Fallback**: EF Core automatically falls back to SQLite (`matdarsathi_dev.db`) if PostgreSQL is unconfigured or offline.

## 3. Blockers / Deferred
- **Offline Sync Queue**: Volunteer registrations and anomaly records save locally (`localStorage`) during backend offline states; background sync mechanism can be expanded for full service worker caching.
- **Multi-region Roll Archives**: Direct PDF scraping for non-MCGM municipalities deferred to future sub-microservice iteration.

## 4. Next Steps
1. **Verify Live GitHub Pages Site**: Confirm `https://nparvezten.github.io/MatdarSathi/` renders correctly and all client-side tools (Epic Decoder, Time Machine, Transliterator) function in web browser sandbox.
2. **Expand E2E Playwright Tests**: Write Playwright E2E browser automation scripts verifying end-to-end volunteer registration, Super Admin approval, and anomaly submission flows.
3. **PWA Offline Caching & Service Worker**: Enhance `ngsw-config.json` for offline asset caching across Capacitor mobile iOS/Android webviews.
