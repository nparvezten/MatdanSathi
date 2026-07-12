# MatdanSathi (मतदान साथी) — Voter Companion

MatdanSathi is a free, privacy-first, open-source community utility built to assist citizens during Special Intensive Revision (SIR) electoral roll verification drives. It simplifies finding local booth resources, mapping family structures, and identifying unintended roll discrepancies without compromising individual data privacy.

## 🚀 Key Modules
*   **Module A (Roll Watchdog & Parser):** A local Python-based microservice that securely parses official electoral roll PDFs to detect unexpected name deletions or section transfers.
*   **Module B (BLO Coordinator):** A responsive map interface that links geographic coordinates or building names to local Booth Level Officers (BLOs).
*   **Module C (Interactive Wizard):** An accessible, step-by-step assistant that helps users verify their current records against historical benchmarks (e.g., 2002 datasets) and prepares formatted inputs for official portals.

## 🛠️ The Tech Stack
*   **Backend API:** ASP.NET Core 10 Web API (Clean Architecture, MediatR CQRS, PostgreSQL)
*   **PDF Parsing Service:** Python 3.11 (FastAPI, PyMuPDF engine)
*   **Frontend UI:** Angular 17+ (Standalone Components, Signals, TailwindCSS)
*   **Mobile Delivery:** Progressive Web App (PWA) with Capacitor wrappers for Android WebView and iOS WKWebView compatibility.

## 🛡️ Security & Privacy Architecture
To eliminate structural legal liabilities, this application employs strict cryptographic architecture:
1. **No Raw PII Storage:** Publicly Identifiable Information (Names, Phone numbers) is fully encrypted using AES-256 before hitting the database context.
2. **Deterministic Search:** Lookups use blind indexing via HMAC-SHA256, allowing users to verify their registration status securely without decrypting records globally.
3. **Local Processing Priority:** PDF extractions are designed to run transiently in memory or completely client-side.

## 📝 License
Distributed under the **MIT License**. See `LICENSE` for more information.