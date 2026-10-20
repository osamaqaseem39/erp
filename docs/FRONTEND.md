# Frontend — ERP SaaS

## Supported SPA: `frontend-new`

| Item | Value |
|------|-------|
| Package | `frontend-new/package.json` |
| UI | React 18, TypeScript, Vite |
| Data | TanStack Query |
| Engine | Registry-driven CRUD (`src/modules/registry.ts` + `src/engine/*`) |
| Styling | Tailwind CSS |
| Auth | JWT session in `src/api/session.ts` |

```bash
cd frontend-new && npm install && npm run dev
```

Set `VITE_API_URL` to `https://erp-api.tekvers.com` or your local API origin (e.g. `https://localhost:7062`).

Coverage inventory: [frontend-new/docs/FRONTEND_NEW_COVERAGE.md](../frontend-new/docs/FRONTEND_NEW_COVERAGE.md). Vitest smoke: `npm test` in `frontend-new`.

## Legacy SPA: `frontend/` (deprecated)

The older Konrix/Redux tree under `frontend/` remains in the repo for reference only. Do not add product screens there. Prefer `frontend-new` for all new UI work.
