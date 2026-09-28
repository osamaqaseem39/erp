# User Roles — ERP SaaS

## Built-in role name constants

From `ERP.Shared/Constants/AppConstants.Roles`:

| Constant | Value |
|----------|-------|
| `SUPER_ADMIN` | `SuperAdmin` |
| `ADMIN` | `Admin` |
| `MANAGER` | `Manager` |
| `USER` | `User` |

Permission action constants: `Read`, `Write`, `Delete`, `Manage` (`AppConstants.Permissions`).

## Bootstrap

`SetupService.SetupSuperAdminAsync`:

1. Ensures no existing SuperAdmin role assignment.
2. Creates tenant (+ company) and first user.
3. Creates SuperAdmin role and assigns permissions (via `PermissionService` seed/assign helpers).

Frontend detects missing super admin and routes to `/setup`.

## Data model

| Entity | Purpose |
|--------|---------|
| `Role` | Tenant-scoped role (`RoleName`, system-role flags) |
| `Permission` | Seeded catalog entries per tenant |
| `RolePermission` | Role ↔ permission links |
| `UserRole` | User ↔ role links |
| `User` | Credentials, refresh token fields, lockout/failed-attempt fields, verification tokens |

Admin APIs:

- Users: `/api/admin/users`
- Roles: `/api/admin/roles` (+ `GET/POST .../permissions`)
- Permissions: `/api/admin/permissions`, `POST .../seed`

## Runtime authorization

- JWT Bearer authentication is enabled globally.
- Many admin and CRM controllers use `[Authorize]` (e.g. `RoleController`).
- Fine-grained permission checks depend on how each service/controller enforces role permissions — do not assume every endpoint checks every seeded permission code.
- Tenant isolation relies on `TenantMiddleware` + repository filters where implemented (CRM is the best-documented example).

## Gaps

| Topic | Reality |
|-------|---------|
| Role constants beyond SuperAdmin | Defined; usage varies by feature |
| Permission seed catalog | `DefaultPermissions` via `PermissionService.SeedDefaultPermissionsAsync` |
| GraphQL authorization | Not uniformly documented as permission-gated |
| Unwired modules | Controllers may declare `[Authorize]` yet still fail DI before auth matters |
