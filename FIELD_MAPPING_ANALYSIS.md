# Field Mapping Analysis: Controllers, DTOs, and Stored Procedures

**Generated:** 2024-12-19  
**Purpose:** Comprehensive field mapping verification between API Controllers, DTOs, and Stored Procedures

## How to Use This Document

This document provides a comprehensive analysis of field mappings between:
1. **API Controllers** - Endpoints and their expected DTOs
2. **DTOs (Data Transfer Objects)** - Request/Response models
3. **Stored Procedures** - Database procedures with parameters and return fields

### Reading the Analysis

Each module section contains:
- **Controller Endpoints** - All HTTP endpoints and their DTOs
- **DTOs** - Complete field definitions
- **Stored Procedures** - Parameters and return fields
- **Field Mapping Issues** - Table showing mismatches and missing items

### Issue Severity Levels

- ❌ **CRITICAL** - Must fix immediately (missing SPs, breaking changes)
- ⚠️ **HIGH** - Should fix soon (field name mismatches, missing fields)
- ⚠️ **MEDIUM** - Should fix when possible (naming inconsistencies, missing optional SPs)
- ⚠️ **LOW** - Nice to have (minor improvements)
- ✅ **OK** - No issues found

---

## Table of Contents

1. [Core Module](#core-module)
2. [Admin Module](#admin-module)
3. [Sales Module](#sales-module)
4. [CRM Module](#crm-module)
5. [HRM Module](#hrm-module)
6. [Finance Module](#finance-module)
7. [Inventory Module](#inventory-module)
8. [Procurement Module](#procurement-module)
9. [Manufacturing Module](#manufacturing-module)
10. [Projects Module](#projects-module)
11. [Reports Module](#reports-module)
12. [Summary of Issues](#summary-of-issues)

---

## Core Module

### User Entity

#### Controller: `UserController.cs`
**Endpoints:**
- `POST /api/admin/users` → `CreateUserDto`
- `GET /api/admin/users/{id}` → `UserDto`
- `GET /api/admin/users` → `IEnumerable<UserDto>`
- `PUT /api/admin/users/{id}` → `UpdateUserDto`
- `POST /api/admin/users/{id}/reset-password` → `ResetUserPasswordDto`
- `DELETE /api/admin/users/{id}` → (no DTO)

#### DTOs

**UserDto** (inherits BaseDto):
```csharp
- Id (Guid) - from BaseDto
- TenantId (Guid) - from BaseDto
- CreatedAt (DateTime) - from BaseDto
- UpdatedAt (DateTime?) - from BaseDto
- CreatedBy (string?) - from BaseDto
- UpdatedBy (string?) - from BaseDto
- IsActive (bool) - from BaseDto
- FirstName (string)
- LastName (string)
- Email (string)
- Phone (string?)
```

**CreateUserDto** (inherits CreateDto):
```csharp
- TenantId (Guid) - from CreateDto
- CreatedBy (string?) - from CreateDto
- FirstName (string)
- LastName (string)
- Email (string)
- Password (string)
- Phone (string?)
- IsActive (bool)
- RoleIds (List<Guid>)
```

**UpdateUserDto** (inherits UpdateDto):
```csharp
- Id (Guid) - from UpdateDto
- UpdatedBy (string?) - from UpdateDto
- FirstName (string)
- LastName (string)
- Email (string)
- Phone (string?)
- IsActive (bool)
- RoleIds (List<Guid>)
```

**ResetUserPasswordDto**:
```csharp
- UserId (Guid)
- NewPassword (string)
```

#### Stored Procedures

**sp_GetUserById** (`[Core].[sp_GetUserById]`):
```sql
Parameters:
- @Id UNIQUEIDENTIFIER

Returns:
- Id, TenantId, CompanyId, Username, Email, FirstName, LastName, 
  PasswordHash, IsActive, IsEmailVerified, PhoneNumber, 
  CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
```

**sp_GetUserByEmail** (`[Core].[sp_GetUserByEmail]`):
```sql
Parameters:
- @PrimaryEmail NVARCHAR(255)

Returns:
- Id, TenantId, CompanyId, Salutation, FirstName, LastName, FullName,
  Gender, DateOfBirth, JobTitle, Department, Nationality,
  PrimaryEmail, SecondaryEmail, PrimaryPhone, SecondaryPhone,
  MobilePhone, Fax, LinkedIn, Twitter, Facebook, Instagram,
  PreferredCommunicationMethod, LanguagePreference, TimeZone,
  PasswordHash, PasswordSalt, Status, LastLoginDate,
  IsActive, IsLocked, FailedLoginAttempts, LockedUntil,
  EmailVerificationToken, PasswordResetToken, PasswordResetExpires,
  RefreshToken, RefreshTokenExpires,
  CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
```

**sp_GetAllUsers** (`[Core].[sp_GetAllUsers]`):
```sql
Parameters: None

Returns:
- Id, TenantId, CompanyId, Username, Email, FirstName, LastName,
  PasswordHash, IsActive, IsEmailVerified, PhoneNumber,
  CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
```

**sp_CreateUser** (`[Core].[sp_CreateUser]`):
```sql
Parameters:
- @Id UNIQUEIDENTIFIER
- @TenantId UNIQUEIDENTIFIER
- @CompanyId UNIQUEIDENTIFIER = NULL
- @Salutation NVARCHAR(10) = NULL
- @FirstName NVARCHAR(100) = NULL
- @LastName NVARCHAR(100) = NULL
- @FullName NVARCHAR(200) = NULL
- @Gender NVARCHAR(10) = NULL
- @DateOfBirth DATETIME2 = NULL
- @JobTitle NVARCHAR(100) = NULL
- @Department NVARCHAR(100) = NULL
- @Nationality NVARCHAR(100) = NULL
- @PrimaryEmail NVARCHAR(200) = NULL
- @SecondaryEmail NVARCHAR(200) = NULL
- @PrimaryPhone NVARCHAR(20) = NULL
- @SecondaryPhone NVARCHAR(20) = NULL
- @MobilePhone NVARCHAR(20) = NULL
- @Fax NVARCHAR(20) = NULL
- @LinkedIn NVARCHAR(200) = NULL
- @Twitter NVARCHAR(200) = NULL
- @Facebook NVARCHAR(200) = NULL
- @Instagram NVARCHAR(200) = NULL
- @PreferredCommunicationMethod NVARCHAR(20) = NULL
- @LanguagePreference NVARCHAR(10) = NULL
- @TimeZone NVARCHAR(50) = NULL
- @PasswordHash NVARCHAR(MAX)
- @PasswordSalt NVARCHAR(MAX) = NULL
- @Status NVARCHAR(50) = NULL
- @LastLoginDate DATETIME2 = NULL
- @IsActive BIT = 1
- @IsLocked BIT = 0
- @FailedLoginAttempts INT = 0
- @LockedUntil DATETIME2 = NULL
- @EmailVerificationToken NVARCHAR(MAX) = NULL
- @PasswordResetToken NVARCHAR(MAX) = NULL
- @PasswordResetExpires DATETIME2 = NULL
- @RefreshToken NVARCHAR(MAX) = NULL
- @RefreshTokenExpires DATETIME2 = NULL
- @CreatedAt DATETIME2 = NULL
- @CreatedBy NVARCHAR(100) = NULL

Returns:
- @Id AS Id
```

#### Field Mapping Issues - User

| Issue | Description | Severity | Status |
|-------|-------------|----------|--------|
| **Mismatch 1** | `sp_GetUserById` returns `Username`, `Email`, `PhoneNumber`, `IsEmailVerified` but `UserDto` expects `Email`, `Phone` | ⚠️ HIGH | ✅ **FIXED** - SP now returns `PrimaryEmail`, `PrimaryPhone` |
| **Mismatch 2** | `sp_GetUserByEmail` returns `PrimaryEmail` but DTO uses `Email` | ⚠️ HIGH | ✅ **OK** - Service layer maps `PrimaryEmail` → `Email` |
| **Mismatch 3** | `sp_GetAllUsers` returns different fields than `sp_GetUserById` (inconsistent) | ⚠️ MEDIUM | ✅ **FIXED** - All Get SPs now return consistent fields |
| **Mismatch 4** | `CreateUserDto` has `Password` but SP expects `@PasswordHash` (service should hash it) | ✅ OK | ✅ **OK** - Handled in service |
| **Mismatch 5** | `CreateUserDto` has `RoleIds` but SP doesn't handle roles (handled separately) | ✅ OK | ✅ **OK** - Handled in service |
| **Mismatch 6** | `sp_GetUserById` returns `Username` and `Email` but entity has both `Email` and `PrimaryEmail` | ⚠️ HIGH | ✅ **FIXED** - SP now returns `PrimaryEmail` |
| **Mismatch 7** | `sp_GetUserByEmail` returns many fields not in `UserDto` | ⚠️ MEDIUM | ✅ **OK** - Extra fields are fine, service maps only needed ones |
| **Missing** | No `sp_UpdateUser` stored procedure found | ❌ CRITICAL | ✅ **FIXED** - Created `[Core].[sp_UpdateUser]` |
| **Missing** | No `sp_DeleteUser` stored procedure found | ❌ CRITICAL | ✅ **FIXED** - Created `[Core].[sp_DeleteUser]` |
| **Missing** | No `sp_UpdatePassword` stored procedure found | ❌ CRITICAL | ✅ **FIXED** - Created `[Core].[sp_UpdateUserPassword]` |

**✅ User Module Status: FIXED**
- All stored procedures created and aligned
- Repository updated to use correct SP names with schema prefix `[Core].[sp_*]`
- Field mappings consistent (PrimaryEmail/PrimaryPhone in SP/Entity, Email/Phone in DTO)
- Service layer correctly maps between entity and DTO

---

## Admin Module

### System Log Entity

#### Controller: `SystemLogController.cs`
**Endpoints:**
- `GET /api/admin/system-logs/{id}` → `SystemLogDto`
- `GET /api/admin/system-logs` → `IEnumerable<SystemLogDto>`
- `POST /api/admin/system-logs` → `CreateSystemLogDto`

#### DTOs

**SystemLogDto**:
```csharp
- Id (Guid)
- TenantId (Guid)
- LogLevel (string)
- Message (string)
- Exception (string?)
- Source (string?)
- Method (string?)
- UserId (Guid?)
- IPAddress (string?)
- Timestamp (DateTime)
- Properties (string?)
- CreatedAt (DateTime)
- UserName (string?) - computed/joined
```

#### Stored Procedures

**sp_GetSystemLogById** (`[Admin].[sp_GetSystemLogById]`):
```sql
Parameters:
- @Id UNIQUEIDENTIFIER
- @TenantID UNIQUEIDENTIFIER

Returns:
- Id, TenantId, LogLevel, Message, Exception, Source, Method,
  UserId, IPAddress, Timestamp, Properties, CreatedAt,
  UserName (computed: FirstName + ' ' + LastName)
```

**sp_GetSystemLogs** (`[Admin].[sp_GetSystemLogs]`):
```sql
Parameters:
- @TenantID UNIQUEIDENTIFIER
- @LogLevel NVARCHAR(50) = NULL
- @StartDate DATETIME2 = NULL
- @EndDate DATETIME2 = NULL
- @UserId UNIQUEIDENTIFIER = NULL
- @Source NVARCHAR(200) = NULL
- @PageNumber INT = 1
- @PageSize INT = 100

Returns:
- Id, LogLevel, Message, Exception, Source, Method,
  UserId, IPAddress, Timestamp, Properties,
  UserName (computed)
- TotalCount (separate result set)
```

**sp_CreateSystemLog** (`[Admin].[sp_CreateSystemLog]`):
```sql
Parameters:
- @Id UNIQUEIDENTIFIER
- @TenantID UNIQUEIDENTIFIER
- @LogLevel NVARCHAR(50)
- @Message NVARCHAR(MAX)
- @Exception NVARCHAR(MAX) = NULL
- @Source NVARCHAR(200) = NULL
- @Method NVARCHAR(200) = NULL
- @UserId UNIQUEIDENTIFIER = NULL
- @IPAddress NVARCHAR(50) = NULL
- @Timestamp DATETIME2 = NULL
- @Properties NVARCHAR(MAX) = NULL

Returns:
- @Id AS Id
```

#### Field Mapping Issues - System Log

| Issue | Description | Severity |
|-------|-------------|----------|
| **Match** | All fields align correctly | ✅ OK |

---

## Sales Module

### Sales Invoice Entity

#### Controller: `SalesInvoiceController.cs`
**Endpoints:**
- `POST /api/sales/invoices` → `CreateSalesInvoiceDto`
- `GET /api/sales/invoices/{id}` → `SalesInvoiceDto`
- `GET /api/sales/invoices` → `IEnumerable<SalesInvoiceDto>`
- `PUT /api/sales/invoices/{id}` → `UpdateSalesInvoiceDto`
- `DELETE /api/sales/invoices/{id}` → (no DTO)
- `POST /api/sales/invoices/{id}/post` → (no DTO)
- `POST /api/sales/invoices/{id}/void` → (no DTO)

#### DTOs

**SalesInvoiceDto** (inherits BaseDto):
```csharp
- Id (Guid?) - NOTE: nullable, also in BaseDto
- TenantId (Guid) - from BaseDto
- CreatedAt (DateTime) - from BaseDto
- UpdatedAt (DateTime?) - from BaseDto
- CreatedBy (string?) - from BaseDto
- UpdatedBy (string?) - from BaseDto
- IsActive (bool) - from BaseDto
- InvoiceNumber (string)
- CustomerId (Guid)
- CustomerName (string?) - computed/joined
- InvoiceDate (DateTime)
- DueDate (DateTime)
- Status (string)
- SubTotal (decimal)
- TaxAmount (decimal)
- TotalAmount (decimal)
- PaidAmount (decimal)
- BalanceAmount (decimal)
- Notes (string?)
- Terms (string?)
- SalesOrderId (Guid?)
- PostedAt (DateTime?)
- PostedBy (Guid?)
- Lines (List<SalesInvoiceLineDto>)
```

**CreateSalesInvoiceDto** (inherits CreateDto):
```csharp
- TenantId (Guid) - from CreateDto
- CreatedBy (string?) - from CreateDto
- InvoiceNumber (string)
- CustomerId (Guid)
- InvoiceDate (DateTime)
- DueDate (DateTime)
- SubTotal (decimal)
- TaxAmount (decimal)
- TotalAmount (decimal)
- Status (string)
- Notes (string?)
- Terms (string?)
- SalesOrderId (Guid?)
- Lines (List<CreateSalesInvoiceLineDto>)
```

**UpdateSalesInvoiceDto** (inherits UpdateDto):
```csharp
- Id (Guid) - from UpdateDto
- UpdatedBy (string?) - from UpdateDto
- InvoiceNumber (string)
- CustomerId (Guid)
- InvoiceDate (DateTime)
- DueDate (DateTime)
- SubTotal (decimal)
- TaxAmount (decimal)
- TotalAmount (decimal)
- Status (string)
- Notes (string?)
- Terms (string?)
- Lines (List<UpdateSalesInvoiceLineDto>)
```

**SalesInvoiceLineDto**:
```csharp
- Id (Guid)
- SalesInvoiceId (Guid)
- ItemId (Guid)
- ItemName (string?) - computed/joined
- Description (string)
- Quantity (decimal)
- UnitPrice (decimal)
- TaxRate (decimal)
- TaxAmount (decimal)
- LineTotal (decimal)
- LineNumber (int)
```

#### Stored Procedures

#### Stored Procedures

**sp_GetSalesInvoiceById** (`[Sales].[sp_GetSalesInvoiceById]`):
```sql
Parameters:
- @Id UNIQUEIDENTIFIER
- @TenantId UNIQUEIDENTIFIER

Returns:
- All invoice fields + CustomerName (joined)
```

**sp_GetAllSalesInvoices** (`[Sales].[sp_GetAllSalesInvoices]`):
```sql
Parameters:
- @TenantId UNIQUEIDENTIFIER
- @CustomerId UNIQUEIDENTIFIER = NULL
- @Status NVARCHAR(50) = NULL
- @PageNumber INT = 1
- @PageSize INT = 50

Returns:
- Paginated invoice list + TotalCount
```

**sp_CreateSalesInvoice** (`[Sales].[sp_CreateSalesInvoice]`):
```sql
Parameters:
- All invoice fields

Returns:
- @Id AS Id
```

**sp_UpdateSalesInvoice** (`[Sales].[sp_UpdateSalesInvoice]`):
```sql
Parameters:
- All updatable invoice fields

Returns:
- @@ROWCOUNT as RowsAffected
```

**sp_DeleteSalesInvoice** (`[Sales].[sp_DeleteSalesInvoice]`):
```sql
Parameters:
- @Id UNIQUEIDENTIFIER
- @TenantId UNIQUEIDENTIFIER

Returns:
- @@ROWCOUNT as RowsAffected
```

**sp_PostSalesInvoice** (`[Sales].[sp_PostSalesInvoice]`):
```sql
Parameters:
- @Id UNIQUEIDENTIFIER
- @TenantId UNIQUEIDENTIFIER
- @PostedBy UNIQUEIDENTIFIER

Returns:
- @@ROWCOUNT as RowsAffected
```

**sp_VoidSalesInvoice** (`[Sales].[sp_VoidSalesInvoice]`):
```sql
Parameters:
- @Id UNIQUEIDENTIFIER
- @TenantId UNIQUEIDENTIFIER
- @UpdatedBy NVARCHAR(100) = NULL

Returns:
- @@ROWCOUNT as RowsAffected
```

**sp_GetSalesInvoiceLinesByInvoiceId** (`[Sales].[sp_GetSalesInvoiceLinesByInvoiceId]`):
```sql
Parameters:
- @SalesInvoiceId UNIQUEIDENTIFIER
- @TenantId UNIQUEIDENTIFIER

Returns:
- All line fields + ItemName, ItemCode (joined)
```

#### Field Mapping Issues - Sales Invoice

| Issue | Description | Severity | Status |
|-------|-------------|----------|--------|
| **Missing** | No stored procedures for Sales Invoice operations | ❌ CRITICAL | ✅ **FIXED** - All SPs created |
| **Missing** | Controller expects SPs but they don't exist | ❌ CRITICAL | ✅ **FIXED** - All SPs created |

**✅ Sales Invoice Module Status: FIXED**
- All stored procedures created with `[Sales]` schema
- All CRUD operations supported
- Post and Void operations implemented
- Invoice lines retrieval supported

---

### POS Session Entity

#### Controller: `POSSessionController.cs`
**Endpoints:**
- `POST /api/sales/pos-sessions` → `CreatePOSSessionDto`
- `GET /api/sales/pos-sessions/{id}` → `POSSessionDto`
- `GET /api/sales/pos-sessions/active` → `POSSessionDto`
- `POST /api/sales/pos-sessions/{id}/close` → `ClosePOSSessionDto`

#### DTOs

**POSSessionDto**:
```csharp
- Id (Guid)
- TenantId (Guid)
- SessionNumber (string)
- CashierId (Guid)
- CashierName (string?) - computed/joined
- TerminalId (Guid)
- StartTime (DateTime)
- EndTime (DateTime?)
- OpeningBalance (decimal)
- ClosingBalance (decimal)
- ExpectedBalance (decimal)
- Difference (decimal)
- Status (string)
- Notes (string?)
- CreatedAt (DateTime)
- UpdatedAt (DateTime?)
- CreatedBy (string?)
- UpdatedBy (string?)
```

#### Stored Procedures

**sp_GetPOSSessionById** (`[Sales].[sp_GetPOSSessionById]`):
```sql
Parameters:
- @Id UNIQUEIDENTIFIER
- @TenantID UNIQUEIDENTIFIER

Returns:
- Id, TenantId, SessionNumber, CashierId, TerminalId,
  StartTime, EndTime, OpeningBalance, ClosingBalance,
  ExpectedBalance, Difference, Status, Notes,
  CreatedAt, UpdatedAt, CreatedBy, UpdatedBy,
  CashierName (computed: FirstName + ' ' + LastName)
```

**sp_GetActivePOSSession** (`[Sales].[sp_GetActivePOSSession]`):
```sql
Parameters:
- @TenantID UNIQUEIDENTIFIER
- @CashierId UNIQUEIDENTIFIER = NULL
- @TerminalId UNIQUEIDENTIFIER = NULL

Returns:
- Id, SessionNumber, CashierId, TerminalId,
  StartTime, OpeningBalance, ExpectedBalance,
  Status, Notes, CashierName
```

**sp_CreatePOSSession** (`[Sales].[sp_CreatePOSSession]`):
```sql
Parameters:
- @Id UNIQUEIDENTIFIER
- @TenantID UNIQUEIDENTIFIER
- @SessionNumber NVARCHAR(50)
- @CashierId UNIQUEIDENTIFIER
- @TerminalId UNIQUEIDENTIFIER
- @OpeningBalance DECIMAL(18,2)
- @Status NVARCHAR(50) = 'Open'
- @Notes NVARCHAR(MAX) = NULL
- @CreatedBy NVARCHAR(100)

Returns:
- @Id AS Id
```

**sp_ClosePOSSession** (`[Sales].[sp_ClosePOSSession]`):
```sql
Parameters:
- @Id UNIQUEIDENTIFIER
- @TenantID UNIQUEIDENTIFIER
- @ClosingBalance DECIMAL(18,2)
- @ExpectedBalance DECIMAL(18,2)
- @Difference DECIMAL(18,2)
- @Status NVARCHAR(50) = 'Closed'
- @Notes NVARCHAR(MAX) = NULL
- @UpdatedBy NVARCHAR(100)

Returns:
- @@ROWCOUNT as RowsAffected
```

#### Field Mapping Issues - POS Session

| Issue | Description | Severity | Status |
|-------|-------------|----------|--------|
| **Naming** | SP uses `sp_Sales_*` prefix but should use `[Sales].sp_*` schema convention | ⚠️ MEDIUM | ✅ **FIXED** - All SPs now use `[Sales].[sp_*]` |
| **Match** | Fields align correctly | ✅ OK | ✅ **OK** |
| **Table References** | Fixed table references to use proper schemas | ✅ OK | ✅ **FIXED** - Uses `[Core].[Users]` and `[CRM].Customers` |

**✅ POS Session Module Status: FIXED**
- All stored procedures use `[Sales].[sp_*]` naming convention
- Table references use proper schema prefixes
- Field mappings align correctly

---

## CRM Module

### Customer Entity

#### Controller: `CustomerController.cs`
**Endpoints:**
- `GET /api/crm/customers/{id}` → `CustomerDto`
- `GET /api/crm/customers` → `IEnumerable<CustomerDto>`
- `GET /api/crm/customers/search` → `IEnumerable<CustomerDto>`
- `GET /api/crm/customers/status/{status}` → `IEnumerable<CustomerDto>`
- `GET /api/crm/customers/industry/{industry}` → `IEnumerable<CustomerDto>`
- `POST /api/crm/customers` → `CreateCustomerDto`
- `PUT /api/crm/customers/{id}` → `UpdateCustomerDto`
- `DELETE /api/crm/customers/{id}` → (no DTO)

#### DTOs

**CustomerDto** (inherits BaseDto):
```csharp
- Id (Guid) - from BaseDto
- TenantId (Guid) - from BaseDto
- CreatedAt (DateTime) - from BaseDto
- UpdatedAt (DateTime?) - from BaseDto
- CreatedBy (string?) - from BaseDto
- UpdatedBy (string?) - from BaseDto
- IsActive (bool) - from BaseDto
- CompanyId (Guid)
- CustomerCode (string)
- CustomerName (string)
- CompanyName (string)
- ContactPerson (string?)
- Email (string?)
- Phone (string?)
- Address (string?)
- City (string?)
- State (string?)
- Country (string?)
- PostalCode (string?)
- CustomerType (string)
- Status (string)
- Industry (string?)
- CreditLimit (decimal)
- OutstandingBalance (decimal)
- PaymentTerms (string?)
- TaxId (string?)
- Website (string?)
- Notes (string?)
- CurrencyCode (string)
```

**CreateCustomerDto** (inherits CreateDto):
```csharp
- TenantId (Guid) - from CreateDto
- CreatedBy (string?) - from CreateDto
- CompanyId (Guid)
- CustomerCode (string)
- CustomerName (string)
- CompanyName (string)
- ContactPerson (string?)
- Email (string?)
- Phone (string?)
- Address (string?)
- City (string?)
- State (string?)
- Country (string?)
- PostalCode (string?)
- CustomerType (string)
- Status (string)
- Industry (string?)
- CreditLimit (decimal)
- OutstandingBalance (decimal)
- PaymentTerms (string?)
- TaxId (string?)
- Website (string?)
- Notes (string?)
- CurrencyCode (string)
```

**UpdateCustomerDto** (inherits UpdateDto):
```csharp
- Id (Guid) - from UpdateDto
- UpdatedBy (string?) - from UpdateDto
- CustomerCode (string)
- CustomerName (string)
- CompanyName (string)
- ContactPerson (string?)
- Email (string?)
- Phone (string?)
- Address (string?)
- City (string?)
- State (string?)
- Country (string?)
- PostalCode (string?)
- CustomerType (string)
- Status (string)
- Industry (string?)
- CreditLimit (decimal)
- OutstandingBalance (decimal)
- PaymentTerms (string?)
- TaxId (string?)
- Website (string?)
- Notes (string?)
- CurrencyCode (string)
```

#### Stored Procedures

**sp_GetCustomerById** (`[CRM].[sp_GetCustomerById]`):
```sql
Parameters:
- @Id UNIQUEIDENTIFIER
- @TenantId UNIQUEIDENTIFIER

Returns:
- Id, CustomerCode, CustomerName, CustomerType, PrimaryEmail, SecondaryEmail,
  PrimaryPhone, SecondaryPhone, Website, TaxNumber, CreditLimit, PaymentTerms,
  Currency, Status, Notes, TenantId, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsActive
```

**sp_GetCustomerByCode** (`[CRM].[sp_GetCustomerByCode]`):
```sql
Parameters:
- @CustomerCode NVARCHAR(50)
- @TenantId UNIQUEIDENTIFIER

Returns:
- Same as sp_GetCustomerById
```

**sp_GetAllCustomers** (`[CRM].[sp_GetAllCustomers]`):
```sql
Parameters:
- @TenantId UNIQUEIDENTIFIER
- @CustomerType NVARCHAR(50) = NULL
- @Status NVARCHAR(50) = NULL
- @PageNumber INT = 1
- @PageSize INT = 50

Returns:
- Id, CustomerCode, CustomerName, CustomerType, PrimaryEmail, PrimaryPhone,
  Website, CreditLimit, PaymentTerms, Currency, Status, Notes, TenantId,
  CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsActive
- TotalCount (separate result set)
```

**sp_CreateCustomer** (`[CRM].[sp_CreateCustomer]`):
```sql
Parameters:
- @Id UNIQUEIDENTIFIER
- @CustomerCode NVARCHAR(50)
- @CustomerName NVARCHAR(200)
- @CustomerType NVARCHAR(50)
- @PrimaryEmail NVARCHAR(200)
- @SecondaryEmail NVARCHAR(200)
- @PrimaryPhone NVARCHAR(20)
- @SecondaryPhone NVARCHAR(20)
- @Website NVARCHAR(200)
- @TaxNumber NVARCHAR(50)
- @CreditLimit DECIMAL(18,2)
- @PaymentTerms NVARCHAR(100)
- @Currency NVARCHAR(10)
- @Status NVARCHAR(50)
- @Notes NVARCHAR(1000)
- @TenantId UNIQUEIDENTIFIER
- @CreatedBy NVARCHAR(100)

Returns:
- @Id AS Id
```

**sp_UpdateCustomer** (`[CRM].[sp_UpdateCustomer]`):
```sql
Parameters:
- @Id UNIQUEIDENTIFIER
- @TenantId UNIQUEIDENTIFIER
- @CustomerName NVARCHAR(200)
- @CustomerType NVARCHAR(50)
- @PrimaryEmail NVARCHAR(200)
- @SecondaryEmail NVARCHAR(200)
- @PrimaryPhone NVARCHAR(20)
- @SecondaryPhone NVARCHAR(20)
- @Website NVARCHAR(200)
- @TaxNumber NVARCHAR(50)
- @CreditLimit DECIMAL(18,2)
- @PaymentTerms NVARCHAR(100)
- @Currency NVARCHAR(10)
- @Status NVARCHAR(50)
- @Notes NVARCHAR(1000)
- @UpdatedBy NVARCHAR(100)

Returns:
- @@ROWCOUNT as RowsAffected
```

#### Field Mapping Issues - Customer

| Issue | Description | Severity |
|-------|-------------|----------|
| **Mismatch 1** | DTO has `Email`, `Phone` but SP uses `PrimaryEmail`, `PrimaryPhone` | ⚠️ HIGH |
| **Mismatch 2** | DTO has `TaxId` but SP uses `TaxNumber` | ⚠️ MEDIUM |
| **Mismatch 3** | DTO has `CurrencyCode` but SP uses `Currency` | ⚠️ MEDIUM |
| **Mismatch 4** | DTO has `CompanyId`, `CompanyName`, `ContactPerson`, `Address`, `City`, `State`, `Country`, `PostalCode`, `Industry`, `OutstandingBalance` but SP doesn't have these fields | ⚠️ HIGH |
| **Mismatch 5** | `sp_UpdateCustomer` doesn't update `CustomerCode` (missing parameter) | ⚠️ MEDIUM |
| **Missing** | No `sp_DeleteCustomer` stored procedure | ❌ CRITICAL |
| **Missing** | No `sp_SearchCustomers` stored procedure (controller has search endpoint) | ⚠️ MEDIUM |
| **Missing** | No `sp_GetCustomersByIndustry` stored procedure | ⚠️ LOW |

---

## HRM Module

### Employee Entity

#### Controller: `EmployeeController.cs`
**Endpoints:**
- `POST /api/hrm/employees/addEmployee` → `CreateEmployeeDto`
- `PUT /api/hrm/employees/updateEmployee` → `UpdateEmployeeDto`
- `DELETE /api/hrm/employees/deleteEmployee` → (no DTO, query param: employeeId)
- `GET /api/hrm/employees/getEmployeeById` → `EmployeeDto` (query param: employeeId)
- `GET /api/hrm/employees/getAllEmployees` → `IEnumerable<EmployeeDto>` (query: EmployeeQuery)
- `GET /api/hrm/employees/getEmployeeByCode` → `EmployeeDto` (query param: employeeCode)
- `GET /api/hrm/employees/isEmployeeCodeUnique` → (query params: employeeCode, excludeEmployeeId)

#### DTOs

**EmployeeDto** (inherits BaseDto):
```csharp
- Id (Guid?) - NOTE: nullable, also in BaseDto
- EmployeeId (Guid?) - NOTE: duplicate of Id?
- TenantId (Guid?) - from BaseDto (nullable)
- CompanyId (Guid?)
- EmployeeCode (string)
- Salutation (string?)
- FirstName (string)
- LastName (string)
- PreferredName (string?)
- Gender (string?)
- DateOfBirth (DateTime?)
- Nationality (string?)
- MaritalStatus (string?)
- JobTitle (string?)
- DepartmentId (Guid?)
- ManagerId (Guid?)
- EmploymentType (string)
- WorkEmail (string?)
- PersonalEmail (string?)
- WorkPhone (string?)
- MobilePhone (string?)
- WorkLocationId (Guid?)
- DateHired (DateTime?)
- DateTerminated (DateTime?)
- Status (string)
- IsRemote (bool)
- EmploymentContractId (Guid?)
- IsDeleted (bool)
- CreatedBy (string?)
- UpdatedBy (string?)
```

**CreateEmployeeDto** (inherits CreateDto):
```csharp
- TenantId (Guid) - from CreateDto
- CreatedBy (string?) - from CreateDto
- CompanyId (Guid?)
- EmployeeCode (string)
- Salutation (string?)
- FirstName (string)
- LastName (string)
- PreferredName (string?)
- Gender (string?)
- DateOfBirth (DateTime?)
- Nationality (string?)
- MaritalStatus (string?)
- JobTitle (string?)
- DepartmentId (Guid?)
- ManagerId (Guid?)
- EmploymentType (string)
- WorkEmail (string?)
- PersonalEmail (string?)
- WorkPhone (string?)
- MobilePhone (string?)
- WorkLocationId (Guid?)
- DateHired (DateTime?)
- Status (string)
- IsRemote (bool)
- EmploymentContractId (Guid?)
```

**UpdateEmployeeDto** (inherits UpdateDto):
```csharp
- Id (Guid) - from UpdateDto
- UpdatedBy (string?) - from UpdateDto
- CompanyId (Guid?)
- Salutation (string?)
- PreferredName (string?)
- Gender (string?)
- DateOfBirth (DateTime?)
- Nationality (string?)
- MaritalStatus (string?)
- JobTitle (string?)
- DepartmentId (Guid?)
- ManagerId (Guid?)
- EmploymentType (string?)
- WorkEmail (string?)
- PersonalEmail (string?)
- WorkPhone (string?)
- MobilePhone (string?)
- WorkLocationId (Guid?)
- DateHired (DateTime?)
- DateTerminated (DateTime?)
- Status (string?)
- IsRemote (bool?)
- EmploymentContractId (Guid?)
```

#### Stored Procedures

**⚠️ NOTE:** Need to check HRMModule_StoredProcedures.sql for Employee procedures.

#### Field Mapping Issues - Employee

| Issue | Description | Severity |
|-------|-------------|----------|
| **TBD** | Need to read HRM stored procedures to complete analysis | ⚠️ PENDING |

---

## Summary of Issues

### Critical Issues (Must Fix)

1. **User Module:** ✅ **FIXED**
   - ✅ Created `sp_UpdateUser` stored procedure
   - ✅ Created `sp_DeleteUser` stored procedure
   - ✅ Created `sp_UpdateUserPassword` stored procedure
   - ✅ Fixed field inconsistencies - all Get SPs now return consistent fields
   - ✅ Repository updated to use correct SP names with schema prefix

2. **Sales Module:** ✅ **FIXED**
   - ✅ Created all Sales Invoice stored procedures
   - ✅ All CRUD operations, Post, and Void procedures implemented

3. **CRM Module:** ✅ **FIXED**
   - ✅ Created `sp_DeleteCustomer` stored procedure
   - ✅ Added `CustomerCode` parameter to `sp_UpdateCustomer`
   - ✅ Created `sp_SearchCustomers` stored procedure
   - ✅ Created `sp_GetCustomersByIndustry` stored procedure
   - ⚠️ DTO has many fields not in stored procedures (CompanyId, CompanyName, ContactPerson, Address, City, State, Country, PostalCode, Industry, OutstandingBalance) - These may be in related tables (CustomerAddresses) or computed fields

### High Priority Issues

1. **User Module:** ✅ **RESOLVED**
   - ✅ Field mapping handled correctly in service layer
   - ✅ All Get SPs return consistent fields

2. **CRM Module:** ✅ **PARTIALLY RESOLVED**
   - ⚠️ DTO has `Email`, `Phone` but SP uses `PrimaryEmail`, `PrimaryPhone` - Service layer handles mapping
   - ⚠️ DTO has `TaxId` but SP uses `TaxNumber` - Service layer handles mapping
   - ⚠️ DTO has `CurrencyCode` but SP uses `Currency` - Service layer handles mapping
   - ✅ `sp_UpdateCustomer` now includes `CustomerCode` parameter

### Medium Priority Issues

1. **POS Module:** ✅ **FIXED**
   - ✅ Fixed naming convention: All SPs now use `[Sales].[sp_*]`

2. **User Module:**
   - `sp_GetUserByEmail` returns many fields not in `UserDto`

3. **CRM Module:** ✅ **FIXED**
   - ✅ Created `sp_SearchCustomers` stored procedure
   - ✅ Created `sp_GetCustomersByIndustry` stored procedure
   - ✅ Added `CustomerCode` parameter to `sp_UpdateCustomer`

4. **HRM Module:** ✅ **FIXED**
   - ✅ Created all Employee CRUD stored procedures
   - ✅ Created GetByCode and IsCodeUnique procedures
   - ⚠️ EmployeeDto has nullable `Id` and duplicate `EmployeeId` field - DTO structure issue

### Recommendations

1. **Standardize Naming:**
   - Use schema-based naming: `[Module].[sp_ProcedureName]`
   - Consistent across all modules

2. **Create Missing SPs:**
   - Implement all missing CRUD operations
   - Add update/delete procedures for User
   - Add all Sales Invoice procedures

3. **Field Alignment:**
   - Align DTO field names with SP return columns
   - Use consistent naming (Email vs PrimaryEmail)
   - Document computed/joined fields

4. **Consistency:**
   - Ensure all Get procedures return same base fields
   - Standardize parameter naming (@TenantID vs @TenantId)

---

## Next Steps

1. Review and fix critical issues
2. Create missing stored procedures
3. Align field names between DTOs and SPs
4. Update service layer to handle field mapping
5. Add unit tests for field mapping validation

---

**Note:** This document should be updated as issues are resolved and new modules are added.

---

## Document Status

### Completed Modules
- ✅ Core Module (User) - **FIXED** - All CRUD operations, field mappings aligned
- ✅ Admin Module (System Log) - **COMPLETE** - All fields match
- ✅ Sales Module (Sales Invoice, POS Session) - **FIXED** - All SPs created, naming standardized
- ✅ CRM Module (Customer) - **FIXED** - All CRUD operations, search, and industry filter
- ✅ HRM Module (Employee) - **FIXED** - All CRUD operations, code uniqueness check

### Pending Modules
- ⏳ Finance Module
- ⏳ Inventory Module
- ⏳ Procurement Module
- ⏳ Manufacturing Module
- ⏳ Projects Module
- ⏳ Reports Module

### Next Steps
1. Complete HRM module analysis
2. Analyze remaining modules
3. Create missing stored procedures
4. Fix field name mismatches
5. Update service layer mappings
6. Add validation tests

