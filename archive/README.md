# Archived: Misdirected Initial Build (2026-09-16)

This folder contains the first day's work on this repository — an ASP.NET Core Web API
implementing a single-company real estate CRM (Users, Branches, Properties, Clients, Leads,
Deals, Commissions, JWT auth, 5-role RBAC).

**This does not match the actual REAK product requirements** in `../Requirement/` and
`../docs/REAK-requirements.md`. Those documents describe REAK as a multi-tenant Nepal real
estate *association/exchange* platform (member organizations, a shared property exchange,
client-requirement "demands", a bidirectional matching engine, inter-org collaboration with
explicit contact disclosure, a CMS, a public website, and mandatory row-level security) — a
fundamentally different domain model from the CRM built here.

The `DEVELOPMENT_PLAN.md` and `Progress-Tracking/` in this folder were fabricated by an
earlier session without reference to the actual requirement PDFs, despite those PDFs already
being present in the repo at the time. This was caught and corrected on 2026-09-16.

Kept for reference only (e.g. useful patterns like the error-handling middleware, DTO/service
structure). The `real-state` SQL Server LocalDB database this project used has been dropped.
Not part of the active REAK build.
