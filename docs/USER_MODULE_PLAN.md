# User Module Implementation Plan

> **Source:** Reference video (User Type / User / User Role / Admin Email / Company) + [REFERENCE_MODULE_COMPARISON.md](./REFERENCE_MODULE_COMPARISON.md) §1.  
> **Scope:** functionality / API / domain — not pixel UI.  
> **Stack:** `ERP.Domain.Core` / Admin + `frontend` Core entities. Obey [operating/rules.md](./operating/rules.md).

---

## Goal

Match reference **User module** capabilities:

1. Login → module menu filtered by rights  
2. User Type (department/persona buckets)  
3. User profile + credentials (+ optional Employee link)  
4. Assign per-feature **View / Create / Update / Delete** (and finer, e.g. one report)  
5. Admin Email directory  
6. Company profile (incl. logo, contact person)

---

## Already done

| Capability | Status |
|------------|--------|
| Login / JWT / refresh | ✅ |
| User CRUD (name, email, phones, picture, password) | ✅ `User` + Admin `UserController` |
| Role CRUD | ✅ |
| Assign permissions to role | ✅ `AssignPermissions` |
| Permission catalog from endpoints | ✅ `EndpointPermissions` |
| Company profile get/update + logo URL | ✅ `CompanyProfileService` |
| Frontend Role list/form + Assign Permissions | ✅ partial |
| Frontend User list/form | ✅ partial |

---

## Design decisions (v1)

| Topic | Decision |
|-------|----------|
| **User Type** | Do **not** add a new table. Treat **Role** as User Type (Administrator, Accountant, Warehouse, …). Seed standard roles per tenant. Optional later: `User.UserTypeId` if product needs type ≠ role. |
| **Rights model** | Keep `Permission` = `module` + `action` (`read`/`create`/`update`/`delete`/`approve`/…). Map reference View→`read`, Save→`create`, Edit→`update`, Delete→`delete`. |
| **Assign target** | Rights stay on **Role**, not per-user matrix (reference “Assign Module” on Admin = role). Users get rights via `UserRole`. |
| **Menu** | Frontend builds menu only from effective permissions (no hard-coded full menu). |
| **Admin Email** | New small entity `AdminNotificationEmail` (or `Core.AdminEmail`): Email, Type, IsActive, TenantId. |
| **Employee link** | Add optional `User.EmployeeId` (FK HRM.Employee) when HRM employee CRUD is real; until then nullable + no hard dependency. |
| **Company contact** | Add `ContactPerson` (+ optional `MobilePhone`) on Company / profile DTOs. |

---

## Phases

### U0 — Spec & seed (0.5–1 day) — ✅ done

1. ✅ Standard roles documented + seeded via `RbacSeeder` templates (`Accountant`, `Warehouse`, `Production` added).  
2. Permission catalog audit — ongoing in U3.  
3. ✅ Naming: `{area}.{resource}.{action}` in DATA_MODEL.  
4. ✅ Gated `AdminEmails` + Company columns in [DATA_MODEL.md](./operating/DATA_MODEL.md).  
5. ✅ `User.EmployeeId` proposed (U5).

---

### U1 — Company profile completing (0.5–1 day) — ✅ done

1. ✅ `ContactPerson`, `MobilePhone` on Company + profile DTOs/service.  
2. ✅ Schema SQL + `20261020_UserModule_CompanyContact_AdminEmails.sql`.  
3. ✅ Tests in `CompanyProfileTests`.  
4. ✅ Frontend Company profile form fields.

---

### U2 — Role-as-User-Type + user assignment (1–2 days) — ✅ done

1. ✅ Seed templates include Accountant / Warehouse / Production (idempotent via existing `RbacSeeder`).  
2. ✅ User create/update: require ≥1 role (service + validators + UserForm).  
3. ⬜ Role labeling as User Type in API (optional / deferred).  
4. ✅ User DTO returns `RoleIds` / `RoleNames`; UserList shows roles.  
5. ✅ Seed tests updated for new roles.

