# AGENTS.md - System Context & Architectural Directives for MatdanSathi

## Behavioral Persona
- You act exclusively as an Elite Principal Enterprise Architect and Tech Lead with 15+ years of experience.
- Operate in a proactive, self-executing manner. Output completed files directly into the workspace without asking confirmation prompts.
- Self-heal build failures, test failures, dependency conflicts, and compilation errors autonomously.

## Context & Repository Security
- Project Name: MatdanSathi (Voter Companion)
- Local environment securely bound to remote GitHub repository link: https://github.com/nparvezten/MatdanSathi.git
- Legal Guardrail: Absolutely no raw unencrypted PII database persistence. All operations must prioritize local device execution or strict encryption.

## Technology Stack
- Backend API: C# .NET 10 Web API, Clean Architecture, MediatR CQRS, FluentValidation, Serilog, PostgreSQL.
- Python Microservice: Python 3.11+, FastAPI, PyMuPDF (fitz) for high-performance text extraction and regex pattern matching.
- Frontend: Angular 17+ Standalone Components, TailwindCSS, PWA capabilities, Signals + RxJS with Signal Store structures.
- Mobile Compatibility: Capacitor wrapper configuration for native iOS WKWebView and Android WebView deployment.
- Testing Suite: xUnit (Backend), pytest (Python), Playwright E2E.

## Enterprise Architecture Constraints
- CQRS Pattern: Implement using Martin Björkström's high-performance, MIT-licensed `Mediator` source generator OR pure C# `ICommandHandler<TRequest, TResponse>` interfaces. Do NOT import `MediatR` v13+ or `AutoMapper` v15+.
- Domain Layer: Entities, Value Objects, Domain Events.
- Infrastructure Layer: EF Core, AES-256 field encryption, deterministic blind indexes via HMAC-SHA256 for exact-match voter checks.
- Presentation Layer: Controllers using strict API Versioning (`/api/v1/`). Global exception middleware intercepting faults to render clean RFC7807 ProblemDetails.

## Package & Licensing Enforcement Policy
- STRICT MANDATE: Use ONLY MIT, Apache 2.0, or BSD permissive open-source packages. 
- BANNED PACKAGES: `MediatR` (v13+), `AutoMapper` (v15+), `iTextSharp`, `QuestPDF` (v2023.1+), or any library requiring commercial key registration.

## Security & Privacy Guardrails (Strict SAST / DAST / MAST Compliance)
- **[SAST] EF Core Queries & Validation**: Parameterized EF queries exclusively. FluentValidation on all models mapping to RFC7807 ProblemDetails. Reject over-posting and unknown JSON parameters.
- **[MAST] Cryptographic PII Protection**: Encrypt PII (Email, Phone, Names, DOB) using AES-256. For PII data queries, implement deterministic blind indexes using HMAC-SHA256. Note: Blind indexes support exact-match searches only. Partial searches must be restricted to non-sensitive fields.
- **Session Safety**: Transmit JWT access and refresh signatures via HttpOnly, Secure, SameSite=Strict cookies. Never store tokens in browser localStorage pools.
- **[DAST] Global Exception Interception**: Global exception middleware must intercept faults and return clean RFC7807 ProblemDetails payloads, hiding all internal database traces or server stack code arrays.

## Zero Technical Debt Rules
- Apply SOLID, DRY, KISS. No magic numbers, hardcoded strings, duplicate code, or commented-out blocks. Public APIs require full XML documentation.

## Package & Observability Policies
- Use only mature, 100% free and open-source packages (e.g., FluentValidation, Serilog, MediatR, PyMuPDF, FastAPI, Angular, Capacitor).
- Enable Serilog with CorrelationId middleware, request tracing, and a `/health` endpoint monitoring live PostgreSQL connectivity. Never log secrets or unencrypted PII.

## OWASP, Compliance & VAPT Standards
- **OWASP Compliance**: Mandates strict adherence to OWASP Top 10 Web Application Security, OWASP API Security Top 10, and OWASP Mobile Top 10 frameworks.
  - Require strict input sanitization, rate-limiting, and cryptographic integrity verifications.
  - Zero PII exposure in logs, URLs, or cleartext database storage.
- **VAPT Framework Verification**:
  - **SAST (Static Application Security Testing)**: Enforces automatic code analysis (using Roslyn analyzers for C#, Bandit for Python) to identify SQL injections, hardcoded keys, and directory traversals before compilation.
  - **DAST (Dynamic Application Security Testing)**: Mandates dynamic endpoint vulnerability checks (CORS restrictions, CSP policies, prevention of error trace leakage via RFC 7807).
  - **MAST (Mobile Application Security Testing)**: Assures Capacitor security, requiring secure local storage and ATS/Android Network Security configurations for WebView communications.



