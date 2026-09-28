# Changelog — ERP SaaS

## Documentation

| Date | Change |
|------|--------|
| 2026-07-31 | Replaced shallow scaffold docs under `docs/` with source-verified enterprise documentation. Preserved [CRM_MODULE_STATUS.md](CRM_MODULE_STATUS.md). Polished root `README.md` Documentation section. |

## Application state snapshot (docs inventory date)

Documented from repository inspection (not a git release tag):

- **Runnable focus:** Core tenancy/auth/setup, Admin users/roles/permissions/settings/logs, CRM (core + extended), Sales invoices + POS
- **Scaffolded / not DI-wired:** Finance, HRM, Inventory/SCM, Procurement, Manufacturing, Projects, Reports controllers and services
- **Frontend:** Broad module routes + Konrix template apps
- **Data:** EF migrations + module schema SQL + startup deploy service
- **Tests:** CRM line calculator + lead conversion unit tests

For CRM-specific historical notes and backlog, see [CRM_MODULE_STATUS.md](CRM_MODULE_STATUS.md) (last reviewed there independently).

## Source control note

This changelog describes documentation and observed codebase state. Prefer `git log` in the ERP repository for commit-level history when available.