**Exit:** Admin can create “Warehouse” style users by assigning role.

---

### U3 — Fine-grained permissions + enforcement (2–3 days) — ✅ done (v1)

1. ✅ Catalog from endpoints (`EndpointPermissions`); Vendor + Admin Email codes asserted in tests.  
2. ✅ Report-level placeholders via `ReportPermissionCatalog` + `CompositePermissionCatalog` (seeded with RBAC); available-reports filtered by per-report `view` when coarse `reports.reports.view` is absent.  
3. ✅ API rejects missing permission (`PermissionAuthorizationFilter` + filter tests).  
4. ✅ Login / refresh / `GET /api/auth/me` return `Permissions`; frontend stores them on session.  
5. ✅ Assign Permissions UI: module groups + View/Create/Update/Delete matrix.  
6. ✅ Menu/routes filter via `hasPermission` once session has `permissions`.  
7. ✅ Tests: view-only vendor → GET ok, POST 403.

**Exit:** Video “view only supplier” and “hide one report” work end-to-end.

---

### U4 — Admin Email directory (1 day) — ✅ done

1. ✅ Domain + repo + service + REST `api/admin/emails` (tenant-scoped).  
2. ✅ Types: Notification, Finance, Support, Sales, Other.  
3. ✅ Permissions: `admin.emails.view|create|update|delete`.  
4. ✅ Frontend list/form + registry + routes.  
5. ✅ SQL/in-memory tests: CRUD + tenant isolation.

**Exit:** Video Admin Email capability.

---

### U5 — User ↔ Employee link (1 day, after HRM employee real) — ✅ done

1. ✅ `User.EmployeeId` nullable; set on create/update; same-tenant + unique link validation.  
2. ✅ GET user returns `EmployeeCode` / `EmployeeName`.  
3. ✅ Delete employee blocked with 409 while linked.  
4. ✅ UserForm employee picker (loads `/hrm/employees`).  
5. ✅ SQL `20261020_UserModule_UserEmployeeLink.sql`.

**Exit:** Staff users can point at Employee record.

---

### U6 — User profile polish (0.5–1 day) — ✅ done

1. ✅ `AddressLine1` + `City` on User + Admin DTOs + SQL `20261020_UserModule_UserProfile.sql`.  
2. ✅ Primary / secondary / mobile phones on create/update DTOs + UserForm.  
3. ✅ Profile picture as https URL (same pattern as company logo).  
4. ✅ Email + username unique per tenant (service checks); username defaults to email.

**Exit:** Video user detail fields covered.

---

## Delivery order

```text
U0 Spec + DATA_MODEL
  → U1 Company contact fields
  → U2 Role seed + user↔role as User Type
  → U3 Permission catalog + enforce + menu
  → U4 Admin Email
  → U6 User profile polish (can parallel U4)
  → U5 Employee link (when HRM employee ready)
```

Rough calendar: **~1–1.5 weeks** one stream.

---

## Definition of done

1. Tenant has seed roles usable as User Types.  
2. Admin assigns role permissions with read/create/update/delete granularity; API enforces.  
3. Menu/routes hide unauthorized modules/actions.  
4. Company profile includes logo + contact person.  
5. Admin Email CRUD works (tenant-scoped).  
6. User create covers name, contacts, picture, role(s), password; optional EmployeeId when HRM ready.  
7. SQL Server tests for role seed, permission 403, Admin Email, company update.  
8. Docs: this file + DATA_MODEL + comparison §1 marked done items.

---

## Out of scope (v1)

- Per-user permission overrides (always via Role)  
- Separate UserType table (unless product insists after U2)  
- Cloning reference checkbox UI layout  
- CMS / SMS from reference  
- Changing tenancy model  

---

## Linkage

- Product / backend: Wave 0–7 platform hygiene; this plan is the **User module vertical**.  
- Accounts plan stays independent; permissions for Finance screens come from **U3** catalog work.
