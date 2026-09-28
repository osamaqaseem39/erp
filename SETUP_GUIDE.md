# ERP Application Setup Guide

## 🚀 Application Starting Point

### Frontend Entry Point
1. **`index.html`** → Entry HTML file
2. **`src/index.tsx`** → React application entry point
   - Renders the App component
   - Sets up Redux Provider and BrowserRouter
3. **`src/App.tsx`** → Main application component
   - Configures fake backend (for development)
   - Renders AllRoutes component
4. **`src/routes/Routes.tsx`** → Route configuration
   - Handles authentication checks
   - Redirects unauthenticated users to `/auth/login`
   - Redirects root `/` to `/erp/dashboard` (requires auth)

### Backend Entry Point
1. **`ERP.Api/Program.cs`** → ASP.NET Core application entry
   - Configures services, middleware, and database
   - Creates database on startup (`EnsureCreated()`)
   - Swagger UI available at `/swagger` (development mode)
   - GraphQL endpoint at `/graphql`

### Default Routes
- **Root (`/`)** → Redirects to `/erp/dashboard` (if authenticated) or `/auth/login`
- **Login (`/auth/login`)** → Public login page
- **Register (`/auth/register`)** → Public registration page
- **ERP Dashboard (`/erp/dashboard`)** → Main dashboard (requires authentication)

---

## 🏢 Where Tenants Are Created

### Option 1: Through Admin UI (Recommended)
**Location:** `/erp/tenants/new`

