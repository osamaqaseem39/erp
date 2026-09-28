# Portfolio Content — ERP SaaS

Use only claims that match the repository.

## One-liner

Full-stack multi-tenant ERP SaaS: Clean Architecture ASP.NET Core 9 API with React/Vite admin UI — Core/Admin/CRM/POS wired end-to-end; additional ERP modules scaffolded pending DI registration.

## Problem

Mid-market operators need a modular ERP (CRM, finance, inventory, HR, procurement) with shared tenancy, RBAC, and a modern admin portal — without buying a dozen disconnected tools.

## Solution (as built)

- Layered .NET solution (`Api` / `Application` / `Domain` / `Infrastructure` / `Shared`) with EF Core SQL Server and Dapper SPs
- React admin portal with module routes for CRM, HRM, finance, inventory, POS, procurement, manufacturing, projects
- JWT auth, refresh tokens, SuperAdmin setup flow, permission seeding
- CRM sales pipeline with conversions, line items, and extended entities (documented in CRM status)
- HotChocolate GraphQL alongside REST
- Startup migrations + SQL schema deployment service

## Honest scope notes (required for accurate portfolio copy)

- Finance, HRM, Inventory, Procurement, Manufacturing, Projects, and Reports have substantial code and UI but are **not registered** in `Program.cs` DI — they are incomplete as runnable product modules
- Connection strings and JWT secrets are hardcoded constants
- Redis is required at API startup
- Automated tests cover a small CRM slice only
- UI includes Konrix template demo apps unrelated to ERP domains

## Suggested bullets

- Built a multi-tenant ERP platform with Clean Architecture and a React admin shell
- Delivered a production-shaped CRM module (REST + GraphQL + UI) with lead→order conversions
- Implemented SuperAdmin bootstrap, JWT auth, roles/permissions, and audit/system logging APIs
- Scaffolded finance/inventory/HR domains with EF models and controllers ahead of DI completion