**Steps:**
1. Login as an authenticated user (you'll need to create the first user manually or via API)
2. Navigate to **ERP → Tenants** (`/erp/tenants`)
3. Click **"Add Tenant"** button
4. Fill in the tenant form:
   - Name
   - Domain (e.g., `company1`)
   - Industry
   - Country
   - Status
   - Database Name
   - Connection String
   - Is Active
5. Submit the form

**Files:**
- Frontend: `frontend/src/pages/erp/Tenants/Form.tsx`
- Backend API: `POST /api/tenants`
- Backend Controller: `ERP.Api/Controllers/TenantController.cs`

### Option 2: Via API Directly
**Endpoint:** `POST /api/tenants`

**Request Body:**
```json
{
  "name": "Company Name",
  "domain": "company1",
  "industry": "Technology",
  "country": "United States",
  "status": "Active",
  "databaseName": "ERP_company1",
  "connectionString": "Server=localhost;Database=ERP_company1;...",
  "isActive": true
}
```

**Using Swagger:**
1. Start the backend API
2. Navigate to `https://localhost:7062/swagger`
3. Find `POST /api/tenants` endpoint
4. Click "Try it out"
5. Enter tenant data
6. Execute

### Option 3: Via GraphQL
**Endpoint:** `POST /graphql`

**Mutation:**
```graphql
mutation {
  addTenant(input: {
    name: "Company Name"
    domain: "company1"
    industry: "Technology"
    country: "United States"
    status: "Active"
    databaseName: "ERP_company1"
    isActive: true
  }) {
    id
    name
    domain
  }
}
```

---

## 🔧 Initial Setup Process

### Current State
⚠️ **There is NO automatic initial setup/bootstrap process.** The system requires manual setup.

### Recommended Initial Setup Flow

#### Step 1: Start Backend API
```bash
cd ERP.Api
dotnet run
```
- Database will be created automatically
- Swagger UI available at `https://localhost:7062/swagger`

#### Step 2: Create First Tenant
**Option A: Via Swagger (Easiest)**
1. Open `https://localhost:7062/swagger`
2. Use `POST /api/tenants` to create first tenant
3. Save the tenant ID (GUID) returned

**Option B: Via SQL (Direct)**
```sql
INSERT INTO [Core].[Tenants] (
    [Id], [TenantName], [Domain], [Industry], [Country], 
    [Status], [DatabaseName], [ConnectionString], [IsActive], 
    [CreatedAt], [CreatedBy]
)
VALUES (
    NEWID(), 
    'Default Company', 
    'default', 
    'Technology', 
    'United States',
    'Active',
    'ERP_default',
    'Server=localhost;Database=ERP_default;Trusted_Connection=true;TrustServerCertificate=true;',
    1,
    GETUTCDATE(),
    'System'
);
```

#### Step 3: Create First Admin User
**Via Swagger:**
1. Use `POST /api/admin/users` endpoint
2. **Note:** This requires authentication, so you may need to:
   - Temporarily remove `[Authorize]` attribute from UserController
   - Or create user via SQL directly

**Via SQL (Direct):**
```sql
-- First, get the tenant ID from Step 2
DECLARE @TenantId UNIQUEIDENTIFIER = 'YOUR-TENANT-ID-HERE';
DECLARE @UserId UNIQUEIDENTIFIER = NEWID();

-- Create user
INSERT INTO [Core].[Users] (
    [Id], [TenantId], [Username], [Email], [PasswordHash],
    [FirstName], [LastName], [IsActive], [IsEmailVerified],
    [CreatedAt], [CreatedBy]
)
VALUES (
    @UserId,
    @TenantId,
    'admin',
    'admin@example.com',
    -- Password hash for 'Admin@123' (you'll need to hash it properly)
    'HASHED_PASSWORD_HERE',
    'Admin',
    'User',
    1,
    0,
    GETUTCDATE(),
    'System'
);

-- Create admin role
DECLARE @RoleId UNIQUEIDENTIFIER = NEWID();
INSERT INTO [Core].[Roles] (
    [Id], [TenantId], [Name], [Description], [IsActive], [IsSystemRole],
    [CreatedAt], [CreatedBy]
)
VALUES (
    @RoleId,
    @TenantId,
    'Admin',
    'Administrator role with full access',
    1,
    1,
    GETUTCDATE(),
    'System'
);

-- Assign role to user
INSERT INTO [Core].[UserRoles] ([UserId], [RoleId], [CreatedAt], [CreatedBy])
VALUES (@UserId, @RoleId, GETUTCDATE(), 'System');
```

#### Step 4: Start Frontend
```bash
cd frontend
npm install
npm run dev
```

#### Step 5: Login
1. Navigate to `http://localhost:5173/auth/login`
2. Use credentials from Step 3
3. Enter tenant ID from Step 2
4. Login and access the system

---

## 📋 Recommended Improvements

### 1. Create Initial Setup Page
Create a setup page at `/setup` that:
- Checks if system is initialized
- If not, allows creating first tenant and admin user
- If yes, redirects to login

### 2. Add Seed Data Script
Create a seed script that:
- Creates default tenant
- Creates default admin user
- Creates default roles and permissions
- Can be run on first startup

### 3. Add Bootstrap Endpoint
Create a public endpoint `/api/setup/bootstrap` that:
- Checks if system is initialized
- Creates initial tenant and admin user
- Returns credentials

---

## 🔐 Authentication Flow

1. **User visits app** → Redirected to `/auth/login` (if not authenticated)
2. **User logs in** → Provides:
   - Email
   - Password
   - Tenant ID
3. **Backend validates** → Returns JWT token
4. **Frontend stores token** → Redirects to `/erp/dashboard`
5. **Subsequent requests** → Include JWT token in Authorization header

---

## 📍 Key Routes

### Public Routes
- `/auth/login` - Login page
- `/auth/register` - Registration page (requires tenant)
- `/auth/recover-password` - Password recovery
- `/auth/lock-screen` - Lock screen

### Protected Routes (Require Authentication)
- `/erp/dashboard` - Main dashboard
- `/erp/tenants` - Tenant list
- `/erp/tenants/new` - Create tenant
- `/erp/tenants/:id/edit` - Edit tenant
- `/erp/admin/users` - User management
- `/erp/admin/roles` - Role management
- `/erp/admin/system-settings` - System settings

---

## 🎯 Summary

**Where tenants are added:**
- ✅ Admin UI: `/erp/tenants/new` (requires authentication)
- ✅ API: `POST /api/tenants` (via Swagger or direct API call)
- ✅ GraphQL: `addTenant` mutation

**Starting point:**
- Frontend: `index.html` → `index.tsx` → `App.tsx` → `Routes.tsx`
- Backend: `Program.cs` → Creates database → Starts API server
- Default route: `/` → `/erp/dashboard` (or `/auth/login` if not authenticated)

**Initial setup:**
- ⚠️ Currently manual - needs to be done via API/SQL
- Recommended: Create first tenant via Swagger, then create admin user

